// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Cohere;

namespace Vercel.AI.Tests;

/// <summary>Cohere embedding requests and parsed vectors.</summary>
public sealed class CohereEmbeddingParityTests
{
    private const string Embed = "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed::";

    private const string Body = """
        {"id":"f5aa3e7b-f011-4c5c-a825-f94669f760e5","texts":["sunny day at the beach","rainy day in the city"],"embeddings":{"float":[[0.03302002,0.020904541,-0.019744873,-0.0625,0.04437256],[-0.04660034,0.00037765503,-0.061157227,-0.08239746,-0.010360718]]},"meta":{"api_version":{"version":"2"},"billed_units":{"input_tokens":10}},"response_type":"embeddings_by_type"}
        """;

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest(Embed + "should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_float_embeddings()
    {
        var result = await EmbedAsync(new CohereEmbedRequest(Values));

        using var document = JsonDocument.Parse(Body);
        var floats = document.RootElement.GetProperty("embeddings").GetProperty("float");
        Assert.Equal(2, result.Embeddings.Count);
        for (var row = 0; row < 2; row++)
        {
            Assert.Equal(5, result.Embeddings[row].Length);
            for (var column = 0; column < 5; column++)
            {
                Assert.Equal(floats[row][column].GetSingle(), result.Embeddings[row][column]);
            }
        }
    }

    [Fact]
    [UpstreamTest(Embed + "should expose the raw response", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_the_raw_embedding_response()
    {
        var handler = CohereParity.Handler(Body);
        handler.ResponseHeaders["test-header"] = "test-value";
        var model = (CohereEmbeddingModel)CohereParity.Provider(handler).EmbeddingModel("embed-english-v3.0");
        var result = await model.EmbedAsync(new CohereEmbedRequest(Values), CancellationToken.None);

        Assert.Equal(Body, result.RawBody);
        Assert.Equal("test-value", result.Headers["test-header"]);
        Assert.Contains("application/json", result.Headers["Content-Type"], StringComparison.Ordinal);
        Assert.Contains("f5aa3e7b-f011-4c5c-a825-f94669f760e5", result.RawBody, StringComparison.Ordinal);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Embed + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_billed_input_tokens()
    {
        var result = await EmbedAsync(new CohereEmbedRequest(Values));

        Assert.Equal(10, result.Tokens);
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_model_and_texts()
    {
        var handler = await Send(new CohereEmbedRequest(Values), "embed-english-v3.0");

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "embed-english-v3.0",
              "embedding_types": ["float"],
              "input_type": "search_query",
              "texts": ["sunny day at the beach", "rainy day in the city"]
            }
            """);
        Assert.Contains("https://api.cohere.com/v2/embed", handler.Uri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the input_type setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_input_type()
    {
        var handler = await Send(new CohereEmbedRequest(Values) { InputType = "search_document" }, "embed-english-v3.0");

        Assert.Equal("search_document", JsonNodeInputType(handler.Body));
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the output_dimension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_output_dimension()
    {
        var handler = await Send(new CohereEmbedRequest(Values) { OutputDimension = 256 }, "embed-v4.0");

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "embed-v4.0",
              "embedding_types": ["float"],
              "input_type": "search_query",
              "output_dimension": 256,
              "texts": ["sunny day at the beach", "rainy day in the city"]
            }
            """);
    }

    [Fact]
    [UpstreamTest(Embed + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_embedding_headers()
    {
        var handler = CohereParity.Handler(Body);
        var provider = CohereParity.Provider(handler, options =>
        {
            options.Headers = new Dictionary<string, string?>
            {
                ["Custom-Provider-Header"] = "provider-header-value",
            };
        });
        var model = (CohereEmbeddingModel)provider.EmbeddingModel("embed-english-v3.0");
        await model.EmbedAsync(
            new CohereEmbedRequest(Values)
            {
                Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/cohere/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }

    private static async Task<CohereEmbeddingResult> EmbedAsync(CohereEmbedRequest request)
    {
        var handler = CohereParity.Handler(Body);
        var model = (CohereEmbeddingModel)CohereParity.Provider(handler).EmbeddingModel("embed-english-v3.0");
        return await model.EmbedAsync(request, CancellationToken.None);
    }

    private static async Task<CaptureHandler> Send(CohereEmbedRequest request, string modelId)
    {
        var handler = CohereParity.Handler(Body);
        var model = (CohereEmbeddingModel)CohereParity.Provider(handler).EmbeddingModel(modelId);
        await model.EmbedAsync(request, CancellationToken.None);
        return handler;
    }

    private static string JsonNodeInputType(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("input_type").GetString()!;
    }
}
