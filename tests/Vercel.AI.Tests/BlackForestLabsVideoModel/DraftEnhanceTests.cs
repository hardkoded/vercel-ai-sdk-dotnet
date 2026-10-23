// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>draft enhance</c>.</summary>
public sealed class DraftEnhanceTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > draft enhance::";

    [Fact]
    [UpstreamTest(Id + "should replay a bundle with only mode and draft_cache", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReplayABundleWithOnlyModeAndDraft_cache()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\"}"), CancellationToken.None);

        AssertJson("{\"mode\":\"draft_enhance\",\"draft_cache\":\"YmluYXJ5\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should accept the draft_cache download URL as the bundle", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAcceptTheDraft_cacheDownloadUrlAsTheBundle()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(
            Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"https://api.example.com/draft/bundle.bin\"}"),
            CancellationToken.None);

        AssertMatches("{\"draft_cache\":\"https://api.example.com/draft/bundle.bin\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should keep safetyTolerance, the one field enhance still honors", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldKeepSafetyToleranceTheOneFieldEnhanceStillHonors()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(
            Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\",\"safetyTolerance\":1}"),
            CancellationToken.None);

        AssertJson("{\"mode\":\"draft_enhance\",\"draft_cache\":\"YmluYXJ5\",\"safety_tolerance\":1}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should not warn when only the required empty prompt is passed", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnWhenOnlyTheRequiredEmptyPromptIsPassed()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(
            Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\"}"),
            CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should warn that a prompt carrying text is pinned in the bundle", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnThatAPromptCarryingTextIsPinnedInTheBundle()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(Call(blackForestLabs: "{\"draftCache\":\"YmluYXJ5\"}"), CancellationToken.None);

        Assert.Equal(
            new[]
            {
                (
                    "unsupported",
                    (string?)"prompt",
                    (string?)("FLUX 3 draft enhance replays the draft bundle as it was generated, so \"prompt\" was ignored. "
                        + "Set it on the original draft request instead.")),
            },
            result.Warnings.Select(Warning).ToArray());
    }

    [Fact]
    [UpstreamTest(Id + "should warn about every generation option the bundle pins", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutEveryGenerationOptionTheBundlePins()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(
                prompt: string.Empty,
                aspectRatio: "16:9",
                resolution: "1920x1080",
                duration: 10,
                fps: 24,
                seed: 5,
                generateAudio: true,
                image: ImageUrlFile,
                inputReferences: new[] { VideoUrlFile },
                blackForestLabs: "{\"draftCache\":\"YmluYXJ5\",\"version\":\"latest\",\"keyframes\":[\"https://cdn.example.com/a.png\"]}"),
            CancellationToken.None);

        // Still a bare replay: none of the above reaches the API.
        AssertJson("{\"mode\":\"draft_enhance\",\"draft_cache\":\"YmluYXJ5\"}", RequestBody(server));
        // `prompt` is empty here, so it is the one pinned field not reported.
        Assert.Equal(
            new[] { "aspectRatio", "resolution", "duration", "fps", "seed", "generateAudio", "image", "inputReferences", "keyframes", "version" },
            result.Warnings.Select(warning => warning.Feature ?? warning.Type).ToArray());
    }

    [Fact]
    [UpstreamTest(Id + "should warn that draft is meaningless while enhancing", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnThatDraftIsMeaninglessWhileEnhancing()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(
            Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\",\"draft\":true}"),
            CancellationToken.None);

        Assert.Equal(
            new[]
            {
                (
                    "unsupported",
                    (string?)"draft",
                    (string?)"FLUX 3 draft enhance always renders at full quality. The draft option was ignored."),
            },
            result.Warnings.Select(Warning).ToArray());
    }

    [Fact]
    [UpstreamTest(Id + "should warn about n above 1", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutNAbove1()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(
            Call(prompt: string.Empty, n: 3, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\"}"),
            CancellationToken.None);

        Assert.Equal(
            new[]
            {
                (
                    "unsupported",
                    (string?)"n",
                    (string?)"FLUX 3 video generates a single video per call. Only 1 video will be generated."),
            },
            result.Warnings.Select(Warning).ToArray());
    }

    [Fact]
    [UpstreamTest(Id + "should return the full-quality video from the replay", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnTheFullQualityVideoFromTheReplay()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(
            Call(prompt: string.Empty, blackForestLabs: "{\"draftCache\":\"YmluYXJ5\"}"),
            CancellationToken.None);

        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal(VideoUrl, video.Url);
        Assert.Equal("video/mp4", video.MediaType);
    }
}
