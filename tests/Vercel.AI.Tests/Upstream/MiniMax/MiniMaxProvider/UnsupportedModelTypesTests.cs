// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; unsupported model types</c>.</summary>
public sealed class UnsupportedModelTypesTests
{
    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > unsupported model types::should throw NoSuchModelError for embeddingModel",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_NoSuchModelError_for_embeddingModel()
    {
        var error = Assert.Throws<AiSdkException>(() =>
            Vercel
                .AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { ApiKey = "secret" })
                .EmbeddingModel("embed")
        );
        Assert.Equal("Provider 'minimax' does not support embedding model 'embed'.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > unsupported model types::should throw NoSuchModelError for textEmbeddingModel",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no textEmbeddingModel alias, so EmbeddingModel is the only entry point checked."
    )]
    public void Should_throw_NoSuchModelError_for_textEmbeddingModel()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();

        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("foo"));
    }
}
