// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>Client secret returned by <c>/realtime/client_secrets</c>.</summary>
public sealed class XaiClientSecret
{
    /// <summary>Creates a client secret.</summary>
    public XaiClientSecret(string token, string url, long? expiresAt)
    {
        Token = token ?? string.Empty;
        Url = url ?? string.Empty;
        ExpiresAt = expiresAt;
    }

    /// <summary>Secret token.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL. The model is a query parameter.</summary>
    public string Url { get; }

    /// <summary>Unix expiration, when the response includes one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>
/// Settings of <see cref="XaiRealtimeModel"/>. Maps to <c>XaiRealtimeModelConfig</c>.
/// <see cref="Headers"/> maps the upstream <c>Resolvable</c>: an object, a promise, a callback or an async callback
/// all fit a function that returns a task.
/// </summary>
public sealed class XaiRealtimeModelConfig
{
    /// <summary>Creates a config.</summary>
    public XaiRealtimeModelConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = string.IsNullOrEmpty(baseUrl) ? XaiProvider.DefaultBaseUrl : baseUrl;
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Provider id, for example <c>xai.realtime</c>.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP client used to mint client secrets. Defaults to a shared client.</summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>xAI realtime voice model. Maps to <c>XaiRealtimeModel</c>.</summary>
public sealed class XaiRealtimeModel : IServerWebSocketRealtimeModel
{
    private static readonly Lazy<HttpClient> DefaultHttpClient = new(() => new HttpClient());

    private readonly XaiRealtimeModelConfig _config;

    /// <summary>Creates a realtime model.</summary>
    public XaiRealtimeModel(string modelId, XaiRealtimeModelConfig config)
    {
        ModelId = modelId ?? string.Empty;
        _config = config ?? throw new ArgumentNullException(nameof(config));
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
        return new Uri(ClientSecretWebSocketUrl());
    }

    /// <summary>WebSocket sessions are opened by the caller with <see cref="XaiClientSecret.Url"/>.</summary>
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("xAI realtime connects through the client-secret WebSocket URL.");
    }

    /// <summary>Posts to <c>/realtime/client_secrets</c> and returns a WebSocket URL that includes the model. Maps to <c>doCreateClientSecret</c>.</summary>
    public async Task<XaiClientSecret> CreateClientSecretAsync(
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        var body = new JsonObject();
        if (expiresAfterSeconds is { } seconds)
        {
            body["expires_after"] = new JsonObject { ["seconds"] = seconds };
        }

        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        var response = await ProviderExchange
            .SendAsync(
                _config.HttpClient ?? DefaultHttpClient.Value,
                HttpMethod.Post,
                ApiKeys.Combine(_config.BaseUrl, "/realtime/client_secrets"),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        using (
            var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            )
        )
        {
            var token =
                document.RootElement.TryGetProperty("value", out var value)
                && value.ValueKind == JsonValueKind.String
                    ? value.GetString() ?? string.Empty
                    : string.Empty;
            long? expires = null;
            if (
                document.RootElement.TryGetProperty("expires_at", out var expiresValue)
                && expiresValue.ValueKind == JsonValueKind.Number
            )
            {
                expires = expiresValue.GetInt64();
            }

            return new XaiClientSecret(token, ClientSecretWebSocketUrl(), expires);
        }
    }

    /// <summary>
    /// The URL and subprotocol a client opens with a client secret: the token travels as the
    /// <c>xai-client-secret.</c> subprotocol. Maps to <c>getWebSocketConfig</c>.
    /// </summary>
    public RealtimeModelV4WebSocketConfig GetWebSocketConfig(string token, string url)
    {
        return new RealtimeModelV4WebSocketConfig(url, ["xai-client-secret." + token]);
    }

    /// <inheritdoc />
    public Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(
        RealtimeModelV4SessionIntent? intent,
        CancellationToken cancellationToken
    )
    {
        // xAI exposes only the conversational voice endpoint; fail fast instead of
        // silently downgrading unsupported intents.
        AssertConversationIntent(intent);
        var url = new Uri(_config.BaseUrl);
        var protocol = url.Scheme == "http" ? "ws:" : "wss:";
        var path = url.AbsolutePath.TrimEnd('/');
        var serverUrl =
            protocol
            + "//"
            + url.Authority
            + path
            + "/realtime?model="
            + Uri.EscapeDataString(ModelId);
        return ResolveServerConnectionAsync(serverUrl, cancellationToken);
    }

    /// <summary>Maps one xAI server event.</summary>
    public RealtimeModelV4ServerEvent ParseServerEvent(JsonElement raw)
    {
        return XaiRealtimeEventMapper.ParseXaiRealtimeServerEvent(raw);
    }

    /// <summary>
    /// Serializes a client event. <c>conversation-item-truncate</c> is dropped (null) because xAI ignores it over
    /// WebSocket.
    /// </summary>
    public JsonObject? SerializeClientEvent(
        RealtimeModelV4ClientEvent clientEvent,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        AssertConversationIntent(intent);
        return XaiRealtimeEventMapper.SerializeXaiRealtimeClientEvent(clientEvent);
    }

    /// <summary>Builds the xAI <c>session</c> object for a normalized session config.</summary>
    public JsonObject BuildSessionConfig(
        RealtimeModelV4SessionConfig config,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        AssertConversationIntent(intent);
        return XaiRealtimeEventMapper.BuildXaiSessionConfig(config);
    }

    private static void AssertConversationIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent != null && intent != RealtimeModelV4SessionIntent.Conversation)
        {
            throw new AiSdkException(
                $"xAI realtime does not support the '{intent.Value.ToString().ToLowerInvariant()}' session intent."
            );
        }
    }

    private async Task<RealtimeModelV4ServerConnection> ResolveServerConnectionAsync(
        string url,
        CancellationToken cancellationToken
    )
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

        return new RealtimeModelV4ServerConnection(url, defined);
    }

    // xAI selects the voice model from the `model` query parameter on the
    // WebSocket URL. Without it the model choice is silently ignored and the
    // server falls back to its default voice model.
    private string ClientSecretWebSocketUrl()
    {
        return "wss://"
            + new Uri(_config.BaseUrl).Authority
            + "/v1/realtime?model="
            + Uri.EscapeDataString(ModelId);
    }
}
