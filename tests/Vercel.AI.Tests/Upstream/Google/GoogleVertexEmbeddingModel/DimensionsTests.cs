// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel &gt; embedding &gt; %s dimensions</c>.</summary>
public sealed class DimensionsTests
{
    public static TheoryData<string, int?, int?, int?> Cases
    {
        get
        {
            var data = new TheoryData<string, int?, int?, int?>();
            foreach (var endpoint in new[] { "predict", "embedContent" })
            {
                foreach (var row in new (int?, int?, int?)[] { (null, null, null), (256, null, 256), (null, 512, 512), (256, 512, 512) })
                {
                    data.Add(endpoint, row.Item1, row.Item2, row.Item3);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding > %s dimensions::maps $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(string endpoint, int? dimensions, int? providerDimensions, int? expected)
    {
        var handler = new RecordingHandler
        {
            ResponseText = endpoint == "predict" ? "{\"predictions\":[{\"embeddings\":{\"values\":[0.1,0.2,0.3]}}]}" : "{\"embedding\":{\"values\":[0.1,0.2,0.3]}}",
        };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "k" }, handler);
        var model = provider.EmbeddingModel(endpoint == "predict" ? "textembedding-gecko@001" : "gemini-embedding-2");

        await model.DoEmbedAsync(
            new[] { "test text one" },
            EmbeddingDimensionCases.Options("googleVertex", "outputDimensionality", providerDimensions),
            CancellationToken.None,
            dimensions);

        var body = JsonNode.Parse(handler.Body)!;
        var config = endpoint == "predict" ? body["parameters"] : body["embedContentConfig"];
        EmbeddingDimensionCases.AssertDimension(config?["outputDimensionality"], expected);
    }
}
