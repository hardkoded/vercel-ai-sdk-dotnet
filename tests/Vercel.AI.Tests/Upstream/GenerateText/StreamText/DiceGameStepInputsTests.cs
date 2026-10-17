// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateText.DiceGameFixture;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; programmatic tool calling &gt; 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) &gt; step inputs</c>.</summary>
public sealed class DiceGameStepInputsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::";
    private const string PartialNote = "The port keeps the `type` key in the provider-executed code_execution input, which upstream strips by parsing it with the tool's input schema. See #521.";

    private const string Intro = "I'll help you simulate this game between two players where one is using a loaded die.";
    private const string CodeInput = "{\"type\":\"programmatic-tool-call\",\"code\":\"game_loop()\"}";

    [Fact]
    [UpstreamTest(Prefix + "should send correct prompt in step 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_prompt_in_step_1()
    {
        var run = await RunStreamAsync();
        Assert.Equal(new[] { User("Play a dice game between two players.") }, Render(run.ModelCalls[0].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct tools in step 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_tools_in_step_1()
    {
        var run = await RunStreamAsync();
        AssertTools(run.ModelCalls[0].Tools);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include assistant messages and tool results in step 2 prompt", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Should_include_assistant_messages_and_tool_results_in_step_2_prompt()
    {
        var run = await RunStreamAsync();
        Assert.Equal(Round1(), Render(run.ModelCalls[1].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 3 prompt (round 2)", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Should_include_all_previous_messages_in_step_3_prompt_round_2()
    {
        var run = await RunStreamAsync();
        Assert.Equal(Round1().Concat(Round2()).ToArray(), Render(run.ModelCalls[2].Prompt));
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward container ID via providerOptions in step 2", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_container_ID_via_providerOptions_in_step_2()
    {
        var run = await RunStreamAsync();
        AssertContainerForwarded(run.ModelCalls[1], StreamContainerId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 5 prompt (final step)", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Should_include_all_previous_messages_in_step_5_prompt_final_step()
    {
        var run = await RunStreamAsync();
        Assert.Equal(Round1().Concat(Round2()).Concat(Round3()).Concat(Round4()).ToArray(), Render(run.ModelCalls[4].Prompt));
    }

    private static string[] Round1() => new[]
    {
        User("Play a dice game between two players."),
        Assistant(
            Text(Intro),
            Call(StreamCodeExecId, "code_execution", CodeInput, true),
            Call("toolu_019jKkXz4jAdwHweHBw92CVY", "rollDie", "{\"player\":\"player1\"}")),
        ToolResults(Result("toolu_019jKkXz4jAdwHweHBw92CVY", "rollDie", "json", "6")),
    };

    private static string[] Round2() => Round("toolu_015dGLMbwBKv1ZRQr6KdJzeH", "player2", "3");

    private static string[] Round3() => Round("toolu_01YYqBNq5mk1wMtv3PAqY44m", "player1", "6");

    private static string[] Round4() => Round("toolu_018WxjDkQG8h7i63poySGT2x", "player2", "3");

    private static string[] Round(string id, string player, string value) => new[]
    {
        Assistant(Call(id, "rollDie", "{\"player\":\"" + player + "\"}")),
        ToolResults(Result(id, "rollDie", "json", value)),
    };
}
