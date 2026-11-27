// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.AlibabaVideoModel;
using static Vercel.AI.Tests.AlibabaProvider.AlibabaProviderVideoFixture;
using Model = Vercel.AI.Alibaba.AlibabaVideoModel;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>video</c>.</summary>
public sealed class VideoTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > video::";

    [Fact]
    [UpstreamTest(
        Id + "should construct a video model with correct provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldConstructAVideoModelWithCorrectProvider()
    {
        var provider = CreateAlibaba(new AlibabaVideoServer());
        var model = provider.Video("wan2.6-t2v");

        var video = Assert.IsType<Model>(model);
        Assert.Equal("wan2.6-t2v", video.ModelId);
        Assert.Equal("alibaba.video", video.Provider);
    }

    [Fact]
    [UpstreamTest(Id + "should use default videoBaseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseDefaultVideoBaseUrl()
    {
        var server = new AlibabaVideoServer();
        var provider = CreateAlibaba(server);

        await StartAsync(provider.Video("wan2.6-t2v"));

        Assert.Equal(CreateUrl("https://dashscope-intl.aliyuncs.com"), server.Calls[0].Url);
    }

    [Fact]
    [UpstreamTest(Id + "should use custom videoBaseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseCustomVideoBaseUrl()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            CreateUrl("https://dashscope.aliyuncs.com"),
            AlibabaVideoModelFixture.CreateTaskResponse
        );
        var provider = CreateAlibaba(server, videoBaseUrl: "https://dashscope.aliyuncs.com");

        await StartAsync(provider.Video("wan2.6-t2v"));

        Assert.Equal(CreateUrl("https://dashscope.aliyuncs.com"), server.Calls[0].Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass custom fetch to video model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassCustomFetchToVideoModel()
    {
        var customFetch = new AlibabaVideoServer();
        var provider = CreateAlibaba(customFetch);

        await StartAsync(provider.Video("wan2.6-t2v"));

        Assert.Single(customFetch.Calls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers function to video model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassHeadersFunctionToVideoModel()
    {
        var server = new AlibabaVideoServer();
        var provider = CreateAlibaba(
            server,
            headers: new Dictionary<string, string> { ["X-Custom"] = "value" }
        );

        await StartAsync(provider.Video("wan2.6-t2v"));

        var headers = server.Calls[0].Headers;
        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Equal("value", headers["x-custom"]);
    }
}
