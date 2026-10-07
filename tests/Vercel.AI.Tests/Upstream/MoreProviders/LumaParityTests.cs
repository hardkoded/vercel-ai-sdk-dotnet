// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Luma;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class LumaParityTests
{
    private const string Model = "packages/luma/src/luma-image-model.test.ts::LumaImageModel > ";
    private const string Provider = "packages/luma/src/luma-provider.test.ts::createLuma > image::";
    private const string Prompt = "A cute baby sea otter";
    private const string GenerationUrl = "https://api.example.com/dream-machine/v1/generations/image";
    private const string StatusUrl = "https://api.example.com/dream-machine/v1/generations/test-generation-id";
    private const string ImageUrl = "https://api.example.com/image.png";
    private const string Queued = "{\"id\":\"test-generation-id\",\"generation_type\":\"image\",\"state\":\"queued\",\"created_at\":\"2024-01-01T00:00:00Z\",\"model\":\"test-model\",\"request\":{\"generation_type\":\"image\",\"model\":\"test-model\",\"prompt\":\"A cute baby sea otter\"}}";
    private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");

    [Fact]
    [UpstreamTest(Model + "doGenerate::should pass the correct parameters including aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task Body_has_prompt_aspect_ratio_model_and_extra_options()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest(Prompt) { AspectRatio = "16:9", ProviderOptions = Options("{\"additional_param\":\"value\"}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"aspect_ratio\":\"16:9\",\"model\":\"test-model\",\"additional_param\":\"value\"}");
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate::should call the correct urls in sequence", Coverage = UpstreamCoverage.Covered)]
    public async Task Generation_polls_then_downloads_the_image()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest(Prompt) { AspectRatio = "16:9" });
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal(GenerationUrl, handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(StatusUrl, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, handler.Calls[2].Method);
        Assert.Equal(ImageUrl, handler.Calls[2].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generation_sends_json_and_custom_headers()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { BaseUrl = "https://api.example.com" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var request = new LumaImageRequest(Prompt) { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await LumaProvider.Create(options, handler).GenerateImageAsync("test-model", request, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate::should not pass providerOptions.{pollIntervalMillis,maxPollAttempts}", Coverage = UpstreamCoverage.Covered)]
    public async Task Poll_settings_are_not_sent()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest(Prompt) { AspectRatio = "16:9", ProviderOptions = Options("{\"pollIntervalMillis\":1000,\"maxPollAttempts\":5,\"additional_param\":\"value\"}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"prompt\":\"A cute baby sea otter\",\"aspect_ratio\":\"16:9\",\"model\":\"test-model\",\"additional_param\":\"value\"}");
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate::should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Api_errors_keep_the_status_and_body()
    {
        var handler = Server();
        var poll = handler.Respond;
        handler.Respond = call => call.Uri.AbsoluteUri == GenerationUrl
            ? new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Bad Request") }
            : poll(call);
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Generate(handler, new LumaImageRequest(Prompt)));
        Assert.Equal("Bad Request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Bad Request", error.ResponseBody);
        Assert.Equal(GenerationUrl, Assert.Single(handler.Calls).Uri.AbsoluteUri);
        Assert.Equal(Prompt, JsonNode.Parse(handler.Calls[0].Text)!["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate::should handle failed generation state", Coverage = UpstreamCoverage.Covered)]
    public async Task A_failed_generation_throws_invalid_response_data()
    {
        var handler = Server("{\"id\":\"test-generation-id\",\"generation_type\":\"image\",\"state\":\"failed\",\"failure_reason\":\"Generation failed\",\"created_at\":\"2024-01-01T00:00:00Z\",\"model\":\"test-model\"}");
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Generate(handler, new LumaImageRequest(Prompt)));
        Assert.Equal("Image generation failed.", error.Message);
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate > warnings::should return warnings for unsupported parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Seed_and_size_warn()
    {
        var result = await Generate(Server(), new LumaImageRequest(Prompt) { Size = "1024x1024", Seed = 123 });
        Assert.Collection(
            result.Warnings,
            warning => AssertUnsupported(warning, "seed", "This model does not support the `seed` option."),
            warning => AssertUnsupported(warning, "size", "This model does not support the `size` option. Use `aspectRatio` instead."));
    }

    [Fact]
    [UpstreamTest(Model + "doGenerate > response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_timestamp_headers_and_model_id()
    {
        var date = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var handler = Server();
        var model = new LumaImageModel(LumaProvider.Create(new OpenAICompatibleOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" }, handler), "test-model", () => date);
        var result = await model.GenerateAsync(new LumaImageRequest(Prompt), CancellationToken.None);
        Assert.Equal(date, result.Timestamp);
        Assert.Equal("test-model", result.ModelId);
        Assert.Equal("application/json", result.Headers["Content-Type"]);
        Assert.Equal(ImageBytes, Assert.Single(result.Images).Data);
    }

    [Fact]
    [UpstreamTest(Model + "constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Model_exposes_provider_and_limits()
    {
        var model = (LumaImageModel)LumaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }).ImageModel("test-model");
        Assert.Equal("luma.image", model.Provider);
        Assert.Equal("test-model", model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send image by default when URL file is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_file_is_sent_as_image_reference()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("A warrior with sunglasses") { Files = Urls("https://example.com/input.jpg") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image\":[{\"url\":\"https://example.com/input.jpg\",\"weight\":0.85}],\"model\":\"test-model\",\"prompt\":\"A warrior with sunglasses\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send modify_image when referenceType is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Modify_image_reference_is_sent()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("Transform flowers to sunflowers") { Files = Urls("https://example.com/input.jpg"), ProviderOptions = Options("{\"referenceType\":\"modify_image\"}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"model\":\"test-model\",\"modify_image\":{\"url\":\"https://example.com/input.jpg\",\"weight\":1},\"prompt\":\"Transform flowers to sunflowers\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send style when referenceType is style", Coverage = UpstreamCoverage.Covered)]
    public async Task Style_reference_is_sent()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("A dog in this style") { Files = Urls("https://example.com/style.jpg"), ProviderOptions = Options("{\"referenceType\":\"style\"}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"model\":\"test-model\",\"prompt\":\"A dog in this style\",\"style\":[{\"url\":\"https://example.com/style.jpg\",\"weight\":0.8}]}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send character when referenceType is character", Coverage = UpstreamCoverage.Covered)]
    public async Task Character_references_share_the_default_identity()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("A warrior") { Files = Urls("https://example.com/person1.jpg", "https://example.com/person2.jpg"), ProviderOptions = Options("{\"referenceType\":\"character\"}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"character\":{\"identity0\":{\"images\":[\"https://example.com/person1.jpg\",\"https://example.com/person2.jpg\"]}},\"model\":\"test-model\",\"prompt\":\"A warrior\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send character with custom identity id from images config", Coverage = UpstreamCoverage.Covered)]
    public async Task Character_identity_comes_from_images_config()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("A woman with a cat") { Files = Urls("https://example.com/person.jpg"), ProviderOptions = Options("{\"referenceType\":\"character\",\"images\":[{\"id\":\"identity0\"}]}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"character\":{\"identity0\":{\"images\":[\"https://example.com/person.jpg\"]}},\"model\":\"test-model\",\"prompt\":\"A woman with a cat\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should send character with multiple identities from images config", Coverage = UpstreamCoverage.Covered)]
    public async Task Character_images_group_by_identity()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("Two people talking") { Files = Urls("https://example.com/person1.jpg", "https://example.com/person2.jpg"), ProviderOptions = Options("{\"referenceType\":\"character\",\"images\":[{\"id\":\"identity0\"},{\"id\":\"identity1\"}]}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"character\":{\"identity0\":{\"images\":[\"https://example.com/person1.jpg\"]},\"identity1\":{\"images\":[\"https://example.com/person2.jpg\"]}},\"model\":\"test-model\",\"prompt\":\"Two people talking\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should support multiple images for image", Coverage = UpstreamCoverage.Covered)]
    public async Task Multiple_image_references_use_the_default_weight()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("Combine these concepts") { Files = Urls("https://example.com/input1.jpg", "https://example.com/input2.jpg") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image\":[{\"url\":\"https://example.com/input1.jpg\",\"weight\":0.85},{\"url\":\"https://example.com/input2.jpg\",\"weight\":0.85}],\"model\":\"test-model\",\"prompt\":\"Combine these concepts\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should use custom weights from images config", Coverage = UpstreamCoverage.Covered)]
    public async Task Images_config_sets_the_weight()
    {
        var handler = Server();
        await Generate(handler, new LumaImageRequest("Styled image") { Files = Urls("https://example.com/input.jpg"), ProviderOptions = Options("{\"images\":[{\"weight\":0.5}]}") });
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"image\":[{\"url\":\"https://example.com/input.jpg\",\"weight\":0.5}],\"model\":\"test-model\",\"prompt\":\"Styled image\"}");
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should throw error when mask is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_mask_throws()
    {
        var request = new LumaImageRequest("Replace sky with sunset") { Files = Urls("https://example.com/input.jpg"), Mask = Urls("https://example.com/mask.png")[0] };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(Server(), request));
        Assert.StartsWith("Luma AI does not support mask-based image editing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should throw error when base64 file data is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Inline_file_data_throws()
    {
        var request = new LumaImageRequest("Edit this image") { Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null) } };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(Server(), request));
        Assert.StartsWith("Luma AI only supports URL-based images", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should throw error when base64 mask data is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task An_inline_mask_throws()
    {
        var request = new LumaImageRequest("Edit with mask") { Files = Urls("https://example.com/input.jpg"), Mask = new OpenAICompatibleImageFile("image/png", new byte[] { 255, 255, 255, 0 }, null, null) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(Server(), request));
        Assert.StartsWith("Luma AI does not support mask-based image editing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should throw error when more than 4 images for image", Coverage = UpstreamCoverage.Covered)]
    public async Task More_than_four_image_references_throw()
    {
        var request = new LumaImageRequest("Too many images") { Files = Urls("https://example.com/1.jpg", "https://example.com/2.jpg", "https://example.com/3.jpg", "https://example.com/4.jpg", "https://example.com/5.jpg") };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(Server(), request));
        Assert.StartsWith("Luma AI image supports up to 4 reference images", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Model + "Image Editing::should throw error when multiple files for modify_image", Coverage = UpstreamCoverage.Covered)]
    public async Task Modify_image_with_two_files_throws()
    {
        var request = new LumaImageRequest("Edit multiple images") { Files = Urls("https://example.com/input1.jpg", "https://example.com/input2.jpg"), ProviderOptions = Options("{\"referenceType\":\"modify_image\"}") };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(Server(), request));
        Assert.StartsWith("Luma AI modify_image only supports a single input image", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Model + "response schema validation::should parse response with image references", Coverage = UpstreamCoverage.Covered)]
    public async Task Completed_response_with_image_references_parses()
    {
        await AssertCompletedResponseParses("\"image_ref\":[{\"url\":\"https://example.com/ref1.jpg\",\"weight\":0.85}]");
    }

    [Fact]
    [UpstreamTest(Model + "response schema validation::should parse response with style references", Coverage = UpstreamCoverage.Covered)]
    public async Task Completed_response_with_style_references_parses()
    {
        await AssertCompletedResponseParses("\"style_ref\":[{\"url\":\"https://example.com/style1.jpg\",\"weight\":0.8}]");
    }

    [Fact]
    [UpstreamTest(Model + "response schema validation::should parse response with character references", Coverage = UpstreamCoverage.Covered)]
    public async Task Completed_response_with_character_references_parses()
    {
        await AssertCompletedResponseParses("\"character_ref\":{\"identity0\":{\"images\":[\"https://example.com/character1.jpg\"]}}");
    }

    [Fact]
    [UpstreamTest(Model + "response schema validation::should parse response with modify image reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Completed_response_with_modify_image_reference_parses()
    {
        await AssertCompletedResponseParses("\"modify_image_ref\":{\"url\":\"https://example.com/modify.jpg\",\"weight\":1.0}");
    }

    [Fact]
    [UpstreamTest(Model + "response schema validation::should parse response with multiple reference types", Coverage = UpstreamCoverage.Covered)]
    public async Task Completed_response_with_every_reference_type_parses()
    {
        await AssertCompletedResponseParses("\"image_ref\":[{\"url\":\"https://example.com/ref1.jpg\",\"weight\":0.85}],\"style_ref\":[{\"url\":\"https://example.com/style1.jpg\",\"weight\":0.8}],\"character_ref\":{\"identity0\":{\"images\":[\"https://example.com/character1.jpg\"]}},\"modify_image_ref\":{\"url\":\"https://example.com/modify.jpg\",\"weight\":1.0}");
    }

    [Fact]
    [UpstreamTest(Provider + "should construct an image model with default configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_provider_posts_to_the_luma_api_with_bearer_auth()
    {
        var handler = new ParityHandler(call => call.Uri.AbsolutePath.EndsWith("/image", StringComparison.Ordinal)
            ? ParityHandler.Json(Queued)
            : call.Uri.AbsoluteUri == "https://api.lumalabs.ai/image.png"
                ? ParityHandler.Bytes(ImageBytes, null)
                : ParityHandler.Json(Completed(string.Empty).Replace(ImageUrl, "https://api.lumalabs.ai/image.png", StringComparison.Ordinal)));
        var model = LumaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, handler).ImageModel("luma-v1");
        Assert.IsType<LumaImageModel>(model);
        Assert.Equal("luma.image", model.Provider);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("https://api.lumalabs.ai/dream-machine/v1/generations/image", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("Bearer test-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Provider + "should respect custom configuration options", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_base_url_headers_and_handler_are_used()
    {
        var handler = new ParityHandler(call => call.Uri.AbsolutePath.EndsWith("/image", StringComparison.Ordinal)
            ? ParityHandler.Json(Queued)
            : call.Uri.AbsoluteUri == "https://custom-api.lumalabs.ai/image.png"
                ? ParityHandler.Bytes(ImageBytes, null)
                : ParityHandler.Json(Completed(string.Empty).Replace(ImageUrl, "https://custom-api.lumalabs.ai/image.png", StringComparison.Ordinal)));
        var options = new OpenAICompatibleOptions { ApiKey = "custom-api-key", BaseUrl = "https://custom-api.lumalabs.ai" };
        options.Headers["X-Custom-Header"] = "value";
        var model = LumaProvider.Create(options, handler).ImageModel("luma-v1");
        Assert.Equal("luma.image", model.Provider);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("https://custom-api.lumalabs.ai/dream-machine/v1/generations/image", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("Bearer custom-api-key", handler.Calls[0].Header("Authorization"));
        Assert.Equal("value", handler.Calls[0].Header("X-Custom-Header"));
    }

    private static async Task AssertCompletedResponseParses(string references)
    {
        var result = await Generate(Server(Completed(references)), new LumaImageRequest(Prompt));
        Assert.Equal(ImageBytes, Assert.Single(result.Images).Data);
    }

    private static void AssertUnsupported(ModelWarning warning, string feature, string details)
    {
        var unsupported = Assert.IsType<UnsupportedWarning>(warning);
        Assert.Equal("unsupported", unsupported.Type);
        Assert.Equal(feature, unsupported.Feature);
        Assert.Equal(details, unsupported.Details);
    }

    private static Task<LumaImageGeneration> Generate(ParityHandler handler, LumaImageRequest request)
    {
        var options = new OpenAICompatibleOptions { BaseUrl = "https://api.example.com" };
        options.Headers["api-key"] = "test-key";
        return LumaProvider.Create(options, handler).GenerateImageAsync("test-model", request, CancellationToken.None);
    }

    private static ParityHandler Server(string? status = null)
    {
        status ??= Completed(string.Empty);
        return new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            GenerationUrl => ParityHandler.Json(Queued),
            StatusUrl => ParityHandler.Json(status),
            ImageUrl => ParityHandler.Bytes(ImageBytes, null),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        });
    }

    private static string Completed(string references)
    {
        var extra = references.Length == 0 ? string.Empty : "," + references;
        return "{\"id\":\"test-generation-id\",\"generation_type\":\"image\",\"state\":\"completed\",\"created_at\":\"2024-01-01T00:00:00Z\",\"assets\":{\"image\":\"" + ImageUrl + "\"},\"model\":\"test-model\",\"request\":{\"generation_type\":\"image\",\"model\":\"test-model\",\"prompt\":\"A cute baby sea otter\"" + extra + "}}";
    }

    private static JsonElement Options(string luma)
    {
        return JsonDocument.Parse("{\"luma\":" + luma + "}").RootElement.Clone();
    }

    private static OpenAICompatibleImageFile[] Urls(params string[] urls)
    {
        return urls.Select(url => new OpenAICompatibleImageFile("image/*", null, url, null)).ToArray();
    }
}
