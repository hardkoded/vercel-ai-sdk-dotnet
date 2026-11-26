// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveEmbeddingModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveEmbeddingModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id
            + "resolveEmbeddingModel > when an embedding model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new FakeEmbeddingModel("test-provider", "test-model-id");

        var resolved = Resolve.ResolveEmbeddingModel(original, null);

        Assert.Same(original, resolved);
        Assert.Equal("v4", resolved.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveEmbeddingModel > when a string is provided and the global default provider is not set::should return a gateway embedding model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAGatewayEmbeddingModel()
    {
        using var scope = new GlobalProviderScope(null);

        var resolved = Resolve.ResolveEmbeddingModel(null, "test-model-id");

        Assert.Equal("gateway", resolved.Provider);
        Assert.Equal("test-model-id", resolved.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveEmbeddingModel > when a string is provided and the global default provider is set::should return a embedding model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAEmbeddingModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddEmbeddingModel(
            "test-model-id",
            new FakeEmbeddingModel("global-test-provider", "actual-test-model-id")
        );
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveEmbeddingModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }
}
