// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.GoogleVideoModel.GoogleVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVideoModel;

/// <summary>Port of <c>google-video-model.test.ts</c> &gt; <c>GoogleVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/google/src/google-video-model.test.ts::GoogleVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var (model, _) = CreateMockModel();

        Assert.Equal("google.generative-ai", model.Provider);
        Assert.Equal("veo-3.1-generate-preview", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(4, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Id + "should support different model IDs", Coverage = UpstreamCoverage.Covered)]
    public void ShouldSupportDifferentModelIds()
    {
        var (model, _) = CreateMockModel(modelId: "veo-3.1-generate");

        Assert.Equal("veo-3.1-generate", model.ModelId);
    }
}
