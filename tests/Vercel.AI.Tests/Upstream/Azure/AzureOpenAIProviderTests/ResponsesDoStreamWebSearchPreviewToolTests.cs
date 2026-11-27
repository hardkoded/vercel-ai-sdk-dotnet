// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c> &gt; <c>web search preview tool</c>.</summary>
public sealed class ResponsesDoStreamWebSearchPreviewToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > web search preview tool::should retain streamed web search preview citation sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_streamed_web_search_preview_citation_sources()
    {
        var events = await Stream(
            ChunksFixture("azure-web-search-preview-tool.1"),
            Call(ProviderTool("openai.web_search_preview", "web_search_preview"))
        );

        Assert.Contains(events, part =>
            part
                is SourceStreamPart
                {
                    Url: "https://www.straitstimes.com/world/while-you-were-sleeping-5-stories-you-might-have-missed-nov-19-2025",
                }
        );
        OpenAIResponsesSupport.AssertStreamSnapshot(
            events,
            "azure-web-search-preview-tool.1",
            SequentialIds(),
            includeCitations: true
        );
    }
}
