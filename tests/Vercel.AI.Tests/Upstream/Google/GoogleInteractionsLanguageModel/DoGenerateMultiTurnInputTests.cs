// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>multi-turn input</c>.
/// </summary>
public sealed class DoGenerateMultiTurnInputTests
{
    private const string Prefix = DoGenerateSupport.Id + "multi-turn input::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits Array<Turn> when there is more than one turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_Array_Turn_when_there_is_more_than_one_turn()
    {
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("basic")
        );

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(
                GoogleUpstream.Prompt(
                    new SystemModelMessage("You are a helpful assistant."),
                    new UserModelMessage("Hi"),
                    new AssistantModelMessage("Hello!", null, null),
                    new UserModelMessage("How are you?")
                ),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler),
            "{\"input\":[{\"content\":[{\"text\":\"Hi\",\"type\":\"text\"}],\"type\":\"user_input\"},{\"content\":[{\"text\":\"Hello!\",\"type\":\"text\"}],\"type\":\"model_output\"},{\"content\":[{\"text\":\"How are you?\",\"type\":\"text\"}],\"type\":\"user_input\"}],\"model\":\"gemini-2.5-flash\",\"system_instruction\":\"You are a helpful assistant.\"}"
        );
    }
}
