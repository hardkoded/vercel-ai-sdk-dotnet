// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.StreamGoogleInteractions;

/// <summary>Port of <c>stream-google-interactions.test.ts</c> &gt; <c>streamGoogleInteractionEvents</c>.</summary>
public sealed class StreamGoogleInteractionEventsTests
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private const string InteractionId = "v1_test-stream-id";
    private const string StreamUrl = BaseUrl + "/interactions/" + InteractionId;
    private const string CancelUrl = StreamUrl + "/cancel";

    private static readonly IReadOnlyDictionary<string, string?> NoHeaders = new Dictionary<string, string?>();

    private static string Sse(params string[] events)
    {
        return string.Concat(events.Select(e => "data: " + e + "\n\n"));
    }

    private static IAsyncEnumerable<GoogleInteractionsEvent> Stream(SequencedHandler handler, int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        return GoogleInteractionsStreaming.StreamEvents(
            new ProviderHttp(new HttpClient(handler)),
            BaseUrl,
            InteractionId,
            NoHeaders,
            maxRetries,
            TimeSpan.FromMilliseconds(1),
            cancellationToken);
    }

    private static async Task<List<GoogleInteractionsEvent>> Collect(IAsyncEnumerable<GoogleInteractionsEvent> stream)
    {
        var list = new List<GoogleInteractionsEvent>();
        await foreach (var item in stream.ConfigureAwait(false))
        {
            list.Add(item);
        }

        return list;
    }

    private static List<string?> EventTypes(IEnumerable<GoogleInteractionsEvent> events)
    {
        return events.Where(e => e.Value != null).Select(e => e.EventType).ToList();
    }

    private static Dictionary<string, string> Query(string uri)
    {
        var query = new Uri(uri).Query.TrimStart('?');
        return query.Length == 0
            ? new Dictionary<string, string>()
            : query.Split('&').Select(pair => pair.Split('=')).ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/stream-google-interactions.test.ts::streamGoogleInteractionEvents::reads the SSE feed and exits when interaction.complete arrives", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_the_SSE_feed_and_exits_when_interaction_complete_arrives()
    {
        var handler = new SequencedHandler().Sse(Sse(
            "{\"event_type\":\"interaction.created\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}}",
            "{\"event_type\":\"step.start\",\"event_id\":\"evt-2\",\"index\":0,\"content\":{\"type\":\"text\"}}",
            "{\"event_type\":\"step.delta\",\"event_id\":\"evt-3\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"hello\"}}",
            "{\"event_type\":\"step.stop\",\"event_id\":\"evt-4\",\"index\":0}",
            "{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-5\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"completed\"}}"));

        var parsed = await Collect(Stream(handler));

        Assert.Equal(new List<string?> { "interaction.created", "step.start", "step.delta", "step.stop", "interaction.completed" }, EventTypes(parsed));
        Assert.Single(handler.Uris);
        var query = Query(handler.Uris[0]);
        Assert.Equal("true", query["stream"]);
        Assert.False(query.ContainsKey("last_event_id"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/stream-google-interactions.test.ts::streamGoogleInteractionEvents::reconnects with last_event_id when the stream ends without completion", Coverage = UpstreamCoverage.Covered)]
    public async Task Reconnects_with_last_event_id_when_the_stream_ends_without_completion()
    {
        var handler = new SequencedHandler()
            .Sse(Sse(
                "{\"event_type\":\"interaction.created\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}}",
                "{\"event_type\":\"step.start\",\"event_id\":\"evt-2\",\"index\":0,\"content\":{\"type\":\"text\"}}",
                "{\"event_type\":\"step.delta\",\"event_id\":\"evt-3\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"first\"}}"))
            .Sse(Sse(
                "{\"event_type\":\"step.delta\",\"event_id\":\"evt-4\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"second\"}}",
                "{\"event_type\":\"step.stop\",\"event_id\":\"evt-5\",\"index\":0}",
                "{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-6\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"completed\"}}"));

        var parsed = await Collect(Stream(handler));

        Assert.Equal(new List<string?> { "interaction.created", "step.start", "step.delta", "step.delta", "step.stop", "interaction.completed" }, EventTypes(parsed));
        Assert.Equal(2, handler.Uris.Count);
        Assert.Equal("evt-3", Query(handler.Uris[1])["last_event_id"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/stream-google-interactions.test.ts::streamGoogleInteractionEvents::fires POST /interactions/{id}/cancel when the abort signal fires mid-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Fires_POST_interactions_id_cancel_when_the_abort_signal_fires_mid_stream()
    {
        var handler = new SequencedHandler()
            .HangingSse(Sse("{\"event_type\":\"interaction.created\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}}"))
            .Json("{\"id\":\"" + InteractionId + "\",\"status\":\"cancelled\"}");
        using var abort = new CancellationTokenSource();

        await using var enumerator = Stream(handler, cancellationToken: abort.Token).GetAsyncEnumerator(abort.Token);
        Assert.True(await enumerator.MoveNextAsync());
        abort.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());

        var cancelIndexes = Enumerable.Range(0, handler.Uris.Count).Where(i => handler.Uris[i] == CancelUrl).ToList();
        var cancelIndex = Assert.Single(cancelIndexes);
        Assert.Equal("POST", handler.Methods[cancelIndex]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/stream-google-interactions.test.ts::streamGoogleInteractionEvents::does not fire cancel when the stream completes normally", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_fire_cancel_when_the_stream_completes_normally()
    {
        var handler = new SequencedHandler().Sse(Sse(
            "{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + InteractionId + "\",\"status\":\"completed\"}}"));

        await Collect(Stream(handler));

        Assert.DoesNotContain(CancelUrl, handler.Uris);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/stream-google-interactions.test.ts::streamGoogleInteractionEvents::errors out after maxRetries empty connections", Coverage = UpstreamCoverage.Covered)]
    public async Task Errors_out_after_maxRetries_empty_connections()
    {
        var handler = new SequencedHandler().Sse(string.Empty);

        var error = await Assert.ThrowsAsync<AiSdkException>(() => Collect(Stream(handler, maxRetries: 2)));

        Assert.Matches("(?i)closed without producing any events", error.Message);
        Assert.Equal(2, handler.Uris.Count);
    }
}
