// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Baseten;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class BasetenUpstreamTests
{
    private const string Embedding = "packages/baseten/src/baseten-embedding-model.test.ts::doEmbed";
    private const string Provider = "packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > ";
    private const string SyncUrl = "https://model-123.api.baseten.co/environments/production/sync";
    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-embedding-model.test.ts::doEmbed > modelURL forms::should append /v1 to a bare /sync URL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_sync_url_gains_a_v1_embeddings_path()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Embed(capture, "https://model.example/sync", "embed");
        await model.DoEmbedAsync(new[] { "hi" }, null, CancellationToken.None);
        Assert.Equal("https://model.example/sync/v1/embeddings", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-embedding-model.test.ts::doEmbed > modelURL forms::should not double up /v1 for a /sync/v1 URL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_sync_v1_url_is_not_doubled()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Embed(capture, "https://model.example/sync/v1", "embed");
        await model.DoEmbedAsync(new[] { "hi" }, null, CancellationToken.None);
        Assert.Equal("https://model.example/sync/v1/embeddings", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-embedding-model.test.ts::doEmbed > batch limit::should reject more than 128 values", Coverage = UpstreamCoverage.Covered)]
    public async Task More_than_128_embeddings_are_rejected()
    {
        var values = new string[129];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = "v";
        }

        var model = Embed(new UpstreamCapture(), "https://model.example/sync/v1", "embed");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoEmbedAsync(values, null, CancellationToken.None));
        Assert.Contains("128", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > chatModel::should throw error for /predict endpoints with chat models", Coverage = UpstreamCoverage.Covered)]
    public void Predict_urls_are_rejected_for_chat()
    {
        var provider = BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = "https://model.example/predict" });
        var error = Assert.Throws<AiSdkException>(() => provider.LanguageModel("chat"));
        Assert.Equal("Not supported. You must use a /sync/v1 endpoint for chat models.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > chatModel::should handle /sync/v1 endpoints correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Sync_v1_chat_uses_the_deployment_chat_completions_url()
    {
        var capture = new UpstreamCapture();
        var provider = BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = "https://model.example/sync/v1" }, capture);
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("llama");
        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://model.example/sync/v1/chat/completions", capture.Requests[0].Uri!.AbsoluteUri);
        Assert.Equal("llama", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > chatModel > includeUsage::should be set for the default Model APIs path", Coverage = UpstreamCoverage.Covered)]
    public async Task The_default_chat_path_requests_usage()
    {
        var capture = Stream();
        var model = (OpenAICompatibleLanguageModel)BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
        Assert.Equal("https://inference.baseten.co/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > chatModel > supportsStructuredOutputs::should be set for the default Model APIs path", Coverage = UpstreamCoverage.Covered)]
    public void The_default_chat_model_supports_structured_outputs()
    {
        var model = (OpenAICompatibleLanguageModel)BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("m");
        Assert.True(model.SupportsStructuredOutputs);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > imageModel::should throw NoSuchModelError for unsupported image models", Coverage = UpstreamCoverage.Covered)]
    public void Images_are_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel("image"));
        Assert.Equal("Provider 'baseten' does not support image model 'image'.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-provider.unit.test.ts::BasetenProvider > textEmbeddingModel::should throw error when no modelURL is provided", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_require_a_sync_url()
    {
        var error = Assert.Throws<AiSdkException>(() => BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("embed"));
        Assert.Equal("No model URL provided for embeddings. Please set modelURL option for embeddings.", error.Message);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should extract embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_are_read_from_the_response()
    {
        var model = Embed(EmbeddingCapture(), SyncUrl, null);
        var result = await model.DoEmbedAsync(TestValues, CancellationToken.None);
        Assert.Equal(new[] { 0.1f, 0.2f, 0.3f }, result.Embeddings[0]);
        Assert.Equal(new[] { 0.4f, 0.5f, 0.6f }, result.Embeddings[1]);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should extract usage from prompt_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Embedding_usage_comes_from_prompt_tokens()
    {
        var result = await Embed(EmbeddingCapture(), SyncUrl, null).DoEmbedAsync(TestValues, CancellationToken.None);
        Assert.Equal(8, result.Tokens);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should send the values and a model field", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_send_the_values_and_the_default_model()
    {
        var capture = EmbeddingCapture();
        await Embed(capture, SyncUrl, null).DoEmbedAsync(TestValues, CancellationToken.None);
        JsonAssert.Equal(UpstreamChat.Body(capture), "{\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"model\":\"embeddings\",\"encoding_format\":\"float\"}");
    }

    [Fact]
    [UpstreamTest(Embedding + "::should pass the api key as a bearer token", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_send_a_bearer_token()
    {
        var capture = EmbeddingCapture();
        await Embed(capture, SyncUrl, null).DoEmbedAsync(TestValues, CancellationToken.None);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should expose real response headers rather than an empty object", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_expose_the_response_headers()
    {
        var capture = EmbeddingCapture();
        capture.ResponseHeaders["x-request-id"] = "abc123";
        var model = Embed(capture, SyncUrl, null);
        await model.DoEmbedAsync(TestValues, CancellationToken.None);
        Assert.Equal("abc123", model.LastResponseHeaders["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Embedding + " > batch limit::should accept 128 values", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_accept_128_values()
    {
        var values = new string[128];
        var data = new string[128];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = "value " + i;
            data[i] = "{\"object\":\"embedding\",\"index\":" + i + ",\"embedding\":[0.1]}";
        }

        var capture = new UpstreamCapture { ResponseBody = "{\"object\":\"list\",\"data\":[" + string.Join(",", data) + "]}" };
        var result = await Embed(capture, SyncUrl, null).DoEmbedAsync(values, CancellationToken.None);
        Assert.Equal(128, result.Embeddings.Count);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should surface Baseten string-shaped error messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_surface_a_string_error()
    {
        var capture = new UpstreamCapture { Status = HttpStatusCode.Forbidden, ResponseBody = "{\"error\":\"please check the api-key you provided\"}" };
        var error = await Assert.ThrowsAnyAsync<ApiException>(() => Embed(capture, SyncUrl, null).DoEmbedAsync(TestValues, CancellationToken.None));
        Assert.Contains("please check the api-key you provided", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should surface object-shaped error messages from a dedicated deployment", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_surface_an_object_error()
    {
        var capture = new UpstreamCapture
        {
            Status = HttpStatusCode.NotFound,
            ResponseBody = "{\"error\":{\"code\":404,\"message\":\"The model `not-a-real-model` does not exist.\",\"param\":\"model\",\"type\":\"NotFoundError\"}}",
        };
        var error = await Assert.ThrowsAnyAsync<ApiException>(() => Embed(capture, SyncUrl, null).DoEmbedAsync(TestValues, CancellationToken.None));
        Assert.Contains("The model `not-a-real-model` does not exist.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "createBaseten::should create a BasetenProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public void The_default_provider_reads_the_api_key_from_the_environment()
    {
        var previous = Environment.GetEnvironmentVariable("BASETEN_API_KEY");
        Environment.SetEnvironmentVariable("BASETEN_API_KEY", "environment-key");
        try
        {
            var provider = BasetenProvider.Create();
            var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("deepseek-ai/DeepSeek-V3-0324");
            Assert.Equal("Bearer environment-key", provider.CreateHeaders()["Authorization"]);
            Assert.Equal("baseten.chat", model.Provider);
        }
        finally
        {
            Environment.SetEnvironmentVariable("BASETEN_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest(Provider + "createBaseten::should create a BasetenProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public void Custom_options_set_the_key_and_headers()
    {
        var options = new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url" };
        options.Headers["Custom-Header"] = "value";
        var headers = BasetenProvider.Create(options).CreateHeaders();
        Assert.Equal("Bearer custom-key", headers["Authorization"]);
        Assert.Equal("value", headers["custom-header"]);
    }

    [Fact]
    [UpstreamTest(Provider + "createBaseten::should support optional modelId parameter", Coverage = UpstreamCoverage.Covered)]
    public void The_provider_creates_a_chat_model_with_or_without_an_id()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.LanguageModel(string.Empty));
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.LanguageModel("deepseek-ai/DeepSeek-V3-0324"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel::should construct a chat model with correct configuration for default Model APIs", Coverage = UpstreamCoverage.Covered)]
    public void The_default_chat_model_uses_the_baseten_error_reader()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("deepseek-ai/DeepSeek-V3-0324");
        Assert.Equal("deepseek-ai/DeepSeek-V3-0324", model.ModelId);
        Assert.Equal("baseten.chat", model.Provider);
        Assert.Equal("bad model", provider.Options.SelectErrorMessage!("{\"error\":\"bad model\"}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > includeUsage::should be set for a dedicated /sync/v1 deployment", Coverage = UpstreamCoverage.Covered)]
    public async Task A_deployment_chat_stream_requests_usage()
    {
        var capture = Stream();
        var model = (OpenAICompatibleLanguageModel)BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = SyncUrl + "/v1" }, capture).LanguageModel(string.Empty);
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
        Assert.Equal("placeholder", model.ModelId);
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > supportsStructuredOutputs::should be set for a dedicated /sync/v1 deployment", Coverage = UpstreamCoverage.Covered)]
    public void A_deployment_chat_model_supports_structured_outputs()
    {
        var model = (OpenAICompatibleLanguageModel)BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = SyncUrl + "/v1" }).LanguageModel(string.Empty);
        Assert.True(model.SupportsStructuredOutputs);
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > errorStructure::should parse the string envelope the Model APIs return", Coverage = UpstreamCoverage.Covered)]
    public void The_error_reader_accepts_a_string_envelope()
    {
        Assert.Equal("please check the model you provided", OpenAICompatibleTransforms.BasetenError("{\"error\":\"please check the model you provided\"}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > errorStructure::should parse the object envelope a dedicated deployment returns", Coverage = UpstreamCoverage.Covered)]
    public void The_error_reader_accepts_an_object_envelope()
    {
        Assert.Equal(
            "The model `not-a-real-model` does not exist.",
            OpenAICompatibleTransforms.BasetenError("{\"error\":{\"code\":404,\"message\":\"The model `not-a-real-model` does not exist.\",\"param\":\"model\",\"type\":\"NotFoundError\"}}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > errorStructure::should parse an error object with a null param and string code", Coverage = UpstreamCoverage.Covered)]
    public void The_error_reader_accepts_a_null_param_and_string_code()
    {
        Assert.Equal(
            "Invalid value for `temperature`.",
            OpenAICompatibleTransforms.BasetenError("{\"error\":{\"message\":\"Invalid value for `temperature`.\",\"param\":null,\"code\":\"invalid_request_error\",\"type\":\"BadRequestError\"}}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > errorStructure::should ignore unknown keys alongside the error", Coverage = UpstreamCoverage.Covered)]
    public void The_error_reader_ignores_unknown_keys()
    {
        Assert.Equal("Model not found", OpenAICompatibleTransforms.BasetenError("{\"error\":{\"message\":\"Model not found\"},\"request_id\":\"chatcmpl-abc123\"}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel > errorStructure::should reject an error object without a message", Coverage = UpstreamCoverage.Covered)]
    public void The_error_reader_rejects_an_object_without_a_message()
    {
        Assert.Null(OpenAICompatibleTransforms.BasetenError("{\"error\":{\"code\":404}}"));
    }

    [Fact]
    [UpstreamTest(Provider + "chatModel::should construct a chat model with optional modelId", Coverage = UpstreamCoverage.Covered)]
    public void The_chat_model_id_defaults_to_chat()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.Equal("chat", provider.CreateChatModel(string.Empty).ModelId);
        Assert.Equal("deepseek-ai/DeepSeek-V3-0324", provider.CreateChatModel("deepseek-ai/DeepSeek-V3-0324").ModelId);
    }

    [Fact]
    [UpstreamTest(Provider + "languageModel::should be an alias for chatModel", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_the_chat_model()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.CreateChatModel("deepseek-ai/DeepSeek-V3-0324"));
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.LanguageModel("deepseek-ai/DeepSeek-V3-0324"));
    }

    [Fact]
    [UpstreamTest(Provider + "languageModel::should support optional modelId parameter", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_id_is_optional()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.Equal("chat", provider.LanguageModel(string.Empty).ModelId);
        Assert.Equal("deepseek-ai/DeepSeek-V3-0324", provider.LanguageModel("deepseek-ai/DeepSeek-V3-0324").ModelId);
    }

    [Fact]
    [UpstreamTest(Provider + "textEmbeddingModel::should construct embedding model for /sync endpoints", Coverage = UpstreamCoverage.Covered)]
    public async Task A_sync_url_builds_the_embedding_model()
    {
        var capture = EmbeddingCapture();
        var model = Embed(capture, SyncUrl, null);
        await model.DoEmbedAsync(TestValues, CancellationToken.None);
        Assert.Equal("embeddings", model.ModelId);
        Assert.Equal("baseten.embedding", model.Provider);
        Assert.Equal(SyncUrl + "/v1/embeddings", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Provider + "textEmbeddingModel::should throw error for /predict endpoints (not supported with Performance Client)", Coverage = UpstreamCoverage.Covered)]
    public void A_predict_url_is_rejected_for_embeddings()
    {
        var provider = BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = "https://model-123.api.baseten.co/environments/production/predict" });
        var error = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel(string.Empty));
        Assert.Equal("Not supported. You must use a /sync or /sync/v1 endpoint for embeddings.", error.Message);
    }

    [Fact]
    [UpstreamTest(Provider + "textEmbeddingModel::should support /sync/v1 endpoints (strips /v1 before passing to Performance Client)", Coverage = UpstreamCoverage.Covered)]
    public void A_sync_v1_url_builds_the_embedding_model()
    {
        Assert.Equal("embeddings", Embed(new UpstreamCapture(), SyncUrl + "/v1", null).ModelId);
    }

    [Fact]
    [UpstreamTest(Provider + "textEmbeddingModel::should support custom modelId for embeddings", Coverage = UpstreamCoverage.Covered)]
    public void The_embedding_model_id_defaults_to_embeddings()
    {
        Assert.Equal("embeddings", Embed(new UpstreamCapture(), SyncUrl, null).ModelId);
    }

    [Fact]
    [UpstreamTest(Provider + "textEmbeddingModel > default (plain HTTP) path::should cap embeddings per call at 128 so embedMany splits larger inputs", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_are_capped_at_128_per_call()
    {
        Assert.Equal(128, Embed(new UpstreamCapture(), SyncUrl, null).MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest(Provider + "URL construction::should use default baseURL when no modelURL is provided", Coverage = UpstreamCoverage.Covered)]
    public void Chat_uses_the_default_base_url()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.Equal("https://inference.baseten.co/v1/chat/completions", provider.ChatUri("test-model").AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Provider + "URL construction::should use custom baseURL when provided", Coverage = UpstreamCoverage.Covered)]
    public void Chat_uses_a_custom_base_url()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", BaseUrl = "https://custom.baseten.co/v1" });
        Assert.Equal("https://custom.baseten.co/v1/chat/completions", provider.ChatUri("test-model").AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Provider + "URL construction::should use modelURL for custom endpoints", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_uses_the_model_url()
    {
        var capture = new UpstreamCapture();
        var model = (OpenAICompatibleLanguageModel)BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = SyncUrl + "/v1" }, capture).LanguageModel(string.Empty);
        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(SyncUrl + "/v1/chat/completions", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Provider + "Headers::should include Authorization header with API key", Coverage = UpstreamCoverage.Covered)]
    public void Headers_include_the_bearer_token()
    {
        Assert.Equal("Bearer secret", BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateHeaders()["Authorization"]);
    }

    [Fact]
    [UpstreamTest(Provider + "Headers::should include custom headers when provided", Coverage = UpstreamCoverage.Covered)]
    public void Headers_include_custom_headers()
    {
        var options = new OpenAICompatibleOptions { ApiKey = "secret" };
        options.Headers["Custom-Header"] = "custom-value";
        var headers = BasetenProvider.Create(options).CreateHeaders();
        Assert.Equal("Bearer secret", headers["Authorization"]);
        Assert.Equal("custom-value", headers["custom-header"]);
    }

    [Fact]
    [UpstreamTest(Provider + "Headers::should include user-agent with version", Coverage = UpstreamCoverage.Covered)]
    public void Headers_include_the_sdk_user_agent()
    {
        Assert.Contains("ai-sdk/baseten/" + AiSdkVersion.Version, BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateHeaders()["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "Error handling::should handle missing modelURL for embeddings gracefully", Coverage = UpstreamCoverage.Covered)]
    public void A_missing_model_url_explains_the_embedding_requirement()
    {
        var error = Assert.Throws<AiSdkException>(() => BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel(string.Empty));
        Assert.Equal("No model URL provided for embeddings. Please set modelURL option for embeddings.", error.Message);
    }

    [Fact]
    [UpstreamTest(Provider + "Error handling::should handle unsupported image models", Coverage = UpstreamCoverage.Covered)]
    public void Unsupported_image_models_are_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel("unsupported-model"));
        Assert.Equal("Provider 'baseten' does not support image model 'unsupported-model'.", error.Message);
    }

    [Fact]
    [UpstreamTest(Provider + "Provider interface::should allow calling provider as function", Coverage = UpstreamCoverage.Covered)]
    public void The_language_model_factory_takes_an_optional_id()
    {
        var provider = BasetenProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.LanguageModel(string.Empty));
        Assert.IsType<OpenAICompatibleLanguageModel>(provider.LanguageModel("test-model"));
    }

    private static UpstreamCapture EmbeddingCapture()
    {
        return new UpstreamCapture
        {
            ResponseBody = "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"index\":0,\"embedding\":[0.1,0.2,0.3]},{\"object\":\"embedding\",\"index\":1,\"embedding\":[0.4,0.5,0.6]}],\"model\":\"not-required\",\"usage\":{\"prompt_tokens\":8,\"total_tokens\":8}}",
        };
    }

    private static OpenAICompatibleEmbeddingModel Embed(UpstreamCapture capture, string url, string? modelId)
    {
        var provider = BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = url }, capture);
        return (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel(modelId ?? string.Empty);
    }

    private static UpstreamCapture Stream()
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
    }
}
