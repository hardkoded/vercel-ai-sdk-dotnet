// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>provider-executed tools</c> &gt; <c>provider-executed tool error with structured object preserves error in ui message stream</c>.</summary>
public sealed class ProviderExecutedToolErrorWithStructuredObjectPreservesErrorInUiMessageStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > provider-executed tools > provider-executed tool error with structured object preserves error in ui message stream::should preserve structured error despite custom onError for provider-executed tools",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_structured_error_despite_custom_onError_for_provider_executed_tools()
    {
        var args = JsonDocument.Parse("{}").RootElement;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "web_fetch", "{ \"url\": \"https://example.com\" }") { ProviderExecuted = true },
                new ToolResultStreamPart(
                    "call-1",
                    "web_fetch",
                    JsonDocument.Parse("{\"type\":\"web_fetch_tool_result_error\",\"errorCode\":\"url_not_accessible\"}").RootElement.Clone())
                {
                    IsError = true,
                },
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.ProviderDefined("test.web_fetch", "web_fetch", args) },
            StopWhen = StopWhen.IsStepCount(4),
        });

        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(result, _ => "Oops, an error occurred!");

        var errorChunk = chunks.Single(chunk => (string?)chunk!["type"] == "tool-output-error")!;
        var parsed = JsonNode.Parse((string)errorChunk["errorText"]!);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"web_fetch_tool_result_error\",\"errorCode\":\"url_not_accessible\"}"),
            parsed));
    }
}
