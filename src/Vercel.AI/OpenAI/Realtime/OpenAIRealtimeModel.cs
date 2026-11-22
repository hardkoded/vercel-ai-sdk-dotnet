// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>A realtime client secret.</summary>
public sealed class OpenAIRealtimeClientSecret
{
    internal OpenAIRealtimeClientSecret(string token, string url, long? expiresAt)
    {
        Token = token;
        Url = url;
        ExpiresAt = expiresAt;
    }

    /// <summary>Client secret value.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL that includes the model.</summary>
    public string Url { get; }

    /// <summary>Expiry as unix seconds, when the provider sent one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>URL and subprotocols a client opens a WebSocket with. Maps to the result of <c>getWebSocketConfig</c>.</summary>
public sealed class OpenAIRealtimeWebSocketConfig
{
    internal OpenAIRealtimeWebSocketConfig(string url, IReadOnlyList<string>? protocols)
    {
        Url = url;
        Protocols = protocols;
    }

    /// <summary>The WebSocket URL.</summary>
    public string Url { get; }

    /// <summary>Subprotocols to offer.</summary>
    public IReadOnlyList<string>? Protocols { get; }
}

/// <summary>
/// Settings of <see cref="OpenAIRealtimeModel"/>. Maps to <c>OpenAIRealtimeModelConfig</c>.
/// <see cref="Headers"/> maps the upstream <c>Resolvable</c>: an object, a promise, a callback or an async callback
/// all fit a function that returns a task.
/// </summary>
public sealed class OpenAIRealtimeModelConfig
{
    /// <summary>Creates a config.</summary>
    public OpenAIRealtimeModelConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Provider id, for example <c>openai.realtime</c>.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP client used to mint client secrets. Defaults to a shared client.</summary>
    public HttpClient? HttpClient { get; set; }

    internal ProviderHttp? Http { get; set; }
}

/// <summary>OpenAI Realtime session URI builder and WebSocket client.</summary>
public sealed class OpenAIRealtimeModel : IServerWebSocketRealtimeModel
{
    private static readonly Lazy<HttpClient> DefaultHttpClient = new(() => new HttpClient());

    private readonly OpenAIRealtimeModelConfig _config;
    private readonly ProviderHttp _http;

    /// <summary>Creates a realtime model from a provider.</summary>
    public OpenAIRealtimeModel(OpenAIProvider provider, string modelId)
        : this(
            modelId,
            new OpenAIRealtimeModelConfig(
                (provider ?? throw new ArgumentNullException(nameof(provider))).Name + ".realtime",
                provider.Options.BaseUrl,
                async cancellationToken =>
                    (IReadOnlyDictionary<string, string?>)
                        await provider
                            .CreateOpenAIHeadersAsync(null, cancellationToken)
                            .ConfigureAwait(false)
            )
            {
                Http = provider.Http,
            }
        ) { }

    /// <summary>Creates a realtime model.</summary>
    public OpenAIRealtimeModel(string modelId, OpenAIRealtimeModelConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        ModelId = modelId;
        _http = config.Http ?? new ProviderHttp(config.HttpClient ?? DefaultHttpClient.Value);
    }

    /// <summary>The model configuration (base URL, headers, transport). Maps to <c>config</c>.</summary>
    internal OpenAIRealtimeModelConfig Config => _config;

    /// <summary>The realtime model specification version. Maps to <c>specificationVersion</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>
    /// The conversation mode and the connection kinds this model supports. Maps to <c>capabilities</c>.
    /// </summary>
    public RealtimeCapabilities Capabilities { get; } =
        new()
        {
            Conversation = "turn-based",
            Transports = ["websocket"],
            Connections = ["client-secret-websocket", "server-websocket"],
        };

    /// <inheritdoc />
    public Uri BuildUri()
    {
        var http = ApiKeys.Combine(_config.BaseUrl, "realtime");
        var builder = new UriBuilder(http)
        {
            Scheme = http.Scheme == "https" ? "wss" : "ws",
            Query = "model=" + Uri.EscapeDataString(ModelId),
        };
        return builder.Uri;
    }

    /// <summary>Requests a realtime client secret. <paramref name="expiresAfterSeconds"/> is omitted when null.</summary>
    public async Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        return await CreateClientSecretAsync(null, expiresAfterSeconds, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Requests a realtime client secret for a session built from <paramref name="sessionConfig"/>.
    /// Maps to <c>doCreateClientSecret</c>.
    /// </summary>
    public async Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(
        RealtimeModelV4SessionConfig? sessionConfig,
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        var session =
            sessionConfig != null
                ? OpenAIRealtimeEventMapper.BuildOpenAISessionConfig(sessionConfig, ModelId)
                : new JsonObject { ["type"] = "realtime", ["model"] = ModelId };
        var body = new JsonObject { ["session"] = session };
        if (expiresAfterSeconds != null)
        {
            // `anchor` is required by the client secrets endpoint; without it the request fails with
            // "Missing required parameter: 'expires_after.anchor'".
            body["expires_after"] = new JsonObject
            {
                ["anchor"] = "created_at",
                ["seconds"] = expiresAfterSeconds.Value,
            };
        }

        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        var response = await _http
            .SendJsonStringAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_config.BaseUrl, "realtime/client_secrets"),
                body.ToJsonString(),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var token = document.RootElement.TryGetProperty("value", out var value)
            ? value.GetString() ?? string.Empty
            : string.Empty;
        long? expires = null;
        if (
            document.RootElement.TryGetProperty("expires_at", out var expiresElement)
            && expiresElement.TryGetInt64(out var seconds)
        )
        {
            expires = seconds;
        }

        var host = new Uri(_config.BaseUrl).Authority;
        var url = "wss://" + host + "/v1/realtime?model=" + Uri.EscapeDataString(ModelId);
        return new OpenAIRealtimeClientSecret(token, url, expires);
    }

