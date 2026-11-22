// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ReplicateVideoModel.ReplicateVideoModelFixture;

namespace Vercel.AI.Tests.ReplicateVideoModel;

/// <summary>Port of <c>replicate-video-model.test.ts</c> &gt; <c>ReplicateVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Id =
        "packages/replicate/src/replicate-video-model.test.ts::ReplicateVideoModel > doStatus::";

    [Fact]
    [UpstreamTest(
        Id + "should return completed with video data when succeeded",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnCompletedWithVideoDataWhenSucceeded()
    {
        var (model, _) = CreateModel(
            predictionId: "status-pred-123",
            predictionStatus: "succeeded",
            output: "https://replicate.delivery/video-output.mp4"
        );

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://replicate.delivery/video-output.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return pending when processing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnPendingWhenProcessing()
    {
        var (model, _) = CreateModel(
            predictionId: "status-pred-123",
            predictionStatus: "processing"
        );

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Id + "should return pending when starting", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnPendingWhenStarting()
    {
        var (model, _) = CreateModel(predictionId: "status-pred-123", predictionStatus: "starting");

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on failed prediction",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnFailedPrediction()
    {
        var (model, _) = CreateModel(
            predictionId: "status-pred-123",
            predictionStatus: "failed",
            error: "GPU out of memory"
        );

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal("error", result.Status);
        Assert.Contains("GPU out of memory", result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return error status on canceled prediction",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnErrorStatusOnCanceledPrediction()
    {
        var (model, _) = CreateModel(predictionId: "status-pred-123", predictionStatus: "canceled");

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal("error", result.Status);
        Assert.Contains("canceled", result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when no output on succeeded",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowWhenNoOutputOnSucceeded()
    {
        var (model, _) = CreateModel(
            predictionId: "status-pred-123",
            predictionStatus: "succeeded",
            output: null
        );

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoStatusAsync(
                StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
                CancellationToken.None
            )
        );

        Assert.Equal("No video URL in response", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should include providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeProviderMetadata()
    {
        var (model, _) = CreateModel(
            predictionId: "meta-pred-456",
            predictionStatus: "succeeded",
            output: "https://replicate.delivery/video.mp4",
            metrics: "{\"predict_time\":30.2}"
        );

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/meta-pred-456"),
            CancellationToken.None
        );

        Assert.Equal("completed", result.Status);
        AssertMatches(
            "{\"replicate\":{\"predictionId\":\"meta-pred-456\",\"metrics\":{\"predict_time\":30.2},\"videos\":[{\"url\":\"https://replicate.delivery/video.mp4\"}]}}",
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
        );
    }

    [Fact]
    [UpstreamTest(Id + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeResponseMetadata()
    {
        var testDate = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var (model, _) = CreateModel(
            predictionId: "status-pred-123",
            currentDate: () => testDate,
            predictionStatus: "processing"
        );

        var result = await model.DoStatusAsync(
            StatusCall("https://api.replicate.com/v1/predictions/status-pred-123"),
            CancellationToken.None
        );

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("minimax/video-01", result.Response.ModelId);
    }
}
