// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock Agent Runtime rerank model.</summary>
public sealed class AmazonBedrockRerankingModel : IRerankingModel
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
        var sources = new JsonArray();
        foreach (var document in documents)
        {
            sources.Add(new JsonObject
            {
                ["type"] = "INLINE",
                ["inlineDocumentSource"] = new JsonObject
                {
                    ["type"] = "TEXT",
                    ["textDocument"] = new JsonObject { ["text"] = document },
                },
            });
        }

        return await PostAsync(query, sources, topN, providerOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reranks JSON documents. Each value is sent as an inline JSON document.</summary>
    public async Task<RerankResult> DoRerankObjectsAsync(string query, IReadOnlyList<JsonElement> documents, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
    {
        var sources = new JsonArray();
        foreach (var document in documents)
        {
            sources.Add(new JsonObject
            {
                ["type"] = "INLINE",
                ["inlineDocumentSource"] = new JsonObject
                {
                    ["type"] = "JSON",
                    ["jsonDocument"] = JsonNode.Parse(document.GetRawText()),
                },
            });
        }

        return await PostAsync(query, sources, topN, providerOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<RerankResult> PostAsync(string query, JsonArray sources, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
    {
        var options = ReadOptions(providerOptions);
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
        if (topN is { } count)
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
                    ["textQuery"] = new JsonObject { ["text"] = query },
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

        using var document = JsonDocument.Parse(text);
        var items = new List<RerankItem>();
        if (document.RootElement.TryGetProperty("results", out var results))
        {
            foreach (var result in results.EnumerateArray())
            {
                var index = result.TryGetProperty("index", out var indexElement) ? indexElement.GetInt32() : 0;
                var score = result.TryGetProperty("relevanceScore", out var scoreElement) ? scoreElement.GetDouble() : 0;
                items.Add(new RerankItem(index, score));
            }
        }

        return new RerankResult(items);
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
