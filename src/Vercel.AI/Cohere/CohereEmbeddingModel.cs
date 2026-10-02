// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>Cohere embedding model. The default input type is <c>search_query</c>.</summary>
public sealed class CohereEmbeddingModel : IEmbeddingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public CohereEmbeddingModel(CohereProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum values accepted in one call.</summary>
    public int MaxEmbeddingsPerCall => 96;

    /// <inheritdoc />
    public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        return EmbedCoreAsync(new CohereEmbedRequest(values), cancellationToken);
    }

    /// <summary>Embeds values with Cohere options.</summary>
    public async Task<CohereEmbeddingResult> EmbedAsync(CohereEmbedRequest request, CancellationToken cancellationToken)
    {
        request ??= new CohereEmbedRequest(Array.Empty<string>());
        if (request.Values.Count > MaxEmbeddingsPerCall)
        {
            throw new InvalidOperationException("Too many embedding values for one call.");
        }

        var texts = new JsonArray();
        foreach (var value in request.Values)
        {
            texts.Add(value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["embedding_types"] = new JsonArray { "float" },
            ["texts"] = texts,
            ["input_type"] = string.IsNullOrEmpty(request.InputType) ? "search_query" : request.InputType,
        };
        if (request.OutputDimension is { } dimension)
        {
            body["output_dimension"] = dimension;
        }

        if (!string.IsNullOrEmpty(request.Truncate))
        {
            body["truncate"] = request.Truncate;
        }

        var response = await _provider.PostJsonAsync(ApiKeys.Combine(_provider.Options.BaseUrl, "embed"), body.ToJsonString(), request.Headers, cancellationToken).ConfigureAwait(false);
        return Parse(response.Body, response.Headers);
    }

    private async Task<EmbeddingResult> EmbedCoreAsync(CohereEmbedRequest request, CancellationToken cancellationToken)
    {
        var result = await EmbedAsync(request, cancellationToken).ConfigureAwait(false);
        return new EmbeddingResult(result.Embeddings, result.Tokens);
    }

    private static CohereEmbeddingResult Parse(string body, IReadOnlyDictionary<string, string> headers)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        var root = document.RootElement;
        var vectors = new List<float[]>();
        if (root.TryGetProperty("embeddings", out var embeddings) && embeddings.ValueKind == JsonValueKind.Object && embeddings.TryGetProperty("float", out var floats))
        {
            foreach (var embedding in floats.EnumerateArray())
            {
                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray())
                {
                    vector[index++] = number.GetSingle();
                }

                vectors.Add(vector);
            }
        }

        int? tokens = null;
        if (root.TryGetProperty("meta", out var meta)
            && meta.TryGetProperty("billed_units", out var billed)
            && billed.TryGetProperty("input_tokens", out var input)
            && input.ValueKind == JsonValueKind.Number)
        {
            tokens = input.GetInt32();
        }

        return new CohereEmbeddingResult(vectors, tokens, body ?? string.Empty, headers);
    }
}
