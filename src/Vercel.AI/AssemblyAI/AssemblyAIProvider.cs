// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.AssemblyAI;

/// <summary>AssemblyAI transcription provider. Uploads audio and polls <c>/v2/transcript</c>.</summary>
public sealed class AssemblyAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "assemblyai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.assemblyai.com";

    /// <summary>User-Agent suffix sent on every call. Matches the package version fallback.</summary>
    public const string UserAgent = "ai-sdk/assemblyai/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public AssemblyAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new AssemblyAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AssemblyAIProvider(client, options);
    }

    /// <summary>HTTP client used by <see cref="AssemblyAITranscriptionModel"/>.</summary>
    internal HttpClient Client
    {
        get { return _httpClient; }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ASSEMBLYAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.RawAuthorization;
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
        return new AssemblyAITranscriptionModel(Client, modelId, "assemblyai.transcription", Options.BaseUrl, () => CreateHeaders());
    }

    private static AiSdkException Unsupported(string modelId, string modelType)
    {
        return new AiSdkException("AssemblyAI does not provide " + modelType + " '" + modelId + "'.");
    }
}

/// <summary>Registers AssemblyAI.</summary>
public static class AssemblyAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddAssemblyAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(AssemblyAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new AssemblyAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AssemblyAIProvider.ProviderId), options);
        });
        return services;
    }
}
