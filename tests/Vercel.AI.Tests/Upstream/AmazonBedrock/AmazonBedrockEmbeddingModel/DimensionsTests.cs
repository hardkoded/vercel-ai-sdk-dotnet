// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockEmbeddingModel;

/// <summary>Port of <c>amazon-bedrock-embedding-model.test.ts</c> &gt; <c>doEmbed &gt; %s dimensions</c>.</summary>
public sealed class DimensionsTests
{
    private const string TitanResponse = "{\"embedding\":[-0.09,0.05,-0.02,0.01,0.04],\"inputTextTokenCount\":8}";

    public static TheoryData<string, int?, int?, int?> Cases
    {
        get
        {
            var data = new TheoryData<string, int?, int?, int?>();
            foreach (var family in new[] { "titan", "cohere" })
            {
                foreach (var row in new (int?, int?, int?)[] { (null, null, null), (256, null, 256), (null, 512, 512), (256, 512, 512) })
                {
                    data.Add(family, row.Item1, row.Item2, row.Item3);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-embedding-model.test.ts::doEmbed > %s dimensions::maps $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(string family, int? dimensions, int? providerDimensions, int? expected)
    {
        var handler = new UpstreamRecordingHandler(family == "titan" ? TitanResponse : "{\"embeddings\":{\"float\":[[-0.09,0.05]]}}");
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" }, handler);
        var model = provider.EmbeddingModel(family == "titan" ? "amazon.titan-embed-text-v2:0" : "cohere.embed-v4:0");

        await model.DoEmbedAsync(
            new[] { "sunny day at the beach" },
            EmbeddingDimensionCases.Options("amazonBedrock", family == "titan" ? "dimensions" : "outputDimension", providerDimensions),
            CancellationToken.None,
            dimensions);

        var body = JsonNode.Parse(handler.Body)!;
        EmbeddingDimensionCases.AssertDimension(family == "titan" ? body["dimensions"] : body["output_dimension"], expected);
    }
}
