// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Voyage;

/// <summary>Voyage embeddings and reranking. Calls <c>https://api.voyageai.com/v1</c>.</summary>
public sealed class VoyageProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "voyage";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.voyageai.com/v1";

    /// <summary>User-Agent suffix sent on every call.</summary>
    public const string UserAgent = "ai-sdk/voyage/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public VoyageProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new VoyageProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new VoyageProvider(client, options);
    }

    /// <summary>HTTP client used by the Voyage models.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "VOYAGE_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        if (!options.Headers.ContainsKey("User-Agent"))
        {
            options.Headers["User-Agent"] = UserAgent;
        }

        return options;
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw Unsupported(modelId, "languageModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw Unsupported(modelId, "imageModel");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new VoyageEmbeddingModel(Client, modelId, Options.BaseUrl, () => CreateHeaders());
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return new VoyageRerankingModel(Client, modelId, Options.BaseUrl, () => CreateHeaders());
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("NoSuchModelError: Voyage does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers Voyage.</summary>
public static class VoyageServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddVoyage(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(VoyageProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new VoyageProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(VoyageProvider.ProviderId), options);
        });
        return services;
    }
}
