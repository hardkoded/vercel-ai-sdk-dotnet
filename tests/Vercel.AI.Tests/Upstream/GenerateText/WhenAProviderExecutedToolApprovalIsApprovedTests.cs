// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class WhenAProviderExecutedToolApprovalIsApprovedTests
{
    private const string Suite = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > provider-executed tool (MCP) approval > when a provider-executed tool approval is approved::";

    [Fact]
    [UpstreamTest(Suite + "should send tool-approval-response to the model exactly once", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_tool_approval_response_to_the_model_exactly_once()
    {
        var (_, prompts) = await RunAsync();
        var response = ProviderExecutedToolApprovalSupport.AssertPrompt(prompts[0]);
        Assert.Equal("mcp-approval-1", response.ApprovalId);
        Assert.True(response.Approved);
        Assert.Null(response.Reason);
        Assert.Null(response.DeniedToolResult);
    }

    [Fact]
    [UpstreamTest(Suite + "should include provider-executed tool result in content", Coverage = UpstreamCoverage.Partial, Note = "The tool result has no input property in the port.")]
    public async Task Should_include_provider_executed_tool_result_in_content()
    {
        var (result, _) = await RunAsync();
        Assert.Equal(new[] { "tool-call", "tool-result", "text" }, result.Content.Select(part => part.Type).ToArray());
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("mcp-call-1", call.ToolCallId);
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.Equal("mcp-call-1", toolResult.ToolCallId);
        Assert.Equal("mcp_tool", toolResult.ToolName);
        using (var output = JsonDocument.Parse(toolResult.ResultJson))
        {
            Assert.Equal("https://short.url/abc", output.RootElement.GetProperty("shortened_url").GetString());
        }

        Assert.Equal("Here is your shortened URL: https://short.url/abc", Assert.IsType<GeneratedText>(result.Content[2]).Text);
    }

    private static async Task<(GenerateTextResult Result, List<IReadOnlyList<ModelMessage>> Prompts)> RunAsync()
    {
        var prompts = new List<IReadOnlyList<ModelMessage>>();
        var model = new TestLanguageModel
        {
            OnGenerate = options =>
            {
                prompts.Add(options.Prompt);
                return prompts.Count == 1
                    ? new LanguageModelGenerateResult(
                        new GeneratedContent[]
                        {
                            new GeneratedToolCall("mcp-call-1", "mcp_tool", "{ \"query\": \"test\" }") { ProviderExecuted = true },
                            new GeneratedToolResult("mcp-call-1", "mcp_tool", "{\"shortened_url\":\"https://short.url/abc\"}"),
                            new GeneratedText("Here is your shortened URL: https://short.url/abc"),
                        },
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        "stop")
                    : TestLanguageModel.Text("Done");
            },
        };
        var history = ProviderExecutedToolApprovalSupport.History(new ToolApprovalResponseModelMessage("mcp-approval-1", true, providerExecuted: true));
        var result = await ProviderExecutedToolApprovalSupport.Client().GenerateTextAsync(ProviderExecutedToolApprovalSupport.Options(model, history));
        return (result, prompts);
    }
}
