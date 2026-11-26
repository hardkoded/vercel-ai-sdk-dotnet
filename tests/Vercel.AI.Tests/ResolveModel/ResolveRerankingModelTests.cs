// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveRerankingModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveRerankingModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id
            + "resolveRerankingModel > when a reranking model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Partial,
        Note = "IRerankingModel has no SpecificationVersion, so the v4 assertion is not ported."
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new FakeRerankingModel("test-provider", "test-model-id");

        var resolved = Resolve.ResolveRerankingModel(original, null);

        Assert.Same(original, resolved);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveRerankingModel > when a string is provided and the global default provider is set::should return a reranking model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnARerankingModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddRerankingModel(
            "test-model-id",
            new FakeRerankingModel("global-test-provider", "actual-test-model-id")
        );
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveRerankingModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveRerankingModel > when a string is provided and the provider does not support reranking models::should throw an error",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts that an error is thrown; upstream also asserts its message text."
    )]
    public void ShouldThrowAnError()
    {
        using var scope = new GlobalProviderScope(new ProviderWithoutOptionalModels());

        Assert.Throws<AiSdkException>(() => Resolve.ResolveRerankingModel(null, "test-model-id"));
    }
}
