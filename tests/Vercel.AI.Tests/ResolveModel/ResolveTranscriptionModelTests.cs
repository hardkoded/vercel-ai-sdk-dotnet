// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using Resolve = Vercel.AI.ResolveModel;

namespace Vercel.AI.Tests.ResolveModel;

/// <summary>Port of <c>resolve-model.test.ts</c> &gt; <c>resolveTranscriptionModel</c>.</summary>
[Collection(GlobalProviderCollection.Name)]
public sealed class ResolveTranscriptionModelTests
{
    private const string Id = "packages/ai/src/model/resolve-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "resolveTranscriptionModel > when a transcription model v4 is provided::should return it as-is",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnItAsIs()
    {
        var original = new FakeTranscriptionModel("test-provider", "test-model-id");

        var resolved = Resolve.ResolveTranscriptionModel(original, null);

        Assert.Same(original, resolved);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolveTranscriptionModel > when a string is provided and the global default provider is set::should return a transcription model from the global default provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldReturnATranscriptionModelFromTheGlobalDefaultProvider()
    {
        var provider = new CustomProvider().AddTranscriptionModel("test-model-id", new FakeTranscriptionModel("global-test-provider", "actual-test-model-id"));
        using var scope = new GlobalProviderScope(provider);

        var resolved = Resolve.ResolveTranscriptionModel(null, "test-model-id");

        Assert.Equal("global-test-provider", resolved.Provider);
        Assert.Equal("actual-test-model-id", resolved.ModelId);
    }
}
