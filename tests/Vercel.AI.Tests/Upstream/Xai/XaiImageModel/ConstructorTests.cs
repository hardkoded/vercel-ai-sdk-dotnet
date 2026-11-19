// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Xai.XaiImageModel.XaiImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Port of <c>xai-image-model.test.ts</c> &gt; <c>XaiImageModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/xai/src/xai-image-model.test.ts::XaiImageModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = CreateModel(Server());

        Assert.Equal("xai.image", model.Provider);
        Assert.Equal("grok-imagine-image", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(3, model.MaxImagesPerCall);
        Assert.True(model.SupportsFileInputs);
        Assert.False(model.SupportsMaskInputs);
    }
}
