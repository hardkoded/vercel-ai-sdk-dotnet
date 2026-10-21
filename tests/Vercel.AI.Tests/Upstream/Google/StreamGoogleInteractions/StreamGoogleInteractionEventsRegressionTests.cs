// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.StreamGoogleInteractions;

/// <summary>Defects the upstream tests do not cover. Upstream has no matching <c>it</c>.</summary>
public sealed class StreamGoogleInteractionEventsRegressionTests
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private const string Id = "v1_regression";

    private static string Sse(params string[] events)
    {
        return string.Concat(events.Select(e => "data: " + e + "\n\n"));
    }

    [Fact]
    public async Task Counts_only_failures_in_a_row_against_maxRetries()
    {
        // Each connection delivers an event and then drops. Only consecutive failures count, so a long run survives.
        var handler = new SequencedHandler()
            .FailingSse(Sse("{\"event_type\":\"step.delta\",\"event_id\":\"evt-1\",\"index\":0}"))
            .FailingSse(Sse("{\"event_type\":\"step.delta\",\"event_id\":\"evt-2\",\"index\":0}"))
            .Sse(Sse("{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-3\",\"interaction\":{\"id\":\"" + Id + "\",\"status\":\"completed\"}}"));

        var events = new List<GoogleInteractionsEvent>();
        await foreach (var item in GoogleInteractionsStreaming.StreamEvents(new ProviderHttp(new HttpClient(handler)), BaseUrl, Id, new Dictionary<string, string?>(), 2, TimeSpan.FromMilliseconds(1), CancellationToken.None))
        {
            events.Add(item);
        }

        Assert.Equal(new List<string?> { "step.delta", "step.delta", "interaction.completed" }, events.Select(e => e.EventType).ToList());
        Assert.Equal(3, handler.Uris.Count);
    }

    [Fact]
    public void Closes_text_blocks_that_are_still_open_before_the_finish_part()
    {
        var transform = new GoogleInteractionsStreamTransform(new List<CallWarning>(), false);
        var parts = new List<LanguageModelStreamPart>();
        foreach (var json in new[]
        {
            "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"" + Id + "\"}}",
            "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"model_output\"}}",
            "{\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"partial\"}}",
            "{\"event_type\":\"error\",\"error\":{\"message\":\"boom\"}}",
        })
        {
            parts.AddRange(transform.Transform(new GoogleInteractionsEvent(json)));
        }

        parts.AddRange(transform.Flush());

        var types = parts.Select(part => part.Type).ToList();
        Assert.Equal(new[] { "response-metadata", "text-start", "text-delta", "error", "text-end", "finish" }, types);
        Assert.Equal(Assert.IsType<TextStartStreamPart>(parts[1]).Id, Assert.IsType<TextEndStreamPart>(parts[4]).Id);
    }

    [Fact]
    public void Does_not_close_a_text_block_twice()
    {
        var transform = new GoogleInteractionsStreamTransform(new List<CallWarning>(), false);
        var parts = new List<LanguageModelStreamPart>();
        foreach (var json in new[]
        {
            "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"model_output\"}}",
            "{\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"done\"}}",
            "{\"event_type\":\"step.stop\",\"index\":0}",
        })
        {
            parts.AddRange(transform.Transform(new GoogleInteractionsEvent(json)));
        }

        parts.AddRange(transform.Flush());

        Assert.Single(parts.OfType<TextEndStreamPart>());
    }
}
