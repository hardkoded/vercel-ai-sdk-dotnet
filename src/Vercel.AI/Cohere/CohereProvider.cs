// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>Cohere settings. The default origin is <c>https://api.cohere.com/v2</c>.</summary>
public sealed class CohereOptions
{
    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://api.cohere.com/v2";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Headers sent with every request. Call headers override them.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Cohere provider for chat, embeddings, and rerank.</summary>
public sealed class CohereProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "cohere";

    internal readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public CohereProvider(HttpClient httpClient, CohereOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new CohereOptions();
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Http = new ProviderHttp(_httpClient);
    }

    /// <summary>Options.</summary>
    public CohereOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static CohereProvider Create(CohereOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new CohereProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new CohereLanguageModel(this, modelId);

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId) => new CohereEmbeddingModel(this, modelId);

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId) => new CohereRerankingModel(this, modelId);

    internal Dictionary<string, string?> Headers(IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(Options.ApiKey, "COHERE_API_KEY"),
        };
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}

/// <summary>Cohere embedding model.</summary>
public sealed class CohereEmbeddingModel : IEmbeddingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public CohereEmbeddingModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken)
    {
        var embeddingType = EmbeddingType(providerOptions);
        var texts = new JsonArray();
        foreach (var value in values)
        {
            texts.Add(value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["texts"] = texts,
            ["embedding_types"] = new JsonArray(embeddingType),
            ["input_type"] = Option(providerOptions, "inputType")?.GetString() ?? "search_query",
        };
        if (Option(providerOptions, "truncate") is { } truncate)
        {
            body["truncate"] = truncate.GetString();
        }

        if (Option(providerOptions, "outputDimension") is { } outputDimension)
        {
            body["output_dimension"] = outputDimension.GetInt32();
        }

        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "embed"), body.ToJsonString(), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        var root = document.RootElement;
        var embeddings = root.GetProperty("embeddings");
        var vectorsElement = embeddings;
        var found = embeddings.ValueKind == JsonValueKind.Object ? embeddings.TryGetProperty(embeddingType, out vectorsElement) : embeddingType == "float";
        if (!found)
        {
            throw new AiSdkException("Cohere response has no " + embeddingType + " embeddings.");
        }

        // Integer and packed binary formats are returned as-is, not scaled or unpacked.
        var vectors = new List<float[]>();
        foreach (var embedding in vectorsElement.EnumerateArray())
        {
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                if (number.ValueKind != JsonValueKind.Number)
                {
                    throw new AiSdkException("Cohere " + embeddingType + " embeddings must contain only numbers.");
                }

                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        int? tokens = root.TryGetProperty("meta", out var meta) && meta.TryGetProperty("billed_units", out var billed) && billed.TryGetProperty("input_tokens", out var inputTokens) ? inputTokens.GetInt32() : null;
        return new EmbeddingResult(vectors, tokens);
    }

    private static string EmbeddingType(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions == null || !providerOptions.TryGetValue(CohereProvider.ProviderName, out var options) || options.ValueKind != JsonValueKind.Object || !options.TryGetProperty("embeddingType", out var value))
        {
            return "float";
        }

        var type = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        return type is "float" or "int8" or "uint8" or "binary" or "ubinary"
            ? type
            : throw new AiSdkException("invalid cohere provider options: embeddingType must be float, int8, uint8, binary, or ubinary.");
    }

    private static JsonElement? Option(IReadOnlyDictionary<string, JsonElement>? providerOptions, string name)
    {
        return providerOptions != null
            && providerOptions.TryGetValue(CohereProvider.ProviderName, out var options)
            && options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value
            : null;
    }
}

