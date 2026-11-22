// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.FalVideoModel.FalVideoModelFixture;

namespace Vercel.AI.Tests.FalVideoModel;

/// <summary>Port of <c>fal-video-model.test.ts</c> &gt; <c>FalVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/fal/src/fal-video-model.test.ts::FalVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateBasicModel(new FalVideoServer());

        Assert.Equal("fal.video", model.Provider);
        Assert.Equal("luma-dream-machine", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Id + "should support different model IDs", Coverage = UpstreamCoverage.Covered)]
    public void ShouldSupportDifferentModelIds()
    {
        var model = CreateBasicModel(new FalVideoServer(), modelId: "luma-ray-2");

        Assert.Equal("luma-ray-2", model.ModelId);
    }
}
