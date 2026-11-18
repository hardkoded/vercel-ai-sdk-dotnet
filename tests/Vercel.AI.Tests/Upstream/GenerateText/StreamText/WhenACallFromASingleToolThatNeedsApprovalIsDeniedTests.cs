// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a call from a single tool that needs approval is denied</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsDeniedTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a call from a single tool that needs approval is denied::";

    [Fact]
    [UpstreamTest(Id + "should not execute the tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_execute_the_tool()
    {
        var run = await RunAsync();

        Assert.Equal(0, run.ExecuteCalls);
    }

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
        var denied = Assert.IsType<ToolModelMessage>(prompt[2]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("tool1", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal(string.Empty, denied.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool error in the response messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_error_in_the_response_messages()
    {
        var run = await RunAsync();

        var messages = await run.Result.ResponseMessages;
        Assert.Equal(2, messages.Count);
        var denied = Assert.IsType<ToolModelMessage>(messages[0]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("tool1", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal(string.Empty, denied.OutputJson);
        AssertText(messages[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool denied in the full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_denied_in_the_full_stream()
    {
        var run = await RunAsync();

        var parts = await ReadAsync(run.Result);

        Assert.Equal(
            new[]
            {
                "start",
                "tool-output-denied",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            },
            Types(parts)
        );
        var denied = Assert.IsType<ToolOutputDeniedPart>(parts[1]);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("tool1", denied.ToolName);
        AssertStartStep(parts[2]);
        AssertStepAndFinish(parts, 6, FinishReason.Stop, "stop");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include the tool denied in the ui message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_tool_denied_in_the_ui_message_stream()
    {
        var run = await RunAsync();

        AssertJson(
            """
            [
              { "type": "start" },
              { "toolCallId": "call-1", "type": "tool-output-denied" },
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

    private static async Task<(
        StreamTextResult Result,
        ScriptedModel Model,
        int ExecuteCalls
    )> RunAsync()
    {
        var executeCalls = 0;
        var model = new ScriptedModel(_ => TextStep());
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[]
                    {
                        ValueTool(
                            (_, _) =>
                            {
                                executeCalls++;
                                return Task.FromResult("result1");
                            }
                        ),
                    },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Messages = History(
                        "tool1",
                        new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
                        new ToolApprovalResponseModelMessage("id-1", false)
                    ),
                }
            );
        await result.ConsumeStreamAsync();
        return (result, model, executeCalls);
    }
}
