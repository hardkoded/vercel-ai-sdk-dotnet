// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Cohere;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests;

/// <summary>Cohere v2 embed requests that select an embedding format.</summary>
public sealed class CohereEmbeddingParityTests
{
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
