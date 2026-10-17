// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.ProgrammaticToolCalling;

public sealed class StreamDiceGameFixtureStepInputsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct prompt in step 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_prompt_in_step_1()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(DiceGameFixture.View(fixture.DoStreamCalls[0].Prompt), "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Play a dice game between two players.\"}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct tools in step 1", Coverage = UpstreamCoverage.Partial, Note = "Compares the id, name, args, description and input schema, not the exact object.")]
    public async Task Should_send_correct_tools_in_step_1()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        var tools = fixture.DoStreamCalls[0].Tools!;
        Assert.Equal(2, tools.Count);
        Assert.True(tools[0].IsProviderDefined);
        Assert.Equal("anthropic.code_execution_20250825", tools[0].ProviderToolId);
        Assert.Equal("code_execution", tools[0].Name);
        JsonAssert.Equal(tools[0].Args, "{}");
        Assert.False(tools[1].IsProviderDefined);
        Assert.Equal("rollDie", tools[1].Name);
        Assert.Equal("Roll a die and return the result.", tools[1].Description);
        JsonAssert.Equal(
            tools[1].InputSchema,
            "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"player\":{\"enum\":[\"player1\",\"player2\"],\"type\":\"string\"}},\"required\":[\"player\"],\"type\":\"object\"}");
        JsonAssert.Equal(tools[1].ProviderOptions!.Value, "{\"anthropic\":{\"allowedCallers\":[\"code_execution_20250825\"]}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should include assistant messages and tool results in step 2 prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_assistant_messages_and_tool_results_in_step_2_prompt()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoStreamCalls[1].Prompt),
            "[" +
        "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Play a dice game between two players.\"}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"I'll help you simulate this game between two players where one is using a loaded die.\"},{\"type\":\"tool-call\",\"toolCallId\":\"srvtoolu_01MzSrFWsmzBdcoQkGWLyRjK\",\"toolName\":\"code_execution\",\"input\":{\"code\":\"game_loop()\"},\"providerExecuted\":true},{\"type\":\"tool-call\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}]}" +
        "]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 3 prompt (round 2)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_3_prompt_round_2()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoStreamCalls[2].Prompt),
            "[" +
        "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Play a dice game between two players.\"}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"I'll help you simulate this game between two players where one is using a loaded die.\"},{\"type\":\"tool-call\",\"toolCallId\":\"srvtoolu_01MzSrFWsmzBdcoQkGWLyRjK\",\"toolName\":\"code_execution\",\"input\":{\"code\":\"game_loop()\"},\"providerExecuted\":true},{\"type\":\"tool-call\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_015dGLMbwBKv1ZRQr6KdJzeH\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_015dGLMbwBKv1ZRQr6KdJzeH\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}" +
        "]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward container ID via providerOptions in step 2", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_container_ID_via_providerOptions_in_step_2()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            fixture.DoStreamCalls[1].ProviderOptions!["anthropic"],
            "{\"container\":{\"id\":\"" + StreamDiceGameFixture.ContainerId + "\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should include all previous messages in step 5 prompt (final step)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_5_prompt_final_step()
    {
        var fixture = new StreamDiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoStreamCalls[4].Prompt),
            "[" +
        "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Play a dice game between two players.\"}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"I'll help you simulate this game between two players where one is using a loaded die.\"},{\"type\":\"tool-call\",\"toolCallId\":\"srvtoolu_01MzSrFWsmzBdcoQkGWLyRjK\",\"toolName\":\"code_execution\",\"input\":{\"code\":\"game_loop()\"},\"providerExecuted\":true},{\"type\":\"tool-call\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_019jKkXz4jAdwHweHBw92CVY\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_015dGLMbwBKv1ZRQr6KdJzeH\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_015dGLMbwBKv1ZRQr6KdJzeH\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01YYqBNq5mk1wMtv3PAqY44m\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01YYqBNq5mk1wMtv3PAqY44m\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}]}," +
        "{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_018WxjDkQG8h7i63poySGT2x\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}," +
        "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_018WxjDkQG8h7i63poySGT2x\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}" +
        "]");
    }
}
