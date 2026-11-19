// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a single tool is automatically approved</c>.</summary>
public sealed class WhenASingleToolIsAutomaticallyApprovedTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a single tool is automatically approved::";

    [Fact]
    [UpstreamTest(
        Id + "should continue to a second step and execute the tool",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream expects abortSignal to be undefined. The port always passes the cancellation token of the stream, which can be canceled, so abortSignal is not asserted."
    )]
    public async Task Should_continue_to_a_second_step_and_execute_the_tool()
    {
        var run = await RunAsync();

        Assert.Equal(1, run.ExecuteCalls);
        Assert.Equal("call-1", run.Options[0].ToolCallId);
        Assert.NotNull(run.Options[0].Messages);
        Assert.Equal("Hello, world!", await run.Result.Text);
        Assert.Equal(2, (await run.Result.Steps).Count);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call the model again for the second step with the tool result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_model_again_for_the_second_step_with_the_tool_result()
    {
        var run = await RunAsync();

        Assert.Equal(2, run.Model.Prompts.Count);
        var first = run.Model.Prompts[0];
        Assert.Single(first);
        Assert.Equal("test-input", PromptText(first));
        var second = run.Model.Prompts[1];
        Assert.Equal(3, second.Count);
        Assert.Equal("test-input", PromptText(second));
        var assistant = Assert.IsType<AssistantModelMessage>(second[1]);
        AssertToolCallPart(
            Assert.Single(assistant.Parts),
            "call-1",
            "tool1",
            "{\"value\":\"value\"}"
        );
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(second[2]),
            "call-1",
            "tool1",
            "text",
            "result1"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should produce response messages with automatic approval metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_produce_response_messages_with_automatic_approval_metadata()
    {
        var run = await RunAsync();

        var messages = await run.Result.ResponseMessages;
        Assert.Equal(3, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal(2, assistant.Parts.Count);
        AssertToolCallPart(assistant.Parts[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequestPart(assistant.Parts[1], "id-1", "call-1", isAutomatic: true);
        var response = Assert.Single(
            Assert.IsType<ToolModelMessage>(messages[1]).ApprovalResponses
        );
        Assert.Equal("id-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.False(response.ProviderExecuted);
        Assert.Equal("trusted internal tool", response.Reason);
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(messages[1]),
            "call-1",
            "tool1",
            "text",
            "result1"
        );
        AssertText(messages[2], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should include automatic approval in the ui message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_automatic_approval_in_the_ui_message_stream()
    {
        var run = await RunAsync();

        AssertJson(
            """
            [
              { "type": "start" },
              { "type": "start-step" },
              { "input": { "value": "value" }, "toolCallId": "call-1", "toolName": "tool1", "type": "tool-input-available" },
              { "approvalId": "id-1", "isAutomatic": true, "toolCallId": "call-1", "type": "tool-approval-request" },
              { "approvalId": "id-1", "approved": true, "reason": "trusted internal tool", "type": "tool-approval-response" },
              { "output": "result1", "toolCallId": "call-1", "type": "tool-output-available" },
              { "type": "finish-step" },
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
        int ExecuteCalls,
        List<ToolExecutionOptions> Options
    )> RunAsync()
    {
        var executeCalls = 0;
        var options = new List<ToolExecutionOptions>();
        var model = new ScriptedModel(step =>
            step == 0 ? ToolCallsStep(("call-1", "{ \"value\": \"value\" }")) : TextStep()
        );
        var tool = ValueTool(null);
        tool.ExecuteWithOptions = (_, executionOptions) =>
        {
            executeCalls++;
            options.Add(executionOptions);
            return Task.FromResult("result1");
        };
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "tool1", new ToolApprovalStatus("approved", "trusted internal tool") },
                    },
                }
            );
        await result.ConsumeStreamAsync();
        return (result, model, executeCalls, options);
    }
}
