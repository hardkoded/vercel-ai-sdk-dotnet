// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace Vercel.AI;

/// <summary>
/// Publishes spans on the telemetry tracing channel. Maps to <c>telemetry/tracing-channel-publisher.ts</c>.
/// </summary>
internal static class TracingChannelPublisher
{
    private static readonly DiagnosticListener s_tracingChannel = new(
        TracingChannel.AiSdkTelemetryTracingChannel
    );

    /// <summary>
    /// Runs an async operation inside the AI SDK telemetry tracing channel when tracing subscribers exist. Without a subscriber this is
    /// a direct pass-through. The original result or error of the operation is preserved if tracing itself throws, and the operation
    /// never runs twice. Maps to <c>runWithTracingChannelSpan</c>.
    /// </summary>
    internal static async Task<T> RunWithTracingChannelSpan<T>(
        TelemetryTracingChannelMessage message,
        Func<Task<T>> execute,
        bool recordResult = true
    )
    {
        if (!s_tracingChannel.IsEnabled())
        {
            return await execute().ConfigureAwait(false);
        }

        SafePublish("start", message);
        SafePublish("end", message);

        T result;
        try
        {
            result = await execute().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            // Raw operation errors may carry credentials, so only the redacted copy is published.
            message.Error = recordResult
                ? RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(error)
                : new InvalidOperationException("AI SDK operation failed");
            SafePublish("error", message);
            SafePublish("asyncStart", message);
            SafePublish("asyncEnd", message);
            throw;
        }

        // The result may hold both inputs and outputs, so it is published only when both are recorded.
        if (recordResult)
        {
            message.Result = result is GenerateTextResult generated
                ? RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(generated)
                : result;
        }

        SafePublish("asyncStart", message);
        SafePublish("asyncEnd", message);
        return result;
    }

    // Subscribers never affect the operation.
    private static void SafePublish(string name, TelemetryTracingChannelMessage message)
    {
        try
        {
            s_tracingChannel.Write(name, message);
        }
        catch (Exception)
        {
            // Diagnostics subscribers should never affect SDK behavior.
        }
    }
}
