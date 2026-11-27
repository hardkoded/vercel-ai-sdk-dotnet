// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>tool call</c>.</summary>
public sealed class DoGenerateToolCallTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doGenerate > tool call::should extract tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_tool_calls()
    {
        var options = TestPrompt();
        options.Tools = new[]
        {
            FunctionTool(
                "test-tool",
                "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        var result = await Model(JsonFixture("cohere-tool-call"))
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Collection(
            result.Content,
            part =>
            {
                var call = Assert.IsType<GeneratedToolCall>(part);
                Assert.Equal("weather_dqgshstja6p9", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("{\"location\":\"San Francisco\"}", call.ArgumentsJson);
            },
            part =>
            {
                var call = Assert.IsType<GeneratedToolCall>(part);
                Assert.Equal("cityAttractions_dcxfx4myvx68", call.ToolCallId);
                Assert.Equal("cityAttractions", call.ToolName);
                Assert.Equal("{\"city\":\"San Francisco\"}", call.ArgumentsJson);
            }
        );
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("TOOL_CALL", result.RawFinishReason);
        Assert.Equal(1549, result.Usage.InputTokens);
        Assert.Equal(103, result.Usage.OutputTokens);
        Assert.Equal(992, result.Usage.CacheReadTokens);
        Assert.Equal(557, result.Usage.NoCacheInputTokens);
    }
}
