// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsImageModel.BlackForestLabsImageModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>Port of <c>black-forest-labs-image-model.test.ts</c> &gt; <c>BlackForestLabsImageModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    [Fact]
    [UpstreamTest(
        "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > constructor::exposes correct provider and model information",
        Coverage = UpstreamCoverage.Covered)]
    public void ExposesCorrectProviderAndModelInformation()
    {
        var model = CreateBasicModel(new BlackForestLabsServer());

        Assert.Equal("black-forest-labs.image", model.Provider);
        Assert.Equal("flux-2-pro", model.ModelId);
        Assert.Equal("V4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxImagesPerCall);
    }
}
