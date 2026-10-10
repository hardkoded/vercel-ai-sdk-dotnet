// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web search tool &gt; with fixture</c>.</summary>
public sealed class DoGenerateWebSearchToolWithFixtureTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > web search tool > with fixture::should expose search results as sources and keep citations in text metadata",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_expose_search_results_as_sources_and_keep_citations_in_text_metadata()
    {
        var result = AnthropicResponse.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "anthropic-web-search-tool.1.json")),
            new AnthropicParseContext(false, "anthropic", false));

        var sources = result.Content.OfType<GeneratedSource>().ToList();
        var textParts = result.Content.OfType<GeneratedText>().ToList();

        Assert.Equal(3, textParts.SelectMany(part => part.Citations ?? Array.Empty<Citation>()).Count());
        Assert.Equal(10, sources.Count);
        Assert.Equal(10, sources.Select(source => source.Url).Distinct().Count());
        Assert.Equal(
            3,
            textParts
                .OfType<AnthropicText>()
                .Sum(part => part.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("citations").GetArrayLength()));
    }
}
