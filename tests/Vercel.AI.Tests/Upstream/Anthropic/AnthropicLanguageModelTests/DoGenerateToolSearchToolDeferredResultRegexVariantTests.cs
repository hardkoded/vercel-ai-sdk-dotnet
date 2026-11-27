// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; tool search tool &gt; deferred result - regex variant</c>.</summary>
public sealed class DoGenerateToolSearchToolDeferredResultRegexVariantTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > tool search tool > deferred result - regex variant::should correctly map tool_search_tool_result when result comes without server_tool_use in same response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_correctly_map_tool_search_tool_result_when_result_comes_without_server_tool_use_in_same_response()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-tool-search-deferred-regex.2"),
            Options(
                text: "Find weather data in NYC",
                tools:
                [
                    ProviderTool("anthropic.tool_search_regex_20251119", "tool_search"),
                    FunctionTool(
                        "get_temp_data",
                        "For a location",
                        """{"type":"object","properties":{"location":{"type":"string"}}}""",
                        """{"anthropic":{"deferLoading":true}}"""
                    ),
                ]
            ),
            "claude-sonnet-4-5"
        );

        var toolResult = result.Content.FirstOrDefault(part => part.Type == "tool-result");
        Assert.NotNull(toolResult);
        Assert.Equal("tool_search", ToolName(toolResult));
    }

    private static string ToolName(GeneratedContent content)
    {
        return (string)ContentJson([content]).AsArray()[0]!["toolName"]!;
    }
}
