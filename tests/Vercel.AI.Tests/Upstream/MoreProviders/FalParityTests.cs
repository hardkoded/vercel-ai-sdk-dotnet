// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Fal;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class FalImageParityTests
{
    private const string ImagePrefix = "packages/fal/src/fal-image-model.test.ts::FalImageModel > ";
    private const string ProviderPrefix = "packages/fal/src/fal-provider.test.ts::createFal > image::";
    private const string Prompt = "A cute baby sea otter";
    private const string GenerateUrl = "https://api.example.com/fal-ai/qwen-image";
    private const string ImageUrl = "https://api.example.com/image.png";
    private const string DefaultResponse = "{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"width\":1024,\"height\":1024,\"content_type\":\"image/png\"}]}";
    private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");
    private static readonly byte[] PngBytes = { 137, 80, 78, 71 };

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should pass the correct parameters including size", Coverage = UpstreamCoverage.Covered)]
    public async Task Body_has_size_seed_and_extra_options()
    {
        var handler = Server();
        await Generate(handler, Call(size: "1024x1024", seed: 123, fal: "{\"additional_param\":\"value\"}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"seed\":123,\"image_size\":{\"width\":1024,\"height\":1024},\"num_images\":1,\"additional_param\":\"value\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should convert camelCase provider options to snake_case for API", Coverage = UpstreamCoverage.Covered)]
    public async Task Camel_case_options_are_sent_as_snake_case()
    {
        var handler = Server();
        var result = await Generate(handler, Call(fal: "{\"imageUrl\":\"https://example.com/image.png\",\"guidanceScale\":7.5,\"numInferenceSteps\":50,\"enableSafetyChecker\":false}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"num_images\":1,\"image_url\":\"https://example.com/image.png\",\"guidance_scale\":7.5,\"num_inference_steps\":50,\"enable_safety_checker\":false}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should accept deprecated snake_case provider options with warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Snake_case_options_are_sent_with_a_deprecation_warning()
    {
        var handler = Server();
        var result = await Generate(handler, Call(fal: "{\"image_url\":\"https://example.com/image.png\",\"guidance_scale\":7.5,\"num_inference_steps\":50}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"num_images\":1,\"image_url\":\"https://example.com/image.png\",\"guidance_scale\":7.5,\"num_inference_steps\":50}");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Contains("deprecated snake_case", warning.Message);
        Assert.Contains("'image_url' (use 'imageUrl')", warning.Message);
        Assert.Contains("'guidance_scale' (use 'guidanceScale')", warning.Message);
        Assert.Contains("'num_inference_steps' (use 'numInferenceSteps')", warning.Message);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should convert aspect ratio to size", Coverage = UpstreamCoverage.Covered)]
    public async Task Aspect_ratio_is_sent_as_a_named_size()
    {
        var handler = Server();
        await Generate(handler, Call(aspectRatio: "16:9", fal: "{}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"image_size\":\"landscape_16_9\",\"num_images\":1}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generation_sends_json_and_custom_headers()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { BaseUrl = "https://api.example.com" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = new FalImageModel(FalProvider.Create(options, handler), "fal-ai/qwen-image");
        await model.DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal(GenerateUrl, call.Uri.AbsoluteUri);
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate::should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Validation_errors_name_the_invalid_field()
    {
        var handler = Server();
        handler.Respond = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"detail\":[{\"loc\":[\"prompt\"],\"msg\":\"Invalid prompt\",\"type\":\"value_error\"}]}"),
        };
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Generate(handler, Call()));
        Assert.Equal("prompt: Invalid prompt", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(GenerateUrl, Assert.Single(handler.Calls).Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate > response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_timestamp_headers_and_model_id()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await Generate(Server(), Call(), () => date);
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("fal-ai/qwen-image", result.Response.ModelId);
        Assert.Equal("application/json", result.Response.Headers!["Content-Type"]);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate > providerMetaData::for lora", Coverage = UpstreamCoverage.Covered)]
    public async Task Lora_metadata_renames_file_fields_and_keeps_debug_latents()
    {
        var latents = "\"debug_latents\":{\"url\":\"<debug_latents url>\",\"content_type\":\"<debug_latents content_type>\",\"file_name\":\"<debug_latents file_name>\",\"file_data\":\"<debug_latents file_data>\",\"file_size\":123},"
            + "\"debug_per_pass_latents\":{\"url\":\"<debug_per_pass_latents url>\",\"content_type\":\"<debug_per_pass_latents content_type>\",\"file_name\":\"<debug_per_pass_latents file_name>\",\"file_data\":\"<debug_per_pass_latents file_data>\",\"file_size\":456}";
        var handler = Server("{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"width\":1024,\"height\":1024,\"content_type\":\"image/png\",\"file_data\":\"<image file_data>\",\"file_size\":123,\"file_name\":\"<image file_name>\"}],"
            + "\"prompt\":\"<prompt>\",\"seed\":123,\"has_nsfw_concepts\":[true]," + latents + "}");
        var result = await Generate(handler, Call());
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fal\":{\"images\":[{\"width\":1024,\"height\":1024,\"contentType\":\"image/png\",\"fileName\":\"<image file_name>\",\"fileData\":\"<image file_data>\",\"fileSize\":123,\"nsfw\":true}],\"seed\":123," + latents + "}}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "doGenerate > providerMetaData::for lcm", Coverage = UpstreamCoverage.Covered)]
    public async Task Lcm_metadata_reads_nsfw_content_detected()
    {
        var handler = Server("{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"width\":1024,\"height\":1024}],\"seed\":123,\"num_inference_steps\":456,\"nsfw_content_detected\":[false]}");
        var result = await Generate(handler, Call());
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fal\":{\"images\":[{\"width\":1024,\"height\":1024,\"nsfw\":false}],\"seed\":123,\"num_inference_steps\":456}}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Model_exposes_provider_and_limits()
    {
        var model = new FalImageModel(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }), "fal-ai/qwen-image");
        Assert.Equal("fal.image", model.Provider);
        Assert.Equal("fal-ai/qwen-image", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should send edit request with files as data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task A_file_is_sent_as_a_data_uri()
    {
        var handler = Server();
        await Generate(handler, Call("Turn the cat into a dog", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png") }));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_url\":\"data:image/png;base64,iVBORw==\",\"num_images\":1,\"prompt\":\"Turn the cat into a dog\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should send edit request with files and mask", Coverage = UpstreamCoverage.Covered)]
    public async Task A_mask_is_sent_as_a_data_uri()
    {
        var handler = Server();
        await Generate(handler, Call("Add a flamingo to the pool", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png") }, mask: ImageModelFile.FromFile(new byte[] { 255, 255, 255, 0 }, "image/png")));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_url\":\"data:image/png;base64,iVBORw==\",\"mask_url\":\"data:image/png;base64,////AA==\",\"num_images\":1,\"prompt\":\"Add a flamingo to the pool\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should send edit request with URL-based file", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_file_is_sent_as_is()
    {
        var handler = Server();
        await Generate(handler, Call("Edit this image", files: new[] { ImageModelFile.FromUrl("https://example.com/input.png") }));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_url\":\"https://example.com/input.png\",\"num_images\":1,\"prompt\":\"Edit this image\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should warn when multiple files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Extra_files_warn_without_use_multiple_images()
    {
        var result = await Generate(Server(), Call("Edit images", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png"), ImageModelFile.FromFile(PngBytes, "image/png") }));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Contains("useMultipleImages is not enabled", warning.Message);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should send image_urls when useMultipleImages is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Use_multiple_images_sends_every_file()
    {
        var handler = Server();
        await Generate(handler, Call("Edit these images", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png"), ImageModelFile.FromFile(PngBytes, "image/png") }, fal: "{\"useMultipleImages\":true}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_urls\":[\"data:image/png;base64,iVBORw==\",\"data:image/png;base64,iVBORw==\"],\"num_images\":1,\"prompt\":\"Edit these images\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should not warn when multiple files provided with useMultipleImages", Coverage = UpstreamCoverage.Covered)]
    public async Task Use_multiple_images_does_not_warn()
    {
        var result = await Generate(Server(), Call("Edit images", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png"), ImageModelFile.FromFile(PngBytes, "image/png") }, fal: "{\"useMultipleImages\":true}"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should send single image as image_urls array when useMultipleImages is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Use_multiple_images_sends_one_file_as_an_array()
    {
        var handler = Server();
        await Generate(handler, Call("Edit this image", files: new[] { ImageModelFile.FromFile(PngBytes, "image/png") }, fal: "{\"useMultipleImages\":true}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_urls\":[\"data:image/png;base64,iVBORw==\"],\"num_images\":1,\"prompt\":\"Edit this image\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should allow imageUrl via provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_url_option_is_sent()
    {
        var handler = Server();
        await Generate(handler, Call("Edit via provider options", fal: "{\"imageUrl\":\"https://example.com/provider-image.png\"}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_url\":\"https://example.com/provider-image.png\",\"num_images\":1,\"prompt\":\"Edit via provider options\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "Image Editing::should allow maskUrl via provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Mask_url_option_is_sent()
    {
        var handler = Server();
        await Generate(handler, Call("Inpaint this", fal: "{\"imageUrl\":\"https://example.com/image.png\",\"maskUrl\":\"https://example.com/mask.png\"}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image_url\":\"https://example.com/image.png\",\"mask_url\":\"https://example.com/mask.png\",\"num_images\":1,\"prompt\":\"Inpaint this\"}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "response schema validation::should parse single image response", Coverage = UpstreamCoverage.Covered)]
    public async Task A_single_image_response_is_downloaded()
    {
        var result = await Generate(Server("{\"image\":{\"url\":\"https://api.example.com/image.png\",\"width\":1024,\"height\":1024,\"content_type\":\"image/png\"}}"), Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "response schema validation::should parse multiple images response", Coverage = UpstreamCoverage.Covered)]
    public async Task Every_image_is_downloaded()
    {
        var image = "{\"url\":\"https://api.example.com/image.png\",\"width\":1024,\"height\":1024,\"content_type\":\"image/png\"}";
        var result = await Generate(Server("{\"images\":[" + image + "," + image + "]}"), Call(n: 2));
        Assert.Equal(2, result.Images.Count);
        Assert.All(result.Images, downloaded => Assert.Equal(ImageBytes, (byte[])downloaded!));
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "response schema validation::should handle null file_name and file_size values", Coverage = UpstreamCoverage.Covered)]
    public async Task Null_file_fields_stay_null_in_metadata()
    {
        var handler = Server("{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"content_type\":\"image/png\",\"file_name\":null,\"file_size\":null,\"width\":944,\"height\":1104}],"
            + "\"timings\":{\"inference\":5.875932216644287},\"seed\":328395684,\"has_nsfw_concepts\":[false],\"prompt\":\"A female model holding this book, keeping the book unchanged.\"}");
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fal\":{\"images\":[{\"width\":944,\"height\":1104,\"contentType\":\"image/png\",\"fileName\":null,\"fileSize\":null,\"nsfw\":false}],\"timings\":{\"inference\":5.875932216644287},\"seed\":328395684}}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "response schema validation::should handle empty timings object", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_timings_are_kept()
    {
        var handler = Server("{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"content_type\":\"image/png\",\"file_name\":null,\"file_size\":null,\"width\":880,\"height\":1184}],"
            + "\"timings\":{},\"seed\":235205040,\"has_nsfw_concepts\":[false],\"prompt\":\"Change the plates to colorful ones\"}");
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fal\":{\"images\":[{\"width\":880,\"height\":1184,\"contentType\":\"image/png\",\"fileName\":null,\"fileSize\":null,\"nsfw\":false}],\"timings\":{},\"seed\":235205040}}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "response schema validation::should handle null width and height values with images array only", Coverage = UpstreamCoverage.Covered)]
    public async Task Null_dimensions_and_extra_fields_are_kept()
    {
        var handler = Server("{\"images\":[{\"url\":\"https://api.example.com/image.png\",\"content_type\":\"image/png\",\"file_name\":\"output.png\",\"file_size\":663399,\"width\":null,\"height\":null}],"
            + "\"description\":\"here is an image with null width and height\"}");
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"fal\":{\"images\":[{\"width\":null,\"height\":null,\"contentType\":\"image/png\",\"fileName\":\"output.png\",\"fileSize\":663399}],\"description\":\"here is an image with null width and height\"}}");
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "should construct an image model with default configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_image_models_post_to_fal_run()
    {
        var handler = Server(DefaultResponse.Replace("https://api.example.com", "https://fal.run"), "https://fal.run/image.png");
        var model = Assert.IsType<FalImageModel>(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler).ImageModel("fal-ai/flux/dev"));
        await model.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal("fal.image", model.Provider);
        Assert.Equal("https://fal.run/fal-ai/flux/dev", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "should respect custom configuration options", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_image_models_use_the_custom_base_url_key_and_headers()
    {
        var handler = Server(DefaultResponse.Replace("https://api.example.com", "https://custom.fal.run"), "https://custom.fal.run/image.png");
        var options = new OpenAICompatibleOptions { ApiKey = "custom-api-key", BaseUrl = "https://custom.fal.run" };
        options.Headers["X-Custom-Header"] = "value";
        var model = FalProvider.Create(options, handler).ImageModel("fal-ai/flux/dev");
        await ((FalImageModel)model).DoGenerateAsync(Call(), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("fal.image", model.Provider);
        Assert.Equal("https://custom.fal.run/fal-ai/flux/dev", call.Uri.AbsoluteUri);
        Assert.Equal("Key custom-api-key", call.Header("Authorization"));
        Assert.Equal("value", call.Header("X-Custom-Header"));
    }

    private static ParityHandler Server(string response = DefaultResponse, string imageUrl = ImageUrl)
    {
        return new ParityHandler(call => call.Uri.AbsoluteUri == imageUrl ? ParityHandler.Bytes(ImageBytes, "image/png") : ParityHandler.Json(response));
    }

    private static Task<ImageModelResult> Generate(ParityHandler handler, ImageModelCall call, Func<DateTime>? clock = null)
    {
        var provider = FalProvider.Create(new OpenAICompatibleOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" }, handler);
        return new FalImageModel(provider, "fal-ai/qwen-image", clock).DoGenerateAsync(call, CancellationToken.None);
    }

    private static ImageModelCall Call(string prompt = Prompt, int n = 1, string? size = null, string? aspectRatio = null, int? seed = null, string? fal = null, IReadOnlyList<ImageModelFile>? files = null, ImageModelFile? mask = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        var providerOptions = JsonSerializer.SerializeToElement(fal == null ? new JsonObject() : new JsonObject { ["fal"] = JsonNode.Parse(fal) });
        return new ImageModelCall(prompt, files, mask, n, size, aspectRatio, seed, providerOptions, headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }
}

public sealed class FalSpeechParityTests
{
    private const string SpeechPrefix = "packages/fal/src/fal-speech-model.test.ts::FalSpeechModel.doGenerate::";
    private const string SpeechUrl = "https://fal.run/fal-ai/minimax/speech-02-hd";
    private const string AudioUrl = "https://fal.media/files/test.mp3";
    private const string Text = "Hello from the AI SDK!";
    private static readonly byte[] Audio = new byte[100];

    [Fact]
    [UpstreamTest(SpeechPrefix + "should pass text and default output_format", Coverage = UpstreamCoverage.Covered)]
    public async Task Body_has_text_and_url_output_format()
    {
        var handler = Server();
        await CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal(SpeechUrl, handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"text\":\"Hello from the AI SDK!\",\"output_format\":\"url\"}");
    }

    [Fact]
    [UpstreamTest(SpeechPrefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Request_sends_key_json_and_custom_headers()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = FalProvider.Create(options, handler).SpeechModel("fal-ai/minimax/speech-02-hd");
        await ((FalSpeechModel)model).DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Key test-api-key", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest(SpeechPrefix + "should return audio data", Coverage = UpstreamCoverage.Covered)]
    public async Task Audio_is_downloaded_from_the_returned_url()
    {
        var handler = Server();
        var result = await CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal(Audio, result.Audio);
        Assert.Equal(AudioUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(SpeechPrefix + "should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_timestamp_model_id_and_headers()
    {
        var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await CreateModel(Server(("x-request-id", "test-request-id")), () => date).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("fal-ai/minimax/speech-02-hd", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(SpeechPrefix + "should include warnings for unsupported settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Language_and_unknown_output_format_warn()
    {
        var handler = Server();
        var result = await CreateModel(handler).DoGenerateAsync(Call(language: "en", outputFormat: "wav"), CancellationToken.None);
        Assert.Collection(
            result.Warnings,
            warning => Assert.Equal("language", warning.Feature),
            warning => Assert.Equal("outputFormat", warning.Feature));
        Assert.Equal("url", JsonNode.Parse(handler.Calls[0].Text)!["output_format"]!.GetValue<string>());
    }

    private static ParityHandler Server(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(call => call.Uri.AbsoluteUri == AudioUrl
            ? ParityHandler.Bytes(Audio, "audio/mp3")
            : ParityHandler.Json("{\"audio\":{\"url\":\"https://fal.media/files/test.mp3\"},\"duration_ms\":1234}", headers));
    }

    private static FalSpeechModel CreateModel(ParityHandler handler, Func<DateTime>? clock = null)
    {
        return new FalSpeechModel(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), "fal-ai/minimax/speech-02-hd", clock);
    }

    private static SpeechModelCall Call(string? language = null, string? outputFormat = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        return new SpeechModelCall(Text, null, outputFormat, null, null, language, JsonValues.EmptyObject(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }
}

public sealed class FalTranscriptionParityTests
{
    private const string TranscriptionPrefix = "packages/fal/src/fal-transcription-model.test.ts::doGenerate > ";
    private const string QueueUrl = "https://queue.fal.run/fal-ai/wizper";
    private const string StatusUrl = "https://queue.fal.run/fal-ai/wizper/requests/test-id";
    private const string Queued = "{\"status\":\"IN_QUEUE\",\"request_id\":\"test-id\",\"response_url\":\"https://queue.fal.run/fal-ai/wizper/requests/test-id/result\",\"status_url\":\"https://queue.fal.run/fal-ai/wizper/requests/test-id\",\"cancel_url\":\"https://queue.fal.run/fal-ai/wizper/requests/test-id/cancel\",\"logs\":null,\"metrics\":{},\"queue_position\":4}";
    private const string Transcript = "{\"text\":\"Hello from the Versal AISDK.\",\"chunks\":[{\"timestamp\":[0,2.508],\"text\":\"Hello from the Versal AISDK.\"}],\"languages\":[\"en\"]}";
    private static readonly byte[] AudioData = { 0x49, 0x44, 0x33, 0x04 };

    [Fact]
    [UpstreamTest(TranscriptionPrefix + "transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Queue_body_has_audio_data_uri_and_defaults()
    {
        var handler = Server();
        await CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal(QueueUrl, handler.Calls[0].Uri.AbsoluteUri);
        var body = JsonNode.Parse(handler.Calls[0].Text)!.AsObject();
        Assert.StartsWith("data:audio/", body["audio_url"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Equal("transcribe", body["task"]!.GetValue<string>());
        Assert.True(body["diarize"]!.GetValue<bool>());
        Assert.Equal("word", body["chunk_level"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(TranscriptionPrefix + "transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Queue_request_sends_key_json_and_custom_headers()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = FalProvider.Create(options, handler).TranscriptionModel("wizper");
        await ((FalTranscriptionModel)model).DoGenerateAsync(Call(new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Key test-api-key", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest(TranscriptionPrefix + "transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_comes_from_the_polled_result()
    {
        var handler = Server();
        var result = await CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal("Hello from the Versal AISDK.", result.Text);
        Assert.Equal(StatusUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(TranscriptionPrefix + "response headers::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_body_headers_model_id_and_timestamp()
    {
        var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await CreateModel(Server(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123")), () => date).DoGenerateAsync(Call(), CancellationToken.None);
        var response = result.Response;
        JsonAssert.Equal(response.Body!.Value, Transcript);
        Assert.Equal("application/json", response.Headers!["content-type"]);
        Assert.Equal("123", response.Headers["x-ratelimit-remaining"]);
        Assert.Equal("test-request-id", response.Headers["x-request-id"]);
        Assert.Equal("wizper", response.ModelId);
        Assert.Equal(date, response.Timestamp);
    }

    [Fact]
    [UpstreamTest(TranscriptionPrefix + "response metadata::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_the_clock_timestamp_and_model_id()
    {
        var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await CreateModel(Server(), () => date).DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("wizper", result.Response.ModelId);
    }

    private static ParityHandler Server(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(call => ParityHandler.Json(call.Uri.AbsoluteUri == StatusUrl ? Transcript : Queued, headers));
    }

    private static FalTranscriptionModel CreateModel(ParityHandler handler, Func<DateTime>? clock = null)
    {
        return new FalTranscriptionModel(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), "wizper", clock);
    }

    private static TranscriptionModelCall Call(IReadOnlyDictionary<string, string>? headers = null)
    {
        return new TranscriptionModelCall(AudioData, "audio/wav", JsonValues.EmptyObject(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }
}

public sealed class FalErrorParityTests
{
    [Fact]
    [UpstreamTest("packages/fal/src/fal-error.test.ts::falErrorDataSchema::should parse Fal resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Reads_the_resource_exhausted_message_and_code()
    {
        var body = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";
        Assert.True(ProviderExchange.TryParseError(body, out var message, out var code));
        Assert.Equal("{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n", message);
        Assert.Equal(429, code);
    }
}
