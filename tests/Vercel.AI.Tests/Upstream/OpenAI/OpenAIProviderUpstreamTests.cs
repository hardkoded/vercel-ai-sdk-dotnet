// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Provider construction, base URL, and forward-compatible family defaults.</summary>
[Collection("OpenAIEnvironment")]
public sealed class OpenAIProviderUpstreamTests
{
    private static readonly object EnvGate = new();

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::uses the default OpenAI base URL when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesTheDefaultOpenAIBaseUrl()
    {
        await WithBaseUrl(null, async () =>
        {
            var capture = new OpenAICapture
            {
                ResponseJson = "{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":1}}",
            };
            var provider = OpenAIUpstream.Provider(capture);
            await provider.EmbeddingModel("text-embedding-3-small").DoEmbedAsync(new[] { "hello" }, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("https://api.openai.com/v1/embeddings", capture.Uri);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::uses OPENAI_BASE_URL when set", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesOpenAIBaseUrlWhenSet()
    {
        await WithBaseUrl("https://proxy.openai.example/v1/", async () =>
        {
            var capture = new OpenAICapture
            {
                ResponseJson = "{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":1}}",
            };
            var provider = OpenAIUpstream.Provider(capture);
            await provider.EmbeddingModel("text-embedding-3-small").DoEmbedAsync(new[] { "hello" }, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("https://proxy.openai.example/v1/embeddings", capture.Uri);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::prefers the baseURL option over OPENAI_BASE_URL", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersTheBaseUrlOption()
    {
        await WithBaseUrl("https://env.openai.example/v1", async () =>
        {
            var capture = new OpenAICapture
            {
                ResponseJson = "{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":1}}",
            };
            var provider = OpenAIUpstream.Provider(capture, options => options.BaseUrl = "https://option.openai.example/v1/");
            await provider.EmbeddingModel("text-embedding-3-small").DoEmbedAsync(new[] { "hello" }, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("https://option.openai.example/v1/embeddings", capture.Uri);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::rejects an empty baseURL option during provider creation", Coverage = UpstreamCoverage.Covered)]
    public void RejectsAnEmptyBaseUrlOption()
    {
        var exception = Assert.Throws<ArgumentException>(() => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key", BaseUrl = string.Empty }));
        Assert.StartsWith("baseURL must be a non-empty string.", exception.Message, StringComparison.Ordinal);
        Assert.Equal("baseURL", exception.ParamName);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::rejects an empty OPENAI_BASE_URL during provider creation", Coverage = UpstreamCoverage.Covered)]
    public void RejectsAnEmptyOpenAIBaseUrl()
    {
        lock (EnvGate)
        {
            var previous = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
            try
            {
                Environment.SetEnvironmentVariable("OPENAI_BASE_URL", string.Empty);
                var exception = Assert.Throws<ArgumentException>(() => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }));
                Assert.StartsWith("baseURL must be a non-empty string.", exception.Message, StringComparison.Ordinal);
                Assert.Equal("baseURL", exception.ParamName);
            }
            finally
            {
                Environment.SetEnvironmentVariable("OPENAI_BASE_URL", previous);
            }
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::uses the Responses API for the default language model",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port keeps Chat Completions as LanguageModel() unless UseResponsesApi is set, so existing chat request tests stay on /chat/completions.")]
    public async Task DefaultLanguageModelStaysOnChatCompletions()
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture, options => options.BaseUrl = "https://proxy.openai.example/v1/");
        await provider.LanguageModel("gpt-4o-mini").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://proxy.openai.example/v1/chat/completions", capture.Uri);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-provider.test.ts::createOpenAI > baseURL configuration::uses the Chat Completions API for chat models", Coverage = UpstreamCoverage.Covered)]
    public async Task ChatModelsUseChatCompletions()
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture, options => options.BaseUrl = "https://proxy.openai.example/v1/");
        await provider.ChatModel("gpt-4o-mini").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://proxy.openai.example/v1/chat/completions", capture.Uri);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-forward-compatible-defaults.test.ts::OpenAI forward-compatible model-family defaults::uses reasoning-safe Chat Completions request defaults for gpt-99", Coverage = UpstreamCoverage.Covered)]
    public async Task Gpt99ChatDefaultsAreReasoningSafe()
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("Follow the instructions."),
                new UserModelMessage("Say ok."),
            },
            MaxOutputTokens = 64,
            Temperature = 0.2,
            TopP = 0.8,
            FrequencyPenalty = 0.1,
            PresencePenalty = 0.1,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"logitBias\":{\"1\":1},\"logprobs\":2}"),
        };
        var result = await provider.ChatModel("gpt-99").DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        var body = JsonNode.Parse(capture.Body)!.AsObject();
        Assert.Equal("developer", body["messages"]![0]!["role"]!.GetValue<string>());
        Assert.Equal(64, body["max_completion_tokens"]!.GetValue<int>());
        Assert.Null(body["max_tokens"]);
        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
        Assert.Null(body["frequency_penalty"]);
        Assert.Null(body["presence_penalty"]);
        Assert.Null(body["logit_bias"]);
        Assert.Null(body["logprobs"]);
        Assert.Equal("ok", ((GeneratedText)result.Content[0]).Text);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-forward-compatible-defaults.test.ts::OpenAI forward-compatible model-family defaults::uses reasoning-safe Responses API request defaults for gpt-99", Coverage = UpstreamCoverage.Covered)]
    public async Task Gpt99ResponsesDefaultsAreReasoningSafe()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIUpstream.ResponsesEmpty };
        var provider = OpenAIUpstream.Provider(capture);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("Follow the instructions."),
                new UserModelMessage("Say ok."),
            },
            Temperature = 0.2,
            TopP = 0.8,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"reasoningEffort\":\"medium\"}"),
        };
        await provider.ResponsesModel("gpt-99").DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        var body = JsonNode.Parse(capture.Body)!.AsObject();
        Assert.Equal("developer", body["input"]![0]!["role"]!.GetValue<string>());
        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
        Assert.Equal("medium", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.False(body.ContainsKey("stream"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-forward-compatible-defaults.test.ts::OpenAI forward-compatible model-family defaults::uses GPT Image family defaults for gpt-image-99", Coverage = UpstreamCoverage.Covered)]
    public async Task GptImage99OmitsResponseFormat()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"created\":1,\"data\":[{\"b64_json\":\"AQID\"},{\"b64_json\":\"AQID\"}]}" };
        var provider = OpenAIUpstream.Provider(capture);
        var model = (OpenAIImageModel)provider.ImageModel("gpt-image-99");
        Assert.Equal(10, model.MaxImagesPerCall);
        await model.GenerateAsync(new OpenAIImageCall("A black square.") { Count = 2 }, CancellationToken.None).ConfigureAwait(false);
        var body = JsonNode.Parse(capture.Body)!.AsObject();
        Assert.Null(body["response_format"]);
        Assert.Equal(2, body["n"]!.GetValue<int>());
        Assert.Equal("https://api.openai.com/v1/images/generations", capture.Uri);
    }

    private static async Task WithBaseUrl(string? value, Func<Task> body)
    {
        lock (EnvGate)
        {
            var previous = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
            Environment.SetEnvironmentVariable("OPENAI_BASE_URL", value);
            try
            {
                body().GetAwaiter().GetResult();
            }
            finally
            {
                Environment.SetEnvironmentVariable("OPENAI_BASE_URL", previous);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
