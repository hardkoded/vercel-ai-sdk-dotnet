// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a single tool needs approval (tool-defined)</c>.</summary>
public sealed class WhenASingleToolNeedsApprovalToolDefinedTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a single tool needs approval (tool-defined)::";

    [Fact]
    [UpstreamTest(Id + "should only execute 1 step when the tool needs approval", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_1_step_when_the_tool_needs_approval()
    {
        var result = await RunAsync();

        Assert.Single(result.Steps);
    }

    [Fact]
    [UpstreamTest(Id + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        var result = await RunAsync();

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(Id + "should add a tool approval request to the content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_a_tool_approval_request_to_the_content()
    {
        var result = await RunAsync();

        Assert.Equal(2, result.Content.Count);
        AssertToolCall(result.Content[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequest(result.Content[1], "id-1", "call-1", "tool1", "{\"value\":\"value\"}");
    }

    [Fact]
    [UpstreamTest(Id + "should include tool approval request in response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_tool_approval_request_in_response_messages()
    {
        var result = await RunAsync();

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.ResponseMessages));
        Assert.Equal(2, message.Parts.Count);
        AssertToolCallPart(message.Parts[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequestPart(message.Parts[1], "id-1", "call-1");
    }

    private static Task<GenerateTextResult> RunAsync() =>
        Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => ToolCalls("tool1", ("call-1", "{ \"value\": \"value\" }")) },
            Tools = new[] { ValueTool((_, _) => Task.FromResult("result1"), needsApproval: true) },
            StopWhen = StopWhen.IsStepCount(3),
            Prompt = "test-input",
            GenerateId = MockId("id"),
        });
}
