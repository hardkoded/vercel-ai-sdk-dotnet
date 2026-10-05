// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>One streamed tool-call delta from an OpenAI-compatible chat completion.</summary>
internal sealed class StreamingToolCallDelta
{
    /// <summary>Creates a delta. A null name or arguments value means that field was omitted or null.</summary>
    public StreamingToolCallDelta(int? index, string? id, string? name, string? arguments)
    {
        Index = index;
        Id = id;
        Name = name;
        NameIsString = name != null;
        Arguments = arguments;
        HasArguments = arguments != null;
    }

    /// <summary>Wire index, when the delta included one.</summary>
    public int? Index { get; }

    /// <summary>Wire id, including blank text. Null when the field was omitted or null.</summary>
    public string? Id { get; }

    /// <summary>Function name when it was sent as a string, including blank text.</summary>
    public string? Name { get; }

    /// <summary>True when <see cref="Name"/> was a string. False for null and omitted names.</summary>
    public bool NameIsString { get; }

    /// <summary>Argument fragment when it was sent as a string.</summary>
    public string? Arguments { get; }

    /// <summary>True when <see cref="Arguments"/> was a string, including an empty string.</summary>
    public bool HasArguments { get; }
}

/// <summary>
/// Correlates streamed Chat Completions tool-call deltas. A non-blank id, an index, and a
/// function name are labels. Blank labels are absent. Calls stay distinct when those labels
/// are omitted, repeated, or changed.
/// </summary>
internal sealed class StreamingToolCallTracker
{
    private readonly List<TrackedToolCall> _calls = new();
    private readonly Dictionary<string, List<TrackedToolCall>> _byId = new(StringComparer.Ordinal);
    private readonly Dictionary<int, List<TrackedToolCall>> _byIndex = new();
    private readonly HashSet<string> _usedIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _nextSuffix = new(StringComparer.Ordinal);
    private readonly Func<string> _generateId;

    /// <summary>Creates a tracker. <paramref name="generateId"/> supplies an id when the wire id is missing or already used.</summary>
    public StreamingToolCallTracker(Func<string>? generateId = null)
    {
        _generateId = generateId ?? (() => JsonValues.GenerateId("call_"));
    }

    /// <summary>Applies one tool-call delta.</summary>
    public void ProcessDelta(StreamingToolCallDelta toolCallDelta)
    {
        var rawName = toolCallDelta.NameIsString ? toolCallDelta.Name : null;
        var hasBlankName = toolCallDelta.NameIsString && string.IsNullOrWhiteSpace(toolCallDelta.Name);
        var wireId = GetNonBlankString(toolCallDelta.Id);
        var name = GetNonBlankString(rawName);
        var index = toolCallDelta.Index;
        var arguments = toolCallDelta.HasArguments ? toolCallDelta.Arguments : null;
        var hasExplicitCallStart = name != null && StreamingToolCallArgumentState.StartsWithStructuredValue(arguments);
        var resolution = ResolveToolCall(wireId, index, name, hasExplicitCallStart);
        if (resolution.Kind == ToolCallResolutionKind.Ambiguous)
        {
            return;
        }

        TrackedToolCall call;
        if (resolution.Kind == ToolCallResolutionKind.New)
        {
            // A blank name cannot start a call. Match continuations first, then ignore only the unmatched delta.
            if (hasBlankName)
            {
                return;
            }

            call = ProcessNewToolCall(wireId, index, name, arguments ?? string.Empty);
        }
        else
        {
            call = resolution.Call!;
            if (wireId != null)
            {
                AssociateWireId(call, wireId);
            }

            ProcessExistingToolCall(call, arguments);
        }

        if (index != null)
        {
            AssociateIndex(call, index.Value);
        }
    }

