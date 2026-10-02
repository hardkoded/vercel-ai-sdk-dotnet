// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Voyage;

/// <summary>A compatibility warning from the Voyage reranker.</summary>
public sealed class VoyageRerankWarning
{
    /// <summary>Creates a warning.</summary>
    public VoyageRerankWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning category.</summary>
    public string Type { get; }

    /// <summary>Feature that was adapted.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>One ranked document.</summary>
public sealed class VoyageRank
{
    /// <summary>Creates a rank.</summary>
    public VoyageRank(int index, double relevanceScore)
    {
        Index = index;
        RelevanceScore = relevanceScore;
    }

    /// <summary>Index in the submitted document list.</summary>
    public int Index { get; }

    /// <summary>Relevance score.</summary>
    public double RelevanceScore { get; }
}

/// <summary>Voyage rerank result.</summary>
public sealed class VoyageRerankGeneration
{
    /// <summary>Creates a result.</summary>
    public VoyageRerankGeneration(IReadOnlyList<VoyageRank> ranking, IReadOnlyList<VoyageRerankWarning> warnings, IReadOnlyDictionary<string, string> responseHeaders, string responseBody)
    {
        Ranking = ranking ?? Array.Empty<VoyageRank>();
        Warnings = warnings ?? Array.Empty<VoyageRerankWarning>();
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        ResponseBody = responseBody ?? string.Empty;
    }

    /// <summary>Documents ordered as the provider returned them.</summary>
    public IReadOnlyList<VoyageRank> Ranking { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<VoyageRerankWarning> Warnings { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Raw response JSON.</summary>
    public string ResponseBody { get; }
}

/// <summary>Voyage reranking model. Posts to <c>/rerank</c>.</summary>
public sealed class VoyageRerankingModel : IRerankingModel
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a reranking model.</summary>
    public VoyageRerankingModel(HttpClient http, string modelId, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? VoyageProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <inheritdoc />
    public string Provider { get; set; } = "voyage.reranking";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var result = await RerankTextAsync(query, documents, topN, null, cancellationToken).ConfigureAwait(false);
        var items = new List<RerankItem>(result.Ranking.Count);
        foreach (var rank in result.Ranking)
        {
            items.Add(new RerankItem(rank.Index, rank.RelevanceScore));
        }

        return new RerankResult(items);
    }

    /// <summary>Reranks plain text documents.</summary>
    public Task<VoyageRerankGeneration> RerankTextAsync(string query, IReadOnlyList<string> documents, int? topN, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return SendAsync(query, documents ?? Array.Empty<string>(), topN, headers, objectDocuments: false, cancellationToken);
    }

    /// <summary>Reranks JSON objects. Each object is sent as a compact JSON string.</summary>
    public Task<VoyageRerankGeneration> RerankObjectsAsync(string query, IReadOnlyList<JsonElement> documents, int? topN, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var values = new List<string>();
        if (documents != null)
        {
            foreach (var document in documents)
            {
                values.Add(JsonSerializer.Serialize(document));
            }
        }

        return SendAsync(query, values, topN, headers, objectDocuments: true, cancellationToken);
    }

    private async Task<VoyageRerankGeneration> SendAsync(string query, IReadOnlyList<string> documents, int? topN, IDictionary<string, string>? headers, bool objectDocuments, CancellationToken cancellationToken)
    {
        var warnings = new List<VoyageRerankWarning>();
        if (objectDocuments)
        {
            warnings.Add(new VoyageRerankWarning("compatibility", "object documents", "Object documents are converted to strings."));
        }

        var docs = new JsonArray();
        foreach (var document in documents)
        {
            docs.Add(document);
        }

        var body = new JsonObject
        {
            ["query"] = query ?? string.Empty,
            ["documents"] = docs,
            ["model"] = ModelId,
        };
        if (topN is int top)
        {
            body["top_k"] = top;
        }

        var response = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            ApiKeys.Combine(_baseUrl, "/rerank"),
            MediaExchange.Json(body.ToJsonString()),
            Merge(headers),
            cancellationToken).ConfigureAwait(false);
        if (response.StatusCode < 200 || response.StatusCode > 299)
        {
            throw ProviderHttp.MapStatus(response.StatusCode, response.Text);
        }

        var ranking = new List<VoyageRank>();
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Text) ? "{}" : response.Text))
        {
            if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    var index = item.TryGetProperty("index", out var indexElement) ? indexElement.GetInt32() : 0;
                    var score = item.TryGetProperty("relevance_score", out var scoreElement) ? scoreElement.GetDouble() : 0;
                    ranking.Add(new VoyageRank(index, score));
                }
            }
        }

        return new VoyageRerankGeneration(ranking, warnings, response.Headers, response.Text);
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
}
