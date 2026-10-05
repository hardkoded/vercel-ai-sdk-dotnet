// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock Agent Runtime rerank model.</summary>
public sealed class AmazonBedrockRerankingModel : IRerankingModel, Operations.IRerankCaller
{
    private readonly AmazonBedrockProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public AmazonBedrockRerankingModel(AmazonBedrockProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        return DoRerankAsync(query, documents, topN, null, cancellationToken);
    }

    /// <summary>
    /// Reranks text documents. <paramref name="providerOptions"/> is the V4 provider-options object
    /// (<c>amazonBedrock</c> or legacy <c>bedrock</c>).
    /// </summary>
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
    {
        var call = new Operations.RerankModelCall(new Operations.RerankModelDocuments("text", documents.ToArray()), query, topN, providerOptions, null, cancellationToken);
        return ToResult(await DoRerankAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Reranks JSON documents. Each value is sent as an inline JSON document.</summary>
    public async Task<RerankResult> DoRerankObjectsAsync(string query, IReadOnlyList<JsonElement> documents, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
    {
        var call = new Operations.RerankModelCall(new Operations.RerankModelDocuments("object", documents.Cast<object?>().ToArray()), query, topN, providerOptions, null, cancellationToken);
        return ToResult(await DoRerankAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Reranks text or object documents. Call headers are sent before SigV4 signing.
    /// The response carries the headers and raw body, with no warnings or provider metadata.
    /// </summary>
    public async Task<Operations.RerankModelResponse> DoRerankAsync(Operations.RerankModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var sources = new JsonArray();
        foreach (var document in call.Documents.Values)
        {
            sources.Add(new JsonObject
            {
                ["type"] = "INLINE",
                ["inlineDocumentSource"] = call.Documents.Type == "text"
                    ? new JsonObject
                    {
                        ["type"] = "TEXT",
                        ["textDocument"] = new JsonObject { ["text"] = (string?)document },
                    }
                    : new JsonObject
                    {
                        ["type"] = "JSON",
                        ["jsonDocument"] = JsonSerializer.SerializeToNode(document),
                    },
            });
        }

        var options = ReadOptions(call.ProviderOptions);
        var modelConfiguration = new JsonObject
        {
            ["modelArn"] = "arn:aws:bedrock:" + _provider.Options.Region + "::foundation-model/" + ModelId,
        };
        if (options.AdditionalModelRequestFields != null)
        {
            modelConfiguration["additionalModelRequestFields"] = options.AdditionalModelRequestFields;
        }

        var configuration = new JsonObject
        {
            ["modelConfiguration"] = modelConfiguration,
        };
        if (call.TopN is { } count)
        {
            configuration["numberOfResults"] = count;
        }

        var body = new JsonObject
        {
            ["queries"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "TEXT",
                    ["textQuery"] = new JsonObject { ["text"] = call.Query },
                },
            },
            ["rerankingConfiguration"] = new JsonObject
            {
                ["type"] = "BEDROCK_RERANKING_MODEL",
                ["bedrockRerankingConfiguration"] = configuration,
            },
            ["sources"] = sources,
        };
        if (!string.IsNullOrEmpty(options.NextToken))
        {
            body["nextToken"] = options.NextToken;
        }

        var json = body.ToJsonString();
        var uri = new Uri(AmazonBedrockEndpoints.Resolve(_provider.Options.AgentRuntimeBaseUrl, _provider.Options.Region, "bedrock-agent-runtime") + "/rerank");
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        if (call.Headers != null)
        {
            foreach (var header in call.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        var payload = Encoding.UTF8.GetBytes(json);
        var accessKey = _provider.Options.AccessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = _provider.Options.SecretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        var token = _provider.Options.SessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        AwsSigV4.Sign(request, payload, _provider.Options.Region, "bedrock", accessKey!, secret!, token, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        using var parsed = JsonDocument.Parse(text);
        var root = parsed.RootElement;
        var ranking = new List<Operations.RerankModelRank>();
        if (root.TryGetProperty("results", out var results))
        {
            foreach (var result in results.EnumerateArray())
            {
                var index = result.TryGetProperty("index", out var indexElement) ? indexElement.GetInt32() : 0;
                var score = result.TryGetProperty("relevanceScore", out var scoreElement) ? scoreElement.GetDouble() : 0;
                ranking.Add(new Operations.RerankModelRank(index, score));
            }
        }

        return new Operations.RerankModelResponse(ranking, response: new Operations.ProviderResponse(JsonStreams.ExtractResponseHeaders(response), root.Clone()));
    }

    private static RerankResult ToResult(Operations.RerankModelResponse response)
    {
        return new RerankResult(response.Ranking.Select(rank => new RerankItem(rank.Index, rank.RelevanceScore)).ToList());
    }

    private static RerankOptions ReadOptions(JsonElement? providerOptions)
    {
        if (providerOptions is not { } root || root.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        JsonElement provider;
        if (!root.TryGetProperty("amazonBedrock", out provider) && !root.TryGetProperty("bedrock", out provider))
        {
            return default;
        }

        if (provider.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        string? nextToken = provider.TryGetProperty("nextToken", out var token) && token.ValueKind == JsonValueKind.String ? token.GetString() : null;
        JsonNode? fields = provider.TryGetProperty("additionalModelRequestFields", out var extra) && extra.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(extra.GetRawText())
            : null;
        return new RerankOptions(nextToken, fields);
    }

    private readonly struct RerankOptions
    {
        public RerankOptions(string? nextToken, JsonNode? additionalModelRequestFields)
        {
            NextToken = nextToken;
            AdditionalModelRequestFields = additionalModelRequestFields;
        }

        public string? NextToken { get; }

        public JsonNode? AdditionalModelRequestFields { get; }
    }
}
