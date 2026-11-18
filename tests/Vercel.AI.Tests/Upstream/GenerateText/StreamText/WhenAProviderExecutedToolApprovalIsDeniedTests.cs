// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; provider-executed tool (MCP) approval &gt; when a provider-executed tool approval is denied</c>.</summary>
public sealed class WhenAProviderExecutedToolApprovalIsDeniedTests
{
    private const string Suite =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool approval is denied::";

    [Fact]
    [UpstreamTest(
        Suite + "should send denied tool-approval-response to the model exactly once",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_denied_tool_approval_response_to_the_model_exactly_once()
    {
        var (result, model) = Run();
        await result.ConsumeStreamAsync();

        var response = ProviderExecutedToolApprovalSupport.AssertPrompt(model.Prompts[0]);
        Assert.Equal("mcp-approval-1", response.ApprovalId);
        Assert.False(response.Approved);
        Assert.Equal("User denied the request", response.Reason);
        var denied = Assert.IsType<ToolModelMessage>(response.DeniedToolResult);
        Assert.Equal("mcp-call-1", denied.ToolCallId);
        Assert.Equal("mcp_tool", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal("User denied the request", denied.OutputJson);
        Assert.Equal(
            "mcp-approval-1",
            denied
                .OutputProviderOptions!.Value.GetProperty("openai")
                .GetProperty("approvalId")
                .GetString()
        );
        Assert.Null(denied.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should include text response from model in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_text_response_from_model_in_content()
    {
        var (result, _) = Run();

        Assert.Equal(
            "I understand. The tool execution was not approved.",
            Assert.IsType<GeneratedText>(Assert.Single(await ContentAsync(result))).Text
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should have stop finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_stop_finish_reason()
    {
        var (result, _) = Run();

        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    private static (StreamTextResult Result, ScriptedModel Model) Run()
    {
        var model = new ScriptedModel(_ =>
            TextStep("I understand. The tool execution was not approved.")
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
                            false,
                            "User denied the request",
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
