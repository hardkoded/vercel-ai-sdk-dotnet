// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Provider;

/// <summary>
/// The realtime endpoint variant to open. Maps to <c>RealtimeModelV4SessionIntent</c>.
/// <see cref="Conversation"/> is the conversational voice endpoint; <see cref="Transcription"/> is a dedicated
/// transcription endpoint. Providers fail fast when an intent is not supported by their endpoint and codec.
/// </summary>
public enum RealtimeModelV4SessionIntent
{
    /// <summary>The conversational voice endpoint.</summary>
    Conversation,

    /// <summary>A dedicated transcription endpoint.</summary>
    Transcription,
}

/// <summary>
/// Descriptor for a server-initiated upstream WebSocket connection. Maps to <c>RealtimeModelV4ServerConnection</c>:
/// the URL to dial plus the headers (and optional subprotocols) to send on the handshake. It carries the
/// long-lived provider credential in <see cref="Headers"/> and is meant for a trusted server that owns the socket.
/// </summary>
public sealed class RealtimeModelV4ServerConnection
{
    /// <summary>Creates a server connection descriptor.</summary>
    public RealtimeModelV4ServerConnection(string url, IReadOnlyDictionary<string, string> headers, IReadOnlyList<string>? protocols = null)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Protocols = protocols;
    }

    /// <summary>The <c>wss://</c> (or <c>ws://</c>) URL, including provider-specific query parameters.</summary>
    public string Url { get; }

    /// <summary>Headers for the WebSocket handshake, including provider authentication.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Optional WebSocket subprotocols to negotiate.</summary>
    public IReadOnlyList<string>? Protocols { get; }
}

/// <summary>
/// A realtime model that can open a server-owned WebSocket. Maps to the optional
/// <c>RealtimeModelV4.getServerWebSocketConfig</c>. It is a separate interface because
/// <c>netstandard2.0</c> has no default interface members.
/// </summary>
public interface IServerWebSocketRealtimeModel : IRealtimeModel
{
    /// <summary>
    /// Returns the upstream WebSocket URL and auth headers for a server-initiated connection, using the
    /// credential the model was constructed with. Unsupported intents throw before any connection is made.
    /// </summary>
    Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent? intent, CancellationToken cancellationToken);
}
