// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; image</c>.</summary>
public sealed class ImageTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > image::should construct an image model through both factory methods", Coverage = UpstreamCoverage.Covered)]
    public void Should_construct_an_image_model_through_both_factory_methods()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();
        foreach (var model in new[] { provider.Image("MiniMax-H3-Image"), (Vercel.AI.MiniMax.MiniMaxImageModel)provider.ImageModel("MiniMax-H3-Image-Highspeed") })
        {
            Assert.Equal("minimax.image", model.Provider);
            Assert.Equal("v4", model.SpecificationVersion);
            Assert.True(model.SupportsFileInputs);
            Assert.False(model.SupportsMaskInputs);
        }
    }
}
