// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Operations;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>embedding</c>.</summary>
public sealed class EmbeddingTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::embedding::should expose the aggregate token limit",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_the_aggregate_token_limit()
    {
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }
        );

        Assert.Equal(
            300_000,
            (
                (Vercel.AI.OpenAICompatible.OpenAICompatibleEmbeddingModel)
                    provider.EmbeddingModel("my-embedding")
            ).MaxInputBytesPerCall
        );
    }
}
