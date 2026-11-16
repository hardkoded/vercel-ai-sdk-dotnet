// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.XaiVideoModel.XaiVideoModelFixture;

namespace Vercel.AI.Tests.XaiVideoModel;

/// <summary>Port of <c>xai-video-model.test.ts</c> &gt; <c>XaiVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/xai/src/xai-video-model.test.ts::XaiVideoModel > doStatus::";
    private static readonly DateTimeOffset TestDate = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    [UpstreamTest(
        Id + "should encode the request ID as a single URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldEncodeTheRequestIdAsASingleUrlPathSegment()
    {
        var server = new XaiVideoServer();
        const string requestId = "abc/../../internal";

        await CreateModel(server)
            .DoStatusAsync(
                StatusCall("{\"requestId\":\"" + requestId + "\"}"),
                CancellationToken.None
            );

        Assert.Equal(
            TestBaseUrl + "/videos/" + Uri.EscapeDataString(requestId),
            server.Calls[0].Url
        );
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [UpstreamTest(
        Id + "should preserve the $requestId request ID as a URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreserveTheRequestIdRequestIdAsAUrlPathSegment(
        string requestId,
        string encodedRequestId
    )
    {
        var server = new XaiVideoServer();

        await CreateModel(server)
            .DoStatusAsync(
                StatusCall("{\"requestId\":\"" + requestId + "\"}"),
                CancellationToken.None
            );

        Assert.Equal(TestBaseUrl + "/videos/" + encodedRequestId, server.Calls[0].Url);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return completed with video data when done",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithVideoDataWhenDone()
    {
        var result = await CreateModel(new XaiVideoServer())
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://vidgen.x.ai/output/video-001.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Null(video.Data);
        AssertJson(
            "{\"xai\":{\"requestId\":\"req-123\",\"videoUrl\":\"https://vidgen.x.ai/output/video-001.mp4\",\"duration\":5,\"progress\":100}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should return pending when status is pending",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnPendingWhenStatusIsPending()
    {
        var server = new XaiVideoServer();
        server.Json(Req123Url, "{\"status\":\"pending\"}");

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on expired",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnExpired()
    {
        var server = new XaiVideoServer();
        server.Json(Req123Url, "{\"status\":\"expired\",\"model\":\"grok-imagine-video\"}");

        var result = await CreateModel(server, currentDate: () => TestDate)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation request expired.", result.Error);
        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("grok-imagine-video", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Id + "should return error status on failed", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnErrorStatusOnFailed()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"failed\",\"model\":\"grok-imagine-video\",\"progress\":0,\"error\":{\"message\":\"Content policy violation\"}}"
        );

        var result = await CreateModel(server, currentDate: () => TestDate)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation failed: Content policy violation", result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "should report an error status when video URL missing on done",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReportAnErrorStatusWhenVideoUrlMissingOnDone()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":null,\"model\":\"grok-imagine-video\"}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation completed but no video URL was returned.", result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "should report an error status when respect_moderation is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReportAnErrorStatusWhenRespectModerationIsFalse()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":{\"url\":\"\",\"respect_moderation\":false},\"model\":\"grok-imagine-video\"}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal(
            "Video generation was blocked due to a content policy violation.",
            result.Error
        );
    }

    [Fact]
    [UpstreamTest(Id + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeResponseMetadata()
    {
        var result = await CreateModel(new XaiVideoServer(), currentDate: () => TestDate)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("grok-imagine-video", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers to request", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassHeadersToRequest()
    {
        var server = new XaiVideoServer();
        var model = CreateModel(
            server,
            headers: new Dictionary<string, string> { ["Authorization"] = "Bearer custom-token" }
        );

        await model.DoStatusAsync(
            StatusCall(
                Req123(),
                new Dictionary<string, string> { ["X-Request-Header"] = "request-value" }
            ),
            CancellationToken.None
        );

        Assert.Equal("Bearer custom-token", server.Calls[0].Headers["authorization"]);
        Assert.Equal("request-value", server.Calls[0].Headers["x-request-header"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include costInUsdTicks when returned in usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeCostInUsdTicksWhenReturnedInUsage()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":{\"url\":\"https://vidgen.x.ai/output/video-001.mp4\",\"duration\":5,\"respect_moderation\":true},\"model\":\"grok-imagine-video\",\"progress\":100,\"usage\":{\"cost_in_usd_ticks\":4000000000}}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        AssertJson(
            "{\"xai\":{\"requestId\":\"req-123\",\"videoUrl\":\"https://vidgen.x.ai/output/video-001.mp4\",\"duration\":5,\"progress\":100,\"costInUsdTicks\":4000000000}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should omit duration from metadata when absent in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldOmitDurationFromMetadataWhenAbsentInResponse()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":{\"url\":\"https://vidgen.x.ai/output/video-001.mp4\",\"respect_moderation\":true},\"model\":\"grok-imagine-video\"}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        AssertNoProperty("duration", Metadata(result)!["xai"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should omit progress from metadata when absent in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldOmitProgressFromMetadataWhenAbsentInResponse()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":{\"url\":\"https://vidgen.x.ai/output/video-001.mp4\",\"duration\":5,\"respect_moderation\":true},\"model\":\"grok-imagine-video\"}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        AssertNoProperty("progress", Metadata(result)!["xai"]);
    }
}
