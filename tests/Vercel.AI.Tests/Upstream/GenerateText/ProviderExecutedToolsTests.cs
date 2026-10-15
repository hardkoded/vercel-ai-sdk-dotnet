// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class ProviderExecutedToolsTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > provider-executed tools::should not call execute for provider-executed tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_call_execute_for_provider_executed_tool_calls()
    {
        var toolExecuted = false;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("call-1", "providerTool", "{ \"value\": \"test\" }") { ProviderExecuted = true },
                    new GeneratedToolResult("call-1", "providerTool", "{\"example\":\"example\"}"),
                },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
        };
        var result = await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function("providerTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}", (_, _) =>
                {
                    toolExecuted = true;
                    return Task.FromResult("should-not-execute");
                }),
            },
        });

        Assert.False(toolExecuted);
        Assert.Equal(new[] { "tool-call", "tool-result" }, result.Content.Select(part => part.Type).ToArray());
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("{\"example\":\"example\"}", Assert.IsType<GeneratedToolResult>(result.Content[1]).ResultJson);
    }
}
