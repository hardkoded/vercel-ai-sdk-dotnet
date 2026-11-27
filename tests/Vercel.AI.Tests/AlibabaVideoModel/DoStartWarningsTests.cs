// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c> &gt; <c>warnings</c>.</summary>
public sealed class DoStartWarningsTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart > warnings::";

    [Fact]
    [UpstreamTest(
        Id + "should warn about unsupported aspectRatio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnAboutUnsupportedAspectRatio()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call(aspectRatio: "16:9"));

        Assert.True(HasWarning(result.Warnings, "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about unsupported fps", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutUnsupportedFps()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call(fps: 30));

        Assert.True(HasWarning(result.Warnings, "fps"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn when n > 1", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnWhenNGreaterThan1()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call(n: 3));

        Assert.True(HasWarning(result.Warnings, "n"));
    }

    [Fact]
    [UpstreamTest(Id + "should not warn when n is 1", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnWhenNIs1()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call(n: 1));

        Assert.False(HasFeature(result.Warnings, "n"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should return empty warnings for supported features",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnEmptyWarningsForSupportedFeatures()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call());

        Assert.Empty(result.Warnings);
    }
}
