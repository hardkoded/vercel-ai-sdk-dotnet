// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentProvider.TencentProviderSupport;
using TencentProviderType = Vercel.AI.Tencent.TencentProvider;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentProvider;

/// <summary>Port of <c>tencent-provider.test.ts</c> &gt; <c>createTencent</c> &gt; <c>video models</c>.</summary>
[Collection(TencentEnvironmentCollection.Name)]
public sealed class VideoModelsTests : IDisposable
{
    private const string Prefix = UpstreamFile + "::createTencent > video models::";

    private readonly TencentEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "creates video models", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Creates_video_models()
    {
        var provider = TencentProviderType.Create();

        Assert.IsType<Vercel.AI.Tencent.TencentVideoModel>(provider.Video("kling-video-v3"));
        Assert.IsType<Vercel.AI.Tencent.TencentVideoModel>(provider.VideoModel("vod/VS/2.0"));
        Assert.Equal("tencent.video", provider.Video("kling-video-v3").Provider);
    }

    [Fact]
    [UpstreamTest(Prefix + "throws NoSuchModelError for unsupported video model IDs", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Throws_NoSuchModelError_for_unsupported_video_model_IDs()
    {
        Assert.Throws<NoSuchModelError>(() => TencentProviderType.Create().Video("not-a-model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "signs VOD requests with configured credentials", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Signs_VOD_requests_with_configured_credentials()
    {
        var server = Server();
        var options = new TencentOptions { SecretId = "AKIDEXAMPLE", SecretKey = "EXAMPLESECRET", SubAppId = "1500000000", Region = "ap-singapore" };
        options.Headers["x-custom"] = "value";

        await StartVideo(Create(options, server).Video("vod/VS/2.0"));

        var call = server.Calls[0];
        Assert.Matches(new Regex(@"^TC3-HMAC-SHA256 Credential=AKIDEXAMPLE/"), call.Header("authorization")!);
        Assert.Equal("ap-singapore", call.Header("x-tc-region"));
        Assert.Equal("value", call.Header("x-custom"));
        Assert.Contains("ai-sdk/tencent/", call.Header("user-agent"));
        Assert.Equal(1500000000L, System.Text.Json.Nodes.JsonNode.Parse(call.Text)!["SubAppId"]!.GetValue<long>());
    }

    [Fact]
    [UpstreamTest(Prefix + "reads VOD credentials from the environment", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Reads_VOD_credentials_from_the_environment()
    {
        _env.Stub("TENCENTCLOUD_SECRET_ID", "AKIDENV");
        _env.Stub("TENCENTCLOUD_SECRET_KEY", "ENVSECRET");
        _env.Stub("TENCENTCLOUD_VOD_SUB_APP_ID", "1500000001");
        var server = Server();

        await StartVideo(Create(new TencentOptions(), server).Video("vod/VS/2.0"));

        Assert.Matches(new Regex(@"^TC3-HMAC-SHA256 Credential=AKIDENV/"), server.Calls[0].Header("authorization")!);
        Assert.Equal(1500000001L, System.Text.Json.Nodes.JsonNode.Parse(server.Calls[0].Text)!["SubAppId"]!.GetValue<long>());
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects an invalid SubAppId", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Rejects_an_invalid_SubAppId()
    {
        var options = new TencentOptions { SecretId = "AKIDEXAMPLE", SecretKey = "EXAMPLESECRET", SubAppId = "not-a-number" };

        var error = await Assert.ThrowsAsync<AiSdkException>(() => StartVideo(Create(options, Server()).Video("vod/VS/2.0")));

        Assert.Equal("Tencent VOD SubAppId must be a positive integer.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not need VOD credentials for TokenHub video models", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Does_not_need_VOD_credentials_for_TokenHub_video_models()
    {
        var exception = Record.Exception(() => TencentProviderType.Create(new TencentOptions { ApiKey = "test-key" }).Video("kling-video-v3"));

        Assert.Null(exception);
    }
}
