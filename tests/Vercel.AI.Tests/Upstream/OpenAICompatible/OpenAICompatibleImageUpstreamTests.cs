// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleImageUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_exposes_provider_and_model()
    {
        var model = Image(new UpstreamCapture());
        Assert.Equal("openai-compatible.image", model.Provider);
        Assert.Equal("dall-e-3", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should pass the correct parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_model_prompt_count_and_size()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        await Image(capture).DoGenerateAsync(new ImageCallOptions("a cat") { Count = 2, Size = "1024x1024" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("dall-e-3", body["model"]!.GetValue<string>());
        Assert.Equal("a cat", body["prompt"]!.GetValue<string>());
        Assert.Equal(2, body["n"]!.GetValue<int>());
        Assert.Equal("1024x1024", body["size"]!.GetValue<string>());
        Assert.EndsWith("/images/generations", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should use provider name from config for providerOptions key", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_use_the_camel_case_provider_name()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs");
        model.ProviderOptions = UpstreamChat.Bag("blackForestLabs", "{\"quality\":\"hd\"}");
        var result = await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal("hd", JsonNode.Parse(capture.Requests[0].Body)!["quality"]!.GetValue<string>());
        Assert.DoesNotContain(model.LastWarnings, warning => warning.Type == "deprecated");
        Assert.NotNull(result);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should pass typed image output options", Coverage = UpstreamCoverage.Covered)]
    public async Task Typed_image_options_are_merged_into_the_body()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        model.ProviderOptions = UpstreamChat.Bag("openaiCompatible", "{\"size\":\"auto\",\"quality\":\"high\",\"output_format\":\"jpeg\",\"output_compression\":80,\"background\":\"opaque\"}");
        await model.DoGenerateAsync(new ImageCallOptions("draw") { Size = "1024x1024" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("auto", body["size"]!.GetValue<string>());
        Assert.Equal("high", body["quality"]!.GetValue<string>());
        Assert.Equal("jpeg", body["output_format"]!.GetValue<string>());
        Assert.Equal(80, body["output_compression"]!.GetValue<int>());
        Assert.Equal("opaque", body["background"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should emit deprecated warning when raw provider options key is used for hyphenated provider", Coverage = UpstreamCoverage.Covered)]
    public async Task A_raw_hyphenated_image_key_warns()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs");
        model.ProviderOptions = UpstreamChat.Bag("black-forest-labs", "{\"quality\":\"hd\"}");
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Type == "deprecated" && warning.Message.Contains("blackForestLabs"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should not emit deprecated warning when camelCase provider options key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task A_camel_case_image_key_does_not_warn()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs");
        model.ProviderOptions = UpstreamChat.Bag("blackForestLabs", "{\"quality\":\"hd\"}");
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.DoesNotContain(model.LastWarnings, warning => warning.Type == "deprecated");
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should add warnings for unsupported settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Aspect_ratio_and_seed_warn()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        model.Seed = 123;
        await model.DoGenerateAsync(new ImageCallOptions("draw") { AspectRatio = "16:9" }, CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Type == "unsupported" && warning.Message == "This model does not support aspect ratio. Use `size` instead.");
        Assert.Contains(model.LastWarnings, warning => warning.Type == "unsupported" && warning.Message == "seed");
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["seed"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_calls_pass_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        model.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal("request-header-value", capture.Requests[0].Headers["Custom-Request-Header"]);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should handle API errors with custom error structure", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_error_selector_becomes_the_exception_message()
    {
        var capture = new UpstreamCapture
        {
            Status = System.Net.HttpStatusCode.BadRequest,
            ResponseBody = "{\"status\":\"error\",\"details\":{\"errorMessage\":\"Custom provider error format\",\"errorCode\":1234}}",
        };
        var model = Image(capture, configure: options =>
        {
            options.SelectErrorMessage = body => JsonNode.Parse(body!)!["details"]!["errorMessage"]!.GetValue<string>();
        });
        var error = await Assert.ThrowsAsync<BadRequestException>(() => model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None));
        Assert.Equal("Custom provider error format", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should handle API errors with default error structure", Coverage = UpstreamCoverage.Covered)]
    public async Task The_default_error_message_omits_param()
    {
        var capture = new UpstreamCapture
        {
            Status = System.Net.HttpStatusCode.BadRequest,
            ResponseBody = "{\"error\":{\"message\":\"Invalid prompt content\",\"type\":\"invalid_request_error\",\"param\":null,\"code\":null}}",
        };
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Image(capture).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None));
        Assert.Equal("Invalid prompt content", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.EndsWith("/images/generations", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate > usage::should map the usage object reported by the provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_usage_maps_input_output_and_total()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[{\"b64_json\":\"aGVsbG8=\"}],\"usage\":{\"input_tokens\":12,\"output_tokens\":4,\"total_tokens\":16}}" };
        var model = Image(capture);
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal(12, model.LastUsage!.InputTokens);
        Assert.Equal(4, model.LastUsage.OutputTokens);
        Assert.Equal(16, model.LastUsage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate > usage::should return undefined usage when the provider omits it", Coverage = UpstreamCoverage.Covered)]
    public async Task Omitted_image_usage_stays_unset()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Null(model.LastUsage);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate > usage::should map null usage fields to undefined", Coverage = UpstreamCoverage.Partial, Note = "Input and output stay unset. LanguageModelUsage computes a zero total when both counts are unset.")]
    public async Task Null_image_usage_fields_leave_counts_unset()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[],\"usage\":{\"input_tokens\":null,\"output_tokens\":null,\"total_tokens\":null}}" };
        var model = Image(capture);
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Null(model.LastUsage!.InputTokens);
        Assert.Null(model.LastUsage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate > usage::should ignore unknown usage fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_image_usage_fields_are_ignored()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[],\"usage\":{\"input_tokens\":3,\"output_tokens\":1,\"total_tokens\":4,\"extra\":9}}" };
        var model = Image(capture);
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal(3, model.LastUsage!.InputTokens);
        Assert.Equal(1, model.LastUsage.OutputTokens);
        Assert.Equal(4, model.LastUsage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should pass the user setting in the request", Coverage = UpstreamCoverage.Covered)]
    public async Task User_provider_option_is_sent()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        model.ProviderOptions = UpstreamChat.Bag("openaiCompatible", "{\"user\":\"test-user-id\"}");
        await model.DoGenerateAsync(new ImageCallOptions("draw") { Size = "1024x1024" }, CancellationToken.None);
        Assert.Equal("test-user-id", JsonNode.Parse(capture.Requests[0].Body)!["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > doGenerate::should not include user field in request when not set via provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task An_empty_provider_options_object_omits_user()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture);
        model.ProviderOptions = UpstreamChat.Bag("openaiCompatible", "{}");
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["user"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should send edit request with files", Coverage = UpstreamCoverage.Partial, Note = "The edits URL and image bytes are sent. Generated images are decoded bytes, so the raw base64 string is not returned.")]
    public async Task Edits_post_to_the_edits_route()
    {
        var capture = EditCapture();
        var model = Image(capture);
        var bytes = new byte[] { 137, 80, 78, 71 };
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", bytes, null, "cat.png") };
        await model.DoGenerateAsync(new ImageCallOptions("Turn the cat into a dog"), CancellationToken.None);
        Assert.EndsWith("/images/edits", capture.Requests[0].Uri!.AbsolutePath);
        Assert.Contains("cat.png", capture.Requests[0].Body);
        Assert.Contains("Turn the cat into a dog", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should send edit request with files and mask", Coverage = UpstreamCoverage.Partial, Note = "Mask bytes are sent on the edits route. The response image is decoded rather than returned as the raw base64 string.")]
    public async Task Edits_include_a_mask_part()
    {
        var capture = EditCapture();
        var model = Image(capture);
        var bytes = new byte[] { 137, 80, 78, 71 };
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", bytes, null, "image.png") };
        model.Mask = new OpenAICompatibleImageFile("image/png", bytes, null, "mask.png");
        await model.DoGenerateAsync(new ImageCallOptions("Add a flamingo to the pool"), CancellationToken.None);
        Assert.EndsWith("/images/edits", capture.Requests[0].Uri!.AbsolutePath);
        Assert.True(
            capture.Requests[0].Body.IndexOf("name=\"mask\"", StringComparison.Ordinal) >= 0
            || capture.Requests[0].Body.IndexOf("name=mask", StringComparison.Ordinal) >= 0);
        Assert.Contains("mask.png", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should send edit request with Uint8Array data", Coverage = UpstreamCoverage.Partial, Note = "Inline bytes are uploaded. The response image is decoded rather than returned as the raw base64 string.")]
    public async Task Edits_send_inline_bytes()
    {
        var capture = EditCapture();
        var model = Image(capture);
        var bytes = new byte[] { 104, 101, 108, 108, 111 };
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", bytes, null, "image-0.png") };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this image"), CancellationToken.None);
        Assert.Contains("hello", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should send edit request with multiple images", Coverage = UpstreamCoverage.Partial, Note = "Each file is a multipart image part. The response image is decoded rather than returned as the raw base64 string.")]
    public async Task Edits_send_every_image()
    {
        var capture = EditCapture();
        var model = Image(capture);
        var bytes = new byte[] { 137, 80, 78, 71 };
        model.Files = new[]
        {
            new OpenAICompatibleImageFile("image/png", bytes, null, "one.png"),
            new OpenAICompatibleImageFile("image/png", bytes, null, "two.png"),
        };
        await model.DoGenerateAsync(new ImageCallOptions("combine"), CancellationToken.None);
        Assert.Contains("one.png", capture.Requests[0].Body);
        Assert.Contains("two.png", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should map usage for edit requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Edit_usage_is_mapped()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[{\"b64_json\":\"aGVsbG8=\"}],\"usage\":{\"input_tokens\":5,\"output_tokens\":2,\"total_tokens\":7}}" };
        var model = Image(capture);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 1, 2, 3 }, null, "a.png") };
        await model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None);
        Assert.Equal(5, model.LastUsage!.InputTokens);
        Assert.Equal(2, model.LastUsage.OutputTokens);
        Assert.Equal(7, model.LastUsage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::OpenAICompatibleImageModel > Image Editing::should return undefined usage for edit requests when omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Omitted_edit_usage_stays_unset()
    {
        var model = Image(EditCapture());
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 1 }, null, "a.png") };
        await model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None);
        Assert.Null(model.LastUsage);
    }

    private static UpstreamCapture EditCapture()
    {
        return new UpstreamCapture { ResponseBody = "{\"data\":[{\"b64_json\":\"aGVsbG8=\"}]}" };
    }

    private static OpenAICompatibleImageModel Image(UpstreamCapture capture, string name = "openai-compatible", Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new OpenAICompatibleOptions
        {
            ProviderName = name,
            BaseUrl = "https://api.example.com/v1",
            ApiKey = "secret",
            SupportsImages = true,
        };
        configure?.Invoke(options);
        return (OpenAICompatibleImageModel)OpenAICompatibleProvider.Create(options, capture).ImageModel("dall-e-3");
    }
}
