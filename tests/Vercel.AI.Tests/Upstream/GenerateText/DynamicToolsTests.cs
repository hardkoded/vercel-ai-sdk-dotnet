// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; dynamic tools</c>.</summary>
public sealed class DynamicToolsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > dynamic tools::";

    [Fact]
    [UpstreamTest(Prefix + "should execute dynamic tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_dynamic_tools()
    {
        var toolExecuted = false;
        var model = new Vercel.AI.Testing.TestLanguageModel
        {
            OnGenerate = _ => ToolCallResponse("{ \"value\": \"test\" }", "dynamicTool"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.DynamicTool("dynamicTool", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) =>
                {
                    toolExecuted = true;
                    return Task.FromResult("{\"value\":\"test-result\"}");
                }),
            },
            Prompt = "test-input",
        });

        // tool should be executed by client
        Assert.True(toolExecuted);

        // tool call should be included in content
        Assert.Equal(new[] { "tool-call", "tool-result" }, result.Content.Select(part => part.Type));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.Dynamic);
        Assert.Equal("{\"value\":\"test\"}", Compact(call.ArgumentsJson));
        Assert.False(call.ProviderExecuted);
        Assert.Null(call.ProviderMetadata);
        Assert.Null(call.Title);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("dynamicTool", call.ToolName);
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.True(toolResult.Dynamic);
        Assert.Equal("{\"value\":\"test-result\"}", Compact(toolResult.ResultJson));
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("dynamicTool", toolResult.ToolName);
        var executed = Assert.Single(result.ToolResults);
        Assert.True(executed.Dynamic);
        Assert.Equal("{\"value\":\"test\"}", Compact(executed.InputJson!));
    }
}
