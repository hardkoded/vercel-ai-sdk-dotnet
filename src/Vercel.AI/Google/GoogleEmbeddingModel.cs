// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini embedding model. One value uses <c>:embedContent</c>; several values use <c>:batchEmbedContents</c>.</summary>
public sealed class GoogleEmbeddingModel : IEmbeddingModel
{
    /// <summary>Maximum values in one Generative Language batch.</summary>
    public const int MaxEmbeddingsPerCall = 100;

    private readonly GoogleProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public GoogleEmbeddingModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "V4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        var inputs = values ?? Array.Empty<string>();
        if (inputs.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException(
                "Too many embedding values for one call. The limit is " + MaxEmbeddingsPerCall + "."
            );
        }

        if (inputs.Count <= 1)
        {
            var value = inputs.Count == 0 ? string.Empty : inputs[0];
            var body = new JsonObject
            {
                ["model"] = "models/" + ModelId,
                ["content"] = new JsonObject
                {
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = value } },
                },
            };
            ApplyEmbeddingOptions(body, _provider.Options, dimensions);
            using var document = await PostAsync(":embedContent", body, headers, cancellationToken)
                .ConfigureAwait(false);
            return new EmbeddingResult(
                new[]
                {
                    ReadVector(document.RootElement.GetProperty("embedding").GetProperty("values")),
                },
                null
            );
        }

        var requests = new JsonArray();
        foreach (var value in inputs)
        {
            var request = new JsonObject
            {
                ["model"] = "models/" + ModelId,
                ["content"] = new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = value } },
                },
            };
            ApplyEmbeddingOptions(request, _provider.Options, dimensions);
            requests.Add(request);
        }

        using var batch = await PostAsync(
                ":batchEmbedContents",
                new JsonObject { ["requests"] = requests },
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var embedding in batch.RootElement.GetProperty("embeddings").EnumerateArray())
        {
            vectors.Add(ReadVector(embedding.GetProperty("values")));
        }

        return new EmbeddingResult(vectors, null);
    }

    private async Task<JsonDocument> PostAsync(
        string method,
        JsonObject body,
        IReadOnlyDictionary<string, string?>? callHeaders,
        CancellationToken cancellationToken
    )
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return await _provider
            .Http.SendJsonAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + method),
                GoogleJson.Write(body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private static void ApplyEmbeddingOptions(
        JsonObject body,
        GoogleOptions options,
        int? dimensions
    )
    {
        var google = EmbeddingOptions(options);
        if (
            google != null
            && google.Value.TryGetProperty("outputDimensionality", out var providerDimensions)
            && providerDimensions.TryGetInt32(out var count)
        )
        {
            body["outputDimensionality"] = count;
        }
        else if (dimensions is { } width)
        {
            body["outputDimensionality"] = width;
        }

        if (google == null)
        {
            return;
        }

        var task = GoogleJson.String(google.Value, "taskType");
        GoogleJson.Set(body, "taskType", task);
    }

    private static JsonElement? EmbeddingOptions(GoogleOptions options)
    {
        return options.EmbeddingProviderOptions;
    }

    private static float[] ReadVector(JsonElement values)
    {
        var vector = new float[values.GetArrayLength()];
        var index = 0;
        foreach (var number in values.EnumerateArray())
        {
            vector[index++] = number.GetSingle();
        }

        return vector;
    }
}
