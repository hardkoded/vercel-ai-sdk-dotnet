// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tencent;

/// <summary>
/// Tencent provider settings. Adds the Tencent Cloud credentials for VOD video models to <see cref="OpenAICompatibleOptions"/>.
/// <see cref="OpenAICompatibleOptions.ApiKey"/> is the TokenHub API key, used by language models and TokenHub video models.
/// It defaults to the <c>TENCENT_TOKENHUB_API_KEY</c> environment variable.
/// <see cref="OpenAICompatibleOptions.BaseUrl"/> is the TokenHub base URL and defaults to <c>https://tokenhub-intl.tencentcloudmaas.com/v1</c>.
/// </summary>
public sealed class TencentOptions : OpenAICompatibleOptions
{
    /// <summary>Tencent Cloud API SecretId, used by VOD video models. Defaults to the <c>TENCENTCLOUD_SECRET_ID</c> environment variable.</summary>
    public string? SecretId { get; set; }

    /// <summary>Tencent Cloud API SecretKey, used by VOD video models. Defaults to the <c>TENCENTCLOUD_SECRET_KEY</c> environment variable.</summary>
    public string? SecretKey { get; set; }

    /// <summary>Session token for temporary Tencent Cloud credentials, sent as <c>X-TC-Token</c>. Defaults to the <c>TENCENTCLOUD_SESSION_TOKEN</c> environment variable.</summary>
    public string? SessionToken { get; set; }

    /// <summary>VOD application id (<c>SubAppId</c>), used by VOD video models. Defaults to the <c>TENCENTCLOUD_VOD_SUB_APP_ID</c> environment variable.</summary>
    public string? SubAppId { get; set; }

    /// <summary>Tencent Cloud region sent as <c>X-TC-Region</c> on VOD requests. VOD AIGC actions do not require a region.</summary>
    public string? Region { get; set; }

    /// <summary>VOD API base URL. Defaults to <c>https://vod.intl.tencentcloudapi.com</c>.</summary>
    public string? VodBaseUrl { get; set; }
}

/// <summary>
/// Tencent Cloud provider: TokenHub chat models (OpenAI-compatible) and Kling (TokenHub) and VOD AIGC video models.
/// </summary>
public sealed class TencentProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "tencent";

    /// <summary>Default TokenHub API origin.</summary>
    public const string DefaultBaseUrl = "https://tokenhub-intl.tencentcloudmaas.com/v1";

    /// <summary>Default VOD API origin.</summary>
    public const string DefaultVodBaseUrl = "https://vod.intl.tencentcloudapi.com";

    internal readonly HttpClient HttpClient;

    /// <summary>
    /// Creates a provider. Pass <see cref="TencentOptions"/> to use VOD video models.
    /// Credentials are read when a request is sent, so a missing key fails then and not here.
    /// </summary>
    public TencentProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    internal TencentOptions TencentOptions => Options as TencentOptions ?? new TencentOptions();

    internal string VodBaseUrl => string.IsNullOrEmpty(TencentOptions.VodBaseUrl) ? DefaultVodBaseUrl : TencentOptions.VodBaseUrl!.TrimEnd('/');

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
    /// It sets <c>AllowAutoRedirect = false</c> on a supplied handler that exposes it (see <see cref="Vercel.AI.KlingAI.KlingAIProvider.Create"/>).
    /// </param>
    public static new TencentProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        RedirectGuard.DisableAutoRedirect(handler);
        var client = handler is null ? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) : new HttpClient(handler, disposeHandler: false);
        if (handler is not null)
        {
            options = options?.Copy() ?? new TencentOptions();
            options.DownloadHttpClient ??= client;
        }

        return new TencentProvider(client, options);
    }

    /// <inheritdoc />
    public override OpenAICompatibleLanguageModel CreateChatModel(string modelId) => new TencentChatLanguageModel(this, modelId);

    /// <summary>Creates a TokenHub model for text generation.</summary>
    public ILanguageModel Chat(string modelId) => LanguageModel(modelId);

    /// <summary>Creates a model for video generation. TokenHub Kling models use their TokenHub id; VOD AIGC models use <c>vod/&lt;ModelName&gt;/&lt;ModelVersion&gt;</c>.</summary>
    public override IVideoModel VideoModel(string modelId) => new TencentVideoModel(this, modelId);

    /// <summary>Creates a model for video generation. Same as <see cref="VideoModel"/>.</summary>
    public IVideoModel Video(string modelId) => VideoModel(modelId);

    internal Dictionary<string, string?> CreateVodHeaders()
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        ProviderExchange.AddUserAgent(headers, ProviderId);
        return headers;
    }

    /// <summary>Resolves the VOD credentials when a VOD request is made, so TokenHub-only use needs none.</summary>
    internal TencentVodCredentials GetVodCredentials()
    {
        var options = TencentOptions;
        var subAppId = !string.IsNullOrEmpty(options.SubAppId)
            ? options.SubAppId!
            : LoadSetting(null, "subAppId", "TENCENTCLOUD_VOD_SUB_APP_ID", "Tencent VOD SubAppId");
        return new TencentVodCredentials
        {
            SecretId = LoadSetting(options.SecretId, "secretId", "TENCENTCLOUD_SECRET_ID", "Tencent Cloud SecretId"),
            SecretKey = LoadSetting(options.SecretKey, "secretKey", "TENCENTCLOUD_SECRET_KEY", "Tencent Cloud SecretKey"),
            SessionToken = options.SessionToken ?? OptionalEnvironment("TENCENTCLOUD_SESSION_TOKEN"),
            SubAppId = ParseSubAppId(subAppId),
        };
    }

    private static long ParseSubAppId(string value)
    {
        if (!long.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var subAppId) || subAppId <= 0)
        {
            throw new AiSdkException("Tencent VOD SubAppId must be a positive integer.");
        }

        return subAppId;
    }

    private static string? OptionalEnvironment(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string LoadSetting(string? value, string settingName, string environmentVariable, string description)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return value!;
        }

        var fromEnvironment = OptionalEnvironment(environmentVariable);
        return fromEnvironment
            ?? throw new AiSdkException(description + " setting is missing. Pass it using the '" + settingName + "' parameter or the " + environmentVariable + " environment variable.");
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new TencentOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl.TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "TENCENT_TOKENHUB_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.RequireApiKey = true;
        options.IncludeUsage = true;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = ProviderExchange.UserAgent(ProviderId);
        }

        return options;
    }
}

/// <summary>Registers Tencent.</summary>
public static class TencentServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddTencent(this IServiceCollection services, Action<TencentOptions>? configure = null)
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        services.AddHttpClient(TencentProvider.ProviderId).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton(sp =>
        {
            var options = new TencentOptions();
            configure?.Invoke(options);
            return new TencentProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(TencentProvider.ProviderId), options);
        });
        return services;
    }
}
