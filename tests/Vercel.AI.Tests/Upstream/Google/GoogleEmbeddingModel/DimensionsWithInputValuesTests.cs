// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleEmbeddingModel;

/// <summary>Port of <c>google-embedding-model.test.ts</c> &gt; <c>GoogleEmbeddingModel &gt; dimensions with %i input values</c>.</summary>
public sealed class DimensionsWithInputValuesTests
{
    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };

    public static TheoryData<int, int?, int?, int?> Cases
    {
        get
        {
            var data = new TheoryData<int, int?, int?, int?>();
            foreach (var count in new[] { 1, 2 })
            {
                foreach (var row in new (int?, int?, int?)[] { (null, null, null), (256, null, 256), (null, 512, 512), (256, 512, 512) })
                {
                    data.Add(count, row.Item1, row.Item2, row.Item3);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel > dimensions with %i input values::maps $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int count, int? dimensions, int? providerDimensions, int? expected)
    {
        var values = TestValues.Take(count).ToArray();
        var response = count == 1 ? "{\"embedding\":{\"values\":[0.1,0.2]}}" : "{\"embeddings\":[{\"values\":[0.1]},{\"values\":[0.2]}]}";

        var handler = new RecordingHandler { ResponseText = response };
        var options = new GoogleOptions { ApiKey = "test-api-key" };
        if (providerDimensions != null)
        {
            options.EmbeddingProviderOptions = JsonSerializer.Deserialize<JsonElement>("{\"outputDimensionality\":" + providerDimensions + "}");
        }

        await GoogleProvider.Create(options, handler).EmbeddingModel("gemini-embedding-001").DoEmbedAsync(values, null, CancellationToken.None, dimensions);
        AssertBody(JsonNode.Parse(handler.Body)!, count, expected);

        // The same call through the caller that embed and embedMany use, with the provider override on the call.
        var callHandler = new RecordingHandler { ResponseText = response };
        var caller = new GoogleMultimodalEmbedding("gemini-embedding-001", new HttpClient(callHandler), "https://generativelanguage.googleapis.com/v1beta");
        var providerOptions = JsonSerializer.Deserialize<JsonElement>(providerDimensions == null ? "{\"google\":{}}" : "{\"google\":{\"outputDimensionality\":" + providerDimensions + "}}");
        await caller.DoEmbedAsync(new EmbeddingModelCall(values, new Dictionary<string, string>(), providerOptions, CancellationToken.None, dimensions), CancellationToken.None);
        AssertBody(JsonNode.Parse(callHandler.Body)!, count, expected);
    }

    private static void AssertBody(JsonNode body, int count, int? expected)
    {
        if (count == 1)
        {
            EmbeddingDimensionCases.AssertDimension(body["outputDimensionality"], expected);
            return;
        }

        var requests = body["requests"]!.AsArray();
        Assert.Equal(count, requests.Count);
        foreach (var request in requests)
        {
            EmbeddingDimensionCases.AssertDimension(request!["outputDimensionality"], expected);
        }
    }
}
