// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; VOD &gt; doStatus</c>.</summary>
public sealed class VodDoStatusTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > VOD > doStatus::";

    private static string Finish(string aigcVideoTask) => """{"Response":{"TaskType":"AigcVideoTask","Status":"FINISH",""" + "\"AigcVideoTask\":" + aigcVideoTask + "}}";

    [Fact]
    [UpstreamTest(Prefix + "returns videos and usage for a finished task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_videos_and_usage_for_a_finished_task()
    {
        var server = new Server();
        server.Respond(VodUrl, """{"Response":{"TaskType":"AigcVideoTask","Status":"FINISH","RequestId":"req-vod-2","AigcVideoTask":{"TaskId":"task-123","Status":"FINISH","ErrCode":0,"Message":"","Progress":100,"Input":{"ModelName":"VS","ModelVersion":"2.0","OutputConfig":{"StorageMode":"Temporary","Duration":8,"Resolution":"720P","AspectRatio":"16:9","AudioGeneration":""}},"Output":{"FileInfos":[{"FileUrl":"https://vod.example.com/out.mp4","FileType":"mp4","FileId":"","MetaData":{"Duration":8.04}}],"Usage":{"InputTokens":0,"ThoughtTokens":0,"InputImageCount":0,"InputSeconds":5,"OutputSeconds":8,"TotalSeconds":13,"SpecificationUsages":[{"Specification":"SV2.0_720P","Category":"aigc_video_sv2_720"}]}}}}}""");

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("DescribeTaskDetail", server.Calls[0].Header("x-tc-action"));
        Body(server, """{"TaskId":"task-123","SubAppId":1500000000}""");
        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://vod.example.com/out.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
        Assert.Empty(result.Warnings);
        Assert.Equal(CurrentDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("vod/VS/2.0", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"tencent":{"taskId":"task-123","requestId":"req-vod-2","resolution":"720P","videos":[{"url":"https://vod.example.com/out.mp4","duration":8.04}],"usage":{"inputImageCount":0,"inputSeconds":5,"inputTokens":0,"outputSeconds":8,"specificationUsages":[{"specification":"SV2.0_720P","category":"aigc_video_sv2_720"}],"thoughtTokens":0,"totalSeconds":13}}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "returns the last-frame image as metadata instead of a video", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_the_last_frame_image_as_metadata_instead_of_a_video()
    {
        var server = new Server();
        server.Respond(VodUrl, Finish("""{"TaskId":"task-123","Status":"FINISH","ErrCode":0,"Output":{"FileInfos":[{"FileUrl":"https://vod.example.com/out.mp4","FileType":"","UsageType":"","MetaData":{"Duration":4.04}},{"FileUrl":"https://vod.example.com/last.png","FileType":"","UsageType":"last_frame_url","MetaData":null}],"Usage":{"OutputSeconds":4}}}"""));

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://vod.example.com/out.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"tencent":{"taskId":"task-123","lastFrameUrl":"https://vod.example.com/last.png","videos":[{"url":"https://vod.example.com/out.mp4","duration":4.04}],"usage":{"outputSeconds":4}}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "skips file entries with an empty URL", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Skips_file_entries_with_an_empty_URL()
    {
        var server = new Server();
        server.Respond(VodUrl, Finish("""{"TaskId":"task-123","Status":"FINISH","ErrCode":0,"Output":{"FileInfos":[{"FileUrl":""},{"FileUrl":"https://vod.example.com/out.mp4"}]}}"""));

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://vod.example.com/out.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits an empty resolution and empty specification usages", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Omits_an_empty_resolution_and_empty_specification_usages()
    {
        var server = new Server();
        server.Respond(VodUrl, Finish("""{"TaskId":"task-123","Status":"FINISH","ErrCode":0,"Input":{"OutputConfig":{"Resolution":""}},"Output":{"FileInfos":[{"FileUrl":"https://vod.example.com/out.mp4"}],"Usage":{"OutputSeconds":4,"SpecificationUsages":[{"Specification":"","Category":""}]}}}"""));

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"tencent":{"taskId":"task-123","videos":[{"url":"https://vod.example.com/out.mp4","duration":4}],"usage":{"outputSeconds":4}}}""");
    }

    [Theory]
    [InlineData("WAITING")]
    [InlineData("PROCESSING")]
    [UpstreamTest(Prefix + "returns pending for status %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_pending_for_status(string status)
    {
        var server = new Server();
        server.Respond(VodUrl, "{\"Response\":{\"TaskType\":\"AigcVideoTask\",\"Status\":\"" + status + "\"}}");

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns an error for a failed task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_an_error_for_a_failed_task()
    {
        var server = new Server();
        server.Respond(VodUrl, Finish("""{"Status":"FINISH","ErrCode":70000,"ErrCodeExt":"ContentModerationFailed","Message":"Content moderation failed."}"""));

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation failed (ContentModerationFailed): Content moderation failed. (task task-123)", result.Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns an error for an aborted task", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns_an_error_for_an_aborted_task()
    {
        var server = new Server();
        server.Respond(VodUrl, """{"Response":{"TaskType":"AigcVideoTask","Status":"ABORTED"}}""");

        var result = await CreateModel(server, "vod/VS/2.0").DoStatusAsync(Status(), CancellationToken.None);

        Assert.Equal("error", result.Status);
    }
}
