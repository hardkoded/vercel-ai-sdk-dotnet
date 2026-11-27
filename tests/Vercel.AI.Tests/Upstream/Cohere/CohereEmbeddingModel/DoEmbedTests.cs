// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereEmbeddingModel;

/// <summary>Port of <c>cohere-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_dimensions_with_provider_override(
        int? dimensions,
        int? providerDimensions,
        int? expected
    )
    {
        var capture = new EmbeddingCapture(
            "{\"embeddings\":{\"float\":[[0.1],[0.2]]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}"
        );
        var provider = CohereProvider.Create(
            new CohereOptions { ApiKey = "test-api-key" },
            capture
        );

        await provider
            .EmbeddingModel("embed-v4.0")
            .DoEmbedAsync(
                Values,
                EmbeddingDimensionCases.Options("cohere", "outputDimension", providerDimensions),
                CancellationToken.None,
                dimensions
            );

        EmbeddingDimensionCases.AssertDimension(capture.Body["output_dimension"], expected);
    }

    private const string Suite = "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed::";

    private static IEmbeddingModel Model(
        ParityHandler handler,
        string modelId = "embed-english-v3.0"
    )
    {
        return CohereProvider
            .Create(new CohereOptions { ApiKey = "test-api-key" }, handler)
            .EmbeddingModel(modelId);
    }

    private static ParityHandler Fixture() =>
        new(_ => ParityHandler.Json(CohereTestSupport.Fixture("cohere-embedding.json")));

    [Fact]
    [UpstreamTest(Suite + "should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_embedding()
    {
        var result = await Model(Fixture()).DoEmbedAsync(Values, null, CancellationToken.None);

        Assert.Equal(
            new[]
            {
                new[] { 0.03302002f, 0.020904541f, -0.019744873f, -0.0625f, 0.04437256f },
                new[] { -0.04660034f, 0.00037765503f, -0.061157227f, -0.08239746f, -0.010360718f },
            },
            result.Embeddings
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should expose the raw response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_raw_response()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                CohereTestSupport.Fixture("cohere-embedding.json"),
                ("test-header", "test-value")
            )
        );

        var result = await Model(handler).DoEmbedAsync(Values, null, CancellationToken.None);

        Assert.Equal("test-value", result.Response!.Headers["test-header"]);
        Assert.Equal("application/json", result.Response.Headers["content-type"]);
        JsonAssert.Equal(
            CohereTestSupport.Json(result.Response.Body!),
            CohereTestSupport.Fixture("cohere-embedding.json")
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var result = await Model(Fixture()).DoEmbedAsync(Values, null, CancellationToken.None);

        Assert.Equal(10, result.Tokens);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass the model and the values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_model_and_the_values()
    {
        var handler = Fixture();

        await Model(handler).DoEmbedAsync(Values, null, CancellationToken.None);

        JsonAssert.Equal(
            CohereTestSupport.Json(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_query\",\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass the input_type setting",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_input_type_setting()
    {
        var handler = Fixture();

        await Model(handler)
            .DoEmbedAsync(
                Values,
                new Dictionary<string, JsonElement>
                {
                    ["cohere"] = CohereTestSupport.Json("{\"inputType\":\"search_document\"}"),
                },
                CancellationToken.None
            );

        JsonAssert.Equal(
            CohereTestSupport.Json(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_document\",\"model\":\"embed-english-v3.0\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass the output_dimension setting",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_output_dimension_setting()
    {
        var handler = Fixture();

        await Model(handler, "embed-v4.0")
            .DoEmbedAsync(
                Values,
                new Dictionary<string, JsonElement>
                {
                    ["cohere"] = CohereTestSupport.Json("{\"outputDimension\":256}"),
                },
                CancellationToken.None
            );

        JsonAssert.Equal(
            CohereTestSupport.Json(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_query\",\"model\":\"embed-v4.0\",\"output_dimension\":256,\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}"
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = Fixture();
        var options = new CohereOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";

        await CohereProvider
            .Create(options, handler)
            .EmbeddingModel("embed-english-v3.0")
            .DoEmbedAsync(Values, null, CancellationToken.None, null, RequestHeaders());

        AssertTestHeaders(handler.Calls[0]);
    }
}
