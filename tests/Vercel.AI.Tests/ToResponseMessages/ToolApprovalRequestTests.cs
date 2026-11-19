// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ToResponseMessages.ResponseMessagesAssert;

namespace Vercel.AI.Tests.ToResponseMessages;

/// <summary>Port of <c>to-response-messages.test.ts</c> &gt; <c>toResponseMessages</c> &gt; <c>tool approval request</c>.</summary>
public sealed class ToolApprovalRequestTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/to-response-messages.test.ts::toResponseMessages > tool approval request::";

    private const string TokyoInput = "{\"city\":\"Tokyo\"}";

    private static Tool Weather() => TestTool("weather", "Get weather information");

    private static GeneratedToolCall WeatherCall(string id = "call-1") =>
        new(id, "weather", TokyoInput);

    private static GeneratedToolCall McpCall() =>
        new("mcp-call-1", "mcp_tool", "{\"query\":\"test\"}")
        {
            ProviderExecuted = true,
            Dynamic = true,
        };

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve schema input when the approved input was transformed",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port's tool input schemas are JSON Schema and have no transform, so the input is transformed with RefineToolInput, which keeps the schema input like upstream's parseToolCall."
    )]
    public async Task Should_preserve_schema_input_when_the_approved_input_was_transformed()
    {
        var count = new Tool(
            "count",
            null,
            Json("{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"string\"}}}"),
            null
        );
        var options = new GenerateTextOptions
        {
            Tools = [count],
            RefineToolInput = new Dictionary<
                string,
                Func<JsonElement, CancellationToken, Task<JsonElement>>
            >
            {
                ["count"] = (input, _) =>
                    Task.FromResult(
                        Json(
                            "{\"count\":" + int.Parse(input.GetProperty("count").GetString()!) + "}"
                        )
                    ),
            },
        };
        var toolCall = await Generation.ParseToolCallAsync(
            new GeneratedToolCall("count-call", "count", "{\"count\":\"3\"}"),
            options,
            [],
            null,
            0,
            false,
            CancellationToken.None
        );

        var result = ToResponse(
            [
                toolCall,
                new GeneratedToolApprovalRequest("approval-1", "count-call")
                {
                    ToolCall = toolCall,
                },
            ],
            [count]
        );

        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(assistant, 0, "count-call", "count", "{\"count\":3}");
        var request = ApprovalRequest(assistant, 1, "approval-1", "count-call");
        Assert.Equal("{\"count\":\"3\"}", request.InputSchemaInput!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool approval request in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_approval_request_in_the_assistant_message()
    {
        var call = new GeneratedToolCall("123", "weather", TokyoInput);

        var result = ToResponse(
            [
                new GeneratedText("Let me check the weather"),
                call,
                new GeneratedToolApprovalRequest("approval-1", "123") { ToolCall = call },
            ],
            [Weather()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(3, assistant.Parts.Count);
        Text(assistant, 0, "Let me check the weather");
        ToolCall(assistant, 1, "123", "weather", TokyoInput);
        var request = ApprovalRequest(assistant, 2, "approval-1", "123");
        Assert.Null(request.InputSchemaInput);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool approval request for provider-executed tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_tool_approval_request_for_provider_executed_tools()
    {
        var call = McpCall();

        var result = ToResponse(
            [
                call,
                new GeneratedToolApprovalRequest("mcp-approval-1", "mcp-call-1")
                {
                    ToolCall = call,
                },
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(
            assistant,
            0,
            "mcp-call-1",
            "mcp_tool",
            "{\"query\":\"test\"}",
            providerExecuted: true
        );
        ApprovalRequest(assistant, 1, "mcp-approval-1", "mcp-call-1");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve automatic approval request stage in the assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_automatic_approval_request_stage_in_the_assistant_message()
    {
        var call = WeatherCall();

        var result = ToResponse(
            [
                call,
                new GeneratedToolApprovalRequest("approval-1", "call-1")
                {
                    ToolCall = call,
                    IsAutomatic = true,
                },
            ],
            [Weather()]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(assistant, 0, "call-1", "weather", TokyoInput);
        ApprovalRequest(assistant, 1, "approval-1", "call-1", isAutomatic: true);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include approval response and tool result stages in the tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_approval_response_and_tool_result_stages_in_the_tool_message()
    {
        var call = WeatherCall();

        var result = ToResponse(
            [
                call,
                new GeneratedToolApprovalRequest("approval-1", "call-1")
                {
                    ToolCall = call,
                    IsAutomatic = true,
                },
                new GeneratedToolApprovalResponse("approval-1", call, true),
                ClientResult("call-1", "weather", "72F and sunny"),
            ],
            [Weather()]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(assistant, 0, "call-1", "weather", TokyoInput);
        ApprovalRequest(assistant, 1, "approval-1", "call-1", isAutomatic: true);
        var response = Assert.Single(ToolMessage(result, 1).ApprovalResponses);
        Assert.Equal("approval-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.False(response.ProviderExecuted);
        Assert.Null(response.Reason);
        AssertToolResult(ToolMessage(result, 1), "call-1", "weather", "text", "72F and sunny");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add an execution-denied tool result when tool approval is denied",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_an_execution_denied_tool_result_when_tool_approval_is_denied()
    {
        var call = WeatherCall();

        var result = ToResponse(
            [
                call,
                new GeneratedToolApprovalRequest("approval-1", "call-1") { ToolCall = call },
                new GeneratedToolApprovalResponse("approval-1", call, false, "User denied access"),
            ],
            [Weather()]
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(assistant, 0, "call-1", "weather", TokyoInput);
        ApprovalRequest(assistant, 1, "approval-1", "call-1");
        var response = Assert.Single(ToolMessage(result, 1).ApprovalResponses);
        Assert.Equal("approval-1", response.ApprovalId);
        Assert.False(response.Approved);
        Assert.Equal("User denied access", response.Reason);
        var denied = ToolMessage(result, 1);
        Assert.Equal("call-1", denied.ToolCallId);
        Assert.Equal("weather", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal("User denied access", denied.ExecutionDeniedReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include provider-executed approval response stages in the tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_provider_executed_approval_response_stages_in_the_tool_message()
    {
        var call = McpCall();

        var result = ToResponse(
            [
                call,
                new GeneratedToolApprovalRequest("mcp-approval-1", "mcp-call-1")
                {
                    ToolCall = call,
                },
                new GeneratedToolApprovalResponse("mcp-approval-1", call, true, null, true),
            ],
            []
        );

        Assert.Equal(2, result.Count);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(
            assistant,
            0,
            "mcp-call-1",
            "mcp_tool",
            "{\"query\":\"test\"}",
            providerExecuted: true
        );
        ApprovalRequest(assistant, 1, "mcp-approval-1", "mcp-call-1");
        var toolMessage = ToolMessage(result, 1);
        Assert.Empty(toolMessage.Results);
        var response = Assert.Single(toolMessage.ApprovalResponses);
        Assert.Equal("mcp-approval-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.True(response.ProviderExecuted);
        Assert.Null(response.Reason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep provider-executed tool result stages in the assistant message only",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_keep_provider_executed_tool_result_stages_in_the_assistant_message_only()
    {
        var result = ToResponse(
            [
                McpCall(),
                new GeneratedToolResult("mcp-call-1", "mcp_tool", "{\"value\":\"provider result\"}")
                {
                    Dynamic = true,
                },
            ],
            []
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(2, assistant.Parts.Count);
        ToolCall(
            assistant,
            0,
            "mcp-call-1",
            "mcp_tool",
            "{\"query\":\"test\"}",
            providerExecuted: true
        );
        var toolResult = Part<AssistantToolResultPart>(assistant, 1);
        Assert.Null(toolResult.ProviderOptions);
        AssertToolResult(
            toolResult.Output,
            "mcp-call-1",
            "mcp_tool",
            "json",
            "{\"value\":\"provider result\"}"
        );
    }
}