/// <summary>Cohere rerank model.</summary>
public sealed class CohereRerankingModel : IRerankingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public CohereRerankingModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var result = await RerankAsync(new CohereRerankRequest { Query = query, TextDocuments = documents, TopN = topN }, cancellationToken).ConfigureAwait(false);
        return new RerankResult(result.Ranking);
    }

    /// <summary>Reranks text or JSON documents and returns the response body.</summary>
    public async Task<CohereRerankResult> RerankAsync(CohereRerankRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var warnings = new List<CohereRerankWarning>();
        var documents = new JsonArray();
        if (request.ObjectDocuments != null)
        {
            warnings.Add(new CohereRerankWarning("compatibility", "object documents", "Object documents are converted to strings."));
            foreach (var document in request.ObjectDocuments)
            {
                documents.Add(document.ToJsonString());
            }
        }
        else if (request.TextDocuments != null)
        {
            foreach (var document in request.TextDocuments)
            {
                documents.Add(document);
            }
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

        if (request.MaxTokensPerDoc is { } maxTokens)
        {
            body["max_tokens_per_doc"] = maxTokens;
        }

        if (request.Priority is { } priority)
        {
            body["priority"] = priority;
        }

        var headers = ProviderExchange.Merge(_provider.Headers(), request.Headers);
        var response = await ProviderExchange.SendAsync(
            _provider._httpClient,
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "rerank"),
            ProviderExchange.Json(body.ToJsonString()),
            headers,
            cancellationToken).ConfigureAwait(false);
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
        {
            var ranking = new List<RerankItem>();
            if (document.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in results.EnumerateArray())
                {
                    var score = item.TryGetProperty("relevance_score", out var relevance) ? relevance.GetDouble() : 0;
                    ranking.Add(new RerankItem(item.GetProperty("index").GetInt32(), score));
                }
            }

            var id = document.RootElement.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String
                ? idValue.GetString()
                : null;
            return new CohereRerankResult(ranking, warnings, null, new CohereRerankResponse(id, response.Headers, document.RootElement.Clone()));
        }
    }
}

/// <summary>A compatibility warning from a Cohere rerank call.</summary>
public sealed class CohereRerankWarning
{
    /// <summary>Creates a warning.</summary>
    public CohereRerankWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning type.</summary>
    public string Type { get; }

    /// <summary>Feature name.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>Cohere rerank request.</summary>
public sealed class CohereRerankRequest
{
    /// <summary>Query text.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Plain-text documents.</summary>
    public IReadOnlyList<string>? TextDocuments { get; set; }

    /// <summary>JSON documents. They are stringified before they are sent.</summary>
    public IReadOnlyList<JsonObject>? ObjectDocuments { get; set; }

    /// <summary><c>top_n</c>, when set.</summary>
    public int? TopN { get; set; }

    /// <summary><c>max_tokens_per_doc</c>, when set.</summary>
    public int? MaxTokensPerDoc { get; set; }

    /// <summary><c>priority</c>, when set.</summary>
    public int? Priority { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Cohere rerank HTTP response.</summary>
public sealed class CohereRerankResponse
{
    /// <summary>Creates response metadata.</summary>
    public CohereRerankResponse(string? id, IReadOnlyDictionary<string, string> headers, JsonElement body)
    {
        Id = id;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>Response id, when the body includes one.</summary>
    public string? Id { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response body.</summary>
    public JsonElement Body { get; }
}

/// <summary>Cohere rerank result. Provider metadata stays unset.</summary>
public sealed class CohereRerankResult
{
    /// <summary>Creates a rerank result.</summary>
    public CohereRerankResult(IReadOnlyList<RerankItem> ranking, IReadOnlyList<CohereRerankWarning> warnings, JsonElement? providerMetadata, CohereRerankResponse response)
    {
        Ranking = ranking ?? Array.Empty<RerankItem>();
        Warnings = warnings ?? Array.Empty<CohereRerankWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>Documents in provider order.</summary>
    public IReadOnlyList<RerankItem> Ranking { get; }

    /// <summary>Warnings. Object documents produce one compatibility warning.</summary>
    public IReadOnlyList<CohereRerankWarning> Warnings { get; }

    /// <summary>Always null. The raw payload is <see cref="CohereRerankResponse.Body"/>.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response id, headers, and body.</summary>
    public CohereRerankResponse Response { get; }
}

/// <summary>Registers Cohere.</summary>
public static class CohereServiceCollectionExtensions
{
    /// <summary>Adds <see cref="CohereProvider"/>.</summary>
    public static IServiceCollection AddCohere(this IServiceCollection services, Action<CohereOptions>? configure = null)
    {
        services.AddHttpClient(CohereProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new CohereOptions();
            configure?.Invoke(options);
            return new CohereProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(CohereProvider.ProviderName), options);
        });
        return services;
    }
}
