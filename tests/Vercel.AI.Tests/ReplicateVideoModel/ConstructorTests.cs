// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ReplicateVideoModel.ReplicateVideoModelFixture;

namespace Vercel.AI.Tests.ReplicateVideoModel;

/// <summary>Port of <c>replicate-video-model.test.ts</c> &gt; <c>ReplicateVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/replicate/src/replicate-video-model.test.ts::ReplicateVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var (model, _) = CreateModel();

        Assert.Equal("replicate.video", model.Provider);
        Assert.Equal("minimax/video-01", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(
        Id + "should support model IDs with versions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldSupportModelIDsWithVersions()
    {
        var (model, _) = CreateModel(modelId: "stability-ai/stable-video-diffusion:abc123");

        Assert.Equal("stability-ai/stable-video-diffusion:abc123", model.ModelId);
    }
}
