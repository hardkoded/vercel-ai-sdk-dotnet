// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel.GatewayRerankingModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel;

/// <summary>Port of <c>gateway-reranking-model.test.ts</c> &gt; <c>GatewayRerankingModel &gt; doRerank</c>.</summary>
public sealed class GatewayRerankingModelDoRerankTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-reranking-model.test.ts::GatewayRerankingModel > doRerank::";

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var (model, capture) = Setup();
        await model.DoRerankAsync(
            Call(headers: new Dictionary<string, string> { ["Custom-Header"] = "test-value" }),
            CancellationToken.None
        );
        Assert.Equal("Bearer test-token", capture.Headers["authorization"]);
        Assert.Equal("test-value", capture.Headers["custom-header"]);
        Assert.Equal("4", capture.Headers["ai-reranking-model-specification-version"]);
        Assert.Equal("cohere/rerank-v3.5", capture.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers()
    {
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "deployment-1",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };
        var (model, capture) = Setup(o11yHeaders: o11yHeaders);
        await model.DoRerankAsync(Call(), CancellationToken.None);
        foreach (var pair in o11yHeaders)
        {
            Assert.Equal(pair.Value, capture.Headers[pair.Key]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract ranking from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_ranking_from_response()
    {
        var (model, _) = Setup();
        var result = await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.Equal(
            [(0, 0.89), (2, 0.15), (1, 0.12)],
            result.Ranking.Select(rank => (rank.Index, rank.RelevanceScore)).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_warnings()
    {
        var (model, _) = Setup(
            "{\"ranking\":"
                + DummyRanking
                + ",\"warnings\":[{\"type\":\"unsupported\",\"feature\":\"topN\",\"details\":\"This model ignores topN.\"}]}"
        );
        var result = await model.DoRerankAsync(Call(), CancellationToken.None);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("topN", warning.Feature);
        Assert.Equal("This model ignores topN.", warning.Details);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default warnings to an empty array when absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_warnings_to_an_empty_array_when_absent()
    {
        var (model, _) = Setup();
        var result = await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send documents and query in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_documents_and_query_in_request_body()
    {
        var (model, capture) = Setup();
        await model.DoRerankAsync(Call(topN: 2), CancellationToken.None);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"documents\":"
                        + TestDocuments
                        + ",\"query\":\""
                        + TestQuery
                        + "\",\"topN\":2}"
                ),
                JsonNode.Parse(capture.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerOptions into request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_providerOptions_into_request_body()
    {
        var (model, capture) = Setup();
        await model.DoRerankAsync(
            Call(providerOptions: "{\"cohere\":{\"maxTokensPerDoc\":512}}"),
            CancellationToken.None
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"cohere\":{\"maxTokensPerDoc\":512}}"),
                JsonNode.Parse(capture.Body)!["providerOptions"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit topN when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_topN_when_not_provided()
    {
        var (model, capture) = Setup();
        await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"documents\":" + TestDocuments + ",\"query\":\"" + TestQuery + "\"}"
                ),
                JsonNode.Parse(capture.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should return response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_response_headers()
    {
        var (model, capture) = Setup();
        capture.ResponseHeaders["x-request-id"] = "req-123";
        var result = await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.Equal("req-123", result.Response!.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_provider_metadata()
    {
        var (model, _) = Setup(
            "{\"ranking\":"
                + DummyRanking
                + ",\"providerMetadata\":{\"gateway\":{\"cost\":\"0.002\"}}}"
        );
        var result = await model.DoRerankAsync(Call(), CancellationToken.None);
        Assert.NotNull(result.ProviderMetadata);
        Assert.Equal(
            "0.002",
            result.ProviderMetadata!.Value.GetProperty("gateway").GetProperty("cost").GetString()
        );
    }
}
