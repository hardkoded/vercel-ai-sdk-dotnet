// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a single tool has a needsApproval function</c>.</summary>
public sealed class WhenASingleToolHasANeedsApprovalFunctionTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a single tool has a needsApproval function::";

    [Fact]
    [UpstreamTest(Id + "should only execute 1 step when the tool needs approval", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_1_step_when_the_tool_needs_approval()
    {
        var (result, _) = await RunAsync();

        Assert.Single(result.Steps);
    }

    [Fact]
    [UpstreamTest(Id + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(Id + "should add a tool approval request to the content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_a_tool_approval_request_to_the_content()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(4, result.Content.Count);
        AssertToolCall(result.Content[0], "call-1", "tool1", "{\"value\":\"value-needs-approval\"}");
        AssertToolCall(result.Content[1], "call-2", "tool1", "{\"value\":\"value-no-approval\"}");
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[2]);
        Assert.False(toolResult.Dynamic);
        Assert.Equal("result for value-no-approval", toolResult.ResultJson);
        Assert.Equal("call-2", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        AssertApprovalRequest(result.Content[3], "id-1", "call-1", "tool1", "{\"value\":\"value-needs-approval\"}");
    }

    [Fact]
    [UpstreamTest(Id + "should include tool approval request in response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_tool_approval_request_in_response_messages()
    {
        var (result, _) = await RunAsync();

        Assert.Equal(2, result.ResponseMessages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(result.ResponseMessages[0]);
        Assert.Equal(3, assistant.Parts.Count);
        AssertToolCallPart(assistant.Parts[0], "call-1", "tool1", "{\"value\":\"value-needs-approval\"}");
        AssertToolCallPart(assistant.Parts[1], "call-2", "tool1", "{\"value\":\"value-no-approval\"}");
        AssertApprovalRequestPart(assistant.Parts[2], "id-1", "call-1");
        var tool = Assert.IsType<ToolModelMessage>(result.ResponseMessages[1]);
        AssertToolResult(tool, "call-2", "tool1", "text", "result for value-no-approval");
    }

    [Fact]
    [UpstreamTest(
        Id + "should call the needsApproval function with the correct input and options",
        Coverage = UpstreamCoverage.Partial,
        Note = "The options carry toolCallId and messages. The port has no tool context or runtime context, so toolContext and runtimeContext are not checked.")]
    public async Task Should_call_the_needsApproval_function_with_the_correct_input_and_options()
    {
        var (_, calls) = await RunAsync();

        Assert.Equal(2, calls.Count);
        Assert.Equal("{\"value\":\"value-needs-approval\"}", Compact(calls[0].Input.GetRawText()));
        Assert.Equal("call-1", calls[0].Options.ToolCallId);
        Assert.Equal("test-input", PromptText(calls[0].Options.Messages));
        Assert.Single(calls[0].Options.Messages);
        Assert.Equal("{\"value\":\"value-no-approval\"}", Compact(calls[1].Input.GetRawText()));
        Assert.Equal("call-2", calls[1].Options.ToolCallId);
        Assert.Equal("test-input", PromptText(calls[1].Options.Messages));
        Assert.Single(calls[1].Options.Messages);
    }

    private static async Task<(GenerateTextResult Result, List<(JsonElement Input, SingleToolApprovalOptions Options)> Calls)> RunAsync()
    {
        var calls = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => ToolCalls("tool1", ("call-1", "{ \"value\": \"value-needs-approval\" }"), ("call-2", "{ \"value\": \"value-no-approval\" }")),
            },
            Tools = new[]
            {
                ValueTool((input, _) => Task.FromResult("result for " + input.GetProperty("value").GetString())),
            },
            ToolApproval = new ToolApprovalConfiguration
            {
                {
                    "tool1",
                    (input, options) =>
                    {
                        calls.Add((input, options));
                        return Task.FromResult<ToolApprovalStatus?>(input.GetProperty("value").GetString() == "value-needs-approval" ? "user-approval" : "not-applicable");
                    }
                },
            },
            StopWhen = StopWhen.IsStepCount(3),
            Prompt = "test-input",
            GenerateId = MockId("id"),
        });
        return (result, calls);
    }
}
