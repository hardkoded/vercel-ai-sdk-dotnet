// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveImageModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveImageModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id
            + "resolveImageModel > when a string is provided and the global default provider is not set::should return a gateway image model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAGatewayImageModel()
    {
        using var scope = new GlobalProviderScope(null);

        var resolved = Resolve.ResolveImageModel(null, "test-model-id");

        Assert.Equal("gateway", resolved.Provider);
        Assert.Equal("test-model-id", resolved.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveImageModel > when a string is provided and the global default provider is set::should return an image model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAnImageModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddImageModel(
            "test-model-id",
            new FakeImageModel("global-test-provider", "actual-test-model-id")
        );
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveImageModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }
}
