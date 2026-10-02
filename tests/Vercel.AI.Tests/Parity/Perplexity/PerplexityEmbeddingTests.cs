// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;

namespace Vercel.AI.Tests;

public sealed class PerplexityEmbeddingTests
{
    private static readonly int[][] Vectors =
    {
        new[] { 1, -2, 3, 127, -128 },
        new[] { -1, 2, -3, 100, -50 },
    };

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should decode base64 int8 embeddings into signed number vectors",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Decodes_base64_int8_embeddings()
    {
        var result = await Embed(Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"));

        Assert.Equal(2, result.Embeddings.Count);
        AssertVector(Vectors[0], result.Embeddings[0]);
        AssertVector(Vectors[1], result.Embeddings[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should extract usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_prompt_tokens()
    {
        var result = await Embed(Response(Vectors, "{\"prompt_tokens\":20,\"total_tokens\":20}"));

        Assert.Equal(20, result.Tokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should surface the cost breakdown as provider metadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_cost_metadata()
    {
        var usage = "{\"prompt_tokens\":8,\"total_tokens\":8,\"cost\":{\"input_cost\":0.0001,\"total_cost\":0.0001,\"currency\":\"USD\"}}";
        var result = await Embed(Response(Vectors, usage));
        var cost = result.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("cost");

        Assert.Equal(0.0001, cost.GetProperty("inputCost").GetDouble(), 8);
        Assert.Equal(0.0001, cost.GetProperty("totalCost").GetDouble(), 8);
        Assert.Equal("USD", cost.GetProperty("currency").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should omit provider metadata when no cost is returned",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_metadata_without_cost()
    {
        var result = await Embed(Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"));

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_response_headers()
    {
        var handler = new PerplexityScriptedHandler
        {
            ResponseBody = Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"),
            ResponseHeaders = new Dictionary<string, string> { ["test-header"] = "test-value" },
        };
        var result = await Model(handler).EmbedAsync(new PerplexityEmbedRequest(Values), CancellationToken.None);

        Assert.Equal("test-value", Header(result.ResponseHeaders, "test-header"));
        Assert.Contains("application/json", Header(result.ResponseHeaders, "content-type"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should pass the model, values, and default encoding format",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_model_input_and_default_encoding()
    {
        var handler = new PerplexityScriptedHandler
        {
            ResponseBody = Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"),
        };
        await Model(handler).EmbedAsync(new PerplexityEmbedRequest(Values), CancellationToken.None);
        using var document = JsonDocument.Parse(handler.Body);
        var body = document.RootElement;

        Assert.Equal("https://api.perplexity.ai/v1/embeddings", handler.Uri);
        Assert.Equal("pplx-embed-v1-4b", body.GetProperty("model").GetString());
        Assert.Equal(Values[0], body.GetProperty("input")[0].GetString());
        Assert.Equal(Values[1], body.GetProperty("input")[1].GetString());
        Assert.Equal("base64_int8", body.GetProperty("encoding_format").GetString());
        Assert.False(body.TryGetProperty("dimensions", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should pass dimensions and encoding format provider options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_dimensions_and_encoding_format()
    {
        var handler = new PerplexityScriptedHandler
        {
            ResponseBody = Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"),
        };
        await Model(handler).EmbedAsync(new PerplexityEmbedRequest(Values)
        {
            Dimensions = 256,
            EncodingFormat = "base64_binary",
        }, CancellationToken.None);
        using var document = JsonDocument.Parse(handler.Body);
        var body = document.RootElement;

        Assert.Equal(256, body.GetProperty("dimensions").GetInt32());
        Assert.Equal("base64_binary", body.GetProperty("encoding_format").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = new PerplexityScriptedHandler
        {
            ResponseBody = Response(Vectors, "{\"prompt_tokens\":8,\"total_tokens\":8}"),
        };
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "test-api-key",
            Headers = { ["Custom-Provider-Header"] = "provider-header-value" },
        }, handler);
        var model = (PerplexityEmbeddingModel)provider.EmbeddingModel("pplx-embed-v1-4b");
        await model.EmbedAsync(new PerplexityEmbedRequest(Values)
        {
            Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
        }, CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.Contains("application/json", handler.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(PerplexityEmbeddingModel.UserAgent, handler.Headers["User-Agent"]);
        Assert.Contains(PerplexityEmbeddingModel.UserAgentPrefix, handler.Headers["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::should throw when exceeding the max embeddings per call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_more_than_512_inputs()
    {
        var handler = new PerplexityScriptedHandler();
        var values = new string[513];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = "value " + index.ToString();
        }

        var exception = await Assert.ThrowsAsync<Vercel.AI.Provider.AiSdkException>(() => Model(handler).EmbedAsync(new PerplexityEmbedRequest(values), CancellationToken.None));

        Assert.Contains("Too many values", exception.Message);
        Assert.Equal(0, handler.Calls);
    }

    private static async Task<PerplexityEmbedResult> Embed(string body)
    {
        var handler = new PerplexityScriptedHandler { ResponseBody = body };
        return await Model(handler).EmbedAsync(new PerplexityEmbedRequest(Values), CancellationToken.None);
    }

    private static PerplexityEmbeddingModel Model(PerplexityScriptedHandler handler)
    {
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        return (PerplexityEmbeddingModel)provider.EmbeddingModel("pplx-embed-v1-4b");
    }

    private static string Response(int[][] vectors, string usage)
    {
        var body = new StringBuilder();
        body.Append("{\"object\":\"list\",\"data\":[");
        for (var index = 0; index < vectors.Length; index++)
        {
            if (index > 0)
            {
                body.Append(',');
            }

            body.Append("{\"object\":\"embedding\",\"index\":").Append(index).Append(",\"embedding\":\"").Append(Encode(vectors[index])).Append("\"}");
        }

        body.Append("],\"model\":\"pplx-embed-v1-4b\",\"usage\":").Append(usage).Append('}');
        return body.ToString();
    }

    private static string Encode(int[] values)
    {
        var bytes = new byte[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            bytes[index] = unchecked((byte)values[index]);
        }

        return Convert.ToBase64String(bytes);
    }

    private static void AssertVector(int[] expected, float[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], actual[index], 0);
        }
    }

    private static string Header(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        throw new InvalidOperationException("Missing header " + name);
    }
}
