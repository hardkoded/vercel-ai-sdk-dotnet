// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; tool search tool &gt; bm25 variant</c>.</summary>
public sealed class DoStreamToolSearchToolBm25VariantTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > tool search tool > bm25 variant::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream tool search bm25 results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_search_bm25_results()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("What is the weather in San Francisco?"),
            },
            Tools = new[]
            {
                LanguageModelTool.ProviderDefined(
                    "anthropic.tool_search_bm25_20251119",
                    "tool_search",
                    AnthropicParity.Json("""{}""")
                ),
                new LanguageModelTool(
                    "get_weather",
                    "Get the current weather at a specific location",
                    AnthropicParity.Json(
                        """{"type":"object","properties":{"location":{"type":"string"},"unit":{"type":"string","enum":["celsius","fahrenheit"]}}}"""
                    )
                )
                {
                    ProviderOptions = AnthropicParity.Json(
                        """{"anthropic":{"deferLoading":true}}"""
                    ),
                },
            },
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-tool-search-bm25.1")),
            "claude-sonnet-4-5",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "tool-search-bm25-stream");
    }
}
