// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>stateless multi-turn (store: false, no previousInteractionId)</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputStatelessMultiTurnTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id
        + "stateless multi-turn (store: false, no previousInteractionId)::";

    [Fact]
    [UpstreamTest(
        Prefix + "forwards full history verbatim and emits no warnings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Forwards_full_history_verbatim_and_emits_no_warnings()
    {
        var result = S.Convert(
            null,
            false,
            new UserModelMessage("What are the three largest cities in Spain?"),
            S.Assistant(S.Text("Madrid, Barcelona, Valencia.")),
            new UserModelMessage("What is the most famous landmark in the second one?")
        );

        Assert.Empty(result.Warnings);
        Assert.Equal(3, result.Input.Count);
        Assert.Equal(
            new List<string?> { "user_input", "model_output", "user_input" },
            S.StepTypes(result)
        );
        GoogleUpstream.JsonEqual(
            result.Input[0]!["content"]![0],
            "{\"type\":\"text\",\"text\":\"What are the three largest cities in Spain?\"}"
        );
        GoogleUpstream.JsonEqual(
            result.Input[1]!["content"]![0],
            "{\"type\":\"text\",\"text\":\"Madrid, Barcelona, Valencia.\"}"
        );
        GoogleUpstream.JsonEqual(
            result.Input[2]!["content"]![0],
            "{\"type\":\"text\",\"text\":\"What is the most famous landmark in the second one?\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not compact assistant turns even when they carry a stale interactionId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_compact_assistant_turns_even_when_they_carry_a_stale_interactionId()
    {
        var result = S.Convert(
            null,
            false,
            new UserModelMessage("q1"),
            S.Assistant(S.Text("a1", "{\"google\":{\"interactionId\":\"v1_stale-interaction\"}}")),
            new UserModelMessage("q2")
        );

        Assert.Empty(result.Warnings);
        Assert.Equal(3, result.Input.Count);
        Assert.Equal(
            new List<string?> { "user_input", "model_output", "user_input" },
            S.StepTypes(result)
        );
    }
}
