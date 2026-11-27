// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses > doGenerate > text</c>.</summary>
public sealed class ResponsesDoGenerateTextTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > text::should extract text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("azure-text.1");

        var result = await Generate(capture, Call());

        AssertResultSnapshot(result, capture, "azure-text.1", "azure-text.1");
    }
}
