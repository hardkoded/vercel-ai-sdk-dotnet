// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;

namespace Vercel.AI.Tests.Upstream.Perplexity.PerplexityEmbeddingModel;

/// <summary>Port of <c>perplexity-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/perplexity/src/perplexity-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var vector = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var capture = new EmbeddingCapture("{\"object\":\"list\",\"data\":[{\"index\":0,\"embedding\":\"" + vector + "\"},{\"index\":1,\"embedding\":\"" + vector + "\"}],\"usage\":{\"prompt_tokens\":8,\"total_tokens\":8}}");
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("pplx-embed-v1-4b").DoEmbedAsync(Values, EmbeddingDimensionCases.Options("perplexity", "dimensions", providerDimensions), CancellationToken.None, dimensions);

        EmbeddingDimensionCases.AssertDimension(capture.Body["dimensions"], expected);
    }
}
