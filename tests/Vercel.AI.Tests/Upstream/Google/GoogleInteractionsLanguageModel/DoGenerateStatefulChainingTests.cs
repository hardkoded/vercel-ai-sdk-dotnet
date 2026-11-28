// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>stateful chaining (previousInteractionId, store, thinking)</c>.
/// </summary>
public sealed class DoGenerateStatefulChainingTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > stateful chaining (previousInteractionId, store, thinking)::";

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
    [UpstreamTest(
        Prefix
            + "emits previous_interaction_id and omits store when previousInteractionId is set + default store",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_previous_interaction_id_and_omits_store_when_previousInteractionId_is_set_plus_default_store()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(
                Options("{\"google\":{\"previousInteractionId\":\"v1_prev-abc\"}}"),
                CancellationToken.None
            );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("v1_prev-abc", (string?)body["previous_interaction_id"]);
        Assert.False(body.ContainsKey("store"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits store: false (and no previous_interaction_id) for stateless mode",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_store_false_and_no_previous_interaction_id_for_stateless_mode()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options("{\"google\":{\"store\":false}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.False(body.ContainsKey("previous_interaction_id"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits both previous_interaction_id and store: false when both are set, and surfaces a warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_both_previous_interaction_id_and_store_false_when_both_are_set_and_surfaces_a_warning()
    {
        var handler = Basic();

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(
                Options("{\"google\":{\"previousInteractionId\":\"v1_prev\",\"store\":false}}"),
                CancellationToken.None
            );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("v1_prev", (string?)body["previous_interaction_id"]);
        Assert.False((bool?)body["store"]);
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "other" && warning.Message.Contains("store: false")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "compacts assistant turns matching previousInteractionId out of the wire body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Compacts_assistant_turns_matching_previousInteractionId_out_of_the_wire_body()
    {
        const string PrevId = "v1_prev-compaction";
        var handler = Basic();
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What is the largest city in Spain?"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart(
                        "Madrid is the largest.",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"interactionId\":\"" + PrevId + "\"}}"
                        )
                    ),
                }
            ),
            new UserModelMessage("And the second largest?")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"previousInteractionId\":\"" + PrevId + "\"}}"
        );

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal(PrevId, (string?)body["previous_interaction_id"]);
        GoogleUpstream.JsonEqual(
            body["input"],
            "[{\"content\":[{\"text\":\"What is the largest city in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"},{\"content\":[{\"text\":\"And the second largest?\",\"type\":\"text\"}],\"type\":\"user_input\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits thinking_level and thinking_summaries inside generation_config",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_thinking_level_and_thinking_summaries_inside_generation_config()
    {
        var handler = Basic();

        var options = Options(
            "{\"google\":{\"thinkingLevel\":\"high\",\"thinkingSummaries\":\"auto\"}}"
        );
        options.Reasoning = "xhigh";
        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        var generationConfig = JsonNode.Parse(handler.Bodies[0])!["generation_config"]!;
        Assert.Equal("high", (string?)generationConfig["thinking_level"]);
        Assert.Equal("auto", (string?)generationConfig["thinking_summaries"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps top-level reasoning to the Hydrogen thinking level",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8e60cb4ba35426e7f4a21e341a5e16f37a478a5b"
    )]
    public async Task Maps_top_level_reasoning_to_the_Hydrogen_thinking_level()
    {
        var handler = Basic();
        var options = Options();
        options.Reasoning = "xhigh";

        var result = await GoogleInteractionsStreamSupport
            .Model(handler, "gemini-4-hydrogen")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "xhigh",
            (string?)JsonNode.Parse(handler.Bodies[0])!["generation_config"]!["thinking_level"]
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns and falls back for unsupported reasoning levels",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8e60cb4ba35426e7f4a21e341a5e16f37a478a5b"
    )]
    public async Task Warns_and_falls_back_for_unsupported_reasoning_levels()
    {
        var handler = Basic();
        var options = Options();
        options.Reasoning = "xhigh";

        var result = await GoogleInteractionsStreamSupport
            .Model(handler, "gemini-4-argon")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "high",
            (string?)JsonNode.Parse(handler.Bodies[0])!["generation_config"]!["thinking_level"]
        );
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"high\".",
            warning.Message
        );
    }

    [Theory]
    [InlineData("gemini-4-hydrogen")]
    [InlineData("gemini-4-hydrogen-eap")]
    [InlineData("gemini-4-neon")]
    [UpstreamTest(
        Prefix + "passes the xhigh thinking level for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4e43721f73006cda79d95cb3e8733360493ed388"
    )]
    public async Task Passes_the_xhigh_thinking_level_for_model_id(string modelId)
    {
        var handler = Basic();

        var result = await GoogleInteractionsStreamSupport
            .Model(handler, modelId)
            .DoGenerateAsync(
                Options("{\"google\":{\"thinkingLevel\":\"xhigh\"}}"),
                CancellationToken.None
            );

        var body = JsonNode.Parse(handler.Bodies[0])!;
        Assert.Equal(modelId, (string?)body["model"]);
        Assert.Equal("xhigh", (string?)body["generation_config"]!["thinking_level"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards topK and warns for unsupported penalties",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_topK_and_warns_for_unsupported_penalties()
    {
        var handler = Basic();
        var options = Options();
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Bodies[0])!["generation_config"],
            "{\"top_k\":10}"
        );
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
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns interactionId for turn 1 from a captured fixture",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_interactionId_for_turn_1_from_a_captured_fixture()
    {
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("multi-turn-stateful-turn1")
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(
                GoogleUpstream.Prompt(
                    new UserModelMessage("What are the three largest cities in Spain?")
                ),
                CancellationToken.None
            );

        Assert.Equal(
            "v1_ChdWV3NIYXNYZEc5S19xdHNQcmVYeG1BRRIXVldzSGFzWGRHOUtfcXRzUHJlWHhtQUU",
            result
                .ProviderMetadata!.Value.GetProperty("google")
                .GetProperty("interactionId")
                .GetString()
        );
        var text = string.Concat(result.Content.OfType<GeneratedText>().Select(part => part.Text));
        Assert.Contains("Madrid", text);
        Assert.Contains("Barcelona", text);
        Assert.Contains("Valencia", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "compacts the turn-2 wire body and references prior interaction (fixture-driven)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Compacts_the_turn_2_wire_body_and_references_prior_interaction_fixture_driven()
    {
        const string Turn1Id =
            "v1_ChdWV3NIYXNYZEc5S19xdHNQcmVYeG1BRRIXVldzSGFzWGRHOUtfcXRzUHJlWHhtQUU";
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("multi-turn-stateful-turn2")
        );
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What are the three largest cities in Spain?"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart(
                        "",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"interactionId\":\""
                                + Turn1Id
                                + "\",\"signature\":\"thought-sig-from-turn-1\"}}"
                        )
                    ),
                    new AssistantTextPart(
                        "The three largest cities in Spain are Madrid, Barcelona, and Valencia.",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"interactionId\":\"" + Turn1Id + "\"}}"
                        )
                    ),
                }
            ),
            new UserModelMessage("What is the most famous landmark in the second one?")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"previousInteractionId\":\"" + Turn1Id + "\"}}"
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal(Turn1Id, (string?)body["previous_interaction_id"]);
        // Compaction: only the two surviving user messages remain on the wire.
        GoogleUpstream.JsonEqual(
            body["input"],
            "[{\"content\":[{\"text\":\"What are the three largest cities in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"},{\"content\":[{\"text\":\"What is the most famous landmark in the second one?\",\"type\":\"text\"}],\"type\":\"user_input\"}]"
        );

        // Turn 2 references "Barcelona", the second city from the prior turn.
        var text = string.Concat(result.Content.OfType<GeneratedText>().Select(part => part.Text));
        Assert.Contains("Barcelona", text);
        Assert.Matches("Sagrada Fam.lia", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "round-trips a thought signature on an input reasoning part to the wire thought block",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Round_trips_a_thought_signature_on_an_input_reasoning_part_to_the_wire_thought_block()
    {
        var handler = Basic();
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("q"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart(
                        "",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"signature\":\"sig-roundtrip\"}}"
                        )
                    ),
                    new AssistantTextPart("old answer"),
                }
            ),
            new UserModelMessage("q2")
        );

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        // `thought` is a top-level step type, not a content block under a turn.
        var thought = JsonNode.Parse(handler.Bodies[0])!["input"]!
            .AsArray()
            .First(step => (string?)step!["type"] == "thought");
        Assert.Equal("sig-roundtrip", (string?)thought!["signature"]);
    }
}
