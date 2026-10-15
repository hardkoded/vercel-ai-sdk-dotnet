// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class TwoProviderExecutedToolCallsAndResultsTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > provider-executed tools > two provider-executed tool calls and results::should include provider-executed tool calls and results in the content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_provider_executed_tool_calls_and_results_in_the_content()
    {
        var result = await RunAsync();
        Assert.Equal(new[] { "tool-call", "tool-result", "tool-call", "tool-result" }, result.Content.Select(part => part.Type).ToArray());
        var call1 = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call1.ProviderExecuted);
        Assert.Equal("call-1", call1.ToolCallId);
        Assert.Equal("web_search", call1.ToolName);
        var result1 = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.False(result1.IsError);
        Assert.Equal("{ \"value\": \"result1\" }", result1.ResultJson);
        Assert.Equal("tool-result-1", result1.ProviderMetadata!.Value.GetProperty("openai").GetProperty("itemId").GetString());
        var call2 = Assert.IsType<GeneratedToolCall>(result.Content[2]);
        Assert.True(call2.ProviderExecuted);
        var result2 = Assert.IsType<GeneratedToolResult>(result.Content[3]);
        Assert.True(result2.IsError);
        Assert.Equal("ERROR", result2.ResultJson);
        Assert.Equal("tool-result-2", result2.ProviderMetadata!.Value.GetProperty("openai").GetProperty("itemId").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > provider-executed tools > two provider-executed tool calls and results::should include provider-executed tool calls in staticToolCalls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_provider_executed_tool_calls_in_staticToolCalls()
    {
        var result = await RunAsync();
        Assert.Equal(new[] { "call-1", "call-2" }, result.ToolCalls.Select(call => call.ToolCallId).ToArray());
        Assert.All(result.ToolCalls, call => Assert.True(call.ProviderExecuted));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > provider-executed tools > two provider-executed tool calls and results::should include provider-executed results in staticToolResults (errors excluded)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_provider_executed_results_in_staticToolResults_errors_excluded()
    {
        var result = await RunAsync();
        var toolResult = Assert.Single(result.FinalStep.ToolResults);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("web_search", toolResult.ToolName);
        Assert.Equal("{ \"value\": \"result1\" }", toolResult.OutputJson);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > provider-executed tools > two provider-executed tool calls and results::should only execute a single step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_a_single_step()
    {
        var result = await RunAsync();
        Assert.Single(result.Steps);
    }

    private static async Task<GenerateTextResult> RunAsync()
    {
        using var args = JsonDocument.Parse("{}");
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("call-1", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                    new GeneratedToolResult("call-1", "web_search", "{ \"value\": \"result1\" }") { ProviderMetadata = JsonDocument.Parse("{\"openai\":{\"itemId\":\"tool-result-1\"}}").RootElement.Clone() },
                    new GeneratedToolCall("call-2", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                    new GeneratedToolResult("call-2", "web_search", "ERROR", true) { ProviderMetadata = JsonDocument.Parse("{\"openai\":{\"itemId\":\"tool-result-2\"}}").RootElement.Clone() },
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        return await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.ProviderDefined("test.web_search", "web_search", args.RootElement) },
            StopWhen = StopWhen.IsStepCount(4),
        });
    }
}
