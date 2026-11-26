// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// The event an integration receives in <see cref="ITelemetry.OnError"/>. Upstream passes the plain object
/// <c>{ callId, error, aiVersion }</c> to <c>onError</c>; the port needs a type for it.
/// </summary>
public sealed class TelemetryErrorEvent
{
    /// <summary>Creates an event.</summary>
    public TelemetryErrorEvent(string? callId, Exception error)
    {
        CallId = callId;
        Error = error;
    }

    /// <summary>Unique identifier of the failed call.</summary>
    public string? CallId { get; }

    /// <summary>The error. A download error arrives without its URL.</summary>
    public Exception Error { get; }

    /// <summary>The version of the AI SDK that emitted this event.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>.</summary>
    public string? FunctionId { get; init; }
}
