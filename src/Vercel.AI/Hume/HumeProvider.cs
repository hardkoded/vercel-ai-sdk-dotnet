// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Hume;

/// <summary>Hume speech provider. Calls <c>POST https://api.hume.ai/v0/tts/file</c>.</summary>
public sealed class HumeProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "hume";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.hume.ai";

    /// <summary>User-Agent suffix sent on every call.</summary>
    public const string UserAgent = "ai-sdk/hume/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public HumeProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new HumeProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new HumeProvider(client, options);
    }

    /// <summary>HTTP client used by <see cref="HumeSpeechModel"/>.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "HUME_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "X-Hume-Api-Key";
        if (!options.Headers.ContainsKey("User-Agent"))
        {
            options.Headers["User-Agent"] = UserAgent;
        }

        return options;
    }

    /// <summary>Creates a speech model with the empty Hume model id.</summary>
    public HumeSpeechModel Speech()
    {
        return CreateSpeech(string.Empty);
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
        return CreateSpeech(modelId ?? string.Empty);
    }

    private HumeSpeechModel CreateSpeech(string modelId)
    {
        return new HumeSpeechModel(Client, modelId, Options.BaseUrl, () => CreateHeaders())
        {
            Provider = "hume.speech",
        };
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("NoSuchModelError: Hume does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers Hume.</summary>
public static class HumeServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddHume(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(HumeProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new HumeProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HumeProvider.ProviderId), options);
        });
        return services;
    }
}
