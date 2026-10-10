// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; web search tool</c>.</summary>
public sealed class DoGenerateWebSearchToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > web search tool::should expose visited URLs as sources and keep citations in text metadata",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_expose_visited_URLs_as_sources_and_keep_citations_in_text_metadata()
    {
        var result = OpenAIResponsesSupport.Generate(OpenAIResponsesSupport.FixtureBody("openai-web-search-tool.1.json"));

        var sources = result.Content.OfType<GeneratedSource>().ToList();
        var textParts = result.Content.OfType<GeneratedText>().ToList();
        var citations = textParts.SelectMany(part => part.Citations ?? Array.Empty<Citation>()).ToList();

        Assert.Equal(10, citations.Count);
        Assert.Contains(citations, citation =>
            citation.Source is GeneratedSource { Title: not null }
            && citation.StartIndex != null
            && citation.EndIndex != null);
        Assert.Equal(16, sources.Count);
        Assert.Contains(
            "https://www.investing.com/news/stock-market-news/ai-coding-startup-vercel-raises-300-million-valued-at-93-billion-4264199",
            sources.Select(source => source.Url));
        Assert.DoesNotContain(
            "https://www.investopedia.com/5-things-to-know-before-the-stock-market-opens-december-5-2025-11862701?utm_source=openai",
            sources.Select(source => source.Url));
        Assert.Equal(
            10,
            textParts
                .OfType<OpenAIText>()
                .Sum(part => part.ProviderMetadata.GetProperty("openai").TryGetProperty("annotations", out var annotations) ? annotations.GetArrayLength() : 0));
    }
}
