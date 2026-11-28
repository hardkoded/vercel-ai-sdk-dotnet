// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt;
/// <c>stateless multi-turn (store: false, no previousInteractionId)</c>.
/// </summary>
public sealed class DoStreamStatelessMultiTurnTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > stateless multi-turn (store: false, no previousInteractionId)::";

    private static string Text(IEnumerable<LanguageModelStreamPart> parts)
    {
        return string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams turn 1 with store: false in the body and no interactionId on finish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_turn_1_with_store_false_in_the_body_and_no_interactionId_on_finish()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("multi-turn-stateless-turn1");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What are the three largest cities in Spain?")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"store\":false}}");
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            options
        );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.Null(body["previous_interaction_id"]);
        Assert.True((bool?)body["stream"]);

        var finish = Assert.IsType<FinishStreamPart>(parts.First(part => part is FinishStreamPart));
        // The streaming API returns id: "" for store: false; the transformer normalizes the empty string to undefined
        // so providerMetadata stays clean.
        Assert.False(
            finish
                .ProviderMetadata!.Value.GetProperty("google")
                .TryGetProperty("interactionId", out _)
        );

        var text = Text(parts);
        Assert.Contains("Madrid", text);
        Assert.Contains("Barcelona", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams turn 2 forwarding full history verbatim with store: false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_turn_2_forwarding_full_history_verbatim_with_store_false()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("multi-turn-stateless-turn2");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What are the three largest cities in Spain?"),
            new AssistantModelMessage(
                "The three largest cities in Spain are Madrid, Barcelona, and Valencia.",
                null,
                null
            ),
            new UserModelMessage("What is the most famous landmark in the second one?")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"store\":false}}");
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            options
        );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.Null(body["previous_interaction_id"]);
        // Full history forwarded verbatim, with no compaction.
        GoogleUpstream.JsonEqual(
            body["input"],
            "[{\"content\":[{\"text\":\"What are the three largest cities in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"},"
                + "{\"content\":[{\"text\":\"The three largest cities in Spain are Madrid, Barcelona, and Valencia.\",\"type\":\"text\"}],\"type\":\"model_output\"},"
                + "{\"content\":[{\"text\":\"What is the most famous landmark in the second one?\",\"type\":\"text\"}],\"type\":\"user_input\"}]"
        );
        Assert.Contains("Sagrada", Text(parts));
    }
}
