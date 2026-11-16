// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway realtime model ids. Maps to <c>GatewayRealtimeModelId</c>; any other string is accepted too.</summary>
public static class GatewayRealtimeModelId
{
    /// <summary><c>elevenlabs/eleven-realtime-v1-max</c>.</summary>
    public const string ElevenlabsElevenRealtimeV1Max = "elevenlabs/eleven-realtime-v1-max";

    /// <summary><c>elevenlabs/eleven-realtime-v1-mini</c>.</summary>
    public const string ElevenlabsElevenRealtimeV1Mini = "elevenlabs/eleven-realtime-v1-mini";

    /// <summary><c>google/gemini-3.8-live</c>.</summary>
    public const string GoogleGemini38Live = "google/gemini-3.8-live";

    /// <summary><c>google/gemini-3.8-live-extended-thinking</c>.</summary>
    public const string GoogleGemini38LiveExtendedThinking = "google/gemini-3.8-live-extended-thinking";

    /// <summary><c>openai/gpt-live-1</c>.</summary>
    public const string OpenaiGptLive1 = "openai/gpt-live-1";

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
