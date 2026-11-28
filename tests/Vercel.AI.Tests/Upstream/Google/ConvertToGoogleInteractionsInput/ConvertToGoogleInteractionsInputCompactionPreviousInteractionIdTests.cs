// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>compaction (previousInteractionId)</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputCompactionPreviousInteractionIdTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "compaction (previousInteractionId)::";
    private const string PrevId = "v1_prev-interaction-abc";
    private const string PrevOptions = "{\"google\":{\"interactionId\":\"" + PrevId + "\"}}";

    [Fact]
    [UpstreamTest(
        Prefix + "drops an assistant turn whose parts carry the matching interactionId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Drops_an_assistant_turn_whose_parts_carry_the_matching_interactionId()
    {
        var result = S.Convert(
            PrevId,
            null,
            new UserModelMessage("first user input"),
            S.Assistant(S.Text("old answer", PrevOptions)),
            new UserModelMessage("follow-up user input")
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"first user input\"}]},{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"follow-up user input\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps an assistant turn from a different interaction",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_an_assistant_turn_from_a_different_interaction()
    {
        var result = S.Convert(
            PrevId,
            null,
            new UserModelMessage("first user input"),
            S.Assistant(
                S.Text(
                    "unrelated old answer",
                    "{\"google\":{\"interactionId\":\"some-other-interaction\"}}"
                )
            ),
            new UserModelMessage("follow-up user input")
        );

        Assert.Equal(3, result.Input.Count);

        // After compaction, the surviving 3 steps are: user_input (q1),
        // model_output (the assistant turn from a different interaction -
        // kept), and user_input (follow-up).
        Assert.Equal("model_output", (string?)result.Input[1]!["type"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "drops the matching assistant tool-call turn AND its paired tool-result message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Drops_the_matching_assistant_tool_call_turn_AND_its_paired_tool_result_message()
    {
        var result = S.Convert(
            PrevId,
            null,
            new UserModelMessage("q1"),
            S.Assistant(
                S.ToolCall("call_old", "getWeather", "{\"location\":\"Boston\"}", PrevOptions)
            ),
            new ToolModelMessage("call_old", "getWeather", "sunny", false, "text"),
            new UserModelMessage("q2")
        );

        Assert.Equal(new List<string?> { "user_input", "user_input" }, S.StepTypes(result));
        GoogleUpstream.JsonEqual(
            result.Input[0]!["content"]![0],
            "{\"type\":\"text\",\"text\":\"q1\"}"
        );
        GoogleUpstream.JsonEqual(
            result.Input[1]!["content"]![0],
            "{\"type\":\"text\",\"text\":\"q2\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not compact when store=false (incoherent combo) and emits a warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_compact_when_store_false_incoherent_combo_and_emits_a_warning()
    {
        var result = S.Convert(
            PrevId,
            false,
            new UserModelMessage("q1"),
            S.Assistant(S.Text("old answer", PrevOptions)),
            new UserModelMessage("q2")
        );

        Assert.Equal(3, result.Input.Count);
        GoogleUpstream.JsonEqual(
            S.Warnings(result),
            "[{\"type\":\"other\",\"message\":\"google.interactions: providerOptions.google.previousInteractionId was set together with store: false. These are incoherent (the prior interaction cannot be referenced when nothing was stored on the server); the full history will be sent and previous_interaction_id will still be emitted.\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reduces a stateful turn-2 wire body to only the new user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reduces_a_stateful_turn_2_wire_body_to_only_the_new_user_message()
    {
        var result = S.Convert(
            PrevId,
            null,
            new UserModelMessage("first user input"),
            S.Assistant(
                S.Reasoning(
                    "",
                    "{\"google\":{\"interactionId\":\"" + PrevId + "\",\"signature\":\"sig-1\"}}"
                ),
                S.Text("first model answer", PrevOptions)
            ),
            new UserModelMessage("follow-up")
        );

        // After compaction, only "first user input" + "follow-up" remain. The
        // single-turn fast-path doesn't kick in because two user turns survive,
        // so we still get the Array<Turn> shape - but no model turn.
        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"first user input\"}]},{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"follow-up\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "round-trips a thought signature on a turn-1 reasoning output back onto the turn-2 wire block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Round_trips_a_thought_signature_on_a_turn_1_reasoning_output_back_onto_the_turn_2_wire_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("q"),
            S.Assistant(
                S.Reasoning("", "{\"google\":{\"signature\":\"thought-sig-XYZ\"}}"),
                S.Text("a")
            ),
            new UserModelMessage("q2")
        );

        // Assistant content fans out into discrete steps: a `thought` step
        // (carrying the signature) followed by a `model_output` step.
        var thoughtStep = S.FindStep(result, "thought");
        Assert.Equal("thought", (string?)thoughtStep?["type"]);
        Assert.Equal("thought-sig-XYZ", (string?)thoughtStep?["signature"]);
    }
}
