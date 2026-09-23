// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.Perplexity;

/// <summary>Perplexity provider. OpenAI Chat Completions compatible at <c>https://api.perplexity.ai</c>.</summary>
public sealed class PerplexityProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "perplexity";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.perplexity.ai";

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
