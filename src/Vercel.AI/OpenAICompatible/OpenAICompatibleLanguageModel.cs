// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Chat Completions language model.</summary>
public sealed class OpenAICompatibleLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a language model.</summary>
    public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var body = BuildBody(options, stream: false);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.ChatUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return ParseGenerate(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = BuildBody(options, stream: true);
        var toolCalls = new ToolAccumulator();
        string? finishRaw = null;
        LanguageModelUsage? usage = null;
        await foreach (var data in _provider.Http.SendSseAsync(
            _provider.ChatUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false))
        {
            JsonNode? node;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (System.Text.Json.JsonException)
            {
                continue;
            }

            if (node is not JsonObject root)
            {
                continue;
            }

            var usageNode = root["usage"];
            if (usageNode is JsonObject usageObject)
            {
                usage = ReadUsage(usageObject);
            }

            var choice = root["choices"]?[0] as JsonObject;
            if (choice is null)
            {
                continue;
            }

            var finishText = AsString(choice["finish_reason"]);
            if (!string.IsNullOrEmpty(finishText))
            {
                finishRaw = finishText;
            }

            if (choice["delta"] is not JsonObject delta)
            {
                continue;
            }

            var text = AsString(delta["content"]);
            if (!string.IsNullOrEmpty(text))
            {
                yield return new TextDeltaStreamPart("text", text!);
            }

            var reasoning = AsString(delta["reasoning_content"]);
            if (!string.IsNullOrEmpty(reasoning))
            {
                yield return new ReasoningDeltaStreamPart("reasoning", reasoning!);
            }

            if (delta["tool_calls"] is JsonArray toolDeltas)
            {
                foreach (var item in toolDeltas)
                {
                    if (item is JsonObject tool)
                    {
                        toolCalls.Add(tool);
                    }
                }
            }
        }

        foreach (var call in toolCalls.Finish())
        {
            yield return call;
        }

        yield return new FinishStreamPart(FinishReasons.Parse(finishRaw), usage ?? LanguageModelUsage.Empty, finishRaw);
    }

    private JsonObject BuildBody(LanguageModelCallOptions options, bool stream)
    {
        var messages = new JsonArray();
        foreach (var message in options.Prompt)
        {
            messages.Add(MapMessage(message));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = messages,
            ["stream"] = stream,
        };

        if (stream)
        {
            body["stream_options"] = new JsonObject { ["include_usage"] = true };
        }

        AddSampling(body, options);
        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = tool.Name,
                        ["description"] = tool.Description,
                        ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                    },
                });
            }

            body["tools"] = tools;
            body["tool_choice"] = MapToolChoice(options.ToolChoice);
        }

        if (options.JsonSchema is { } schema)
        {
            body["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = options.JsonSchemaName ?? "response",
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                },
            };
        }

        return body;
    }

    private static void AddSampling(JsonObject body, LanguageModelCallOptions options)
    {
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_tokens"] = maxTokens;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }
    }

    private static JsonNode MapToolChoice(ToolChoice? toolChoice)
    {
        if (toolChoice is null || toolChoice.Type == "auto")
        {
            return "auto";
        }

        if (toolChoice.Type == "none")
        {
            return "none";
        }

        if (toolChoice.Type == "required")
        {
            return "required";
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = named.ToolName },
            };
        }

        return "auto";
    }

    private static JsonObject MapMessage(ModelMessage message)
    {
        switch (message)
        {
            case SystemModelMessage system:
                return new JsonObject { ["role"] = "system", ["content"] = system.Content };
            case UserModelMessage user:
                return new JsonObject { ["role"] = "user", ["content"] = MapUserContent(user) };
            case AssistantModelMessage assistant:
                var assistantJson = new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = assistant.Text,
                };
                if (assistant.ToolCalls.Count > 0)
                {
                    var calls = new JsonArray();
                    foreach (var call in assistant.ToolCalls)
                    {
                        calls.Add(new JsonObject
                        {
                            ["id"] = call.ToolCallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject
                            {
                                ["name"] = call.ToolName,
                                ["arguments"] = call.ArgumentsJson,
                            },
                        });
                    }

                    assistantJson["tool_calls"] = calls;
                }

                return assistantJson;
            case ToolModelMessage tool:
                return new JsonObject
                {
                    ["role"] = "tool",
                    ["tool_call_id"] = tool.ToolCallId,
                    ["content"] = tool.OutputJson,
                };
            default:
                throw new AiSdkException("Unsupported message role '" + message.Role + "'.");
        }
    }

    private static JsonNode MapUserContent(UserModelMessage user)
    {
        var onlyText = true;
        foreach (var part in user.Content)
        {
            if (part is not TextContentPart)
            {
                onlyText = false;
                break;
            }
        }

        if (onlyText)
        {
            var builder = new StringBuilder();
            foreach (var part in user.Content)
            {
                if (part is TextContentPart text)
                {
                    builder.Append(text.Text);
                }
            }

            return builder.ToString();
        }

        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart)
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
            }
            else if (part is FileContentPart file)
            {
                var url = file.Url;
                if (url is null && file.Data != null)
                {
                    url = "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data);
                }

                parts.Add(new JsonObject
                {
                    ["type"] = "image_url",
                    ["image_url"] = new JsonObject { ["url"] = url },
                });
            }
        }

        return parts;
    }

    private static LanguageModelGenerateResult ParseGenerate(System.Text.Json.JsonElement root)
    {
        var choice = root.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var content = new List<GeneratedContent>();
        if (message.TryGetProperty("content", out var text) && text.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var value = text.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                content.Add(new GeneratedText(value!));
            }
        }

        if (message.TryGetProperty("reasoning_content", out var reasoning) && reasoning.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            content.Add(new GeneratedReasoning(reasoning.GetString() ?? string.Empty));
        }

        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                var function = call.GetProperty("function");
                content.Add(new GeneratedToolCall(
                    call.GetProperty("id").GetString() ?? JsonValues.GenerateId("call_"),
                    function.GetProperty("name").GetString() ?? string.Empty,
                    function.GetProperty("arguments").GetString() ?? "{}"));
            }
        }

        var raw = choice.TryGetProperty("finish_reason", out var finish) ? finish.GetString() : null;
        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("prompt_tokens", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("completion_tokens", out var output) ? output.GetInt32() : null,
                usageElement.TryGetProperty("total_tokens", out var total) ? total.GetInt32() : null);
        }

        var id = root.TryGetProperty("id", out var responseId) ? responseId.GetString() : null;
        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw, responseId: id);
    }

    private static LanguageModelUsage ReadUsage(JsonObject usage)
    {
        return new LanguageModelUsage(
            ReadInt(usage, "prompt_tokens"),
            ReadInt(usage, "completion_tokens"),
            ReadInt(usage, "total_tokens"));
    }

    private static string? AsString(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static int? ReadInt(JsonObject obj, string name)
    {
        if (obj[name] is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return null;
    }

    /// <summary>
    /// Correlates streamed Chat Completions tool-call deltas. A non-blank id, an index, and a
    /// function name are labels. Blank labels are absent. Calls stay distinct when those labels
    /// are omitted, repeated, or changed.
    /// </summary>
    private sealed class ToolAccumulator
    {
        private readonly List<PendingToolCall> _calls = new();
        private readonly Dictionary<string, List<PendingToolCall>> _byId = new(StringComparer.Ordinal);
        private readonly Dictionary<int, List<PendingToolCall>> _byIndex = new();
        private readonly HashSet<string> _usedIds = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _nextSuffix = new(StringComparer.Ordinal);

        public void Add(JsonObject tool)
        {
            var function = tool["function"] as JsonObject;
            var rawName = function == null ? null : AsString(function["name"]);
            var wireId = NonBlank(AsString(tool["id"]));
            var name = NonBlank(rawName);
            int? index = TryReadIndex(tool, out var parsedIndex) ? parsedIndex : null;
            var arguments = function == null ? null : AsString(function["arguments"]);
            var startsStructured = name != null && StartsWithStructuredValue(arguments);
            var match = Resolve(wireId, index, name, startsStructured);
            if (match.Kind == MatchKind.Ambiguous)
            {
                return;
            }

            PendingToolCall call;
            if (match.Kind == MatchKind.New)
            {
                // A blank or missing name cannot start a call. Continuations are matched first,
                // so this only drops the unmatched delta and leaves calls already accumulated.
                if (name == null)
                {
                    return;
                }

                call = Start(wireId, index, name, arguments ?? string.Empty);
            }
            else
            {
                call = match.Call!;
                if (wireId != null)
                {
                    AssociateId(call, wireId);
                }

                if (arguments != null)
                {
                    call.Structure.Append(arguments);
                    call.Arguments.Append(arguments);
                }
            }

            if (index != null)
            {
                AssociateIndex(call, index.Value);
            }
        }

        public List<ToolCallStreamPart> Finish()
        {
            var ordered = _calls;
            if (EveryCallHasIndex())
            {
                ordered = new List<PendingToolCall>(_calls);
                ordered.Sort(CompareByIndex);
            }

            var parts = new List<ToolCallStreamPart>(ordered.Count);
            foreach (var call in ordered)
            {
                parts.Add(new ToolCallStreamPart(call.Id, call.Name, call.Arguments.ToString()));
            }

            return parts;
        }

        private Match Resolve(string? wireId, int? index, string? name, bool startsStructured)
        {
            List<PendingToolCall>? indexed = null;
            if (index != null && _byIndex.TryGetValue(index.Value, out var indexedCalls))
            {
                indexed = indexedCalls;
            }

            var matchingIndexed = FilterByName(indexed, name);
            if (wireId != null)
            {
                if (_byId.TryGetValue(wireId, out var withId))
                {
                    if (index != null)
                    {
                        var matching = new List<PendingToolCall>();
                        foreach (var call in matchingIndexed)
                        {
                            if (withId.Contains(call))
                            {
                                matching.Add(call);
                            }
                        }

                        var resolved = ResolveMatching(matching, startsStructured);
                        if (resolved.Kind != MatchKind.New)
                        {
                            return resolved;
                        }

                        // A named delta at a different index is a new call even when the id repeats.
                        if (name != null)
                        {
                            return Match.New();
                        }

                        if (indexed != null)
                        {
                            return Match.Ambiguous();
                        }

                        return ResolveMatching(withId, false);
                    }

                    if (name != null)
                    {
                        var matching = new List<PendingToolCall>();
                        foreach (var call in withId)
                        {
                            if (call.Name == name)
                            {
                                matching.Add(call);
                            }
                        }

                        return ResolveMatching(matching, startsStructured);
                    }

                    return ResolveMatching(withId, false);
                }

                if (matchingIndexed.Count > 0)
                {
                    // An unseen id plus a fresh object or array is a new call. An ordinary
                    // fragment keeps the call already stored under that index and name, including
                    // when the continuation's id differs from the first id.
                    return startsStructured ? Match.New() : ResolveMatching(matchingIndexed, false);
                }

                return Match.New();
            }

            if (indexed != null)
            {
                return ResolveMatching(matchingIndexed, startsStructured);
            }

            if (name != null)
            {
                return Match.New();
            }

            if (_calls.Count == 1)
            {
                return Match.Existing(_calls[0]);
            }

            return _calls.Count > 1 ? Match.Ambiguous() : Match.New();
        }

        private static Match ResolveMatching(List<PendingToolCall> calls, bool startsStructured)
        {
            if (calls.Count == 0)
            {
                return Match.New();
            }

            if (!startsStructured)
            {
                return calls.Count == 1 ? Match.Existing(calls[0]) : Match.Ambiguous();
            }

            var open = new List<PendingToolCall>();
            foreach (var call in calls)
            {
                if (!call.Structure.HasCompleteValue)
                {
                    open.Add(call);
                }
            }

            if (open.Count == 1)
            {
                return Match.Existing(open[0]);
            }

            return open.Count > 1 ? Match.Ambiguous() : Match.New();
        }

        private static List<PendingToolCall> FilterByName(List<PendingToolCall>? calls, string? name)
        {
            var result = new List<PendingToolCall>();
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

        private PendingToolCall Start(string? wireId, int? index, string name, string arguments)
        {
            var call = new PendingToolCall(CreateId(wireId), index, _calls.Count, name, arguments);
            _calls.Add(call);
            if (wireId != null)
            {
                AssociateId(call, wireId);
            }

            return call;
        }

        private string CreateId(string? wireId)
        {
            if (wireId != null && _usedIds.Add(wireId))
            {
                return wireId;
            }

            var generated = NonBlank(JsonValues.GenerateId("call_")) ?? "tool-call";
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
                var candidate = generated + "-" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (_usedIds.Add(candidate))
                {
                    _nextSuffix[generated] = suffix + 1;
                    return candidate;
                }
            }

            throw new InvalidOperationException("Failed to create a unique tool call id.");
        }

        private void AssociateId(PendingToolCall call, string wireId)
        {
            if (!_byId.TryGetValue(wireId, out var calls))
            {
                calls = new List<PendingToolCall>();
                _byId[wireId] = calls;
            }

            if (!calls.Contains(call))
            {
                calls.Add(call);
            }
        }

        private void AssociateIndex(PendingToolCall call, int index)
        {
            if (!_byIndex.TryGetValue(index, out var calls))
            {
                calls = new List<PendingToolCall>();
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

        private static int CompareByIndex(PendingToolCall left, PendingToolCall right)
        {
            var compared = left.Index.GetValueOrDefault().CompareTo(right.Index.GetValueOrDefault());
            if (compared != 0)
            {
                return compared;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private static string? NonBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }

        private static bool TryReadIndex(JsonObject tool, out int index)
        {
            index = 0;
            if (tool["index"] is JsonValue value && value.TryGetValue<int>(out index))
            {
                return true;
            }

            return false;
        }

        private static bool StartsWithStructuredValue(string? value)
        {
            if (value == null)
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                {
                    continue;
                }

                return value[i] == '{' || value[i] == '[';
            }

            return false;
        }

        private enum MatchKind
        {
            Existing,
            New,
            Ambiguous,
        }

        private readonly struct Match
        {
            private Match(MatchKind kind, PendingToolCall? call)
            {
                Kind = kind;
                Call = call;
            }

            public MatchKind Kind { get; }

            public PendingToolCall? Call { get; }

            public static Match Existing(PendingToolCall call)
            {
                return new Match(MatchKind.Existing, call);
            }

            public static Match New()
            {
                return new Match(MatchKind.New, null);
            }

            public static Match Ambiguous()
            {
                return new Match(MatchKind.Ambiguous, null);
            }
        }

        private sealed class PendingToolCall
        {
            public PendingToolCall(string id, int? index, int sequence, string name, string arguments)
            {
                Id = id;
                Index = index;
                Sequence = sequence;
                Name = name;
                Arguments = new StringBuilder(arguments);
                Structure = new ArgumentStructure(arguments);
            }

            public string Id { get; }

            public int? Index { get; }

            public int Sequence { get; }

            public string Name { get; }

            public StringBuilder Arguments { get; }

            public ArgumentStructure Structure { get; }
        }

        /// <summary>
        /// Tracks whether arguments contain one complete object or array. A scalar such as
        /// <c>1</c> followed by <c>2</c> stays incomplete, so the fragments remain one call.
        /// </summary>
        private sealed class ArgumentStructure
        {
            private enum StructureKind
            {
                Undetermined,
                Other,
                Structured,
            }

            private readonly List<char> _stack = new();
            private StructureKind _kind;
            private bool _inString;
            private bool _escaped;
            private bool _complete;

            public ArgumentStructure(string initial)
            {
                Append(initial);
            }

            public bool HasCompleteValue
            {
                get { return _kind == StructureKind.Structured && _complete; }
            }

            public void Append(string delta)
            {
                foreach (var character in delta)
                {
                    if (_kind == StructureKind.Undetermined)
                    {
                        if (char.IsWhiteSpace(character))
                        {
                            continue;
                        }

                        if (character != '{' && character != '[')
                        {
                            _kind = StructureKind.Other;
                            continue;
                        }

                        _kind = StructureKind.Structured;
                        _stack.Add(character);
                        _inString = false;
                        _escaped = false;
                        _complete = false;
                        continue;
                    }

                    if (_kind != StructureKind.Structured || _complete)
                    {
                        continue;
                    }

                    if (_inString)
                    {
                        if (_escaped)
                        {
                            _escaped = false;
                        }
                        else if (character == '\\')
                        {
                            _escaped = true;
                        }
                        else if (character == '"')
                        {
                            _inString = false;
                        }

                        continue;
                    }

                    if (character == '"')
                    {
                        _inString = true;
                    }
                    else if (character == '{' || character == '[')
                    {
                        _stack.Add(character);
                    }
                    else if (character == '}' || character == ']')
                    {
                        var expected = character == '}' ? '{' : '[';
                        if (_stack.Count == 0 || _stack[_stack.Count - 1] != expected)
                        {
                            _kind = StructureKind.Other;
                            continue;
                        }

                        _stack.RemoveAt(_stack.Count - 1);
                        if (_stack.Count == 0)
                        {
                            _complete = true;
                        }
                    }
                }
            }
        }
    }
}
