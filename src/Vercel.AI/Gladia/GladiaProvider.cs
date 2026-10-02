// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Gladia;

/// <summary>Gladia transcription provider. Uploads audio, then calls <c>/v2/pre-recorded</c>.</summary>
public sealed class GladiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gladia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.gladia.io";

    /// <summary>User-Agent suffix sent on every call.</summary>
    public const string UserAgent = "ai-sdk/gladia/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public GladiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new GladiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GladiaProvider(client, options);
    }

    /// <summary>HTTP client used by <see cref="GladiaTranscriptionModel"/>.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "GLADIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-gladia-key";
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
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw Unsupported(modelId, "embeddingModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw Unsupported(modelId, "imageModel");
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GladiaTranscriptionModel(Client, string.IsNullOrEmpty(modelId) ? "default" : modelId, Options.BaseUrl, () => CreateHeaders())
        {
            Provider = "gladia.transcription",
        };
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("NoSuchModelError: Gladia does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers Gladia.</summary>
public static class GladiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddGladia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(GladiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new GladiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GladiaProvider.ProviderId), options);
        });
        return services;
    }
}
