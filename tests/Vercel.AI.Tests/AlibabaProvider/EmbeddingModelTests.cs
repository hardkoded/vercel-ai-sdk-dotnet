// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using AlibabaEmbeddingModelType = Vercel.AI.Alibaba.AlibabaEmbeddingModel;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>embeddingModel</c>.</summary>
public sealed class EmbeddingModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > embeddingModel::should construct an embedding model with correct configuration",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_an_embedding_model_with_correct_configuration()
    {
        var provider = AlibabaProviderType.Create();
        var model = provider.EmbeddingModel("text-embedding-v3");

        var embedding = Assert.IsType<AlibabaEmbeddingModelType>(model);
        Assert.Equal("text-embedding-v3", embedding.ModelId);
        Assert.Equal("alibaba.embedding", embedding.Config.Provider);
    }
}
