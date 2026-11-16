// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Provider;

/// <summary>
/// URL and optional subprotocols a client opens a realtime WebSocket with. Maps to the result of
/// <c>RealtimeModelV4.getWebSocketConfig</c>.
/// </summary>
public sealed class RealtimeModelV4WebSocketConfig
{
    /// <summary>Creates a WebSocket config.</summary>
    public RealtimeModelV4WebSocketConfig(string url, IReadOnlyList<string>? protocols = null)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Protocols = protocols;
    }

    /// <summary>The WebSocket URL.</summary>
    public string Url { get; }

    /// <summary>Subprotocols to offer, when the provider authenticates through them.</summary>
    public IReadOnlyList<string>? Protocols { get; }
}
