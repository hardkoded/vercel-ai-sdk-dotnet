// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>A client secret minted through the Gateway. <see cref="ExpiresAt"/> is epoch seconds.</summary>
public sealed class GatewayMintedClientSecret
{
    /// <summary>Creates a minted secret.</summary>
    public GatewayMintedClientSecret(string token, long? expiresAt = null)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = expiresAt;
    }

    /// <summary>The <c>vcst_</c> token.</summary>
    public string Token { get; }

    /// <summary>Expiry in epoch seconds, when the endpoint returned one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>A client secret together with the WebSocket URL to open. Maps to <c>RealtimeModelV4ClientSecretResult</c>.</summary>
public sealed class GatewayRealtimeClientSecret
{
    /// <summary>Creates a client secret result.</summary>
    public GatewayRealtimeClientSecret(string token, string url, long? expiresAt = null)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        ExpiresAt = expiresAt;
    }

    /// <summary>The <c>vcst_</c> token.</summary>
    public string Token { get; }

    /// <summary>The realtime WebSocket URL.</summary>
    public string Url { get; }

    /// <summary>Expiry in epoch seconds, when known.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>URL and subprotocols a client opens a WebSocket with. Maps to the result of <c>getWebSocketConfig</c>.</summary>
public sealed class GatewayRealtimeWebSocketConfig
{
    /// <summary>Creates a WebSocket config.</summary>
    public GatewayRealtimeWebSocketConfig(string url, IReadOnlyList<string>? protocols)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Protocols = protocols;
    }

    /// <summary>The WebSocket URL.</summary>
    public string Url { get; }

    /// <summary>Subprotocols to offer.</summary>
    public IReadOnlyList<string>? Protocols { get; }
}

/// <summary>Configuration of a <see cref="GatewayRealtimeModel"/>. Maps to <c>GatewayRealtimeModelConfig</c>.</summary>
public sealed class GatewayRealtimeModelConfig
{
    /// <summary>Creates a config.</summary>
    public GatewayRealtimeModelConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> getHeaders,
        Func<string, int?, CancellationToken, Task<GatewayMintedClientSecret>> createClientSecret,
        string? teamIdOrSlug = null)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        GetHeaders = getHeaders ?? throw new ArgumentNullException(nameof(getHeaders));
        CreateClientSecret = createClientSecret ?? throw new ArgumentNullException(nameof(createClientSecret));
        TeamIdOrSlug = teamIdOrSlug;
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>HTTP(S) base URL of the Gateway V4 routes.</summary>
    public string BaseUrl { get; }

    /// <summary>Optional Vercel team scope.</summary>
    public string? TeamIdOrSlug { get; }

    /// <summary>Resolves the auth headers for a server-side connection.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> GetHeaders { get; }

    /// <summary>
    /// Mints a short-lived client secret (<c>vcst_</c>) through the Gateway's <c>/v1/realtime/client-secrets</c> endpoint.
    /// Arguments are the model id and <c>expiresAfterSeconds</c>. Implemented by the provider because minting needs the
    /// long-lived Gateway credential.
    /// </summary>
    public Func<string, int?, CancellationToken, Task<GatewayMintedClientSecret>> CreateClientSecret { get; }
}

/// <summary>
/// Realtime model backed by the AI Gateway. The Gateway normalizes realtime like every other modality, so this model is
/// a thin identity codec over the normalized protocol; only the connection and authentication are Gateway-specific.
/// </summary>
public sealed class GatewayRealtimeModel : IRealtimeModel, IServerWebSocketRealtimeModel
{
    private readonly GatewayRealtimeModelConfig _config;

    /// <summary>Creates a realtime model.</summary>
    public GatewayRealtimeModel(string modelId, GatewayRealtimeModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Specification version. Always <c>v4</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>The Gateway realtime WebSocket URL for this model.</summary>
    public Uri BuildUri()
    {
        return new Uri(ToGatewayRealtimeUrl(_config.BaseUrl, ModelId));
    }

    /// <summary>The Gateway connects through the client-secret or server WebSocket config, not through a session.</summary>
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("AI Gateway realtime connects through the WebSocket config from GetWebSocketConfig or GetServerWebSocketConfigAsync.");
    }

    /// <summary>
    /// Mints a single-use, short-lived client secret (<c>vcst_</c>) the browser uses to open the realtime WebSocket without
    /// holding the long-lived Gateway credential. <paramref name="expiresAfterSeconds"/> is forwarded to the mint endpoint.
    /// </summary>
    public async Task<GatewayRealtimeClientSecret> DoCreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
    {
        var secret = await _config.CreateClientSecret(ModelId, expiresAfterSeconds, cancellationToken).ConfigureAwait(false);
        return new GatewayRealtimeClientSecret(secret.Token, ToGatewayRealtimeUrl(_config.BaseUrl, ModelId), secret.ExpiresAt);
    }

    /// <summary>Returns the URL and the subprotocols that carry the token (a browser has no header channel).</summary>
    public GatewayRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
    {
        return new GatewayRealtimeWebSocketConfig(url, GatewayRealtimeAuth.GetGatewayRealtimeProtocols(token, _config.TeamIdOrSlug));
    }

    /// <inheritdoc />
    public async Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent? intent, CancellationToken cancellationToken)
    {
        AssertConversationIntent(intent);

        var headers = await _config.GetHeaders(cancellationToken).ConfigureAwait(false);
        var defined = new Dictionary<string, string>();
        foreach (var pair in headers)
        {
            if (pair.Value is not null)
            {
                defined[pair.Key] = pair.Value;
            }
        }

        return new RealtimeModelV4ServerConnection(ToGatewayRealtimeUrl(_config.BaseUrl, ModelId), defined);
    }

    /// <summary>The Gateway emits normalized events, so no provider-specific mapping is needed.</summary>
    public RealtimeModelV4ServerEvent ParseServerEvent(object raw)
    {
        return (RealtimeModelV4ServerEvent)raw;
    }

    /// <summary>The Gateway accepts normalized client events directly.</summary>
    public object SerializeClientEvent(RealtimeModelV4ClientEvent @event, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return @event;
    }

    /// <summary>The session config is already normalized; the Gateway maps it to the upstream payload server-side.</summary>
    public object BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return config;
    }

    private static void AssertConversationIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent is { } value && value != RealtimeModelV4SessionIntent.Conversation)
        {
            throw new InvalidOperationException(
                "AI Gateway realtime does not support the '" + value.ToString().ToLowerInvariant() + "' session intent on the normalized realtime route.");
        }
    }

    // The HTTP(S) base URL is upgraded to WS(S) and the model id rides the ?ai-model-id= query, passed through verbatim.
    private static string ToGatewayRealtimeUrl(string baseUrl, string modelId)
    {
        var wsBase = baseUrl.StartsWith("http", StringComparison.Ordinal) ? "ws" + baseUrl.Substring(4) : baseUrl;
        var url = new Uri(wsBase + "/realtime-model").AbsoluteUri;
        return url + "?ai-model-id=" + FormEncode(modelId);
    }

    // application/x-www-form-urlencoded, as URLSearchParams serializes.
    private static string FormEncode(string value)
    {
        var builder = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '*' || c == '-' || c == '.' || c == '_')
            {
                builder.Append(c);
            }
            else if (c == ' ')
            {
                builder.Append('+');
            }
            else
            {
                builder.Append('%').Append(((int)b).ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }
}
