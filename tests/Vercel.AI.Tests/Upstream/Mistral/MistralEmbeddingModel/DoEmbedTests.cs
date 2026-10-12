// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralEmbeddingModel;

/// <summary>Port of <c>mistral-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };

    private const string Response = "{\"id\":\"b322cfc2b9d34e2f8e14fc99874faee5\",\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"embedding\":[0.1,0.2,0.3,0.4,0.5],\"index\":0},{\"object\":\"embedding\",\"embedding\":[0.6,0.7,0.8,0.9,1],\"index\":1}],\"model\":\"mistral-embed\",\"usage\":{\"prompt_tokens\":8,\"total_tokens\":8}}";

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("codestral-embed").DoEmbedAsync(TestValues, EmbeddingDimensionCases.Options("mistral", "outputDimension", providerDimensions), CancellationToken.None, dimensions);

        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        EmbeddingDimensionCases.AssertDimension(body["output_dimension"], expected);
        Assert.Null(body["dimensions"]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_embedding()
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        var result = await provider.EmbeddingModel("mistral-embed").DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f }, result.Embeddings[0]);
        Assert.Equal(new[] { 0.6f, 0.7f, 0.8f, 0.9f, 1.0f }, result.Embeddings[1]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var capture = new UpstreamCapture { ResponseBody = Response.Replace("\"prompt_tokens\":8,\"total_tokens\":8", "\"prompt_tokens\":20,\"total_tokens\":20") };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        var result = await provider.EmbeddingModel("mistral-embed").DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(20, result.Tokens);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should expose the raw response", Coverage = UpstreamCoverage.Covered, Note = "The test handler's StringContent adds a utf-8 charset to content-type. The snapshot body is compared as JSON.")]
    public async Task Should_expose_the_raw_response()
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        capture.ResponseHeaders["test-header"] = "test-value";
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);
        var model = (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel("mistral-embed");

        await model.DoEmbedAsync(TestValues, null, CancellationToken.None);

        var headers = new Dictionary<string, string>(model.LastResponseHeaders, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(3, headers.Count);
        Assert.Equal("267", headers["content-length"]);
        Assert.Equal("application/json; charset=utf-8", headers["content-type"]);
        Assert.Equal("test-value", headers["test-header"]);
        JsonAssert.Equal(JsonNode.Parse(model.LastResponseBody!), Response);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_model_and_the_values()
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("mistral-embed").DoEmbedAsync(TestValues, null, CancellationToken.None);

        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"model\":\"mistral-embed\",\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"encoding_format\":\"float\"}");
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = MistralProvider.Create(options, capture);
        var model = (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel("mistral-embed");
        model.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };

        await model.DoEmbedAsync(TestValues, null, CancellationToken.None);

        var headers = capture.Requests[0].Headers;
        Assert.Equal("Bearer test-api-key", headers["Authorization"]);
        Assert.StartsWith("application/json", headers["Content-Type"]);
        Assert.Equal("provider-header-value", headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", headers["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/mistral/" + AiSdkVersion.Version, headers["User-Agent"]);
    }
}
