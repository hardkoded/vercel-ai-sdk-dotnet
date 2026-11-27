// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; tool search tool &gt; deferred result - bm25 variant</c>.</summary>
public sealed class DoGenerateToolSearchToolDeferredResultBm25VariantTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > tool search tool > deferred result - bm25 variant::should correctly map tool_search_tool_result when result comes without server_tool_use in same response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_correctly_map_tool_search_tool_result_when_result_comes_without_server_tool_use_in_same_response()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-tool-search-deferred-bm25.2"),
            Options(
                text: "What is the weather in San Francisco?",
                tools:
                [
                    ProviderTool("anthropic.tool_search_bm25_20251119", "tool_search"),
                    FunctionTool(
                        "get_weather",
                        "Get the current weather at a specific location",
                        """{"type":"object","properties":{"location":{"type":"string"}}}""",
                        """{"anthropic":{"deferLoading":true}}"""
                    ),
                ]
            ),
            "claude-sonnet-4-5"
        );

        // The tool result should be correctly mapped to 'tool_search' (the user's custom name)
        // even though serverToolCalls map is empty (no server_tool_use in this response)
        var toolResult = result.Content.FirstOrDefault(part => part.Type == "tool-result");
        Assert.NotNull(toolResult);
        Assert.Equal("tool_search", ToolName(toolResult));
    }

    private static string ToolName(GeneratedContent content)
    {
        return (string)ContentJson([content]).AsArray()[0]!["toolName"]!;
    }
}
