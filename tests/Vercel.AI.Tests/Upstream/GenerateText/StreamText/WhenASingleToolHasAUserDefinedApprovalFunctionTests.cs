// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a single tool has a user-defined approval function</c>.</summary>
public sealed class WhenASingleToolHasAUserDefinedApprovalFunctionTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a single tool has a user-defined approval function::";

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_full_stream()
    {
        var run = Run();
        var parts = await ReadAsync(run.Result);

        Assert.Equal(
            new[]
            {
                "start",
                "start-step",
                "tool-call",
                "tool-approval-request",
                "tool-call",
                "tool-result",
                "finish-step",
                "finish",
            },
            Types(parts)
        );
        AssertStartStep(parts[1]);
        AssertToolCall(
            Assert.IsType<ToolCallPart>(parts[2]).ToolCall,
            "call-1",
            "tool1",
            "{\"value\":\"value-needs-approval\"}"
        );
        var request = Assert.IsType<ToolApprovalRequestPart>(parts[3]);
        Assert.Equal("id-1", request.ApprovalId);
        AssertToolCall(request.ToolCall, "call-1", "tool1", "{\"value\":\"value-needs-approval\"}");
        AssertToolCall(
            Assert.IsType<ToolCallPart>(parts[4]).ToolCall,
            "call-2",
            "tool1",
            "{\"value\":\"value-no-approval\"}"
        );
        var toolResult = Assert.IsType<ToolResultPart>(parts[5]).Result;
        Assert.False(toolResult.Dynamic);
        Assert.Equal("call-2", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.Equal("result for value-no-approval", toolResult.OutputJson);
        AssertStepAndFinish(parts, 6, FinishReason.ToolCalls, null, toolExecutionMs =>
            Assert.Equal(new[] { "call-2" }, toolExecutionMs.Keys.ToArray())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the UI message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_UI_message_stream()
    {
        AssertJson(
            """
            [
              { "type": "start" },
              { "type": "start-step" },
              { "input": { "value": "value-needs-approval" }, "toolCallId": "call-1", "toolName": "tool1", "type": "tool-input-available" },
              { "approvalId": "id-1", "toolCallId": "call-1", "type": "tool-approval-request" },
              { "input": { "value": "value-no-approval" }, "toolCallId": "call-2", "toolName": "tool1", "type": "tool-input-available" },
              { "output": "result for value-no-approval", "toolCallId": "call-2", "type": "tool-output-available" },
              { "type": "finish-step" },
              { "finishReason": "tool-calls", "type": "finish" }
            ]
            """,
            await UiChunksAsync(Run().Result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should only execute 1 step when the tool needs approval",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_execute_1_step_when_the_tool_needs_approval()
    {
        Assert.Single(await Run().Result.Steps);
    }

    [Fact]
    [UpstreamTest(Id + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        Assert.Equal(FinishReason.ToolCalls, await Run().Result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Id + "should add a tool approval request to the content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_a_tool_approval_request_to_the_content()
    {
        var content = await ContentAsync(Run().Result);

        Assert.Equal(4, content.Count);
        AssertToolCall(content[0], "call-1", "tool1", "{\"value\":\"value-needs-approval\"}");
        AssertApprovalRequest(
            content[1],
            "id-1",
            "call-1",
            "tool1",
            "{\"value\":\"value-needs-approval\"}"
        );
        AssertToolCall(content[2], "call-2", "tool1", "{\"value\":\"value-no-approval\"}");
        var toolResult = Assert.IsType<GeneratedToolResult>(content[3]);
        Assert.False(toolResult.Dynamic);
        Assert.Equal("result for value-no-approval", toolResult.ResultJson);
        Assert.Equal("call-2", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the response messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_response_messages()
    {
        var messages = await Run().Result.ResponseMessages;

        Assert.Equal(2, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal(3, assistant.Parts.Count);
        AssertToolCallPart(
            assistant.Parts[0],
            "call-1",
            "tool1",
            "{\"value\":\"value-needs-approval\"}"
        );
        AssertApprovalRequestPart(assistant.Parts[1], "id-1", "call-1");
        AssertToolCallPart(
            assistant.Parts[2],
            "call-2",
            "tool1",
            "{\"value\":\"value-no-approval\"}"
        );
        AssertToolResult(
            Assert.IsType<ToolModelMessage>(messages[1]),
            "call-2",
            "tool1",
            "text",
            "result for value-no-approval"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should call the approval function with the correct input and options",
        Coverage = UpstreamCoverage.Partial,
        Note = "The options carry toolCallId and messages. The port has no tool context or runtime context, so toolContext and runtimeContext are not checked."
    )]
    public async Task Should_call_the_approval_function_with_the_correct_input_and_options()
    {
        var run = Run();
        await run.Result.ConsumeStreamAsync();

        var calls = run.Calls;
        Assert.Equal(2, calls.Count);
        AssertInput("{\"value\":\"value-needs-approval\"}", calls[0].Input);
        Assert.Equal("call-1", calls[0].Options.ToolCallId);
        Assert.Equal("test-input", PromptText(calls[0].Options.Messages));
        Assert.Single(calls[0].Options.Messages);
        AssertInput("{\"value\":\"value-no-approval\"}", calls[1].Input);
        Assert.Equal("call-2", calls[1].Options.ToolCallId);
        Assert.Equal("test-input", PromptText(calls[1].Options.Messages));
        Assert.Single(calls[1].Options.Messages);
    }

    private static (
        StreamTextResult Result,
        List<(JsonElement Input, SingleToolApprovalOptions Options)> Calls
    ) Run()
    {
        var calls = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new ScriptedModel(_ =>
                        ToolCallsStep(
                            ("call-1", "{ \"value\": \"value-needs-approval\" }"),
                            ("call-2", "{ \"value\": \"value-no-approval\" }")
                        )
                    ),
                    Tools = new[]
                    {
                        ValueTool(
                            (input, _) =>
                                Task.FromResult(
                                    "result for " + input.GetProperty("value").GetString()
                                )
                        ),
                    },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        {
                            "tool1",
                            (input, options) =>
                            {
                                calls.Add((input, options));
                                return Task.FromResult<ToolApprovalStatus?>(
                                    input.GetProperty("value").GetString() == "value-needs-approval"
                                        ? "user-approval"
                                        : "not-applicable"
                                );
                            }
                        },
                    },
                    StopWhen = StopWhen.IsStepCount(3),
                    Prompt = "test-input",
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                }
            );
        return (result, calls);
    }
}
