// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>Serializable tracing identity shared by the processes of a durable run. Maps to <c>DurableTelemetryContext</c> in <c>telemetry/telemetry.ts</c>.</summary>
public sealed class DurableTelemetryContext
{
    /// <summary>The trace id, as 32 lowercase hex characters. Maps to <c>traceId</c>.</summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>The span id of the run's root span, as 16 lowercase hex characters. Maps to <c>rootSpanId</c>.</summary>
    public string RootSpanId { get; init; } = string.Empty;

    /// <summary>The integration's root-span name, reused when the run is reported. Maps to <c>rootSpanName</c>.</summary>
    public string? RootSpanName { get; init; }

    /// <summary>The integration's root-span attributes, reused when the run is reported. Maps to <c>rootAttributes</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RootAttributes { get; init; }

    /// <summary>The W3C trace flags. Maps to <c>traceFlags</c>.</summary>
    public int TraceFlags { get; init; }

    /// <summary>The serialized W3C trace state. Maps to <c>traceState</c>.</summary>
    public string? TraceState { get; init; }

    /// <summary>The sampling decision: 0 not recorded, 1 recorded, 2 recorded and sampled. Maps to <c>samplingDecision</c>.</summary>
    public int SamplingDecision { get; init; }

    /// <summary>The span the run was started under, when there was one. Maps to <c>parentSpanContext</c>.</summary>
    public DurableParentSpanContext? ParentSpanContext { get; init; }
}

/// <summary>The span a durable run was started under. Maps to the <c>parentSpanContext</c> of <c>DurableTelemetryContext</c>.</summary>
public sealed class DurableParentSpanContext
{
    /// <summary>Maps to <c>traceId</c>.</summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>Maps to <c>spanId</c>.</summary>
    public string SpanId { get; init; } = string.Empty;

    /// <summary>Maps to <c>traceFlags</c>.</summary>
    public int TraceFlags { get; init; }

    /// <summary>Maps to <c>traceState</c>.</summary>
    public string? TraceState { get; init; }
}

/// <summary>Whether a durable event is reported live from a step or replayed after the run completed. Maps to the <c>phase</c> of <c>DurableTelemetryEventContext</c>.</summary>
public enum DurableTelemetryPhase
{
    /// <summary>Reported live from a durable step. Maps to <c>live</c>.</summary>
    Live,

    /// <summary>Reported after the run completed. Maps to <c>replay</c>.</summary>
    Replay,
}

/// <summary>Identifies live model events and the run events reported after completion. Maps to <c>DurableTelemetryEventContext</c>.</summary>
public sealed class DurableTelemetryEventContext
{
    /// <summary>The tracing identity of the run. Maps to <c>run</c>.</summary>
    public DurableTelemetryContext Run { get; init; } = new();

    /// <summary>The step the event belongs to. Maps to <c>stepNumber</c>.</summary>
    public int? StepNumber { get; init; }

    /// <summary>Maps to <c>phase</c>.</summary>
    public DurableTelemetryPhase Phase { get; init; }

    /// <summary>Context that was already filtered before crossing the step boundary. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }

    /// <summary>Maps to <c>headers</c>.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; init; }
}

/// <summary>The durable hook of a telemetry integration. Maps to the type of <c>experimental_durable</c> in <c>Telemetry</c>.</summary>
public interface IDurableTelemetry
{
    /// <summary>
    /// Captures a serializable tracing identity before a durable run's model calls. Return <c>null</c> to preserve live per-model
    /// operations. Maps to <c>captureContext</c>.
    /// </summary>
    Task<DurableTelemetryContext?> CaptureContextAsync(
        GenerateTextStartEvent @event,
        CancellationToken cancellationToken = default
    );
}
