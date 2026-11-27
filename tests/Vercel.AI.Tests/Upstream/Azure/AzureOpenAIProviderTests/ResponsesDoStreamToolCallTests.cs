// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c> &gt; <c>tool call</c>.</summary>
public sealed class ResponsesDoStreamToolCallTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream > tool call::should stream tool call content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_call_content()
    {
        var parts = await Stream(ChunksFixture("azure-tool-call.1"), Call());

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-tool-call.1",
            SequentialIds(),
            includeCitations: true
        );
    }
}
