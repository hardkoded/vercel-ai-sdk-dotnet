// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Fireworks;

/// <summary>Fireworks provider. OpenAI Chat Completions compatible at <c>https://api.fireworks.ai/inference/v1</c>.</summary>
public sealed class FireworksProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "fireworks";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.fireworks.ai/inference/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "FIREWORKS_API_KEY";

    /// <summary>Creates a provider.</summary>
    public FireworksProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new FireworksProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new FireworksProvider(client, options);
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
        options.SupportsImages = true;
        
        return options;
    }
}

/// <summary>Registers <see cref="FireworksProvider"/>.</summary>
public static class FireworksServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddFireworks(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(FireworksProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new FireworksProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(FireworksProvider.ProviderId), options);
        });
        return services;
    }
}
