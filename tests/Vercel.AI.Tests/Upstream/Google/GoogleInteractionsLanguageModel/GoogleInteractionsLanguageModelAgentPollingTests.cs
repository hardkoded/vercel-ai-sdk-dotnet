// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel agent polling</c>.
/// The two tests that surface image and video outputs as file parts are not ported: <c>FileStreamPart</c> carries bytes only,
/// so it cannot hold the <c>uri</c> outputs they check.
/// </summary>
public sealed class GoogleInteractionsLanguageModelAgentPollingTests
{
    private const string AgentName = "deep-research-pro-preview-12-2025";
    private const string PollId = "v1_poll-test";

    private static string Sse(params string[] events)
    {
        return string.Concat(events.Select(e => "data: " + e + "\n\n"));
    }

    private static GoogleProvider Provider(SequencedHandler handler)
    {
        return GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
    }

    private static LanguageModelCallOptions BackgroundOptions()
    {
        var options = GoogleUpstream.Hello();
        options.IncludeRawChunks = false;
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"background\":true}}");
        return options;
    }

    private static Dictionary<string, string> Query(string uri)
    {
        var query = new Uri(uri).Query.TrimStart('?');
        return query.Length == 0
            ? new Dictionary<string, string>()
            : query.Split('&').Select(pair => pair.Split('=')).ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
    }

    private static async Task<List<LanguageModelStreamPart>> StreamParts(ILanguageModel model, LanguageModelCallOptions options)
    {
        var result = await model.DoStreamAsync(options, CancellationToken.None);
        return await GoogleUpstream.Collect(result.Stream);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::returns the polled terminal response from doGenerate when POST returns in_progress", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_polled_terminal_response_from_doGenerate_when_POST_returns_in_progress()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"in_progress\",\"model\":\"deep-research-pro-preview-12-2025\"}")
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"in_progress\"}")
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"researched answer\"}]}],\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":3,\"total_tokens\":8}}");
        var agentModel = Provider(handler).Interactions(new GoogleInteractionsAgent(AgentName));

        var result = await agentModel.DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Contains(result.Content, content => content is GeneratedText text && text.Text == "researched answer");
        Assert.Equal(new[] { "POST", "GET", "GET" }, handler.Methods);
        Assert.Equal(AgentName, (string?)JsonNode.Parse(handler.Bodies[0])!["agent"]);
        Assert.Null(JsonNode.Parse(handler.Bodies[0])!["model"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::polls doGenerate until terminal for model calls with background: true", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_doGenerate_until_terminal_for_model_calls_with_background_true()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"v1_poll-test\",\"status\":\"in_progress\"}")
            .Json("{\"id\":\"v1_poll-test\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"long answer\"}]}],\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":100000,\"total_tokens\":100005}}");
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 200_000;
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"background\":true}}");

        var result = await provider.Interactions("gemini-4-argon").DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!;
        Assert.Equal("gemini-4-argon", (string?)body["model"]);
        Assert.True((bool?)body["background"]);
        Assert.Equal(200_000, (int?)body["generation_config"]!["max_output_tokens"]);
        Assert.Equal("GET", handler.Methods[1]);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("long answer", Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content)).Text);
        Assert.Equal(100_000, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::opens GET /interactions/{id}?stream=true and pipes SSE events through when POST is non-terminal", Coverage = UpstreamCoverage.Covered)]
    public async Task Opens_GET_interactions_id_stream_true_and_pipes_SSE_events_through_when_POST_is_non_terminal()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"in_progress\"}")
            .Sse(Sse(
                "{\"event_type\":\"interaction.created\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + PollId + "\",\"status\":\"in_progress\",\"model\":\"deep-research-pro-preview-12-2025\"}}",
                "{\"event_type\":\"step.start\",\"event_id\":\"evt-2\",\"index\":0,\"step\":{\"type\":\"model_output\"}}",
                "{\"event_type\":\"step.delta\",\"event_id\":\"evt-3\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"streamed agent answer\"}}",
                "{\"event_type\":\"step.stop\",\"event_id\":\"evt-4\",\"index\":0}",
                "{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-5\",\"interaction\":{\"id\":\"" + PollId + "\",\"status\":\"completed\",\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":3,\"total_tokens\":8}}}"));
        var agentModel = Provider(handler).Interactions(new GoogleInteractionsAgent(AgentName));

        var parts = await StreamParts(agentModel, BackgroundOptions());

        var types = parts.Select(part => part.Type).ToList();
        Assert.Contains("text-start", types);
        Assert.Contains("text-delta", types);
        Assert.Contains("text-end", types);
        Assert.Contains("finish", types);
        Assert.Equal("streamed agent answer", parts.OfType<TextDeltaStreamPart>().First().Delta);
        Assert.Equal("POST", handler.Methods[0]);
        Assert.Equal("GET", handler.Methods[1]);
        var query = Query(handler.Uris[1]);
        Assert.Equal("true", query["stream"]);
        Assert.False(query.ContainsKey("last_event_id"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::reconnects to GET stream with last_event_id when the SSE connection drops", Coverage = UpstreamCoverage.Covered)]
    public async Task Reconnects_to_GET_stream_with_last_event_id_when_the_SSE_connection_drops()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"in_progress\"}")
            .Sse(Sse(
                "{\"event_type\":\"interaction.created\",\"event_id\":\"evt-1\",\"interaction\":{\"id\":\"" + PollId + "\",\"status\":\"in_progress\",\"model\":\"deep-research-pro-preview-12-2025\"}}",
                "{\"event_type\":\"step.start\",\"event_id\":\"evt-2\",\"index\":0,\"step\":{\"type\":\"model_output\"}}",
                "{\"event_type\":\"step.delta\",\"event_id\":\"evt-3\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"first half \"}}"))
            .Sse(Sse(
                "{\"event_type\":\"step.delta\",\"event_id\":\"evt-4\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"second half\"}}",
                "{\"event_type\":\"step.stop\",\"event_id\":\"evt-5\",\"index\":0}",
                "{\"event_type\":\"interaction.completed\",\"event_id\":\"evt-6\",\"interaction\":{\"id\":\"" + PollId + "\",\"status\":\"completed\",\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":3,\"total_tokens\":8}}}"));
        var agentModel = Provider(handler).Interactions(new GoogleInteractionsAgent(AgentName));

        var parts = await StreamParts(agentModel, BackgroundOptions());

        Assert.Equal(new[] { "first half ", "second half" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal(new[] { "POST", "GET", "GET" }, handler.Methods);
        Assert.False(Query(handler.Uris[1]).ContainsKey("last_event_id"));
        Assert.Equal("evt-3", Query(handler.Uris[2])["last_event_id"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::synthesizes a complete stream when POST already returns a terminal status", Coverage = UpstreamCoverage.Covered)]
    public async Task Synthesizes_a_complete_stream_when_POST_already_returns_a_terminal_status()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"" + PollId + "\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"instant answer\"}]}],\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":3,\"total_tokens\":8}}");
        var agentModel = Provider(handler).Interactions(new GoogleInteractionsAgent(AgentName));

        var parts = await StreamParts(agentModel, BackgroundOptions());

        var types = parts.Select(part => part.Type).ToList();
        Assert.Contains("text-start", types);
        Assert.Contains("text-delta", types);
        Assert.Contains("text-end", types);
        Assert.Contains("finish", types);
        Assert.Equal("instant answer", parts.OfType<TextDeltaStreamPart>().First().Delta);
        Assert.Single(handler.Methods);
        Assert.Equal("POST", handler.Methods[0]);
    }
}
