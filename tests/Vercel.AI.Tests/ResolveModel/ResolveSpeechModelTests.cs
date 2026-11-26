// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveSpeechModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveSpeechModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "resolveSpeechModel > when a speech model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new FakeSpeechModel("test-provider", "test-model-id");

        var resolved = Resolve.ResolveSpeechModel(original, null);

        Assert.Same(original, resolved);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "resolveSpeechModel > when a string is provided and the global default provider is set::should return a speech model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnASpeechModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddSpeechModel(
            "test-model-id",
            new FakeSpeechModel("global-test-provider", "actual-test-model-id")
        );
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveSpeechModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }
}
