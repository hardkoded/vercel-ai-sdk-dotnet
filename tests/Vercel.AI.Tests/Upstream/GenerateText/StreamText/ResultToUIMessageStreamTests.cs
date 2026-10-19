// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.toUIMessageStream</c>. Only the test below is ported.</summary>
public sealed class ResultToUIMessageStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toUIMessageStream::should include tool metadata in ui message stream chunks",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_tool_metadata_in_ui_message_stream_chunks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "test-tool", "{ \"value\": \"value\" }"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), "tool-calls"),
            },
        };
        var tool = Tool.DynamicTool("test-tool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}", (_, _) => Task.FromResult("\"result\""));
        tool.Metadata = new Dictionary<string, JsonElement> { ["clientName"] = JsonSerializer.SerializeToElement("MyMCPClient") };
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Tools = new[] { tool },
            Prompt = "test-input",
        });

        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(result);

        var chunk = chunks.Single(item => item!["type"]!.GetValue<string>() == "tool-input-available");
        var expected = JsonNode.Parse("""
            {
              "dynamic": true,
              "input": { "value": "value" },
              "toolCallId": "call-1",
              "toolMetadata": { "clientName": "MyMCPClient" },
              "toolName": "test-tool",
              "type": "tool-input-available"
            }
            """);
        Assert.True(JsonNode.DeepEquals(expected, chunk), chunk!.ToJsonString());
    }
}
