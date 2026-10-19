// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsImageModel.BlackForestLabsImageModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>Port of <c>black-forest-labs-image-model.test.ts</c> &gt; <c>BlackForestLabsImageModel</c> &gt; <c>capabilities</c>.</summary>
public sealed class CapabilitiesTests
{
    [Theory]
    [InlineData("flux-3-image", true, false)]
    [InlineData("flux-3-image-klein", true, false)]
    [InlineData("flux-pro-1.0-fill", true, true)]
    [InlineData("flux-kontext-pro", true, false)]
    [InlineData("flux-kontext-max", true, false)]
    [InlineData("flux-pro-1.1", false, false)]
    [InlineData("flux-pro-1.1-ultra", false, false)]
    [InlineData("flux-2-pro", null, null)]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > capabilities::advertises file=$supportsFileInputs and mask=$supportsMaskInputs for $modelId",
        Coverage = UpstreamCoverage.Covered)]
    public void AdvertisesFileAndMaskForModelId(string modelId, bool? supportsFileInputs, bool? supportsMaskInputs)
    {
        var model = CreateBasicModel(new BlackForestLabsServer(), modelId);

        Assert.Equal(supportsFileInputs, model.SupportsFileInputs);
        Assert.Equal(supportsMaskInputs, model.SupportsMaskInputs);
    }
}
