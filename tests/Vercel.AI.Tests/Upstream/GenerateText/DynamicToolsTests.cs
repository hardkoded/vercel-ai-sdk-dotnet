// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; dynamic tools</c>.</summary>
public sealed class DynamicToolsTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > dynamic tools::should execute dynamic tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_dynamic_tools()
    {
        var toolExecuted = false;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "dynamicTool", "{ \"value\": \"test\" }") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13),
                "tool-calls"),
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.DynamicTool("dynamicTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) =>
                {
                    toolExecuted = true;
                    return Task.FromResult("{\"value\":\"test-result\"}");
                }),
            },
        });

        // tool should be executed by client
        Assert.True(toolExecuted);

        // tool call should be included in content
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.Dynamic);
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("dynamicTool", call.ToolName);
        using (var input = JsonDocument.Parse(call.ArgumentsJson))
        {
            Assert.Equal("test", input.RootElement.GetProperty("value").GetString());
        }

        var toolResult = Assert.Single(result.ToolResults);
        Assert.True(toolResult.Dynamic);
        Assert.False(toolResult.IsError);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("dynamicTool", toolResult.ToolName);
        using (var input = JsonDocument.Parse(toolResult.InputJson!))
        {
            Assert.Equal("test", input.RootElement.GetProperty("value").GetString());
        }

        using (var output = JsonDocument.Parse(toolResult.OutputJson))
        {
            Assert.Equal("test-result", output.RootElement.GetProperty("value").GetString());
        }
    }
}
