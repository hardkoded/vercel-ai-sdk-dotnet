// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; provider-executed tool (MCP) approval &gt; when a provider-executed tool approval is approved</c>.</summary>
public sealed class WhenAProviderExecutedToolApprovalIsApprovedTests
{
    private const string Suite =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool approval is approved::";

    [Fact]
    [UpstreamTest(
        Suite + "should send tool-approval-response to the model exactly once",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_tool_approval_response_to_the_model_exactly_once()
    {
        var (result, model) = Run();
        await result.ConsumeStreamAsync();

        var response = ProviderExecutedToolApprovalSupport.AssertPrompt(model.Prompts[0]);
        Assert.Equal("mcp-approval-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.Null(response.Reason);
        Assert.Null(response.DeniedToolResult);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should include provider-executed tool result in content",
        Coverage = UpstreamCoverage.Partial,
        Note = "The tool result has no input property in the port. The step content of streamText lists the text before the tool call and the tool result, where upstream keeps the stream order (tool-call, tool-result, text)."
    )]
    public async Task Should_include_provider_executed_tool_result_in_content()
    {
        var (result, _) = Run();

        var content = await ContentAsync(result);

        Assert.Equal(
            new[] { "text", "tool-call", "tool-result" },
            content.Select(part => part.Type)
        );
        var call = Assert.IsType<GeneratedToolCall>(content[1]);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCallId);
        Assert.Equal("mcp_tool", call.ToolName);
        var toolResult = Assert.IsType<GeneratedToolResult>(content[2]);
        Assert.True(toolResult.ProviderExecuted);
        Assert.Equal("mcp-call-1", toolResult.ToolCallId);
        Assert.Equal("mcp_tool", toolResult.ToolName);
        using (var output = JsonDocument.Parse(toolResult.ResultJson))
        {
            Assert.Equal(
                "https://short.url/abc",
                output.RootElement.GetProperty("shortened_url").GetString()
            );
        }

        Assert.Equal(
            "Here is your shortened URL: https://short.url/abc",
            Assert.IsType<GeneratedText>(content[0]).Text
        );
    }

    private static (StreamTextResult Result, ScriptedModel Model) Run()
    {
        var model = new ScriptedModel(_ =>
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new ToolCallStreamPart("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }")
                {
                    ProviderExecuted = true,
                },
                new ToolResultStreamPart(
                    "mcp-call-1",
                    "mcp_tool",
                    JsonDocument.Parse("{\"shortened_url\":\"https://short.url/abc\"}").RootElement
                ),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Here is your shortened URL: https://short.url/abc"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(
                    FinishReason.Stop,
                    ToolExecutionApprovalStreamSupport.Usage,
                    "stop"
                ),
            }
        );
        var result = ProviderExecutedToolApprovalSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Messages = ProviderExecutedToolApprovalSupport.History(
                        new ToolApprovalResponseModelMessage(
                            "mcp-approval-1",
                            true,
                            providerExecuted: true
                        )
                    ),
                    Tools = new[] { ProviderExecutedToolApprovalSupport.McpTool() },
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                }
            );
        return (result, model);
    }
}
