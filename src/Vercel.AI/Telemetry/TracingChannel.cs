// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// The telemetry tracing channel. Maps to <c>telemetry/tracing-channel.ts</c>. Upstream publishes on a Node
/// <c>diagnostics_channel</c> tracing channel. The port publishes on a <see cref="System.Diagnostics.DiagnosticListener"/> with
/// the same name, one event per sub-channel of the tracing channel: <c>start</c>, <c>end</c>, <c>asyncStart</c>, <c>asyncEnd</c> and
/// <c>error</c>. Each event value is a <see cref="TelemetryTracingChannelMessage"/>.
/// </summary>
public static class TracingChannel
{
    /// <summary>The name of the tracing channel. Maps to <c>AI_SDK_TELEMETRY_TRACING_CHANNEL</c>.</summary>
    public const string AiSdkTelemetryTracingChannel = "ai:telemetry";
}

/// <summary>The kinds of span the tracing channel publishes. Maps to <c>TelemetryTracingEventType</c>.</summary>
public static class TelemetryTracingEventType
{
    /// <summary>Maps to <c>'generateText'</c>.</summary>
    public const string GenerateText = "generateText";

    /// <summary>Maps to <c>'streamText'</c>.</summary>
    public const string StreamText = "streamText";

    /// <summary>Maps to <c>'step'</c>.</summary>
    public const string Step = "step";

    /// <summary>Maps to <c>'languageModelCall'</c>.</summary>
    public const string LanguageModelCall = "languageModelCall";

    /// <summary>Maps to <c>'executeTool'</c>.</summary>
    public const string ExecuteTool = "executeTool";

    /// <summary>Maps to <c>'embed'</c>.</summary>
    public const string Embed = "embed";

    /// <summary>Maps to <c>'embedMany'</c>.</summary>
    public const string EmbedMany = "embedMany";

    /// <summary>Maps to <c>'rerank'</c>.</summary>
    public const string Rerank = "rerank";

    /// <summary>Maps to <c>'generateSpeech'</c>.</summary>
    public const string GenerateSpeech = "generateSpeech";

    /// <summary>Maps to <c>'transcribe'</c>.</summary>
    public const string Transcribe = "transcribe";

    /// <summary>Maps to <c>'streamTranscribe'</c>.</summary>
    public const string StreamTranscribe = "streamTranscribe";

    /// <summary>Maps to <c>'experimental_decide'</c>.</summary>
    public const string ExperimentalDecide = "experimental_decide";

    /// <summary>Maps to <c>'experimental_extract'</c>.</summary>
    public const string ExperimentalExtract = "experimental_extract";
}

/// <summary>
/// The value a tracing channel subscriber receives. Maps to <c>TelemetryTracingChannelMessage</c>. <see cref="Result"/> and
/// <see cref="Error"/> are set before the <c>asyncEnd</c> and <c>error</c> events, like the context object of a Node tracing channel.
/// </summary>
public sealed class TelemetryTracingChannelMessage
{
    /// <summary>Creates a message.</summary>
    public TelemetryTracingChannelMessage(string type, object? @event)
    {
        Type = type;
        Event = @event;
    }

    /// <summary>The kind of span. One of the <see cref="TelemetryTracingEventType"/> values.</summary>
    public string Type { get; }

    /// <summary>The telemetry event of the span, already filtered and redacted.</summary>
    public object? Event { get; }

    /// <summary>The result of the operation. Set on <c>asyncEnd</c>, and only when inputs and outputs are recorded.</summary>
    public object? Result { get; set; }

    /// <summary>The error of the operation. Set on <c>error</c>.</summary>
    public Exception? Error { get; set; }
}

/// <summary>
/// The event of a <c>step</c> span. Upstream publishes the object literal <c>{ callId, stepNumber }</c>, which needs a type here.
/// </summary>
public sealed class TracingChannelStepEvent
{
    /// <summary>Creates the event of a step span.</summary>
    public TracingChannelStepEvent(string callId, int stepNumber)
    {
        CallId = callId;
        StepNumber = stepNumber;
    }

    /// <summary>The id of the call that the step belongs to.</summary>
    public string CallId { get; }

    /// <summary>The number of the step, starting at zero.</summary>
    public int StepNumber { get; }

    /// <summary>Whether inputs are recorded. Null when the call does not say.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Whether outputs are recorded. Null when the call does not say.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>The function id of the telemetry settings.</summary>
    public string? FunctionId { get; init; }

    /// <summary>The version of the SDK.</summary>
    public string? AiVersion { get; init; }
}
