// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>
/// Perplexity provider. Language generation uses the Agent API at <c>{base}/v1/agent</c>.
/// Embeddings stay on the OpenAI-compatible embeddings route. Preset ids are
/// <c>fast</c>, <c>low</c>, <c>medium</c>, <c>high</c>, and <c>xhigh</c>; any other id is sent as a model id.
/// Legacy Sonar model ids are not aliased and Sonar provider options are not translated.
/// Sonar PDF input, video input, and image or video results have no Agent API equivalent.
/// </summary>
public sealed class PerplexityProvider : OpenAICompatibleProvider
{
    private readonly HttpClient _httpClient;

    /// <summary>Provider id.</summary>
    public const string ProviderId = "perplexity";

    /// <summary>Default API origin. Language calls append <c>/v1/agent</c>.</summary>
    public const string DefaultBaseUrl = "https://api.perplexity.ai";

    /// <summary>Relative Agent API path.</summary>
    public const string AgentPath = "v1/agent";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "PERPLEXITY_API_KEY";

    /// <summary>Header Perplexity uses to attribute SDK requests.</summary>
    public const string IntegrationHeader = "X-Pplx-Integration";

    /// <summary>Default <see cref="IntegrationHeader"/> value.</summary>
    public const string DefaultIntegration = "vercel-ai-sdk";

    /// <summary>Creates a provider.</summary>
    public PerplexityProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new PerplexityProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new PerplexityProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new PerplexityLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new PerplexityEmbeddingModel(this, modelId);
    }

    /// <summary>
    /// Sonar chat-completions model. <see cref="LanguageModel"/> uses the Agent API.
    /// This client is the compatibility-snapshot chat route for citations, PDF files, and image results.
    /// </summary>
    public PerplexityChatLanguageModel ChatLanguageModel(string modelId)
    {
        return new PerplexityChatLanguageModel(this, modelId ?? throw new ArgumentNullException(nameof(modelId)));
    }

    /// <summary>HTTP client passed to the constructor. Chat streaming reads its response headers.</summary>
    internal HttpClient HttpClient => _httpClient;

    /// <summary>Agent API URL. A custom base URL is prefixed to <c>v1/agent</c>.</summary>
    public Uri AgentUri()
    {
        return ApiKeys.Combine(Options.BaseUrl, AgentPath);
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

        options.SupportsEmbeddings = true;
        options.SupportsImages = false;
        if (!ContainsHeader(options.Headers, IntegrationHeader))
        {
            options.Headers[IntegrationHeader] = DefaultIntegration;
        }

        return options;
    }

    private static bool ContainsHeader(Dictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Registers <see cref="PerplexityProvider"/>.</summary>
public static class PerplexityServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddPerplexity(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(PerplexityProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new PerplexityProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(PerplexityProvider.ProviderId), options);
        });
        return services;
    }
}
