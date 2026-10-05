// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepInfra;

/// <summary>DeepInfra provider. OpenAI Chat Completions compatible at <c>https://api.deepinfra.com/v1/openai</c>.</summary>
public sealed class DeepInfraProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "deepinfra";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.deepinfra.com/v1/openai";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "DEEPINFRA_API_KEY";

    /// <summary>Creates a provider.</summary>
    public DeepInfraProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new DeepInfraImageModel(this, modelId);
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new DeepInfraProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new DeepInfraProvider(client, options);
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
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        options.SupportsStructuredOutputs = true;
        options.SelectErrorMessage = DeepInfraImageModel.ReadError;
        if (string.IsNullOrEmpty(options.ImageBaseUrl))
        {
            options.ImageBaseUrl = options.BaseUrl == DefaultBaseUrl
                ? "https://api.deepinfra.com/v1/inference"
                : options.BaseUrl.TrimEnd('/') + "/inference";
        }

        return options;
    }
}

/// <summary>Registers <see cref="DeepInfraProvider"/>.</summary>
public static class DeepInfraServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddDeepInfra(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(DeepInfraProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new DeepInfraProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(DeepInfraProvider.ProviderId), options);
        });
        return services;
    }
}
