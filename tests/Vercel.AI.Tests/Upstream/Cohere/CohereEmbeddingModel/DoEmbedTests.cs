// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Cohere;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereEmbeddingModel;

/// <summary>Port of <c>cohere-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/cohere/src/cohere-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new EmbeddingCapture("{\"embeddings\":{\"float\":[[0.1],[0.2]]},\"meta\":{\"billed_units\":{\"input_tokens\":10}}}");
        var provider = CohereProvider.Create(new CohereOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("embed-v4.0").DoEmbedAsync(Values, EmbeddingDimensionCases.Options("cohere", "outputDimension", providerDimensions), CancellationToken.None, dimensions);

        EmbeddingDimensionCases.AssertDimension(capture.Body["output_dimension"], expected);
    }
}
