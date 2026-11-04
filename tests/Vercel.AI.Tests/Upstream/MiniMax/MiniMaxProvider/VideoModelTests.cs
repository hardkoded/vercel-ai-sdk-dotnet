// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; videoModel</c>.</summary>
public sealed class VideoModelTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > videoModel::should construct the same video model as video()", Coverage = UpstreamCoverage.Covered)]
    public void Should_construct_the_same_video_model_as_video()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();
        var model = provider.VideoModel("MiniMax-H3");

        Assert.IsType<MiniMaxVideoModel>(model);
        var minimax = (MiniMaxVideoModel)model;
        Assert.Equal("MiniMax-H3", minimax.ModelId);
        Assert.Equal("minimax.video", minimax.Config.Provider);
        Assert.Equal("https://api.minimax.io", minimax.Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > videoModel::should use the same custom videoBaseURL as video()", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_the_same_custom_videoBaseURL_as_video()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { VideoBaseUrl = "https://custom-video.example.com" });

        var minimax = (MiniMaxVideoModel)provider.VideoModel("MiniMax-H3");
        Assert.Equal("https://custom-video.example.com", minimax.Config.BaseUrl);
    }
}
