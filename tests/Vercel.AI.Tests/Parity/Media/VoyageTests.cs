// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Parity.Media;
using Vercel.AI.Voyage;

namespace Vercel.AI.Tests;

public sealed class VoyageTests
{
    private const string EmbeddingUrl = "https://api.voyageai.com/v1/embeddings";
    private const string RerankUrl = "https://api.voyageai.com/v1/rerank";
    private const string EmbeddingJson = "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"embedding\":[0.000344163,-0.022529466,0.010127448,0.063431956,0.016145896],\"index\":0,\"text\":null},{\"object\":\"embedding\",\"embedding\":[0.018987041,-0.029901529,-0.005134966,0.082804598,-0.008740067],\"index\":1,\"text\":null}],\"model\":\"voyage-3.5\",\"usage\":{\"total_tokens\":12}}";
    private const string RerankJson = "{\"object\":\"list\",\"data\":[{\"relevance_score\":0.5703125,\"index\":1},{\"relevance_score\":0.255859375,\"index\":0}],\"model\":\"rerank-2.5\",\"usage\":{\"total_tokens\":12}}";

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should create embedding model with correct provider and modelId",
        Coverage = UpstreamCoverage.Covered)]
    public void Creates_an_embedding_model()
    {
        var model = Provider().EmbeddingModel("voyage-3.5");
        Assert.Equal("voyage-3.5", model.ModelId);
        Assert.Equal("voyage.embedding", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should create reranking model with correct provider and modelId",
        Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_reranking_model()
    {
        var model = Assert.IsType<VoyageRerankingModel>(Provider().RerankingModel("rerank-2.5"));
        Assert.Equal("rerank-2.5", model.ModelId);
        Assert.Equal("voyage.reranking", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should throw NoSuchModelError for languageModel",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_language_model()
    {
        var error = Assert.Throws<AiSdkException>(() => Provider().LanguageModel("some-model"));
        Assert.Contains("languageModel", error.Message, StringComparison.Ordinal);
        Assert.Contains("some-model", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should throw NoSuchModelError for imageModel",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_an_image_model()
    {
        var error = Assert.Throws<AiSdkException>(() => Provider().ImageModel("some-model"));
        Assert.Contains("imageModel", error.Message, StringComparison.Ordinal);
        Assert.Contains("some-model", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should extract embedding",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_embeddings()
    {
        var result = await Embed(Embeddings());
        Assert.Equal(new[] { 0.000344163d, -0.022529466d, 0.010127448d, 0.063431956d, 0.016145896d }, result.Embeddings[0]);
        Assert.Equal(new[] { 0.018987041d, -0.029901529d, -0.005134966d, 0.082804598d, -0.008740067d }, result.Embeddings[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should expose the raw response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_the_raw_embedding_response()
    {
        var handler = new MediaHandler();
        handler.Json(EmbeddingUrl, EmbeddingJson, new Dictionary<string, string> { ["test-header"] = "test-value" });
        var result = await Embed(handler);

        Assert.Equal("317", result.ResponseHeaders["content-length"]);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal("voyage-3.5", MediaJson.Parse(result.ResponseBody)["model"]!.GetValue<string>());
        Assert.Equal(12, MediaJson.Parse(result.ResponseBody)["usage"]!["total_tokens"]!.GetValue<int>());
        Assert.Null(MediaJson.Parse(result.ResponseBody)["data"]![0]!["text"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should extract usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_embedding_usage()
    {
        var result = await Embed(Embeddings());
        Assert.Equal(12, result.Tokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the model and the values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_model_and_values()
    {
        var handler = Embeddings();
        await Embed(handler);
        var body = MediaJson.Parse(handler.Calls[0].Body);

        Assert.Equal("voyage-3.5", body["model"]!.GetValue<string>());
        Assert.Equal("sunny day at the beach", body["input"]![0]!.GetValue<string>());
        Assert.Equal("rainy day in the city", body["input"]![1]!.GetValue<string>());
        Assert.Null(body["input_type"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the input_type setting",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_input_type()
    {
        var handler = Embeddings();
        var model = EmbeddingModel(handler);
        await model.EmbedAsync(new VoyageEmbeddingRequest(Values) { InputType = "document" }, CancellationToken.None);

        Assert.Equal("document", MediaJson.Parse(handler.Calls[0].Body)["input_type"]!.GetValue<string>());
        Assert.Equal("voyage-3.5", MediaJson.Parse(handler.Calls[0].Body)["model"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the output_dimension setting",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_output_dimension()
    {
        var handler = Embeddings();
        var model = EmbeddingModel(handler);
        await model.EmbedAsync(new VoyageEmbeddingRequest(Values) { OutputDimension = 256 }, CancellationToken.None);

        Assert.Equal(256, MediaJson.Parse(handler.Calls[0].Body)["output_dimension"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_embedding_headers()
    {
        var handler = Embeddings();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = VoyageProvider.Create(options, handler);
        var model = (VoyageEmbeddingModel)provider.EmbeddingModel("voyage-3.5");
        await model.EmbedAsync(
            new VoyageEmbeddingRequest(Values)
            {
                Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("custom-request-header"));
        Assert.Contains("ai-sdk/voyage/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should send request with stringified json documents",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_stringified_object_documents()
    {
        var handler = Rerank();
        await Objects(handler);
        var documents = MediaJson.Parse(handler.Calls[0].Body)["documents"]!.AsArray();

        Assert.Equal("{\"example\":\"sunny day at the beach\"}", documents[0]!.GetValue<string>());
        Assert.Equal("{\"example\":\"rainy day in the city\"}", documents[1]!.GetValue<string>());
        Assert.Equal("rerank-2.5", MediaJson.Parse(handler.Calls[0].Body)["model"]!.GetValue<string>());
        Assert.Equal("rainy day", MediaJson.Parse(handler.Calls[0].Body)["query"]!.GetValue<string>());
        Assert.Equal(2, MediaJson.Parse(handler.Calls[0].Body)["top_k"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should send request with the correct headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_object_rerank_headers()
    {
        var handler = Rerank();
        await Objects(handler);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with warnings",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_that_object_documents_are_converted()
    {
        var result = await Objects(Rerank());
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("object documents", result.Warnings[0].Feature);
        Assert.Equal("Object documents are converted to strings.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with the correct ranking",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_object_document_ranking()
    {
        var result = await Objects(Rerank());
        Assert.Equal(1, result.Ranking[0].Index);
        Assert.Equal(0.5703125d, result.Ranking[0].RelevanceScore);
        Assert.Equal(0, result.Ranking[1].Index);
        Assert.Equal(0.255859375d, result.Ranking[1].RelevanceScore);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with the correct response body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_object_rerank_response()
    {
        var result = await Objects(Rerank());
        Assert.Equal("157", result.ResponseHeaders["content-length"]);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("rerank-2.5", MediaJson.Parse(result.ResponseBody)["model"]!.GetValue<string>());
        Assert.Equal(12, MediaJson.Parse(result.ResponseBody)["usage"]!["total_tokens"]!.GetValue<int>());
        Assert.Equal(1, MediaJson.Parse(result.ResponseBody)["data"]![0]!["index"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should send request with text documents",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_documents()
    {
        var handler = Rerank();
        await Text(handler);
        var documents = MediaJson.Parse(handler.Calls[0].Body)["documents"]!.AsArray();

        Assert.Equal("sunny day at the beach", documents[0]!.GetValue<string>());
        Assert.Equal("rainy day in the city", documents[1]!.GetValue<string>());
        Assert.Equal(2, MediaJson.Parse(handler.Calls[0].Body)["top_k"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_rerank_headers()
    {
        var handler = Rerank();
        await Text(handler);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("content-type"));
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result without warnings",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_no_warnings_for_text_documents()
    {
        var result = await Text(Rerank());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_text_document_ranking()
    {
        var result = await Text(Rerank());
        Assert.Equal(1, result.Ranking[0].Index);
        Assert.Equal(0.5703125d, result.Ranking[0].RelevanceScore);
        Assert.Equal(0, result.Ranking[1].Index);
        Assert.Equal(0.255859375d, result.Ranking[1].RelevanceScore);
    }

    [Fact]
    [UpstreamTest(
        "packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result with the correct response body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_text_rerank_response()
    {
        var result = await Text(Rerank());
        Assert.Equal("157", result.ResponseHeaders["content-length"]);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal(0.5703125d, MediaJson.Parse(result.ResponseBody)["data"]![0]!["relevance_score"]!.GetValue<double>());
    }

    private static VoyageProvider Provider()
    {
        return VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
    }

    private static MediaHandler Embeddings()
    {
        var handler = new MediaHandler();
        handler.Json(EmbeddingUrl, EmbeddingJson);
        return handler;
    }

    private static MediaHandler Rerank()
    {
        var handler = new MediaHandler();
        handler.Json(RerankUrl, RerankJson);
        return handler;
    }

    private static VoyageEmbeddingModel EmbeddingModel(MediaHandler handler)
    {
        return (VoyageEmbeddingModel)VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler).EmbeddingModel("voyage-3.5");
    }

    private static Task<VoyageEmbeddingGeneration> Embed(MediaHandler handler)
    {
        return EmbeddingModel(handler).EmbedAsync(new VoyageEmbeddingRequest(Values), CancellationToken.None);
    }

    private static VoyageRerankingModel RerankingModel(MediaHandler handler)
    {
        return (VoyageRerankingModel)VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler).RerankingModel("rerank-2.5");
    }

    private static Task<VoyageRerankGeneration> Text(MediaHandler handler)
    {
        return RerankingModel(handler).RerankTextAsync("rainy day", Values, 2, null, CancellationToken.None);
    }

    private static Task<VoyageRerankGeneration> Objects(MediaHandler handler)
    {
        using var sunny = JsonDocument.Parse("{\"example\":\"sunny day at the beach\"}");
        using var rainy = JsonDocument.Parse("{\"example\":\"rainy day in the city\"}");
        return RerankingModel(handler).RerankObjectsAsync(
            "rainy day",
            new[] { sunny.RootElement.Clone(), rainy.RootElement.Clone() },
            2,
            null,
            CancellationToken.None);
    }
}
