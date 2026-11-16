// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini settings.</summary>
public class GoogleOptions
{
    /// <summary>Settings for downloading JSON Lines batch results.</summary>
    public BatchResultDownloads? BatchResultDownloads { get; set; }

    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "GOOGLE_GENERATIVE_AI_API_KEY";

    /// <summary>When set, requests use <c>Authorization: Bearer</c> instead of <c>x-goog-api-key</c>.</summary>
    public bool UseBearerToken { get; set; }

    /// <summary>Extra headers merged into every request.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>User-Agent suffix. Defaults to <c>ai-sdk/google</c>.</summary>
    public string UserAgent { get; set; } = "ai-sdk/google";

    /// <summary>Id factory used for tool calls that the response does not identify.</summary>
    public Func<string>? GenerateId { get; set; }

    /// <summary>Embedding provider options read by <see cref="GoogleEmbeddingModel"/>.</summary>
    public JsonElement? EmbeddingProviderOptions { get; set; }
}

/// <summary>Google Gemini provider.</summary>
public class GoogleProvider : ProviderBase, Operations.IBatchProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "google";

    /// <summary>Creates a provider.</summary>
    public GoogleProvider(HttpClient httpClient, GoogleOptions? options = null)
        : this(httpClient, options, "google.generative-ai")
    {
    }

    /// <summary>Creates a provider with an explicit model provider id.</summary>
    protected GoogleProvider(HttpClient httpClient, GoogleOptions? options, string modelProvider)
        : base(ProviderName)
    {
        Options = options ?? new GoogleOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
        ModelProvider = modelProvider ?? "google.generative-ai";
    }

    /// <summary>Options.</summary>
    public GoogleOptions Options { get; }

    /// <summary>Settings for downloading remote files in tool results before conversion. Null leaves tool-result URLs alone.</summary>
    internal virtual GoogleToolResultDownloadSettings? ToolResultDownloadSettings => null;

    /// <summary>
    /// URLs a Gemini model reads itself: Generative Language file URLs and YouTube URLs for any media type, plus any https URL
    /// for the media types in <c>supportedExternalUrlMediaTypes</c> (Gemini models other than 2.0). Maps to <c>getSupportedUrls</c>.
    /// </summary>
    internal virtual IReadOnlyDictionary<string, string> GetSupportedUrls(string? modelId)
    {
        var baseUrl = System.Text.RegularExpressions.Regex.Escape(Options.BaseUrl);
        var urls = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["*"] = "^https://generativelanguage\\.googleapis\\.com/v1beta/files/.*\\z"
                + "|^" + baseUrl + "/files/.*\\z"
                + "|^https://(?:www\\.)?youtube\\.com/watch\\?v=[\\w-]+(?:&[\\w=&.-]*)?\\z"
                + "|^https://youtu\\.be/[\\w-]+(?:\\?[\\w=&.-]*)?\\z",
        };
        if (modelId is null || (System.Text.RegularExpressions.Regex.IsMatch(modelId, "(^|/)gemini-") && !System.Text.RegularExpressions.Regex.IsMatch(modelId, "(^|/)gemini-2\\.0")))
        {
            foreach (var mediaType in ExternalUrlMediaTypes)
            {
                urls[mediaType] = "^https://.*\\z";
            }
        }

        return urls;
    }

    private static readonly string[] ExternalUrlMediaTypes =
    {
        "text/html", "text/css", "text/plain", "text/xml", "text/csv", "text/rtf", "text/javascript", "application/json", "application/pdf",
        "image/bmp", "image/jpeg", "image/png", "image/webp", "video/mp4", "video/mpeg", "video/quicktime", "video/avi",
        "video/x-flv", "video/mpg", "video/webm", "video/wmv", "video/3gpp",
    };

    /// <summary>Gemini Batch API. Downloads batch results.</summary>
    public Operations.IBatchApi? ExperimentalBatch()
    {
        return new GoogleBatchApi(this);
    }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Provider id recorded on models, such as <c>google.generative-ai</c> or <c>google.vertex.chat</c>.</summary>
    public string ModelProvider { get; }

    /// <summary>Clock used for response timestamps.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <summary>Creates a provider.</summary>
    public static GoogleProvider Create(GoogleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GoogleProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GoogleLanguageModel(this, modelId);
    }

    /// <summary>
    /// Experimental decision model that uses the Gemini Decisions API (<c>POST /decisions</c>).
    /// The provider id is <c>google.decision</c>.
    /// </summary>
    public GoogleDecisionModel DecisionModel(string modelId)
    {
        var provider = System.Text.RegularExpressions.Regex.Replace(ModelProvider, @"\.generative-ai$", string.Empty) + ".decision";
        return new GoogleDecisionModel(this, modelId, provider);
    }

    /// <summary>Deprecated alias of <see cref="DecisionModel"/>. Maps to <c>evaluationModel</c>.</summary>
    [Obsolete("Use DecisionModel instead.")]
    public GoogleDecisionModel EvaluationModel(string modelId)
    {
        return DecisionModel(modelId);
    }

    /// <summary>Experimental typed field extraction model that uses Gemini structured output. The provider id is <c>google.extraction</c>.</summary>
    public Operations.ExtractionLanguageModel ExtractionModel(string modelId)
    {
        var provider = System.Text.RegularExpressions.Regex.Replace(ModelProvider, @"\.generative-ai$", string.Empty) + ".extraction";
        return new Operations.ExtractionLanguageModel(LanguageModel(modelId), provider);
    }

    /// <summary>Language model that posts to the Interactions API.</summary>
    public virtual ILanguageModel Interactions(string modelId)
    {
        return new GoogleInteractionsModel(this, modelId);
    }

    /// <summary>Language model that calls an agent through the Interactions API. The request sends <c>agent</c> instead of <c>model</c>.</summary>
    public virtual ILanguageModel Interactions(GoogleInteractionsAgent agent)
    {
        return new GoogleInteractionsModel(this, agent);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new GoogleEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new GoogleImageModel(this, modelId);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new GoogleSpeechModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GoogleTranscriptionModel(this, modelId);
    }

    /// <inheritdoc />
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return new GoogleRealtimeModel(this, modelId);
    }

    /// <summary>Builds request headers. An API key or a bearer token is required.</summary>
    internal async Task<Dictionary<string, string?>> HeadersAsync(CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        if (!ContainsKey(headers, "user-agent") && !string.IsNullOrEmpty(Options.UserAgent))
        {
            headers["user-agent"] = Options.UserAgent;
        }

        if (Options.UseBearerToken)
        {
            var token = await BearerTokenAsync(cancellationToken).ConfigureAwait(false);
            if (!ContainsKey(headers, "Authorization"))
            {
                headers["Authorization"] = "Bearer " + token;
            }
        }
        else
        {
            var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable);
            if (!ContainsKey(headers, "x-goog-api-key"))
            {
                headers["x-goog-api-key"] = key;
            }
        }

        return headers;
    }

    /// <summary>Token sent when <see cref="GoogleOptions.UseBearerToken"/> is set. Defaults to the API key.</summary>
    private protected virtual Task<string> BearerTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable));
    }

    private static bool ContainsKey(Dictionary<string, string?> headers, string name)
    {
        foreach (var key in headers.Keys)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>An ephemeral Gemini Live auth token and the WebSocket URL it opens.</summary>
public sealed class GoogleRealtimeClientSecret
{
    internal GoogleRealtimeClientSecret(string token, string url, long? expiresAt)
    {
        Token = token;
        Url = url;
        ExpiresAt = expiresAt;
    }

    /// <summary>Auth token name.</summary>
    public string Token { get; }

    /// <summary>Constrained Live API WebSocket URL.</summary>
    public string Url { get; }

    /// <summary>Token expiry as unix seconds, when the provider sent one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>
/// Settings of <see cref="GoogleRealtimeModel"/>. Maps to <c>GoogleRealtimeModelConfig</c>.
/// <see cref="Headers"/> maps the upstream <c>Resolvable</c>: an object, a promise, a callback or an async callback
/// all fit a function that returns a task.
/// </summary>
public sealed class GoogleRealtimeModelConfig
{
    /// <summary>Creates a config.</summary>
    public GoogleRealtimeModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Provider id, for example <c>google.realtime</c>.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP helper used to mint client secrets. Defaults to a shared client.</summary>
    public ProviderHttp? Http { get; set; }

    /// <summary>Clock used to compute token expiry. Defaults to the system clock.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }
}

/// <summary>
/// Realtime model for Gemini Live. Maps to <c>GoogleRealtimeModel</c>. Event mapping is local; opening the Live
/// socket is left to the caller, which uses <see cref="GetServerWebSocketConfigAsync"/> or a client secret.
/// </summary>
public sealed class GoogleRealtimeModel : IServerWebSocketRealtimeModel
{
    private const string RealtimeWebSocketPath = "google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateContentConstrained";

    // Server-side connections use the unconstrained Bidi endpoint: a trusted server
    // holds the API key and sends the session `setup` over the socket, rather than
    // baking it into an ephemeral token at mint time (which the constrained
    // endpoint requires).
    private const string RealtimeServerWebSocketPath = "google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";

    private static readonly Lazy<ProviderHttp> DefaultHttp = new(() => new ProviderHttp(new HttpClient()));

    private readonly GoogleRealtimeModelConfig _config;
    private readonly GoogleRealtimeEventMapper _mapper = new();

    /// <summary>Creates a realtime model.</summary>
    public GoogleRealtimeModel(string modelId, GoogleRealtimeModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Creates a realtime model that takes its base URL, headers and HTTP client from <paramref name="provider"/>.</summary>
    public GoogleRealtimeModel(GoogleProvider provider, string modelId)
        : this(
            modelId,
            new GoogleRealtimeModelConfig(
                (provider ?? throw new ArgumentNullException(nameof(provider))).ModelProvider,
                provider.Options.BaseUrl,
                async cancellationToken => await provider.HeadersAsync(cancellationToken).ConfigureAwait(false))
            {
                Http = provider.Http,
                Clock = () => provider.Clock(),
            })
    {
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _config.Provider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        return GoogleSpeechTranslation.WebSocketUrl(_config.BaseUrl, null);
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("Gemini Live WebSocket sessions are not opened by this port. Map events with GoogleRealtimeEventMapper instead.");
    }

    /// <summary>
    /// Creates an ephemeral auth token that a client uses to open a Live session.
    /// <paramref name="expiresAfterSeconds"/> is the window to open a session and defaults to 60 seconds.
    /// The token itself lives 30 minutes longer, so the opened session has room to run.
    /// Maps to <c>doCreateClientSecret</c>.
    /// </summary>
    public async Task<GoogleRealtimeClientSecret> CreateClientSecretAsync(RealtimeModelV4SessionConfig? sessionConfig, int? expiresAfterSeconds, CancellationToken cancellationToken)
    {
        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        var key = GetHeader(headers, "x-goog-api-key");
        if (string.IsNullOrEmpty(key))
        {
            throw new AiSdkException("Google Generative AI API key is required for realtime token creation.");
        }

        // `newSessionExpireTime` controls how long the token can be used to *open*
        // a session, so map `expiresAfterSeconds` to it. `expireTime` is the overall
        // token lifetime and must be >= `newSessionExpireTime`, so extend it to leave
        // room for the opened session to run.
        var now = (_config.Clock ?? (() => DateTimeOffset.UtcNow))();
        var newSessionExpireTime = now.AddSeconds(expiresAfterSeconds ?? 60);
        var body = new JsonObject
        {
            // `uses: 0` means no limit is applied to how many times the token can
            // start a session. An unset value would default to 1, which breaks reconnects.
            ["uses"] = 0,
            ["expireTime"] = Iso(newSessionExpireTime.AddMinutes(30)),
            ["newSessionExpireTime"] = Iso(newSessionExpireTime),
            ["bidiGenerateContentSetup"] = GoogleRealtimeEventMapper.BuildGoogleSessionConfig(sessionConfig, ModelId),
        };
        var uri = GoogleSpeechTranslation.LiveBaseUrl(_config.BaseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/v1alpha/auth_tokens";
        uri.Query = "key=" + Uri.EscapeDataString(key!);
        var response = await (_config.Http ?? DefaultHttp.Value).SendJsonStringAsync(HttpMethod.Post, uri.Uri, body.ToJsonString(), null, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        long? expiresAt = null;
        if (GoogleJson.String(document.RootElement, "expireTime") is { } expireTime)
        {
            expiresAt = DateTimeOffset.Parse(expireTime, CultureInfo.InvariantCulture).ToUnixTimeSeconds();
        }

        return new GoogleRealtimeClientSecret(
            GoogleJson.String(document.RootElement, "name") ?? string.Empty,
            GoogleSpeechTranslation.LiveWebSocketUrl(_config.BaseUrl, RealtimeWebSocketPath).Uri.ToString(),
            expiresAt);
    }

    /// <summary>
    /// The URL a client opens with a token from <see cref="CreateClientSecretAsync"/>. The token travels in the query
    /// string, so there are no subprotocols. Maps to <c>getWebSocketConfig</c>.
    /// </summary>
    public RealtimeModelV4WebSocketConfig GetWebSocketConfig(string token, string url)
    {
        return new RealtimeModelV4WebSocketConfig(url + "?access_token=" + Uri.EscapeDataString(token));
    }

    /// <inheritdoc />
    public Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent? intent, CancellationToken cancellationToken)
    {
        // Gemini Live exposes only the conversational endpoint; fail fast instead
        // of silently downgrading unsupported intents.
        AssertConversationIntent(intent);
        return ResolveServerConnectionAsync(cancellationToken);
    }

    /// <summary>Maps one server message with this model's shared event mapper.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
    {
        return _mapper.ParseServerEvent(raw);
    }

    /// <summary>
    /// Creates a parser with its own turn and usage state, so concurrent server connections do not share it.
    /// Maps to <c>createServerEventParser</c>.
    /// </summary>
    public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CreateServerEventParser()
    {
        var mapper = new GoogleRealtimeEventMapper();
        return raw => mapper.ParseServerEvent(raw);
    }

    /// <summary>Serializes a client event with this model's event mapper.</summary>
    public JsonObject? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return _mapper.SerializeClientEvent(clientEvent, ModelId);
    }

    /// <summary>Builds the Live <c>setup</c> object for a normalized session config.</summary>
    public JsonObject BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return GoogleRealtimeEventMapper.BuildGoogleSessionConfig(config, ModelId);
    }

    private static string? GetHeader(IReadOnlyDictionary<string, string?> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private static string GetServerWebSocketUrl(string baseUrl)
    {
        var url = GoogleSpeechTranslation.LiveBaseUrl(baseUrl);
        url.Scheme = url.Scheme == "https" ? "wss" : "ws";
        url.Path = url.Path.TrimEnd('/') + "/ws/" + RealtimeServerWebSocketPath;
        return url.Uri.ToString();
    }

    private static string Iso(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    private static void AssertConversationIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent != null && intent != RealtimeModelV4SessionIntent.Conversation)
        {
            throw new AiSdkException($"Google realtime does not support the '{intent.Value.ToString().ToLowerInvariant()}' session intent.");
        }
    }

    private async Task<RealtimeModelV4ServerConnection> ResolveServerConnectionAsync(CancellationToken cancellationToken)
    {
        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        var defined = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in headers)
        {
            if (pair.Value != null)
            {
                defined[pair.Key] = pair.Value;
            }
        }

        return new RealtimeModelV4ServerConnection(GetServerWebSocketUrl(_config.BaseUrl), defined);
    }
}

/// <summary>Registers Google and Vertex.</summary>
public static class GoogleServiceCollectionExtensions
{
    /// <summary>Adds <see cref="GoogleProvider"/>.</summary>
    public static IServiceCollection AddGoogle(this IServiceCollection services, Action<GoogleOptions>? configure = null)
    {
        services.AddHttpClient(GoogleProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new GoogleOptions();
            configure?.Invoke(options);
            return new GoogleProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GoogleProvider.ProviderName), options);
        });
        return services;
    }

    /// <summary>Adds <see cref="GoogleVertexProvider"/>.</summary>
    public static IServiceCollection AddGoogleVertex(this IServiceCollection services, Action<VertexOptions>? configure = null)
    {
        services.AddHttpClient("google-vertex");
        services.AddSingleton(sp =>
        {
            var options = new VertexOptions();
            configure?.Invoke(options);
            return new GoogleVertexProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient("google-vertex"), options);
        });
        return services;
    }
}
