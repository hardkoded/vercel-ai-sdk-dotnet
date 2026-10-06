// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GmiCloud;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GmiCloudUpstreamTests
{
    private const string MaxTokensBody = "{\"error\":{\"message\":\"Backend request failed with status 400\",\"type\":\"backend_error\",\"code\":400,\"details\":\"{\\\"error\\\":{\\\"type\\\":\\\"invalid_request_error\\\",\\\"code\\\":\\\"400001\\\",\\\"message\\\":\\\"The request is invalid: Invalid max_tokens value, the valid range of max_tokens is [1, 393216]. Please check the request body, required fields, and request format.\\\",\\\"message_zh\\\":\\\"请求不合法：Invalid max_tokens value, the valid range of max_tokens is [1, 393216]，请检查请求体、必填字段及请求格式是否正确。\\\",\\\"source\\\":\\\"client\\\",\\\"request_id\\\":\\\"6d6429ae-0ee8-49c1-9308-cbd2e2889b45\\\"}}\"}}";

    private const string ThinkingBody = "{\"error\":{\"message\":\"Backend request failed with status 400\",\"type\":\"backend_error\",\"code\":400,\"details\":\"{\\\"error\\\":{\\\"type\\\":\\\"invalid_request_error\\\",\\\"code\\\":\\\"400001\\\",\\\"message\\\":\\\"The request is invalid: Thinking mode does not support this tool_choice. Please check the request body, required fields, and request format.\\\",\\\"message_zh\\\":\\\"请求不合法：Thinking mode does not support this tool_choice，请检查请求体、必填字段及请求格式是否正确。\\\",\\\"source\\\":\\\"client\\\",\\\"request_id\\\":\\\"42068987-101f-4872-b45c-ca3ea683886a\\\"}}\"}}";

    private const string ImageBody = "{\"error\":{\"message\":\"Backend request failed with status 400\",\"type\":\"backend_error\",\"code\":400,\"details\":\"{\\\"error\\\":{\\\"type\\\":\\\"invalid_request_error\\\",\\\"code\\\":\\\"400001\\\",\\\"message\\\":\\\"The request is invalid: Failed to deserialize the JSON body into the target type: messages[0]: unknown variant `image_url`, expected `text` at line 1 column 265. Please check the request body, required fields, and request format.\\\",\\\"message_zh\\\":\\\"请求不合法：Failed to deserialize the JSON body into the target type: messages[0]: unknown variant `image_url`, expected `text` at line 1 column 265，请检查请求体、必填字段及请求格式是否正确。\\\",\\\"source\\\":\\\"client\\\",\\\"request_id\\\":\\\"266609a9-b5f3-4e94-9f11-f61b5904c1e3\\\"}}\"}}";

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-chat-language-model.test.ts::GmicloudChatLanguageModel error handling::surfaces the engine diagnostic from error.details on a 400", Coverage = UpstreamCoverage.Covered)]
    public async Task A_400_surfaces_the_inner_engine_diagnostic()
    {
        var capture = new UpstreamCapture { Status = System.Net.HttpStatusCode.BadRequest, ResponseBody = MaxTokensBody };
        var model = (OpenAICompatibleLanguageModel)GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture).LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731");
        var error = await Assert.ThrowsAsync<BadRequestException>(() => model.DoGenerateAsync(UpstreamChat.Prompt("hi"), CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("The request is invalid: Invalid max_tokens value, the valid range of max_tokens is [1, 393216]. Please check the request body, required fields, and request format.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (max_tokens)", Coverage = UpstreamCoverage.Covered)]
    public void Max_tokens_details_are_unwrapped()
    {
        Assert.Equal(
            "The request is invalid: Invalid max_tokens value, the valid range of max_tokens is [1, 393216]. Please check the request body, required fields, and request format.",
            OpenAICompatibleTransforms.GmiCloudError(MaxTokensBody));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (thinking tool_choice)", Coverage = UpstreamCoverage.Covered)]
    public void Thinking_tool_choice_details_are_unwrapped()
    {
        Assert.Equal(
            "The request is invalid: Thinking mode does not support this tool_choice. Please check the request body, required fields, and request format.",
            OpenAICompatibleTransforms.GmiCloudError(ThinkingBody));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (image input)", Coverage = UpstreamCoverage.Covered)]
    public void Image_input_details_are_unwrapped()
    {
        Assert.Equal(
            "The request is invalid: Failed to deserialize the JSON body into the target type: messages[0]: unknown variant `image_url`, expected `text` at line 1 column 265. Please check the request body, required fields, and request format.",
            OpenAICompatibleTransforms.GmiCloudError(ImageBody));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details is absent", Coverage = UpstreamCoverage.Covered)]
    public void A_missing_details_field_uses_the_outer_message()
    {
        Assert.Equal(
            "Backend request failed with status 400",
            OpenAICompatibleTransforms.GmiCloudError("{\"error\":{\"message\":\"Backend request failed with status 400\"}}"));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details is not JSON", Coverage = UpstreamCoverage.Covered)]
    public void Non_json_details_use_the_outer_message()
    {
        Assert.Equal("banner", OpenAICompatibleTransforms.GmiCloudError("{\"error\":{\"message\":\"banner\",\"details\":\"<html>nginx</html>\"}}"));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details has no inner message", Coverage = UpstreamCoverage.Covered)]
    public void Details_without_an_inner_message_use_the_outer_message()
    {
        Assert.Equal("banner", OpenAICompatibleTransforms.GmiCloudError("{\"error\":{\"message\":\"banner\",\"details\":\"{\\\"error\\\":{\\\"code\\\":\\\"500\\\"}}\"}}"));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::rejects GMI’s plain-text 404 body, deferring to status text", Coverage = UpstreamCoverage.Covered)]
    public void Plain_text_is_not_a_gmi_error_document()
    {
        Assert.Null(OpenAICompatibleTransforms.GmiCloudError("No matching target server found for model foo"));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::defaults to the GMI Cloud endpoint and GMI_CLOUD_APIKEY", Coverage = UpstreamCoverage.Covered)]
    public async Task The_default_endpoint_reads_GMI_CLOUD_APIKEY()
    {
        var previous = Environment.GetEnvironmentVariable("GMI_CLOUD_APIKEY");
        Environment.SetEnvironmentVariable("GMI_CLOUD_APIKEY", "mock-api-key");
        try
        {
            var capture = new UpstreamCapture();
            var provider = GmiCloudProvider.Create(handler: capture);
            var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731");
            await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
            Assert.Equal("gmicloud.chat", model.Provider);
            Assert.Equal("GMI_CLOUD_APIKEY", provider.Options.ApiKeyEnvironmentVariable);
            Assert.Equal("https://api.gmi-serving.com/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
            Assert.Equal("Bearer mock-api-key", capture.Requests[0].Headers["Authorization"]);
            Assert.Contains("ai-sdk/gmicloud/" + AiSdkVersion.Version, capture.Requests[0].Headers["User-Agent"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GMI_CLOUD_APIKEY", previous);
        }
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::attaches the gmicloud error structure and includeUsage", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_usage_is_on_and_errors_use_the_gmi_structure()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
        var provider = GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture);
        Assert.True(provider.Options.IncludeUsage);
        Assert.Equal(
            "The request is invalid: Invalid max_tokens value, the valid range of max_tokens is [1, 393216]. Please check the request body, required fields, and request format.",
            provider.Options.SelectErrorMessage!(MaxTokensBody));
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731");
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::respects a custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_base_url_is_used()
    {
        var capture = new UpstreamCapture();
        var model = (OpenAICompatibleLanguageModel)GmiCloudProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/gmi" },
            capture).LanguageModel("model");
        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://example.com/gmi/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
    }

    [Fact]
    [UpstreamTest("packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::throws NoSuchModelError for embedding and image models", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_and_images_are_rejected()
    {
        var provider = GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" });
        var embedding = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("model"));
        var image = Assert.Throws<AiSdkException>(() => provider.ImageModel("model"));
        Assert.Equal("Provider 'gmicloud' does not support embedding model 'model'.", embedding.Message);
        Assert.Equal("Provider 'gmicloud' does not support image model 'model'.", image.Message);
    }
}
