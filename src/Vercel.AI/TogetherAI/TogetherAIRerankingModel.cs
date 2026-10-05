// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.TogetherAI;

/// <summary>Together reranking model. Posts <c>/rerank</c> and reads <c>relevance_score</c>.</summary>
public sealed class TogetherAIRerankingModel : IRerankingModel, Operations.IRerankCaller
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
        var call = new Operations.RerankModelCall(new Operations.RerankModelDocuments("text", documents ?? Array.Empty<string>()), query, topN, null, null, cancellationToken);
        var response = await DoRerankAsync(call, cancellationToken).ConfigureAwait(false);
        return new RerankResult(response.Ranking.Select(rank => new RerankItem(rank.Index, rank.RelevanceScore)).ToList());
    }

    /// <summary>Reranks text or object documents. <c>togetherai.rankFields</c> is sent as <c>rank_fields</c>.</summary>
    public async Task<Operations.RerankModelResponse> DoRerankAsync(Operations.RerankModelCall call, CancellationToken cancellationToken)
    {
        var docs = new JsonArray();
        foreach (var document in call.Documents.Values)
        {
            docs.Add(JsonSerializer.SerializeToNode(document));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["documents"] = docs,
            ["query"] = call.Query,
            ["top_n"] = call.TopN,
            ["return_documents"] = false,
        };
        if (call.ProviderOptions is { ValueKind: JsonValueKind.Object } options
            && options.TryGetProperty("togetherai", out var together) && together.ValueKind == JsonValueKind.Object
            && together.TryGetProperty("rankFields", out var rankFields))
        {
            body["rank_fields"] = JsonNode.Parse(rankFields.GetRawText());
        }

        var headers = _provider.CreateHeaders(Headers);
        if (call.Headers != null)
        {
            foreach (var header in call.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        var response = await _provider.PostJsonAsync(_provider.RerankUri(), body.ToJsonString(), headers, cancellationToken).ConfigureAwait(false);
        LastResponseBody = response.Body;
        LastResponseHeaders = response.Headers;
        using var parsed = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = parsed.RootElement;
        var ranking = new List<Operations.RerankModelRank>();
        if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var result in results.EnumerateArray())
            {
                var index = result.TryGetProperty("index", out var indexElement) && indexElement.TryGetInt32(out var parsedIndex) ? parsedIndex : 0;
                var score = result.TryGetProperty("relevance_score", out var scoreElement) && scoreElement.TryGetDouble(out var parsedScore) ? parsedScore : 0;
                ranking.Add(new Operations.RerankModelRank(index, score));
            }
        }

        return new Operations.RerankModelResponse(ranking, response: new Operations.ProviderResponse(response.Headers, root.Clone(), String(root, "id"), modelId: String(root, "model")));
    }

    private static string? String(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
