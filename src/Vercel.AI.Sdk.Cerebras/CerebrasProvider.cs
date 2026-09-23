// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.Cerebras;

/// <summary>Cerebras provider. OpenAI Chat Completions compatible at <c>https://api.cerebras.ai/v1</c>.</summary>
public sealed class CerebrasProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "cerebras";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.cerebras.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "CEREBRAS_API_KEY";

    /// <summary>Creates a provider.</summary>
    public CerebrasProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new CerebrasProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new CerebrasProvider(client, options);
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
        
        return options;
    }
}

/// <summary>Registers <see cref="CerebrasProvider"/>.</summary>
public static class CerebrasServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddCerebras(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(CerebrasProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new CerebrasProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(CerebrasProvider.ProviderId), options);
        });
        return services;
    }
}
