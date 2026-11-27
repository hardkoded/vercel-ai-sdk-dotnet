// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; web fetch tool &gt; txt response</c>.</summary>
public sealed class DoStreamWebFetchToolTxtResponseTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > web fetch tool > txt response::";

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
                "anthropic.web_fetch_20250910",
                "web_fetch",
                AnthropicParity.Json("""{"maxUses":1}""")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-web-fetch-tool.1")),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "web-fetch-txt-response-stream");
    }
}
