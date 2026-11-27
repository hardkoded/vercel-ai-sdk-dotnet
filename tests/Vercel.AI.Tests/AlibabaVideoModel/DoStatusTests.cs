// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStatus::";

    [Fact]
    [UpstreamTest(
        Id + "should encode the task ID as a single URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldEncodeTheTaskIdAsASingleUrlPathSegment()
    {
        var server = new AlibabaVideoServer();
        const string taskId = "abc/../../internal";

        await Status(CreateModel(server), taskId);

        Assert.Equal(
            TestBaseUrl + "/api/v1/tasks/" + Uri.EscapeDataString(taskId),
            server.Calls[0].Url
        );
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [UpstreamTest(
        Id + "should preserve the $taskId task ID as a URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreserveTheTaskIdTaskIdAsAUrlPathSegment(
        string taskId,
        string encodedTaskId
    )
    {
        var server = new AlibabaVideoServer();

        await Status(CreateModel(server), taskId);

        Assert.Equal(TestBaseUrl + "/api/v1/tasks/" + encodedTaskId, server.Calls[0].Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return completed with video data when SUCCEEDED",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithVideoDataWhenSucceeded()
    {
        var result = await Status(CreateModel(new AlibabaVideoServer()), "task-abc-123");

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://dashscope-result.oss.aliyuncs.com/output/video-001.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Null(video.Data);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending when PENDING", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingWhenPending()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            TaskUrl,
            "{\"output\":{\"task_id\":\"task-abc-123\",\"task_status\":\"PENDING\"},\"request_id\":\"req-pending\"}"
        );

        var result = await Status(CreateModel(server), "task-abc-123");

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending when RUNNING", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingWhenRunning()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            TaskUrl,
            "{\"output\":{\"task_id\":\"task-abc-123\",\"task_status\":\"RUNNING\"},\"request_id\":\"req-running\"}"
        );

        var result = await Status(CreateModel(server), "task-abc-123");

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Id + "should return error status on FAILED", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnErrorStatusOnFailed()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            TaskUrl,
            "{\"output\":{\"task_id\":\"task-abc-123\",\"task_status\":\"FAILED\",\"message\":\"Content policy violation\"},\"request_id\":\"req-failed\"}"
        );

        var result = await Status(CreateModel(server), "task-abc-123");

        Assert.Equal("error", result.Status);
        Assert.Matches("(?i)failed", result.Error);
        Assert.Contains("Content policy violation", result.Error);
        Assert.Equal("wan2.6-t2v", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on CANCELED",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnCanceled()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            TaskUrl,
            "{\"output\":{\"task_id\":\"task-abc-123\",\"task_status\":\"CANCELED\"},\"request_id\":\"req-canceled\"}"
        );

        var result = await Status(CreateModel(server), "task-abc-123");

        Assert.Equal("error", result.Status);
        Assert.Matches("(?i)canceled", result.Error);
        Assert.Equal("wan2.6-t2v", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include providerMetadata in completed result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeProviderMetadataInCompletedResult()
    {
        var result = await Status(CreateModel(new AlibabaVideoServer()), "task-abc-123");

        Assert.Equal("completed", result.Status);
        AssertJson(
            "{\"alibaba\":{\"taskId\":\"task-abc-123\",\"videoUrl\":\"https://dashscope-result.oss.aliyuncs.com/output/video-001.mp4\",\"actualPrompt\":\"An enhanced prompt with more cinematic details\",\"usage\":{\"duration\":5.0,\"outputVideoDuration\":5,\"resolution\":1080,\"size\":\"1920x1080\"}}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should surface wan3 usage facts when the task reports them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSurfaceWan3UsageFactsWhenTheTaskReportsThem()
    {
        var server = new AlibabaVideoServer();
        server.Json(
            TaskUrl,
            SucceededTaskResponse.Replace(
                "\"usage\":{\"duration\":5.0,\"output_video_duration\":5,\"SR\":1080,\"size\":\"1920x1080\"}",
                "\"usage\":{\"duration\":12,\"output_video_duration\":8,\"input_video_duration\":4,\"fps\":24,\"SR\":720,\"size\":\"1280x720\",\"ratio\":\"16:9\"}"
            )
        );

        var result = await Status(CreateModel(server, "wan3.0-video"), "task-abc-123");

        Assert.Equal("completed", result.Status);
        AssertJson(
            "{\"duration\":12,\"outputVideoDuration\":8,\"inputVideoDuration\":4,\"fps\":24,\"resolution\":720,\"size\":\"1280x720\",\"ratio\":\"16:9\"}",
            Metadata(result)!["alibaba"]!["usage"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include timestamp, modelId, and headers in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeTimestampModelIdAndHeadersInResponse()
    {
        var testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var model = CreateModel(new AlibabaVideoServer(), currentDate: () => testDate);

        var result = await Status(model, "task-abc-123");

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("wan2.6-t2v", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
        Assert.Null(result.Response.Id);
        Assert.Null(result.Response.Body);
        Assert.Null(result.Response.ProviderMetadata);
    }
}
