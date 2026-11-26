// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Vercel.AI.OpenTelemetry;

/// <summary>The result of a <see cref="DurableSampler"/>. Maps to the return type of the <c>Sampler</c> of <c>durable-span-ids.ts</c>.</summary>
public sealed class DurableSamplingResult
{
    /// <summary>0 not recorded, 1 recorded, 2 recorded and sampled. Maps to <c>decision</c>.</summary>
    public int Decision { get; init; }

    /// <summary>The serialized trace state the sampler wants on the span. Maps to <c>traceState</c>.</summary>
    public string? TraceState { get; init; }

    /// <summary>Attributes the sampler adds to the span. Maps to <c>attributes</c>.</summary>
    public IReadOnlyDictionary<string, object?>? Attributes { get; init; }
}

/// <summary>A sampler for <see cref="OpenTelemetryIdGeneratorOptions.Sampler"/>. Maps to the <c>Sampler</c> type of <c>durable-span-ids.ts</c>.</summary>
public delegate DurableSamplingResult DurableSampler(
    ActivityContext parentContext,
    string traceId,
    string spanName,
    ActivityKind spanKind,
    IReadOnlyDictionary<string, object?> attributes
);

/// <summary>Options of <see cref="OpenTelemetryIdGenerator"/>. Maps to the constructor options of <c>OpenTelemetryIdGenerator</c>.</summary>
public sealed class OpenTelemetryIdGeneratorOptions
{
    /// <summary>The sampler that decides for a durable run. Null uses parent-based sampling with roots enabled. Maps to <c>sampler</c>.</summary>
    public DurableSampler? Sampler { get; init; }
}

/// <summary>The ids a span of a durable run must start with. Maps to the <c>ids</c> argument of <c>OpenTelemetryIdGenerator.withIds</c>.</summary>
public sealed class DurableRequestedIds
{
    /// <summary>Maps to <c>traceId</c>.</summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Maps to <c>spanId</c>. .NET gives every <see cref="Activity"/> a random span id, so the port cannot honor it. See
    /// <see cref="OpenTelemetryIdGenerator"/>.
    /// </summary>
    public string? SpanId { get; init; }

    /// <summary>Maps to <c>samplingDecision</c>.</summary>
    public int? SamplingDecision { get; init; }

    /// <summary>Maps to <c>traceState</c>.</summary>
    public string? TraceState { get; init; }
}

/// <summary>
/// Instance-scoped SDK bridge for durable tracing. Pass the same instance as <c>OpenTelemetryOptions.Durable</c>. Without a custom
/// sampler, uses parent-based sampling with roots enabled. Maps to <c>OpenTelemetryIdGenerator</c> in <c>durable-span-ids.ts</c>.
/// </summary>
/// <remarks>
/// .NET has no id-generator hook, and <see cref="Activity.SpanId"/> cannot be set. The requested trace id, sampling decision, and trace
/// state reach a span through its parent <see cref="ActivityContext"/>. The requested span id does not: a span keeps a random id, so the
/// root and step spans of a run do not link by their derived ids across processes.
/// </remarks>
public sealed class OpenTelemetryIdGenerator
{
    private readonly OpenTelemetryIdGeneratorOptions _options;

    private readonly ThreadLocal<DurableRequestedIds?> _requested = new();

    /// <summary>Creates the bridge.</summary>
    /// <param name="options">Options. Null uses the defaults.</param>
    public OpenTelemetryIdGenerator(OpenTelemetryIdGeneratorOptions? options = null) =>
        _options = options ?? new OpenTelemetryIdGeneratorOptions();

    /// <summary>Maps to <c>generateTraceId</c>.</summary>
    public string GenerateTraceId() => _requested.Value?.TraceId ?? RandomHex(16);

    /// <summary>Maps to <c>generateSpanId</c>.</summary>
    public string GenerateSpanId() => _requested.Value?.SpanId ?? RandomHex(8);

    /// <summary>Maps to <c>shouldSample</c>.</summary>
    public DurableSamplingResult ShouldSample(
        ActivityContext parentContext,
        string traceId,
        string spanName,
        ActivityKind spanKind,
        IReadOnlyDictionary<string, object?> attributes
    )
    {
        if (_requested.Value is { SamplingDecision: { } decision } requested)
        {
            return new DurableSamplingResult
            {
                Decision = decision,
                TraceState = requested.TraceState ?? EmptyToNull(parentContext.TraceState),
            };
        }

        if (_options.Sampler is { } sampler)
        {
            return sampler(parentContext, traceId, spanName, spanKind, attributes);
        }

        var hasParent = parentContext != default && parentContext.TraceId != default;
        return new DurableSamplingResult
        {
            Decision =
                !hasParent || (parentContext.TraceFlags & ActivityTraceFlags.Recorded) != 0 ? 2 : 0,
            TraceState = EmptyToNull(parentContext.TraceState),
        };
    }

