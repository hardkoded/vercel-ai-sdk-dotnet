// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Cohere;

namespace Vercel.AI.Tests;

/// <summary>Cohere rerank requests for object and text documents.</summary>
public sealed class CohereRerankingParityTests
{
    private const string Rerank = "packages/cohere/src/reranking/cohere-reranking-model.test.ts::doRerank > ";

    private const string Body = """
        {"id":"b44fe75b-e3d3-489a-b61e-1a1aede3ef72","results":[{"index":1,"relevance_score":0.10183054},{"index":0,"relevance_score":0.03762639}],"meta":{"api_version":{"version":"2"},"billed_units":{"search_units":1}}}
        """;

    [Fact]
    [UpstreamTest(Rerank + "json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Stringifies_object_documents()
    {
        var handler = await Rank(ObjectDocuments());

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "rerank-english-v3.0",
              "query": "rainy day",
              "documents": [
                "{\"example\":\"sunny day at the beach\"}",
                "{\"example\":\"rainy day in the city\"}"
              ],
              "top_n": 2,
              "max_tokens_per_doc": 1000,
              "priority": 1
            }
            """);
        Assert.Contains("https://api.cohere.com/v2/rerank", handler.Uri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Rerank + "json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Object_documents_send_bearer_json()
    {
        var handler = await Rank(ObjectDocuments());

        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Rerank + "json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Object_documents_warn_about_string_conversion()
    {
        var result = await Result(ObjectDocuments());

        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("object documents", result.Warnings[0].Feature);
        Assert.Equal("Object documents are converted to strings.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Rerank + "json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Object_documents_keep_the_provider_ranking()
    {
        await AssertRanking(ObjectDocuments());
    }

    [Fact]
    [UpstreamTest(Rerank + "json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Object_documents_have_no_provider_metadata()
    {
        var result = await Result(ObjectDocuments());

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Rerank + "json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Object_documents_return_the_raw_response()
    {
        await AssertResponse(ObjectDocuments());
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_documents()
    {
        var handler = await Rank(TextDocuments());

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "rerank-english-v3.0",
              "query": "rainy day",
              "documents": ["sunny day at the beach", "rainy day in the city"],
              "top_n": 2,
              "max_tokens_per_doc": 1000,
              "priority": 1
            }
            """);
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_send_bearer_json()
    {
        var handler = await Rank(TextDocuments());

        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_have_no_warnings()
    {
        var result = await Result(TextDocuments());

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_keep_the_provider_ranking()
    {
        await AssertRanking(TextDocuments());
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_have_no_provider_metadata()
    {
        var result = await Result(TextDocuments());

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Rerank + "text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_return_the_raw_response()
    {
        await AssertResponse(TextDocuments());
    }

    private static async Task AssertRanking(CohereRerankRequest request)
    {
        var result = await Result(request);

        Assert.Equal(1, result.Ranking[0].Index);
        Assert.Equal(0.10183054, result.Ranking[0].Score, 8);
        Assert.Equal(0, result.Ranking[1].Index);
        Assert.Equal(0.03762639, result.Ranking[1].Score, 8);
    }

    private static async Task AssertResponse(CohereRerankRequest request)
    {
        var result = await Result(request);

        Assert.Equal("b44fe75b-e3d3-489a-b61e-1a1aede3ef72", result.Id);
        CohereParity.JsonEqual(result.RawBody, Body);
        Assert.Contains("application/json", result.Headers["Content-Type"], StringComparison.Ordinal);
        Assert.Equal(1, JsonNode.Parse(result.RawBody)!["meta"]!["billed_units"]!["search_units"]!.GetValue<int>());
    }

    private static async Task<CohereRerankResult> Result(CohereRerankRequest request)
    {
        var handler = CohereParity.Handler(Body);
        return await Model(handler).RerankAsync(request, CancellationToken.None);
    }

    private static async Task<CaptureHandler> Rank(CohereRerankRequest request)
    {
        var handler = CohereParity.Handler(Body);
        await Model(handler).RerankAsync(request, CancellationToken.None);
        return handler;
    }

    private static CohereRerankingModel Model(CaptureHandler handler)
    {
        return (CohereRerankingModel)CohereParity.Provider(handler).RerankingModel("rerank-english-v3.0");
    }

    private static CohereRerankRequest ObjectDocuments()
    {
        return new CohereRerankRequest(
            "rainy day",
            new[]
            {
                CohereRerankDocument.Object(CohereParity.Element("{\"example\":\"sunny day at the beach\"}")),
                CohereRerankDocument.Object(CohereParity.Element("{\"example\":\"rainy day in the city\"}")),
            })
        {
            TopN = 2,
            MaxTokensPerDoc = 1000,
            Priority = 1,
        };
    }

    private static CohereRerankRequest TextDocuments()
    {
        return new CohereRerankRequest(
            "rainy day",
            new[]
            {
                CohereRerankDocument.Text("sunny day at the beach"),
                CohereRerankDocument.Text("rainy day in the city"),
            })
        {
            TopN = 2,
            MaxTokensPerDoc = 1000,
            Priority = 1,
        };
    }
}
