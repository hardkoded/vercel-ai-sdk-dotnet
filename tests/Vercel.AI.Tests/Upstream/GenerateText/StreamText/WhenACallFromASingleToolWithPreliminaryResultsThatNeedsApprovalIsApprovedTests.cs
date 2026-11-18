// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a call from a single tool with preliminary results that needs approval is approved</c>.</summary>
public sealed class WhenACallFromASingleToolWithPreliminaryResultsThatNeedsApprovalIsApprovedTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a call from a single tool with preliminary results that needs approval is approved::";

    [Fact]
    [UpstreamTest(
        Id + "should call the model with a prompt that includes the tool result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_model_with_a_prompt_that_includes_the_tool_result()
    {
        var run = await RunAsync();

        var prompt = Assert.Single(run.Model.Prompts);
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        AssertToolCallPart(
            Assert.Single(Assert.IsType<AssistantModelMessage>(prompt[1]).Parts),
            "call-1",
            "tool1",
            "{\"value\":\"value\"}"
        );
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(prompt[2]),
            "call-1",
            "tool1",
            "text",
            "final-result"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the response messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_response_messages()
    {
        var run = await RunAsync();

        var messages = await run.Result.ResponseMessages;
        Assert.Equal(2, messages.Count);
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(messages[0]),
            "call-1",
            "tool1",
            "text",
            "final-result"
        );
        AssertText(messages[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_full_stream()
    {
        var run = await RunAsync();

        var parts = await ReadAsync(run.Result);

        Assert.Equal(
            new[]
            {
                "start",
                "tool-result",
                "tool-result",
                "tool-result",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            },
            Types(parts)
        );
        foreach (
            var (index, output, preliminary) in new[]
            {
                (1, "preliminary-result", true),
                (2, "final-result", true),
                (3, "final-result", false),
            }
        )
        {
            var result = Assert.IsType<ToolResultPart>(parts[index]).Result;
            Assert.Equal(output, result.OutputJson);
            Assert.Equal(preliminary ? true : null, result.Preliminary);
            Assert.Equal("call-1", result.ToolCallId);
            Assert.Equal("tool1", result.ToolName);
            AssertInput(
                "{\"value\":\"value\"}",
                System.Text.Json.JsonDocument.Parse(result.InputJson!).RootElement
            );
            Assert.False(result.Dynamic);
        }

        AssertStartStep(parts[4]);
        AssertStepAndFinish(parts, 8, FinishReason.Stop, "stop");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool result in the ui message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_result_in_the_ui_message_stream()
    {
        var run = await RunAsync();

        AssertJson(
            """
            [
              { "type": "start" },
              { "output": "preliminary-result", "preliminary": true, "toolCallId": "call-1", "type": "tool-output-available" },
              { "output": "final-result", "preliminary": true, "toolCallId": "call-1", "type": "tool-output-available" },
              { "output": "final-result", "toolCallId": "call-1", "type": "tool-output-available" },
              { "type": "start-step" },
              { "id": "1", "type": "text-start" },
              { "delta": "Hello, world!", "id": "1", "type": "text-delta" },
              { "id": "1", "type": "text-end" },
              { "type": "finish-step" },
              { "finishReason": "stop", "type": "finish" }
            ]
            """,
            await UiChunksAsync(run.Result)
        );
    }

    private static async Task<(StreamTextResult Result, ScriptedModel Model)> RunAsync()
    {
        var model = new ScriptedModel(_ => TextStep());
        var tool = ValueTool(null);
        tool.ExecuteStream = (_, _) => Chunks();
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Messages =
                        WhenACallFromASingleToolThatNeedsApprovalIsApprovedTests.ApprovedHistory(),
                }
            );
        await result.ConsumeStreamAsync();
        return (result, model);
    }

    private static async IAsyncEnumerable<string> Chunks()
    {
        await Task.Yield();
        yield return "preliminary-result";
        yield return "final-result";
    }
}
