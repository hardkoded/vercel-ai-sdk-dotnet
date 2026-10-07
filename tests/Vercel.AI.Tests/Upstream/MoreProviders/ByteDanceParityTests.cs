// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ByteDance;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>ByteDance Seedream image and Seedance video models matched to the upstream catalog.</summary>
public sealed class ByteDanceParityTests
{
    private const string Image = "packages/bytedance/src/bytedance-image-model.test.ts::ByteDanceImageModel > ";
    private const string Video = "packages/bytedance/src/bytedance-video-model.test.ts::ByteDanceVideoModel > ";
    private const string ImagePrompt = "A salamander in a forest pond at dusk surrounded by fireflies";
    private const string ImageModelId = "seedream-5-0-260128";
    private const string VideoPrompt = "A futuristic city with flying cars";
    private const string VideoModelId = "seedance-1-0-pro-250528";
    private const string TasksUrl = "https://ark.ap-southeast.bytepluses.com/api/v3/contents/generations/tasks";
    private const string TaskId = "test-task-id-123";
    private const string TextPart = "{\"type\":\"text\",\"text\":\"" + VideoPrompt + "\"}";
    private const string Succeeded = "{\"id\":\"test-task-id-123\",\"model\":\"seedance-1-0-pro-250528\",\"status\":\"succeeded\",\"content\":{\"video_url\":\"https://bytedance.cdn/files/video-output.mp4\",\"last_frame_url\":\"https://bytedance.cdn/files/video-output-last-frame.png\"},\"usage\":{\"completion_tokens\":100}}";
    private const string PngDataUri = "data:image/png;base64,iVBORw==";
    private const string DeprecatedPollMessage = "poll: { intervalMs, timeoutMs }";

    private static readonly DateTimeOffset TestDate = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly byte[] PngBytes = { 137, 80, 78, 71 };

