// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c> &gt; <c>image generation tool</c>.</summary>
public sealed class ResponsesDoStreamImageGenerationToolTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > image generation tool::should stream image generation tool results include",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_image_generation_tool_results_include()
    {
        var parts = await Stream(
            ChunksFixture("azure-image-generation-tool.1"),
            Call(ProviderTool("openai.image_generation", "image_generation"))
        );

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-image-generation-tool.1",
            SequentialIds(),
            includeCitations: true
        );
    }
}
