// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaEmbeddingModel;

/// <summary>Port of <c>alibaba-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/alibaba/src/alibaba-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new EmbeddingCapture("{\"output\":{\"embeddings\":[{\"text_index\":0,\"embedding\":[0.1]},{\"text_index\":1,\"embedding\":[0.2]}]},\"usage\":{\"total_tokens\":12}}");
        var provider = AlibabaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("text-embedding-v4").DoEmbedAsync(Values, EmbeddingDimensionCases.Options("alibaba", "dimension", providerDimensions), CancellationToken.None, dimensions);

        EmbeddingDimensionCases.AssertDimension(capture.Body["parameters"]!["dimension"], expected);
    }
}
