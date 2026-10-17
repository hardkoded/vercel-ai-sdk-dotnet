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
        get
        {
            return _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.embedding"
                : _provider.ModelProvider;
        }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null, IReadOnlyDictionary<string, string?>? headers = null)
    {
        var inputs = values ?? Array.Empty<string>();
        if (_provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return await EmbedVertexAsync(inputs, providerOptions, dimensions, headers, cancellationToken).ConfigureAwait(false);
        }

        if (inputs.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException("Too many embedding values for one call. The limit is " + MaxEmbeddingsPerCall + ".");
        }

        if (inputs.Count <= 1)
        {
            var value = inputs.Count == 0 ? string.Empty : inputs[0];
            var body = new JsonObject
            {
                ["model"] = "models/" + ModelId,
                ["content"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = value } } },
            };
            ApplyEmbeddingOptions(body, _provider.Options, dimensions);
            using var document = await PostAsync(":embedContent", body, headers, cancellationToken).ConfigureAwait(false);
            return new EmbeddingResult(new[] { ReadVector(document.RootElement.GetProperty("embedding").GetProperty("values")) }, null);
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

        using var batch = await PostAsync(":batchEmbedContents", new JsonObject { ["requests"] = requests }, headers, cancellationToken).ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var embedding in batch.RootElement.GetProperty("embeddings").EnumerateArray())
        {
            vectors.Add(ReadVector(embedding.GetProperty("values")));
        }

        return new EmbeddingResult(vectors, null);
    }

    private async Task<EmbeddingResult> EmbedVertexAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, int? dimensions, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
    {
        var outputDimensionality = VertexDimensionality(providerOptions) ?? dimensions;
        var limit = ModelId.StartsWith("gemini-embedding-2", StringComparison.Ordinal) ? 1 : 250;
        if (values.Count > limit)
        {
            throw new AiSdkException("Too many embedding values for one call. The limit is " + limit + ".");
        }

        if (limit == 1)
        {
            var vectors = new List<float[]>();
            foreach (var value in values)
            {
                var body = new JsonObject
                {
                    ["content"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = value } } },
                };
                if (outputDimensionality is { } width)
                {
                    body["embedContentConfig"] = new JsonObject { ["outputDimensionality"] = width };
                }

                using var document = await PostAsync(":embedContent", body, headers, cancellationToken).ConfigureAwait(false);
                vectors.Add(ReadVector(document.RootElement.GetProperty("embedding").GetProperty("values")));
            }

            return new EmbeddingResult(vectors, null);
        }

        var instances = new JsonArray();
        foreach (var value in values)
        {
            var instance = new JsonObject { ["content"] = value };
            var options = EmbeddingOptions(_provider.Options);
            if (options != null)
            {
                var task = GoogleJson.String(options.Value, "taskType");
                var title = GoogleJson.String(options.Value, "title");
                GoogleJson.Set(instance, "task_type", task);
                GoogleJson.Set(instance, "title", title);
            }

            instances.Add(instance);
        }

        var predictBody = new JsonObject { ["instances"] = instances };
        if (outputDimensionality is { } predictWidth)
        {
            predictBody["parameters"] = new JsonObject { ["outputDimensionality"] = predictWidth };
        }

        using var predict = await PostAsync(":predict", predictBody, headers, cancellationToken).ConfigureAwait(false);
        var predicted = new List<float[]>();
        if (predict.RootElement.TryGetProperty("predictions", out var predictions))
        {
            foreach (var prediction in predictions.EnumerateArray())
            {
                var valuesElement = prediction.TryGetProperty("embeddings", out var embeddings)
                    ? embeddings.GetProperty("values")
                    : prediction.GetProperty("values");
                predicted.Add(ReadVector(valuesElement));
            }
        }

        return new EmbeddingResult(predicted, null);
    }

    private async Task<JsonDocument> PostAsync(string method, JsonObject body, IReadOnlyDictionary<string, string?>? callHeaders, CancellationToken cancellationToken)
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + method),
            GoogleJson.Write(body),
            headers,
            cancellationToken).ConfigureAwait(false);
    }

    private int? VertexDimensionality(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        foreach (var key in new[] { "googleVertex", "vertex", "google" })
        {
            if (providerOptions != null && providerOptions.TryGetValue(key, out var bag) && bag.ValueKind == JsonValueKind.Object
                && bag.TryGetProperty("outputDimensionality", out var value) && value.TryGetInt32(out var count))
            {
                return count;
            }
        }

        var configured = EmbeddingOptions(_provider.Options);
        if (configured != null && configured.Value.TryGetProperty("outputDimensionality", out var fallback) && fallback.TryGetInt32(out var width))
        {
            return width;
        }

        return null;
    }

    private static void ApplyEmbeddingOptions(JsonObject body, GoogleOptions options, int? dimensions)
    {
        var google = EmbeddingOptions(options);
        if (google != null && google.Value.TryGetProperty("outputDimensionality", out var providerDimensions) && providerDimensions.TryGetInt32(out var count))
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