    /// <summary>Returns the URL and the subprotocols that carry the token (a browser has no header channel). Maps to <c>getWebSocketConfig</c>.</summary>
    public OpenAIRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
    {
        return new OpenAIRealtimeWebSocketConfig(
            url,
            ["realtime", "openai-insecure-api-key." + token]
        );
    }

    /// <inheritdoc />
    public async Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(
        RealtimeModelV4SessionIntent? intent,
        CancellationToken cancellationToken
    )
    {
        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        var url = BuildServerUrl(
            _config.BaseUrl,
            ModelId,
            intent ?? RealtimeModelV4SessionIntent.Conversation
        );
        return new RealtimeModelV4ServerConnection(url, RemoveUndefinedEntries(headers));
    }

    /// <summary>Parses a raw provider event into a normalized event. Maps to <c>parseServerEvent</c>.</summary>
    public RealtimeModelV4ServerEvent ParseServerEvent(JsonElement raw)
    {
        return OpenAIRealtimeEventMapper.ParseOpenAIRealtimeServerEvent(raw);
    }

    /// <summary>Serializes a normalized client event into the provider wire format. Maps to <c>serializeClientEvent</c>.</summary>
    public JsonNode? SerializeClientEvent(
        RealtimeModelV4ClientEvent clientEvent,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        return OpenAIRealtimeEventMapper.SerializeOpenAIRealtimeClientEvent(
            clientEvent,
            ModelId,
            new OpenAIRealtimeSessionOptions { Intent = intent }
        );
    }

    /// <summary>Builds the provider session configuration from a normalized config. Maps to <c>buildSessionConfig</c>.</summary>
    public JsonObject BuildSessionConfig(
        RealtimeModelV4SessionConfig config,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        return OpenAIRealtimeEventMapper.BuildOpenAISessionConfig(
            config,
            ModelId,
            new OpenAIRealtimeSessionOptions { Intent = intent }
        );
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        throw new PlatformNotSupportedException(
            "OpenAI Realtime WebSocket headers require .NET 10."
        );
#else
        return ConnectCoreAsync(cancellationToken);
#endif
    }

    /// <summary>Resolves the provider headers, dropping entries without a value. Maps to <c>removeUndefinedEntries(headers)</c>.</summary>
    internal static async Task<Dictionary<string, string>> ResolveHeadersAsync(
        OpenAIProvider provider,
        CancellationToken cancellationToken
    )
    {
        return RemoveUndefinedEntries(
            await provider.CreateOpenAIHeadersAsync(null, cancellationToken).ConfigureAwait(false)
        );
    }

    private static Dictionary<string, string> RemoveUndefinedEntries(
        IReadOnlyDictionary<string, string?> source
    )
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in source)
        {
            if (pair.Value != null)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    /// <summary>
    /// Builds the upstream realtime WebSocket URL for a server-initiated connection. Transcription keeps the model in
    /// <c>session.update</c>; a <c>?model=</c> here would open a voice session instead. Maps to <c>buildServerUrl</c>.
    /// </summary>
    private static string BuildServerUrl(
        string baseUrl,
        string modelId,
        RealtimeModelV4SessionIntent intent
    )
    {
        var url = new Uri(baseUrl);
        var scheme = url.Scheme == "http" ? "ws:" : "wss:";
        var path = url.AbsolutePath;
        if (path.EndsWith("/", StringComparison.Ordinal))
        {
            path = path.Substring(0, path.Length - 1);
        }

        var root = scheme + "//" + url.Authority + path;
        switch (intent)
        {
            case RealtimeModelV4SessionIntent.Transcription:
                return root + "/realtime?intent=transcription";
            case RealtimeModelV4SessionIntent.Conversation:
                return root + "/realtime?model=" + Uri.EscapeDataString(modelId);
            default:
                throw new InvalidOperationException(
                    "OpenAI realtime session intent '"
                        + OpenAIRealtimeEventMapper.IntentName(intent)
                        + "' is not supported."
                );
        }
    }

#if !NETSTANDARD2_0
    private async Task<IRealtimeSession> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        var socket = new System.Net.WebSockets.ClientWebSocket();
        try
        {
            var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
            socket.Options.SetRequestHeader(
                "Authorization",
                headers["Authorization"] ?? string.Empty
            );
            socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
            await socket.ConnectAsync(BuildUri(), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            socket.Dispose();
            throw;
        }

        return new WebSocketRealtimeSession(socket);
    }
#endif
}
