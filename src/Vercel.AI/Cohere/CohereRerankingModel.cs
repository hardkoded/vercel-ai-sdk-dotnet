// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>Cohere rerank model.</summary>
public sealed class CohereRerankingModel : IRerankingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public CohereRerankingModel(CohereProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var docs = new List<CohereRerankDocument>();
        if (documents != null)
        {
            foreach (var document in documents)
            {
                docs.Add(CohereRerankDocument.Text(document));
            }
        }

        var result = await RerankAsync(new CohereRerankRequest(query, docs) { TopN = topN }, cancellationToken).ConfigureAwait(false);
        return new RerankResult(result.Ranking);
    }

    /// <summary>Reranks text or object documents.</summary>
    public async Task<CohereRerankResult> RerankAsync(CohereRerankRequest request, CancellationToken cancellationToken)
    {
        request ??= new CohereRerankRequest(string.Empty, Array.Empty<CohereRerankDocument>());
        var warnings = new List<CohereWarning>();
        var documents = new JsonArray();
        var sawObject = false;
        foreach (var entry in request.Documents)
        {
            if (entry.IsObject)
            {
                sawObject = true;
                documents.Add(JsonSerializer.Serialize(entry.Json!.Value));
            }
            else
            {
                documents.Add(entry.PlainText);
            }
        }

        if (sawObject)
        {
            warnings.Add(new CohereWarning("compatibility", "object documents", "Object documents are converted to strings."));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["query"] = request.Query,
            ["documents"] = documents,
        };
        if (request.TopN is { } top)
        {
            body["top_n"] = top;
        }

        if (request.MaxTokensPerDoc is { } max)
        {
            body["max_tokens_per_doc"] = max;
        }

        if (request.Priority is { } priority)
        {
            body["priority"] = priority;
        }

        var response = await _provider.PostJsonAsync(ApiKeys.Combine(_provider.Options.BaseUrl, "rerank"), body.ToJsonString(), request.Headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var items = new List<RerankItem>();
        if (document.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in results.EnumerateArray())
            {
                var score = item.TryGetProperty("relevance_score", out var relevance) && relevance.ValueKind == JsonValueKind.Number ? relevance.GetDouble() : 0;
                items.Add(new RerankItem(item.GetProperty("index").GetInt32(), score));
            }
        }

        string? id = null;
        if (document.RootElement.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
        {
            id = idElement.GetString();
        }

        return new CohereRerankResult(items, warnings, id, response.Body, response.Headers);
    }
}
