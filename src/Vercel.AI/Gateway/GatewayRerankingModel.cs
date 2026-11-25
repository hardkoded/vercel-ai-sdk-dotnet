// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>Reranks documents through the Gateway <c>/reranking-model</c> route. Maps to <c>GatewayRerankingModel</c>.</summary>
public sealed class GatewayRerankingModel : IRerankingModel, IRerankCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public GatewayRerankingModel(GatewayProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<Provider.RerankResult> DoRerankAsync(
        string query,
        IReadOnlyList<string> documents,
        int? topN,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        var call = new RerankModelCall(
            new RerankModelDocuments("text", (documents ?? []).Cast<object?>().ToList()),
            query,
            topN,
            null,
            ProviderExchange.ToRequestHeaders(headers),
            cancellationToken
        );
        var response = await DoRerankAsync(call, cancellationToken).ConfigureAwait(false);
        return new Provider.RerankResult(
            response
                .Ranking.Select(rank => new RerankItem(rank.Index, rank.RelevanceScore))
                .ToList()
        );
    }

    /// <summary>Reranks text or object documents.</summary>
    public async Task<RerankModelResponse> DoRerankAsync(
        RerankModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var values = new JsonArray();
        foreach (var document in call.Documents.Values)
        {
            values.Add(JsonSerializer.SerializeToNode(document));
        }

        var body = new JsonObject
        {
            ["documents"] = new JsonObject { ["type"] = call.Documents.Type, ["values"] = values },
            ["query"] = call.Query,
        };
        if (call.TopN is { } topN)
        {
            body["topN"] = topN;
        }

        if (call.ProviderOptions is { ValueKind: JsonValueKind.Object } options)
        {
            body["providerOptions"] = JsonNode.Parse(options.GetRawText());
        }

        var headers = new Dictionary<string, string?>();
        if (call.Headers != null)
        {
            foreach (var pair in call.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        ProviderTextResponse response;
        var requestHeaders = await _provider
            .HeadersAsync(
                "ai-reranking-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        try
        {
            response = await _provider
                .Http.SendJsonStringAsync(
                    HttpMethod.Post,
                    _provider.Route("reranking-model"),
                    body.ToJsonString(),
                    requestHeaders,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders);
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders);
        }
        catch (ApiConnectionException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders);
        }

        JsonElement root;
        var ranking = new List<RerankModelRank>();
        try
        {
            using var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            );
            root = document.RootElement.Clone();
            foreach (var rank in root.GetProperty("ranking").EnumerateArray())
            {
                ranking.Add(
                    new RerankModelRank(
                        rank.GetProperty("index").GetInt32(),
                        rank.GetProperty("relevanceScore").GetDouble()
                    )
                );
            }
        }
        catch (Exception exception)
            when (exception is JsonException
                || exception is KeyNotFoundException
                || exception is InvalidOperationException
                || exception is FormatException
            )
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid reranking model response: " + exception.Message
            );
        }

        var warnings = new List<OperationWarning>();
        if (
            root.TryGetProperty("warnings", out var warningList)
            && warningList.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var warning in warningList.EnumerateArray())
            {
                warnings.Add(GatewayWarningSchema.Parse(warning));
            }
        }

        JsonElement? providerMetadata = null;
        if (
            root.TryGetProperty("providerMetadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
        )
        {
            providerMetadata = metadata;
        }

        return new RerankModelResponse(
            ranking,
            warnings,
            providerMetadata,
            new ProviderResponse(response.Headers, root)
        );
    }
}
