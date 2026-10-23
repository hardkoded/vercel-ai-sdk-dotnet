// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>polling</c>.</summary>
public sealed class PollingTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > polling::";

    private const string PendingOperation = "{\"requestId\":\"" + RequestId + "\",\"pollingUrl\":\"" + PollUrl + "\"}";

    [Fact]
    [UpstreamTest(Id + "should expose a serializable operation from doStart", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldExposeASerializableOperationFromDoStart()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoStartAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"requestId\":\"" + RequestId + "\",\"pollingUrl\":\"" + PollUrl + "\",\"cost\":0.42,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}",
            Operation(result));
        Assert.Empty(result.Warnings);
        Assert.Single(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending from a single doStatus check", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingFromASingleDoStatusCheck()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Generating\"}");

        var result = await CreateModel(server).DoStatusAsync(StatusCall(PendingOperation), CancellationToken.None);

        Assert.Equal("pending", result.Status);
        Assert.Single(server.Calls);
        Assert.Equal(PollUrl + "?id=" + RequestId, server.Calls[0].Url);
    }

    [Fact]
    [UpstreamTest(Id + "should return a completed video from doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnACompletedVideoFromDoStatus()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoStatusAsync(
            StatusCall("{\"requestId\":\"" + RequestId + "\",\"pollingUrl\":\"" + PollUrl + "\",\"cost\":0.42,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}"),
            CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal(VideoUrl, video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8,\"cost\":0.42,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}]",
            Videos(result));
        Assert.Single(server.Calls);
    }

    // A resumed operation carries whatever the submit response estimated,
    // which may be stale or absent by the time the video is ready.
    [Fact]
    [UpstreamTest(Id + "should prefer the settled cost over the cost carried on the operation", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferTheSettledCostOverTheCostCarriedOnTheOperation()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Ready\",\"result\":{\"sample\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8},\"cost\":0.85}");

        var result = await CreateModel(server).DoStatusAsync(
            StatusCall("{\"requestId\":\"" + RequestId + "\",\"pollingUrl\":\"" + PollUrl + "\",\"cost\":0.42}"),
            CancellationToken.None);

        Assert.Equal("completed", result.Status);
        AssertMatches("{\"cost\":0.85}", Videos(result)![0]);
    }

    [Fact]
    [UpstreamTest(Id + "should return an error from doStatus for terminal failures", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnAnErrorFromDoStatusForTerminalFailures()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Content Moderated\",\"details\":\"blocked by policy\"}");

        var result = await CreateModel(server).DoStatusAsync(StatusCall(PendingOperation), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal(
            "Black Forest Labs video generation failed with status \"Content Moderated\": blocked by policy. Request id: req-123",
            result.Error);
        Assert.Single(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "should keep polling through non-terminal statuses", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldKeepPollingThroughNonTerminalStatuses()
    {
        var server = CreateServer();
        var callNumber = 0;
        server.Urls[PollUrl] = _ =>
        {
            callNumber++;
            return Task.FromResult(JsonResponse(callNumber < 3
                ? "{\"status\":\"" + (callNumber == 1 ? "Pending" : "Generating") + "\"}"
                : ReadyResponse));
        };

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(3, callNumber);
        Assert.Equal(VideoUrl, result.Videos[0].Url);
    }

    [Fact]
    [UpstreamTest(Id + "should append the request id to a polling URL that lacks one", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAppendTheRequestIdToAPollingUrlThatLacksOne()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(PollUrl + "?id=" + RequestId, server.Calls[1].Url);
    }

    [Fact]
    [UpstreamTest(Id + "should throw on a moderated result", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowOnAModeratedResult()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Content Moderated\",\"details\":\"blocked by policy\"}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains(
            "Black Forest Labs video generation failed with status \"Content Moderated\": blocked by policy. Request id: req-123",
            exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when the job errors", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowWhenTheJobErrors()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Error\"}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains("Black Forest Labs video generation failed with status \"Error\". Request id: req-123", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should accept `state` in place of `status`", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAcceptStateInPlaceOfStatus()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"state\":\"Ready\",\"result\":{\"sample\":\"" + VideoUrl + "\"}}");

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(VideoUrl, result.Videos[0].Url);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when Ready arrives without a sample URL", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowWhenReadyArrivesWithoutASampleUrl()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Ready\",\"result\":{}}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains(
            "Black Forest Labs reported the video as Ready but returned no result.sample URL. Request id: req-123",
            exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should time out while the job is still running", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldTimeOutWhileTheJobIsStillRunning()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Generating\"}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            CreateModel(server, pollIntervalMillis: 10, pollTimeoutMillis: 25).DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains("Black Forest Labs video generation timed out after 25ms. Request id: req-123", exception.Message);
    }
}
