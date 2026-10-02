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
    /// <summary>Provider id.</summary>
    public const string ProviderId = "perplexity";

    /// <summary>Default API origin. Language calls append <c>/v1/agent</c>.</summary>
    public const string DefaultBaseUrl = "https://api.perplexity.ai";

    /// <summary>Relative Agent API path.</summary>
    public const string AgentPath = "v1/agent";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "PERPLEXITY_API_KEY";

    /// <summary>Creates a provider.</summary>
    public PerplexityProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
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

        return options;
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
