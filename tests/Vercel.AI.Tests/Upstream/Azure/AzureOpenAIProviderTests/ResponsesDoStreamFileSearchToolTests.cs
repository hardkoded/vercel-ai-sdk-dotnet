// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c> &gt; <c>file search tool</c>.</summary>
public sealed class ResponsesDoStreamFileSearchToolTests
{
    private const string Args = "{\"vectorStoreIds\":[\"vs_68caad8bd5d88191ab766cf043d89a18\"]}";

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > file search tool::should stream file search results without results include",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_file_search_results_without_results_include()
    {
        var parts = await Stream(
            ChunksFixture("openai-file-search-tool.1"),
            Call(ProviderTool("openai.file_search", "file_search", Args))
        );

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-file-search-tool.1",
            SequentialIds(),
            includeCitations: true
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > file search tool::should stream file search results with results include",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_file_search_results_with_results_include()
    {
        var parts = await Stream(
            ChunksFixture("openai-file-search-tool.2"),
            Call(
                ProviderTool("openai.file_search", "file_search", Args),
                azureOptions: "{\"include\":[\"file_search_call.results\"]}",
                optionsKey: "openai"
            )
        );

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-file-search-tool.2",
            SequentialIds(),
            includeCitations: true
        );
    }
}
