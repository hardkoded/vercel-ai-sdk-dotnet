// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.MiniMax;

/// <summary>MiniMax provider. OpenAI Chat Completions compatible at <c>https://api.minimax.io/v1</c>.</summary>
public sealed class MiniMaxProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "minimax";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.minimax.io/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "MINIMAX_API_KEY";

    /// <summary>Creates a provider.</summary>
    public MiniMaxProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new MiniMaxProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new MiniMaxProvider(client, options);
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

/// <summary>Registers <see cref="MiniMaxProvider"/>.</summary>
public static class MiniMaxServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddMiniMax(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(MiniMaxProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new MiniMaxProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(MiniMaxProvider.ProviderId), options);
        });
        return services;
    }
}