    /// <summary>Emits one tool call per unfinished call, in index order when every call has an index.</summary>
    public List<ToolCallStreamPart> Flush()
    {
        var ordered = _calls;
        if (EveryCallHasIndex())
        {
            ordered = new List<TrackedToolCall>(_calls);
            ordered.Sort(CompareByIndex);
        }

        var parts = new List<ToolCallStreamPart>(ordered.Count);
        foreach (var call in ordered)
        {
            if (call.HasFinished)
            {
                continue;
            }

            parts.Add(new ToolCallStreamPart(call.Id, call.Name, call.Arguments.ToString()));
            call.HasFinished = true;
        }

        return parts;
    }

    private ToolCallResolution ResolveToolCall(string? wireId, int? index, string? name, bool hasExplicitCallStart)
    {
        List<TrackedToolCall>? indexed = null;
        if (index != null && _byIndex.TryGetValue(index.Value, out var indexedCalls))
        {
            indexed = indexedCalls;
        }

        var matchingIndexed = FilterToolCallsByName(indexed, name);
        if (wireId != null)
        {
            if (_byId.TryGetValue(wireId, out var withId))
            {
                if (index != null)
                {
                    var matching = new List<TrackedToolCall>();
                    foreach (var call in matchingIndexed)
                    {
                        if (withId.Contains(call))
                        {
                            matching.Add(call);
                        }
                    }

                    var resolved = ResolveMatchingToolCall(matching, hasExplicitCallStart);
                    if (resolved.Kind != ToolCallResolutionKind.New)
                    {
                        return resolved;
                    }

                    // A named delta at a different index is a new call even when the id repeats.
                    if (name != null)
                    {
                        return ToolCallResolution.New();
                    }

                    if (indexed != null)
                    {
                        return ToolCallResolution.Ambiguous();
                    }

                    return ResolveMatchingToolCall(withId, false);
                }

                if (name != null)
                {
                    var matching = new List<TrackedToolCall>();
                    foreach (var call in withId)
                    {
                        if (call.Name == name)
                        {
                            matching.Add(call);
                        }
                    }

                    return ResolveMatchingToolCall(matching, hasExplicitCallStart);
                }

                return ResolveMatchingToolCall(withId, false);
            }

            if (matchingIndexed.Count > 0)
            {
                // An unseen id plus a fresh object or array is a new call. An ordinary
                // fragment keeps the call already stored under that index and name, including
                // when the continuation's id differs from the first id.
                return hasExplicitCallStart
                    ? ToolCallResolution.New()
                    : ResolveMatchingToolCall(matchingIndexed, false);
            }

            return ToolCallResolution.New();
        }

        if (indexed != null)
        {
            return ResolveMatchingToolCall(matchingIndexed, hasExplicitCallStart);
        }

        if (name != null)
        {
            return ToolCallResolution.New();
        }

        if (_calls.Count == 1)
        {
            return ToolCallResolution.Existing(_calls[0]);
        }

        return _calls.Count > 1 ? ToolCallResolution.Ambiguous() : ToolCallResolution.New();
    }

    private static ToolCallResolution ResolveMatchingToolCall(List<TrackedToolCall> calls, bool hasExplicitCallStart)
    {
        if (calls.Count == 0)
        {
            return ToolCallResolution.New();
        }

        if (!hasExplicitCallStart)
        {
            return calls.Count == 1 ? ToolCallResolution.Existing(calls[0]) : ToolCallResolution.Ambiguous();
        }

        var open = new List<TrackedToolCall>();
        foreach (var call in calls)
        {
            if (!call.ArgumentState.HasCompleteStructuredValue)
            {
                open.Add(call);
            }
        }

        if (open.Count == 1)
        {
            return ToolCallResolution.Existing(open[0]);
        }

        return open.Count > 1 ? ToolCallResolution.Ambiguous() : ToolCallResolution.New();
    }

    private static List<TrackedToolCall> FilterToolCallsByName(List<TrackedToolCall>? calls, string? name)
    {
        var result = new List<TrackedToolCall>();
        if (calls == null)
        {
            return result;
        }

        foreach (var call in calls)
        {
            if (name == null || call.Name == name)
            {
                result.Add(call);
            }
        }

        return result;
    }

