// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a single tool is automatically approved</c>.</summary>
public sealed class WhenASingleToolIsAutomaticallyApprovedTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a single tool is automatically approved::";

    [Fact]
    [UpstreamTest(Id + "should continue to a second step and execute the tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_continue_to_a_second_step_and_execute_the_tool()
    {
        var run = await RunAsync();

        var call = Assert.Single(run.ExecuteCalls);
        Assert.Equal("{\"value\":\"value\"}", Compact(call.Input.GetRawText()));
        Assert.False(call.Options.AbortSignal.CanBeCanceled);
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.NotNull(call.Options.Messages);
        Assert.Equal("Hello, world!", run.Result.Text);
        Assert.Equal(2, run.Result.Steps.Count);
    }

    [Fact]
    [UpstreamTest(Id + "should call the model again for the second step with the tool result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_the_model_again_for_the_second_step_with_the_tool_result()
    {
        var run = await RunAsync();

        Assert.Equal(2, run.Model.Calls.Count);
        var first = run.Model.Calls[0].Prompt;
        Assert.Equal("test-input", PromptText(first));
        Assert.Single(first);
        var second = run.Model.Calls[1].Prompt;
        Assert.Equal(3, second.Count);
        Assert.Equal("test-input", PromptText(second));
        var assistant = Assert.IsType<AssistantModelMessage>(second[1]);
        AssertToolCallPart(Assert.Single(assistant.Parts), "call-1", "tool1", "{\"value\":\"value\"}");
        AssertToolResult(Assert.IsType<ToolModelMessage>(second[2]), "call-1", "tool1", "text", "result1");
    }

    [Fact]
    [UpstreamTest(
        Id + "should produce response messages with automatic approval metadata",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream puts the approval response and the tool result in one tool message. The port keeps the approval response as its own tool-role message, followed by the tool message.")]
    public async Task Should_produce_response_messages_with_automatic_approval_metadata()
    {
        var run = await RunAsync();

        var messages = run.Result.ResponseMessages;
        Assert.Equal(4, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal(2, assistant.Parts.Count);
        AssertToolCallPart(assistant.Parts[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequestPart(assistant.Parts[1], "id-1", "call-1", isAutomatic: true);
        var response = Assert.IsType<ToolApprovalResponseModelMessage>(messages[1]);
        Assert.Equal("id-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.False(response.ProviderExecuted);
        Assert.Equal("trusted internal tool", response.Reason);
        AssertToolResult(Assert.IsType<ToolModelMessage>(messages[2]), "call-1", "tool1", "text", "result1");
        AssertText(messages[3], "Hello, world!");
    }

    private static async Task<(GenerateTextResult Result, TestLanguageModel Model, List<(JsonElement Input, ToolExecutionOptions Options)> ExecuteCalls)> RunAsync()
    {
        var executeCalls = new List<(JsonElement Input, ToolExecutionOptions Options)>();
        var step = 0;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => step++ == 0 ? ToolCalls("tool1", ("call-1", "{ \"value\": \"value\" }")) : Text("Hello, world!"),
        };
        var tool = ValueTool(null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            executeCalls.Add((input, options));
            return Task.FromResult("result1");
        };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Tools = new[] { tool },
            ToolApproval = new ToolApprovalConfiguration
            {
                { "tool1", new ToolApprovalStatus("approved", "trusted internal tool") },
            },
        });
        return (result, model, executeCalls);
    }
}
