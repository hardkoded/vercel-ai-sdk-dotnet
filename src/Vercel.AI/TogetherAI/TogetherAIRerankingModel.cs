// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.TogetherAI;

/// <summary>Together reranking model. Posts <c>/rerank</c> and reads <c>relevance_score</c>.</summary>
public sealed class TogetherAIRerankingModel : IRerankingModel
{
    private readonly TogetherAIProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public TogetherAIRerankingModel(TogetherAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "togetherai.reranking";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Raw response body from the most recent call.</summary>
    public string? LastResponseBody { get; private set; }

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var docs = new JsonArray();
        if (documents != null)
        {
            foreach (var document in documents)
            {
                docs.Add(document);
            }
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["documents"] = docs,
            ["query"] = query ?? string.Empty,
            ["top_n"] = topN,
            ["return_documents"] = false,
        };
        var response = await _provider.PostJsonAsync(_provider.RerankUri(), body.ToJsonString(), _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
        LastResponseBody = response.Body;
        LastResponseHeaders = response.Headers;
        using var parsed = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var items = new List<RerankItem>();
        if (parsed.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var result in results.EnumerateArray())
            {
                var index = result.TryGetProperty("index", out var indexElement) && indexElement.TryGetInt32(out var parsedIndex) ? parsedIndex : 0;
                var score = result.TryGetProperty("relevance_score", out var scoreElement) && scoreElement.TryGetDouble(out var parsedScore) ? parsedScore : 0;
                items.Add(new RerankItem(index, score));
            }
        }

        return new RerankResult(items);
    }
}
