// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>
/// Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c> &gt; <c>wan3 (all-in-one)</c>.
/// wan3 ships one id for every mode, so the request's own media, not the model id, decides text- vs image- vs
/// reference-to-video.
/// </summary>
public sealed class DoStartWan3AllInOneTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart > wan3 (all-in-one)::";

    private const string Wan3 = "wan3.0-video";

    [Fact]
    [UpstreamTest(
        Id + "should send a bare prompt with no media for text-to-video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendABarePromptWithNoMediaForTextToVideo()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, Wan3), Call());

        var body = RequestBody(server);
        AssertMatches(
            "{\"model\":\"wan3.0-video\",\"input\":{\"prompt\":\"" + PromptText + "\"}}",
            body
        );
        AssertNoProperty("media", body["input"]);
        AssertNoProperty("img_url", body["input"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should carry the start image in input.media instead of img_url",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCarryTheStartImageInInputMediaInsteadOfImgUrl()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, Wan3), Call(image: Url("https://example.com/first.jpg")));

        var body = RequestBody(server);
        AssertMatches(
            "{\"media\":[{\"type\":\"first_frame\",\"url\":\"https://example.com/first.jpg\"}]}",
            body["input"]
        );
        AssertNoProperty("img_url", body["input"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send both frame images without warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendBothFrameImagesWithoutWarning()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(
                frameImages:
                [
                    Frame("first_frame", Url("https://example.com/first.jpg")),
                    Frame("last_frame", Url("https://example.com/last.jpg")),
                ]
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"first_frame\",\"url\":\"https://example.com/first.jpg\"},{\"type\":\"last_frame\",\"url\":\"https://example.com/last.jpg\"}]}",
            RequestBody(server)["input"]
        );
        Assert.False(HasFeature(result.Warnings, "frameImages"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should map inputReferences to media without a dedicated r2v id",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldMapInputReferencesToMediaWithoutADedicatedR2VId()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(
                inputReferences:
                [
                    Url("https://example.com/ref.jpg"),
                    Url("https://example.com/ref.mp4"),
                ]
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"reference_image\",\"url\":\"https://example.com/ref.jpg\"},{\"type\":\"reference_video\",\"url\":\"https://example.com/ref.mp4\"}]}",
            RequestBody(server)["input"]
        );
        Assert.False(HasFeature(result.Warnings, "inputReferences"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should send resolution tiers, including 480P",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendResolutionTiersIncluding480P()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(CreateModel(server, Wan3), Call(resolution: "832x480"));

        AssertMatches("{\"parameters\":{\"resolution\":\"480P\"}}", RequestBody(server));
        Assert.False(HasFeature(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn on a resolution outside the three tiers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnOnAResolutionOutsideTheThreeTiers()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(CreateModel(server, Wan3), Call(resolution: "3840x2160"));

        AssertNoProperty("resolution", RequestBody(server)["parameters"]);
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should send the audio toggle, adaptive ratio, and smart duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendTheAudioToggleAdaptiveRatioAndSmartDuration()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(generateAudio: false, duration: -1, alibaba: "{\"ratio\":\"adaptive\"}")
        );

        AssertMatches(
            "{\"parameters\":{\"audio\":false,\"ratio\":\"adaptive\",\"duration\":-1}}",
            RequestBody(server)
        );
        Assert.False(HasFeature(result.Warnings, "generateAudio"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through wan3-only media types from providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassThroughWan3OnlyMediaTypesFromProviderOptions()
    {
        var server = new AlibabaVideoServer();
        const string media =
            "[{\"type\":\"reference_audio\",\"url\":\"https://example.com/voice.mp3\"},{\"type\":\"file\",\"url\":\"https://example.com/deck.pdf\"},{\"type\":\"link\",\"url\":\"https://example.com/article\"}]";

        await Start(CreateModel(server, Wan3), Call(alibaba: "{\"media\":" + media + "}"));

        AssertMatches("{\"media\":" + media + "}", RequestBody(server)["input"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn about shotType, which wan3 dropped",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnAboutShotTypeWhichWan3Dropped()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(alibaba: "{\"shotType\":\"multi\"}")
        );

        AssertNoProperty("shot_type", RequestBody(server)["parameters"]);
        Assert.True(HasWarning(result.Warnings, "shotType"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should prefer frameImages first_frame over the legacy image option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreferFrameImagesFirstFrameOverTheLegacyImageOption()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, Wan3),
            Call(
                image: Url("https://example.com/legacy.jpg"),
                frameImages: [Frame("first_frame", Url("https://example.com/first.jpg"))]
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"first_frame\",\"url\":\"https://example.com/first.jpg\"}]}",
            RequestBody(server)["input"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send last_frame alone without warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendLastFrameAloneWithoutWarning()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(frameImages: [Frame("last_frame", Url("https://example.com/last.jpg"))])
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"last_frame\",\"url\":\"https://example.com/last.jpg\"}]}",
            RequestBody(server)["input"]
        );
        Assert.False(HasFeature(result.Warnings, "frameImages"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should combine inputReferences with first and last frames",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCombineInputReferencesWithFirstAndLastFrames()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(
                inputReferences:
                [
                    Url("https://example.com/character.png"),
                    Url("https://example.com/role.mp4"),
                ],
                frameImages:
                [
                    Frame("first_frame", Url("https://example.com/first.jpg")),
                    Frame("last_frame", Url("https://example.com/last.jpg")),
                ]
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"reference_image\",\"url\":\"https://example.com/character.png\"},{\"type\":\"reference_video\",\"url\":\"https://example.com/role.mp4\"},{\"type\":\"first_frame\",\"url\":\"https://example.com/first.jpg\"},{\"type\":\"last_frame\",\"url\":\"https://example.com/last.jpg\"}]}",
            RequestBody(server)["input"]
        );
        Assert.False(HasFeature(result.Warnings, "frameImages"));
        Assert.False(HasFeature(result.Warnings, "inputReferences"));
    }

    [Theory]
    [InlineData("1280x720", "720P")]
    [InlineData("1920x1080", "1080P")]
    [InlineData("720x1280", "720P")]
    [InlineData("832x480", "480P")]
    [UpstreamTest(Id + "should map resolution %s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapResolutionTo(string resolution, string tier)
    {
        var server = new AlibabaVideoServer();

        var result = await Start(CreateModel(server, Wan3), Call(resolution: resolution));

        var parameters = RequestBody(server)["parameters"];
        AssertMatches("{\"resolution\":\"" + tier + "\"}", parameters);
        AssertNoProperty("size", parameters);
        Assert.False(HasFeature(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should derive a supported ratio from the resolution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldDeriveASupportedRatioFromTheResolution()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, Wan3), Call(resolution: "720x1280"));

        AssertMatches(
            "{\"resolution\":\"720P\",\"ratio\":\"9:16\"}",
            RequestBody(server)["parameters"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map aspectRatio, including adaptive, without warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldMapAspectRatioIncludingAdaptiveWithoutWarning()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(CreateModel(server, Wan3), Call(aspectRatio: "adaptive"));

        AssertMatches("{\"ratio\":\"adaptive\"}", RequestBody(server)["parameters"]);
        Assert.False(HasFeature(result.Warnings, "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should prefer providerOptions.alibaba.ratio over aspectRatio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreferProviderOptionsAlibabaRatioOverAspectRatio()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, Wan3),
            Call(aspectRatio: "9:16", alibaba: "{\"ratio\":\"1:1\"}")
        );

        AssertMatches("{\"ratio\":\"1:1\"}", RequestBody(server)["parameters"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should let generateAudio override the legacy audio option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldLetGenerateAudioOverrideTheLegacyAudioOption()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(generateAudio: false, alibaba: "{\"audio\":true}")
        );

        AssertMatches("{\"audio\":false}", RequestBody(server)["parameters"]);
        Assert.False(HasFeature(result.Warnings, "generateAudio"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should send the legacy audio option when generateAudio is omitted",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendTheLegacyAudioOptionWhenGenerateAudioIsOmitted()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, Wan3), Call(alibaba: "{\"audio\":false}"));

        AssertMatches("{\"audio\":false}", RequestBody(server)["parameters"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should still send audio_url, prompt rewrite, watermark, and seed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldStillSendAudioUrlPromptRewriteWatermarkAndSeed()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, Wan3),
            Call(
                seed: 42,
                duration: 8,
                alibaba: "{\"audioUrl\":\"https://example.com/soundtrack.mp3\",\"negativePrompt\":\"blurry, low quality\",\"promptExtend\":false,\"watermark\":true}"
            )
        );

        AssertMatches(
            "{\"input\":{\"prompt\":\""
                + PromptText
                + "\",\"audio_url\":\"https://example.com/soundtrack.mp3\",\"negative_prompt\":\"blurry, low quality\"},\"parameters\":{\"seed\":42,\"duration\":8,\"prompt_extend\":false,\"watermark\":true}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send file images as data URIs for frames and references",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendFileImagesAsDataUrisForFramesAndReferences()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, Wan3),
            Call(
                inputReferences: [VideoModelFile.FromFile(ImageData, "image/png")],
                frameImages: [Frame("first_frame", VideoModelFile.FromFile(ImageData, "image/png"))]
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"reference_image\",\"url\":\"data:image/png;base64,iVBORw==\"},{\"type\":\"first_frame\",\"url\":\"data:image/png;base64,iVBORw==\"}]}",
            RequestBody(server)["input"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn and skip non-URL video references",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnAndSkipNonUrlVideoReferences()
    {
        var server = new AlibabaVideoServer();

        var result = await Start(
            CreateModel(server, Wan3),
            Call(inputReferences: [VideoModelFile.FromFile([0, 0, 0, 24], "video/mp4")])
        );

        AssertNoProperty("media", RequestBody(server)["input"]);
        Assert.True(HasWarning(result.Warnings, "inputReferences"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should use providerOptions.alibaba.media as an override",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseProviderOptionsAlibabaMediaAsAnOverride()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, Wan3),
            Call(
                inputReferences: [Url("https://example.com/ignored.png")],
                frameImages: [Frame("first_frame", Url("https://example.com/ignored-first.jpg"))],
                alibaba: "{\"media\":[{\"type\":\"reference_video\",\"url\":\"https://example.com/character.mp4\",\"referenceVoice\":\"https://example.com/voice.mp3\"},{\"type\":\"last_frame\",\"url\":\"https://example.com/closing.png\"}]}"
            )
        );

        AssertMatches(
            "{\"media\":[{\"type\":\"reference_video\",\"url\":\"https://example.com/character.mp4\",\"reference_voice\":\"https://example.com/voice.mp3\"},{\"type\":\"last_frame\",\"url\":\"https://example.com/closing.png\"}]}",
            RequestBody(server)["input"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should still warn about unsupported fps and n > 1",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldStillWarnAboutUnsupportedFpsAndNGreaterThan1()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer(), Wan3), Call(fps: 24, n: 3));

        Assert.True(HasWarning(result.Warnings, "fps"));
        Assert.True(HasWarning(result.Warnings, "n"));
    }
}
