// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Bedrock Agent Runtime rerank requests and rankings.</summary>
public sealed class BedrockRerankTests
{
    private const string Fixture = "{\"results\":[{\"index\":0,\"relevanceScore\":0.5110583305358887},{\"index\":5,\"relevanceScore\":0.30241215229034424}]}";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_documents()
    {
        var body = await Send(true, new[] { "{\"example\":\"sunny day at the beach\"}", "{\"example\":\"rainy day in the city\"}" });

        Assert.Equal("test-token", body.GetProperty("nextToken").GetString());
        Assert.Equal("rainy day", body.GetProperty("queries")[0].GetProperty("textQuery").GetProperty("text").GetString());
        Assert.Equal("TEXT", body.GetProperty("queries")[0].GetProperty("type").GetString());
        Assert.Equal("JSON", body.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("type").GetString());
        Assert.Equal("sunny day at the beach", body.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("jsonDocument").GetProperty("example").GetString());
        Assert.Equal("rainy day in the city", body.GetProperty("sources")[1].GetProperty("inlineDocumentSource").GetProperty("jsonDocument").GetProperty("example").GetString());
        Assert.Equal("arn:aws:bedrock:us-west-2::foundation-model/cohere.rerank-v3-5:0", body.GetProperty("rerankingConfiguration").GetProperty("bedrockRerankingConfiguration").GetProperty("modelConfiguration").GetProperty("modelArn").GetString());
        Assert.Equal("test-value", body.GetProperty("rerankingConfiguration").GetProperty("bedrockRerankingConfiguration").GetProperty("modelConfiguration").GetProperty("additionalModelRequestFields").GetProperty("test").GetString());
        Assert.Equal(2, body.GetProperty("rerankingConfiguration").GetProperty("bedrockRerankingConfiguration").GetProperty("numberOfResults").GetInt32());
        Assert.Equal("BEDROCK_RERANKING_MODEL", body.GetProperty("rerankingConfiguration").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send the AWS-required `bedrockRerankingConfiguration` wire key", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_bedrock_reranking_configuration_member_name()
    {
        var body = await Send(true, new[] { "{\"example\":\"sunny day at the beach\"}" });

        Assert.True(body.GetProperty("rerankingConfiguration").TryGetProperty("bedrockRerankingConfiguration", out _));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_configured_rerank_headers()
    {
        var captured = await Capture(true, new[] { "{\"example\":\"sunny day at the beach\"}" });

        Assert.Equal("config-value", captured.Handler.Headers["config-header"]);
        Assert.Equal("config-shared", captured.Handler.Headers["shared-header"]);
        Assert.Equal("application/json", captured.Handler.Headers["Content-Type"].Split(';')[0]);
        Assert.EndsWith("/rerank", captured.Handler.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_json_document_rankings()
    {
        var captured = await Capture(true, new[] { "{\"example\":\"sunny day at the beach\"}", "{\"example\":\"rainy day in the city\"}" });

        Assert.Equal(0, captured.Result.Ranking[0].Index);
        Assert.Equal(0.5110583305358887, captured.Result.Ranking[0].RelevanceScore);
        Assert.Equal(5, captured.Result.Ranking[1].Index);
        Assert.Equal(0.30241215229034424, captured.Result.Ranking[1].RelevanceScore);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_raw_json_rerank_response()
    {
        var captured = await Capture(true, new[] { "{\"example\":\"sunny day at the beach\"}" });

        Assert.Contains("\"relevanceScore\":0.5110583305358887", captured.Result.RawBody, StringComparison.Ordinal);
        Assert.Equal("application/json", captured.Result.Headers["Content-Type"].Split(';')[0]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_rerank_metadata_in_the_response_body()
    {
        var captured = await Capture(true, new[] { "{\"example\":\"sunny day at the beach\"}" });

        Assert.Equal(2, captured.Result.Ranking.Count);
        Assert.Contains("\"results\"", captured.Result.RawBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_rankings_without_a_separate_warning_list()
    {
        var captured = await Capture(true, new[] { "{\"example\":\"sunny day at the beach\"}" });

        Assert.Equal(2, captured.Result.Ranking.Count);
        Assert.IsType<AmazonBedrockRerankResult>(captured.Result);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_documents()
    {
        var body = await Send(false, new[] { "sunny day at the beach", "rainy day in the city" });

        Assert.Equal("TEXT", body.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("type").GetString());
        Assert.Equal("sunny day at the beach", body.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("textDocument").GetProperty("text").GetString());
        Assert.Equal("rainy day in the city", body.GetProperty("sources")[1].GetProperty("inlineDocumentSource").GetProperty("textDocument").GetProperty("text").GetString());
        Assert.Equal("INLINE", body.GetProperty("sources")[0].GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_rerank_headers()
    {
        var captured = await Capture(false, new[] { "sunny day at the beach" });

        Assert.Equal("config-value", captured.Handler.Headers["config-header"]);
        Assert.StartsWith("AWS4-HMAC-SHA256", captured.Handler.Headers["Authorization"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_text_document_rankings()
    {
        var captured = await Capture(false, new[] { "sunny day at the beach", "rainy day in the city" });

        Assert.Equal(0.5110583305358887, captured.Result.Ranking[0].RelevanceScore);
        Assert.Equal(5, captured.Result.Ranking[1].Index);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_text_rankings_from_the_response_body()
    {
        var captured = await Capture(false, new[] { "sunny day at the beach" });

        Assert.Equal(0, captured.Result.Ranking[0].Index);
        Assert.Contains("results", captured.Result.RawBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_text_rerank_metadata_in_the_response_body()
    {
        var captured = await Capture(false, new[] { "sunny day at the beach" });

        Assert.Contains("\"index\":5", captured.Result.RawBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_raw_text_rerank_response()
    {
        var captured = await Capture(false, new[] { "sunny day at the beach" });

        Assert.Contains("0.30241215229034424", captured.Result.RawBody, StringComparison.Ordinal);
    }

    private static async Task<JsonElement> Send(bool jsonDocuments, string[] documents)
    {
        var captured = await Capture(jsonDocuments, documents);
        using var document = JsonDocument.Parse(captured.Handler.Body);
        return document.RootElement.Clone();
    }

    private static async Task<CapturedRerank> Capture(bool jsonDocuments, string[] documents)
    {
        var handler = new BedrockJsonHandler(Fixture);
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            Region = "us-west-2",
            BaseUrl = "https://bedrock-agent-runtime.us-east-1.amazonaws.com",
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        var model = provider.RerankingModel("cohere.rerank-v3-5:0");
        model.Headers = new Dictionary<string, string?>
        {
            ["config-header"] = "config-value",
            ["shared-header"] = "config-shared",
        };
        var result = await model.DoRerankAsync(
            "rainy day",
            documents,
            jsonDocuments,
            2,
            BedrockParity.Options("{\"bedrock\":{\"nextToken\":\"test-token\",\"additionalModelRequestFields\":{\"test\":\"test-value\"}}}"),
            null,
            CancellationToken.None);
        return new CapturedRerank(handler, result);
    }

    private sealed class CapturedRerank
    {
        public CapturedRerank(BedrockJsonHandler handler, AmazonBedrockRerankResult result)
        {
            Handler = handler;
            Result = result;
        }

        public BedrockJsonHandler Handler { get; }

        public AmazonBedrockRerankResult Result { get; }
    }
}
