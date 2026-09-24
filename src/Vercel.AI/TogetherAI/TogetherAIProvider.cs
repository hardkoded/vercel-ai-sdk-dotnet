// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.TogetherAI;

/// <summary>TogetherAI provider. OpenAI Chat Completions compatible at <c>https://api.together.xyz/v1</c>.</summary>
public sealed class TogetherAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "togetherai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.together.xyz/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "TOGETHER_AI_API_KEY";

    /// <summary>Creates a provider.</summary>
    public TogetherAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new TogetherAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new TogetherAIProvider(client, options);
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
        options.AdditionalApiKeyEnvironmentVariables = new[] { "TOGETHER_API_KEY" };
        return options;
    }
}

/// <summary>Registers <see cref="TogetherAIProvider"/>.</summary>
public static class TogetherAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddTogetherAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(TogetherAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new TogetherAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(TogetherAIProvider.ProviderId), options);
        });
        return services;
    }
}
