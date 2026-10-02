// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.RevAI;

/// <summary>Rev.ai speech-to-text provider. Calls <c>https://api.rev.ai/speechtotext/v1/jobs</c>.</summary>
public sealed class RevAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "revai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.rev.ai";

    /// <summary>User-Agent suffix sent on every call. Matches the package version fallback.</summary>
    public const string UserAgent = "ai-sdk/revai/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public RevAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new RevAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new RevAIProvider(client, options);
    }

    /// <summary>HTTP client used by <see cref="RevAITranscriptionModel"/>.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "REVAI_API_KEY";
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
        return new RevAITranscriptionModel(Client, modelId, Options.BaseUrl, () => CreateHeaders())
        {
            Provider = "revai.transcription",
        };
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("NoSuchModelError: Rev.ai does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers Rev.ai.</summary>
public static class RevAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddRevAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(RevAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new RevAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(RevAIProvider.ProviderId), options);
        });
        return services;
    }
}
