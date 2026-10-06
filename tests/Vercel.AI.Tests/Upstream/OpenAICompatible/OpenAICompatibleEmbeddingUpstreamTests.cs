// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleEmbeddingUpstreamTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_returns_the_vectors()
    {
        var result = await Embed("{\"data\":[{\"embedding\":[0.1,0.2]},{\"embedding\":[0.3,0.4]}],\"usage\":{\"prompt_tokens\":8}}");
        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(0.1f, result.Embeddings[0][0]);
        Assert.Equal(0.4f, result.Embeddings[1][1]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_exposes_response_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[{\"embedding\":[0.1]}],\"usage\":{\"prompt_tokens\":1}}" };
        capture.ResponseHeaders["test-header"] = "test-value";
        var model = Model(capture);
        await model.DoEmbedAsync(new[] { "hi" }, null, CancellationToken.None);
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_uses_prompt_tokens()
    {
        var result = await Embed("{\"data\":[{\"embedding\":[0.1]}],\"usage\":{\"prompt_tokens\":20,\"total_tokens\":20}}");
        Assert.Equal(20, result.Tokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_model_input_and_float_encoding()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        await Model(capture, "text-embedding-3-large").DoEmbedAsync(Values, null, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("text-embedding-3-large", body["model"]!.GetValue<string>());
        Assert.Equal("sunny day at the beach", body["input"]![0]!.GetValue<string>());
        Assert.Equal("rainy day in the city", body["input"]![1]!.GetValue<string>());
        Assert.Equal("float", body["encoding_format"]!.GetValue<string>());
        Assert.EndsWith("/embeddings", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should pass the dimensions setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_dimensions_from_provider_options()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Model(capture, "text-embedding-3-large", "test-provider");
        model.ProviderOptions = UpstreamChat.Bag("openaiCompatible", "{\"dimensions\":64}");
        await model.DoEmbedAsync(Values, null, CancellationToken.None);
        Assert.Equal(64, JsonNode.Parse(capture.Requests[0].Body)!["dimensions"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should pass settings with deprecated openai-compatible key and emit warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_accepts_the_deprecated_key_and_warns()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Model(capture, "text-embedding-3-large", "test-provider");
        model.ProviderOptions = UpstreamChat.Bag("openai-compatible", "{\"dimensions\":64}");
        await model.DoEmbedAsync(Values, null, CancellationToken.None);
        Assert.Equal(64, JsonNode.Parse(capture.Requests[0].Body)!["dimensions"]!.GetValue<int>());
        Assert.Contains(model.LastWarnings, warning => warning.Type == "deprecated" && warning.Message.Contains("openaiCompatible"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should emit deprecated warning when raw provider name key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_warns_when_the_raw_provider_key_is_used()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Model(capture, "text-embedding-3-large", "test-provider");
        model.ProviderOptions = UpstreamChat.Bag("test-provider", "{\"dimensions\":64}");
        await model.DoEmbedAsync(Values, null, CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Type == "deprecated" && warning.Message.Contains("testProvider"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should not emit deprecated warning when camelCase provider name key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_does_not_warn_for_the_camel_case_key()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Model(capture, "text-embedding-3-large", "test-provider");
        model.ProviderOptions = UpstreamChat.Bag("testProvider", "{\"dimensions\":64}");
        await model.DoEmbedAsync(Values, null, CancellationToken.None);
        Assert.DoesNotContain(model.LastWarnings, warning => warning.Type == "deprecated");
        Assert.Equal(64, JsonNode.Parse(capture.Requests[0].Body)!["dimensions"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_passes_provider_and_call_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var provider = OpenAICompatibleProvider.Create(
            new OpenAICompatibleOptions
            {
                ProviderName = "test-provider",
                BaseUrl = "https://my.api.com/v1",
                ApiKey = "test-api-key",
            },
            capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel("text-embedding-3-large");
        model.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await model.DoEmbedAsync(Values, null, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("provider-header-value", capture.Requests[0].Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", capture.Requests[0].Headers["Custom-Request-Header"]);
    }

    private static async Task<EmbeddingResult> Embed(string body)
    {
        var capture = new UpstreamCapture { ResponseBody = body };
        return await Model(capture).DoEmbedAsync(new[] { "hi", "there" }, null, CancellationToken.None);
    }

    private static OpenAICompatibleEmbeddingModel Model(UpstreamCapture capture, string modelId = "embed", string name = "openai-compatible")
    {
        var provider = OpenAICompatibleProvider.Create(
            new OpenAICompatibleOptions
            {
                ProviderName = name,
                BaseUrl = "https://my.api.com/v1",
                ApiKey = "secret",
            },
            capture);
        return (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel(modelId);
    }
}
