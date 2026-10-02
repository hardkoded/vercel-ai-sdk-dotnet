// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.GmiCloud;

/// <summary>GMI Cloud provider. OpenAI Chat Completions compatible at <c>https://api.gmi-serving.com/v1</c>.</summary>
public sealed class GmiCloudProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gmicloud";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.gmi-serving.com/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "GMI_CLOUD_APIKEY";

    /// <summary>User-Agent suffix sent on every call.</summary>
    public const string UserAgent = "ai-sdk/gmicloud/0.0.0-test";

    /// <summary>Creates a provider.</summary>
    public GmiCloudProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), Wrap(httpClient))
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new GmiCloudProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GmiCloudProvider(client, options);
    }

    private static HttpClient Wrap(HttpClient httpClient)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        return new HttpClient(new GmiCloudErrorRewriter(httpClient), disposeHandler: true);
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
        if (!options.Headers.ContainsKey("User-Agent"))
        {
            options.Headers["User-Agent"] = UserAgent;
        }

        return options;
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GmiCloudChatLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw Unsupported(modelId, "embeddingModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw Unsupported(modelId, "imageModel");
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("NoSuchModelError: GMI Cloud does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers <see cref="GmiCloudProvider"/>.</summary>
public static class GmiCloudServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddGmiCloud(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(GmiCloudProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new GmiCloudProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GmiCloudProvider.ProviderId), options);
        });
        return services;
    }
}
