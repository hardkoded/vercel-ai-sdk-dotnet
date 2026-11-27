// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereEmbeddingModel;

/// <summary>Port of <c>cohere-embedding-model.test.ts</c> &gt; <c>doEmbed</c> &gt; <c>embeddingType</c>.</summary>
public sealed class EmbeddingTypeTests
{
    private const string Suite =
        "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed > embeddingType::";

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    private static IEmbeddingModel Model(ParityHandler handler) =>
        CohereProvider
            .Create(new CohereOptions { ApiKey = "test-api-key" }, handler)
            .EmbeddingModel("embed-english-v3.0");

    private static Task<EmbeddingResult> Embed(ParityHandler handler, string optionsJson) =>
        Model(handler)
            .DoEmbedAsync(
                Values,
                new Dictionary<string, JsonElement> { ["cohere"] = Json(optionsJson) },
                CancellationToken.None
            );

    private static ParityHandler Respond(string body) => new(_ => ParityHandler.Json(body));

    [Theory]
    [InlineData("float", "[[0.25,-0.5],[0.75,0]]")]
    [InlineData("int8", "[[-128,127],[0,-1]]")]
    [InlineData("uint8", "[[0,255],[128,1]]")]
    [InlineData("binary", "[[-128,127],[0,-1]]")]
    [InlineData("ubinary", "[[0,255],[128,1]]")]
    [UpstreamTest(
        Suite + "should request and return $embeddingType embeddings without conversion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_request_and_return_embeddings_without_conversion(
        string embeddingType,
        string embeddings
    )
    {
        var responseBody =
            "{\"embeddings\":{\""
            + embeddingType
            + "\":"
            + embeddings
            + ",\"base64\":[\"extra-format\"]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}";
        var handler = Respond(responseBody);

        var result = await Embed(
            handler,
            "{\"embeddingType\":\"" + embeddingType + "\",\"inputType\":\"search_document\"}"
        );

        JsonAssert.Equal(
            Json(handler.Calls[0].Text),
            "{\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"],\"embedding_types\":[\""
                + embeddingType
                + "\"],\"input_type\":\"search_document\"}"
        );
        Assert.Equal(JsonSerializer.Deserialize<float[][]>(embeddings)!, result.Embeddings);
        Assert.Equal(10, result.Tokens);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(Json(result.Response!.Body!), responseBody);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should return the selected format when float embeddings are also present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_the_selected_format_when_float_embeddings_are_also_present()
    {
        var handler = Respond(
            "{\"embeddings\":{\"float\":[[0.1],[0.2]],\"int8\":[[10],[20]]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}"
        );

        var result = await Embed(handler, "{\"embeddingType\":\"int8\"}");

        Assert.Equal(new[] { new[] { 10f }, new[] { 20f } }, result.Embeddings);
    }

    [Theory]
    [InlineData("float")]
    [InlineData("int8")]
    [InlineData("uint8")]
    [InlineData("binary")]
    [InlineData("ubinary")]
    [UpstreamTest(
        Suite + "should reject a response missing the requested %s embeddings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_a_response_missing_the_requested_embeddings(
        string embeddingType
    )
    {
        var handler = Respond(
            "{\"embeddings\":"
                + (embeddingType == "float" ? "{}" : "{\"float\":[[0.1]]}")
                + ",\"meta\":{\"billed_units\":{\"input_tokens\":10}}}"
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Embed(handler, "{\"embeddingType\":\"" + embeddingType + "\"}")
        );

        Assert.Contains("Invalid JSON response", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should reject non-numeric embeddings in the selected format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_non_numeric_embeddings_in_the_selected_format()
    {
        var handler = Respond(
            "{\"embeddings\":{\"int8\":[[\"invalid\"]]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}"
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Embed(handler, "{\"embeddingType\":\"int8\"}")
        );

        Assert.Contains("Invalid JSON response", error.Message);
    }

    [Theory]
    [InlineData("\"base64\"")]
    [InlineData("\"invalid\"")]
    [InlineData("[\"float\",\"int8\"]")]
    [InlineData("null")]
    [UpstreamTest(
        Suite + "should reject an unsupported embeddingType %j before sending a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_an_unsupported_embeddingType_before_sending_a_request(
        string embeddingType
    )
    {
        var handler = Respond("{}");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Embed(handler, "{\"embeddingType\":" + embeddingType + "}")
        );

        Assert.Contains("invalid cohere provider options", error.Message);
        Assert.Empty(handler.Calls);
    }
}
