// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    [Fact]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > constructor::should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered)]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateModel(CreateServer());

        Assert.Equal("black-forest-labs.video", model.Provider);
        Assert.Equal("flux-3-video", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }
}
