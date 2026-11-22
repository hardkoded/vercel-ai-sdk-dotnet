// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayRealtimeModelId</c>; any other string is accepted too.</summary>
public static class GatewayRealtimeModelId
{
    /// <summary><c>elevenlabs/eleven-realtime-v1-max</c>.</summary>
    public const string ElevenlabsElevenRealtimeV1Max = "elevenlabs/eleven-realtime-v1-max";

    /// <summary><c>elevenlabs/eleven-realtime-v1-mini</c>.</summary>
    public const string ElevenlabsElevenRealtimeV1Mini = "elevenlabs/eleven-realtime-v1-mini";

    /// <summary><c>google/gemini-3.8-live</c>.</summary>
    public const string GoogleGemini38Live = "google/gemini-3.8-live";

    /// <summary><c>google/gemini-3.8-live-extended-thinking</c>.</summary>
    public const string GoogleGemini38LiveExtendedThinking =
        "google/gemini-3.8-live-extended-thinking";

    /// <summary><c>openai/gpt-live-1</c>.</summary>
    public const string OpenaiGptLive1 = "openai/gpt-live-1";

    /// <summary><c>openai/gpt-live-1.1</c>.</summary>
    public const string OpenaiGptLive11 = "openai/gpt-live-1.1";

    /// <summary><c>openai/gpt-realtime-1.5</c>.</summary>
    public const string OpenaiGptRealtime15 = "openai/gpt-realtime-1.5";

    /// <summary><c>openai/gpt-realtime-2</c>.</summary>
    public const string OpenaiGptRealtime2 = "openai/gpt-realtime-2";

    /// <summary><c>openai/gpt-realtime-2.1</c>.</summary>
    public const string OpenaiGptRealtime21 = "openai/gpt-realtime-2.1";

    /// <summary><c>openai/gpt-realtime-mini</c>.</summary>
    public const string OpenaiGptRealtimeMini = "openai/gpt-realtime-mini";

    /// <summary><c>spacexai/grok-voice-3.0</c>.</summary>
    public const string SpacexaiGrokVoice30 = "spacexai/grok-voice-3.0";
}

/// <summary>
/// Realtime model ids that use the continuous Live API by default. Maps to <c>gatewayLiveModelIds</c> and
/// <c>GatewayKnownLiveModelId</c>. Other ids use the realtime API unless <see cref="GatewayRealtimeApi.Live"/> is passed.
/// </summary>
public static class GatewayKnownLiveModelId
{
    /// <summary>Maps to <c>gatewayLiveModelIds</c>.</summary>
    public static IReadOnlyList<string> GatewayLiveModelIds { get; } =
    [GatewayRealtimeModelId.OpenaiGptLive1, GatewayRealtimeModelId.OpenaiGptLive11];
}

/// <summary>The Gateway API a realtime model id is routed to. Maps to <c>GatewayRealtimeOptions.api</c>.</summary>
public enum GatewayRealtimeApi
{
    /// <summary><c>'live'</c>: the continuous Live API on <c>/live-model</c>.</summary>
    Live,

    /// <summary><c>'realtime'</c>: the turn-based realtime API on <c>/realtime-model</c>.</summary>
    Realtime,
}

/// <summary>Maps to <c>GatewayRealtimeOptions</c>.</summary>
public sealed class GatewayRealtimeOptions
{
    /// <summary>Selects the Gateway API for the model id, including for new Live models. Maps to <c>api</c>.</summary>
    public GatewayRealtimeApi? Api { get; init; }
}
