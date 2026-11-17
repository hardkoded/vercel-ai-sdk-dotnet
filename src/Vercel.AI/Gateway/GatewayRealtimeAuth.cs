// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.Gateway;

/// <summary>
/// Shared WebSocket subprotocol contract for AI Gateway realtime, streaming transcription, and streaming speech
/// auth. Maps to <c>gateway-realtime-auth.ts</c>. The Gateway auth (bearer) token travels through the
/// <c>Sec-WebSocket-Protocol</c> handshake because a browser <c>WebSocket</c> cannot set request headers.
/// </summary>
public static class GatewayRealtimeAuth
{
    /// <summary>Marker subprotocol offered on every realtime handshake. Maps to <c>GATEWAY_REALTIME_SUBPROTOCOL</c>.</summary>
    public const string GatewayRealtimeSubprotocol = "ai-gateway-realtime.v1";

    /// <summary>Marker subprotocol for streaming transcription handshakes. Maps to <c>GATEWAY_TRANSCRIPTION_SUBPROTOCOL</c>.</summary>
    public const string GatewayTranscriptionSubprotocol = "ai-gateway-transcription.v1";

    /// <summary>Marker subprotocol for streaming speech handshakes. Maps to <c>GATEWAY_SPEECH_SUBPROTOCOL</c>.</summary>
    public const string GatewaySpeechSubprotocol = "ai-gateway-speech.v1";

    /// <summary>Subprotocol prefix that carries the Gateway auth (bearer) token. Maps to <c>GATEWAY_AUTH_SUBPROTOCOL_PREFIX</c>.</summary>
    public const string GatewayAuthSubprotocolPrefix = "ai-gateway-auth.";

    /// <summary>Subprotocol prefix that carries optional Vercel team scoping. Maps to <c>GATEWAY_TEAM_SUBPROTOCOL_PREFIX</c>.</summary>
    public const string GatewayTeamSubprotocolPrefix = "ai-gateway-team.";

    /// <summary>Builds the WebSocket subprotocols that carry <paramref name="token"/> to the Gateway.</summary>
    public static IReadOnlyList<string> GetGatewayRealtimeProtocols(
        string token,
        string? teamIdOrSlug = null
    )
    {
        return BuildGatewayProtocols(GatewayRealtimeSubprotocol, token, teamIdOrSlug);
    }

    /// <summary>Like <see cref="GetGatewayRealtimeProtocols"/>, with the streaming transcription marker subprotocol.</summary>
    public static IReadOnlyList<string> GetGatewayTranscriptionProtocols(
        string token,
        string? teamIdOrSlug = null
    )
    {
        return BuildGatewayProtocols(GatewayTranscriptionSubprotocol, token, teamIdOrSlug);
    }

    /// <summary>Like <see cref="GetGatewayRealtimeProtocols"/>, with the streaming speech marker subprotocol.</summary>
    public static IReadOnlyList<string> GetGatewaySpeechProtocols(
        string token,
        string? teamIdOrSlug = null
    )
    {
        return BuildGatewayProtocols(GatewaySpeechSubprotocol, token, teamIdOrSlug);
    }

    /// <summary>
    /// Server side: extracts the Gateway auth (bearer) token from a <c>Sec-WebSocket-Protocol</c> header value, or null
    /// when it is absent or empty.
    /// </summary>
    public static string? GetGatewayRealtimeAuthToken(string? secWebSocketProtocol)
    {
        var token = FindProtocol(secWebSocketProtocol, GatewayAuthSubprotocolPrefix)
            ?.Substring(GatewayAuthSubprotocolPrefix.Length);
        return string.IsNullOrEmpty(token) ? null : token;
    }

    /// <summary>
    /// Server side: extracts the optional Vercel team ID or slug from a <c>Sec-WebSocket-Protocol</c> header value. The
    /// team is base64url-encoded.
    /// </summary>
    public static string? GetGatewayRealtimeTeamIdOrSlug(string? secWebSocketProtocol)
    {
        var encoded = FindProtocol(secWebSocketProtocol, GatewayTeamSubprotocolPrefix)
            ?.Substring(GatewayTeamSubprotocolPrefix.Length);
        if (string.IsNullOrEmpty(encoded))
        {
            return null;
        }

        try
        {
            var decoded = DecodeSubprotocolValue(encoded!);
            return string.IsNullOrEmpty(decoded) ? null : decoded;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> BuildGatewayProtocols(
        string marker,
        string token,
        string? teamIdOrSlug
    )
    {
        var protocols = new List<string> { marker, GatewayAuthSubprotocolPrefix + token };
        if (!string.IsNullOrEmpty(teamIdOrSlug))
        {
            protocols.Add(GatewayTeamSubprotocolPrefix + EncodeSubprotocolValue(teamIdOrSlug!));
        }

        return protocols;
    }

    private static string? FindProtocol(string? secWebSocketProtocol, string prefix)
    {
        if (secWebSocketProtocol is null)
        {
            return null;
        }

        foreach (var part in secWebSocketProtocol.Split(','))
        {
            var protocol = part.Trim();
            if (protocol.StartsWith(prefix, StringComparison.Ordinal))
            {
                return protocol;
            }
        }

        return null;
    }

    private static string EncodeSubprotocolValue(string value)
    {
        return Convert
            .ToBase64String(Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string DecodeSubprotocolValue(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 += new string('=', (4 - (base64.Length % 4)) % 4);
        return new UTF8Encoding(false, true).GetString(Convert.FromBase64String(base64));
    }
}
