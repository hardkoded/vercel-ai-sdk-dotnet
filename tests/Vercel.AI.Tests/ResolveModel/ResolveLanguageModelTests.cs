// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveLanguageModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveLanguageModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "resolveLanguageModel > when a language model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new Vercel.AI.Testing.TestLanguageModel("test-model-id");

        var resolved = Resolve.ResolveLanguageModel(original, null);

        Assert.Same(original, resolved);
        Assert.Equal("v4", resolved.SpecificationVersion, ignoreCase: true);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveLanguageModel > when a string is provided and the global default provider is not set::should return a gateway language model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnAGatewayLanguageModel()
    {
        using var scope = new GlobalProviderScope(null);

        var resolved = Resolve.ResolveLanguageModel(null, "test-model-id");

        Assert.Equal("gateway", resolved.Provider);
        Assert.Equal("test-model-id", resolved.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveLanguageModel > when a string is provided and the global default provider is set::should return a language model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnALanguageModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddLanguageModel("test-model-id", new Vercel.AI.Testing.TestLanguageModel("actual-test-model-id"));
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveLanguageModel(null, "test-model-id");

        Assert.Equal("test", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }
}
