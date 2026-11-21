// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Parallel;

/// <summary>Parallel provider settings. Maps to <c>ParallelProviderSettings</c>.</summary>
public sealed class ParallelProviderSettings
{
    /// <summary>API key. Defaults to the <c>PARALLEL_API_KEY</c> environment variable, read when a request is made.</summary>
    public string? ApiKey { get; set; }

    /// <summary>API base URL. Defaults to <c>https://api.parallel.ai/v1</c>.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Headers sent with every request.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Parallel's experimental web-grounded decision capability. It has no language, embedding, or image models. Maps to <c>ParallelProvider</c>.</summary>
public sealed class ParallelProvider
{
    /// <summary>Provider id of the decision model.</summary>
    public const string DecisionProviderId = "parallel.decision";

    /// <summary>Default API base URL.</summary>
    public const string DefaultBaseUrl = "https://api.parallel.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyEnvironmentVariable = "PARALLEL_API_KEY";

    private static readonly Lazy<ParallelProvider> DefaultProvider = new(() =>
        new ParallelProvider()
    );

    private readonly ParallelProviderSettings _options;
    private readonly HttpClient? _httpClient;
    private readonly string _baseUrl;

    /// <summary>Creates a provider. Maps to <c>createParallel</c>.</summary>
    public ParallelProvider(ParallelProviderSettings? options = null, HttpClient? httpClient = null)
    {
        _options = options ?? new ParallelProviderSettings();
        _httpClient = httpClient;
        _baseUrl = string.IsNullOrEmpty(_options.BaseUrl)
            ? DefaultBaseUrl
            : _options.BaseUrl!.TrimEnd('/');
    }

    /// <summary>Provider that reads <c>PARALLEL_API_KEY</c> when a request is made. Maps to <c>parallel</c>.</summary>
    public static ParallelProvider Default => DefaultProvider.Value;

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Creates a provider. Pass a handler from tests. Maps to <c>createParallel</c>.</summary>
    public static ParallelProvider Create(
        ParallelProviderSettings? options = null,
        HttpMessageHandler? handler = null
    )
    {
        return new ParallelProvider(
            options,
            handler is null ? null : new HttpClient(handler, disposeHandler: false)
        );
    }

    /// <summary>Creates a web-grounded decision model. The id may be a mode this library does not know yet.</summary>
    public DecisionParallelModel DecisionModel(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            throw new NoSuchModelError(modelId ?? string.Empty, "decisionModel");
        }

        return new DecisionParallelModel(
            modelId,
            new ParallelDecisionModelConfig
            {
                Provider = DecisionProviderId,
                BaseUrl = _baseUrl,
                Headers = Headers,
                HttpClient = _httpClient,
            }
        );
    }

    /// <summary>Parallel has no language models.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <summary>Parallel has no embedding models.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>Parallel has no image models.</summary>
    public IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }

    private IReadOnlyDictionary<string, string> Headers()
    {
        var headers = new Dictionary<string, string>
        {
            ["x-api-key"] = ApiKeys.Require(_options.ApiKey, ApiKeyEnvironmentVariable),
        };
        if (_options.Headers != null)
        {
            foreach (var pair in _options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        headers["User-Agent"] = ProviderExchange.UserAgent("parallel");
        return headers;
    }
}

/// <summary>Registers Parallel.</summary>
public static class ParallelServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ParallelProvider"/>.</summary>
    public static IServiceCollection AddParallel(
        this IServiceCollection services,
        Action<ParallelProviderSettings>? configure = null
    )
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(nameof(ParallelProvider));
        services.AddSingleton(sp =>
        {
            var options = new ParallelProviderSettings();
            configure?.Invoke(options);
            return new ParallelProvider(
                options,
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ParallelProvider))
            );
        });
        return services;
    }
}
