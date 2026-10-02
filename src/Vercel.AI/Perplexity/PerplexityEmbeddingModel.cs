// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Perplexity;

/// <summary>Settings for one Perplexity embedding call.</summary>
public sealed class PerplexityEmbedRequest
{
    /// <summary>Creates a request for <paramref name="values"/>.</summary>
    public PerplexityEmbedRequest(IReadOnlyList<string> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>Input strings.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Extra HTTP headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Matryoshka output size. Omitted when unset.</summary>
    public int? Dimensions { get; set; }

    /// <summary>
    /// Quantized encoding. <c>base64_int8</c> is the default. <c>base64_binary</c> returns packed bits.
    /// </summary>
    public string? EncodingFormat { get; set; }
}

/// <summary>Decoded embeddings plus Sonar cost metadata and response headers.</summary>
public sealed class PerplexityEmbedResult
{
    /// <summary>Creates a result.</summary>
    public PerplexityEmbedResult(
        IReadOnlyList<float[]> embeddings,
        int? tokens,
        JsonElement? providerMetadata,
        IReadOnlyDictionary<string, string> responseHeaders)
    {
        Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        Tokens = tokens;
        ProviderMetadata = providerMetadata;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
    }

    /// <summary>One numeric vector per input, decoded from the quantized payload.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Prompt tokens, when the provider reports usage.</summary>
    public int? Tokens { get; }

    /// <summary>Perplexity cost metadata. Absent when the response has no cost object.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
}

/// <summary>
/// Perplexity embeddings. The API returns quantized base64 vectors, decoded to signed int8
/// values by default. At most 512 inputs are accepted per call.
/// </summary>
public sealed class PerplexityEmbeddingModel : IEmbeddingModel
{
    /// <summary>User-Agent product prefix sent on embedding requests.</summary>
    public const string UserAgentPrefix = "ai-sdk/perplexity/";

    /// <summary>User-Agent value sent on embedding requests.</summary>
    public const string UserAgent = "ai-sdk/perplexity/0.1.0";

    private readonly PerplexityProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public PerplexityEmbeddingModel(PerplexityProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum inputs in one call.</summary>
    public int MaxEmbeddingsPerCall => 512;

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var result = await EmbedAsync(new PerplexityEmbedRequest(values), cancellationToken).ConfigureAwait(false);
        return new EmbeddingResult(result.Embeddings, result.Tokens);
    }

    /// <summary>Embeds values and returns cost metadata and response headers.</summary>
    public async Task<PerplexityEmbedResult> EmbedAsync(PerplexityEmbedRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Values.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException(
                "Too many values for a single embedding call to " + ModelId + ". The maximum is " + MaxEmbeddingsPerCall.ToString(CultureInfo.InvariantCulture) + ".");
        }

        var encoding = string.IsNullOrEmpty(request.EncodingFormat) ? "base64_int8" : request.EncodingFormat!;
        if (!string.Equals(encoding, "base64_int8", StringComparison.Ordinal)
            && !string.Equals(encoding, "base64_binary", StringComparison.Ordinal))
        {
            throw new AiSdkException("Invalid perplexity embedding option: encodingFormat is not supported.");
        }

        if (request.Dimensions is { } dimensions && dimensions <= 0)
        {
            throw new AiSdkException("Invalid perplexity embedding option: dimensions must be a positive integer.");
        }

        var input = new JsonArray();
        foreach (var value in request.Values)
        {
            input.Add(value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
            ["encoding_format"] = encoding,
        };
        if (request.Dimensions is { } size)
        {
            body["dimensions"] = size;
        }

        var headers = _provider.CreateHeaders();
        headers["User-Agent"] = UserAgent;
        if (request.Headers != null)
        {
            foreach (var pair in request.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            EmbeddingsUri(),
            body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid Perplexity embedding response.");
        }

        var vectors = new List<float[]>();
        foreach (var item in data.EnumerateArray())
        {
            if (!item.TryGetProperty("embedding", out var embedding))
            {
                throw new AiSdkException("Invalid Perplexity embedding response.");
            }

            vectors.Add(ReadVector(embedding, encoding));
        }

        int? tokens = null;
        JsonElement? metadata = null;
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            if (!usage.TryGetProperty("prompt_tokens", out var prompt) || prompt.ValueKind != JsonValueKind.Number)
            {
                throw new AiSdkException("Invalid Perplexity embedding response.");
            }

            tokens = prompt.GetInt32();
            if (usage.TryGetProperty("cost", out var cost) && cost.ValueKind == JsonValueKind.Object)
            {
                metadata = CostMetadata(cost);
            }
        }

        return new PerplexityEmbedResult(vectors, tokens, metadata, http.Headers);
    }

    private Uri EmbeddingsUri()
    {
        return Vercel.AI.ProviderUtils.ApiKeys.Combine(_provider.Options.BaseUrl, "v1/embeddings");
    }

    private static float[] ReadVector(JsonElement embedding, string encoding)
    {
        if (embedding.ValueKind == JsonValueKind.Array)
        {
            var numbers = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                numbers[index++] = number.GetSingle();
            }

            return numbers;
        }

        if (embedding.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid Perplexity embedding response.");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(embedding.GetString() ?? string.Empty);
        }
        catch (FormatException exception)
        {
            throw new AiSdkException("Invalid Perplexity embedding response.", exception);
        }

        var vector = new float[bytes.Length];
        var signed = string.Equals(encoding, "base64_int8", StringComparison.Ordinal);
        for (var index = 0; index < bytes.Length; index++)
        {
            vector[index] = signed ? (sbyte)bytes[index] : bytes[index];
        }

        return vector;
    }

    private static JsonElement CostMetadata(JsonElement cost)
    {
        var node = new JsonObject
        {
            ["perplexity"] = new JsonObject
            {
                ["cost"] = new JsonObject
                {
                    ["inputCost"] = CloneOrNull(cost, "input_cost"),
                    ["totalCost"] = CloneOrNull(cost, "total_cost"),
                    ["currency"] = CloneOrNull(cost, "currency"),
                },
            },
        };
        return JsonDocument.Parse(node.ToJsonString()).RootElement.Clone();
    }

    private static JsonNode CloneOrNull(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return JsonNode.Parse("null")!;
        }

        return JsonNode.Parse(value.GetRawText())!;
    }
}
