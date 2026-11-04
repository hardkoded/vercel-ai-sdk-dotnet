// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; video</c>.</summary>
public sealed class VideoTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should construct a video model with the video provider id", Coverage = UpstreamCoverage.Covered)]
    public void Should_construct_a_video_model_with_the_video_provider_id()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();
        var model = provider.Video("MiniMax-H3");

        Assert.IsType<MiniMaxVideoModel>(model);
        Assert.Equal("MiniMax-H3", model.ModelId);
        Assert.Equal("minimax.video", model.Config.Provider);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should use the default video base URL, which has no version suffix", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_the_default_video_base_URL_which_has_no_version_suffix()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();

        Assert.Equal("https://api.minimax.io", provider.Video("MiniMax-H3").Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should use a custom videoBaseURL", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_a_custom_videoBaseURL()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { VideoBaseUrl = "https://api.minimaxi.com" });

        Assert.Equal("https://api.minimaxi.com", provider.Video("MiniMax-H3").Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should not derive the video base URL from the chat baseURL", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_derive_the_video_base_URL_from_the_chat_baseURL()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { BaseUrl = "https://custom.url/anthropic/v1" });

        Assert.Equal("https://api.minimax.io", provider.Video("MiniMax-H3").Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should send a bearer token rather than the Anthropic x-api-key header", Coverage = UpstreamCoverage.Partial)]
    public void Should_send_a_bearer_token_rather_than_the_Anthropic_x_api_key_header()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { ApiKey = "test-key" });
        var headers = provider.Video("MiniMax-H3").Config.Headers();

        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Matches("^ai-sdk-minimax/", headers["user-agent"]);
        Assert.False(headers.ContainsKey("x-api-key"));
        Assert.False(headers.ContainsKey("anthropic-version"));
        Assert.Equal("MINIMAX_API_KEY", Vercel.AI.MiniMax.MiniMaxProvider.ApiKeyVariable);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should merge custom headers into the video headers", Coverage = UpstreamCoverage.Covered)]
    public void Should_merge_custom_headers_into_the_video_headers()
    {
        var options = new MiniMaxOptions { ApiKey = "test-key" };
        options.Headers["Custom-Header"] = "value";
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(options);
        var headers = provider.Video("MiniMax-H3").Config.Headers();

        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Equal("value", headers["custom-header"]);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > video::should pass a custom fetch to the video model", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_a_custom_fetch_to_the_video_model()
    {
        using var customClient = new HttpClient();
        var provider = new Vercel.AI.MiniMax.MiniMaxProvider(customClient);

        Assert.Same(customClient, provider.Video("MiniMax-H3").Config.HttpClient);
    }
}
