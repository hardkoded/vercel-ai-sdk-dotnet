// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVertexVideoModel.GoogleVertexVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

/// <summary>Port of <c>google-vertex-video-model.test.ts</c> &gt; <c>GoogleVertexVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/google-vertex/src/google-vertex-video-model.test.ts::GoogleVertexVideoModel > doStatus::";

    [Fact]
    [UpstreamTest(
        Id + "should return completed with video data when done",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithVideoDataWhenDone()
    {
        var server = new VertexVideoServer { OperationName = "operations/my-op-123" };

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Operation("operations/my-op-123")), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("base64", video.Type);
        Assert.Equal("base64-video-data", video.Data);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Null(video.Url);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending when not done", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingWhenNotDone()
    {
        var server = new VertexVideoServer
        {
            OperationName = "operations/my-op-123",
            PollsUntilDone = 3, // Never completes on first poll
        };

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Operation("operations/my-op-123")), CancellationToken.None);

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on operation error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnOperationError()
    {
        var server = new VertexVideoServer
        {
            OperationName = "operations/my-op-123",
            OperationError =
                "{\"code\":400,\"message\":\"Content policy violation\",\"status\":\"FAILED_PRECONDITION\"}",
        };

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Operation("operations/my-op-123")), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Contains("Content policy violation", result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal("veo-2.0-generate-001", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use POST with operationName in body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUsePOSTWithOperationNameInBody()
    {
        var server = new VertexVideoServer
        {
            Respond = url =>
                url.Contains(":fetchPredictOperation")
                    ? "{\"name\":\"operations/check-post\",\"done\":true,\"response\":{\"videos\":[{\"bytesBase64Encoded\":\"video-data\",\"mimeType\":\"video/mp4\"}]}}"
                    : "{\"name\":\"operations/check-post\",\"done\":false}",
        };

        await CreateModel(server)
            .DoStatusAsync(StatusCall(Operation("operations/check-post")), CancellationToken.None);

        var call = server.Last(":fetchPredictOperation")!;
        Assert.Contains(":fetchPredictOperation", call.Url);
        Assert.Equal(HttpMethod.Post, call.Method);
        AssertJson("{\"operationName\":\"operations/check-post\"}", call.Body);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return completed with GCS URI video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithGCSURIVideo()
    {
        var server = new VertexVideoServer
        {
            OperationName = "operations/gcs-op",
            Videos = "[{\"gcsUri\":\"gs://bucket/video.mp4\",\"mimeType\":\"video/mp4\"}]",
        };

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Operation("operations/gcs-op")), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("gs://bucket/video.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Null(video.Data);
    }
}
