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

    /// <summary>Language model that posts to the Interactions API.</summary>
    public virtual ILanguageModel Interactions(string modelId)
    {
        return new GoogleInteractionsModel(this, modelId);
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

/// <summary>Realtime model. Event mapping is local; opening the Live socket is not available on this runtime.</summary>
public sealed class GoogleRealtimeModel : IRealtimeModel
{
    private const string ConstrainedPath = "google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateContentConstrained";

    private readonly GoogleProvider _provider;
    private readonly GoogleRealtime _mapper = new();

    /// <summary>Creates a realtime model.</summary>
    public GoogleRealtimeModel(GoogleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        var key = _provider.Options.UseBearerToken ? null : _provider.Options.ApiKey;
        return GoogleSpeechTranslation.WebSocketUrl(_provider.Options.BaseUrl, key);
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("Gemini Live WebSocket sessions are not opened by this port. Map events with GoogleRealtime instead.");
    }

    /// <summary>
    /// Creates an ephemeral auth token that a client uses to open a Live session.
    /// <paramref name="expiresAfterSeconds"/> is the window to open a session and defaults to 60 seconds.
    /// The token itself lives 30 minutes longer, so the opened session has room to run.
    /// </summary>
    public async Task<GoogleRealtimeClientSecret> CreateClientSecretAsync(GoogleRealtimeSession? session, int? expiresAfterSeconds, CancellationToken cancellationToken)
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (!headers.TryGetValue("x-goog-api-key", out var key) || string.IsNullOrEmpty(key))
        {
            throw new AiSdkException("Google Generative AI API key is required for realtime token creation.");
        }

        var now = _provider.Clock();
        var newSessionExpireTime = now.AddSeconds(expiresAfterSeconds ?? 60);
        var body = new JsonObject
        {
            // 0 lets the token start any number of sessions. The default of 1 breaks reconnects.
            ["uses"] = 0,
            ["expireTime"] = Iso(newSessionExpireTime.AddMinutes(30)),
            ["newSessionExpireTime"] = Iso(newSessionExpireTime),
            ["bidiGenerateContentSetup"] = GoogleRealtime.BuildSession(WithModel(session)),
        };
        var uri = GoogleSpeechTranslation.LiveBaseUrl(_provider.Options.BaseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/v1alpha/auth_tokens";
        uri.Query = "key=" + Uri.EscapeDataString(key!);
        var response = await _provider.Http.SendJsonStringAsync(HttpMethod.Post, uri.Uri, body.ToJsonString(), null, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        long? expiresAt = null;
        if (GoogleJson.String(document.RootElement, "expireTime") is { } expireTime)
        {
            expiresAt = DateTimeOffset.Parse(expireTime, CultureInfo.InvariantCulture).ToUnixTimeSeconds();
        }

        return new GoogleRealtimeClientSecret(
            GoogleJson.String(document.RootElement, "name") ?? string.Empty,
            GoogleSpeechTranslation.LiveWebSocketUrl(_provider.Options.BaseUrl, ConstrainedPath).Uri.ToString(),
            expiresAt);
    }

    /// <summary>The URL a client opens with a token from <see cref="CreateClientSecretAsync"/>.</summary>
    public string GetWebSocketUrl(string token, string url)
    {
        return url + "?access_token=" + Uri.EscapeDataString(token);
    }

    /// <summary>Maps one server message with this model's event mapper.</summary>
    public IReadOnlyList<GoogleRealtimeEvent> ParseServerEvent(JsonElement raw)
    {
        return _mapper.ParseServerEvent(raw);
    }

    /// <summary>Serializes a client event with this model's event mapper. A session update uses this model.</summary>
    public JsonObject? SerializeClientEvent(string type, GoogleRealtimeSession? session, string? audio, string? text, string? callId, string? callName, string? callOutput)
    {
        return _mapper.SerializeClient(type, session == null ? null : WithModel(session), audio, text, callId, callName, callOutput);
    }

    private GoogleRealtimeSession WithModel(GoogleRealtimeSession? session)
    {
        session ??= new GoogleRealtimeSession();
        return new GoogleRealtimeSession
        {
            ModelId = ModelId,
            Instructions = session.Instructions,
            Voice = session.Voice,
            OutputModalities = session.OutputModalities,
            Tools = session.Tools,
            ProviderOptions = session.ProviderOptions,
            InputAudioTranscription = session.InputAudioTranscription,
            OutputAudioTranscription = session.OutputAudioTranscription,
            InputAudioRate = session.InputAudioRate,
        };
    }

    private static string Iso(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
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
