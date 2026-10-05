// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI embeddings model.</summary>
public sealed class OpenAIEmbeddingModel : IEmbeddingModel
{
    /// <summary>Maximum embedding inputs in one request.</summary>
    public const int DefaultMaxEmbeddingsPerCall = 2048;

    /// <summary>Maximum input bytes in one request.</summary>
    public const int DefaultMaxInputBytesPerCall = 300000;

    private readonly OpenAIProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public OpenAIEmbeddingModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".embedding";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum values accepted by one call.</summary>
    public int MaxEmbeddingsPerCall => DefaultMaxEmbeddingsPerCall;

    /// <summary>Maximum combined input size accepted by one call.</summary>
    public int MaxInputBytesPerCall => DefaultMaxInputBytesPerCall;

    /// <summary>Whether the model accepts parallel embedding calls.</summary>
    public bool SupportsParallelCalls => true;

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken)
    {
        var result = await EmbedAsync(values, null, null, null, cancellationToken).ConfigureAwait(false);
        return result.Result;
    }

    /// <summary>Embeds <paramref name="values"/>, sending <paramref name="dimensions"/> and <paramref name="user"/> when they are set.</summary>
    public async Task<OpenAIEmbeddingCallResult> EmbedAsync(
        IReadOnlyList<string> values,
        int? dimensions,
        string? user,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException(
                "Too many embedding values for a single call. The maximum is " + MaxEmbeddingsPerCall.ToString() + ".");
        }

        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
            ["encoding_format"] = "float",
        };
        if (dimensions != null)
        {
            body["dimensions"] = dimensions.Value;
        }

        if (user != null)
        {
            body["user"] = user;
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "embeddings"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var vectors = new List<float[]>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var embedding = item.GetProperty("embedding");
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
        if (document.RootElement.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("prompt_tokens", out var prompt)
            && prompt.TryGetInt32(out var count))
        {
            tokens = count;
        }

        return new OpenAIEmbeddingCallResult(new EmbeddingResult(vectors, tokens), response.Headers, response.Body);
    }
}

/// <summary>Embedding vectors plus the raw response.</summary>
public sealed class OpenAIEmbeddingCallResult
{
    internal OpenAIEmbeddingCallResult(EmbeddingResult result, IReadOnlyDictionary<string, string> headers, string rawBody)
    {
        Result = result;
        Headers = headers;
        RawBody = rawBody;
    }

    /// <summary>Embedding vectors and token count.</summary>
    public EmbeddingResult Result { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response body.</summary>
    public string RawBody { get; }
}
