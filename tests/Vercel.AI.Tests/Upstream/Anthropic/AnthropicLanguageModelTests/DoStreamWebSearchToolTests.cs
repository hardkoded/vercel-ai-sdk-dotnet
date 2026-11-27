// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; web search tool</c>.</summary>
public sealed class DoStreamWebSearchToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > web search tool::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream web search tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_web_search_tool_results()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.web_search_20250305",
                "web_search",
                AnthropicParity.Json(
                    """{"maxUses":1,"userLocation":{"type":"approximate","country":"US"}}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-web-search-tool.1")),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "web-search-stream");
    }
}
