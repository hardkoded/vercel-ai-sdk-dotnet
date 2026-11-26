// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveVideoModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveVideoModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "resolveVideoModel > when a video model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new FakeVideoModel("test-provider", "test-model-id");

        var resolved = Resolve.ResolveVideoModel(original, null);

        Assert.Same(original, resolved);
        Assert.Equal("v4", resolved.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveVideoModel > when a string is provided and the global default provider is set::should return a video model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAVideoModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddVideoModel("test-model-id", new FakeVideoModel("global-test-provider", "actual-test-model-id"));
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveVideoModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveVideoModel > when a string is provided and the provider does not support video models::should throw an error",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts that an error is thrown; upstream also asserts its message text."
    )]
    public void ShouldThrowAnError()
    {
        using var scope = new GlobalProviderScope(new CustomProvider());

        Assert.ThrowsAny<Exception>(() => Resolve.ResolveVideoModel(null, "test-model-id"));
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveVideoModel > when a model with unsupported specification version is provided::should throw UnsupportedModelVersionError",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldThrowUnsupportedModelVersionError()
    {
        var model = new FakeVideoModel("test-provider", "test-model-id", "v1");

        Assert.Throws<UnsupportedModelVersionException>(() => Resolve.ResolveVideoModel(model, null));
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveVideoModel > when a model with unsupported specification version is provided::should throw UnsupportedModelVersionError for v2 models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldThrowUnsupportedModelVersionErrorForV2Models()
    {
        var model = new FakeVideoModel("test-provider", "test-model-id", "v2");

        Assert.Throws<UnsupportedModelVersionException>(() => Resolve.ResolveVideoModel(model, null));
    }
}
