// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.XaiVideoModel.XaiVideoModelFixture;

namespace Vercel.AI.Tests.XaiVideoModel;

/// <summary>Port of <c>xai-video-model.test.ts</c> &gt; <c>XaiVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id = "packages/xai/src/xai-video-model.test.ts::XaiVideoModel > doStart::";
    private const string Grok15 = "grok-imagine-video-1.5";
    private const string SourceVideo = "https://example.com/source-video.mp4";
    private const string EditMode = "{\"mode\":\"edit-video\",\"videoUrl\":\"" + SourceVideo + "\"}";
    private const string ExtendMode = "{\"mode\":\"extend-video\",\"videoUrl\":\"" + SourceVideo + "\"}";

    private static readonly VideoModelFile Voice = VideoModelFile.FromUrl("https://example.com/voice.mp3", "audio/mpeg");

    [Fact]
    [UpstreamTest(Id + "should return operation with requestId", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnOperationWithRequestId()
    {
        var result = await Start(new XaiVideoServer(), Call());

        AssertJson("{\"requestId\":\"req-123\"}", Operation(result));
    }

    [Fact]
    [UpstreamTest(Id + "should pass correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassCorrectRequestBody()
    {
        var server = new XaiVideoServer();

        await Start(server, Call());

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(TestBaseUrl + "/videos/generations", server.Calls[0].Url);
        AssertJson("{\"model\":\"grok-imagine-video\",\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map current xAI generation options", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapCurrentXaiGenerationOptions()
    {
        var server = new XaiVideoServer();

        var result = await Start(
            server,
            Call(
                generateAudio: false,
                frameImages: [Frame("last_frame", VideoModelFile.FromUrl("https://example.com/end.png"))],
                xai: "{\"storageOptions\":{\"filename\":\"result.mp4\",\"expiresAfter\":86400,\"publicUrl\":{\"expiresAfter\":3600}},\"keyframes\":[{\"imageUrl\":\"https://example.com/middle.png\",\"timestampSeconds\":2.5}]}"),
            Grok15);

        AssertMatches(
            "{\"generate_audio\":false,\"last_frame\":{\"url\":\"https://example.com/end.png\"},\"storage_options\":{\"filename\":\"result.mp4\",\"expires_after\":86400,\"public_url\":{\"expires_after\":3600}},\"keyframes\":[{\"image\":{\"url\":\"https://example.com/middle.png\"},\"timestamp_s\":2.5}]}",
            RequestBody(server));
        AssertJson("{\"requestId\":\"req-123\"}", Operation(result));
    }

    [Fact]
    [UpstreamTest(Id + "should warn and omit last_frame for grok-imagine-video", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAndOmitLastFrameForGrokImagineVideo()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(frameImages: [Frame("last_frame", VideoModelFile.FromUrl("https://example.com/end.png"))]));

        AssertNoProperty("last_frame", RequestBody(server));
        Assert.Contains(
            ("unsupported", "frameImages", "xAI only supports last_frame with \"grok-imagine-video-1.5\". The last frame was ignored."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassHeaders()
    {
        var server = new XaiVideoServer();
        var model = CreateModel(server, headers: new Dictionary<string, string> { ["Authorization"] = "Bearer custom-token", ["X-Custom"] = "value" });

        await model.DoStartAsync(Call(headers: new Dictionary<string, string> { ["X-Request-Header"] = "request-value" }), CancellationToken.None);

        Assert.Equal("Bearer custom-token", server.Calls[0].Headers["authorization"]);
        Assert.Equal("value", server.Calls[0].Headers["x-custom"]);
        Assert.Equal("request-value", server.Calls[0].Headers["x-request-header"]);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when no request_id returned", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowWhenNoRequestIdReturned()
    {
        var server = new XaiVideoServer();
        server.Json(TestBaseUrl + "/videos/generations", "{}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => Start(server, Call()));

        Assert.Contains("No request_id", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should use edits endpoint for video editing (legacy videoUrl without mode)", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseEditsEndpointForVideoEditingLegacyVideoUrlWithoutMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"videoUrl\":\"" + SourceVideo + "\"}"));

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(TestBaseUrl + "/videos/edits", server.Calls[0].Url);
        AssertMatches("{\"video\":{\"url\":\"" + SourceVideo + "\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should use edits endpoint for video editing with explicit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseEditsEndpointForVideoEditingWithExplicitMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: EditMode));

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(TestBaseUrl + "/videos/edits", server.Calls[0].Url);
        AssertMatches("{\"video\":{\"url\":\"" + SourceVideo + "\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should use extensions endpoint for extend-video mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseExtensionsEndpointForExtendVideoMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: ExtendMode));

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(TestBaseUrl + "/videos/extensions", server.Calls[0].Url);
        AssertMatches("{\"video\":{\"url\":\"" + SourceVideo + "\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeResponseMetadata()
    {
        var testDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = await CreateModel(new XaiVideoServer(), currentDate: () => testDate).DoStartAsync(Call(), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("grok-imagine-video", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Id + "should return warnings for unsupported features", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnWarningsForUnsupportedFeatures()
    {
        var result = await Start(new XaiVideoServer(), Call(fps: 30, seed: 42));

        Assert.True(HasWarning(result.Warnings, "fps"));
        Assert.True(HasWarning(result.Warnings, "seed"));
    }

    [Fact]
    [UpstreamTest(Id + "should send duration in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendDurationInRequestBody()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(duration: 10));

        AssertMatches("{\"duration\":10}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send aspect_ratio in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendAspectRatioInRequestBody()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(aspectRatio: "9:16"));

        AssertMatches("{\"aspect_ratio\":\"9:16\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map SDK resolution 1280x720 to 720p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapSdkResolution1280x720To720p()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(resolution: "1280x720"));

        AssertMatches("{\"resolution\":\"720p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map SDK resolution 854x480 to 480p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapSdkResolution854x480To480p()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(resolution: "854x480"));

        AssertMatches("{\"resolution\":\"480p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map SDK resolution 640x480 to 480p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapSdkResolution640x480To480p()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(resolution: "640x480"));

        AssertMatches("{\"resolution\":\"480p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map SDK resolution 1920x1080 to 1080p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapSdkResolution1920x1080To1080p()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(resolution: "1920x1080"), Grok15);

        AssertMatches("{\"resolution\":\"1080p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should pass through provider option resolution 1080p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassThroughProviderOptionResolution1080p()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"resolution\":\"1080p\"}"), Grok15);

        AssertMatches("{\"resolution\":\"1080p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should warn when SDK resolution 1920x1080 is used with grok-imagine-video", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnWhenSdkResolution1920x1080IsUsedWithGrokImagineVideo()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(resolution: "1920x1080"));

        AssertMatches("{\"resolution\":\"1080p\"}", RequestBody(server));
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "resolution" && warning.Details!.Contains("does not support 1080p"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn when provider resolution 1080p is used with grok-imagine-video", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnWhenProviderResolution1080pIsUsedWithGrokImagineVideo()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"resolution\":\"1080p\"}"));

        AssertMatches("{\"resolution\":\"1080p\"}", RequestBody(server));
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "resolution" && warning.Details!.Contains("does not support 1080p"));
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about 1080p with grok-imagine-video-1.5", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAbout1080pWithGrokImagineVideo15()
    {
        var result = await Start(new XaiVideoServer(), Call(resolution: "1920x1080"), Grok15);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should prefer provider option resolution over SDK resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferProviderOptionResolutionOverSdkResolution()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(resolution: "1280x720", xai: "{\"resolution\":\"480p\"}"));

        AssertMatches("{\"resolution\":\"480p\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should warn for unrecognized resolution format", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnForUnrecognizedResolutionFormat()
    {
        var result = await Start(new XaiVideoServer(), Call(resolution: "2560x1440"));

        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn and omit body resolution for completely unknown format", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAndOmitBodyResolutionForCompletelyUnknownFormat()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(resolution: "3840x2160"));

        AssertNoProperty("resolution", RequestBody(server));
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should send image object from URL-based image input", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendImageObjectFromUrlBasedImageInput()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(image: VideoModelFile.FromUrl("https://example.com/image.png")));

        AssertMatches("{\"image\":{\"url\":\"https://example.com/image.png\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send image object with data URI from file data", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendImageObjectWithDataUriFromFileData()
    {
        var server = new XaiVideoServer();

        // PNG magic bytes.
        await Start(server, Call(image: VideoModelFile.FromFile([137, 80, 78, 71], "image/png")));

        AssertMatches("{\"image\":{\"url\":\"data:image/png;base64,iVBORw==\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send image object with data URI from base64 string", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendImageObjectWithDataUriFromBase64String()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(image: VideoModelFile.FromFile(Convert.FromBase64String("aGVsbG8="), "image/jpeg")));

        AssertMatches("{\"image\":{\"url\":\"data:image/jpeg;base64,aGVsbG8=\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about duration in edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutDurationInEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(duration: 10, xai: EditMode));

        Assert.True(HasWarning(result.Warnings, "duration"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about aspectRatio in edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutAspectRatioInEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(aspectRatio: "16:9", xai: EditMode));

        Assert.True(HasWarning(result.Warnings, "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about resolution in edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutResolutionInEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(resolution: "1280x720", xai: EditMode));

        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about duration outside edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAboutDurationOutsideEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(duration: 10));

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "duration");
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about aspectRatio outside edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAboutAspectRatioOutsideEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(aspectRatio: "16:9"));

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about resolution outside edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAboutResolutionOutsideEditMode()
    {
        var result = await Start(new XaiVideoServer(), Call(resolution: "1280x720"));

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "resolution");
    }

    [Fact]
    [UpstreamTest(Id + "should omit duration, aspect_ratio, and resolution from body in edit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitDurationAspectRatioAndResolutionFromBodyInEditMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(duration: 10, aspectRatio: "16:9", resolution: "1280x720", xai: EditMode));

        var body = RequestBody(server);
        AssertNoProperty("duration", body);
        AssertNoProperty("aspect_ratio", body);
        AssertNoProperty("resolution", body);
    }

    [Fact]
    [UpstreamTest(Id + "should return empty warnings for supported features", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnEmptyWarningsForSupportedFeatures()
    {
        var result = await Start(new XaiVideoServer(), Call());

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should warn when n > 1", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnWhenNIsGreaterThan1()
    {
        var result = await Start(new XaiVideoServer(), Call(n: 3));

        Assert.True(HasWarning(result.Warnings, "n"));
    }

    [Fact]
    [UpstreamTest(Id + "should not warn when n is 1", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnWhenNIs1()
    {
        var result = await Start(new XaiVideoServer(), Call(n: 1));

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "n");
    }

    [Fact]
    [UpstreamTest(Id + "should allow duration in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAllowDurationInExtensionMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(duration: 6, xai: ExtendMode));

        AssertMatches("{\"duration\":6}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about aspectRatio in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutAspectRatioInExtensionMode()
    {
        var result = await Start(new XaiVideoServer(), Call(aspectRatio: "16:9", xai: ExtendMode));

        Assert.True(HasWarning(result.Warnings, "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn about resolution in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutResolutionInExtensionMode()
    {
        var result = await Start(new XaiVideoServer(), Call(resolution: "1280x720", xai: ExtendMode));

        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should omit aspect_ratio and resolution from body in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitAspectRatioAndResolutionFromBodyInExtensionMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(aspectRatio: "16:9", resolution: "1280x720", xai: ExtendMode));

        var body = RequestBody(server);
        AssertNoProperty("aspect_ratio", body);
        AssertNoProperty("resolution", body);
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about duration in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAboutDurationInExtensionMode()
    {
        var result = await Start(new XaiVideoServer(), Call(duration: 6, xai: ExtendMode));

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "duration");
    }

    [Fact]
    [UpstreamTest(Id + "should warn about provider-level resolution in extension mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAboutProviderLevelResolutionInExtensionMode()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"mode\":\"extend-video\",\"videoUrl\":\"" + SourceVideo + "\",\"resolution\":\"720p\"}"));

        AssertNoProperty("resolution", RequestBody(server));
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should send reference_images array to /videos/generations with explicit mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendReferenceImagesArrayToVideosGenerationsWithExplicitMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\",\"https://example.com/ref2.jpg\"]}"));

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal(TestBaseUrl + "/videos/generations", server.Calls[0].Url);
        AssertMatches("{\"reference_images\":[{\"url\":\"https://example.com/ref1.jpg\"},{\"url\":\"https://example.com/ref2.jpg\"}]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should downgrade R2V 1080p to 720p with a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldDowngradeR2V1080pTo720pWithAWarning()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"resolution\":\"1080p\"}"), Grok15);

        AssertMatches("{\"resolution\":\"720p\"}", RequestBody(server));
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should downgrade R2V 1920x1080 from the SDK resolution to 720p", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldDowngradeR2V1920x1080FromTheSdkResolutionTo720p()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(resolution: "1920x1080", xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"]}"), Grok15);

        AssertMatches("{\"resolution\":\"720p\"}", RequestBody(server));
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(Id + "should separate image and audio inputReferences", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSeparateImageAndAudioInputReferences()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(inputReferences: [VideoModelFile.FromUrl("https://example.com/ref1.jpg"), Voice]), Grok15);

        var body = RequestBody(server);
        AssertMatches("{\"reference_images\":[{\"url\":\"https://example.com/ref1.jpg\"}],\"reference_audios\":[{\"url\":\"https://example.com/voice.mp3\"}]}", body);
        Assert.Single(body["reference_images"]!.AsArray());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should support audio-only reference-to-video", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSupportAudioOnlyReferenceToVideo()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(inputReferences: [Voice]), Grok15);

        var body = RequestBody(server);
        AssertMatches("{\"reference_audios\":[{\"url\":\"https://example.com/voice.mp3\"}]}", body);
        AssertNoProperty("reference_images", body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should not send an empty reference_images array for video-only inputReferences", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotSendAnEmptyReferenceImagesArrayForVideoOnlyInputReferences()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(inputReferences: [VideoModelFile.FromUrl("https://example.com/clip.mp4", "video/mp4")]), Grok15);

        AssertNoProperty("reference_images", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should combine a pinned first frame with an audio reference", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldCombineAPinnedFirstFrameWithAnAudioReference()
    {
        var server = new XaiVideoServer();

        var result = await Start(
            server,
            Call(image: VideoModelFile.FromUrl("https://example.com/start.jpg", "image/jpeg"), inputReferences: [Voice]),
            Grok15);

        var body = RequestBody(server);
        AssertMatches("{\"image\":{\"url\":\"https://example.com/start.jpg\"},\"reference_audios\":[{\"url\":\"https://example.com/voice.mp3\"}]}", body);
        AssertNoProperty("reference_images", body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should support explicit audio-only R2V without reference_images", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSupportExplicitAudioOnlyR2VWithoutReferenceImages()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(inputReferences: [Voice], xai: "{\"mode\":\"reference-to-video\"}"), Grok15);

        var body = RequestBody(server);
        AssertNoProperty("reference_images", body);
        AssertMatches("{\"reference_audios\":[{\"url\":\"https://example.com/voice.mp3\"}]}", body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should warn when explicit R2V has no references at all", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnWhenExplicitR2VHasNoReferencesAtAll()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"mode\":\"reference-to-video\"}"), Grok15);

        AssertNoProperty("reference_images", RequestBody(server));
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "referenceImages" && warning.Details!.Contains("without reference images"));
    }

    [Fact]
    [UpstreamTest(Id + "should fallback to reference-to-video mode when referenceImageUrls is set without mode", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldFallbackToReferenceToVideoModeWhenReferenceImageUrlsIsSetWithoutMode()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"]}"));

        Assert.Equal(TestBaseUrl + "/videos/generations", server.Calls[0].Url);
        AssertMatches("{\"reference_images\":[{\"url\":\"https://example.com/ref1.jpg\"}]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should allow duration and aspectRatio with reference images", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAllowDurationAndAspectRatioWithReferenceImages()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(duration: 8, aspectRatio: "16:9", xai: "{\"referenceImageUrls\":[\"https://example.com/ref.jpg\"]}"));

        AssertMatches("{\"duration\":8,\"aspect_ratio\":\"16:9\"}", RequestBody(server));
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "duration");
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Id + "should send reference_audios for preset reference voices", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendReferenceAudiosForPresetReferenceVoices()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[\"eve\"]}"), Grok15);

        Assert.Equal(TestBaseUrl + "/videos/generations", server.Calls[0].Url);
        AssertMatches("{\"reference_images\":[{\"url\":\"https://example.com/ref1.jpg\"}],\"reference_audios\":[{\"voice_id\":\"eve\"}]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send up to 3 preset reference voices in order", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendUpTo3PresetReferenceVoicesInOrder()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[\"eve\",\"leo\",\"rex\"]}"), Grok15);

        AssertMatches("{\"reference_audios\":[{\"voice_id\":\"eve\"},{\"voice_id\":\"leo\"},{\"voice_id\":\"rex\"}]}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should reject more than 3 preset reference voices", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectMoreThan3PresetReferenceVoices()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() => Start(
            new XaiVideoServer(),
            Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[\"ara\",\"eve\",\"leo\",\"rex\"]}"),
            Grok15));
    }

    [Fact]
    [UpstreamTest(Id + "should reject empty-string preset reference voice ids", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectEmptyStringPresetReferenceVoiceIds()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() => Start(
            new XaiVideoServer(),
            Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[\"\"]}"),
            Grok15));
    }

    [Fact]
    [UpstreamTest(Id + "should omit reference_audios for an empty referenceVoiceIds array", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitReferenceAudiosForAnEmptyReferenceVoiceIdsArray()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[]}"), Grok15);

        AssertNoProperty("reference_audios", RequestBody(server));
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "referenceVoiceIds");
    }

    [Fact]
    [UpstreamTest(Id + "should warn and omit referenceVoiceIds outside reference-to-video", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAndOmitReferenceVoiceIdsOutsideReferenceToVideo()
    {
        var server = new XaiVideoServer();

        var result = await Start(server, Call(xai: "{\"referenceVoiceIds\":[\"eve\"]}"), Grok15);

        AssertNoProperty("reference_audios", RequestBody(server));
        Assert.True(HasWarning(result.Warnings, "referenceVoiceIds"));
    }

    [Fact]
    [UpstreamTest(Id + "should never forward referenceVoiceIds as a raw body key", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNeverForwardReferenceVoiceIdsAsARawBodyKey()
    {
        var server = new XaiVideoServer();

        await Start(server, Call(xai: "{\"mode\":\"reference-to-video\",\"referenceImageUrls\":[\"https://example.com/ref1.jpg\"],\"referenceVoiceIds\":[\"eve\"]}"), Grok15);

        AssertNoProperty("referenceVoiceIds", RequestBody(server));
    }

    private static Task<VideoStartResult> Start(XaiVideoServer server, VideoModelCall call, string modelId = "grok-imagine-video")
    {
        return CreateModel(server, modelId: modelId).DoStartAsync(call, CancellationToken.None);
    }
}
