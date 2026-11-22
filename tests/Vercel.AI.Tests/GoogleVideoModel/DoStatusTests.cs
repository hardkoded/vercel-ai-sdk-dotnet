// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVideoModel.GoogleVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVideoModel;

/// <summary>Port of <c>google-video-model.test.ts</c> &gt; <c>GoogleVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/google/src/google-video-model.test.ts::GoogleVideoModel > doStatus::";

    [Fact]
    [UpstreamTest(
        Id + "should return completed with video data when done",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithVideoDataWhenDone()
    {
        var (model, _) = CreateMockModel(
            operationName: "operations/status-test-op",
            videos: ["https://generativelanguage.googleapis.com/files/video-456.mp4"]
        );

        var result = await model.DoStatusAsync(
            StatusCall("operations/status-test-op"),
            CancellationToken.None
        );

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal(
            "https://generativelanguage.googleapis.com/files/video-456.mp4?key=test-api-key",
            video.Url
        );
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Null(video.Data);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending when not done", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingWhenNotDone()
    {
        var testDate = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var (model, _) = CreateFixedModel(
            "{\"name\":\"operations/pending-op\",\"done\":false}",
            () => testDate
        );

        var result = await model.DoStatusAsync(
            StatusCall("operations/pending-op"),
            CancellationToken.None
        );

        Assert.Equal("pending", result.Status);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("veo-3.1-generate-preview", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on operation error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnOperationError()
    {
        var testDate = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var (model, _) = CreateFixedModel(
            "{\"name\":\"operations/error-op\",\"done\":true,\"error\":{\"code\":400,\"message\":\"Content policy violation\",\"status\":\"FAILED_PRECONDITION\"}}",
            () => testDate
        );

        var result = await model.DoStatusAsync(
            StatusCall("operations/error-op"),
            CancellationToken.None
        );

        Assert.Equal("error", result.Status);
        Assert.Contains("Content policy violation", result.Error);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("veo-3.1-generate-preview", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when no videos in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowWhenNoVideosInResponse()
    {
        var (model, _) = CreateFixedModel(
            "{\"name\":\"operations/empty-op\",\"done\":true,\"response\":{\"generateVideoResponse\":{\"generatedSamples\":[]}}}"
        );

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoStatusAsync(StatusCall("operations/empty-op"), CancellationToken.None)
        );

        Assert.Contains("No videos in response", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers to status request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassHeadersToStatusRequest()
    {
        var (model, fetch) = CreateFixedModel(
            "{\"name\":\"operations/headers-test\",\"done\":false}"
        );

        await model.DoStatusAsync(
            StatusCall(
                "operations/headers-test",
                new Dictionary<string, string> { ["x-custom"] = "value" }
            ),
            CancellationToken.None
        );

        var headers = fetch.Requests[0].Headers;
        Assert.Equal("value", headers["x-custom"]);
        Assert.Equal("test-key", headers["x-goog-api-key"]);
    }
}
