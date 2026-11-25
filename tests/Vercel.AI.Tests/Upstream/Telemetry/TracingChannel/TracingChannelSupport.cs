// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;

/// <summary>
/// Subscribes to the telemetry tracing channel for the tests of <c>tracing-channel.test.ts</c>. Maps to its
/// <c>collectTracingChannel*</c> helpers. The tracing channel is a process-wide listener, so a collection keeps only the
/// messages of the call ids it was asked about.
/// </summary>
internal static class TracingChannelSupport
{
    /// <summary>One event of the tracing channel: the sub-channel name and the message.</summary>
    internal sealed record Published(string Channel, TelemetryTracingChannelMessage Message);

    /// <summary>Runs <paramref name="run"/> while subscribed and returns every event published meanwhile.</summary>
    internal static async Task<IReadOnlyList<Published>> Collect(Func<Task> run)
    {
        var published = new List<Published>();
        using var subscription = Subscribe(published);
        await run();
        return published.ToArray();
    }

    /// <summary>Maps to <c>collectTracingChannelStartMessages</c>: the messages of the <c>start</c> sub-channel.</summary>
    internal static async Task<IReadOnlyList<TelemetryTracingChannelMessage>> CollectStartMessages(
        string callId,
        Func<Task> run
    )
    {
        var published = await Collect(run);
        return published
            .Where(item => item.Channel == "start" && CallIdOf(item.Message) == callId)
            .Select(item => item.Message)
            .ToList();
    }

    /// <summary>
    /// Maps to <c>collectTracingChannelEventSequence</c>. Node reports <c>bindStart</c>, which has no .NET counterpart, so the
    /// sequence has <c>start</c>.
    /// </summary>
    internal static async Task<IReadOnlyList<string>> CollectEventSequence(
        string callId,
        Func<Task> run
    )
    {
        var published = await Collect(run);
        return published
            .Where(item =>
                item.Channel is "start" or "asyncEnd" && CallIdOf(item.Message) == callId
            )
            .Select(item => $"{item.Channel} {item.Message.Type}")
            .ToList();
    }

    // The tracing channel is shared by every test of the process, so a collection keeps the spans of its own call.
    internal static string? CallIdOf(TelemetryTracingChannelMessage message)
    {
        return message.Event switch
        {
            Vercel.AI.Operations.ExtractStartEvent extract => extract.CallId,
            Vercel.AI.Operations.DecideEvent decide => decide.CallId,
            GenerateTextStartEvent text => text.CallId,
            TracingChannelStepEvent step => step.CallId,
            LanguageModelCallStartEvent modelCall => modelCall.CallId,
            ToolExecutionStartEvent tool => tool.CallId,
            _ => null,
        };
    }

    private static IDisposable Subscribe(List<Published> published)
    {
        var observer = new ChannelObserver(published);
        return DiagnosticListener.AllListeners.Subscribe(observer);
    }

    private sealed class ChannelObserver(List<Published> published) : IObserver<DiagnosticListener>
    {
        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name != Vercel.AI.TracingChannel.AiSdkTelemetryTracingChannel)
            {
                return;
            }

            listener.Subscribe(new EventObserver(published));
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }

    private sealed class EventObserver(List<Published> published)
        : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> item)
        {
            if (item.Value is TelemetryTracingChannelMessage message)
            {
                lock (published)
                {
                    published.Add(new Published(item.Key, message));
                }
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
