// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>Cohere v2 embed requests, options, and responses.</summary>
public sealed class CohereEmbeddingParityTests
{
    private const string Embed = "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed::";

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<string, string> EmbeddingTypes => new()
    {
        { "float", "[[0.25,-0.5],[0.75,0]]" },
        { "int8", "[[-128,127],[0,-1]]" },
        { "uint8", "[[0,255],[128,1]]" },
        { "binary", "[[-128,127],[0,-1]]" },
        { "ubinary", "[[0,255],[128,1]]" },
    };

    [Theory]
    [MemberData(nameof(EmbeddingTypes))]
    public async Task ShouldRequestAndReturnEmbeddingTypeEmbeddingsWithoutConversion(string embeddingType, string vectors)
    {
        var (model, handler) = Model("{\"embeddings\":{\"" + embeddingType + "\":" + vectors + ",\"base64\":[\"extra-format\"]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}");

        var result = await model.DoEmbedAsync(Values, Options("{\"embeddingType\":\"" + embeddingType + "\",\"inputType\":\"search_document\"}"), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(
            "{\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"],\"embedding_types\":[\"" + embeddingType + "\"],\"input_type\":\"search_document\"}",
            handler.Calls[0].Text);
        Assert.Equal("https://api.cohere.com/v2/embed", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(JsonSerializer.Deserialize<float[][]>(vectors), result.Embeddings);
        Assert.Equal(10, result.Tokens);
    }

    [Fact]
    public async Task ShouldReturnTheSelectedFormatWhenFloatEmbeddingsAreAlsoPresent()
    {
        var (model, _) = Model("{\"embeddings\":{\"float\":[[0.1],[0.2]],\"int8\":[[10],[20]]}}");

        var result = await model.DoEmbedAsync(Values, Options("{\"embeddingType\":\"int8\"}"), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(new[] { new[] { 10f }, new[] { 20f } }, result.Embeddings);
    }

    [Theory]
    [InlineData("float")]
    [InlineData("int8")]
    [InlineData("uint8")]
    [InlineData("binary")]
    [InlineData("ubinary")]
    public async Task ShouldRejectAResponseMissingTheRequestedEmbeddings(string embeddingType)
    {
        var (model, _) = Model(embeddingType == "float" ? "{\"embeddings\":{}}" : "{\"embeddings\":{\"float\":[[0.1]]}}");

        await Assert.ThrowsAsync<AiSdkException>(() => model.DoEmbedAsync(Values, Options("{\"embeddingType\":\"" + embeddingType + "\"}"), CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ShouldRejectNonNumericEmbeddingsInTheSelectedFormat()
    {
        var (model, _) = Model("{\"embeddings\":{\"int8\":[[\"invalid\"]]}}");

        await Assert.ThrowsAsync<AiSdkException>(() => model.DoEmbedAsync(Values, Options("{\"embeddingType\":\"int8\"}"), CancellationToken.None)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData("\"base64\"")]
    [InlineData("\"invalid\"")]
    [InlineData("[\"float\",\"int8\"]")]
    [InlineData("null")]
    public async Task ShouldRejectAnUnsupportedEmbeddingTypeBeforeSendingARequest(string embeddingType)
    {
        var (model, handler) = Model("{\"embeddings\":{\"float\":[[0.1]]}}");

        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoEmbedAsync(Values, Options("{\"embeddingType\":" + embeddingType + "}"), CancellationToken.None)).ConfigureAwait(false);

        Assert.Contains("invalid cohere provider options", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    public async Task Embed_many_passes_provider_options_to_the_model()
    {
        var (model, handler) = Model("{\"embeddings\":{\"int8\":[[1],[2]]}}");
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));

        var result = await client.EmbedManyAsync(new EmbedOptions { Model = model, Values = Values, ProviderOptions = Options("{\"embeddingType\":\"int8\"}") }).ConfigureAwait(false);

        Assert.Contains("\"embedding_types\":[\"int8\"]", handler.Calls[0].Text, StringComparison.Ordinal);
        Assert.Equal(new[] { new[] { 1f }, new[] { 2f } }, result.Embeddings);
    }

    [Fact]
    [UpstreamTest(Embed + "should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Fixture_embeddings_are_returned()
    {
        var (model, _) = FixtureModel();
        var result = await model.DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                new[] { 0.03302002f, 0.020904541f, -0.019744873f, -0.0625f, 0.04437256f },
                new[] { -0.04660034f, 0.00037765503f, -0.061157227f, -0.08239746f, -0.010360718f },
            },
            result.Embeddings);
    }

    [Fact]
    [UpstreamTest(Embed + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Billed_input_tokens_are_the_usage()
    {
        var (model, _) = FixtureModel();
        Assert.Equal(10, (await model.DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false)).Tokens);
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Request_defaults_to_search_query_floats()
    {
        var (model, handler) = FixtureModel();
        await model.DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            JsonNode.Parse(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_query\",\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}");
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the input_type setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Input_type_option_is_sent()
    {
        var (model, handler) = FixtureModel();
        await model.DoEmbedAsync(Values, Options("{\"inputType\":\"search_document\"}"), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            JsonNode.Parse(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_document\",\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}");
    }

    [Fact]
    [UpstreamTest(Embed + "should pass the output_dimension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Output_dimension_option_is_sent()
    {
        var (model, handler) = FixtureModel("embed-v4.0");
        await model.DoEmbedAsync(Values, Options("{\"outputDimension\":256}"), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            JsonNode.Parse(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_query\",\"model\":\"embed-v4.0\",\"output_dimension\":256,\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}");
    }

    [Fact]
    [UpstreamTest(Embed + "should pass headers", Coverage = UpstreamCoverage.Partial, Note = "Provider headers only. DoEmbedAsync takes no call headers, and no SDK user agent is sent.")]
    public async Task Provider_headers_are_sent()
    {
        var (model, handler) = FixtureModel(configure: options => options.Headers["Custom-Provider-Header"] = "provider-header-value");
        await model.DoEmbedAsync(Values, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Bearer test-api-key", handler.Calls[0].Header("Authorization"));
        Assert.StartsWith("application/json", handler.Calls[0].Header("Content-Type"), StringComparison.Ordinal);
        Assert.Equal("provider-header-value", handler.Calls[0].Header("Custom-Provider-Header"));
    }

    private static (IEmbeddingModel Model, ParityHandler Handler) FixtureModel(string modelId = "embed-english-v3.0", Action<CohereOptions>? configure = null)
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "cohere-embedding.json"));
        var handler = new ParityHandler(_ => ParityHandler.Json(json));
        var options = new CohereOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return (CohereProvider.Create(options, handler).EmbeddingModel(modelId), handler);
    }

    private static (IEmbeddingModel Model, ParityHandler Handler) Model(string response)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(response));
        return (CohereProvider.Create(new CohereOptions { ApiKey = "test-api-key" }, handler).EmbeddingModel("embed-english-v3.0"), handler);
    }

    private static Dictionary<string, JsonElement> Options(string cohere)
    {
        return new Dictionary<string, JsonElement> { ["cohere"] = JsonSerializer.Deserialize<JsonElement>(cohere) };
    }
}
