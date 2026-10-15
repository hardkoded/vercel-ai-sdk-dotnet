// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class SingleProviderExecutedToolCallAndResultTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > single provider-executed tool call and result::should only execute a single step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_execute_a_single_step()
    {
        var result = Run();
        Assert.Single(await result.Steps);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > single provider-executed tool call and result::should include provider-executed tool call and result content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_provider_executed_tool_call_and_result_content()
    {
        var result = Run();
        var step = Assert.Single(await result.Steps);
        Assert.Equal(new[] { "tool-call", "tool-result", "tool-call", "tool-result" }, step.Content.Select(part => part.Type).ToArray());
        Assert.True(Assert.IsType<GeneratedToolCall>(step.Content[0]).ProviderExecuted);
        var result1 = Assert.IsType<GeneratedToolResult>(step.Content[1]);
        Assert.False(result1.IsError);
        Assert.Equal("result-1", result1.ProviderMetadata!.Value.GetProperty("provider").GetProperty("itemId").GetString());
        Assert.True(Assert.IsType<GeneratedToolCall>(step.Content[2]).ProviderExecuted);
        var result2 = Assert.IsType<GeneratedToolResult>(step.Content[3]);
        Assert.True(result2.IsError);
        Assert.Equal("error-1", result2.ProviderMetadata!.Value.GetProperty("provider").GetProperty("itemId").GetString());
    }

    private static StreamTextResult Run()
    {
        var args = JsonDocument.Parse("{}").RootElement;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolInputStartStreamPart("call-1", "web_search", providerExecuted: true),
                new ToolInputDeltaStreamPart("call-1", "{ \"value\": \"value\" }"),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                new ToolResultStreamPart("call-1", "web_search", JsonDocument.Parse("{\"value\":\"result1\"}").RootElement.Clone()) { ProviderMetadata = JsonDocument.Parse("{\"provider\":{\"itemId\":\"result-1\"}}").RootElement.Clone() },
                new ToolCallStreamPart("call-2", "web_search", "{ \"value\": \"value\" }") { ProviderExecuted = true },
                new ToolResultStreamPart("call-2", "web_search", JsonDocument.Parse("\"ERROR\"").RootElement.Clone()) { IsError = true, ProviderMetadata = JsonDocument.Parse("{\"provider\":{\"itemId\":\"error-1\"}}").RootElement.Clone() },
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.ProviderDefined("test.web_search", "web_search", args) },
            StopWhen = StopWhen.IsStepCount(4),
        });
    }
}
