// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt;
/// <c>stateless multi-turn (store: false, no previousInteractionId)</c>.
/// </summary>
public sealed class DoGenerateStatelessMultiTurnTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > stateless multi-turn (store: false, no previousInteractionId)::";

    private static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-interactions-" + name + ".json"));
    }

    private static string Text(LanguageModelGenerateResult result)
    {
        return string.Concat(result.Content.OfType<GeneratedText>().Select(text => text.Text));
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards a single-turn user input verbatim and sets store: false (turn 1 fixture)", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_a_single_turn_user_input_verbatim_and_sets_store_false_turn_1_fixture()
    {
        var handler = new SequencedHandler().Json(Fixture("multi-turn-stateless-turn1"));
        var model = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler).Interactions("gemini-2.5-flash");

        var result = await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("What are the three largest cities in Spain?") },
                ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"store\":false}}"),
            },
            CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.Null(body["previous_interaction_id"]);
        GoogleUpstream.JsonEqual(body["input"], "[{\"content\":[{\"text\":\"What are the three largest cities in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"}]");

        Assert.Empty(result.Warnings);
        // The API omits `id` when `store: false`, so no interactionId is surfaced.
        Assert.Null(result.ResponseId);
        Assert.Null(result.ProviderMetadata);

        var text = Text(result);
        Assert.Contains("Madrid", text);
        Assert.Contains("Barcelona", text);
        Assert.Contains("Valencia", text);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards full message history verbatim on turn 2 with store: false (turn 2 fixture)", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_full_message_history_verbatim_on_turn_2_with_store_false_turn_2_fixture()
    {
        var handler = new SequencedHandler().Json(Fixture("multi-turn-stateless-turn2"));
        var model = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler).Interactions("gemini-2.5-flash");

        var result = await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage("What are the three largest cities in Spain?"),
                    new AssistantModelMessage("The three largest cities in Spain are Madrid, Barcelona, and Valencia.", null, null),
                    new UserModelMessage("What is the most famous landmark in the second one?"),
                },
                ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"store\":false}}"),
            },
            CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False((bool?)body["store"]);
        Assert.Null(body["previous_interaction_id"]);
        // Full history forwarded verbatim, with no compaction.
        GoogleUpstream.JsonEqual(
            body["input"],
            "[{\"content\":[{\"text\":\"What are the three largest cities in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"},"
            + "{\"content\":[{\"text\":\"The three largest cities in Spain are Madrid, Barcelona, and Valencia.\",\"type\":\"text\"}],\"type\":\"model_output\"},"
            + "{\"content\":[{\"text\":\"What is the most famous landmark in the second one?\",\"type\":\"text\"}],\"type\":\"user_input\"}]");

        Assert.Empty(result.Warnings);
        Assert.Matches("Sagrada Fam.lia", Text(result));
    }
}
