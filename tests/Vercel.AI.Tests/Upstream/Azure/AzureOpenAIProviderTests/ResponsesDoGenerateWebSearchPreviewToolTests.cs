// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> &gt; <c>web search preview tool</c>.</summary>
public sealed class ResponsesDoGenerateWebSearchPreviewToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > web search preview tool::should retain web search preview citation sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_web_search_preview_citation_sources()
    {
        var capture = JsonFixture("azure-web-search-preview-tool.1");

        var result = await Generate(
            capture,
            Call(ProviderTool("openai.web_search_preview", "web_search_preview"))
        );

        Assert.Contains(result.Content, part =>
            part
                is GeneratedSource
                {
                    Url: "https://www.straitstimes.com/world/while-you-were-sleeping-5-stories-you-might-have-missed-nov-19-2025",
                }
        );
        OpenAIResponsesSupport.AssertContentSnapshot(
            result.Content,
            "azure-web-search-preview-tool.1",
            includeCitations: true,
            SequentialIds()
        );
    }
}
