// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Prodia;

/// <summary>Prodia provider.</summary>
public sealed class ProdiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "prodia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://inference.prodia.com/v2";

    /// <summary>Creates a provider.</summary>
    public ProdiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>HTTP client used for job requests and image downloads.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Creates a provider.</summary>
    public static new ProdiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ProdiaProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new ProdiaLanguageModel(this, modelId);

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new ProdiaImageModel(this, modelId);

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new ProdiaVideoModel(this, modelId);

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = (string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl).TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "PRODIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("prodia");
        return options;
    }
}

/// <summary>Registers Prodia.</summary>
public static class ProdiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddProdia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ProdiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ProdiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ProdiaProvider.ProviderId), options);
        });
        return services;
    }
}
