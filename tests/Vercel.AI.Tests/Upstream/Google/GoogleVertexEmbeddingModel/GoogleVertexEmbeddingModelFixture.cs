// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Maps to the constants, <c>mockConfig</c> and <c>prepareJsonFixtureResponse</c> of <c>google-vertex-embedding-model.test.ts</c>.</summary>
internal static class GoogleVertexEmbeddingModelFixture
{
    internal static readonly string[] TestValues = ["test text one", "test text two"];

    internal const string ModelId = "textembedding-gecko@001";

    internal const string DefaultUrl =
        "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/us-central1/publishers/google/models/textembedding-gecko@001:predict";

    internal const string Gemini2Url =
        "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/us-central1/publishers/google/models/gemini-embedding-2:embedContent";

    internal const string CustomUrl =
        "https://custom-endpoint.com/models/textembedding-gecko@001:predict";

    /// <summary><c>src/__fixtures__/google-vertex-embedding.json</c>.</summary>
    internal const string Fixture =
        "{\"predictions\":[{\"embeddings\":{\"statistics\":{\"token_count\":5,\"truncated\":false},\"values\":[-0.017999587580561638,-0.006893285550177097,-0.036766719073057175,-0.017558680847287178,-0.019938766956329346]}},{\"embeddings\":{\"statistics\":{\"truncated\":false,\"token_count\":6},\"values\":[-0.06007182598114014,0.004907649010419846,-0.00690646655857563,-0.007314121350646019,-0.048464205116033554]}}],\"metadata\":{\"billableCharacterCount\":35}}";

    internal const string MockProviderOptions =
        "{\"outputDimensionality\":768,\"taskType\":\"SEMANTIC_SIMILARITY\",\"title\":\"test title\",\"autoTruncate\":false}";

    internal static readonly double[][] ExpectedEmbeddings =
    [
        [
            -0.017999587580561638,
            -0.006893285550177097,
            -0.036766719073057175,
            -0.017558680847287178,
            -0.019938766956329346,
        ],
        [
            -0.06007182598114014,
            0.004907649010419846,
            -0.00690646655857563,
            -0.007314121350646019,
            -0.048464205116033554,
        ],
    ];

    internal static RecordingHandler Server(string body = Fixture)
    {
        return new RecordingHandler { ResponseText = body };
    }

    internal static GoogleVertexProvider Provider(
        RecordingHandler handler,
        Action<VertexOptions>? configure = null
    )
    {
        var options = new VertexOptions
        {
            Project = "test-project",
            Region = "us-central1",
            UpstreamRoutes = true,
            GenerateAuthToken = _ => Task.FromResult("token"),
        };
        configure?.Invoke(options);
        return GoogleVertexProvider.Create(options, handler);
    }

    internal static IEmbeddingCaller Model(
        RecordingHandler handler,
        string modelId = ModelId,
        Action<VertexOptions>? configure = null
    )
    {
        return (IEmbeddingCaller)Provider(handler, configure).EmbeddingModel(modelId);
    }

    internal static EmbeddingModelCall Call(
        string[] values,
        string? providerOptions = null,
        int? dimensions = null,
        Dictionary<string, string>? headers = null
    )
    {
        return new EmbeddingModelCall(
            values,
            headers ?? new Dictionary<string, string>(),
            providerOptions is null ? null : JsonDocument.Parse(providerOptions).RootElement,
            CancellationToken.None,
            dimensions
        );
    }

    internal static string Options(string key, string json)
    {
        return "{\"" + key + "\":" + json + "}";
    }

    internal static JsonElement Body(RecordingHandler handler)
    {
        return JsonDocument.Parse(handler.Body).RootElement;
    }

    internal static void AssertEmbeddings(IReadOnlyList<double[]> actual)
    {
        Assert.Equal(ExpectedEmbeddings.Length, actual.Count);
        for (var i = 0; i < actual.Count; i++)
        {
            Assert.Equal(ExpectedEmbeddings[i], actual[i]);
        }
    }
}
