// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Voyage;

/// <summary>Values sent to <c>POST /embeddings</c>.</summary>
public sealed class VoyageEmbeddingRequest
{
    /// <summary>Creates an embedding request.</summary>
    public VoyageEmbeddingRequest(IReadOnlyList<string> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>Input strings.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Voyage <c>input_type</c>, such as <c>document</c>.</summary>
    public string? InputType { get; set; }

    /// <summary>Voyage <c>output_dimension</c>.</summary>
    public int? OutputDimension { get; set; }

    /// <summary>Voyage <c>truncation</c>.</summary>
    public bool? Truncation { get; set; }

    /// <summary>Voyage <c>output_dtype</c>.</summary>
    public string? OutputDtype { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Voyage embedding result, including the raw response.</summary>
public sealed class VoyageEmbeddingGeneration
{
    /// <summary>Creates a result.</summary>
    public VoyageEmbeddingGeneration(IReadOnlyList<double[]> embeddings, int tokens, IReadOnlyDictionary<string, string> responseHeaders, string responseBody)
    {
        Embeddings = embeddings ?? Array.Empty<double[]>();
        Tokens = tokens;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        ResponseBody = responseBody ?? string.Empty;
    }

    /// <summary>Vectors sorted by the provider index.</summary>
    public IReadOnlyList<double[]> Embeddings { get; }

    /// <summary>Total tokens reported by Voyage.</summary>
    public int Tokens { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Raw response JSON.</summary>
    public string ResponseBody { get; }
}

/// <summary>Voyage embedding model. Posts to <c>/embeddings</c>.</summary>
public sealed class VoyageEmbeddingModel : IEmbeddingModel
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates an embedding model.</summary>
    public VoyageEmbeddingModel(HttpClient http, string modelId, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? VoyageProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <inheritdoc />
    public string Provider { get; set; } = "voyage.embedding";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum inputs in one call.</summary>
    public int MaxEmbeddingsPerCall
    {
        get { return 128; }
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var result = await EmbedAsync(new VoyageEmbeddingRequest(values), cancellationToken).ConfigureAwait(false);
        var vectors = new List<float[]>(result.Embeddings.Count);
        foreach (var embedding in result.Embeddings)
        {
            var vector = new float[embedding.Length];
            for (var index = 0; index < embedding.Length; index++)
            {
                vector[index] = (float)embedding[index];
            }

            vectors.Add(vector);
        }

        return new EmbeddingResult(vectors, result.Tokens);
    }

    /// <summary>Embeds values and returns the raw response.</summary>
    public async Task<VoyageEmbeddingGeneration> EmbedAsync(VoyageEmbeddingRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Values.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException("Too many embedding values for " + Provider + " model '" + ModelId + "'. Maximum is " + MaxEmbeddingsPerCall + ".");
        }

        var input = new JsonArray();
        foreach (var value in request.Values)
        {
            input.Add(value);
        }

        var body = new JsonObject
        {
            ["input"] = input,
            ["model"] = ModelId,
        };
        if (!string.IsNullOrEmpty(request.InputType))
        {
            body["input_type"] = request.InputType;
        }

        if (request.Truncation is bool truncation)
        {
            body["truncation"] = truncation;
        }

        if (request.OutputDimension is int dimension)
        {
            body["output_dimension"] = dimension;
        }

        if (!string.IsNullOrEmpty(request.OutputDtype))
        {
            body["output_dtype"] = request.OutputDtype;
        }

        var response = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            ApiKeys.Combine(_baseUrl, "/embeddings"),
            MediaExchange.Json(body.ToJsonString()),
            Merge(request.Headers),
            cancellationToken).ConfigureAwait(false);
        if (response.StatusCode < 200 || response.StatusCode > 299)
        {
            throw ProviderHttp.MapStatus(response.StatusCode, response.Text);
        }

        var rows = new List<IndexedVector>();
        var tokens = 0;
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Text) ? "{}" : response.Text))
        {
            var root = document.RootElement;
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("total_tokens", out var total) && total.TryGetInt32(out var parsedTokens))
            {
                tokens = parsedTokens;
            }

            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                var position = 0;
                foreach (var item in data.EnumerateArray())
                {
                    var index = position;
                    if (item.TryGetProperty("index", out var indexElement) && indexElement.TryGetInt32(out var parsedIndex))
                    {
                        index = parsedIndex;
                    }

                    var embedding = item.GetProperty("embedding");
                    var vector = new double[embedding.GetArrayLength()];
                    var cursor = 0;
                    foreach (var number in embedding.EnumerateArray())
                    {
                        vector[cursor++] = number.GetDouble();
                    }

                    rows.Add(new IndexedVector(index, vector));
                    position++;
                }
            }
        }

        rows.Sort((left, right) => left.Index.CompareTo(right.Index));
        var embeddings = new List<double[]>(rows.Count);
        foreach (var row in rows)
        {
            embeddings.Add(row.Values);
        }

        return new VoyageEmbeddingGeneration(embeddings, tokens, response.Headers, response.Text);
    }

    private Dictionary<string, string?> Merge(IDictionary<string, string>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private sealed class IndexedVector
    {
        public IndexedVector(int index, double[] values)
        {
            Index = index;
            Values = values;
        }

        public int Index { get; }

        public double[] Values { get; }
    }
}