    /// <summary>Maps to <c>toString</c>.</summary>
    public override string ToString() => "AiSdkDurableSampler";

    /// <summary>Captures the tracing identity of a durable run. Maps to <c>captureContext</c>.</summary>
    public DurableTelemetryContext CaptureContext(
        string callId,
        string name,
        ActivityKind kind,
        IReadOnlyDictionary<string, object?> attributes
    )
    {
        var current = Activity.Current?.Context ?? default;
        var hasParent = current != default && current.TraceId != default;
        var traceId = hasParent ? current.TraceId.ToHexString() : DeriveTraceId(callId);
        var sampling = ShouldSample(current, traceId, name, kind, attributes);
        var rootAttributes = attributes.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal
        );
        if (sampling.Attributes is not null)
        {
            foreach (var attribute in sampling.Attributes)
            {
                rootAttributes[attribute.Key] = attribute.Value;
            }
        }

        return new DurableTelemetryContext
        {
            TraceId = traceId,
            RootSpanId = DeriveSpanId(callId, "root"),
            TraceFlags = sampling.Decision == 2 ? (int)ActivityTraceFlags.Recorded : 0,
            SamplingDecision = sampling.Decision,
            RootAttributes = rootAttributes,
            TraceState = sampling.TraceState
                ?? (hasParent ? EmptyToNull(current.TraceState) : null),
            ParentSpanContext = hasParent
                ? new DurableParentSpanContext
                {
                    TraceId = current.TraceId.ToHexString(),
                    SpanId = current.SpanId.ToHexString(),
                    TraceFlags = (int)current.TraceFlags,
                    TraceState = EmptyToNull(current.TraceState),
                }
                : null,
        };
    }

    /// <summary>Scopes requested ids to this bridge and restores nested requests. Maps to <c>withIds</c>.</summary>
    public T WithIds<T>(DurableRequestedIds ids, Func<T> startSpan)
    {
        var previous = _requested.Value;
        _requested.Value = ids;
        try
        {
            return startSpan();
        }
        finally
        {
            _requested.Value = previous;
        }
    }

    /// <summary>
    /// Applies the ids requested by <see cref="WithIds{T}"/> to the parent of a span that is about to start. The trace id, the sampling
    /// decision, and the trace state travel in the returned <see cref="ActivityContext"/>.
    /// </summary>
    internal ActivityContext ApplyRequested(
        ActivityContext parent,
        string spanName,
        ActivityKind kind,
        IReadOnlyDictionary<string, object?> attributes
    )
    {
        if (_requested.Value is not { } requested)
        {
            return parent;
        }

        var traceId = requested.TraceId is { } id
            ? ActivityTraceId.CreateFromString(id.AsSpan())
            : parent.TraceId;
        if (traceId == default)
        {
            return parent;
        }

        var sampling = ShouldSample(parent, traceId.ToHexString(), spanName, kind, attributes);
        return new ActivityContext(
            traceId,
            parent.SpanId,
            sampling.Decision == 2 ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None,
            sampling.TraceState,
            isRemote: parent.IsRemote || parent.SpanId == default
        );
    }

    /// <summary>Derives the trace ID of a durable run from its telemetry call ID. Maps to <c>deriveTraceId</c>.</summary>
    public static string DeriveTraceId(string callId) => DeriveHex(callId + ":trace", 4);

    /// <summary>Derives a span ID of a durable run from its telemetry call ID. Maps to <c>deriveSpanId</c>.</summary>
    public static string DeriveSpanId(string callId, params object[] path)
    {
        var parts = new string[path.Length + 1];
        parts[0] = callId;
        for (var i = 0; i < path.Length; i++)
        {
            parts[i + 1] = Convert.ToString(path[i], CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return DeriveHex(string.Join(":", parts), 2);
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private static string RandomHex(int bytes)
    {
        var values = new byte[bytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(values);
        var hex = new StringBuilder(bytes * 2);
        foreach (var value in values)
        {
            hex.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return NonZero(hex.ToString());
    }

    // Non-cryptographic hash (FNV-1a with one seed per 32-bit word). Collisions only matter between spans of the same run.
    private static string DeriveHex(string input, int words)
    {
        var hex = new StringBuilder(words * 8);
        for (var word = 0; word < words; word++)
        {
            unchecked
            {
                var hash = 0x811c9dc5u ^ ((uint)(word + 1) * 0x9e3779b1u);
                foreach (var character in input)
                {
                    hash ^= character;
                    hash *= 0x01000193u;
                }

                hex.Append(hash.ToString("x8", CultureInfo.InvariantCulture));
            }
        }

        return NonZero(hex.ToString());
    }

    // All-zero IDs are invalid in OpenTelemetry.
    private static string NonZero(string hex) =>
        hex.All(c => c == '0') ? hex.Substring(0, hex.Length - 1) + "1" : hex;
}
