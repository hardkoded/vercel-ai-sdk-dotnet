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

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses > doGenerate > tool call</c>.</summary>
public sealed class ResponsesDoGenerateToolCallTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > tool call::should extract tool call content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_tool_call_content()
    {
        var capture = JsonFixture("azure-tool-call.1");

        var result = await Generate(capture, Call());

        AssertResultSnapshot(result, capture, "azure-tool-call.1", "azure-tool-call.1");
    }
}
