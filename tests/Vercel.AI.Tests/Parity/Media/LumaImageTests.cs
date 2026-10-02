// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Luma;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class LumaImageTests
{
    private const string Prompt = "A cute baby sea otter";

    private const string CreateUrl = "https://api.example.com/dream-machine/v1/generations/image";

    private const string PollUrl = "https://api.example.com/dream-machine/v1/generations/test-generation-id";

    private const string ImageUrl = "https://api.example.com/image.png";

    private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should pass the correct parameters including aspect ratio",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_prompt_aspect_ratio_model_and_extra_fields()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request(Prompt, "16:9", Options("{\"additional_param\":\"value\"}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A cute baby sea otter\",\"aspect_ratio\":\"16:9\",\"model\":\"test-model\",\"additional_param\":\"value\"}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should call the correct urls in sequence",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_create_poll_and_download_urls()
    {
        var handler = Server();
        var result = await Model(handler).GenerateAsync(Request(Prompt, "16:9"), CancellationToken.None);

        Assert.Equal(3, handler.Calls.Count);
        Assert.Equal("POST", handler.Calls[0].Method);
        Assert.Equal(CreateUrl, handler.Calls[0].Url);
        Assert.Equal("GET", handler.Calls[1].Method);
        Assert.Equal(PollUrl, handler.Calls[1].Url);
        Assert.Equal("GET", handler.Calls[2].Method);
        Assert.Equal(ImageUrl, handler.Calls[2].Url);
        Assert.Null(handler.Calls[2].Header("api-key"));
        Assert.Equal(ImageBytes, result.Image);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = Server();
        var model = Model(handler, () => new Dictionary<string, string?> { ["Custom-Provider-Header"] = "provider-header-value" });
        await model.GenerateAsync(
            Request(Prompt, headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }),
            CancellationToken.None);

        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("custom-request-header"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should not pass providerOptions.{pollIntervalMillis,maxPollAttempts}",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_poll_settings_from_the_body()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request(Prompt, "16:9", Options("{\"pollIntervalMillis\":1000,\"maxPollAttempts\":5,\"additional_param\":\"value\"}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A cute baby sea otter\",\"aspect_ratio\":\"16:9\",\"model\":\"test-model\",\"additional_param\":\"value\"}",
            handler.Calls[0].Body);
        Assert.DoesNotContain("pollIntervalMillis", handler.Calls[0].Body, StringComparison.Ordinal);
        Assert.DoesNotContain("maxPollAttempts", handler.Calls[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should handle API errors",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_the_api_status_url_and_bodies()
    {
        var handler = new MediaHandler();
        handler.Text(CreateUrl, "Bad Request", HttpStatusCode.BadRequest, "text/plain");
        var error = await Assert.ThrowsAsync<LumaRequestException>(() => Model(handler).GenerateAsync(Request(Prompt), CancellationToken.None));

        Assert.Equal("Bad Request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(CreateUrl, error.Url);
        Assert.Equal("Bad Request", error.ResponseBody);
        Assert.Contains("A cute baby sea otter", error.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should handle failed generation state",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_generation_fails()
    {
        var handler = new MediaHandler();
        handler.Json(CreateUrl, "{\"id\":\"test-generation-id\",\"state\":\"queued\"}");
        handler.Json(PollUrl, "{\"id\":\"test-generation-id\",\"state\":\"failed\",\"failure_reason\":\"Generation failed\"}");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(handler).GenerateAsync(Request(Prompt), CancellationToken.None));

        Assert.Contains("Image generation failed.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate > warnings::should return warnings for unsupported parameters",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_for_seed_and_size()
    {
        var result = await Model(Server()).GenerateAsync(
            Request(Prompt, size: "1024x1024", seed: 123),
            CancellationToken.None);

        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("seed", result.Warnings[0].Feature);
        Assert.Equal("This model does not support the `seed` option.", result.Warnings[0].Details);
        Assert.Equal("unsupported", result.Warnings[1].Type);
        Assert.Equal("size", result.Warnings[1].Feature);
        Assert.Equal("This model does not support the `size` option. Use `aspectRatio` instead.", result.Warnings[1].Details);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate > response metadata::should include timestamp, headers and modelId in response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_timestamp_model_id_and_response_headers()
    {
        var model = Model(Server());
        var stamp = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        model.Clock = () => stamp;
        var result = await model.GenerateAsync(Request(Prompt), CancellationToken.None);

        Assert.Equal(stamp, result.Timestamp);
        Assert.Equal("test-model", result.ModelId);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > constructor::should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered)]
    public void Exposes_provider_model_version_and_max_images()
    {
        var model = Model(new MediaHandler());

        Assert.Equal("luma", model.Provider);
        Assert.Equal("test-model", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send image by default when URL file is provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_an_image_reference_at_the_default_weight()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request("A warrior with sunglasses", files: Urls("https://example.com/input.jpg")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A warrior with sunglasses\",\"model\":\"test-model\",\"image\":[{\"url\":\"https://example.com/input.jpg\",\"weight\":0.85}]}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send modify_image when referenceType is set",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_modify_image_at_weight_one()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request("Transform flowers to sunflowers", files: Urls("https://example.com/input.jpg"), options: Options("{\"referenceType\":\"modify_image\"}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"Transform flowers to sunflowers\",\"model\":\"test-model\",\"modify_image\":{\"url\":\"https://example.com/input.jpg\",\"weight\":1}}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send style when referenceType is style",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_style_reference()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request("A dog in this style", files: Urls("https://example.com/style.jpg"), options: Options("{\"referenceType\":\"style\"}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A dog in this style\",\"model\":\"test-model\",\"style\":[{\"url\":\"https://example.com/style.jpg\",\"weight\":0.8}]}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send character when referenceType is character",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Groups_character_images_under_the_default_identity()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request(
                "A warrior",
                files: Urls("https://example.com/person1.jpg", "https://example.com/person2.jpg"),
                options: Options("{\"referenceType\":\"character\"}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A warrior\",\"model\":\"test-model\",\"character\":{\"identity0\":{\"images\":[\"https://example.com/person1.jpg\",\"https://example.com/person2.jpg\"]}}}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send character with custom identity id from images config",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_character_identity_from_images_config()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request(
                "A woman with a cat",
                files: Urls("https://example.com/person.jpg"),
                options: Options("{\"referenceType\":\"character\",\"images\":[{\"id\":\"identity0\"}]}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"A woman with a cat\",\"model\":\"test-model\",\"character\":{\"identity0\":{\"images\":[\"https://example.com/person.jpg\"]}}}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should send character with multiple identities from images config",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_one_identity_per_configured_image()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request(
                "Two people talking",
                files: Urls("https://example.com/person1.jpg", "https://example.com/person2.jpg"),
                options: Options("{\"referenceType\":\"character\",\"images\":[{\"id\":\"identity0\"},{\"id\":\"identity1\"}]}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"Two people talking\",\"model\":\"test-model\",\"character\":{\"identity0\":{\"images\":[\"https://example.com/person1.jpg\"]},\"identity1\":{\"images\":[\"https://example.com/person2.jpg\"]}}}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should support multiple images for image",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_multiple_image_references()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request("Combine these concepts", files: Urls("https://example.com/input1.jpg", "https://example.com/input2.jpg")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"Combine these concepts\",\"model\":\"test-model\",\"image\":[{\"url\":\"https://example.com/input1.jpg\",\"weight\":0.85},{\"url\":\"https://example.com/input2.jpg\",\"weight\":0.85}]}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should use custom weights from images config",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_configured_image_weight()
    {
        var handler = Server();
        await Model(handler).GenerateAsync(
            Request("Styled image", files: Urls("https://example.com/input.jpg"), options: Options("{\"images\":[{\"weight\":0.5}]}")),
            CancellationToken.None);

        Assert.Equal(
            "{\"prompt\":\"Styled image\",\"model\":\"test-model\",\"image\":[{\"url\":\"https://example.com/input.jpg\",\"weight\":0.5}]}",
            handler.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should throw error when mask is provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_mask_url()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(Server()).GenerateAsync(
            Request("Replace sky with sunset", files: Urls("https://example.com/input.jpg"), mask: LumaReferenceImage.FromUrl("https://example.com/mask.png")),
            CancellationToken.None));

        Assert.Contains("Luma AI does not support mask-based image editing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should throw error when base64 file data is provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_inline_image_bytes()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(Server()).GenerateAsync(
            Request("Edit this image", files: new[] { LumaReferenceImage.FromBytes(new byte[] { 137, 80, 78, 71 }) }),
            CancellationToken.None));

        Assert.Contains("Luma AI only supports URL-based images", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should throw error when base64 mask data is provided",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_inline_mask_before_file_checks()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(Server()).GenerateAsync(
            Request(
                "Edit with mask",
                files: Urls("https://example.com/input.jpg"),
                mask: LumaReferenceImage.FromBytes(new byte[] { 255, 255, 255, 0 })),
            CancellationToken.None));

        Assert.Contains("Luma AI does not support mask-based image editing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should throw error when more than 4 images for image",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_more_than_four_image_references()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(Server()).GenerateAsync(
            Request(
                "Too many images",
                files: Urls(
                    "https://example.com/1.jpg",
                    "https://example.com/2.jpg",
                    "https://example.com/3.jpg",
                    "https://example.com/4.jpg",
                    "https://example.com/5.jpg")),
            CancellationToken.None));

        Assert.Contains("Luma AI image supports up to 4 reference images", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > Image Editing::should throw error when multiple files for modify_image",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_multiple_modify_image_inputs()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(Server()).GenerateAsync(
            Request(
                "Edit multiple images",
                files: Urls("https://example.com/input1.jpg", "https://example.com/input2.jpg"),
                options: Options("{\"referenceType\":\"modify_image\"}")),
            CancellationToken.None));

        Assert.Contains("Luma AI modify_image only supports a single input image", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > response schema validation::should parse response with image references",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_a_completed_response_that_includes_image_refs()
    {
        await AssertParsed(Completed("\"image_ref\":[{\"url\":\"https://example.com/ref1.jpg\",\"weight\":0.85}]"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > response schema validation::should parse response with style references",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_a_completed_response_that_includes_style_refs()
    {
        await AssertParsed(Completed("\"style_ref\":[{\"url\":\"https://example.com/style1.jpg\",\"weight\":0.8}]"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > response schema validation::should parse response with character references",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_a_completed_response_that_includes_character_refs()
    {
        await AssertParsed(Completed("\"character_ref\":{\"identity0\":{\"images\":[\"https://example.com/character1.jpg\"]}}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > response schema validation::should parse response with modify image reference",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_a_completed_response_that_includes_a_modify_image_ref()
    {
        await AssertParsed(Completed("\"modify_image_ref\":{\"url\":\"https://example.com/modify.jpg\",\"weight\":1.0}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-image-model.test.ts::LumaImageModel > response schema validation::should parse response with multiple reference types",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_a_completed_response_that_includes_every_reference_type()
    {
        await AssertParsed(Completed(
            "\"image_ref\":[{\"url\":\"https://example.com/ref1.jpg\",\"weight\":0.85}]," +
            "\"style_ref\":[{\"url\":\"https://example.com/style1.jpg\",\"weight\":0.8}]," +
            "\"character_ref\":{\"identity0\":{\"images\":[\"https://example.com/character1.jpg\"]}}," +
            "\"modify_image_ref\":{\"url\":\"https://example.com/modify.jpg\",\"weight\":1.0}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-provider.test.ts::createLuma > image::should construct an image model with default configuration",
        Coverage = UpstreamCoverage.Covered)]
    public void Constructs_an_image_model_with_the_default_origin()
    {
        var model = Assert.IsType<LumaImageModel>(LumaProvider.Create().ImageModel("luma-v1"));

        Assert.Equal("luma.image", model.Provider);
        Assert.Equal("luma-v1", model.ModelId);
        Assert.Equal("https://api.lumalabs.ai", model.BaseUrl);
        Assert.Equal("v4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        "packages/luma/src/luma-provider.test.ts::createLuma > image::should respect custom configuration options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_key_origin_headers_and_client()
    {
        var handler = new MediaHandler();
        var create = "https://custom-api.lumalabs.ai/dream-machine/v1/generations/image";
        var poll = "https://custom-api.lumalabs.ai/dream-machine/v1/generations/test-generation-id";
        handler.Json(create, "{\"id\":\"test-generation-id\",\"state\":\"queued\"}");
        handler.Json(poll, "{\"id\":\"test-generation-id\",\"state\":\"completed\",\"assets\":{\"image\":\"" + ImageUrl + "\"}}");
        handler.Bytes(ImageUrl, ImageBytes, "image/png");
        var options = new OpenAICompatibleOptions
        {
            ApiKey = "custom-api-key",
            BaseUrl = "https://custom-api.lumalabs.ai",
        };
        options.Headers["X-Custom-Header"] = "value";
        var model = Assert.IsType<LumaImageModel>(LumaProvider.Create(options, handler).ImageModel("luma-v1"));
        var headers = model.Headers();

        Assert.Equal("luma.image", model.Provider);
        Assert.Equal("https://custom-api.lumalabs.ai", model.BaseUrl);
        Assert.Equal("Bearer custom-api-key", headers["Authorization"]);
        Assert.Equal("value", headers["X-Custom-Header"]);
        Assert.Contains("ai-sdk/luma/0.0.0-test", headers["User-Agent"], StringComparison.Ordinal);

        await model.GenerateAsync(new LumaImageRequest("A cat"), CancellationToken.None);
        Assert.Equal(create, handler.Calls[0].Url);
        Assert.Equal("Bearer custom-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("value", handler.Calls[0].Header("x-custom-header"));
    }

    private static async Task AssertParsed(string pollJson)
    {
        var handler = new MediaHandler();
        handler.Json(CreateUrl, "{\"id\":\"test-generation-id\",\"state\":\"queued\"}");
        handler.Json(PollUrl, pollJson);
        handler.Bytes(ImageUrl, ImageBytes, "image/png");
        var result = await Model(handler).GenerateAsync(Request(Prompt), CancellationToken.None);
        Assert.Equal(ImageBytes, result.Image);
    }

    private static string Completed(string requestFields)
    {
        return "{\"id\":\"test-generation-id\",\"state\":\"completed\",\"assets\":{\"image\":\"" + ImageUrl + "\"},\"request\":{" + requestFields + "}}";
    }

    private static MediaHandler Server()
    {
        var handler = new MediaHandler();
        handler.Json(CreateUrl, "{\"id\":\"test-generation-id\",\"generation_type\":\"image\",\"state\":\"queued\",\"model\":\"test-model\"}");
        handler.Json(PollUrl, Completed("\"prompt\":\"A cute baby sea otter\""));
        handler.Bytes(ImageUrl, ImageBytes, "image/png");
        return handler;
    }

    private static LumaImageModel Model(MediaHandler handler, Func<IReadOnlyDictionary<string, string?>>? headers = null)
    {
        return new LumaImageModel(
            new HttpClient(handler, disposeHandler: false),
            "test-model",
            "luma",
            "https://api.example.com",
            headers ?? (() => new Dictionary<string, string?> { ["api-key"] = "test-key" }));
    }

    private static LumaImageRequest Request(
        string prompt,
        string? aspect = null,
        JsonElement? options = null,
        IReadOnlyList<LumaReferenceImage>? files = null,
        LumaReferenceImage? mask = null,
        string? size = null,
        int? seed = null,
        IDictionary<string, string>? headers = null)
    {
        return new LumaImageRequest(prompt)
        {
            AspectRatio = aspect,
            ProviderOptions = options,
            Files = files,
            Mask = mask,
            Size = size,
            Seed = seed,
            Headers = headers,
        };
    }

    private static IReadOnlyList<LumaReferenceImage> Urls(params string[] urls)
    {
        var images = new List<LumaReferenceImage>(urls.Length);
        foreach (var url in urls)
        {
            images.Add(LumaReferenceImage.FromUrl(url));
        }

        return images;
    }

    private static JsonElement Options(string json)
    {
        using (var document = JsonDocument.Parse(json))
        {
            return document.RootElement.Clone();
        }
    }
}
