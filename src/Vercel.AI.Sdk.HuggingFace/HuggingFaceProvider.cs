// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.HuggingFace;

/// <summary>HuggingFace provider. OpenAI Chat Completions compatible at <c>https://router.huggingface.co/v1</c>.</summary>
public sealed class HuggingFaceProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "huggingface";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://router.huggingface.co/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "HUGGINGFACE_API_KEY";

    /// <summary>Creates a provider.</summary>
    public HuggingFaceProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new HuggingFaceProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new HuggingFaceProvider(client, options);
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

/// <summary>Registers <see cref="HuggingFaceProvider"/>.</summary>
public static class HuggingFaceServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddHuggingFace(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(HuggingFaceProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new HuggingFaceProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HuggingFaceProvider.ProviderId), options);
        });
        return services;
    }
}
