// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class PerplexityUpstreamTests
{
    private const string Embedding = "packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::";
    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };
    private static readonly double[][] DummyEmbeddings =
    {
        new double[] { 1, -2, 3, 127, -128 },
        new double[] { -1, 2, -3, 100, -50 },
    };

    [Fact]
    [UpstreamTest(Embedding + "should decode base64 int8 embeddings into signed number vectors", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_decode_signed_int8_vectors()
    {
        var result = await Model(Capture()).DoEmbedAsync(Call(), CancellationToken.None);
        Assert.Equal(DummyEmbeddings, result.Embeddings);
    }

    [Fact]
    [UpstreamTest(Embedding + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_report_prompt_tokens()
    {
        var result = await Model(Capture(usage: "{\"prompt_tokens\":20,\"total_tokens\":20}")).DoEmbedAsync(Call(), CancellationToken.None);
        Assert.Equal(20, result.Tokens);
    }

    [Fact]
    [UpstreamTest(Embedding + "should surface the cost breakdown as provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_put_the_cost_in_provider_metadata()
    {
        var capture = Capture(usage: "{\"prompt_tokens\":8,\"total_tokens\":8,\"cost\":{\"input_cost\":0.0001,\"total_cost\":0.0001,\"currency\":\"USD\"}}");
        var result = await Model(capture).DoEmbedAsync(Call(), CancellationToken.None);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"perplexity\":{\"cost\":{\"inputCost\":0.0001,\"totalCost\":0.0001,\"currency\":\"USD\"}}}");
    }

    [Fact]
    [UpstreamTest(Embedding + "should omit provider metadata when no cost is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_omit_provider_metadata_without_a_cost()
    {
        var result = await Model(Capture()).DoEmbedAsync(Call(), CancellationToken.None);
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Embedding + "should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_expose_the_response_headers()
    {
        var capture = Capture();
        capture.ResponseHeaders["test-header"] = "test-value";
        var result = await Model(capture).DoEmbedAsync(Call(), CancellationToken.None);
        var headers = result.Response!.Headers!;
        Assert.Equal("test-value", headers["test-header"]);
        Assert.StartsWith("application/json", headers["content-type"], StringComparison.Ordinal);
        Assert.True(headers.ContainsKey("content-length"));
    }

    [Fact]
    [UpstreamTest(Embedding + "should pass the model, values, and default encoding format", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_send_the_model_values_and_int8_encoding()
    {
        var capture = Capture();
        await Model(capture).DoEmbedAsync(Call(), CancellationToken.None);
        Assert.Equal("https://api.perplexity.ai/v1/embeddings", capture.Requests[0].Uri!.AbsoluteUri);
        JsonAssert.Equal(UpstreamChat.Body(capture), "{\"model\":\"pplx-embed-v1-4b\",\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"encoding_format\":\"base64_int8\"}");
    }

    [Fact]
    [UpstreamTest(Embedding + "should pass dimensions and encoding format provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_send_dimensions_and_encoding_options()
    {
        var capture = Capture();
        await Model(capture).DoEmbedAsync(Call(providerOptions: "{\"perplexity\":{\"dimensions\":256,\"encodingFormat\":\"base64_binary\"}}"), CancellationToken.None);
        JsonAssert.Equal(UpstreamChat.Body(capture), "{\"model\":\"pplx-embed-v1-4b\",\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"dimensions\":256,\"encoding_format\":\"base64_binary\"}");
    }

    [Fact]
    [UpstreamTest(
        Embedding + "should pass headers",
        Coverage = UpstreamCoverage.Partial,
        Note = "Authorization, custom provider and request headers, and the ai-sdk/perplexity user agent match. The .NET provider also sends X-Pplx-Integration on every request.")]
    public async Task Embeddings_send_auth_custom_headers_and_user_agent()
    {
        var capture = Capture();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (PerplexityEmbeddingModel)PerplexityProvider.Create(options, capture).EmbeddingModel("pplx-embed-v1-4b");
        await model.DoEmbedAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var headers = capture.Requests[0].Headers;
        Assert.Equal("Bearer test-api-key", headers["Authorization"]);
        Assert.StartsWith("application/json", headers["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", headers["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/perplexity/" + AiSdkVersion.Version, headers["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Embedding + "should throw when exceeding the max embeddings per call", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_reject_more_than_512_values()
    {
        var values = new string[513];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = "value " + i;
        }

        var capture = Capture();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Model(capture).DoEmbedAsync(new EmbeddingModelCall(values, new Dictionary<string, string>(), null, CancellationToken.None), CancellationToken.None));
        Assert.StartsWith("Too many values", error.Message, StringComparison.Ordinal);
        Assert.Empty(capture.Requests);
    }

    private static PerplexityEmbeddingModel Model(UpstreamCapture capture)
    {
        return (PerplexityEmbeddingModel)PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).EmbeddingModel("pplx-embed-v1-4b");
    }

    private static UpstreamCapture Capture(string usage = "{\"prompt_tokens\":8,\"total_tokens\":8}")
    {
        var data = new List<string>();
        for (var i = 0; i < DummyEmbeddings.Length; i++)
        {
            var bytes = Array.ConvertAll(DummyEmbeddings[i], value => unchecked((byte)(sbyte)value));
            data.Add("{\"object\":\"embedding\",\"index\":" + i + ",\"embedding\":\"" + Convert.ToBase64String(bytes) + "\"}");
        }

        return new UpstreamCapture
        {
            ResponseBody = "{\"object\":\"list\",\"data\":[" + string.Join(",", data) + "],\"model\":\"pplx-embed-v1-4b\",\"usage\":" + usage + "}",
        };
    }

    private static EmbeddingModelCall Call(string providerOptions = "{}", IReadOnlyDictionary<string, string>? headers = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new EmbeddingModelCall(TestValues, headers ?? new Dictionary<string, string>(), document.RootElement.Clone(), CancellationToken.None);
    }
}
