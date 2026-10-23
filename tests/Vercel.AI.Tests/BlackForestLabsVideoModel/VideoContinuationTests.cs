// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>video continuation</c>.</summary>
public sealed class VideoContinuationTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > video continuation::";

    [Fact]
    [UpstreamTest(Id + "should continue from a video reference", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldContinueFromAVideoReference()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(inputReferences: new[] { VideoUrlFile }), CancellationToken.None);

        AssertJson("{\"mode\":\"v2v\",\"prompt\":\"" + PromptText + "\",\"start_video\":\"" + VideoUrlFile.Url + "\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should use only the first video reference and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseOnlyTheFirstVideoReferenceAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(inputReferences: new[] { VideoUrlFile, VideoModelFile.FromUrl("https://cdn.example.com/second.mp4", "video/mp4") }),
            CancellationToken.None);

        AssertMatches("{\"start_video\":\"" + VideoUrlFile.Url + "\"}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "inputReferences", "FLUX 3 video continues from a single video. Only the first video reference was used."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should ignore an image reference and point at the keyframe inputs", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIgnoreAnImageReferenceAndPointAtTheKeyframeInputs()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(inputReferences: new[] { ImageUrlFile }), CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "inputReferences",
                "FLUX 3 video has no reference-image input. Pass images as `image`, `frameImages`, or "
                    + "providerOptions.blackForestLabs.keyframes instead. The reference was ignored."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should treat a reference without a mediaType as the video to continue from", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldTreatAReferenceWithoutAMediaTypeAsTheVideoToContinueFrom()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(inputReferences: new[] { VideoModelFile.FromUrl("https://cdn.example.com/untyped") }),
            CancellationToken.None);

        AssertMatches("{\"mode\":\"v2v\",\"start_video\":\"https://cdn.example.com/untyped\"}", RequestBody(server));
        Assert.Contains(
            (
                "compatibility",
                "inputReferences",
                "FLUX 3 video only accepts a video reference, so the reference with no mediaType was treated as "
                    + "the video to continue from. Pass { url, mediaType: \"video/mp4\" } to be explicit."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should ignore a reference that is neither an image nor a video and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIgnoreAReferenceThatIsNeitherAnImageNorAVideoAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(inputReferences: new[] { VideoModelFile.FromUrl("https://cdn.example.com/voice.mp3", "audio/mpeg") }),
            CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "inputReferences", "FLUX 3 video only accepts a video reference; the \"audio/mpeg\" reference was ignored."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should keep keyframes and drop a video reference when both are given", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldKeepKeyframesAndDropAVideoReferenceWhenBothAreGiven()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(image: ImageUrlFile, inputReferences: new[] { VideoUrlFile }), CancellationToken.None);

        AssertJson("{\"mode\":\"i2v\",\"prompt\":\"" + PromptText + "\",\"keyframes\":[\"" + ImageUrlFile.Url + "\"]}", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "inputReferences",
                "FLUX 3 video cannot combine keyframes with a video to continue from. The video reference was ignored."),
            result.Warnings.Select(Warning));
    }
}
