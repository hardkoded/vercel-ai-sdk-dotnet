// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Fireworks;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class FireworksUpstreamTests
{
    private const string Flux = "accounts/fireworks/models/flux-1-dev-fp8";
    private const string Kontext = "accounts/fireworks/models/flux-kontext-pro";
    private const string Sized = "accounts/fireworks/models/playground-v2-5-1024px-aesthetic";
    private const string ImageTests = "packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > ";
    private const string AsyncTests = "packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > async models (flux-kontext-*)::";
    private const string ProviderTests = "packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > ";

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate::should pass the correct parameters including aspect ratio and seed", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_send_aspect_ratio_seed_and_samples()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Flux);
        model.Seed = 4;
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter") { AspectRatio = "16:9", Count = 2 }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("A cute baby sea otter", body["prompt"]!.GetValue<string>());
        Assert.Equal("16:9", body["aspect_ratio"]!.GetValue<string>());
        Assert.Equal(4, body["seed"]!.GetValue<int>());
        Assert.Equal(2, body["samples"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate::should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Workflow_models_post_to_text_to_image()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        await Image(capture, Flux).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.EndsWith("/workflows/" + Flux + "/text_to_image", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate::should handle empty response body", Coverage = UpstreamCoverage.Covered)]
    public async Task An_empty_image_body_fails()
    {
        var capture = new UpstreamCapture { ResponseBody = string.Empty };
        var error = await Assert.ThrowsAsync<ApiException>(() => Image(capture, Flux).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None));
        Assert.Equal("Response body is empty", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate::should handle size parameter for supported models", Coverage = UpstreamCoverage.Covered)]
    public async Task Size_supporting_models_send_width_and_height()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        await Image(capture, Sized).DoGenerateAsync(new ImageCallOptions("draw") { Size = "1024x768" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("1024", body["width"]!.GetValue<string>());
        Assert.Equal("768", body["height"]!.GetValue<string>());
        Assert.EndsWith("/image_generation/" + Sized, capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate > warnings::should return size warning on workflow model", Coverage = UpstreamCoverage.Covered)]
    public async Task Workflow_models_warn_about_size()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Flux);
        await model.DoGenerateAsync(new ImageCallOptions("draw") { Size = "512x512" }, CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Message == "This model does not support the `size` option. Use `aspectRatio` instead.");
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > doGenerate > warnings::should return aspectRatio warning on size-supporting model", Coverage = UpstreamCoverage.Covered)]
    public async Task Size_models_warn_about_aspect_ratio()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Sized);
        await model.DoGenerateAsync(new ImageCallOptions("draw") { AspectRatio = "1:1" }, CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Message == "This model does not support the `aspectRatio` option.");
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > Image Editing::should warn when multiple files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Extra_reference_images_warn()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Flux);
        model.Files = new[]
        {
            new OpenAICompatibleImageFile("image/png", new byte[] { 1 }, null, "a.png"),
            new OpenAICompatibleImageFile("image/png", new byte[] { 2 }, null, "b.png"),
        };
        await model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Message == "Fireworks only supports a single input image. Additional images are ignored.");
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > async models (flux-kontext-*)::should submit request and poll for result", Coverage = UpstreamCoverage.Covered)]
    public async Task Kontext_models_poll_until_ready()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        capture.Bodies.Enqueue("{\"request_id\":\"req-1\"}");
        capture.Bodies.Enqueue("{\"status\":\"Ready\",\"result\":{\"sample\":\"https://images.example/sample.png\"}}");
        var result = await Image(capture, Kontext).DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None);
        Assert.EndsWith("/workflows/" + Kontext, capture.Requests[0].Uri!.AbsolutePath);
        Assert.EndsWith("/get_result", capture.Requests[1].Uri!.AbsolutePath);
        Assert.Equal("https://images.example/sample.png", capture.Requests[2].Uri!.AbsoluteUri);
        Assert.False(capture.Requests[2].Headers.ContainsKey("Authorization"));
        Assert.Equal("PNG", System.Text.Encoding.UTF8.GetString(result.Images[0].Data!));
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > async models (flux-kontext-*)::should throw error when generation fails", Coverage = UpstreamCoverage.Covered)]
    public async Task A_failed_poll_throws()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"status\":\"Error\"}" };
        capture.Bodies.Enqueue("{\"request_id\":\"req-1\"}");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Image(capture, Kontext).DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None));
        Assert.Equal("Fireworks image generation failed with status: Error", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > async models (flux-kontext-*)::should throw error when Ready but missing sample", Coverage = UpstreamCoverage.Covered)]
    public async Task Ready_without_a_sample_throws()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"status\":\"Ready\",\"result\":{}}" };
        capture.Bodies.Enqueue("{\"request_id\":\"req-1\"}");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Image(capture, Kontext).DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None));
        Assert.Equal("Fireworks poll response is Ready but missing result.sample", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-image-model.test.ts::FireworksImageModel > async models (flux-kontext-*)::should enforce pollTimeoutMillis while a polling request is pending", Coverage = UpstreamCoverage.Covered)]
    public async Task Polling_stops_at_the_timeout()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"status\":\"Pending\"}" };
        capture.Bodies.Enqueue("{\"request_id\":\"req-1\"}");
        var model = Image(capture, Kontext);
        model.PollTimeout = TimeSpan.FromMilliseconds(1);
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None));
        Assert.Equal("Fireworks image generation timed out after 1ms", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should set supportsStructuredOutputs so response_format json_schema is forwarded", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_structured_outputs_send_json_schema()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        options.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"responseFormat\":\"json\"}");
        var model = (OpenAICompatibleLanguageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.True(model.SupportsStructuredOutputs);
        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel > errorStructure::should parse the object error envelope Fireworks returns", Coverage = UpstreamCoverage.Covered)]
    public async Task An_object_error_uses_its_message()
    {
        var capture = Error("{\"error\":{\"message\":\"nope\",\"type\":\"invalid_request_error\",\"param\":null,\"code\":1}}");
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("nope", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel > errorStructure::should still accept a bare string error", Coverage = UpstreamCoverage.Covered)]
    public async Task A_string_error_is_the_message()
    {
        var capture = Error("{\"error\":\"nope\"}");
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("nope", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should pass transformRequestBody that converts thinking options", Coverage = UpstreamCoverage.Covered)]
    public void Thinking_budget_tokens_are_renamed()
    {
        var body = Transform("{\"model\":\"test-model\",\"messages\":[],\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":2048},\"reasoningHistory\":\"interleaved\"}");
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(2048, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Equal("interleaved", body["reasoning_history"]!.GetValue<string>());
        Assert.Null(body["reasoningHistory"]);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should map promptCacheKey to prompt_cache_key", Coverage = UpstreamCoverage.Covered)]
    public void Prompt_cache_key_is_renamed()
    {
        var body = Transform("{\"model\":\"test-model\",\"messages\":[],\"promptCacheKey\":\"session-123\"}");
        Assert.Equal("session-123", body["prompt_cache_key"]!.GetValue<string>());
        Assert.Null(body["promptCacheKey"]);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should prefer promptCacheKey over raw prompt_cache_key", Coverage = UpstreamCoverage.Covered)]
    public void Camel_case_prompt_cache_key_wins()
    {
        var body = Transform("{\"model\":\"test-model\",\"messages\":[],\"prompt_cache_key\":\"raw-session\",\"promptCacheKey\":\"typed-session\"}");
        Assert.Equal("typed-session", body["prompt_cache_key"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should map serviceTier to service_tier", Coverage = UpstreamCoverage.Covered)]
    public void Service_tier_is_renamed()
    {
        var body = Transform("{\"model\":\"test-model\",\"messages\":[],\"serviceTier\":\"priority\"}");
        Assert.Equal("priority", body["service_tier"]!.GetValue<string>());
        Assert.Null(body["serviceTier"]);
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should remap reasoning_effort xhigh to high", Coverage = UpstreamCoverage.Covered)]
    public void Xhigh_reasoning_becomes_high()
    {
        Assert.Equal("high", Transform("{\"reasoning_effort\":\"xhigh\"}")["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should remap reasoning_effort minimal to low", Coverage = UpstreamCoverage.Covered)]
    public void Minimal_reasoning_becomes_low()
    {
        Assert.Equal("low", Transform("{\"reasoning_effort\":\"minimal\"}")["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should pass through supported reasoning_effort values unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Medium_reasoning_is_unchanged()
    {
        Assert.Equal("medium", Transform("{\"reasoning_effort\":\"medium\"}")["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > image::should respect custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_base_url_is_used_for_images()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var provider = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", BaseUrl = "https://custom.fireworks.test/v1" }, capture);
        await ((FireworksImageModel)provider.ImageModel(Flux)).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal("https://custom.fireworks.test/v1/workflows/" + Flux + "/text_to_image", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_send_json_and_custom_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var provider = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (FireworksImageModel)provider.ImageModel(Flux);
        model.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        var headers = capture.Requests[0].Headers;
        Assert.StartsWith("application/json", headers["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", headers["Custom-Request-Header"]);
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_api_errors_keep_the_status_and_body()
    {
        var capture = new UpstreamCapture { Status = HttpStatusCode.BadRequest, ResponseBody = "Bad Request" };
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Image(capture, Flux).DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None));
        Assert.Equal("Bad Request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Bad Request", error.ResponseBody);
        Assert.Equal("https://api.fireworks.ai/inference/v1/workflows/" + Flux + "/text_to_image", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Equal("A cute baby sea otter", JsonNode.Parse(capture.Requests[0].Body)!["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should pass typed workflow provider options to the API", Coverage = UpstreamCoverage.Covered)]
    public async Task Workflow_provider_options_are_merged_into_the_body()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Flux);
        model.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"guidance_scale\":4.5,\"num_inference_steps\":8}");
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter") { AspectRatio = "1:1" }, CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"aspect_ratio\":\"1:1\",\"guidance_scale\":4.5,\"num_inference_steps\":8,\"prompt\":\"A cute baby sea otter\",\"samples\":1}");
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should pass typed image_generation provider options to the API", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_provider_options_are_merged_into_the_body()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Sized);
        model.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"cfg_scale\":10,\"steps\":30}");
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter") { Size = "1024x1024" }, CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"cfg_scale\":10,\"height\":\"1024\",\"prompt\":\"A cute baby sea otter\",\"samples\":1,\"steps\":30,\"width\":\"1024\"}");
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should reject invalid provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Invalid_provider_options_are_rejected()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Image(capture, Flux);
        model.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"output_format\":\"webp\"}");
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None));
        Assert.Contains("invalid fireworks provider options", error.Message, StringComparison.Ordinal);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should respect the abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancelling_the_token_aborts_the_call()
    {
        using var cancellation = new CancellationTokenSource();
        var model = (FireworksImageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new PendingHandler()).ImageModel(Flux);
        var call = model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAsync<ApiUserAbortException>(() => call);
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should use custom fetch function when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task The_custom_handler_receives_the_request()
    {
        var capture = new UpstreamCapture { ResponseBody = "mock-image-data" };
        var result = await Image(capture, Flux).DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        Assert.Single(capture.Requests);
        Assert.Equal("mock-image-data", System.Text.Encoding.UTF8.GetString(result.Images[0].Data!));
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate::should pass samples parameter to API", Coverage = UpstreamCoverage.Covered)]
    public async Task Count_is_sent_as_samples()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        await Image(capture, Flux).DoGenerateAsync(new ImageCallOptions("A cute baby sea otter") { Count = 42 }, CancellationToken.None);
        Assert.Equal(42, JsonNode.Parse(capture.Requests[0].Body)!["samples"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate > response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_calls_record_the_timestamp_and_headers()
    {
        var date = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = Image(new UpstreamCapture { ResponseBody = "PNG" }, Flux);
        model.Clock = () => date;
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        Assert.Equal(date, model.LastTimestamp);
        Assert.Equal(Flux, model.ModelId);
        Assert.NotEmpty(model.LastResponseHeaders);
    }

    [Fact]
    [UpstreamTest(ImageTests + "doGenerate > response metadata::should include response headers from API call", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_calls_keep_the_response_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "test-binary-content", MediaType = "image/png" };
        capture.ResponseHeaders["x-request-id"] = "test-request-id";
        var model = Image(capture, Flux);
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        Assert.Equal("test-request-id", model.LastResponseHeaders["x-request-id"]);
        Assert.StartsWith("image/png", model.LastResponseHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("19", model.LastResponseHeaders["Content-Length"]);
    }

    [Fact]
    [UpstreamTest(ImageTests + "constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_exposes_provider_model_and_limit()
    {
        var model = Image(new UpstreamCapture(), Flux);
        Assert.Equal("fireworks.image", model.Provider);
        Assert.Equal(Flux, model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should send edit request with files as data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task Edit_bytes_are_sent_as_a_data_uri()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null) };
        await model.DoGenerateAsync(new ImageCallOptions("Turn the cat into a dog"), CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"input_image\":\"data:image/png;base64,iVBORw==\",\"prompt\":\"Turn the cat into a dog\",\"samples\":1}");
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should use correct URL for Kontext model (no text_to_image suffix)", Coverage = UpstreamCoverage.Covered)]
    public async Task Kontext_edits_post_to_the_workflow_url()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null) };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this image"), CancellationToken.None);
        Assert.Equal("https://api.fireworks.ai/inference/v1/workflows/" + Kontext, capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should send edit request with URL-based file", Coverage = UpstreamCoverage.Covered)]
    public async Task Edit_urls_are_sent_as_is()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", null, "https://example.com/input.png", null) };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this image"), CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"input_image\":\"https://example.com/input.png\",\"prompt\":\"Edit this image\",\"samples\":1}");
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should send edit request with base64 string data", Coverage = UpstreamCoverage.Covered)]
    public async Task Edit_base64_data_keeps_its_encoding()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAE="), null, null) };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this image"), CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"input_image\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAE=\",\"prompt\":\"Edit this image\",\"samples\":1}");
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should warn when mask is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_mask_warns()
    {
        var model = Image(KontextCapture(), Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null) };
        model.Mask = true;
        await model.DoGenerateAsync(new ImageCallOptions("Edit with mask"), CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Type == "unsupported" && warning.Message == "Fireworks Kontext models do not support explicit masks. Use the prompt to describe the areas to edit.");
    }

    [Fact]
    [UpstreamTest(ImageTests + "Image Editing::should pass provider options with edit request", Coverage = UpstreamCoverage.Covered)]
    public async Task Edit_requests_include_provider_options()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null) };
        model.Seed = 42;
        model.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"output_format\":\"jpeg\",\"safety_tolerance\":2}");
        await model.DoGenerateAsync(new ImageCallOptions("Edit with options") { AspectRatio = "16:9" }, CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"aspect_ratio\":\"16:9\",\"input_image\":\"data:image/png;base64,iVBORw==\",\"output_format\":\"jpeg\",\"prompt\":\"Edit with options\",\"safety_tolerance\":2,\"samples\":1,\"seed\":42}");
    }

    [Fact]
    [UpstreamTest(AsyncTests + "should poll multiple times until Ready", Coverage = UpstreamCoverage.Covered)]
    public async Task Kontext_models_poll_until_the_status_is_ready()
    {
        var capture = new UpstreamCapture { ResponseBody = "async-image-content" };
        capture.Bodies.Enqueue("{\"request_id\":\"test-request-123\"}");
        capture.Bodies.Enqueue("{\"id\":\"test-request-123\",\"status\":\"Pending\",\"result\":null}");
        capture.Bodies.Enqueue("{\"id\":\"test-request-123\",\"status\":\"Pending\",\"result\":null}");
        capture.Bodies.Enqueue("{\"id\":\"test-request-123\",\"status\":\"Ready\",\"result\":{\"sample\":\"https://example.com/image.png\"}}");
        var model = Image(capture, Kontext);
        model.PollInterval = TimeSpan.FromMilliseconds(10);
        var result = await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        Assert.Equal(3, capture.Requests.Count(request => request.Uri!.AbsolutePath.EndsWith("/get_result", StringComparison.Ordinal)));
        Assert.Single(result.Images);
    }

    [Fact]
    [UpstreamTest(AsyncTests + "should pass provider options to submit request", Coverage = UpstreamCoverage.Covered)]
    public async Task Kontext_submit_includes_provider_options()
    {
        var capture = KontextCapture();
        var model = Image(capture, Kontext);
        model.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"safety_tolerance\":6,\"output_format\":\"jpeg\",\"prompt_upsampling\":true,\"webhook_url\":\"https://example.com/webhook\",\"webhook_secret\":\"secret\",\"input_image\":\"base64-image-data\"}");
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"prompt\":\"A cute baby sea otter\",\"samples\":1,\"safety_tolerance\":6,\"output_format\":\"jpeg\",\"prompt_upsampling\":true,\"webhook_url\":\"https://example.com/webhook\",\"webhook_secret\":\"secret\",\"input_image\":\"base64-image-data\"}");
    }

    [Fact]
    [UpstreamTest(AsyncTests + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Kontext_calls_record_the_timestamp_and_headers()
    {
        var date = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = Image(KontextCapture(), Kontext);
        model.Clock = () => date;
        await model.DoGenerateAsync(new ImageCallOptions("A cute baby sea otter"), CancellationToken.None);
        Assert.Equal(date, model.LastTimestamp);
        Assert.Equal(Kontext, model.ModelId);
        Assert.NotEmpty(model.LastResponseHeaders);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "createFireworks::should create a FireworksProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_options_target_the_fireworks_api()
    {
        var capture = new UpstreamCapture();
        var provider = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("model-id")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("FIREWORKS_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.Equal("https://api.fireworks.ai/inference/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/fireworks/0.0.0", capture.Requests[0].Headers["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "createFireworks::should create a FireworksProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_key_base_url_and_headers_are_used()
    {
        var capture = new UpstreamCapture();
        var provider = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url" }, capture);
        provider.Options.Headers["Custom-Header"] = "value";
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("model-id")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://custom.url/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("value", capture.Requests[0].Headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "createFireworks::should return a chat model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_a_chat_model()
    {
        var model = Assert.IsType<OpenAICompatibleLanguageModel>(FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("foo-model-id"));
        Assert.Equal("foo-model-id", model.ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel::should construct a chat model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Chat_model_uses_the_fireworks_chat_provider()
    {
        var model = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateChatModel("fireworks-chat-model");
        Assert.Equal("fireworks.chat", model.Provider);
        Assert.Equal("fireworks-chat-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel::should set includeUsage so streaming responses report token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_streams_request_usage()
    {
        var capture = Stream("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}");
        await UpstreamChat.Read(Chat(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel > errorStructure::should parse an error envelope with a null param and numeric code", Coverage = UpstreamCoverage.Covered)]
    public async Task An_error_with_null_param_and_numeric_code_uses_its_message()
    {
        var capture = Error("{\"error\":{\"message\":\"The API key you provided is invalid.\",\"param\":null,\"code\":401,\"type\":\"error\"}}");
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("The API key you provided is invalid.", error.Message);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel > errorStructure::should ignore unknown keys alongside the error object", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_keys_next_to_the_error_are_ignored()
    {
        var capture = Error("{\"error\":{\"message\":\"Model not found\",\"code\":\"NOT_FOUND\"},\"request_id\":\"chatcmpl-abc123\"}");
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("Model not found", error.Message);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel > errorStructure::should reject an error object without a message", Coverage = UpstreamCoverage.Covered)]
    public async Task An_error_object_without_a_message_is_not_used_as_the_message()
    {
        const string body = "{\"error\":{\"code\":\"NOT_FOUND\"}}";
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(Error(body)).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal(body, error.Message);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel::should handle thinking without budgetTokens", Coverage = UpstreamCoverage.Covered)]
    public void Thinking_without_budget_keeps_only_the_type()
    {
        JsonAssert.Equal(Transform("{\"model\":\"test-model\",\"messages\":[],\"thinking\":{\"type\":\"enabled\"}}"), "{\"model\":\"test-model\",\"messages\":[],\"thinking\":{\"type\":\"enabled\"}}");
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel::should prefer serviceTier over raw service_tier", Coverage = UpstreamCoverage.Covered)]
    public void Camel_case_service_tier_wins()
    {
        JsonAssert.Equal(Transform("{\"model\":\"test-model\",\"messages\":[],\"service_tier\":\"standard\",\"serviceTier\":\"priority\"}"), "{\"model\":\"test-model\",\"messages\":[],\"service_tier\":\"priority\"}");
    }

    [Fact]
    [UpstreamTest(ProviderTests + "chatModel::should handle request without thinking options", Coverage = UpstreamCoverage.Covered)]
    public void A_body_without_thinking_is_unchanged()
    {
        JsonAssert.Equal(Transform("{\"model\":\"test-model\",\"messages\":[]}"), "{\"model\":\"test-model\",\"messages\":[]}");
    }

    [Fact]
    [UpstreamTest(ProviderTests + "completionModel::should construct a completion model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Completion_model_uses_the_fireworks_completion_provider()
    {
        var model = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CompletionModel("fireworks-completion-model");
        Assert.Equal("fireworks.completion", model.Provider);
        Assert.Equal("fireworks-completion-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "completionModel::should set includeUsage so streaming responses report token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Completion_streams_request_usage()
    {
        var capture = Stream("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}]}");
        var model = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).CompletionModel("test-model");
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(ProviderTests + "embeddingModel::should construct a text embedding model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_model_uses_the_fireworks_embedding_provider()
    {
        var model = Assert.IsType<OpenAICompatibleEmbeddingModel>(FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("fireworks-embedding-model"));
        Assert.Equal("fireworks.embedding", model.Provider);
        Assert.Equal("fireworks-embedding-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "image::should construct an image model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_model_uses_the_fireworks_image_provider_and_base_url()
    {
        var capture = new UpstreamCapture { ResponseBody = "PNG" };
        var model = Assert.IsType<FireworksImageModel>(FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(Flux));
        Assert.Equal("fireworks.image", model.Provider);
        await model.DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.StartsWith("https://api.fireworks.ai/inference/v1/", capture.Requests[0].Uri!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ProviderTests + "image::should use default settings when none provided", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_has_default_settings()
    {
        var model = Assert.IsType<FireworksImageModel>(FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel(Flux));
        Assert.Equal(Flux, model.ModelId);
        Assert.Equal(TimeSpan.FromMilliseconds(500), model.PollInterval);
        Assert.Equal(TimeSpan.FromMilliseconds(120000), model.PollTimeout);
    }

    private static UpstreamCapture KontextCapture()
    {
        var capture = new UpstreamCapture { ResponseBody = "edited-image-data" };
        capture.Bodies.Enqueue("{\"request_id\":\"edit-request-123\"}");
        capture.Bodies.Enqueue("{\"id\":\"edit-request-123\",\"status\":\"Ready\",\"result\":{\"sample\":\"https://edit-result.example.com/image.png\"}}");
        return capture;
    }

    private static UpstreamCapture Stream(string chunk)
    {
        return new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(chunk) };
    }

    private static JsonObject Transform(string json)
    {
        var provider = FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        var body = JsonNode.Parse(json)!.AsObject();
        return provider.Options.TransformRequestBody!(body, new List<CallWarning>());
    }

    private static FireworksImageModel Image(UpstreamCapture capture, string modelId)
    {
        return (FireworksImageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(modelId);
    }

    private static OpenAICompatibleLanguageModel Chat(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
    }

    private static UpstreamCapture Error(string body)
    {
        return new UpstreamCapture { Status = HttpStatusCode.BadRequest, ResponseBody = body };
    }

    private sealed class PendingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
