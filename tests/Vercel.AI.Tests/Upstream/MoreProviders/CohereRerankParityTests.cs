// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests;

/// <summary>Cohere v2 rerank requests matched to the upstream catalog.</summary>
public sealed class CohereRerankParityTests
{
    private const string Fixture = "{\"id\":\"b44fe75b-e3d3-489a-b61e-1a1aede3ef72\",\"results\":[{\"index\":1,\"relevance_score\":0.10183054},{\"index\":0,\"relevance_score\":0.03762639}],\"meta\":{\"api_version\":{\"version\":\"2\"},\"billed_units\":{\"search_units\":1}}}";

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_are_stringified_with_rerank_options()
    {
        var (provider, handler) = Client();
        await Objects(provider).ConfigureAwait(false);
        AssertRequest(handler.Calls[0].Text, "{\"example\":\"sunny day at the beach\"}", "{\"example\":\"rainy day in the city\"}");
        Assert.Equal("https://api.cohere.com/v2/rerank", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_use_bearer_json()
    {
        var (provider, handler) = Client();
        await Objects(provider).ConfigureAwait(false);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_warn_about_string_conversion()
    {
        var (provider, _) = Client();
        var result = await Objects(provider).ConfigureAwait(false);
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("object documents", result.Warnings[0].Feature);
        Assert.Equal("Object documents are converted to strings.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_keep_provider_order()
    {
        var (provider, _) = Client();
        AssertRanking((await Objects(provider).ConfigureAwait(false)).Ranking);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_leave_provider_metadata_unset()
    {
        var (provider, _) = Client();
        Assert.Null((await Objects(provider).ConfigureAwait(false)).ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_return_the_fixture_response()
    {
        var (provider, _) = Client();
        AssertResponse(await Objects(provider).ConfigureAwait(false));
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_are_sent_with_rerank_options()
    {
        var (provider, handler) = Client();
        await Texts(provider).ConfigureAwait(false);
        AssertRequest(handler.Calls[0].Text, "sunny day at the beach", "rainy day in the city");
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_use_bearer_json()
    {
        var (provider, handler) = Client();
        await Texts(provider).ConfigureAwait(false);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_have_no_warnings()
    {
        var (provider, _) = Client();
        Assert.Empty((await Texts(provider).ConfigureAwait(false)).Warnings);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_keep_provider_order()
    {
        var (provider, _) = Client();
        AssertRanking((await Texts(provider).ConfigureAwait(false)).Ranking);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_leave_provider_metadata_unset()
    {
        var (provider, _) = Client();
        Assert.Null((await Texts(provider).ConfigureAwait(false)).ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_return_the_fixture_response()
    {
        var (provider, _) = Client();
        AssertResponse(await Texts(provider).ConfigureAwait(false));
    }

    private static (CohereProvider Provider, ParityHandler Handler) Client()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(Fixture));
        return (CohereProvider.Create(new CohereOptions { ApiKey = "test-api-key" }, handler), handler);
    }

    private static Task<CohereRerankResult> Objects(CohereProvider provider)
    {
        return ((CohereRerankingModel)provider.RerankingModel("rerank-english-v3.0")).RerankAsync(new CohereRerankRequest
        {
            Query = "rainy day",
            TopN = 2,
            MaxTokensPerDoc = 1000,
            Priority = 1,
            ObjectDocuments = new[]
            {
                new JsonObject { ["example"] = "sunny day at the beach" },
                new JsonObject { ["example"] = "rainy day in the city" },
            },
        }, CancellationToken.None);
    }

    private static Task<CohereRerankResult> Texts(CohereProvider provider)
    {
        return ((CohereRerankingModel)provider.RerankingModel("rerank-english-v3.0")).RerankAsync(new CohereRerankRequest
        {
            Query = "rainy day",
            TopN = 2,
            MaxTokensPerDoc = 1000,
            Priority = 1,
            TextDocuments = new[] { "sunny day at the beach", "rainy day in the city" },
        }, CancellationToken.None);
    }

    private static void AssertRequest(string json, string first, string second)
    {
        using var body = JsonDocument.Parse(json);
        Assert.Equal("rerank-english-v3.0", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("rainy day", body.RootElement.GetProperty("query").GetString());
        Assert.Equal(first, body.RootElement.GetProperty("documents")[0].GetString());
        Assert.Equal(second, body.RootElement.GetProperty("documents")[1].GetString());
        Assert.Equal(2, body.RootElement.GetProperty("top_n").GetInt32());
        Assert.Equal(1000, body.RootElement.GetProperty("max_tokens_per_doc").GetInt32());
        Assert.Equal(1, body.RootElement.GetProperty("priority").GetInt32());
    }

    private static void AssertRanking(IReadOnlyList<RerankItem> ranking)
    {
        Assert.Equal(1, ranking[0].Index);
        Assert.Equal(0.10183054, ranking[0].Score, 8);
        Assert.Equal(0, ranking[1].Index);
        Assert.Equal(0.03762639, ranking[1].Score, 8);
    }

    private static void AssertResponse(CohereRerankResult result)
    {
        Assert.Equal("b44fe75b-e3d3-489a-b61e-1a1aede3ef72", result.Response.Id);
        Assert.Equal(Fixture, result.Response.Body.GetRawText());
        Assert.Equal("212", Header(result.Response.Headers, "content-length"));
        Assert.Equal("application/json", Header(result.Response.Headers, "content-type"));
        Assert.Equal("2", result.Response.Body.GetProperty("meta").GetProperty("api_version").GetProperty("version").GetString());
        Assert.Equal(1, result.Response.Body.GetProperty("meta").GetProperty("billed_units").GetProperty("search_units").GetInt32());
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