    [Fact]
    [UpstreamTest(Image + "constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_exposes_provider_model_and_limit()
    {
        var model = ImageModel(new ParityHandler());
        Assert.Equal("bytedance.image", model.Provider);
        Assert.Equal(ImageModelId, model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should send a JSON text-to-image request to /images/generations", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_request_posts_json_to_images_generations()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(size: "2048x2048", options: "{\"watermark\":false}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.example.com/images/generations", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler), "{\"model\":\"" + ImageModelId + "\",\"prompt\":\"" + ImagePrompt + "\",\"size\":\"2048x2048\",\"watermark\":false,\"response_format\":\"b64_json\"}");
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should map typed provider options to ByteDance request fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_options_map_to_request_fields()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(options: "{\"sequentialImageGeneration\":\"auto\",\"maxImages\":4,\"outputFormat\":\"png\",\"optimizePromptMode\":\"fast\"}"), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"model\":\"" + ImageModelId + "\",\"prompt\":\"" + ImagePrompt + "\",\"sequential_image_generation\":\"auto\",\"sequential_image_generation_options\":{\"max_images\":4},\"output_format\":\"png\",\"optimize_prompt_options\":{\"mode\":\"fast\"},\"response_format\":\"b64_json\"}");
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should let a resolution-level size override the top-level pixel size", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_resolution_level_overrides_size()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(size: "2048x2048", options: "{\"size\":\"2K\"}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("2K", Body(handler)["size"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should pass through unknown provider options unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_unknown_options_pass_through()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(options: "{\"seed\":42}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(42, Body(handler)["seed"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should warn for unsupported settings (aspectRatio, seed, mask)", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_warns_for_aspect_ratio_seed_and_mask()
    {
        var call = ImageCall(aspectRatio: "16:9", seed: 123, mask: ImageModelFile.FromFile(new byte[] { 1 }, "image/png"));
        var result = await ImageModel(ImageHandler()).DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new[] { "aspectRatio", "seed", "mask" }, result.Warnings.Select(warning => warning.Feature));
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should handle API errors", Coverage = UpstreamCoverage.Partial, Note = "Checks the message and status code. ApiException has no request URL.")]
    public async Task Image_api_errors_keep_message_and_status()
    {
        var handler = new ParityHandler(_ => Error(HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Invalid prompt content\",\"code\":\"InvalidParameter\"}}"));
        var error = await Assert.ThrowsAsync<BadRequestException>(() => ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Invalid prompt content", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should return the raw b64_json content", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_returns_raw_base64()
    {
        var result = await ImageModel(ImageHandler()).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new object?[] { "test1234", "test5678" }, result.Images);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_response_has_timestamp_headers_and_model()
    {
        var result = await ImageModel(ImageHandler(), () => TestDate).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(TestDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal(ImageModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate > usage::should map Ark token usage, leaving inputTokens undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_usage_has_output_and_total_tokens()
    {
        var handler = ImageHandler("{\"data\":[{\"b64_json\":\"test1234\"}],\"usage\":{\"generated_images\":1,\"output_tokens\":4096,\"total_tokens\":4096}}");
        var usage = (await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false)).Usage!;
        AssertImageUsage(usage, 4096, 4096);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate > usage::should not map generated_images into usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_usage_ignores_generated_images()
    {
        var handler = ImageHandler("{\"data\":[{\"b64_json\":\"test1234\"},{\"b64_json\":\"test5678\"}],\"usage\":{\"generated_images\":2,\"output_tokens\":8192,\"total_tokens\":8192}}");
        var result = await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        AssertImageUsage(result.Usage!, 8192, 8192);
        Assert.Equal(2, result.Images.Count);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate > usage::should return undefined usage when Ark omits it", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_usage_is_null_without_usage()
    {
        Assert.Null((await ImageModel(ImageHandler()).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false)).Usage);
    }

    [Fact]
    [UpstreamTest(Image + "doGenerate > usage::should map null token fields to undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_usage_keeps_null_tokens_unset()
    {
        var handler = ImageHandler("{\"data\":[{\"b64_json\":\"test1234\"}],\"usage\":{\"generated_images\":1,\"output_tokens\":null,\"total_tokens\":null}}");
        var usage = (await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false)).Usage!;
        AssertImageUsage(usage, null, null);
    }

    [Fact]
    [UpstreamTest(Image + "image editing::should send a single input image as the `image` field on /images/generations", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_edit_sends_one_file_as_a_string()
    {
        var handler = ImageHandler();
        var call = ImageCall(prompt: "Change the salamander to a snow weasel", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png") });
        var result = await ImageModel(handler).DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.example.com/images/generations", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler), "{\"model\":\"" + ImageModelId + "\",\"prompt\":\"Change the salamander to a snow weasel\",\"image\":\"" + PngDataUri + "\",\"response_format\":\"b64_json\"}");
        Assert.Equal(new object?[] { "test1234", "test5678" }, result.Images);
    }

    [Fact]
    [UpstreamTest(Image + "image editing::should send multiple input images as an array in the `image` field", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_edit_sends_several_files_as_an_array()
    {
        var handler = ImageHandler();
        var file = ImageModelFile.FromFile(PngBytes, "image/png");
        await ImageModel(handler).DoGenerateAsync(ImageCall(prompt: "Combine these images", files: new[] { file, file }), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), "{\"model\":\"" + ImageModelId + "\",\"prompt\":\"Combine these images\",\"image\":[\"" + PngDataUri + "\",\"" + PngDataUri + "\"],\"response_format\":\"b64_json\"}");
    }

    [Fact]
    [UpstreamTest(Image + "image editing::should pass a URL input image through unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_edit_passes_url_files_through()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(prompt: "Edit this", files: new[] { ImageModelFile.FromUrl("https://example.com/input.png") }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://example.com/input.png", Body(handler)["image"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Video + "constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Video_model_exposes_provider_model_and_limit()
    {
        var model = VideoModel(new ParityHandler());
        Assert.Equal("bytedance.video", model.Provider);
        Assert.Equal(VideoModelId, model.ModelId);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Video + "constructor::should support different model IDs", Coverage = UpstreamCoverage.Covered)]
    public void Video_model_keeps_other_model_ids()
    {
        Assert.Equal("seedance-1-5-pro-251215", VideoModel(new ParityHandler(), "seedance-1-5-pro-251215").ModelId);
    }

    [Fact]
    [UpstreamTest(Video + "constructor::should support custom model IDs", Coverage = UpstreamCoverage.Covered)]
    public void Video_model_keeps_custom_model_ids()
    {
        Assert.Equal("custom-model-id", VideoModel(new ParityHandler(), "custom-model-id").ModelId);
    }

    [Fact]
    [UpstreamTest(Video + "webhooks::should leave the generic webhook hook undefined", Coverage = UpstreamCoverage.Covered)]
    public void Video_model_has_no_webhook_handler()
    {
        Assert.False(VideoModel(new ParityHandler()).CanWebhook);
    }

    [Theory]
    [InlineData("https://example.com/webhook", null, "https://example.com/webhook")]
    [InlineData(null, null, null)]
    [InlineData(null, "https://example.com/raw", "https://example.com/raw")]
    [InlineData("https://example.com/webhook", "https://example.com/raw", "https://example.com/webhook")]
    [UpstreamTest(Video + "webhooks::should submit $name", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_submits_the_callback_url(string? webhookUrl, string? rawUrl, string? expected)
    {
        var handler = VideoHandler();
        var options = rawUrl == null ? "{}" : "{\"callback_url\":\"" + rawUrl + "\"}";
        await VideoModel(handler).DoStartAsync(Start(options: options, webhookUrl: webhookUrl), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(expected, Body(handler)["callback_url"]?.GetValue<string>());
        Assert.Equal(expected != null, Body(handler).ContainsKey("callback_url"));
    }

    [Theory]
    [InlineData("", null, null, null, null)]
    [InlineData(",\"seed\":42", null, null, null, 42)]
    [InlineData(",\"ratio\":\"16:9\"", "16:9", null, null, null)]
    [InlineData(",\"ratio\":\"adaptive\"", "adaptive", null, null, null)]
    [InlineData(",\"duration\":5", null, null, 5.0, null)]
    [InlineData(",\"resolution\":\"1080p\"", null, "1920x1080", null, null)]
    [InlineData(",\"resolution\":\"720p\"", null, "1280x720", null, null)]
    [InlineData(",\"resolution\":\"480p\"", null, "864x480", null, null)]
    [InlineData(",\"resolution\":\"640x480\"", null, "640x480", null, null)]
    [UpstreamTest(Video + "doStart::should pass the correct parameters including prompt", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should pass seed when provided", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should pass aspect ratio when provided", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should pass an adaptive aspect ratio through unchanged", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should pass duration when provided", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should map WxH resolution to API format", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should map 720p resolution correctly", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should map 480p resolution correctly", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStart::should pass through unmapped resolution values", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_maps_call_settings(string fields, string? aspectRatio, string? resolution, double? duration, int? seed)
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoStartAsync(Start(aspectRatio: aspectRatio, resolution: resolution, duration: duration, seed: seed), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), StartBody(fields));
    }

    [Fact]
    [UpstreamTest(Video + "doStart::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_sends_provider_and_call_headers()
    {
        var handler = VideoHandler();
        var model = VideoModel(handler, configure: options => options.Headers["Custom-Provider-Header"] = "provider-header-value");
        await model.DoStartAsync(Start(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest(Video + "doStart::should return operation with taskId", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_returns_the_task_id_operation()
    {
        var result = await VideoModel(VideoHandler()).DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Assert.IsType<JsonElement>(result.Operation), "{\"taskId\":\"test-task-id-123\"}");
    }

    [Fact]
    [UpstreamTest(Video + "doStart::should return warnings array", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should not warn when no legacy poll options are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_has_no_warnings_by_default()
    {
        Assert.Empty((await VideoModel(VideoHandler()).DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false)).Warnings);
    }

    [Fact]
    [UpstreamTest(Video + "warnings::should warn when fps is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_warns_for_fps()
    {
        var result = await VideoModel(VideoHandler()).DoStartAsync(Start(fps: 30), CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "fps" && warning.Details == "ByteDance video models do not support custom FPS. Frame rate is fixed at 24 fps.");
    }

    [Fact]
    [UpstreamTest(Video + "warnings::should warn when n > 1", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_warns_for_several_videos()
    {
        var result = await VideoModel(VideoHandler()).DoStartAsync(Start(n: 3), CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "n" && warning.Details == "ByteDance video models do not support generating multiple videos per call. Only 1 video will be generated.");
    }

    [Fact]
    [UpstreamTest(Video + "response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_response_has_timestamp_headers_and_model()
    {
        var result = await VideoModel(VideoHandler(), clock: () => TestDate).DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);
        AssertResponse(result.Response);
    }

    [Fact]
    [UpstreamTest(Video + "providerMetadata::should include task ID, usage, and last frame URL in completed status", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_metadata_has_task_usage_and_last_frame()
    {
        var result = await VideoModel(VideoHandler()).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("completed", result.Status);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"bytedance\":{\"taskId\":\"test-task-id-123\",\"usage\":{\"completion_tokens\":100},\"lastFrameUrl\":\"https://bytedance.cdn/files/video-output-last-frame.png\"}}");
    }

    [Fact]
    [UpstreamTest(Video + "Image-to-Video::should send image_url with file data", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_sends_file_images_as_data_uris()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoStartAsync(Start(image: VideoModelFile.FromFile(PngBytes, "image/png")), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), "{\"model\":\"" + VideoModelId + "\",\"content\":[" + TextPart + ",{\"type\":\"image_url\",\"image_url\":{\"url\":\"" + PngDataUri + "\"}}]}");
    }

    [Fact]
    [UpstreamTest(Video + "Image-to-Video::should send image_url with URL-based image", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_sends_url_images_as_is()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoStartAsync(Start(image: VideoModelFile.FromUrl("https://example.com/input-image.png")), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), "{\"model\":\"" + VideoModelId + "\",\"content\":[" + TextPart + ",{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/input-image.png\"}}]}");
    }

    [Theory]
    [InlineData("{\"watermark\":true}", ",\"watermark\":true", VideoModelId)]
    [InlineData("{\"generateAudio\":true}", ",\"generate_audio\":true", VideoModelId)]
    [InlineData("{\"cameraFixed\":true}", ",\"camera_fixed\":true", VideoModelId)]
    [InlineData("{\"returnLastFrame\":true}", ",\"return_last_frame\":true", VideoModelId)]
    [InlineData("{\"serviceTier\":\"flex\"}", ",\"service_tier\":\"flex\"", VideoModelId)]
    [InlineData("{\"draft\":true}", ",\"draft\":true", "seedance-1-5-pro-251215")]
    [InlineData("{\"custom_param\":\"custom_value\",\"another_param\":123}", ",\"custom_param\":\"custom_value\",\"another_param\":123", VideoModelId)]
    [InlineData("{\"pollIntervalMs\":1000,\"pollTimeoutMs\":600000}", "", VideoModelId)]
    [UpstreamTest(Video + "Provider Options::should pass watermark option", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass generateAudio as generate_audio", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass cameraFixed as camera_fixed", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass returnLastFrame as return_last_frame", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass serviceTier as service_tier", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass draft option", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should pass through additional options", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Provider Options::should not pass legacy poll options through to the request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_maps_provider_options(string options, string fields, string modelId)
    {
        var handler = VideoHandler();
        await VideoModel(handler, modelId).DoStartAsync(Start(options: options), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), StartBody(fields, modelId));
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should map the top-level generateAudio option", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_maps_top_level_generate_audio()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoStartAsync(Start(generateAudio: true), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), StartBody(",\"generate_audio\":true"));
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should let the top-level generateAudio override the legacy provider option", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_prefers_top_level_generate_audio()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoStartAsync(Start(generateAudio: false, options: "{\"generateAudio\":true}"), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), StartBody(",\"generate_audio\":false"));
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add last frame image with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_the_provider_last_frame()
    {
        var content = await StartContent(Start(image: VideoModelFile.FromUrl("https://example.com/first-frame.png"), options: "{\"lastFrameImage\":\"https://example.com/last-frame.png\"}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/first-frame.png", "first_frame") + "," + Part("image_url", "https://example.com/last-frame.png", "last_frame") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add last frame image from frameImages last_frame with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_the_last_frame_image()
    {
        var content = await StartContent(Start(image: VideoModelFile.FromUrl("https://example.com/first-frame.png"), frames: Frames(("https://example.com/last-frame.png", "last_frame")))).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/first-frame.png", "first_frame") + "," + Part("image_url", "https://example.com/last-frame.png", "last_frame") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should prefer frameImages last_frame over providerOptions.bytedance.lastFrameImage", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_prefers_the_last_frame_image()
    {
        var content = await StartContent(Start(
            image: VideoModelFile.FromUrl("https://example.com/first-frame.png"),
            frames: Frames(("https://example.com/new-last.png", "last_frame")),
            options: "{\"lastFrameImage\":\"https://example.com/legacy-last.png\"}")).ConfigureAwait(false);
        Assert.Contains(content.AsArray(), part => JsonNode.DeepEquals(part, JsonNode.Parse(Part("image_url", "https://example.com/new-last.png", "last_frame"))));
        Assert.DoesNotContain("legacy-last.png", content.ToJsonString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should use frameImages first_frame as the starting image", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_uses_the_first_frame_image()
    {
        var content = await StartContent(Start(frames: Frames(("https://example.com/first-frame.png", "first_frame")))).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/first-frame.png", null) + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should prefer frameImages first_frame over the legacy image option", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_prefers_the_first_frame_image()
    {
        var content = await StartContent(Start(
            image: VideoModelFile.FromUrl("https://example.com/legacy-image.png"),
            frames: Frames(("https://example.com/first-frame.png", "first_frame"), ("https://example.com/last-frame.png", "last_frame")))).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/first-frame.png", "first_frame") + "," + Part("image_url", "https://example.com/last-frame.png", "last_frame") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should send only last_frame when only last_frame is provided without a legacy image", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_sends_only_the_last_frame()
    {
        var content = await StartContent(Start(frames: Frames(("https://example.com/last-frame.png", "last_frame")))).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/last-frame.png", "last_frame") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add reference images from inputReferences with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_input_reference_images()
    {
        var content = await StartContent(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/ref1.png", "image/png"), VideoModelFile.FromUrl("https://example.com/ref2.png", "image/png") }), "seedance-1-0-lite-i2v-250428").ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/ref1.png", "reference_image") + "," + Part("image_url", "https://example.com/ref2.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should treat a start image as a reference image when combining it with inputReferences", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_image_becomes_a_reference_with_input_references()
    {
        var content = await StartContent(Start(image: VideoModelFile.FromUrl("https://example.com/start.png", "image/png"), references: new[] { VideoModelFile.FromUrl("https://example.com/reference.png", "image/png") })).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/start.png", "reference_image") + "," + Part("image_url", "https://example.com/reference.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add a reference video from inputReferences with video media type", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_routes_video_references_by_media_type()
    {
        var content = await StartContent(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/a.mp4", "video/mp4") })).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("video_url", "https://example.com/a.mp4", "reference_video") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add a reference image from inputReferences with image media type", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_routes_image_references_by_media_type()
    {
        var content = await StartContent(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/a.png", "image/png") })).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/a.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should route a mix of video and image inputReferences by media type", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_routes_mixed_references()
    {
        var content = await StartContent(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/a.mp4", "video/mp4"), VideoModelFile.FromUrl("https://example.com/b.png", "image/png") })).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("video_url", "https://example.com/a.mp4", "reference_video") + "," + Part("image_url", "https://example.com/b.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should warn when a URL inputReference has no mediaType and treat it as an image reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_warns_for_url_references_without_media_type()
    {
        var handler = VideoHandler();
        var result = await VideoModel(handler).DoStartAsync(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/a.mp4") }), CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "inputReferences" && warning.Details == "ByteDance requires an explicit mediaType to route URL references as video or image. Pass { data: url, mediaType: \"video/mp4\" } for video references. The reference was treated as an image.");
        JsonAssert.Equal(Body(handler)["content"], "[" + TextPart + "," + Part("image_url", "https://example.com/a.mp4", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should prefer inputReferences over providerOptions.bytedance.referenceImages", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_prefers_input_references()
    {
        var content = await StartContent(Start(references: new[] { VideoModelFile.FromUrl("https://example.com/new-ref.png") }, options: "{\"referenceImages\":[\"https://example.com/legacy-ref.png\"]}"), "seedance-1-0-lite-i2v-250428").ConfigureAwait(false);
        Assert.Contains(content.AsArray(), part => JsonNode.DeepEquals(part, JsonNode.Parse(Part("image_url", "https://example.com/new-ref.png", "reference_image"))));
        Assert.DoesNotContain("legacy-ref.png", content.ToJsonString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add reference images with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_provider_reference_images()
    {
        var content = await StartContent(Start(options: "{\"referenceImages\":[\"https://example.com/ref1.png\",\"https://example.com/ref2.png\",\"https://example.com/ref3.png\"]}"), "seedance-1-0-lite-i2v-250428").ConfigureAwait(false);
        JsonAssert.Equal(
            content,
            "[" + TextPart + "," + Part("image_url", "https://example.com/ref1.png", "reference_image") + "," + Part("image_url", "https://example.com/ref2.png", "reference_image") + "," + Part("image_url", "https://example.com/ref3.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should treat a start image as a reference image when combining it with referenceImages", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_image_becomes_a_reference_with_provider_references()
    {
        var content = await StartContent(Start(image: VideoModelFile.FromUrl("https://example.com/start.png", "image/png"), options: "{\"referenceImages\":[\"https://example.com/reference.png\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("image_url", "https://example.com/start.png", "reference_image") + "," + Part("image_url", "https://example.com/reference.png", "reference_image") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add reference videos with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_provider_reference_videos()
    {
        var content = await StartContent(Start(options: "{\"referenceVideos\":[\"https://example.com/ref1.mp4\",\"https://example.com/ref2.mp4\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("video_url", "https://example.com/ref1.mp4", "reference_video") + "," + Part("video_url", "https://example.com/ref2.mp4", "reference_video") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add reference audio with role", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_reference_audio()
    {
        var content = await StartContent(Start(options: "{\"referenceAudio\":[\"https://example.com/audio.mp3\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("audio_url", "https://example.com/audio.mp3", "reference_audio") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should add multiple reference audios", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_several_reference_audios()
    {
        var content = await StartContent(Start(options: "{\"referenceAudio\":[\"https://example.com/audio1.mp3\",\"https://example.com/audio2.mp3\",\"https://example.com/audio3.mp3\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(
            content,
            "[" + TextPart + "," + Part("audio_url", "https://example.com/audio1.mp3", "reference_audio") + "," + Part("audio_url", "https://example.com/audio2.mp3", "reference_audio") + "," + Part("audio_url", "https://example.com/audio3.mp3", "reference_audio") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should support data URI for reference audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_keeps_reference_audio_data_uris()
    {
        var content = await StartContent(Start(options: "{\"referenceAudio\":[\"data:audio/mp3;base64,SGVsbG8=\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("audio_url", "data:audio/mp3;base64,SGVsbG8=", "reference_audio") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should support reference videos and audio together", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_adds_reference_videos_and_audio()
    {
        var content = await StartContent(Start(options: "{\"referenceVideos\":[\"https://example.com/ref.mp4\"],\"referenceAudio\":[\"https://example.com/audio.mp3\"]}")).ConfigureAwait(false);
        JsonAssert.Equal(content, "[" + TextPart + "," + Part("video_url", "https://example.com/ref.mp4", "reference_video") + "," + Part("audio_url", "https://example.com/audio.mp3", "reference_audio") + "]");
    }

    [Fact]
    [UpstreamTest(Video + "Provider Options::should warn that legacy poll options are ignored", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_warns_that_poll_options_are_ignored()
    {
        var result = await VideoModel(VideoHandler()).DoStartAsync(Start(options: "{\"pollIntervalMs\":1000,\"pollTimeoutMs\":600000}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Collection(
            result.Warnings,
            warning => AssertDeprecated(warning, "pollIntervalMs"),
            warning => AssertDeprecated(warning, "pollTimeoutMs"));
    }

    [Fact]
    [UpstreamTest(Video + "Error Handling::should throw error when no task ID is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_requires_a_task_id()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => VideoModel(VideoHandler(start: "{}")).DoStartAsync(Start(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("No task ID returned from API", error.Message);
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("cancelled")]
    [InlineData("canceled")]
    [UpstreamTest(Video + "Error Handling::should return error status when task fails", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Error Handling::should return error status when task is cancelled", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Error Handling::should return error status when task is canceled (single-l spelling)", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_reports_failed_and_cancelled_tasks(string status)
    {
        var result = await VideoModel(VideoHandler(status: "{\"id\":\"test-task-id-123\",\"status\":\"" + status + "\"}")).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("error", result.Status);
        Assert.StartsWith("Video generation " + status + ". Task ID: test-task-id-123.", result.Error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{\"code\":\"SensitiveContentDetected\",\"message\":\"The prompt was rejected by the content filter.\"}", "The prompt was rejected by the content filter.")]
    [InlineData("{\"code\":\"InternalServiceError\"}", "InternalServiceError")]
    [UpstreamTest(Video + "Error Handling::should surface the failure reason reported by the task", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "Error Handling::should fall back to the error code when the task reports no message", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_reports_the_failure_reason(string error, string expected)
    {
        var result = await VideoModel(VideoHandler(status: "{\"id\":\"test-task-id-123\",\"status\":\"failed\",\"error\":" + error + "}")).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Video generation failed. Task ID: test-task-id-123. " + expected, result.Error);
    }

    [Theory]
    [InlineData(",\"error\":{\"code\":\"TaskExpired\",\"message\":\"The task has expired.\"}", "The task has expired.")]
    [InlineData("", "{\"id\":\"test-task-id-123\",\"status\":\"expired\"}")]
    [UpstreamTest(Video + "Error Handling::should return an expired error with $name", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_reports_expired_tasks(string error, string expected)
    {
        var result = await VideoModel(VideoHandler(status: "{\"id\":\"test-task-id-123\",\"status\":\"expired\"" + error + "}")).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation expired. Task ID: test-task-id-123. " + expected, result.Error);
    }

    [Fact]
    [UpstreamTest(Video + "Error Handling::should throw error when no video URL in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_requires_a_video_url()
    {
        var handler = VideoHandler(status: "{\"id\":\"test-task-id-123\",\"status\":\"succeeded\",\"content\":{}}");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => VideoModel(handler).DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("No video URL in response. Task ID: test-task-id-123", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + "Error Handling::should handle API errors from task creation", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_start_api_errors_keep_the_status()
    {
        var handler = new ParityHandler(_ => Error(HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Invalid prompt\"}}"));
        var error = await Assert.ThrowsAsync<BadRequestException>(() => VideoModel(handler).DoStartAsync(Start(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Video + "Error Handling::should handle API errors from the status endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_api_errors_keep_the_status_and_message()
    {
        var handler = new ParityHandler(_ => Error(HttpStatusCode.NotFound, "{\"error\":{\"message\":\"Task not found\"}}"));
        var error = await Assert.ThrowsAsync<NotFoundException>(() => VideoModel(handler).DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Task not found", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + "doStatus::should return completed with video data when succeeded", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_returns_the_video_url()
    {
        var handler = VideoHandler();
        var result = await VideoModel(handler).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://bytedance.cdn/files/video-output.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Empty(result.Warnings);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
        Assert.Equal(TasksUrl + "/" + TaskId, handler.Calls[0].Uri.AbsoluteUri);
    }

    [Theory]
    [InlineData("queued")]
    [InlineData("running")]
    [UpstreamTest(Video + "doStatus::should return pending when queued", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Video + "doStatus::should return pending when running", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_is_pending_while_the_task_runs(string status)
    {
        var result = await VideoModel(VideoHandler(status: "{\"id\":\"test-task-id-123\",\"status\":\"" + status + "\"}")).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(Video + "doStatus::should include timestamp, modelId and headers in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_response_has_timestamp_headers_and_model()
    {
        var result = await VideoModel(VideoHandler(), clock: () => TestDate).DoStatusAsync(Status(), CancellationToken.None).ConfigureAwait(false);
        AssertResponse(result.Response);
    }

    [Fact]
    [UpstreamTest(Video + "doStatus::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Video_status_sends_provider_and_call_headers()
    {
        var handler = VideoHandler();
        var model = VideoModel(handler, configure: options => options.Headers["Custom-Provider-Header"] = "provider-header-value");
        await model.DoStatusAsync(Status(new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("provider-header-value", handler.Calls[0].Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("Custom-Request-Header"));
    }

    private static ByteDanceImageModel ImageModel(ParityHandler handler, Func<DateTimeOffset>? clock = null)
    {
        var provider = ByteDanceProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://api.example.com" }, handler);
        return new ByteDanceImageModel(provider, ImageModelId, clock);
    }

    private static ParityHandler ImageHandler(string body = "{\"data\":[{\"b64_json\":\"test1234\"},{\"b64_json\":\"test5678\"}]}")
    {
        return new ParityHandler(_ => ParityHandler.Json(body));
    }

    private static ImageModelCall ImageCall(string prompt = ImagePrompt, IReadOnlyList<ImageModelFile>? files = null, ImageModelFile? mask = null, string? size = null, string? aspectRatio = null, int? seed = null, string options = "{}")
    {
        return new ImageModelCall(prompt, files, mask, 1, size, aspectRatio, seed, OperationJson.Parse("{\"bytedance\":" + options + "}"), new Dictionary<string, string>(), CancellationToken.None);
    }

    private static ByteDanceVideoModel VideoModel(ParityHandler handler, string modelId = VideoModelId, Func<DateTimeOffset>? clock = null, Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-key" };
        configure?.Invoke(options);
        return new ByteDanceVideoModel(ByteDanceProvider.Create(options, handler), modelId, clock);
    }

    // POST creates the task. GET reads its status.
    private static ParityHandler VideoHandler(string start = "{\"id\":\"test-task-id-123\"}", string status = Succeeded)
    {
        return new ParityHandler(call => ParityHandler.Json(call.Method == HttpMethod.Post ? start : status));
    }

    private static VideoModelCall Start(
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        int? fps = null,
        int? seed = null,
        VideoModelFile? image = null,
        IReadOnlyList<VideoFrameFile>? frames = null,
        IReadOnlyList<VideoModelFile>? references = null,
        bool? generateAudio = null,
        string options = "{}",
        Dictionary<string, string>? headers = null,
        string? webhookUrl = null)
    {
        return new VideoModelCall(VideoPrompt, n, aspectRatio, resolution, duration, fps, seed, image, frames, references, generateAudio, OperationJson.Parse("{\"bytedance\":" + options + "}"), headers ?? new Dictionary<string, string>(), CancellationToken.None, webhookUrl);
    }

    private static VideoModelCall Status(Dictionary<string, string>? headers = null)
    {
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), headers ?? new Dictionary<string, string>(), CancellationToken.None, operation: OperationJson.Parse("{\"taskId\":\"test-task-id-123\"}"));
    }

    private static IReadOnlyList<VideoFrameFile> Frames(params (string Url, string FrameType)[] frames)
    {
        return frames.Select(frame => new VideoFrameFile(VideoModelFile.FromUrl(frame.Url), frame.FrameType)).ToList();
    }

    private static async Task<JsonNode> StartContent(VideoModelCall call, string modelId = "seedance-1-5-pro-251215")
    {
        var handler = VideoHandler();
        await VideoModel(handler, modelId).DoStartAsync(call, CancellationToken.None).ConfigureAwait(false);
        return Body(handler)["content"]!;
    }

    private static string StartBody(string fields, string modelId = VideoModelId)
    {
        return "{\"model\":\"" + modelId + "\",\"content\":[" + TextPart + "]" + fields + "}";
    }

    private static string Part(string type, string url, string? role)
    {
        return "{\"type\":\"" + type + "\",\"" + type + "\":{\"url\":\"" + url + "\"}" + (role == null ? string.Empty : ",\"role\":\"" + role + "\"") + "}";
    }

    private static JsonObject Body(ParityHandler handler)
    {
        return JsonNode.Parse(handler.Calls[0].Text)!.AsObject();
    }

    private static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    private static void AssertImageUsage(OperationUsage usage, int? output, int? total)
    {
        Assert.Null(usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(total, usage.TotalTokens);
    }

    private static void AssertResponse(ProviderResponse response)
    {
        Assert.Equal(TestDate.UtcDateTime, response.Timestamp);
        Assert.Equal(VideoModelId, response.ModelId);
        Assert.NotNull(response.Headers);
    }

    private static void AssertDeprecated(OperationWarning warning, string setting)
    {
        Assert.Equal("deprecated", warning.Type);
        Assert.Equal(setting, warning.Setting);
        Assert.Contains(DeprecatedPollMessage, warning.Message, StringComparison.Ordinal);
    }
}
