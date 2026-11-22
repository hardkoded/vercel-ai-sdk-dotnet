// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.FalVideoModel.FalVideoModelFixture;

namespace Vercel.AI.Tests.FalVideoModel;

/// <summary>Port of <c>fal-video-model.test.ts</c> &gt; <c>FalVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/fal/src/fal-video-model.test.ts::FalVideoModel > doStatus::";

    private static readonly string DreamMachineOperation =
        "{\"responseUrl\":\"" + DreamMachineStatusUrl + "\"}";

    private static readonly string TestIdOperation =
        "{\"responseUrl\":\"https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-id\"}";

    [Fact]
    [UpstreamTest(
        Id + "should return completed status with video data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedStatusWithVideoData()
    {
        var model = CreateBasicModel(new FalVideoServer());

        var result = await model.DoStatusAsync(
            StatusCall(DreamMachineOperation),
            CancellationToken.None
        );

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://fal.media/files/video-output.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.NotNull(Metadata(result)!["fal"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return pending status when request is still in progress",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnPendingStatusWhenRequestIsStillInProgress()
    {
        var server = new FalVideoServer();
        server.Respond(
            "https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-id",
            500,
            "{\"detail\":\"Request is still in progress\"}"
        );
        var model = CreateBasicModel(server);

        var result = await model.DoStatusAsync(StatusCall(TestIdOperation), CancellationToken.None);

        Assert.Single(server.Calls);
        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include provider metadata in completed status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeProviderMetadataInCompletedStatus()
    {
        var model = CreateBasicModel(new FalVideoServer());

        var result = await model.DoStatusAsync(
            StatusCall(DreamMachineOperation),
            CancellationToken.None
        );

        Assert.Equal("completed", result.Status);
        AssertJson(
            "{\"fal\":{\"videos\":[{\"url\":\"https://fal.media/files/video-output.mp4\",\"width\":1920,\"height\":1080,\"duration\":5.0,\"fps\":24,\"contentType\":\"video/mp4\"}],\"seed\":12345,\"timings\":{\"inference\":45.5}}}",
            Metadata(result)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw error when video URL is missing in completed response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowErrorWhenVideoUrlIsMissingInCompletedResponse()
    {
        var server = new FalVideoServer();
        server.Json(DreamMachineStatusUrl, "{}");
        var model = CreateBasicModel(server);

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoStatusAsync(StatusCall(DreamMachineOperation), CancellationToken.None)
        );

        Assert.Equal("No video URL in response", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status for non-polling API errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusForNonPollingApiErrors()
    {
        var server = new FalVideoServer();
        server.Respond(
            "https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-id",
            500,
            "{\"error\":{\"message\":\"Internal server error\",\"code\":500}}"
        );
        var model = CreateBasicModel(server, currentDate: () =>
            new DateTimeOffset(2026, 2, 16, 0, 0, 0, TimeSpan.Zero)
        );

        var result = await model.DoStatusAsync(StatusCall(TestIdOperation), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Internal server error", result.Error);
        Assert.Equal("luma-dream-machine", result.Response.ModelId);
        Assert.Equal(
            new DateTime(2026, 2, 16, 0, 0, 0, DateTimeKind.Utc),
            result.Response.Timestamp
        );
    }
}
