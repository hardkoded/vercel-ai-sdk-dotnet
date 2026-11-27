// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateModel(new AlibabaVideoServer());

        Assert.Equal("alibaba.video", model.Provider);
        Assert.Equal("wan2.6-t2v", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Id + "should accept custom model IDs", Coverage = UpstreamCoverage.Covered)]
    public void ShouldAcceptCustomModelIds()
    {
        var model = CreateModel(new AlibabaVideoServer(), modelId: "wan2.6-i2v-flash");

        Assert.Equal("wan2.6-i2v-flash", model.ModelId);
    }
}
