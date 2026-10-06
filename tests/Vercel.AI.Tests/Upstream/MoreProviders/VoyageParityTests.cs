// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Voyage;

namespace Vercel.AI.Tests;

/// <summary>Voyage embedding and rerank requests matched to the upstream catalog.</summary>
public sealed class VoyageParityTests
{
    private const string EmbeddingJson = "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"embedding\":[0.000344163,-0.022529466,0.010127448,0.063431956,0.016145896],\"index\":0,\"text\":null},{\"object\":\"embedding\",\"embedding\":[0.018987041,-0.029901529,-0.005134966,0.082804598,-0.008740067],\"index\":1,\"text\":null}],\"model\":\"voyage-3.5\",\"usage\":{\"total_tokens\":12}}";

    private const string RerankJson = "{\"object\":\"list\",\"data\":[{\"relevance_score\":0.5703125,\"index\":1},{\"relevance_score\":0.255859375,\"index\":0}],\"model\":\"rerank-2.5\",\"usage\":{\"total_tokens\":12}}";

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_returns_the_fixture_vectors_in_index_order()
    {
        var (provider, _) = Embedding();
        var result = await provider.EmbeddingModel("voyage-3.5").DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        AssertVector(result.Embeddings[0], 0.000344163, -0.022529466, 0.010127448, 0.063431956, 0.016145896);
        AssertVector(result.Embeddings[1], 0.018987041, -0.029901529, -0.005134966, 0.082804598, -0.008740067);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should expose the raw response", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_exposes_the_raw_body_and_response_headers()
    {
        var (provider, _) = Embedding(("test-header", "test-value"));
        var model = (VoyageProvider.VoyageEmbeddingModel)provider.EmbeddingModel("voyage-3.5");
        var result = await model.EmbedAsync(new VoyageEmbeddingRequest(Values), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(EmbeddingJson, result.Body.GetRawText());
        Assert.Equal("317", Header(result.Headers, "content-length"));
        Assert.Equal("application/json", Header(result.Headers, "content-type"));
        Assert.Equal("test-value", Header(result.Headers, "test-header"));
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_reads_total_tokens()
    {
        var (provider, _) = Embedding();
        var result = await provider.EmbeddingModel("voyage-3.5").DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(12, result.Tokens);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_input_and_model_only()
    {
        var (provider, handler) = Embedding();
        await provider.EmbeddingModel("voyage-3.5").DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("{\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"model\":\"voyage-3.5\"}", handler.Calls[0].Text);
        Assert.Equal("https://api.voyageai.com/v1/embeddings", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the input_type setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_input_type()
    {
        var (provider, handler) = Embedding();
        var model = (VoyageProvider.VoyageEmbeddingModel)provider.EmbeddingModel("voyage-3.5");
        await model.EmbedAsync(new VoyageEmbeddingRequest(Values) { InputType = "document" }, CancellationToken.None).ConfigureAwait(false);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal("document", body.RootElement.GetProperty("input_type").GetString());
        Assert.Equal("voyage-3.5", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("sunny day at the beach", body.RootElement.GetProperty("input")[0].GetString());
        Assert.Equal("rainy day in the city", body.RootElement.GetProperty("input")[1].GetString());
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass the output_dimension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_output_dimension()
    {
        var (provider, handler) = Embedding();
        var model = (VoyageProvider.VoyageEmbeddingModel)provider.EmbeddingModel("voyage-3.5");
        await model.EmbedAsync(new VoyageEmbeddingRequest(Values) { OutputDimension = 256 }, CancellationToken.None).ConfigureAwait(false);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal(256, body.RootElement.GetProperty("output_dimension").GetInt32());
        Assert.Equal("voyage-3.5", body.RootElement.GetProperty("model").GetString());
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Embed_sends_authorization_custom_headers_and_user_agent()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(EmbeddingJson));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = VoyageProvider.Create(options, handler);
        var model = (VoyageProvider.VoyageEmbeddingModel)provider.EmbeddingModel("voyage-3.5");
        await model.EmbedAsync(new VoyageEmbeddingRequest(Values) { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/voyage/" + AiSdkVersion.Version, call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should create embedding model with correct provider and modelId", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_model_reports_voyage_embedding()
    {
        var model = VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).EmbeddingModel("voyage-3.5");
        Assert.Equal("voyage-3.5", model.ModelId);
        Assert.Equal("voyage.embedding", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should create reranking model with correct provider and modelId", Coverage = UpstreamCoverage.Covered)]
    public void Reranking_model_reports_voyage_reranking()
    {
        var model = VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }).RerankingModel("rerank-2.5");
        Assert.Equal("rerank-2.5", model.ModelId);
        Assert.Equal("voyage.reranking", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should throw NoSuchModelError for languageModel", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_rejected()
    {
        var provider = VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("some-model"));
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/voyage-provider.test.ts::VoyageProvider::should throw NoSuchModelError for imageModel", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_is_rejected()
    {
        var provider = VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" });
        Assert.Throws<AiSdkException>(() => provider.ImageModel("some-model"));
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_stringifies_object_documents()
    {
        var (provider, handler) = Rerank();
        await Objects(provider).ConfigureAwait(false);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal("{\"example\":\"sunny day at the beach\"}", body.RootElement.GetProperty("documents")[0].GetString());
        Assert.Equal("{\"example\":\"rainy day in the city\"}", body.RootElement.GetProperty("documents")[1].GetString());
        Assert.Equal("rerank-2.5", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("rainy day", body.RootElement.GetProperty("query").GetString());
        Assert.Equal(2, body.RootElement.GetProperty("top_k").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_object_documents_use_bearer_json()
    {
        var (provider, handler) = Rerank();
        await Objects(provider).ConfigureAwait(false);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_object_documents_warn_about_string_conversion()
    {
        var (provider, _) = Rerank();
        var result = await Objects(provider).ConfigureAwait(false);
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("object documents", result.Warnings[0].Feature);
        Assert.Equal("Object documents are converted to strings.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_object_documents_keep_provider_order()
    {
        var (provider, _) = Rerank();
        var result = await Objects(provider).ConfigureAwait(false);
        AssertRanking(result.Ranking);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > object documents::should return result with the correct response body", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_object_documents_return_the_fixture_body()
    {
        var (provider, _) = Rerank();
        var result = await Objects(provider).ConfigureAwait(false);
        AssertRerankBody(result);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_sends_text_documents()
    {
        var (provider, handler) = Rerank();
        await Texts(provider).ConfigureAwait(false);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal("sunny day at the beach", body.RootElement.GetProperty("documents")[0].GetString());
        Assert.Equal("rainy day in the city", body.RootElement.GetProperty("documents")[1].GetString());
        Assert.Equal("rerank-2.5", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("rainy day", body.RootElement.GetProperty("query").GetString());
        Assert.Equal(2, body.RootElement.GetProperty("top_k").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_text_documents_use_bearer_json()
    {
        var (provider, handler) = Rerank();
        await Texts(provider).ConfigureAwait(false);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_text_documents_have_no_warnings()
    {
        var (provider, _) = Rerank();
        var result = await Texts(provider).ConfigureAwait(false);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_text_documents_keep_provider_order()
    {
        var (provider, _) = Rerank();
        var result = await Texts(provider).ConfigureAwait(false);
        AssertRanking(result.Ranking);
    }

    [Fact]
    [UpstreamTest("packages/voyage/src/reranking/voyage-reranking-model.test.ts::doRerank > text documents::should return result with the correct response body", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_text_documents_return_the_fixture_body()
    {
        var (provider, _) = Rerank();
        var result = await Texts(provider).ConfigureAwait(false);
        AssertRerankBody(result);
    }

    private static (VoyageProvider Provider, ParityHandler Handler) Embedding(params (string Name, string Value)[] headers)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(EmbeddingJson, headers));
        return (VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), handler);
    }

    private static (VoyageProvider Provider, ParityHandler Handler) Rerank()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(RerankJson));
        return (VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), handler);
    }

    private static Task<VoyageRerankResult> Objects(VoyageProvider provider)
    {
        var model = (VoyageProvider.VoyageRerankingModel)provider.RerankingModel("rerank-2.5");
        return model.RerankAsync(new VoyageRerankRequest
        {
            Query = "rainy day",
            TopK = 2,
            ObjectDocuments = new[]
            {
                new JsonObject { ["example"] = "sunny day at the beach" },
                new JsonObject { ["example"] = "rainy day in the city" },
            },
        }, CancellationToken.None);
    }

    private static Task<VoyageRerankResult> Texts(VoyageProvider provider)
    {
        var model = (VoyageProvider.VoyageRerankingModel)provider.RerankingModel("rerank-2.5");
        return model.RerankAsync(new VoyageRerankRequest { Query = "rainy day", TopK = 2, TextDocuments = Values }, CancellationToken.None);
    }

    private static void AssertVector(float[] actual, params double[] expected)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.True(Math.Abs(expected[index] - actual[index]) < 0.0000001d);
        }
    }

    private static void AssertRanking(IReadOnlyList<RerankItem> ranking)
    {
        Assert.Equal(1, ranking[0].Index);
        Assert.Equal(0.5703125, ranking[0].Score, 7);
        Assert.Equal(0, ranking[1].Index);
        Assert.Equal(0.255859375, ranking[1].Score, 7);
    }

    private static void AssertRerankBody(VoyageRerankResult result)
    {
        Assert.Equal(RerankJson, result.Body.GetRawText());
        Assert.Equal("157", Header(result.Headers, "content-length"));
        Assert.Equal("application/json", Header(result.Headers, "content-type"));
    }

    private static string? Header(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}
