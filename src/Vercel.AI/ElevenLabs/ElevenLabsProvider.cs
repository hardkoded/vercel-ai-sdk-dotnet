// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.ElevenLabs;

/// <summary>ElevenLabs provider.</summary>
public sealed class ElevenLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "elevenlabs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.elevenlabs.io";

    /// <summary>User-Agent suffix sent on every call. Matches the package version fallback.</summary>
    public const string UserAgent = "ai-sdk/elevenlabs/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ElevenLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new ElevenLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ElevenLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ELEVENLABS_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "xi-api-key";
        if (!options.Headers.ContainsKey("User-Agent"))
        {
            options.Headers["User-Agent"] = UserAgent;
        }

        return options;
    }

    /// <summary>HTTP client used by <see cref="ElevenLabsSpeechModel"/>.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
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
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new ElevenLabsSpeechModel(Client, modelId, "elevenlabs.speech", Options.BaseUrl, () => CreateHeaders());
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("ElevenLabs does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers ElevenLabs.</summary>
public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddElevenLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ElevenLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ElevenLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ElevenLabsProvider.ProviderId), options);
        });
        return services;
    }
}
