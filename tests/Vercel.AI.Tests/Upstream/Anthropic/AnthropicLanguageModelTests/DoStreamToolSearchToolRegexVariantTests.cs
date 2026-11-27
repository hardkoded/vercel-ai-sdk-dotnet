// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; tool search tool &gt; regex variant</c>.</summary>
public sealed class DoStreamToolSearchToolRegexVariantTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > tool search tool > regex variant::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream tool search regex results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_search_regex_results()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Find out weather data in SF") },
            Tools = new[]
            {
                LanguageModelTool.ProviderDefined(
                    "anthropic.tool_search_regex_20251119",
                    "tool_search",
                    AnthropicParity.Json("""{}""")
                ),
                new LanguageModelTool(
                    "get_temp_data",
                    "For a location",
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
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-tool-search-regex.1")
            ),
            "claude-sonnet-4-5",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "tool-search-regex-stream");
    }
}
