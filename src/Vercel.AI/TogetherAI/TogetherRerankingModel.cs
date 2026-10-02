// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.TogetherAI;

/// <summary>Together rerank response, including the raw body.</summary>
public sealed class TogetherRerankResult
{
    /// <summary>Creates a rerank response.</summary>
    public TogetherRerankResult(IReadOnlyList<RerankItem> ranking, string? responseId, string? responseModelId, string rawBody)
    {
        Ranking = ranking ?? Array.Empty<RerankItem>();
        ResponseId = responseId;
        ResponseModelId = responseModelId;
        RawBody = rawBody ?? string.Empty;
    }

    /// <summary>Ranked documents.</summary>
    public IReadOnlyList<RerankItem> Ranking { get; }

    /// <summary>Provider response id.</summary>
    public string? ResponseId { get; }

    /// <summary>Provider model id.</summary>
    public string? ResponseModelId { get; }

    /// <summary>Raw response JSON.</summary>
    public string RawBody { get; }

    /// <summary>Together rerank does not emit warnings.</summary>
    public IReadOnlyList<CallWarning>? Warnings
    {
        get { return null; }
    }

    /// <summary>Together rerank does not attach provider metadata.</summary>
    public JsonElement? ProviderMetadata
    {
        get { return null; }
    }
}

/// <summary>Together reranking model. Posts to <c>/rerank</c> with <c>return_documents: false</c>.</summary>
public sealed class TogetherRerankingModel : IRerankingModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public TogetherRerankingModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <summary>Provider id <c>togetherai.reranking</c>.</summary>
    public string Provider => "togetherai.reranking";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var values = new JsonArray();
        if (documents != null)
        {
            foreach (var document in documents)
            {
                values.Add(document);
            }
        }

        var result = await RerankAsync(query, values, topN, null, cancellationToken).ConfigureAwait(false);
        return new RerankResult(result.Ranking);
    }

    /// <summary>Reranks JSON or text documents. JSON documents are sent as objects.</summary>
    public async Task<TogetherRerankResult> RerankAsync(string query, JsonArray documents, int? topN, IReadOnlyList<string>? rankFields, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["documents"] = documents ?? new JsonArray(),
            ["query"] = query ?? string.Empty,
            ["return_documents"] = false,
        };
        if (topN != null)
        {
            body["top_n"] = topN.Value;
        }

        if (rankFields != null)
        {
            var fields = new JsonArray();
            foreach (var field in rankFields)
            {
                fields.Add(field);
            }

            body["rank_fields"] = fields;
        }

        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "rerank"),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var root = document.RootElement;
        var ranking = new List<RerankItem>();
        if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in results.EnumerateArray())
            {
                var index = item.TryGetProperty("index", out var indexElement) && indexElement.TryGetInt32(out var indexValue) ? indexValue : 0;
                var score = item.TryGetProperty("relevance_score", out var scoreElement) && scoreElement.TryGetDouble(out var scoreValue) ? scoreValue : 0;
                ranking.Add(new RerankItem(index, score));
            }
        }

        return new TogetherRerankResult(ranking, JsonValues.GetString(root, "id"), JsonValues.GetString(root, "model"), http.Body);
    }
}
