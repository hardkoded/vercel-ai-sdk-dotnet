// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>text-to-video</c>.</summary>
public sealed class TextToVideoTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > text-to-video::";

    [Fact]
    [UpstreamTest(Id + "should submit a t2v request with only the fields that were set", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSubmitAT2vRequestWithOnlyTheFieldsThatWereSet()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(SubmitUrl, server.Calls[0].Url);
        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should return the signed video URL rather than downloading it", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnTheSignedVideoUrlRatherThanDownloadingIt()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal(VideoUrl, video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        // Submit + poll only: the video itself is never fetched.
        Assert.Equal(2, server.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Id + "should report the submit cost and megapixels in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReportTheSubmitCostAndMegapixelsInProviderMetadata()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8,\"cost\":0.42,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}]",
            Videos(result));
    }

    // The submit response can only estimate, and returns null whenever the
    // price depends on the finished video. `get_result` then answers with the
    // `SettledCostResultResponse` variant, which carries the real charge.
    [Fact]
    [UpstreamTest(Id + "should prefer the settled cost from the result over the submit estimate", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferTheSettledCostFromTheResultOverTheSubmitEstimate()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Ready\",\"result\":{\"sample\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8},\"cost\":0.85}");

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8,\"cost\":0.85,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}]",
            Videos(result));
    }

    [Fact]
    [UpstreamTest(Id + "should report the settled cost when the submit response omits one", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReportTheSettledCostWhenTheSubmitResponseOmitsOne()
    {
        var server = CreateServer();
        server.Json(SubmitUrl, BareSubmitResponse);
        server.Json(PollUrl, "{\"status\":\"Ready\",\"result\":{\"sample\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8},\"cost\":0.85}");

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8,\"cost\":0.85}]",
            Videos(result));
    }

    // The plain `ResultResponse` variant has no cost at all.
    [Fact]
    [UpstreamTest(Id + "should omit cost entirely when neither response reports one", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitCostEntirelyWhenNeitherResponseReportsOne()
    {
        var server = CreateServer();
        server.Json(SubmitUrl, BareSubmitResponse);

        var result = await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"seed\":7,\"duration\":8}]",
            Videos(result));
    }

    [Fact]
    [UpstreamTest(Id + "should send an empty prompt when none is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendAnEmptyPromptWhenNoneIsProvided()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(prompt: null), CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"\"}", RequestBody(server));
    }
}
