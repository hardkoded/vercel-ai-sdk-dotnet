// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateText.DiceGameFixture;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; programmatic tool calling &gt; 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) &gt; step inputs</c>.</summary>
public sealed class DiceGameStepInputsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::";

    private const string Intro = "I'll help you simulate this dice game between two players! Let me run the game where both players roll dice each round until one player wins 3 rounds.";
    private const string CodeInput = "{\"code\":\"game_loop()\"}";

    [Fact]
    [UpstreamTest(Prefix + "should send correct prompt in step 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_prompt_in_step_1()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(new[] { User("Play a dice game between two players.") }, Render(run.ModelCalls[0].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct tools in step 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_tools_in_step_1()
    {
        var run = await RunGenerateAsync();
        AssertTools(run.ModelCalls[0].Tools);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include assistant messages and tool results in step 2 prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_assistant_messages_and_tool_results_in_step_2_prompt()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(Round1(), Render(run.ModelCalls[1].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 3 prompt (round 3)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_3_prompt_round_3()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(Round1().Concat(Round2()).ToArray(), Render(run.ModelCalls[2].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward container ID via providerOptions in step 2", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_container_ID_via_providerOptions_in_step_2()
    {
        var run = await RunGenerateAsync();
        AssertContainerForwarded(run.ModelCalls[1], GenerateContainerId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 5 prompt (final step)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_5_prompt_final_step()
    {
        var run = await RunGenerateAsync();
        Assert.Equal(Round1().Concat(Round2()).Concat(Round3()).Concat(Round4()).ToArray(), Render(run.ModelCalls[4].Prompt));
    }

    private static string[] Round1() => new[]
    {
        User("Play a dice game between two players."),
        Assistant(
            Text(Intro),
            Call(GenerateCodeExecId, "code_execution", CodeInput, true),
            Call("toolu_01PMcE1JBKCeLjn83cgUCvR5", "rollDie", "{\"player\":\"player2\"}"),
            Call("toolu_01MZf5QJ1EQyd2yGyeLzBxAS", "rollDie", "{\"player\":\"player1\"}")),
        ToolResults(
            Result("toolu_01PMcE1JBKCeLjn83cgUCvR5", "rollDie", "json", "3"),
            Result("toolu_01MZf5QJ1EQyd2yGyeLzBxAS", "rollDie", "json", "6")),
    };

    private static string[] Round2() => Round("toolu_01UvVQ2xwA6preZppeajCkYK", "toolu_01BghspNownQFtRgv8jVicr3");

    private static string[] Round3() => Round("toolu_01T7Upuuv8C71nq7DZ9ZPNQW", "toolu_016Da1tDet9Bf7dAdYTkF5Ar");

    private static string[] Round4() => Round("toolu_01DiUBRds64sNajVPTZRrDSM", "toolu_01XQa3r3y1Fe8rnkGSncq626");

    private static string[] Round(string player1Id, string player2Id) => new[]
    {
        Assistant(
            Call(player1Id, "rollDie", "{\"player\":\"player1\"}"),
            Call(player2Id, "rollDie", "{\"player\":\"player2\"}")),
        ToolResults(
            Result(player1Id, "rollDie", "json", "6"),
            Result(player2Id, "rollDie", "json", "3")),
    };
}
