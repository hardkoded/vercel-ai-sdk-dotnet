// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.DeepInfra;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class DeepInfraUpstreamTests
{
    private const string Prompt = "A cute baby sea otter";

    private const string ImageModelId = "stability-ai/sdxl";

    private const string EditModelId = "black-forest-labs/FLUX.1-Kontext-dev";

    private const string ImageResponse = "{\"images\":[\"data:image/png;base64,dGVzdA==\"]}";

    private const string EditResponse = "{\"created\":1234567890,\"data\":[{\"b64_json\":\"ZWRpdGVk\"}]}";

    private static readonly byte[] EditedImage = Encoding.UTF8.GetBytes("edited");

    private static readonly DateTimeOffset Stamp = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should pass the correct parameters including aspect ratio and seed", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_send_aspect_ratio_seed_and_count()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        var model = Image(capture, "stabilityai/sdxl");
        model.Seed = 3;
        await model.DoGenerateAsync(new ImageCallOptions("a cat") { AspectRatio = "16:9", Count = 2 }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("a cat", body["prompt"]!.GetValue<string>());
        Assert.Equal("16:9", body["aspect_ratio"]!.GetValue<string>());
        Assert.Equal(2, body["num_images"]!.GetValue<int>());
        Assert.Equal(3, body["seed"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_use_the_inference_route()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        await Image(capture, "stabilityai/sdxl").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None);
        Assert.Equal("https://api.deepinfra.com/v1/inference/stabilityai/sdxl", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should handle size parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Size_is_sent_as_width_and_height()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        await Image(capture, "stabilityai/sdxl").DoGenerateAsync(new ImageCallOptions("a cat") { Size = "1024x768" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("1024", body["width"]!.GetValue<string>());
        Assert.Equal("768", body["height"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > chatModel::should set supportsStructuredOutputs so response_format json_schema is forwarded", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_structured_outputs_send_json_schema()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        options.ProviderOptions = UpstreamChat.Bag("deepinfra", "{\"responseFormat\":\"json\"}");
        var model = (OpenAICompatibleLanguageModel)DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal("api.deepinfra.com", capture.Requests[0].Uri!.Host);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > image::should respect custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_base_url_appends_inference()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        var provider = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", BaseUrl = "https://custom.api.deepinfra.com" }, capture);
        await ((DeepInfraImageModel)provider.ImageModel("deepinfra-image-model")).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal("https://custom.api.deepinfra.com/inference/deepinfra-image-model", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-chat-language-model.test.ts::DeepInfraChatLanguageModel > usage calculation::should fix incorrect completion_tokens for gemini/gemma models when reasoning_tokens > completion_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_tokens_are_added_when_they_exceed_completion_tokens()
    {
        var capture = new UpstreamCapture { ResponseBody = ChatResponse("google/gemma-2-9b-it", "{\"prompt_tokens\":19,\"completion_tokens\":84,\"total_tokens\":1184,\"prompt_tokens_details\":null,\"completion_tokens_details\":{\"reasoning_tokens\":1081}}") };
        var result = await DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("google/gemma-2-9b-it").DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(1165, result.Usage.OutputTokens);
        Assert.Equal(84, result.Usage.TextTokens);
        Assert.Equal(1081, result.Usage.ReasoningTokens);
        Assert.Equal(2265, result.Usage.TotalTokens);
    }

    [Fact]
    public async Task Streamed_reasoning_tokens_are_added_when_they_exceed_completion_tokens()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                "{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}",
                "{\"choices\":[],\"usage\":{\"prompt_tokens\":19,\"completion_tokens\":84,\"total_tokens\":1184,\"completion_tokens_details\":{\"reasoning_tokens\":1081}}}"),
        };
        var model = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("google/gemma-2-9b-it");
        var parts = await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(1165, finish.Usage.OutputTokens);
        Assert.Equal(84, finish.Usage.TextTokens);
        Assert.Equal(1081, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-chat-language-model.test.ts::DeepInfraChatLanguageModel > usage calculation::should not modify usage for non-gemini models with correct data", Coverage = UpstreamCoverage.Covered)]
    public async Task Correct_usage_is_left_alone()
    {
        var capture = new UpstreamCapture { ResponseBody = ChatResponse("mistralai/Mixtral-8x7B-Instruct-v0.1", "{\"prompt_tokens\":18,\"completion_tokens\":475,\"total_tokens\":493,\"prompt_tokens_details\":null}") };
        var result = await DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("mistralai/Mixtral-8x7B-Instruct-v0.1").DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(475, result.Usage.OutputTokens);
        Assert.Equal(475, result.Usage.TextTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_send_provider_and_request_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var options = new OpenAICompatibleOptions { ApiKey = "secret" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (DeepInfraImageModel)DeepInfraProvider.Create(options, capture).ImageModel(ImageModelId);
        model.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        var headers = capture.Requests[0].Headers;
        Assert.StartsWith("application/json", headers["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", headers["Custom-Request-Header"]);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_errors_use_the_provider_message()
    {
        var capture = new UpstreamCapture { Status = HttpStatusCode.BadRequest, ResponseBody = "{\"error\":{\"message\":\"Bad Request\"}}" };
        var error = await Assert.ThrowsAnyAsync<ApiException>(() => Image(capture, ImageModelId).DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None));
        Assert.Contains("Bad Request", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should respect the abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_honors_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var provider = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new PendingHandler());
        var generating = provider.ImageModel(ImageModelId).DoGenerateAsync(new ImageCallOptions(Prompt), cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAsync<ApiUserAbortException>(() => generating);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate > response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_records_timestamp_headers_and_model_id()
    {
        var model = new DeepInfraImageModel(DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new UpstreamCapture { ResponseBody = ImageResponse }), ImageModelId, () => Stamp);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal(Stamp, model.LastResponseTimestamp);
        Assert.Equal(ImageModelId, model.ModelId);
        Assert.NotEmpty(model.LastResponseHeaders);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate > response metadata::should include response headers from API call", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_returns_the_response_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        capture.ResponseHeaders["x-request-id"] = "test-request-id";
        var model = Image(capture, ImageModelId);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("test-request-id", model.LastResponseHeaders["x-request-id"]);
        Assert.StartsWith("application/json", model.LastResponseHeaders["content-type"], StringComparison.Ordinal);
        Assert.Equal(Encoding.UTF8.GetByteCount(ImageResponse).ToString(CultureInfo.InvariantCulture), model.LastResponseHeaders["content-length"]);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_exposes_provider_model_and_limit()
    {
        var model = Image(new UpstreamCapture(), ImageModelId);
        Assert.Equal("deepinfra.image", model.Provider);
        Assert.Equal(ImageModelId, model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > Image Editing::should send edit request with files", Coverage = UpstreamCoverage.Covered)]
    public async Task Edits_post_files_to_the_openai_edits_route()
    {
        var capture = new UpstreamCapture { ResponseBody = EditResponse };
        var model = EditModel(capture);
        model.Files = new[] { Png() };
        var result = await model.DoGenerateAsync(new ImageCallOptions("Turn the cat into a dog") { Size = "1024x1024" }, CancellationToken.None);
        Assert.Equal(EditedImage, Assert.Single(result.Images).Data);
        Assert.Equal("https://edit.example.com/openai/images/edits", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Contains("name=model", capture.Requests[0].Body, StringComparison.Ordinal);
        Assert.Contains("black-forest-labs/FLUX.1-Kontext-dev", capture.Requests[0].Body, StringComparison.Ordinal);
        Assert.Contains("1024x1024", capture.Requests[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > Image Editing::should send edit request with files and mask", Coverage = UpstreamCoverage.Covered)]
    public async Task Edits_send_the_mask()
    {
        var capture = new UpstreamCapture { ResponseBody = EditResponse };
        var model = EditModel(capture);
        model.Files = new[] { Png() };
        model.Mask = new OpenAICompatibleImageFile("image/png", new byte[] { 255, 255, 255, 0 }, null, null);
        var result = await model.DoGenerateAsync(new ImageCallOptions("Add a flamingo to the pool"), CancellationToken.None);
        Assert.Equal(EditedImage, Assert.Single(result.Images).Data);
        Assert.Equal("https://edit.example.com/openai/images/edits", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Contains("name=mask", capture.Requests[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > Image Editing::should send edit request with multiple images", Coverage = UpstreamCoverage.Covered)]
    public async Task Edits_send_every_image_under_the_same_field()
    {
        var capture = new UpstreamCapture { ResponseBody = EditResponse };
        var model = EditModel(capture);
        model.Files = new[] { Png(), Png() };
        var result = await model.DoGenerateAsync(new ImageCallOptions("Combine these images"), CancellationToken.None);
        Assert.Equal(EditedImage, Assert.Single(result.Images).Data);
        Assert.Equal(2, Regex.Matches(capture.Requests[0].Body, "name=image;").Count);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > Image Editing::should include response metadata for edit requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Edits_record_timestamp_headers_and_model_id()
    {
        var provider = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", ImageBaseUrl = "https://edit.example.com/inference" }, new UpstreamCapture { ResponseBody = EditResponse });
        var model = new DeepInfraImageModel(provider, EditModelId, () => Stamp) { Files = new[] { Png() } };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this image"), CancellationToken.None);
        Assert.Equal(Stamp, model.LastResponseTimestamp);
        Assert.Equal(EditModelId, model.ModelId);
        Assert.NotEmpty(model.LastResponseHeaders);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > Image Editing::should pass provider options in edit request", Coverage = UpstreamCoverage.Covered)]
    public async Task Edits_send_provider_options_as_form_fields()
    {
        var capture = new UpstreamCapture { ResponseBody = EditResponse };
        var model = EditModel(capture);
        model.Files = new[] { Png() };
        model.ProviderOptions = UpstreamChat.Bag("deepinfra", "{\"guidance\":7.5}");
        await model.DoGenerateAsync(new ImageCallOptions("Edit with custom options"), CancellationToken.None);
        Assert.Equal("https://edit.example.com/openai/images/edits", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Matches("name=guidance\\r\\n\\r\\n7.5\\r\\n", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > createDeepInfra::should create a DeepInfraProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public void Default_options_read_the_deepinfra_api_key()
    {
        var previous = Environment.GetEnvironmentVariable("DEEPINFRA_API_KEY");
        Environment.SetEnvironmentVariable("DEEPINFRA_API_KEY", "env-key");
        try
        {
            var provider = DeepInfraProvider.Create();
            Assert.Equal("DEEPINFRA_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
            Assert.Equal("Bearer env-key", provider.CreateHeaders()["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DEEPINFRA_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > createDeepInfra::should create a DeepInfraProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public void Custom_options_set_the_key_base_url_and_headers()
    {
        var options = new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url" };
        options.Headers["Custom-Header"] = "value";
        var provider = DeepInfraProvider.Create(options);
        var headers = provider.CreateHeaders();
        Assert.Equal("Bearer custom-key", headers["Authorization"]);
        Assert.Equal("value", headers["Custom-Header"]);
        Assert.Equal("https://custom.url", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > createDeepInfra::should return a chat model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void The_default_language_model_is_a_chat_model()
    {
        var model = Assert.IsType<OpenAICompatibleLanguageModel>(DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("foo-model-id"));
        Assert.Equal("deepinfra.chat", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > chatModel::should construct a chat model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Chat_models_use_the_deepinfra_chat_provider_id()
    {
        var model = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateChatModel("deepinfra-chat-model");
        Assert.Equal("deepinfra.chat", model.Provider);
        Assert.Equal("deepinfra-chat-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > completionModel::should construct a completion model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Completion_models_use_the_openai_compatible_completion_model()
    {
        var model = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CompletionModel("deepinfra-completion-model");
        Assert.IsType<OpenAICompatibleCompletionLanguageModel>(model);
        Assert.Equal("deepinfra-completion-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > embeddingModel::should construct a text embedding model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_models_use_the_openai_compatible_embedding_model()
    {
        var model = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("deepinfra-embedding-model");
        Assert.IsType<OpenAICompatibleEmbeddingModel>(model);
        Assert.Equal("deepinfra-embedding-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > image::should construct an image model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_models_use_the_deepinfra_image_model_and_inference_url()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Assert.IsType<DeepInfraImageModel>(DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel("deepinfra-image-model"));
        Assert.Equal("deepinfra.image", model.Provider);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("https://api.deepinfra.com/v1/inference/deepinfra-image-model", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > image::should use default settings when none provided", Coverage = UpstreamCoverage.Covered)]
    public void Image_models_need_no_settings()
    {
        var model = Assert.IsType<DeepInfraImageModel>(DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel("deepinfra-image-model"));
        Assert.Equal("deepinfra-image-model", model.ModelId);
        Assert.Null(model.Seed);
        Assert.Null(model.Files);
    }

    private static DeepInfraImageModel Image(UpstreamCapture capture, string modelId)
    {
        return (DeepInfraImageModel)DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(modelId);
    }

    private static DeepInfraImageModel EditModel(UpstreamCapture capture)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "secret", ImageBaseUrl = "https://edit.example.com/inference" };
        return (DeepInfraImageModel)DeepInfraProvider.Create(options, capture).ImageModel(EditModelId);
    }

    private static OpenAICompatibleImageFile Png()
    {
        return new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71 }, null, null);
    }

    private static string ChatResponse(string model, string usage)
    {
        return "{\"id\":\"test-id\",\"object\":\"chat.completion\",\"created\":1234567890,\"model\":\"" + model + "\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Test response\"},\"finish_reason\":\"stop\"}],\"usage\":" + usage + "}";
    }

    private sealed class PendingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("The request was not cancelled.");
        }
    }
}