    private TrackedToolCall ProcessNewToolCall(string? wireId, int? index, string? name, string arguments)
    {
        if (name == null)
        {
            throw new AiSdkException("Expected 'function.name' to be a string.");
        }

        var call = new TrackedToolCall(CreateToolCallId(wireId), index, _calls.Count, name, arguments);
        _calls.Add(call);
        if (wireId != null)
        {
            AssociateWireId(call, wireId);
        }

        return call;
    }

    private static void ProcessExistingToolCall(TrackedToolCall call, string? arguments)
    {
        if (call.HasFinished || arguments == null)
        {
            return;
        }

        call.ArgumentState.Append(arguments);
        call.Arguments.Append(arguments);
    }

    private string CreateToolCallId(string? wireId)
    {
        if (wireId != null && _usedIds.Add(wireId))
        {
            return wireId;
        }

        var generated = GetNonBlankString(_generateId()) ?? "tool-call";
        if (_usedIds.Add(generated))
        {
            return generated;
        }

        var suffix = 1;
        if (_nextSuffix.TryGetValue(generated, out var storedSuffix))
        {
            suffix = storedSuffix;
        }

        var lastSuffix = suffix + _usedIds.Count;
        for (; suffix <= lastSuffix; suffix++)
        {
            var candidate = generated + "-" + suffix.ToString(CultureInfo.InvariantCulture);
            if (_usedIds.Add(candidate))
            {
                _nextSuffix[generated] = suffix + 1;
                return candidate;
            }
        }

        throw new InvalidOperationException("Failed to create a unique tool call id.");
    }

    private static string? GetNonBlankString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value;
    }

    private void AssociateWireId(TrackedToolCall call, string wireId)
    {
        if (!_byId.TryGetValue(wireId, out var calls))
        {
            calls = new List<TrackedToolCall>();
            _byId[wireId] = calls;
        }

        if (!calls.Contains(call))
        {
            calls.Add(call);
        }
    }

    private void AssociateIndex(TrackedToolCall call, int index)
    {
        if (!_byIndex.TryGetValue(index, out var calls))
        {
            calls = new List<TrackedToolCall>();
            _byIndex[index] = calls;
        }

        if (!calls.Contains(call))
        {
            calls.Add(call);
        }
    }

    private bool EveryCallHasIndex()
    {
        foreach (var call in _calls)
        {
            if (call.Index == null)
            {
                return false;
            }
        }

        return true;
    }

    private static int CompareByIndex(TrackedToolCall left, TrackedToolCall right)
    {
        var compared = left.Index.GetValueOrDefault().CompareTo(right.Index.GetValueOrDefault());
        if (compared != 0)
        {
            return compared;
        }

        return left.Sequence.CompareTo(right.Sequence);
    }

    private enum ToolCallResolutionKind
    {
        Existing,
        New,
        Ambiguous,
    }

    private readonly struct ToolCallResolution
    {
        private ToolCallResolution(ToolCallResolutionKind kind, TrackedToolCall? call)
        {
            Kind = kind;
            Call = call;
        }

        public ToolCallResolutionKind Kind { get; }

        public TrackedToolCall? Call { get; }

        public static ToolCallResolution Existing(TrackedToolCall call)
        {
            return new ToolCallResolution(ToolCallResolutionKind.Existing, call);
        }

        public static ToolCallResolution New()
        {
            return new ToolCallResolution(ToolCallResolutionKind.New, null);
        }

        public static ToolCallResolution Ambiguous()
        {
            return new ToolCallResolution(ToolCallResolutionKind.Ambiguous, null);
        }
    }

    private sealed class TrackedToolCall
    {
        public TrackedToolCall(string id, int? index, int sequence, string name, string arguments)
        {
            Id = id;
            Index = index;
            Sequence = sequence;
            Name = name;
            Arguments = new StringBuilder(arguments);
            ArgumentState = new StreamingToolCallArgumentState(arguments);
        }

        public string Id { get; }

        public int? Index { get; }

        public int Sequence { get; }

        public string Name { get; }

        public StringBuilder Arguments { get; }

        public StreamingToolCallArgumentState ArgumentState { get; }

        public bool HasFinished { get; set; }
    }
}
