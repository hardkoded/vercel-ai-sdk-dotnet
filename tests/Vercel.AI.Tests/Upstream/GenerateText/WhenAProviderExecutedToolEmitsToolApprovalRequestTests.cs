// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class WhenAProviderExecutedToolEmitsToolApprovalRequestTests
{
    private const string Suite = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool emits tool-approval-request::";

    [Fact]
    [UpstreamTest(Suite + "should only execute 1 step when waiting for approval", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_1_step_when_waiting_for_approval()
    {
        var result = await RunAsync();
        Assert.Single(result.Steps);
    }

    [Fact]
    [UpstreamTest(Suite + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        var result = await RunAsync();
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(Suite + "should add tool approval request with providerExecuted tool call to content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_tool_approval_request_with_providerExecuted_tool_call_to_content()
    {
        var result = await RunAsync();
        Assert.Equal(new[] { "tool-call", "tool-approval-request" }, result.Content.Select(part => part.Type).ToArray());
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolName);
        Assert.Equal("{ \"query\": \"test\" }", call.ArgumentsJson);
        var request = Assert.IsType<GeneratedToolApprovalRequest>(result.Content[1]);
        Assert.Equal("mcp-approval-1", request.ApprovalId);
        Assert.Same(call, request.ToolCall);
    }

    [Fact]
    [UpstreamTest(Suite + "should include tool approval request in response messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_tool_approval_request_in_response_messages()
    {
        var result = await RunAsync();
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.ResponseMessages));
        Assert.Equal(2, message.Parts.Count);
        var call = Assert.IsType<AssistantToolCallPart>(message.Parts[0]);
        Assert.True(call.ToolCall.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCall.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolCall.ToolName);
        Assert.Equal("{ \"query\": \"test\" }", call.ToolCall.ArgumentsJson);
        Assert.Null(call.ProviderOptions);
        var request = Assert.IsType<AssistantToolApprovalRequestPart>(message.Parts[1]);
        Assert.Equal("mcp-approval-1", request.ApprovalId);
        Assert.Equal("mcp-call-1", request.ToolCallId);
        Assert.Null(request.IsAutomatic);
    }

    private static Task<GenerateTextResult> RunAsync()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }") { ProviderExecuted = true },
                    new GeneratedToolApprovalRequest("mcp-approval-1", "mcp-call-1"),
                },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13),
                null),
        };
        return ProviderExecutedToolApprovalSupport.Client().GenerateTextAsync(ProviderExecutedToolApprovalSupport.Options(model, null));
    }
}
