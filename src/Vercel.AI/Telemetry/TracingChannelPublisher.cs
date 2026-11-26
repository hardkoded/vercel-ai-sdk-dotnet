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
            message.Result = result is null ? null : RedactResult(result);
        }

        SafePublish("asyncStart", message);
        SafePublish("asyncEnd", message);
        return result;
    }

    /// <summary>
    /// Opens a long-lived tracing channel span and returns a context that can re-enter it later. The span ends when
    /// <paramref name="completion"/> does. Without a subscriber, <paramref name="completion"/> is only observed and the result is
    /// null. Maps to <c>openTelemetryChannelSpanContext</c>.
    /// </summary>
    internal static TracingChannelContext? OpenTelemetryChannelSpanContext(
        TelemetryTracingChannelMessage message,
        Task<object?> completion,
        bool recordResult = true
    )
    {
        if (!s_tracingChannel.IsEnabled())
        {
            _ = completion.ContinueWith(
                static task => _ = task.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted
                    | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
            return null;
        }

        var asyncEndPublished = 0;
        SafePublish("start", message);
        SafePublish("end", message);

        // The continuation runs where the completion is set, so the span ends before the code that follows it.
        _ = completion.ContinueWith(
            task =>
            {
                if (Interlocked.Exchange(ref asyncEndPublished, 1) != 0)
                {
                    return;
                }

                if (task.IsFaulted || task.IsCanceled)
                {
                    var error = task.IsCanceled
                        ? new OperationCanceledException()
                        : task.Exception!.InnerException ?? task.Exception;
                    message.Error = recordResult
                        ? RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(error)
                        : new InvalidOperationException("AI SDK operation failed");
                    SafePublish("error", message);
                }
                else if (recordResult && task.Result is { } result)
                {
                    message.Result = RedactResult(result);
                }

                SafePublish("asyncEnd", message);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );

        return new TracingChannelContext();
    }

    private static object RedactResult(object result)
    {
        return result is GenerateTextResult generated
            ? RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(generated)
            : result;
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

/// <summary>
/// A tracing channel span context. Maps to <c>TracingChannelContext</c>. Node re-enters the span through an
/// <c>AsyncResource</c>. A <see cref="System.Diagnostics.DiagnosticListener"/> has no store to bind, so the port runs the delegate
/// where it is called.
/// </summary>
internal sealed class TracingChannelContext
{
    /// <summary>Runs <paramref name="execute"/> inside the span context. Maps to <c>run</c>.</summary>
    internal T Run<T>(Func<T> execute)
    {
        return execute();
    }
}
