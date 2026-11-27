// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c> &gt; <c>text</c>.</summary>
public sealed class ResponsesDoStreamTextTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > text::should stream text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_text_content()
    {
        var parts = await Stream(ChunksFixture("azure-text.1"), Call());

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-text.1",
            SequentialIds(),
            includeCitations: true
        );
    }
}
