// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Voyage;

/// <summary>Optional Voyage embedding fields.</summary>
public sealed class VoyageEmbeddingRequest
{
    /// <summary>Creates a request for <paramref name="values"/>.</summary>
    public VoyageEmbeddingRequest(IReadOnlyList<string> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>Texts to embed.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary><c>input_type</c>, when set.</summary>
    public string? InputType { get; set; }

    /// <summary><c>output_dimension</c>, when set.</summary>
    public int? OutputDimension { get; set; }

    /// <summary><c>truncation</c>, when set.</summary>
    public bool? Truncation { get; set; }

    /// <summary><c>output_dtype</c>, when set.</summary>
    public string? OutputDtype { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Voyage embedding result, including the raw response.</summary>
public sealed class VoyageEmbeddingResult
{
    /// <summary>Creates an embedding result.</summary>
    public VoyageEmbeddingResult(IReadOnlyList<float[]> embeddings, int tokens, IReadOnlyDictionary<string, string> headers, JsonElement body)
    {
        Embeddings = embeddings ?? Array.Empty<float[]>();
        Tokens = tokens;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>Vectors in input order.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Token count. Zero when the provider omits usage.</summary>
    public int Tokens { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response body.</summary>
    public JsonElement Body { get; }
}

/// <summary>A compatibility warning from a Voyage rerank call.</summary>
public sealed class VoyageWarning
{
    /// <summary>Creates a warning.</summary>
    public VoyageWarning(string type, string feature, string details)
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

/// <summary>Voyage rerank request.</summary>
public sealed class VoyageRerankRequest
{
    /// <summary>Query text.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Plain-text documents.</summary>
    public IReadOnlyList<string>? TextDocuments { get; set; }

    /// <summary>JSON documents. They are stringified before they are sent.</summary>
    public IReadOnlyList<JsonObject>? ObjectDocuments { get; set; }

    /// <summary><c>top_k</c>, when set.</summary>
    public int? TopK { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Voyage rerank result.</summary>
public sealed class VoyageRerankResult
{
    /// <summary>Creates a rerank result.</summary>
    public VoyageRerankResult(IReadOnlyList<RerankItem> ranking, IReadOnlyList<VoyageWarning> warnings, IReadOnlyDictionary<string, string> headers, JsonElement body)
    {
        Ranking = ranking ?? Array.Empty<RerankItem>();
        Warnings = warnings ?? Array.Empty<VoyageWarning>();
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>Documents in provider order.</summary>
    public IReadOnlyList<RerankItem> Ranking { get; }

    /// <summary>Warnings. Object documents produce one compatibility warning.</summary>
    public IReadOnlyList<VoyageWarning> Warnings { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response body.</summary>
    public JsonElement Body { get; }
}

/// <summary>Voyage embeddings and reranking.</summary>
public sealed class VoyageProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "voyage";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.voyageai.com/v1";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public VoyageProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new VoyageProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new VoyageProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Provider 'voyage' does not support language model '" + modelId + "'.");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw new AiSdkException("Provider 'voyage' does not support image model '" + modelId + "'.");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new VoyageEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return new VoyageRerankingModel(this, modelId);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "VOYAGE_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("voyage");
        return options;
    }

    /// <summary>Voyage embedding model.</summary>
    public sealed class VoyageEmbeddingModel : IEmbeddingModel
    {
        private readonly VoyageProvider _provider;

        /// <summary>Creates an embedding model.</summary>
        public VoyageEmbeddingModel(VoyageProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <summary>Maximum values in one call.</summary>
        public int MaxEmbeddingsPerCall
        {
            get { return 128; }
        }

        /// <inheritdoc />
        public string SpecificationVersion
        {
            get { return "V4"; }
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "voyage.embedding"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null)
        {
            var outputDimension = providerOptions != null
                && providerOptions.TryGetValue("voyage", out var options)
                && options.ValueKind == JsonValueKind.Object
                && options.TryGetProperty("outputDimension", out var width)
                && width.ValueKind == JsonValueKind.Number
                && width.TryGetInt32(out var parsed)
                ? parsed
                : dimensions;
            var result = await EmbedAsync(new VoyageEmbeddingRequest(values) { OutputDimension = outputDimension }, cancellationToken).ConfigureAwait(false);
            return new EmbeddingResult(result.Embeddings, result.Tokens);
        }

        /// <summary>Embeds values and returns usage plus the raw response.</summary>
        public async Task<VoyageEmbeddingResult> EmbedAsync(VoyageEmbeddingRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Values.Count > MaxEmbeddingsPerCall)
            {
                throw new AiSdkException("Too many embedding values for one Voyage call. The maximum is " + MaxEmbeddingsPerCall.ToString() + ".");
            }

            var input = new JsonArray();
            foreach (var value in request.Values)
            {
                input.Add(value);
            }

            var body = new JsonObject { ["input"] = input, ["model"] = ModelId };
            if (request.InputType != null)
            {
                body["input_type"] = request.InputType;
            }

            if (request.OutputDimension is { } dimension)
            {
                body["output_dimension"] = dimension;
            }

            if (request.Truncation is { } truncation)
            {
                body["truncation"] = truncation;
            }

            if (request.OutputDtype != null)
            {
                body["output_dtype"] = request.OutputDtype;
            }

            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
            ProviderExchange.AddUserAgent(headers, "voyage");
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/embeddings"),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
            {
                var vectors = new List<float[]>();
                var indexed = new List<KeyValuePair<int, float[]>>();
                if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    var position = 0;
                    foreach (var item in data.EnumerateArray())
                    {
                        var index = item.TryGetProperty("index", out var indexValue) && indexValue.TryGetInt32(out var parsed) ? parsed : position;
                        indexed.Add(new KeyValuePair<int, float[]>(index, ReadVector(item.GetProperty("embedding"))));
                        position++;
                    }

                    indexed.Sort(delegate (KeyValuePair<int, float[]> left, KeyValuePair<int, float[]> right)
                    {
                        return left.Key.CompareTo(right.Key);
                    });
                    foreach (var pair in indexed)
                    {
                        vectors.Add(pair.Value);
                    }
                }

                var tokens = 0;
                if (document.RootElement.TryGetProperty("usage", out var usage)
                    && usage.TryGetProperty("total_tokens", out var total)
                    && total.TryGetInt32(out var count))
                {
                    tokens = count;
                }

                return new VoyageEmbeddingResult(vectors, tokens, response.Headers, document.RootElement.Clone());
            }
        }

        private static float[] ReadVector(JsonElement embedding)
        {
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            return vector;
        }
    }

    /// <summary>Voyage reranking model.</summary>
    public sealed class VoyageRerankingModel : IRerankingModel
    {
        private readonly VoyageProvider _provider;

        /// <summary>Creates a reranking model.</summary>
        public VoyageRerankingModel(VoyageProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "voyage.reranking"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
        {
            var result = await RerankAsync(new VoyageRerankRequest { Query = query, TextDocuments = documents, TopK = topN }, cancellationToken).ConfigureAwait(false);
            return new RerankResult(result.Ranking);
        }

        /// <summary>Reranks text or JSON documents.</summary>
        public async Task<VoyageRerankResult> RerankAsync(VoyageRerankRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var warnings = new List<VoyageWarning>();
            var documents = new JsonArray();
            if (request.ObjectDocuments != null)
            {
                warnings.Add(new VoyageWarning("compatibility", "object documents", "Object documents are converted to strings."));
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
                ["query"] = request.Query,
                ["documents"] = documents,
                ["model"] = ModelId,
            };
            if (request.TopK is { } top)
            {
                body["top_k"] = top;
            }

            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
            ProviderExchange.AddUserAgent(headers, "voyage");
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/rerank"),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
            {
                var ranking = new List<RerankItem>();
                if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        var score = item.TryGetProperty("relevance_score", out var relevance) ? relevance.GetDouble() : 0;
                        ranking.Add(new RerankItem(item.GetProperty("index").GetInt32(), score));
                    }
                }

                return new VoyageRerankResult(ranking, warnings, response.Headers, document.RootElement.Clone());
            }
        }
    }
}

/// <summary>Registers Voyage.</summary>
public static class VoyageServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddVoyage(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(VoyageProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new VoyageProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(VoyageProvider.ProviderId), options);
        });
        return services;
    }
}
