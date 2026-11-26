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
    internal sealed record Published(
        string Channel,
        TelemetryTracingChannelMessage Message,
        TelemetryTracingChannelMessage? Parent
    );

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

    /// <summary>
    /// Maps to <c>collectTracingChannelParentage</c>: the type of each span that starts and the type of its parent. Node reads the
    /// parent from the store bound to the <c>start</c> sub-channel. The port reads <see cref="Vercel.AI.TracingChannel.Current"/>.
    /// </summary>
    internal static async Task<IReadOnlyList<string>> CollectParentage(
        string callId,
        Func<Task> run
    )
    {
        var published = await Collect(run);
        return published
            .Where(item => item.Channel == "start" && CallIdOf(item.Message) == callId)
            .Select(item => $"{item.Message.Type} parent {item.Parent?.Type ?? "undefined"}")
            .ToList();
    }

    /// <summary>
    /// Maps to <c>collectTracingChannelAsyncEndMessages</c>: the messages of the <c>asyncEnd</c> sub-channel of one call.
    /// </summary>
    internal static async Task<
        IReadOnlyList<TelemetryTracingChannelMessage>
    > CollectAsyncEndMessages(string callId, Func<Task> run)
    {
        var published = await Collect(run);
        return published
            .Where(item => item.Channel == "asyncEnd" && CallIdOf(item.Message) == callId)
            .Select(item => item.Message)
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
            Vercel.AI.Operations.EmbedStartEvent embed => embed.CallId,
            Vercel.AI.Operations.RerankStartEvent rerank => rerank.CallId,
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
                    published.Add(
                        new Published(item.Key, message, Vercel.AI.TracingChannel.Current)
                    );
                }
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }

    /// <summary>Maps to <c>MockEmbeddingModelV4</c> for the tracing channel tests.</summary>
    internal sealed class MockEmbeddingModel : Vercel.AI.Operations.IEmbeddingCaller
    {
        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public string SpecificationVersion => "v4";

        public int? MaxEmbeddingsPerCall { get; init; }

        public double? MaxInputBytesPerCall => null;

        public bool SupportsParallelCalls => true;

        public Task<System.Text.Json.JsonElement?> TransformProviderOptionsAsync(
            System.Text.Json.JsonElement? providerOptions,
            IReadOnlyList<string> values,
            int startIndex,
            int endIndex,
            CancellationToken cancellationToken
        ) => Task.FromResult(providerOptions);

        public Task<Vercel.AI.Operations.EmbeddingModelResponse> DoEmbedAsync(
            Vercel.AI.Operations.EmbeddingModelCall call,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new Vercel.AI.Operations.EmbeddingModelResponse(
                    call.Values.Select(_ => new[] { 0.1, 0.2, 0.3 }).ToArray(),
                    10
                )
            );
    }

    /// <summary>Maps to <c>MockRerankingModelV4</c> for the tracing channel tests.</summary>
    internal sealed class MockRerankingModel : Vercel.AI.Operations.IRerankCaller
    {
        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public Task<Vercel.AI.Operations.RerankModelResponse> DoRerankAsync(
            Vercel.AI.Operations.RerankModelCall call,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new Vercel.AI.Operations.RerankModelResponse(
                    call.Documents.Values.Count == 1
                        ? [new Vercel.AI.Operations.RerankModelRank(0, 0.9)]
                        :
                        [
                            new Vercel.AI.Operations.RerankModelRank(2, 0.9),
                            new Vercel.AI.Operations.RerankModelRank(0, 0.8),
                        ]
                )
            );
    }
}
