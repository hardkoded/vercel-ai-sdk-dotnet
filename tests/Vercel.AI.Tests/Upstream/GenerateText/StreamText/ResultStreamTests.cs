// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.stream</c>. Only the tests below are ported.</summary>
public sealed class ResultStreamTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.stream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should refine tool input before tool execution, stream parts, and callbacks",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextOptions has no onLanguageModelCallEnd and the port has no onToolExecutionStart, so the two callback events are not checked. See #623.")]
    public async Task Should_refine_tool_input_before_tool_execution_stream_parts_and_callbacks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \" raw \" }"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), "tool-calls"),
            },
        };
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}", (input, _) =>
                {
                    Assert.Equal("raw", input.GetProperty("value").GetString());
                    return Task.FromResult("\"result:" + input.GetProperty("value").GetString() + "\"");
                }),
            },
            RefineToolInput = new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["tool1"] = (input, _) => Task.FromResult(JsonSerializer.SerializeToElement(new { value = input.GetProperty("value").GetString()!.Trim() })),
            },
            Prompt = "test-input",
        });

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        var call = Assert.Single(parts.OfType<ToolCallPart>()).ToolCall;
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.Equal("{\"value\":\"raw\"}", JsonSerializer.Serialize(JsonDocument.Parse(call.ArgumentsJson).RootElement));
        var toolResult = Assert.Single(parts.OfType<ToolResultPart>()).Result;
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.Equal("{\"value\":\"raw\"}", JsonSerializer.Serialize(JsonDocument.Parse(toolResult.InputJson!).RootElement));
        Assert.Equal("\"result:raw\"", toolResult.OutputJson);
    }
}
