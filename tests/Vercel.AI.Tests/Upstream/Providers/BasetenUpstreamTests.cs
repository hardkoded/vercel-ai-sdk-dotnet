// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Baseten;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class BasetenUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-embedding-model.test.ts::doEmbed > modelURL forms::should append /v1 to a bare /sync URL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_sync_url_gains_a_v1_embeddings_path()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Embed(capture, "https://model.example/sync", "embed");
        await model.DoEmbedAsync(new[] { "hi" }, CancellationToken.None);
        Assert.Equal("https://model.example/sync/v1/embeddings", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/baseten/src/baseten-embedding-model.test.ts::doEmbed > modelURL forms::should not double up /v1 for a /sync/v1 URL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_sync_v1_url_is_not_doubled()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Embed(capture, "https://model.example/sync/v1", "embed");
        await model.DoEmbedAsync(new[] { "hi" }, CancellationToken.None);
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
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoEmbedAsync(values, CancellationToken.None));
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
        Assert.Equal("Not supported. You must use a /sync or /sync/v1 endpoint for embeddings.", error.Message);
    }

    private static OpenAICompatibleEmbeddingModel Embed(UpstreamCapture capture, string url, string modelId)
    {
        var provider = BasetenProvider.Create(new BasetenOptions { ApiKey = "secret", ModelUrl = url }, capture);
        return (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel(modelId);
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
