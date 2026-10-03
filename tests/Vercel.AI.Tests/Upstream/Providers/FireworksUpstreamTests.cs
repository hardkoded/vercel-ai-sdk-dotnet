// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Fireworks;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class FireworksUpstreamTests
{
    private const string Flux = "accounts/fireworks/models/flux-1-dev-fp8";
    private const string Kontext = "accounts/fireworks/models/flux-kontext-pro";
    private const string Sized = "accounts/fireworks/models/playground-v2-5-1024px-aesthetic";

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
}
