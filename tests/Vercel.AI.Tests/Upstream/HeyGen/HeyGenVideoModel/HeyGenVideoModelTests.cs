// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using HeyGenProviderType = Vercel.AI.HeyGen.HeyGenProvider;
using HeyGenVideoModelType = Vercel.AI.HeyGen.HeyGenVideoModel;
using static Vercel.AI.Tests.Upstream.HeyGen.HeyGenVideoModel.HeyGenVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenVideoModel;

/// <summary>Port of <c>heygen-video-model.test.ts</c> &gt; <c>HeyGenVideoModel</c>.</summary>
public sealed class HeyGenVideoModelTests
{
    private const string Prefix = "packages/heygen/src/heygen-video-model.test.ts::HeyGenVideoModel::";

    [Fact]
    [UpstreamTest(Prefix + "submits text-to-video with explicit defaults and returns an operation", Coverage = UpstreamCoverage.Covered)]
    public async Task Submits_text_to_video_with_explicit_defaults_and_returns_an_operation()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler).DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(server.RequestBody(), "{\"model\":\"heygen-video-1\",\"mode\":\"text_to_video\",\"prompt\":\"" + PromptText + "\",\"duration\":5,\"resolution\":\"768p\",\"aspect_ratio\":\"16:9\"}");
        Assert.Equal("test-key", server.Calls[0].Header("x-api-key"));
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, "{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\"}");
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"heygen\":{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\"}}");
        Assert.Equal("heygen-video-1", result.Response.ModelId);
        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards supported generation options and idempotency headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_supported_generation_options_and_idempotency_headers()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(
            Start(duration: 15, seed: 0, aspectRatio: "9:16", headers: new Dictionary<string, string> { ["Idempotency-Key"] = "generation-1" }, heygen: "{\"resolution\":\"2k\",\"promptEnhancement\":\"disabled\"}"),
            CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"duration\":15,\"seed\":0,\"aspect_ratio\":\"9:16\",\"resolution\":\"2k\",\"prompt_enhancement\":\"disabled\"}");
        Assert.Equal("generation-1", server.Calls[0].Header("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses an input image as the first frame and omits aspect_ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_an_input_image_as_the_first_frame_and_omits_aspect_ratio()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler).DoStartAsync(Start(image: Image, aspectRatio: "1:1"), CancellationToken.None).ConfigureAwait(false);
        var body = server.RequestBody();
        Matches(body, "{\"mode\":\"image_to_video\",\"image\":{\"type\":\"url\",\"url\":\"" + Image.Url + "\"}}");
        Assert.False(body.ContainsKey("aspect_ratio"));
        Assert.Equal("aspectRatio", Assert.Single(result.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts a first-frame image at a high resolution without an explicit aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_first_frame_image_at_a_high_resolution_without_an_explicit_aspect_ratio()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(Start(frames: Frames((Image, "first_frame")), heygen: "{\"resolution\":\"1080p\"}"), CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"mode\":\"image_to_video\",\"resolution\":\"1080p\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "encodes file bytes and preserves existing base64", Coverage = UpstreamCoverage.Covered, Note = "A .NET file carries bytes only, so existing base64 is passed as a provider reference asset.")]
    public async Task Encodes_file_bytes_and_preserves_existing_base64()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(
            Start(references: new[] { VideoModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png") }, heygen: "{\"referenceImages\":[{\"type\":\"base64\",\"mediaType\":\"image/jpeg\",\"data\":\"BAUG\"}]}"),
            CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"mode\":\"reference_to_video\",\"aspect_ratio\":\"adaptive\",\"reference_images\":[{\"type\":\"base64\",\"media_type\":\"image/png\",\"data\":\"AQID\"},{\"type\":\"base64\",\"media_type\":\"image/jpeg\",\"data\":\"BAUG\"}]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "groups mixed references by media type and appends provider references in order", Coverage = UpstreamCoverage.Covered)]
    public async Task Groups_mixed_references_by_media_type_and_appends_provider_references_in_order()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(
            Start(
                references: new[] { Video, Image, Audio },
                heygen: "{\"referenceImages\":[{\"type\":\"asset_id\",\"assetId\":\"image-2\"}],\"referenceVideos\":[{\"type\":\"asset_id\",\"assetId\":\"video-2\"}],\"referenceAudio\":[{\"type\":\"base64\",\"mediaType\":\"audio/mpeg\",\"data\":\"AQID\"}]}"),
            CancellationToken.None).ConfigureAwait(false);
        Matches(
            server.RequestBody(),
            "{\"reference_images\":[{\"type\":\"url\",\"url\":\"" + Image.Url + "\"},{\"type\":\"asset_id\",\"asset_id\":\"image-2\"}],"
            + "\"reference_videos\":[{\"type\":\"url\",\"url\":\"" + Video.Url + "\"},{\"type\":\"asset_id\",\"asset_id\":\"video-2\"}],"
            + "\"reference_audio\":[{\"type\":\"url\",\"url\":\"" + Audio.Url + "\"},{\"type\":\"base64\",\"media_type\":\"audio/mpeg\",\"data\":\"AQID\"}]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts an existing asset as the first frame", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_an_existing_asset_as_the_first_frame()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(Start(heygen: "{\"image\":{\"type\":\"asset_id\",\"assetId\":\"first-frame\"}}"), CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"mode\":\"image_to_video\",\"image\":{\"type\":\"asset_id\",\"asset_id\":\"first-frame\"}}");
    }

    [Theory]
    [InlineData("960x416", "480p", "21:9")]
    [InlineData("1344x768", "768p", "16:9")]
    [InlineData("768x1344", "768p", "9:16")]
    [InlineData("1080x1890", "1080p", "9:16")]
    [InlineData("2688x1536", "2k", "16:9")]
    [UpstreamTest(Prefix + "maps the documented frame size %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_the_documented_frame_size(string resolution, string tier, string ratio)
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(Start(resolution: resolution), CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"resolution\":\"" + tier + "\",\"aspect_ratio\":\"" + ratio + "\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "gives the provider resolution precedence over the standard resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Gives_the_provider_resolution_precedence_over_the_standard_resolution()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStartAsync(Start(resolution: "1920x1080", heygen: "{\"resolution\":\"480p\"}"), CancellationToken.None).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"resolution\":\"480p\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "warns for unsupported sample count, fps, and disabling audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_for_unsupported_sample_count_fps_and_disabling_audio()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler).DoStartAsync(Start(n: 2, fps: 30, generateAudio: false), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new[] { "n", "fps", "generateAudio" }, result.Warnings.Select(warning => warning.Type == "unsupported" ? warning.Feature : null));
        var body = server.RequestBody();
        Assert.False(body.ContainsKey("fps"));
        Assert.False(body.ContainsKey("generate_audio"));
        Assert.False(body.ContainsKey("n"));
    }

    [Theory]
    [InlineData("missing prompt")]
    [InlineData("empty prompt")]
    [InlineData("long prompt")]
    [InlineData("short duration")]
    [InlineData("long duration")]
    [InlineData("fractional duration")]
    [InlineData("negative seed")]
    [InlineData("unknown resolution")]
    [InlineData("text adaptive ratio")]
    [InlineData("unsupported ratio")]
    [InlineData("high resolution square")]
    [InlineData("high resolution adaptive references")]
    [InlineData("audio-only references")]
    [InlineData("untyped reference")]
    [InlineData("insecure URL")]
    [InlineData("video as image")]
    [InlineData("last frame")]
    [InlineData("multiple first frames")]
    [InlineData("two first-frame sources")]
    [InlineData("image and references")]
    [InlineData("two image sources")]
    [InlineData("image mode without image")]
    [InlineData("reference mode without references")]
    [InlineData("text mode with image")]
    [InlineData("text mode with references")]
    [InlineData("too many images")]
    [InlineData("too many videos")]
    [InlineData("too many audio references")]
    [InlineData("too many combined references")]
    [InlineData("combined image limit")]
    [InlineData("wrong reference media type")]
    [UpstreamTest(Prefix + "rejects %s before submitting", Coverage = UpstreamCoverage.Partial, Note = "The large and fractional seed cases are not representable: the seed is an int.")]
    public async Task Rejects_before_submitting(string name)
    {
        var server = new Server();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => CreateModel(server.Handler).DoStartAsync(Overrides(name), CancellationToken.None)).ConfigureAwait(false);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "validates provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_provider_options()
    {
        var server = new Server();
        await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler).DoStartAsync(Start(heygen: "{\"promptEnhancement\":\"unknown\"}"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Empty(server.Calls);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("processing")]
    [UpstreamTest(Prefix + "maps %s to pending", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_status_to_pending(string status)
    {
        var server = new Server { StatusBody = "{\"data\":{\"status\":\"" + status + "\"}}" };
        Assert.Equal("pending", (await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false)).Status);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns the signed video URL and reported metadata without downloading", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_signed_video_URL_and_reported_metadata_without_downloading()
    {
        var server = new Server();
        var result = await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal(("url", "https://resource2.heygen.ai/video.mp4", "video/mp4"), (video.Type, video.Url, video.MediaType));
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"heygen\":{\"videos\":[{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\",\"duration\":5,\"width\":1344,\"height\":768,\"aspectRatio\":\"16:9\",\"seed\":0}]}}");
        Assert.Single(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns reported output duration and timing without substituting request settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_reported_output_duration_and_timing_without_substituting_request_settings()
    {
        var server = new Server();
        var started = await CreateModel(server.Handler).DoStartAsync(Start(duration: 5), CancellationToken.None).ConfigureAwait(false);
        server.StatusBody = "{\"data\":{\"status\":\"completed\",\"video_url\":\"https://resource2.heygen.ai/video.mp4\",\"duration\":5.042,\"width\":1344,\"height\":768,\"aspect_ratio\":\"16:9\",\"seed\":0,\"timings\":{\"inference\":3.7}}}";
        var call = new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson_Parse("{}"), new Dictionary<string, string>(), CancellationToken.None, operation: started.Operation);
        var result = await CreateModel(server.Handler).DoStatusAsync(call, CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"heygen\":{\"videos\":[{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\",\"duration\":5.042,\"width\":1344,\"height\":768,\"aspectRatio\":\"16:9\",\"seed\":0,\"timings\":{\"inference\":3.7}}]}}");
        var heygen = result.ProviderMetadata!.Value.GetProperty("heygen");
        Assert.False(heygen.TryGetProperty("credits", out _));
        Assert.False(heygen.TryGetProperty("tokens", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves reported zero timing values and failure metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_reported_zero_timing_values_and_failure_metadata()
    {
        var server = new Server { StatusBody = "{\"data\":{\"status\":\"failed\",\"failure_code\":\"generation_failed\",\"timings\":{\"inference\":0}}}" };
        var result = await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("error", result.Status);
        Matches(Node(result.ProviderMetadata), "{\"heygen\":{\"timings\":{\"inference\":0},\"failureCode\":\"generation_failed\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts absent or null optional result metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_absent_or_null_optional_result_metadata()
    {
        var server = new Server { StatusBody = "{\"data\":{\"status\":\"completed\",\"video_url\":\"https://resource2.heygen.ai/video.mp4\",\"duration\":null,\"width\":null}}" };
        var result = await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("completed", result.Status);
        Matches(Node(result.ProviderMetadata), "{\"heygen\":{\"videos\":[" + OperationText + "]}}");
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("cancelled")]
    [UpstreamTest(Prefix + "maps %s to an error with provider details", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_status_to_an_error_with_provider_details(string status)
    {
        var server = new Server { StatusBody = "{\"data\":{\"status\":\"" + status + "\",\"failure_code\":\"generation_failed\",\"failure_message\":\"Generation did not finish.\"}}" };
        var result = await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("error", result.Status);
        Assert.Equal("Generation did not finish. (generation_failed)", result.Error);
        Matches(Node(result.ProviderMetadata), "{\"heygen\":{\"failureCode\":\"generation_failed\"}}");
    }

    [Theory]
    [InlineData("{\"status\":\"completed\"}")]
    [InlineData("{\"status\":\"completed\",\"video_url\":\"\"}")]
    [InlineData("{\"status\":\"unexpected\"}")]
    [UpstreamTest(Prefix + "rejects an unusable status response: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_unusable_status_response(string data)
    {
        var server = new Server { StatusBody = "{\"data\":" + data + "}" };
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{}")]
    [InlineData("{\"videoId\":\"\",\"mode\":\"text_to_video\",\"resolution\":\"768p\"}")]
    [UpstreamTest(Prefix + "validates operation references: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_operation_references(string? invalidOperation)
    {
        var server = new Server();
        await Assert.ThrowsAnyAsync<Exception>(() => CreateModel(server.Handler).DoStatusAsync(Status(invalidOperation), CancellationToken.None)).ConfigureAwait(false);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "refreshes the result URL on repeated status calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Refreshes_the_result_URL_on_repeated_status_calls()
    {
        var server = new Server();
        await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        const string refreshed = "https://resource2.heygen.ai/video.mp4?signature=new";
        server.StatusBody = "{\"data\":{\"status\":\"completed\",\"video_url\":\"" + refreshed + "\",\"duration\":5}}";
        var result = await CreateModel(server.Handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(refreshed, Assert.Single(result.Videos).Url);
    }

    [Theory]
    [InlineData("start")]
    [InlineData("status")]
    [UpstreamTest(Prefix + "preserves API errors on %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_API_errors(string method)
    {
        const string error = "{\"error\":{\"code\":\"invalid_parameter\",\"message\":\"Invalid duration\",\"param\":\"duration\"}}";
        var server = new Server { CreateStatus = 400, CreateBody = error, StatusStatus = 400, StatusBody = error };
        var model = CreateModel(server.Handler);
        var failure = await Assert.ThrowsAsync<BadRequestException>(
            () => method == "start" ? model.DoStartAsync(Start(), CancellationToken.None) : model.DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal(400, failure.StatusCode);
        Assert.Equal("Invalid duration (invalid_parameter) [duration]", failure.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects malformed create responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_create_responses()
    {
        var server = new Server { CreateBody = "{\"data\":{\"status\":\"pending\"}}" };
        await Assert.ThrowsAsync<ApiException>(() => CreateModel(server.Handler).DoStartAsync(Start(), CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes abort signals through the configured fetch", Coverage = UpstreamCoverage.Covered, Note = "The handler sees a token linked to the call's token, so it asserts cancellation reaches it instead of reference equality.")]
    public async Task Passes_abort_signals_through_the_configured_fetch()
    {
        using var controller = new CancellationTokenSource();
        var handler = new AbortingHandler(controller);
        var provider = HeyGenProviderType.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, handler);
        var model = new HeyGenVideoModelType(provider, "heygen-video-1");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoStartAsync(Start(cancellationToken: controller.Token), CancellationToken.None)).ConfigureAwait(false);
        Assert.True(handler.Token.IsCancellationRequested);
        Assert.Equal(1, handler.Calls);
    }

    private static VideoModelCall Overrides(string name)
    {
        return name switch
        {
            "missing prompt" => Start(prompt: null),
            "empty prompt" => Start(prompt: string.Empty),
            "long prompt" => Start(prompt: new string('x', 32001)),
            "short duration" => Start(duration: 4),
            "long duration" => Start(duration: 16),
            "fractional duration" => Start(duration: 5.5),
            "negative seed" => Start(seed: -1),
            "unknown resolution" => Start(resolution: "1920x1080"),
            "text adaptive ratio" => Start(aspectRatio: "adaptive"),
            "unsupported ratio" => Start(aspectRatio: "2:1"),
            "high resolution square" => Start(aspectRatio: "1:1", heygen: "{\"resolution\":\"2k\"}"),
            "high resolution adaptive references" => Start(references: new[] { Image }, heygen: "{\"resolution\":\"1080p\"}"),
            "audio-only references" => Start(references: new[] { Audio }),
            "untyped reference" => Start(references: new[] { VideoModelFile.FromUrl(Image.Url!) }),
            "insecure URL" => Start(image: VideoModelFile.FromUrl("http://example.com/image.png")),
            "video as image" => Start(image: Video),
            "last frame" => Start(frames: Frames((Image, "last_frame"))),
            "multiple first frames" => Start(frames: Frames((Image, "first_frame"), (Image, "first_frame"))),
            "two first-frame sources" => Start(image: Image, frames: Frames((Image, "first_frame"))),
            "image and references" => Start(image: Image, references: new[] { Video }),
            "two image sources" => Start(image: Image, heygen: "{\"image\":{\"type\":\"asset_id\",\"assetId\":\"another\"}}"),
            "image mode without image" => Start(heygen: "{\"mode\":\"image_to_video\"}"),
            "reference mode without references" => Start(heygen: "{\"mode\":\"reference_to_video\"}"),
            "text mode with image" => Start(image: Image, heygen: "{\"mode\":\"text_to_video\"}"),
            "text mode with references" => Start(references: new[] { Image }, heygen: "{\"mode\":\"text_to_video\"}"),
            "too many images" => Start(references: Repeat(Image, 10)),
            "too many videos" => Start(references: Repeat(Video, 4)),
            "too many audio references" => Start(references: new[] { Image }.Concat(Repeat(Audio, 4)).ToList()),
            "too many combined references" => Start(references: Repeat(Image, 9).Concat(Repeat(Video, 3)).Concat(new[] { Audio }).ToList()),
            "combined image limit" => Start(references: Repeat(Image, 9), heygen: "{\"referenceImages\":[{\"type\":\"asset_id\",\"assetId\":\"extra\"}]}"),
            "wrong reference media type" => Start(heygen: "{\"referenceImages\":[{\"type\":\"base64\",\"mediaType\":\"audio/mpeg\",\"data\":\"AQID\"}]}"),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };
    }

    // Waits until the call's token cancels, like a fetch that is aborted mid-request.
    private sealed class AbortingHandler : HttpMessageHandler
    {
        private readonly CancellationTokenSource _controller;

        public AbortingHandler(CancellationTokenSource controller)
        {
            _controller = controller;
        }

        public CancellationToken Token { get; private set; }

        public int Calls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Token = cancellationToken;
            _controller.Cancel();
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("unreachable");
        }
    }
}
