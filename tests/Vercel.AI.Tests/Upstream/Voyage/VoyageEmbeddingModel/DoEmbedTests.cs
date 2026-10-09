// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Voyage;

namespace Vercel.AI.Tests.Upstream.Voyage.VoyageEmbeddingModel;

/// <summary>Port of <c>voyage-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/voyage/src/voyage-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new EmbeddingCapture("{\"data\":[{\"embedding\":[0.1],\"index\":0},{\"embedding\":[0.2],\"index\":1}],\"usage\":{\"total_tokens\":12}}");
        var provider = VoyageProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("voyage-3.5").DoEmbedAsync(Values, EmbeddingDimensionCases.Options("voyage", "outputDimension", providerDimensions), CancellationToken.None, dimensions);

        EmbeddingDimensionCases.AssertDimension(capture.Body["output_dimension"], expected);
    }
}
