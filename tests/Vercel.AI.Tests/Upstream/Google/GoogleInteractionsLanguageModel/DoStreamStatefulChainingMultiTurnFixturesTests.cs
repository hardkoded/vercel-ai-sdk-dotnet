// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>stateful chaining (multi-turn fixtures)</c>.
/// </summary>
public sealed class DoStreamStatefulChainingMultiTurnFixturesTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > stateful chaining (multi-turn fixtures)::";

    private static string Text(IEnumerable<LanguageModelStreamPart> parts)
    {
        return string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams turn-1 reasoning + text and surfaces interactionId on finish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_turn_1_reasoning_plus_text_and_surfaces_interactionId_on_finish()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("multi-turn-stateful-turn1");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What are the three largest cities in Spain?")
        );
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            options
        );

        var finish = Assert.IsType<FinishStreamPart>(parts.First(part => part is FinishStreamPart));
        var interactionId = finish
            .ProviderMetadata!.Value.GetProperty("google")
            .GetProperty("interactionId");
        Assert.Equal(JsonValueKind.String, interactionId.ValueKind);
        var text = Text(parts);
        Assert.Contains("Madrid", text);
        Assert.Contains("Barcelona", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "compacts the turn-2 stream wire body and streams the chained answer",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Compacts_the_turn_2_stream_wire_body_and_streams_the_chained_answer()
    {
        const string turn1Id =
            "v1_ChdWV3NIYXNYZEc5S19xdHNQcmVYeG1BRRIXVldzSGFzWGRHOUtfcXRzUHJlWHhtQUU";
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("multi-turn-stateful-turn2");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What are the three largest cities in Spain?"),
            new AssistantModelMessage(
                "Madrid, Barcelona, Valencia",
                null,
                "",
                null,
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart(
                        "",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"interactionId\":\""
                                + turn1Id
                                + "\",\"signature\":\"sig-1\"}}"
                        )
                    ),
                    new AssistantTextPart(
                        "Madrid, Barcelona, Valencia",
                        GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"interactionId\":\"" + turn1Id + "\"}}"
                        )
                    ),
                }
            ),
            new UserModelMessage("What is the most famous landmark in the second one?")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"previousInteractionId\":\"" + turn1Id + "\"}}"
        );
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler),
            options
        );

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal(turn1Id, (string?)body["previous_interaction_id"]);
        Assert.True((bool?)body["stream"]);
        // Compaction: only the two surviving user messages remain on the wire.
        GoogleUpstream.JsonEqual(
            body["input"],
            "[{\"content\":[{\"text\":\"What are the three largest cities in Spain?\",\"type\":\"text\"}],\"type\":\"user_input\"},"
                + "{\"content\":[{\"text\":\"What is the most famous landmark in the second one?\",\"type\":\"text\"}],\"type\":\"user_input\"}]"
        );
        Assert.Matches("Sagrada Fam.lia", Text(parts));
    }
}
