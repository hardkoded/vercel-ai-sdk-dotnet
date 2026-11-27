// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Alibaba;

/// <summary>
/// DashScope text-embedding model at <c>{embeddingBaseUrl}/services/embeddings/text-embedding/text-embedding</c>.
/// Maps to <c>AlibabaEmbeddingModel</c>. With <c>outputType: 'dense&amp;sparse'</c> the sparse vectors are returned in
/// <c>providerMetadata.alibaba.sparseEmbeddings</c>.
/// </summary>
public sealed class AlibabaEmbeddingModel : IEmbeddingModel, IEmbeddingCaller
{
    private const int MaxValuesPerCall = 10;

    /// <summary>Creates an embedding model.</summary>
    public AlibabaEmbeddingModel(string modelId, AlibabaConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="IEmbeddingCaller.Provider" />
    public string Provider => Config.Provider;

    /// <inheritdoc cref="IEmbeddingCaller.ModelId" />
    public string ModelId { get; }

    /// <summary>Maximum values in one call: 10 for the Alibaba text embedding models.</summary>
    public int? MaxEmbeddingsPerCall => MaxValuesPerCall;

    /// <inheritdoc />
    public double? MaxInputBytesPerCall => null;

    /// <summary>False. Alibaba text embedding calls run one after the other.</summary>
    public bool SupportsParallelCalls => false;

    /// <summary>The configuration the provider passed in.</summary>
    internal AlibabaConfig Config { get; }

    /// <inheritdoc />
    public Task<JsonElement?> TransformProviderOptionsAsync(
        JsonElement? providerOptions,
        IReadOnlyList<string> values,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<JsonElement?>(null);
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        var response = await DoEmbedAsync(
                new EmbeddingModelCall(
                    values,
                    ProviderExchange.ToRequestHeaders(headers) ?? new Dictionary<string, string>(),
                    providerOptions == null
                        ? null
                        : JsonSerializer.SerializeToElement(providerOptions),
                    cancellationToken,
                    dimensions
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var embedding in response.Embeddings)
        {
            vectors.Add(Array.ConvertAll(embedding, value => (float)value));
        }

        return new EmbeddingResult(
            vectors,
            double.IsNaN(response.Tokens) ? null : (int)response.Tokens,
            new List<CallWarning>(),
            new EmbeddingResponseInfo(
                response.Response?.Headers,
                response.Response?.Body?.GetRawText()
            )
        );
    }

    /// <inheritdoc />
    public async Task<EmbeddingModelResponse> DoEmbedAsync(
        EmbeddingModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (call.Values.Count > MaxValuesPerCall)
        {
            throw new TooManyEmbeddingValuesForCallException(
                Provider,
                ModelId,
                MaxValuesPerCall,
                call.Values
            );
        }

        var alibabaOptions = AlibabaEmbeddingModelOptions.Parse(call.ProviderOptions);

        if (string.Equals(alibabaOptions?.OutputType, "sparse", StringComparison.Ordinal))
        {
            throw new UnsupportedFunctionalityException(
                "Alibaba embedding outputType 'sparse'",
                "Alibaba embedding outputType 'sparse' is not supported because AI SDK embeddings require dense number arrays. Use 'dense' or 'dense&sparse' instead."
            );
        }

        var texts = new JsonArray();
        foreach (var value in call.Values)
        {
            texts.Add(value);
        }

        var parameters = new JsonObject();
        if (alibabaOptions?.TextType != null)
        {
            parameters["text_type"] = alibabaOptions.TextType;
        }

        if ((alibabaOptions?.Dimension ?? call.Dimensions) is { } dimension)
        {
            parameters["dimension"] = dimension;
        }

        if (alibabaOptions?.OutputType != null)
        {
            parameters["output_type"] = alibabaOptions.OutputType;
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = new JsonObject { ["texts"] = texts },
            ["parameters"] = parameters,
        };
        var headers = ProviderExchange.Merge(
            await Config.Headers(cancellationToken).ConfigureAwait(false),
            call.Headers
        );
        var response = await ProviderExchange
            .SendAsync(
                Config.HttpClient,
                HttpMethod.Post,
                ApiKeys.Combine(
                    Config.BaseUrl,
                    "/services/embeddings/text-embedding/text-embedding"
                ),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var items = new List<KeyValuePair<int, JsonElement>>();
        foreach (var item in root.GetProperty("output").GetProperty("embeddings").EnumerateArray())
        {
            items.Add(
                new KeyValuePair<int, JsonElement>(item.GetProperty("text_index").GetInt32(), item)
            );
        }

        // List.Sort is not stable; OrderBy keeps the response order of equal indexes like Array.prototype.sort.
        var sorted = items.OrderBy(pair => pair.Key).ToList();
        var embeddings = new List<double[]>();
        var sparseEmbeddings = new JsonArray();
        foreach (var pair in sorted)
        {
            embeddings.Add(ReadVector(pair.Value.GetProperty("embedding")));
            if (
                pair.Value.TryGetProperty("sparse_embedding", out var sparse)
                && sparse.ValueKind == JsonValueKind.Array
            )
            {
                sparseEmbeddings.Add(
                    new JsonObject
                    {
                        ["textIndex"] = pair.Key,
                        ["sparseEmbedding"] = ReadSparseEmbedding(sparse),
                    }
                );
            }
        }

        double? tokens = null;
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            tokens = usage.GetProperty("total_tokens").GetDouble();
        }

        JsonElement? providerMetadata =
            sparseEmbeddings.Count > 0
                ? JsonValues.ParseOrEmpty(
                    new JsonObject
                    {
                        [AlibabaProvider.ProviderId] = new JsonObject
                        {
                            ["sparseEmbeddings"] = sparseEmbeddings,
                        },
                    }.ToJsonString()
                )
                : null;

        return new EmbeddingModelResponse(
            embeddings,
            tokens,
            providerMetadata: providerMetadata,
            response: new ProviderResponse(response.Headers, root.Clone()),
            usageOmitted: tokens is null
        );
    }

    private static double[] ReadVector(JsonElement embedding)
    {
        var vector = new double[embedding.GetArrayLength()];
        var index = 0;
        foreach (var number in embedding.EnumerateArray())
        {
            vector[index++] = number.GetDouble();
        }

        return vector;
    }

    private static JsonArray ReadSparseEmbedding(JsonElement sparse)
    {
        var entries = new JsonArray();
        foreach (var entry in sparse.EnumerateArray())
        {
            var copy = new JsonObject
            {
                ["index"] = entry.GetProperty("index").GetDouble(),
                ["value"] = entry.GetProperty("value").GetDouble(),
            };
            if (entry.TryGetProperty("token", out var token))
            {
                copy["token"] = token.ValueKind == JsonValueKind.Null ? null : token.GetString();
            }

            entries.Add(copy);
        }

        return entries;
    }
}
