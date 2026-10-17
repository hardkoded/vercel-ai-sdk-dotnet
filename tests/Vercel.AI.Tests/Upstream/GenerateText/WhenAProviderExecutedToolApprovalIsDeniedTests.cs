// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class WhenAProviderExecutedToolApprovalIsDeniedTests
{
    private const string Suite = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool approval is denied::";

    [Fact]
    [UpstreamTest(Suite + "should send denied tool-approval-response to the model exactly once", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_denied_tool_approval_response_to_the_model_exactly_once()
    {
        var (_, prompts) = await RunAsync();
        var response = ProviderExecutedToolApprovalSupport.AssertPrompt(prompts[0]);
        Assert.Equal("mcp-approval-1", response.ApprovalId);
        Assert.False(response.Approved);
        Assert.Equal("User denied the request", response.Reason);
        var denied = Assert.IsType<ToolModelMessage>(response.DeniedToolResult);
        Assert.Equal("mcp-call-1", denied.ToolCallId);
        Assert.Equal("mcp_tool", denied.ToolName);
        Assert.Equal("execution-denied", denied.OutputType);
        Assert.Equal("User denied the request", denied.OutputJson);
        Assert.Equal("mcp-approval-1", denied.OutputProviderOptions!.Value.GetProperty("openai").GetProperty("approvalId").GetString());
        Assert.Null(denied.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Suite + "should include text response from model in content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_text_response_from_model_in_content()
    {
        var (result, _) = await RunAsync();
        Assert.Equal("I understand. The tool execution was not approved.", Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text);
    }

    [Fact]
    [UpstreamTest(Suite + "should have stop finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_stop_finish_reason()
    {
        var (result, _) = await RunAsync();
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    private static async Task<(GenerateTextResult Result, List<IReadOnlyList<ModelMessage>> Prompts)> RunAsync()
    {
        var prompts = new List<IReadOnlyList<ModelMessage>>();
        var model = new TestLanguageModel
        {
            OnGenerate = options =>
            {
                prompts.Add(options.Prompt);
                return TestLanguageModel.Text("I understand. The tool execution was not approved.");
            },
        };
        var history = ProviderExecutedToolApprovalSupport.History(new ToolApprovalResponseModelMessage("mcp-approval-1", false, "User denied the request", providerExecuted: true));
        var result = await ProviderExecutedToolApprovalSupport.Client().GenerateTextAsync(ProviderExecutedToolApprovalSupport.Options(model, history));
        return (result, prompts);
    }
}
