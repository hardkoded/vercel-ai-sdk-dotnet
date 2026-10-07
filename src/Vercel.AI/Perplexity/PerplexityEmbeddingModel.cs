// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>
/// Perplexity embeddings at <c>{base}/v1/embeddings</c>. Perplexity returns quantized vectors as base64,
/// so the model decodes <c>base64_int8</c> (the default) or <c>base64_binary</c> into numbers.
/// </summary>
public sealed class PerplexityEmbeddingModel : IEmbeddingModel, IEmbeddingCaller
{
    /// <summary>Maximum values per call.</summary>
    public const int MaxValuesPerCall = 512;

    private readonly PerplexityProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public PerplexityEmbeddingModel(PerplexityProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc cref="IEmbeddingCaller.Provider" />
    public string Provider => "perplexity.embedding";

    /// <inheritdoc cref="IEmbeddingCaller.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public int? MaxEmbeddingsPerCall => MaxValuesPerCall;

    /// <inheritdoc />
    public double? MaxInputBytesPerCall => null;

    /// <inheritdoc />
    public bool SupportsParallelCalls => true;

    /// <inheritdoc />
    public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken)
    {
        return Task.FromResult<JsonElement?>(null);
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken)
    {
        var response = await DoEmbedAsync(new EmbeddingModelCall(values, new Dictionary<string, string>(), providerOptions == null ? null : JsonSerializer.SerializeToElement(providerOptions), cancellationToken), cancellationToken).ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var embedding in response.Embeddings)
        {
            vectors.Add(Array.ConvertAll(embedding, value => (float)value));
        }

        return new EmbeddingResult(vectors, double.IsNaN(response.Tokens) ? null : (int)response.Tokens);
    }

    /// <inheritdoc />
    public async Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (call.Values.Count > MaxValuesPerCall)
        {
            throw new AiSdkException(
                "Too many values for a single embedding call. The " + Provider + " model \"" + ModelId + "\" can only embed up to "
                + MaxValuesPerCall.ToString(CultureInfo.InvariantCulture) + " values per call, but "
                + call.Values.Count.ToString(CultureInfo.InvariantCulture) + " values were provided.");
        }

        JsonElement? perplexity = null;
        if (call.ProviderOptions is { ValueKind: JsonValueKind.Object } options
            && options.TryGetProperty(PerplexityProvider.ProviderId, out var entry)
            && entry.ValueKind == JsonValueKind.Object)
        {
            perplexity = entry;
        }

        var encodingFormat = Option(perplexity, "encodingFormat")?.GetString() ?? "base64_int8";
        var input = new JsonArray();
        foreach (var value in call.Values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        if (Option(perplexity, "dimensions") is { } dimensions)
        {
            body["dimensions"] = dimensions.GetInt32();
        }

        body["encoding_format"] = encodingFormat;
        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var response = await _provider.PostJsonAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/embeddings"),
            body.ToJsonString(),
            _provider.CreateHeaders(headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var embeddings = new List<double[]>();
        foreach (var item in root.GetProperty("data").EnumerateArray())
        {
            embeddings.Add(Decode(item.GetProperty("embedding").GetString() ?? string.Empty, encodingFormat));
        }

        double? tokens = null;
        JsonElement? metadata = null;
        if (Option(root, "usage") is { } usage)
        {
            tokens = usage.GetProperty("prompt_tokens").GetDouble();
            if (Option(usage, "cost") is { } cost)
            {
                metadata = JsonValues.ParseOrEmpty(new JsonObject
                {
                    [PerplexityProvider.ProviderId] = new JsonObject
                    {
                        ["cost"] = new JsonObject
                        {
                            ["inputCost"] = Copy(cost, "input_cost"),
                            ["totalCost"] = Copy(cost, "total_cost"),
                            ["currency"] = Copy(cost, "currency"),
                        },
                    },
                }.ToJsonString());
            }
        }

        return new EmbeddingModelResponse(embeddings, tokens, providerMetadata: metadata, response: new ProviderResponse(response.Headers, root.Clone()));
    }

    private static double[] Decode(string base64, string encodingFormat)
    {
        var bytes = Convert.FromBase64String(base64);
        var values = new double[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            values[i] = encodingFormat == "base64_int8" ? (sbyte)bytes[i] : bytes[i];
        }

        return values;
    }

    private static JsonElement? Option(JsonElement? element, string name)
    {
        return element is { } value && value.TryGetProperty(name, out var property) && property.ValueKind != JsonValueKind.Null
            ? property
            : null;
    }

    private static JsonNode? Copy(JsonElement element, string name)
    {
        return Option(element, name) is { } value ? JsonNode.Parse(value.GetRawText()) : null;
    }
}
