// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.HeyGen;

/// <summary>HeyGen provider. It supports video generation only.</summary>
public sealed class HeyGenProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "heygen";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.heygen.com";

    internal readonly HttpClient _httpClient;

    /// <summary>Creates a provider. The API key is read when a model is used, not here.</summary>
    public HeyGenProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new HeyGenProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new HeyGenProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Provider 'heygen' does not support language model '" + modelId + "'.");
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new HeyGenVideoModel(this, modelId);

    /// <summary>Creates a video model. Same as <see cref="VideoModel"/>.</summary>
    public IVideoModel Video(string modelId) => VideoModel(modelId);

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl.TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "HEYGEN_API_KEY";
        options.RequireApiKey = true;
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-api-key";
        options.UserAgent = ProviderExchange.UserAgent("heygen");
        return options;
    }
}

/// <summary>Registers HeyGen.</summary>
public static class HeyGenServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddHeyGen(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(HeyGenProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new HeyGenProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HeyGenProvider.ProviderId), options);
        });
        return services;
    }
}
