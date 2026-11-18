// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; tool-approval-request parts</c>.</summary>
public sealed class ToolApprovalRequestPartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > tool-approval-request parts::";

    private static Tool McpTool(string name, string property) =>
        Tool.ProviderDefined(
            "mcp." + name,
            name,
            Json("{}"),
            Json(
                "{\"type\":\"object\",\"properties\":{\""
                    + property
                    + "\":{\"type\":\"string\"}},\"required\":[\""
                    + property
                    + "\"]}"
            )
        );

    [Fact]
    [UpstreamTest(
        Prefix + "should emit error when tool call is not found for provider approval request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_error_when_tool_call_is_not_found_for_provider_approval_request()
    {
        var result = await StreamLanguageModelCallResult([
            // No tool-call part before the approval request
            new ToolApprovalRequestStreamPart("mcp-approval-1", "non-existent-call"),
            Finish(FinishReason.Stop, null),
        ]);

        Assert.Equal(2, result.Count);
        var error = Assert.IsType<ErrorPart>(result[0]);
        Assert.IsType<ToolCallNotFoundForApprovalException>(error.Error);
        Assert.Equal(
            "Tool call \"non-existent-call\" not found for approval request \"mcp-approval-1\".",
            error.Error!.Message
        );
        var end = ModelCallEnd(result[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Null(end.Performance.TimeToFirstOutputMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward provider-emitted tool-approval-request with the correct tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_provider_emitted_tool_approval_request_with_the_correct_tool_call()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new ToolCallStreamPart("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }")
                {
                    ProviderExecuted = true,
                },
                new ToolApprovalRequestStreamPart("mcp-approval-1", "mcp-call-1"),
                Finish(FinishReason.ToolCalls, null),
            ],
            tools: [McpTool("mcp_tool", "query")]
        );

        Assert.Equal(3, result.Count);
        var call = Assert.IsType<ToolCallPart>(result[0]).ToolCall;
        Assert.Equal("{\"query\":\"test\"}", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolName);
        var request = Assert.IsType<ToolApprovalRequestPart>(result[1]);
        Assert.Equal("mcp-approval-1", request.ApprovalId);
        Assert.Equal("{\"query\":\"test\"}", request.ToolCall.ArgumentsJson);
        Assert.True(request.ToolCall.ProviderExecuted);
        Assert.Equal("mcp-call-1", request.ToolCall.ToolCallId);
        Assert.Equal("mcp_tool", request.ToolCall.ToolName);
        var end = ModelCallEnd(result[2]);
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multiple provider-executed tool calls with approval requests",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_multiple_provider_executed_tool_calls_with_approval_requests()
    {
        var result = await StreamLanguageModelCallResult(
            [
                new ToolCallStreamPart("mcp-call-1", "mcp_search", "{ \"query\": \"first\" }")
                {
                    ProviderExecuted = true,
                },
                new ToolCallStreamPart("mcp-call-2", "mcp_execute", "{ \"command\": \"ls\" }")
                {
                    ProviderExecuted = true,
                },
                new ToolApprovalRequestStreamPart("approval-1", "mcp-call-1"),
                new ToolApprovalRequestStreamPart("approval-2", "mcp-call-2"),
                Finish(FinishReason.ToolCalls, null),
            ],
            tools: [McpTool("mcp_search", "query"), McpTool("mcp_execute", "command")]
        );

        Assert.Equal(5, result.Count);
        var first = Assert.IsType<ToolCallPart>(result[0]).ToolCall;
        Assert.Equal("{\"query\":\"first\"}", first.ArgumentsJson);
        Assert.Equal("mcp-call-1", first.ToolCallId);
        Assert.Equal("mcp_search", first.ToolName);
        var second = Assert.IsType<ToolCallPart>(result[1]).ToolCall;
        Assert.Equal("{\"command\":\"ls\"}", second.ArgumentsJson);
        Assert.Equal("mcp-call-2", second.ToolCallId);
        Assert.Equal("mcp_execute", second.ToolName);
        var firstRequest = Assert.IsType<ToolApprovalRequestPart>(result[2]);
        Assert.Equal("approval-1", firstRequest.ApprovalId);
        Assert.Equal("mcp-call-1", firstRequest.ToolCall.ToolCallId);
        Assert.Equal("{\"query\":\"first\"}", firstRequest.ToolCall.ArgumentsJson);
        var secondRequest = Assert.IsType<ToolApprovalRequestPart>(result[3]);
        Assert.Equal("approval-2", secondRequest.ApprovalId);
        Assert.Equal("mcp-call-2", secondRequest.ToolCall.ToolCallId);
        Assert.Equal("{\"command\":\"ls\"}", secondRequest.ToolCall.ArgumentsJson);
        var end = ModelCallEnd(result[4]);
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        var gaps = end.Performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(
            [0, 0, 0, 0, 0, 0],
            [gaps.Avg, gaps.Max, gaps.Median, gaps.Min, gaps.P10, gaps.P90]
        );
    }
}
