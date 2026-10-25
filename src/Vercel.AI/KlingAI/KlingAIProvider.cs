// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.KlingAI;

/// <summary>KlingAI provider settings. Adds the legacy access key and secret key to <see cref="OpenAICompatibleOptions"/>.</summary>
public sealed class KlingAIOptions : OpenAICompatibleOptions
{
    /// <summary>
    /// Legacy access key. Defaults to the <c>KLINGAI_ACCESS_KEY</c> environment variable.
    /// Used with <see cref="SecretKey"/> to sign a short-lived JWT. Prefer <see cref="OpenAICompatibleOptions.ApiKey"/>.
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Legacy secret key. Defaults to the <c>KLINGAI_SECRET_KEY</c> environment variable.
    /// Used with <see cref="AccessKey"/> to sign a short-lived JWT. Prefer <see cref="OpenAICompatibleOptions.ApiKey"/>.
    /// </summary>
    public string? SecretKey { get; set; }
}

/// <summary>KlingAI provider. It supports video generation only.</summary>
public sealed class KlingAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "klingai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api-singapore.klingai.com";

    internal readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a provider. Pass <see cref="KlingAIOptions"/> to use the legacy access key and secret key.
    /// The credentials are read when a request is sent, so a missing key fails then and not here.
    /// </summary>
    public KlingAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }

    /// <summary>Creates a provider.</summary>
    /// <param name="options">Provider options.</param>
    /// <param name="handler">
    /// Optional message handler. The provider follows redirects by hand so a status poll cannot send the credentials to another host.
    /// For that reason it sets <c>AllowAutoRedirect = false</c> on a supplied <see cref="HttpClientHandler"/> or (on .NET 5 and later) <c>SocketsHttpHandler</c>,
    /// including one wrapped in <see cref="DelegatingHandler"/>s. This changes the handler you pass, and it throws if the handler has already sent a request.
    /// Any other handler type that exposes a public settable <c>bool AllowAutoRedirect</c> property is changed by reflection. Any remaining handler is left as is. It must not follow redirects on its own.
    /// </param>
    public static new KlingAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        RedirectGuard.DisableAutoRedirect(handler);
        var client = handler is null ? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) : new HttpClient(handler, disposeHandler: false);
        if (handler is not null)
        {
            options = options?.Copy() ?? new KlingAIOptions();
            options.DownloadHttpClient ??= client;
        }

        return new KlingAIProvider(client, options);
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new KlingAIVideoModel(this, modelId);

    /// <summary>Creates a video model. Same as <see cref="VideoModel"/>.</summary>
    public IVideoModel Video(string modelId) => VideoModel(modelId);

    /// <summary>
    /// Resolves the bearer token on every call. An API key is sent as is. A legacy access key and secret key sign a fresh JWT.
    /// </summary>
    protected override string? ResolveApiKey()
    {
        var kling = Options as KlingAIOptions;
        return KlingAIAuth.ResolveAuthToken(Options.ApiKey, kling?.AccessKey, kling?.SecretKey);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new KlingAIOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl.TrimEnd('/');
        options.ApiKeyEnvironmentVariable = KlingAIAuth.ApiKeyEnvironmentVariable;
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("klingai");
        return options;
    }
}

/// <summary>Registers KlingAI.</summary>
public static class KlingAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddKlingAI(this IServiceCollection services, Action<KlingAIOptions>? configure = null)
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        services.AddHttpClient(KlingAIProvider.ProviderId).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton(sp =>
        {
            var options = new KlingAIOptions();
            configure?.Invoke(options);
            return new KlingAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(KlingAIProvider.ProviderId), options);
        });
        return services;
    }
}
