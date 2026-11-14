// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiImageModel;

/// <summary>Port of <c>azure-mai-image-model.test.ts</c> &gt; <c>dimensionsForAspectRatio</c>.</summary>
public sealed class DimensionsForAspectRatioTests
{
    [Theory]
    [InlineData("1:1", 1024, 1024)]
    [InlineData("16:9", 1360, 768)]
    [InlineData("9:16", 768, 1360)]
    [InlineData("4:3", 1168, 880)]
    [InlineData("21:9", 1792, 768)]
    [InlineData("0:1", null, null)]
    [InlineData("wide", null, null)]
    [UpstreamTest("packages/azure/src/azure-mai-image-model.test.ts::dimensionsForAspectRatio::maps %s", Coverage = UpstreamCoverage.Covered)]
    public void Maps(string aspectRatio, int? width, int? height)
    {
        var dimensions = Vercel.AI.Azure.AzureMaiImageModel.DimensionsForAspectRatio(aspectRatio);

        if (width == null)
        {
            Assert.Null(dimensions);
        }
        else
        {
            Assert.Equal((width.Value, height!.Value), dimensions);
        }
    }
}
