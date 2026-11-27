// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.AlibabaVideoModel;
using static Vercel.AI.Tests.AlibabaProvider.AlibabaProviderVideoFixture;
using Model = Vercel.AI.Alibaba.AlibabaVideoModel;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>videoModel</c>.</summary>
public sealed class VideoModelTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > videoModel::";

    [Fact]
    [UpstreamTest(
        Id + "should construct a video model with correct configuration",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldConstructAVideoModelWithCorrectConfiguration()
    {
        var provider = CreateAlibaba(new AlibabaVideoServer());
        var model = provider.VideoModel("wan2.6-i2v-flash");

        var video = Assert.IsType<Model>(model);
        Assert.Equal("wan2.6-i2v-flash", video.ModelId);
        Assert.Equal("alibaba.video", video.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use the same videoBaseURL as video()",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseTheSameVideoBaseUrlAsVideo()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            CreateUrl("https://custom-video.example.com"),
            AlibabaVideoModelFixture.CreateTaskResponse
        );
        var provider = CreateAlibaba(server, videoBaseUrl: "https://custom-video.example.com");

        await StartAsync(provider.VideoModel("wan2.6-r2v"));

        Assert.Equal(CreateUrl("https://custom-video.example.com"), server.Calls[0].Url);
    }
}
