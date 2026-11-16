// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>The data channel a Live WebRTC client opens. Maps to the result of <c>getWebRTCConfig</c>.</summary>
public sealed class OpenAIRealtimeModelLiveWebRTCConfig
{
    internal OpenAIRealtimeModelLiveWebRTCConfig(string dataChannelLabel)
    {
        DataChannelLabel = dataChannelLabel;
    }

    /// <summary>Label of the events data channel.</summary>
    public string DataChannelLabel { get; }
}

/// <summary>The answer of a Live WebRTC session creation. Maps to the result of <c>doCreateWebRTCSession</c>.</summary>
public sealed class OpenAIRealtimeModelLiveWebRTCSession
{
    internal OpenAIRealtimeModelLiveWebRTCSession(string sessionId, string sdp)
    {
        SessionId = sessionId;
        Sdp = sdp;
    }

    /// <summary>Session id.</summary>
    public string SessionId { get; }

    /// <summary>Answer SDP.</summary>
    public string Sdp { get; }
}

/// <summary>
/// OpenAI Live realtime model: a continuous conversation over a server WebSocket or WebRTC. Maps to
/// <c>OpenAIRealtimeModelLive</c>.
/// </summary>
public sealed class OpenAIRealtimeModelLive : IServerWebSocketRealtimeModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a Live realtime model.</summary>
    public OpenAIRealtimeModelLive(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <summary>The realtime model specification version. Maps to <c>specificationVersion</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".live";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>The conversation mode and the connection kinds this model supports. Maps to <c>capabilities</c>.</summary>
    public RealtimeCapabilities Capabilities { get; } = new()
    {
        Conversation = "continuous",
        Transports = ["websocket", "webrtc"],
        Connections = ["server-websocket", "webrtc"],
    };

    /// <summary>How a session starts. Maps to <c>capabilities.startup</c>.</summary>
    public string Startup => "session-start";

    /// <summary>How a session ends. Maps to <c>capabilities.finalization</c>.</summary>
    public string Finalization => "session-close";

    /// <inheritdoc />
    public Uri BuildUri()
    {
        return new Uri(BuildServerUrl());
    }

    /// <summary>Maps to <c>getWebRTCConfig</c>.</summary>
    public OpenAIRealtimeModelLiveWebRTCConfig GetWebRTCConfig()
    {
        return new OpenAIRealtimeModelLiveWebRTCConfig("oai-events");
    }

    /// <inheritdoc />
    public Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent? intent, CancellationToken cancellationToken)
    {
        AssertConversationIntent(intent);
        return Task.FromResult(new RealtimeModelV4ServerConnection(BuildServerUrl(), OpenAIRealtimeModel.ResolveHeaders(_provider)));
    }

    /// <summary>Exchanges a WebRTC offer for an answer. Maps to <c>doCreateWebRTCSession</c>.</summary>
    public async Task<OpenAIRealtimeModelLiveWebRTCSession> CreateWebRTCSessionAsync(
        string sdp,
        RealtimeModelV4SessionConfig? sessionConfig,
        CancellationToken cancellationToken)
    {
        var session = OpenAILiveSessionConfig.BuildOpenAILiveSessionConfig(
            sessionConfig ?? new RealtimeModelV4SessionConfig(),
            ModelId,
            OpenAILiveSessionConfig.WebRTCTransport);
        if (string.IsNullOrEmpty(sdp))
        {
            throw new InvalidArgumentException("sdp", sdp, "sdp must be a non-empty string.");
        }

        var body = new JsonObject
        {
            ["session"] = session,
            ["transport"] = new JsonObject { ["type"] = "webrtc", ["sdp"] = sdp },
        };
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "live/sessions"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        return ReadWebRTCSession(response.Body);
    }

    /// <summary>Parses a raw provider event into normalized events. Maps to <c>parseServerEvent</c>.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
    {
        return OpenAILiveEventMapper.ParseOpenAILiveServerEvent(raw);
    }

    /// <summary>Returns a server event parser. Maps to <c>createServerEventParser</c>.</summary>
    public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CreateServerEventParser()
    {
        return OpenAILiveEventMapper.CreateOpenAILiveServerEventParser();
    }

    /// <summary>Serializes a normalized client event into the provider wire format. Maps to <c>serializeClientEvent</c>.</summary>
    public JsonNode SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return OpenAILiveEventMapper.SerializeOpenAILiveClientEvent(clientEvent, ModelId);
    }

    /// <summary>Builds the provider session configuration from a normalized config. Maps to <c>buildSessionConfig</c>.</summary>
    public JsonObject BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
    {
        AssertConversationIntent(intent);
        return OpenAILiveSessionConfig.BuildOpenAILiveSessionConfig(config, ModelId);
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        throw new PlatformNotSupportedException("OpenAI Live WebSocket headers require .NET 10.");
#else
        return ConnectCoreAsync(cancellationToken);
#endif
    }

    private static OpenAIRealtimeModelLiveWebRTCSession ReadWebRTCSession(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("session", out var session)
                && session.ValueKind == JsonValueKind.Object
                && session.TryGetProperty("id", out var id)
                && id.ValueKind == JsonValueKind.String
                && id.GetString() is { Length: > 0 } sessionId
                && root.TryGetProperty("transport", out var transport)
                && transport.ValueKind == JsonValueKind.Object
                && transport.TryGetProperty("type", out var type)
                && type.ValueKind == JsonValueKind.String
                && type.GetString() == "webrtc"
                && transport.TryGetProperty("sdp", out var sdp)
                && sdp.ValueKind == JsonValueKind.String
                && sdp.GetString() is { Length: > 0 } answer)
            {
                return new OpenAIRealtimeModelLiveWebRTCSession(sessionId, answer);
            }
        }
        catch (JsonException)
        {
            // Reported below as an invalid response.
        }

        throw new ApiException("Invalid JSON response", 200, body);
    }

    private static void AssertConversationIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent != null && intent != RealtimeModelV4SessionIntent.Conversation)
        {
            throw new InvalidOperationException("OpenAI Live session intent '" + OpenAIRealtimeEventMapper.IntentName(intent.Value) + "' is not supported.");
        }
    }

    private string BuildServerUrl()
    {
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "live/sessions").AbsoluteUri;
        return url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
            ? "ws:" + url.Substring("http:".Length)
            : "wss:" + url.Substring(url.IndexOf(':') + 1);
    }

#if !NETSTANDARD2_0
    private async Task<IRealtimeSession> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        var socket = new System.Net.WebSockets.ClientWebSocket();
        try
        {
            foreach (var header in OpenAIRealtimeModel.ResolveHeaders(_provider))
            {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }

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
