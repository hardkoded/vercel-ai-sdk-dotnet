// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.ScaleDown;

/// <summary>ScaleDown provider settings. Maps to <c>ScaleDownProviderSettings</c>.</summary>
public sealed class ScaleDownProviderSettings
{
    /// <summary>API key. Defaults to the <c>SCALEDOWN_API_KEY</c> environment variable, read when a request is made.</summary>
    public string? ApiKey { get; set; }

    /// <summary>API base URL. Defaults to <c>https://api.scaledown.xyz</c>.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Headers sent with every request.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>ScaleDown's experimental decision capability. It has no language, embedding, or image models.</summary>
public sealed class ScaleDownProvider
{
    /// <summary>Provider id of the decision model.</summary>
    public const string DecisionProviderId = "scaledown.decision";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.scaledown.xyz";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyEnvironmentVariable = "SCALEDOWN_API_KEY";

    private static readonly Lazy<ScaleDownProvider> DefaultProvider = new(() => new ScaleDownProvider());

    private readonly ScaleDownProviderSettings _options;
    private readonly HttpClient? _httpClient;
    private readonly string _baseUrl;

    /// <summary>Creates a provider. Maps to <c>createScaleDown</c>.</summary>
    public ScaleDownProvider(ScaleDownProviderSettings? options = null, HttpClient? httpClient = null)
    {
        _options = options ?? new ScaleDownProviderSettings();
        _httpClient = httpClient;
        _baseUrl = string.IsNullOrEmpty(_options.BaseUrl) ? DefaultBaseUrl : _options.BaseUrl!.TrimEnd('/');
    }

    /// <summary>Provider that reads <c>SCALEDOWN_API_KEY</c> when a request is made. Maps to <c>scaledown</c> and <c>scaleDown</c>.</summary>
    public static ScaleDownProvider Default => DefaultProvider.Value;

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Creates a provider. Pass a handler from tests. Maps to <c>createScaleDown</c>.</summary>
    public static ScaleDownProvider Create(ScaleDownProviderSettings? options = null, HttpMessageHandler? handler = null)
    {
        return new ScaleDownProvider(options, handler is null ? null : new HttpClient(handler, disposeHandler: false));
    }

    /// <summary>Creates a model for ScaleDown's experimental Decisions API. The id may be one this library does not know yet.</summary>
    public DecisionScaleDownModel DecisionModel(string modelId)
    {
        return new DecisionScaleDownModel(modelId, new ScaleDownDecisionModelConfig
        {
            Provider = DecisionProviderId,
            BaseUrl = _baseUrl,
            Headers = Headers,
            HttpClient = _httpClient,
        });
    }

    /// <summary>ScaleDown has no language models.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <summary>ScaleDown has no embedding models.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>ScaleDown has no image models.</summary>
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

        headers["User-Agent"] = ProviderExchange.UserAgent("scaledown");
        return headers;
    }
}

/// <summary>Registers ScaleDown.</summary>
public static class ScaleDownServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ScaleDownProvider"/>.</summary>
    public static IServiceCollection AddScaleDown(this IServiceCollection services, Action<ScaleDownProviderSettings>? configure = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(nameof(ScaleDownProvider));
        services.AddSingleton(sp =>
        {
            var options = new ScaleDownProviderSettings();
            configure?.Invoke(options);
            return new ScaleDownProvider(options, sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ScaleDownProvider)));
        });
        return services;
    }
}
