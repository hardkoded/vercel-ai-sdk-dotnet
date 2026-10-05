// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Cerebras;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class CerebrasUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-chat-language-model.test.ts::doGenerate::maps maxOutputTokens to max_completion_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Max_output_tokens_are_sent_as_max_completion_tokens()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.MaxOutputTokens = 64;
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal(64, body["max_completion_tokens"]!.GetValue<int>());
        Assert.Null(body["max_tokens"]);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-chat-language-model.test.ts::doGenerate::maps Cerebras provider options to request fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_are_renamed()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "cerebras",
            "{\"user\":\"user-123\",\"strictJsonSchema\":false,\"parallelToolCalls\":false,\"logprobs\":true,\"topLogprobs\":2,\"logitBias\":{\"42\":1},\"serviceTier\":\"priority\",\"reasoningEffort\":\"none\",\"reasoningFormat\":\"parsed\",\"prediction\":{\"type\":\"content\",\"content\":\"expected\"},\"promptCacheKey\":\"cache-key\"}");
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal("user-123", body["user"]!.GetValue<string>());
        Assert.False(body["parallel_tool_calls"]!.GetValue<bool>());
        Assert.True(body["logprobs"]!.GetValue<bool>());
        Assert.Equal(2, body["top_logprobs"]!.GetValue<int>());
        Assert.Equal(1, body["logit_bias"]!["42"]!.GetValue<int>());
        Assert.Equal("priority", body["service_tier"]!.GetValue<string>());
        Assert.Equal("none", body["reasoning_effort"]!.GetValue<string>());
        Assert.Equal("parsed", body["reasoning_format"]!.GetValue<string>());
        Assert.Equal("expected", body["prediction"]!["content"]!.GetValue<string>());
        Assert.Equal("cache-key", body["prompt_cache_key"]!.GetValue<string>());
        Assert.Null(body["parallelToolCalls"]);
        Assert.Null(body["reasoningFormat"]);
        Assert.Null(body["promptCacheKey"]);
        Assert.Null(body["strictJsonSchema"]);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > createCerebras::should create a CerebrasProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_options_target_the_cerebras_api()
    {
        var capture = new UpstreamCapture();
        await Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://api.cerebras.ai/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/cerebras/0.0.0", capture.Requests[0].Headers["User-Agent"]);
        Assert.Equal("CEREBRAS_API_KEY", CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).Options.ApiKeyEnvironmentVariable);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > createCerebras::should create a CerebrasProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_base_url_and_headers_are_kept()
    {
        var capture = new UpstreamCapture();
        var provider = CerebrasProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.cerebras.test/v1" },
            capture);
        provider.Options.Headers["Custom-Header"] = "value";
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("gpt-oss-120b")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://custom.cerebras.test/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("value", capture.Requests[0].Headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > createCerebras::should pass header", Coverage = UpstreamCoverage.Covered)]
    public async Task Call_headers_are_sent()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > createCerebras::should return a chat model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_a_chat_model()
    {
        var model = (OpenAICompatibleLanguageModel)CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("gpt-oss-120b");
        Assert.Equal("cerebras.chat", model.Provider);
        Assert.Equal("gpt-oss-120b", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > createCerebras::should convert assistant reasoning_content to reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Assistant_reasoning_content_is_renamed_to_reasoning()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new AssistantModelMessage("hi", null, "because") };
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        var message = UpstreamChat.Body(capture)["messages"]![0]!;
        Assert.Equal("because", message["reasoning"]!.GetValue<string>());
        Assert.False(message.AsObject().ContainsKey("reasoning_content"));
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > languageModel::should construct a language model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_configuration_matches_chat()
    {
        var model = (OpenAICompatibleLanguageModel)CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("gpt-oss-120b");
        Assert.True(model.SupportsStructuredOutputs);
        Assert.Equal("cerebras.chat", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > embeddingModel::should throw NoSuchModelError when attempting to create embedding model", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_are_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("embed"));
        Assert.Equal("Provider 'cerebras' does not support embedding model 'embed'.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > chat::should construct a chat model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Chat_model_uses_the_chat_provider_id()
    {
        var model = CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateChatModel("gpt-oss-120b");
        Assert.Equal("cerebras.chat", model.Provider);
        Assert.Equal("gpt-oss-120b", model.ModelId);
    }

    private static OpenAICompatibleLanguageModel Chat(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)CerebrasProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("gpt-oss-120b");
    }
}
