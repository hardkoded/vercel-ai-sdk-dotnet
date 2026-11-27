// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereEmbeddingModel;

/// <summary>Port of <c>cohere-embedding-model.test.ts</c> &gt; <c>doEmbed</c> &gt; <c>%s output dimensions</c> for <c>embed-v5.0-pro</c> and <c>embed-v5.0-fast</c>.</summary>
public sealed class OutputDimensionsTests
{
    private const string Suite =
        "packages/cohere/src/cohere-embedding-model.test.ts::doEmbed > %s output dimensions::";

    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<string, int> Dimensions
    {
        get
        {
            var data = new TheoryData<string, int>();
            foreach (var model in new[] { "embed-v5.0-pro", "embed-v5.0-fast" })
            {
                foreach (var dimension in new[] { 256, 512, 768, 1024, 1536, 2048 })
                {
                    data.Add(model, dimension);
                }
            }

            return data;
        }
    }

    public static TheoryData<string> Models => new() { "embed-v5.0-pro", "embed-v5.0-fast" };

    private static ParityHandler Handler() =>
        new(_ => ParityHandler.Json(Fixture("cohere-embedding.json")));

    private static IEmbeddingModel Model(ParityHandler handler, string modelId) =>
        CohereProvider
            .Create(new CohereOptions { ApiKey = "test-api-key" }, handler)
            .EmbeddingModel(modelId);

    [Theory]
    [MemberData(nameof(Dimensions))]
    [UpstreamTest(Suite + "should pass output_dimension %i", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_output_dimension(string modelId, int outputDimension)
    {
        var handler = Handler();

        await Model(handler, modelId)
            .DoEmbedAsync(
                Values,
                new Dictionary<string, JsonElement>
                {
                    ["cohere"] = Json("{\"outputDimension\":" + outputDimension + "}"),
                },
                CancellationToken.None
            );

        JsonAssert.Equal(
            Json(handler.Calls[0].Text),
            "{\"embedding_types\":[\"float\"],\"input_type\":\"search_query\",\"model\":\""
                + modelId
                + "\",\"output_dimension\":"
                + outputDimension
                + ",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}"
        );
    }

    [Theory]
    [MemberData(nameof(Models))]
    [UpstreamTest(
        Suite + "should leave the default output dimension to the API",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_leave_the_default_output_dimension_to_the_API(string modelId)
    {
        var handler = Handler();

        await Model(handler, modelId).DoEmbedAsync(Values, null, CancellationToken.None);

        Assert.False(Json(handler.Calls[0].Text).TryGetProperty("output_dimension", out _));
    }

    [Theory]
    [MemberData(nameof(Models))]
    [UpstreamTest(
        Suite + "should reject an unsupported output dimension before sending a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_an_unsupported_output_dimension_before_sending_a_request(
        string modelId
    )
    {
        var handler = Handler();

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Model(handler, modelId)
                .DoEmbedAsync(
                    Values,
                    new Dictionary<string, JsonElement>
                    {
                        ["cohere"] = Json("{\"outputDimension\":1000}"),
                    },
                    CancellationToken.None
                )
        );

        Assert.Contains("invalid cohere provider options", error.Message);
        Assert.Empty(handler.Calls);
    }
}
