// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; doStatus</c>.</summary>
public sealed class TokenHubKlingDoStatusTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > doStatus::";

    private const string StatusUrl = TokenHubBaseUrl + "/wand/kling/tasks/" + TaskId;

    [Fact]
    [UpstreamTest(Prefix + "returns videos and usage for a succeeded task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_videos_and_usage_for_a_succeeded_task()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v3").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal(HttpMethod.Get, server.Calls[0].Method);
        Assert.Equal("Bearer test-key", server.Calls[0].Header("authorization"));
        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://cdn.example.com/video.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
        Assert.Empty(result.Warnings);
        Assert.Equal(CurrentDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("kling-video-v3", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"tencent":{"taskId":"task-123","requestId":"req-2","videos":[{"id":"video-1","url":"https://cdn.example.com/video.mp4","duration":5.041}],"usage":{"totalTokens":12345}}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts a single task object in data", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Accepts_a_single_task_object_in_data()
    {
        var server = new Server();
        server.Respond(StatusUrl, """{"code":0,"data":{"id":"task-123","status":"succeeded","outputs":[{"url":"https://cdn.example.com/video.mp4"}]}}""");

        var result = await CreateModel(server, "kling-video-v3").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns pending for a processing task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_pending_for_a_processing_task()
    {
        var server = new Server();
        server.Respond(StatusUrl, """{"code":0,"data":[{"id":"task-123","status":"processing"}]}""");

        var result = await CreateModel(server, "kling-video-v3").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns an error for a failed task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_an_error_for_a_failed_task()
    {
        var server = new Server();
        server.Respond(StatusUrl, """{"code":0,"data":[{"id":"task-123","status":"failed","message":"risk control"}]}""");

        var result = await CreateModel(server, "kling-video-v3").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation failed: risk control (task task-123)", result.Error);
    }
}
