// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>One reranked document.</summary>
public sealed class AmazonBedrockRanking
{
    /// <summary>Creates a ranking row.</summary>
    public AmazonBedrockRanking(int index, double relevanceScore)
    {
        Index = index;
        RelevanceScore = relevanceScore;
    }

    /// <summary>Index in the original document list.</summary>
    public int Index { get; }

    /// <summary>Relevance score.</summary>
    public double RelevanceScore { get; }
}

/// <summary>A Bedrock Agent Runtime rerank result.</summary>
public sealed class AmazonBedrockRerankResult
{
    /// <summary>Creates a rerank result.</summary>
    public AmazonBedrockRerankResult(IReadOnlyList<AmazonBedrockRanking> ranking, string rawBody, IReadOnlyDictionary<string, string> headers)
    {
        Ranking = ranking ?? Array.Empty<AmazonBedrockRanking>();
        RawBody = rawBody ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Ranked documents.</summary>
    public IReadOnlyList<AmazonBedrockRanking> Ranking { get; }

    /// <summary>Raw response body.</summary>
    public string RawBody { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Builds and parses Bedrock Agent Runtime rerank requests.</summary>
public static class AmazonBedrockRerank
{
    /// <summary>Builds a rerank request body.</summary>
    public static JsonObject BuildRequest(string modelId, string region, string query, int? topN, bool jsonDocuments, IReadOnlyList<JsonNode?> documents, string? nextToken, JsonNode? additionalModelRequestFields)
    {
        var sources = new JsonArray();
        foreach (var document in documents)
        {
            JsonObject inline;
            if (jsonDocuments)
            {
                inline = new JsonObject
                {
                    ["type"] = "JSON",
                    ["jsonDocument"] = document == null ? JsonNode.Parse("null") : document.DeepClone(),
                };
            }
            else
            {
                inline = new JsonObject
                {
                    ["type"] = "TEXT",
                    ["textDocument"] = new JsonObject { ["text"] = document is JsonValue value && value.TryGetValue<string>(out var text) ? text : document?.ToJsonString() ?? string.Empty },
                };
            }

            sources.Add(new JsonObject
            {
                ["type"] = "INLINE",
                ["inlineDocumentSource"] = inline,
            });
        }

        var modelConfiguration = new JsonObject
        {
            ["modelArn"] = "arn:aws:bedrock:" + (region ?? string.Empty) + "::foundation-model/" + (modelId ?? string.Empty),
        };
        if (additionalModelRequestFields != null)
        {
            modelConfiguration["additionalModelRequestFields"] = additionalModelRequestFields.DeepClone();
        }

        var configuration = new JsonObject
        {
            ["modelConfiguration"] = modelConfiguration,
        };
        if (topN != null)
        {
            configuration["numberOfResults"] = topN.Value;
        }

        var body = new JsonObject();
        if (nextToken != null)
        {
            body["nextToken"] = nextToken;
        }

        body["queries"] = new JsonArray
        {
            new JsonObject
            {
                ["textQuery"] = new JsonObject { ["text"] = query ?? string.Empty },
                ["type"] = "TEXT",
            },
        };
        body["rerankingConfiguration"] = new JsonObject
        {
            ["bedrockRerankingConfiguration"] = configuration,
            ["type"] = "BEDROCK_RERANKING_MODEL",
        };
        body["sources"] = sources;
        return body;
    }

    /// <summary>Reads provider options <c>nextToken</c> and <c>additionalModelRequestFields</c>.</summary>
    public static void ReadOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions, out string? nextToken, out JsonNode? additionalFields)
    {
        nextToken = null;
        additionalFields = null;
        if (providerOptions == null)
        {
            return;
        }

        JsonElement provider;
        if (!providerOptions.TryGetValue("amazonBedrock", out provider) && !providerOptions.TryGetValue("bedrock", out provider) && !providerOptions.TryGetValue("amazon-bedrock", out provider))
        {
            return;
        }

        if (provider.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (provider.TryGetProperty("nextToken", out var token) && token.ValueKind == JsonValueKind.String)
        {
            nextToken = token.GetString();
        }

        if (provider.TryGetProperty("additionalModelRequestFields", out var fields) && fields.ValueKind == JsonValueKind.Object)
        {
            additionalFields = JsonNode.Parse(fields.GetRawText());
        }
    }

    /// <summary>Parses a rerank response.</summary>
    public static AmazonBedrockRerankResult Parse(string body, IReadOnlyDictionary<string, string>? headers)
    {
        var ranking = new List<AmazonBedrockRanking>();
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body))
        {
            if (document.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                foreach (var result in results.EnumerateArray())
                {
                    var index = result.TryGetProperty("index", out var indexElement) && indexElement.ValueKind == JsonValueKind.Number ? indexElement.GetInt32() : 0;
                    var score = result.TryGetProperty("relevanceScore", out var scoreElement) && scoreElement.ValueKind == JsonValueKind.Number ? scoreElement.GetDouble() : 0;
                    ranking.Add(new AmazonBedrockRanking(index, score));
                }
            }
        }

        return new AmazonBedrockRerankResult(ranking, body ?? string.Empty, headers ?? new Dictionary<string, string>());
    }
}
