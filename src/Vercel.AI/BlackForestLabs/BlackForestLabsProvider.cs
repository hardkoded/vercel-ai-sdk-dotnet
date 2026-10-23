// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.BlackForestLabs;

/// <summary>BlackForestLabs provider.</summary>
public sealed class BlackForestLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "black-forest-labs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.bfl.ai/v1";

    /// <summary>Creates a provider. The poll settings apply to every image and video model it creates.</summary>
    public BlackForestLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null, int? pollIntervalMillis = null, int? pollTimeoutMillis = null)
        : base(Prepare(options), httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        PollIntervalMillis = pollIntervalMillis;
        PollTimeoutMillis = pollTimeoutMillis;
    }

    /// <summary>HTTP client used for submit, poll, and download requests.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Poll interval in milliseconds between status checks. Null uses 500ms for images and 2s for video.</summary>
    public int? PollIntervalMillis { get; }

    /// <summary>Overall timeout in milliseconds for polling before giving up. Null uses 60s for images and 10 minutes for video.</summary>
    public int? PollTimeoutMillis { get; }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Black Forest Labs does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    /// <param name="options">Provider options.</param>
    /// <param name="handler">
    /// Optional message handler. The provider follows redirects by hand so a status poll cannot send the API key to another host.
    /// For that reason it sets <c>AllowAutoRedirect = false</c> on a supplied <see cref="HttpClientHandler"/> or (on .NET 5 and later) <see cref="System.Net.Http.SocketsHttpHandler"/>,
    /// including one wrapped in <see cref="DelegatingHandler"/>s. This changes the handler you pass, and it throws if the handler has already sent a request.
    /// A custom handler of any other type is left as is. It must not follow redirects on its own.
    /// </param>
    /// <param name="pollIntervalMillis">Poll interval in milliseconds between status checks.</param>
    /// <param name="pollTimeoutMillis">Overall polling timeout in milliseconds.</param>
    public static BlackForestLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null, int? pollIntervalMillis = null, int? pollTimeoutMillis = null)
    {
        // Redirects are followed by hand so a status poll cannot send the API key to another host.
        var client = handler is null ? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) : new HttpClient(handler, disposeHandler: false);
        if (handler is not null)
        {
            DisableAutoRedirect(handler);
            options = options?.Copy() ?? new OpenAICompatibleOptions();
            options.DownloadHttpClient ??= client;
        }

        return new BlackForestLabsProvider(client, options, pollIntervalMillis, pollTimeoutMillis);
    }

    private static void DisableAutoRedirect(HttpMessageHandler? handler)
    {
        while (handler is not null)
        {
            switch (handler)
            {
                case HttpClientHandler clientHandler:
                    clientHandler.AllowAutoRedirect = false;
                    return;
#if NET5_0_OR_GREATER
                case SocketsHttpHandler socketsHandler:
                    socketsHandler.AllowAutoRedirect = false;
                    return;
#endif
                case DelegatingHandler delegating:
                    handler = delegating.InnerHandler;
                    break;
                default:
                    return;
            }
        }
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = (string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl).TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "BFL_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-key";
        options.UserAgent = ProviderExchange.UserAgent("black-forest-labs");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new BlackForestLabsImageModel(this, modelId);

    /// <summary>Creates an image model. Same as <see cref="ImageModel"/>.</summary>
    public IImageModel Image(string modelId) => ImageModel(modelId);

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new BlackForestLabsVideoModel(this, modelId);

    /// <summary>Creates a video model. Same as <see cref="VideoModel"/>.</summary>
    public IVideoModel Video(string modelId) => VideoModel(modelId);
}

/// <summary>Registers BlackForestLabs.</summary>
public static class BlackForestLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddBlackForestLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(BlackForestLabsProvider.ProviderId).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new BlackForestLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(BlackForestLabsProvider.ProviderId), options);
        });
        return services;
    }
}
