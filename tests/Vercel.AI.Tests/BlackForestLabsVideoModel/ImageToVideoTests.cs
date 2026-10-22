// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>image-to-video</c>.</summary>
public sealed class ImageToVideoTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > image-to-video::";

    [Fact]
    [UpstreamTest(Id + "should turn a standalone image into a single keyframe", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldTurnAStandaloneImageIntoASingleKeyframe()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(image: ImageUrlFile), CancellationToken.None);

        AssertJson("{\"mode\":\"i2v\",\"prompt\":\"" + PromptText + "\",\"keyframes\":[\"" + ImageUrlFile.Url + "\"]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should order a first_frame and last_frame into two keyframes", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOrderAFirstFrameAndLastFrameIntoTwoKeyframes()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(
            Call(frameImages: new[] { new VideoFrameFile(LastImageUrlFile, "last_frame"), new VideoFrameFile(ImageUrlFile, "first_frame") }),
            CancellationToken.None);

        AssertMatches("{\"mode\":\"i2v\",\"keyframes\":[\"" + ImageUrlFile.Url + "\",\"" + LastImageUrlFile.Url + "\"]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should send inline image data as a bare base64 string",
        Coverage = UpstreamCoverage.Covered,
        Note = "VideoModelFile holds bytes only, so the string form of the data is the bytes of that string.")]
    public async Task ShouldSendInlineImageDataAsABareBase64String()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(image: VideoModelFile.FromFile(Encoding.ASCII.GetBytes("abc"), "image/png")), CancellationToken.None);

        AssertMatches("{\"keyframes\":[\"YWJj\"]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should base64-encode binary image data", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldBase64EncodeBinaryImageData()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(image: VideoModelFile.FromFile(new byte[] { 97, 98, 99 }, "image/png")), CancellationToken.None);

        AssertMatches("{\"keyframes\":[\"YWJj\"]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should drop a last_frame with no first_frame and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldDropALastFrameWithNoFirstFrameAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(frameImages: new[] { new VideoFrameFile(LastImageUrlFile, "last_frame") }),
            CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "frameImages", "FLUX 3 video requires a first_frame when a last_frame is provided. The last_frame was ignored."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should reject a video passed as the starting image and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectAVideoPassedAsTheStartingImageAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(image: VideoUrlFile), CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "image",
                "FLUX 3 video does not accept a video as a keyframe. Pass it as an inputReference to continue from it instead."),
            result.Warnings.Select(Warning));
    }
}
