// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.OpenAICompatible.OpenAICompatibleEmbeddingModel;

/// <summary>Port of <c>openai-compatible-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] Values = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/openai-compatible/src/embedding/openai-compatible-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new EmbeddingCapture("{\"data\":[{\"embedding\":[0.5]},{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":1}}");
        var provider = OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "openai-compatible", BaseUrl = "https://my.api.com/v1", ApiKey = "secret" }, capture);
        var model = provider.EmbeddingModel("text-embedding-3-large");
        ((Vercel.AI.OpenAICompatible.OpenAICompatibleEmbeddingModel)model).ProviderOptions = EmbeddingDimensionCases.Options("openaiCompatible", "dimensions", providerDimensions);

        await model.DoEmbedAsync(Values, null, CancellationToken.None, dimensions);

        EmbeddingDimensionCases.AssertDimension(capture.Body["dimensions"], expected);
    }
}
