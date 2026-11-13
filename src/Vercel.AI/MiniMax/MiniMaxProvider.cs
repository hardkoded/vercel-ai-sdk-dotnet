// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.MiniMax;

/// <summary>MiniMax provider settings.</summary>
public sealed class MiniMaxOptions
{
    /// <summary>MiniMax API key. Defaults to the <c>MINIMAX_API_KEY</c> environment variable.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Base URL for the API calls. Defaults to MiniMax's Anthropic-compatible endpoint (<c>https://api.minimax.io/anthropic/v1</c>).</summary>
    public string BaseUrl { get; set; } = MiniMaxProvider.DefaultBaseUrl;

    /// <summary>
    /// Use a different URL prefix for video generation API calls. The default prefix is <c>https://api.minimax.io</c>.
    /// It has no version suffix and is never derived from <see cref="BaseUrl"/>.
    /// </summary>
    public string? VideoBaseUrl { get; set; }

    /// <summary>Image generation endpoint. Defaults to <c>https://api.minimax.io/v1</c>.</summary>
    public string? ImageBaseUrl { get; set; }

    /// <summary>Headers merged into every request.</summary>
    public Dictionary<string, string?> Headers { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>MiniMax provider. Speaks the Anthropic Messages protocol at <c>https://api.minimax.io/anthropic/v1</c>.</summary>
public sealed class MiniMaxProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "minimax";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.minimax.io/anthropic/v1";

    /// <summary>Default API origin for video generation. It has no version suffix.</summary>
    public const string DefaultVideoBaseUrl = "https://api.minimax.io";

    /// <summary>Default API origin for image generation.</summary>
    public const string DefaultImageBaseUrl = "https://api.minimax.io/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "MINIMAX_API_KEY";

    private readonly MiniMaxMessagesProvider _messages;
    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public MiniMaxProvider(HttpClient httpClient, MiniMaxOptions? options = null)
        : base(ProviderId)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new MiniMaxOptions();
        var anthropic = new AnthropicOptions
        {
            ApiKey = Options.ApiKey,
            ApiKeyEnvironmentVariable = ApiKeyVariable,
            BaseUrl = string.IsNullOrWhiteSpace(Options.BaseUrl) ? DefaultBaseUrl : Options.BaseUrl,
            Name = ProviderId + ".messages",
        };
        foreach (var pair in Options.Headers)
        {
            anthropic.Headers[pair.Key] = pair.Value;
        }

        _messages = new MiniMaxMessagesProvider(httpClient, anthropic);
    }

    /// <summary>Options.</summary>
    public MiniMaxOptions Options { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static MiniMaxProvider Create(MiniMaxOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new MiniMaxProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return Chat(modelId);
    }

    /// <summary>Creates a MiniMax chat model for text generation.</summary>
    public MiniMaxChatLanguageModel Chat(string modelId)
    {
        return new MiniMaxChatLanguageModel(_messages, modelId);
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId)
    {
        return Video(modelId);
    }

    /// <summary>Creates a MiniMax video model for video generation.</summary>
    public MiniMaxVideoModel Video(string modelId)
    {
        var baseUrl = ProviderValues.WithoutTrailingSlash(string.IsNullOrWhiteSpace(Options.VideoBaseUrl) ? DefaultVideoBaseUrl : Options.VideoBaseUrl)!;
        return new MiniMaxVideoModel(modelId, new MiniMaxVideoModelConfig("minimax.video", baseUrl, CreateMediaHeaders, _httpClient));
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return Image(modelId);
    }

    /// <summary>Creates a MiniMax image model for image generation and editing.</summary>
    public MiniMaxImageModel Image(string modelId)
    {
        var baseUrl = ProviderValues.WithoutTrailingSlash(string.IsNullOrWhiteSpace(Options.ImageBaseUrl) ? DefaultImageBaseUrl : Options.ImageBaseUrl)!;
        return new MiniMaxImageModel(modelId, new MiniMaxImageModelConfig("minimax.image", baseUrl, CreateMediaHeaders, _httpClient));
    }

    private Dictionary<string, string> CreateMediaHeaders()
    {
        var headers = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Authorization", "Bearer " + ApiKeys.Require(Options.ApiKey, ApiKeyVariable)),
        };
        headers.AddRange(Options.Headers);
        return ProviderValues.WithUserAgentSuffix(headers, "ai-sdk-minimax/" + Operations.AiSdkVersion.Version);
    }
}

/// <summary>The Messages provider behind <see cref="MiniMaxProvider"/>.</summary>
internal sealed class MiniMaxMessagesProvider : AnthropicProvider
{
    private static readonly IReadOnlyDictionary<string, string> NoUrls = new Dictionary<string, string>();

    public MiniMaxMessagesProvider(HttpClient httpClient, AnthropicOptions options)
        : base(httpClient, options)
    {
    }

    protected internal override IReadOnlyDictionary<string, string> SupportedUrls => NoUrls;

    public override async Task<Dictionary<string, string?>> CreateHeadersAsync(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders, string? body, CancellationToken cancellationToken)
    {
        var headers = await base.CreateHeadersAsync(betas, callHeaders, body, cancellationToken).ConfigureAwait(false);
        if (callHeaders == null || !callHeaders.ContainsKey("user-agent"))
        {
            headers["user-agent"] = "ai-sdk/minimax/" + Operations.AiSdkVersion.Version;
        }

        return headers;
    }
}

/// <summary>Registers <see cref="MiniMaxProvider"/>.</summary>
public static class MiniMaxServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddMiniMax(this IServiceCollection services, Action<MiniMaxOptions>? configure = null)
    {
        services.AddHttpClient(MiniMaxProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new MiniMaxOptions();
            configure?.Invoke(options);
            return new MiniMaxProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(MiniMaxProvider.ProviderId), options);
        });
        return services;
    }
}
