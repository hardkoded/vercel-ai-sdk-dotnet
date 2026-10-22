// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>stateful chaining (previousInteractionId, store, thinking)</c>.
/// Not ported: <c>compacts assistant turns matching previousInteractionId out of the wire body</c>,
/// <c>compacts the turn-2 wire body and references prior interaction (fixture-driven)</c>, and
/// <c>round-trips a thought signature on an input reasoning part to the wire thought block</c>. The port sends the whole history and
/// does not read the interaction id or signature from a message part.
/// </summary>
public sealed class DoGenerateStatefulChainingTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > stateful chaining (previousInteractionId, store, thinking)::";

    private static LanguageModelCallOptions Options(string? providerOptions = null)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        if (providerOptions != null)
        {
            options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        }

        return options;
    }

    private static SequencedHandler Basic()
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits previous_interaction_id and omits store when previousInteractionId is set + default store", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_previous_interaction_id_and_omits_store_when_previousInteractionId_is_set_plus_default_store()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"previousInteractionId\":\"v1_prev-abc\"}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("v1_prev-abc", (string?)body["previous_interaction_id"]);
        Assert.False(body.ContainsKey("store"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits store: false (and no previous_interaction_id) for stateless mode", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_store_false_and_no_previous_interaction_id_for_stateless_mode()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"store\":false}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.False(body.ContainsKey("previous_interaction_id"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits both previous_interaction_id and store: false when both are set, and surfaces a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_both_previous_interaction_id_and_store_false_when_both_are_set_and_surfaces_a_warning()
    {
        var handler = Basic();

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"previousInteractionId\":\"v1_prev\",\"store\":false}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("v1_prev", (string?)body["previous_interaction_id"]);
        Assert.False((bool?)body["store"]);
        Assert.Contains(result.Warnings, warning => warning.Type == "other" && warning.Message.Contains("store: false"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits thinking_level and thinking_summaries inside generation_config", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_thinking_level_and_thinking_summaries_inside_generation_config()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"thinkingLevel\":\"high\",\"thinkingSummaries\":\"auto\"}}"), CancellationToken.None);

        var generationConfig = JsonNode.Parse(handler.Bodies[0])!["generation_config"]!;
        Assert.Equal("high", (string?)generationConfig["thinking_level"]);
        Assert.Equal("auto", (string?)generationConfig["thinking_summaries"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards topK and warns for unsupported penalties", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_topK_and_warns_for_unsupported_penalties()
    {
        var handler = Basic();
        var options = Options();
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(options, CancellationToken.None);

        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Bodies[0])!["generation_config"], "{\"top_k\":10}");
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("frequencyPenalty", warning.Feature);
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("presencePenalty", warning.Feature);
            });
    }

    [Fact]
    [UpstreamTest(Prefix + "returns interactionId for turn 1 from a captured fixture", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_interactionId_for_turn_1_from_a_captured_fixture()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("multi-turn-stateful-turn1"));

        var result = await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(
            GoogleUpstream.Prompt(new UserModelMessage("What are the three largest cities in Spain?")),
            CancellationToken.None);

        Assert.Equal(
            "v1_ChdWV3NIYXNYZEc5S19xdHNQcmVYeG1BRRIXVldzSGFzWGRHOUtfcXRzUHJlWHhtQUU",
            result.ProviderMetadata!.Value.GetProperty("google").GetProperty("interactionId").GetString());
        var text = string.Concat(result.Content.OfType<GeneratedText>().Select(part => part.Text));
        Assert.Contains("Madrid", text);
        Assert.Contains("Barcelona", text);
        Assert.Contains("Valencia", text);
    }
}
