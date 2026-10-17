// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream.GenerateText.ProgrammaticToolCalling;

public sealed class DiceGameFixtureStepInputsTests
{
    private const string UserMessage = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Play a dice game between two players.\"}]}";

    private const string Step1Assistant = "{\"role\":\"assistant\",\"content\":[" +
        "{\"type\":\"text\",\"text\":\"" + DiceGameFixture.Step1Text + "\"}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"" + DiceGameFixture.CodeExecId + "\",\"toolName\":\"code_execution\",\"input\":{\"code\":\"game_loop()\"},\"providerExecuted\":true}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01PMcE1JBKCeLjn83cgUCvR5\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01MZf5QJ1EQyd2yGyeLzBxAS\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}]}";

    private const string Step1Tool = "{\"role\":\"tool\",\"content\":[" +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01PMcE1JBKCeLjn83cgUCvR5\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}," +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01MZf5QJ1EQyd2yGyeLzBxAS\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}]}";

    private const string Step2Assistant = "{\"role\":\"assistant\",\"content\":[" +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01UvVQ2xwA6preZppeajCkYK\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01BghspNownQFtRgv8jVicr3\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}";

    private const string Step2Tool = "{\"role\":\"tool\",\"content\":[" +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01UvVQ2xwA6preZppeajCkYK\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}," +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01BghspNownQFtRgv8jVicr3\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}";

    private const string Step3Assistant = "{\"role\":\"assistant\",\"content\":[" +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01T7Upuuv8C71nq7DZ9ZPNQW\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_016Da1tDet9Bf7dAdYTkF5Ar\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}";

    private const string Step3Tool = "{\"role\":\"tool\",\"content\":[" +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01T7Upuuv8C71nq7DZ9ZPNQW\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}," +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_016Da1tDet9Bf7dAdYTkF5Ar\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}";

    private const string Step4Assistant = "{\"role\":\"assistant\",\"content\":[" +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01DiUBRds64sNajVPTZRrDSM\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player1\"}}," +
        "{\"type\":\"tool-call\",\"toolCallId\":\"toolu_01XQa3r3y1Fe8rnkGSncq626\",\"toolName\":\"rollDie\",\"input\":{\"player\":\"player2\"}}]}";

    private const string Step4Tool = "{\"role\":\"tool\",\"content\":[" +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01DiUBRds64sNajVPTZRrDSM\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":6}}," +
        "{\"type\":\"tool-result\",\"toolCallId\":\"toolu_01XQa3r3y1Fe8rnkGSncq626\",\"toolName\":\"rollDie\",\"output\":{\"type\":\"json\",\"value\":3}}]}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should send correct prompt in step 1",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_prompt_in_step_1()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(DiceGameFixture.View(fixture.DoGenerateCalls[0].Prompt), "[" + UserMessage + "]");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should send correct tools in step 1",
        Coverage = UpstreamCoverage.Partial,
        Note = "Compares the id, name, args, description and input schema, not the exact object.")]
    public async Task Should_send_correct_tools_in_step_1()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        var tools = fixture.DoGenerateCalls[0].Tools!;
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
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should include assistant messages and tool results in step 2 prompt",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_assistant_messages_and_tool_results_in_step_2_prompt()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoGenerateCalls[1].Prompt),
            "[" + UserMessage + "," + Step1Assistant + "," + Step1Tool + "]");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should include all previous messages in step 3 prompt (round 3)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_3_prompt_round_3()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoGenerateCalls[2].Prompt),
            "[" + UserMessage + "," + Step1Assistant + "," + Step1Tool + "," + Step2Assistant + "," + Step2Tool + "]");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should forward container ID via providerOptions in step 2",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_container_ID_via_providerOptions_in_step_2()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            fixture.DoGenerateCalls[1].ProviderOptions!["anthropic"],
            "{\"container\":{\"id\":\"" + DiceGameFixture.ContainerId + "\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > programmatic tool calling > 5 steps: code_execution triggers client tool across multiple turns (dice game fixture) > step inputs::should include all previous messages in step 5 prompt (final step)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_all_previous_messages_in_step_5_prompt_final_step()
    {
        var fixture = new DiceGameFixture();
        await fixture.RunAsync();
        JsonAssert.Equal(
            DiceGameFixture.View(fixture.DoGenerateCalls[4].Prompt),
            "[" + UserMessage + "," + Step1Assistant + "," + Step1Tool + "," + Step2Assistant + "," + Step2Tool + "," + Step3Assistant + "," + Step3Tool + "," + Step4Assistant + "," + Step4Tool + "]");
    }
}
