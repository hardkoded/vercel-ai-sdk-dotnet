// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Alibaba;

/// <summary>Alibaba provider. OpenAI Chat Completions compatible at <c>https://dashscope-intl.aliyuncs.com/compatible-mode/v1</c>.</summary>
public sealed class AlibabaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "alibaba";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "ALIBABA_API_KEY";

    /// <summary>Native DashScope embedding origin.</summary>
    public const string EmbeddingBaseUrl = "https://dashscope-intl.aliyuncs.com/api/v1";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public AlibabaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new AlibabaEmbeddingModel(this, modelId);
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new AlibabaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AlibabaProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        if (string.IsNullOrEmpty(options.ProviderName) || options.ProviderName == "openai-compatible")
        {
            options.ProviderName = ProviderId;
        }

        if (options.BaseUrl == "https://api.openai.com/v1")
        {
            options.BaseUrl = DefaultBaseUrl;
        }

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY")
        {
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }

        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        if (options.AdditionalApiKeyEnvironmentVariables == null || options.AdditionalApiKeyEnvironmentVariables.Length == 0)
        {
            options.AdditionalApiKeyEnvironmentVariables = new[] { "DASHSCOPE_API_KEY" };
        }

        options.UserAgent = ProviderExchange.UserAgent("alibaba");
        return options;
    }

    /// <summary>DashScope text-embedding model.</summary>
    public sealed class AlibabaEmbeddingModel : IEmbeddingModel
    {
        private readonly AlibabaProvider _provider;

        /// <summary>Creates an embedding model.</summary>
        public AlibabaEmbeddingModel(AlibabaProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <summary>Maximum values in one call.</summary>
        public int MaxEmbeddingsPerCall
        {
            get { return 10; }
        }

        /// <inheritdoc />
        public string SpecificationVersion
        {
            get { return "v4"; }
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "alibaba.embedding"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken)
        {
            var result = await EmbedAsync(new AlibabaEmbeddingRequest(values), cancellationToken).ConfigureAwait(false);
            return new EmbeddingResult(result.Embeddings, result.Tokens);
        }

        /// <summary>Embeds texts with the native DashScope embedding route.</summary>
        public async Task<AlibabaEmbeddingResult> EmbedAsync(AlibabaEmbeddingRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Values.Count > MaxEmbeddingsPerCall)
            {
                throw new AiSdkException("Too many embedding values for one Alibaba call. The maximum is 10.");
            }

            if (string.Equals(request.OutputType, "sparse", StringComparison.Ordinal))
            {
                throw new AiSdkException("Alibaba embedding outputType 'sparse' is not supported because embeddings require dense number arrays. Use 'dense' or 'dense&sparse' instead.");
            }

            var texts = new JsonArray();
            foreach (var value in request.Values)
            {
                texts.Add(value);
            }

            var parameters = new JsonObject();
            if (request.TextType != null)
            {
                parameters["text_type"] = request.TextType;
            }

            if (request.Dimension is { } dimension)
            {
                parameters["dimension"] = dimension;
            }

            if (request.OutputType != null)
            {
                parameters["output_type"] = request.OutputType;
            }

            var body = new JsonObject
            {
                ["model"] = ModelId,
                ["input"] = new JsonObject { ["texts"] = texts },
                ["parameters"] = parameters,
            };
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(EmbeddingBaseUrl, "/services/embeddings/text-embedding/text-embedding"),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
            {
                var indexed = new List<KeyValuePair<int, float[]>>();
                if (document.RootElement.TryGetProperty("output", out var output)
                    && output.TryGetProperty("embeddings", out var embeddings)
                    && embeddings.ValueKind == JsonValueKind.Array)
                {
                    var position = 0;
                    foreach (var item in embeddings.EnumerateArray())
                    {
                        var index = item.TryGetProperty("text_index", out var indexValue) && indexValue.TryGetInt32(out var parsed) ? parsed : position;
                        indexed.Add(new KeyValuePair<int, float[]>(index, ReadVector(item.GetProperty("embedding"))));
                        position++;
                    }

                    indexed.Sort(delegate (KeyValuePair<int, float[]> left, KeyValuePair<int, float[]> right)
                    {
                        return left.Key.CompareTo(right.Key);
                    });
                }

                var vectors = new List<float[]>();
                foreach (var pair in indexed)
                {
                    vectors.Add(pair.Value);
                }

                var tokens = 0;
                if (document.RootElement.TryGetProperty("usage", out var usage)
                    && usage.TryGetProperty("total_tokens", out var total)
                    && total.TryGetInt32(out var count))
                {
                    tokens = count;
                }

                return new AlibabaEmbeddingResult(vectors, tokens, response.Headers);
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
}

/// <summary>Alibaba embedding request.</summary>
public sealed class AlibabaEmbeddingRequest
{
    /// <summary>Creates a request for <paramref name="values"/>.</summary>
    public AlibabaEmbeddingRequest(IReadOnlyList<string> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>Texts to embed.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary><c>parameters.text_type</c>, when set.</summary>
    public string? TextType { get; set; }

    /// <summary><c>parameters.dimension</c>, when set.</summary>
    public int? Dimension { get; set; }

    /// <summary><c>parameters.output_type</c>. <c>sparse</c> is rejected before the request.</summary>
    public string? OutputType { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Alibaba embedding result.</summary>
public sealed class AlibabaEmbeddingResult
{
    /// <summary>Creates an embedding result.</summary>
    public AlibabaEmbeddingResult(IReadOnlyList<float[]> embeddings, int tokens, IReadOnlyDictionary<string, string> headers)
    {
        Embeddings = embeddings ?? Array.Empty<float[]>();
        Tokens = tokens;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Vectors ordered by <c>text_index</c>.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Token count. Zero when usage is omitted.</summary>
    public int Tokens { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Registers <see cref="AlibabaProvider"/>.</summary>
public static class AlibabaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddAlibaba(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(AlibabaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new AlibabaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AlibabaProvider.ProviderId), options);
        });
        return services;
    }
}
