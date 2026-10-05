// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleProviderUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > createOpenAICompatible::should create provider with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_sends_bearer_headers_and_query()
    {
        var capture = new UpstreamCapture();
        var provider = Provider(capture, "test-provider", "test-api-key", "https://api.example.com");
        provider.Options.Headers["custom-header"] = "value";
        provider.Options.QueryParameters["Custom-Param"] = "value";
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("model-id");
        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("test-provider.chat", model.Provider);
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("value", capture.Requests[0].Headers["custom-header"]);
        Assert.Contains("ai-sdk/openai-compatible/0.0.0", capture.Requests[0].Headers["User-Agent"]);
        Assert.Equal("/chat/completions", capture.Requests[0].Uri!.AbsolutePath);
        Assert.Equal("?Custom-Param=value", capture.Requests[0].Uri!.Query);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > createOpenAICompatible::should create headers without authorization when no apiKey provided", Coverage = UpstreamCoverage.Covered)]
    public void Missing_key_omits_authorization()
    {
        var provider = Provider(new UpstreamCapture(), "test-provider", null, "https://api.example.com");
        provider.Options.Headers["custom-header"] = "value";
        var headers = provider.CreateHeaders();
        Assert.False(headers.ContainsKey("Authorization"));
        Assert.Equal("value", headers["custom-header"]);
        Assert.Equal("ai-sdk/openai-compatible/0.0.0", headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > model creation methods::should create chat model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Chat_model_uses_the_chat_provider_id_and_query()
    {
        var provider = Configured("test-provider");
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("chat-model");
        Assert.Equal("chat-model", model.ModelId);
        Assert.Equal("test-provider.chat", model.Provider);
        Assert.Equal("Bearer test-api-key", provider.CreateHeaders()["Authorization"]);
        Assert.Contains("Custom-Param=value", provider.ChatUri("chat-model").Query);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > model creation methods::should create completion model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Completion_model_uses_the_completion_provider_id_and_query()
    {
        var provider = Configured("test-provider");
        var model = provider.CompletionModel("completion-model");
        Assert.Equal("completion-model", model.ModelId);
        Assert.Equal("test-provider.completion", model.Provider);
        Assert.Contains("Custom-Param=value", provider.CompletionsUri("completion-model").Query);
        Assert.EndsWith("/completions", provider.CompletionsUri("completion-model").AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > model creation methods::should create embedding model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_model_uses_the_embedding_provider_id_and_query()
    {
        var provider = Configured("test-provider");
        var model = (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel("embedding-model");
        Assert.Equal("embedding-model", model.ModelId);
        Assert.Equal("test-provider.embedding", model.Provider);
        Assert.Contains("Custom-Param=value", provider.EmbeddingsUri("embedding-model").Query);
        Assert.EndsWith("/embeddings", provider.EmbeddingsUri("embedding-model").AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > model creation methods::should use languageModel as default when called as function", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_the_chat_model()
    {
        var provider = Configured("test-provider");
        var model = (OpenAICompatibleLanguageModel)provider.LanguageModel("model-id");
        Assert.Equal("test-provider.chat", model.Provider);
        Assert.Equal("model-id", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > model creation methods::should create URL without query parameters when queryParams is not specified", Coverage = UpstreamCoverage.Covered)]
    public void Chat_url_has_no_query_when_none_are_configured()
    {
        var provider = Provider(new UpstreamCapture(), "test-provider", "test-api-key", "https://api.example.com");
        Assert.Equal(string.Empty, provider.ChatUri("model-id").Query);
        Assert.Equal("https://api.example.com/chat/completions", provider.ChatUri("model-id").AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > includeUsage setting::should pass includeUsage: true to all model types when specified in provider settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_usage_true_is_sent_by_chat_and_completion_streams()
    {
        var capture = Stream();
        var provider = Provider(capture, "test-provider", "k", "https://api.example.com");
        provider.Options.IncludeUsage = true;
        await UpstreamChat.Read(((OpenAICompatibleLanguageModel)provider.LanguageModel("chat")).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
        capture.Requests.Clear();
        await UpstreamChat.Read(provider.CompletionModel("completion").DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(JsonNode.Parse(capture.Requests[0].Body)!["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > includeUsage setting::should pass includeUsage: false to all model types when specified in provider settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_usage_false_omits_stream_options()
    {
        var capture = Stream();
        var provider = Provider(capture, "test-provider", "k", "https://api.example.com");
        provider.Options.IncludeUsage = false;
        await UpstreamChat.Read(((OpenAICompatibleLanguageModel)provider.LanguageModel("chat")).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Null(UpstreamChat.Body(capture)["stream_options"]);
        capture.Requests.Clear();
        await UpstreamChat.Read(provider.CompletionModel("completion").DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["stream_options"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > includeUsage setting::should pass includeUsage: undefined to all model types when not specified in provider settings", Coverage = UpstreamCoverage.Partial, Note = "IncludeUsage is a bool and defaults to false. Unset and false both omit stream_options.")]
    public async Task Unset_include_usage_omits_stream_options()
    {
        var capture = Stream();
        var provider = Provider(capture, "test-provider", "k", "https://api.example.com");
        Assert.False(provider.Options.IncludeUsage);
        await UpstreamChat.Read(((OpenAICompatibleLanguageModel)provider.LanguageModel("chat")).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Null(UpstreamChat.Body(capture)["stream_options"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/openai-compatible-provider.test.ts::OpenAICompatibleProvider > supportsStructuredOutputs setting::should pass supportsStructuredOutputs to .chatModel() and .languageModel() only", Coverage = UpstreamCoverage.Covered)]
    public async Task Structured_outputs_apply_to_chat_models_only()
    {
        var capture = new UpstreamCapture();
        var provider = Provider(capture, "test-provider", "k", "https://api.example.com");
        provider.Options.SupportsStructuredOutputs = true;
        var chat = (OpenAICompatibleLanguageModel)provider.LanguageModel("chat");
        var again = provider.CreateChatModel("chat-2");
        Assert.True(chat.SupportsStructuredOutputs);
        Assert.True(again.SupportsStructuredOutputs);
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        var completion = await provider.CompletionModel("completion").DoGenerateAsync(options, CancellationToken.None);
        Assert.Contains(completion.Warnings, warning => warning.Message == "JSON response format is not supported.");
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["response_format"]);
        Assert.IsType<OpenAICompatibleEmbeddingModel>(provider.EmbeddingModel("embed"));
    }

    private static OpenAICompatibleProvider Configured(string name)
    {
        var provider = Provider(new UpstreamCapture(), name, "test-api-key", "https://api.example.com");
        provider.Options.Headers["custom-header"] = "value";
        provider.Options.QueryParameters["Custom-Param"] = "value";
        return provider;
    }

    private static OpenAICompatibleProvider Provider(UpstreamCapture capture, string name, string? key, string baseUrl)
    {
        return OpenAICompatibleProvider.Create(
            new OpenAICompatibleOptions
            {
                ProviderName = name,
                BaseUrl = baseUrl,
                ApiKey = key,
            },
            capture);
    }

    private static UpstreamCapture Stream()
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"text\":\"Hi\",\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
    }
}
