// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayTranscriptionModelId</c>; any other string is accepted too.</summary>
public static class GatewayTranscriptionModelId
{
    /// <summary><c>elevenlabs/scribe-v2</c>.</summary>
    public const string ElevenlabsScribeV2 = "elevenlabs/scribe-v2";

    /// <summary><c>elevenlabs/scribe-v2-realtime</c>.</summary>
    public const string ElevenlabsScribeV2Realtime = "elevenlabs/scribe-v2-realtime";

    /// <summary><c>fish-audio/transcribe-1</c>.</summary>
    public const string FishAudioTranscribe1 = "fish-audio/transcribe-1";

    /// <summary><c>google/gemini-3.5-transcribe</c>.</summary>
    public const string GoogleGemini35Transcribe = "google/gemini-3.5-transcribe";

    /// <summary><c>google/gemini-3.5-transcribe-live</c>.</summary>
    public const string GoogleGemini35TranscribeLive = "google/gemini-3.5-transcribe-live";

    /// <summary><c>microsoft/mai-transcribe-2</c>.</summary>
    public const string MicrosoftMaiTranscribe2 = "microsoft/mai-transcribe-2";

    /// <summary><c>microsoft/mai-transcribe-2-streaming</c>.</summary>
    public const string MicrosoftMaiTranscribe2Streaming = "microsoft/mai-transcribe-2-streaming";

    /// <summary><c>mistral/voxtral-mini-transcribe-3</c>.</summary>
    public const string MistralVoxtralMiniTranscribe3 = "mistral/voxtral-mini-transcribe-3";

    /// <summary><c>mistral/voxtral-small-transcribe-3</c>.</summary>
    public const string MistralVoxtralSmallTranscribe3 = "mistral/voxtral-small-transcribe-3";

    /// <summary><c>openai/gpt-4o-mini-transcribe</c>.</summary>
    public const string OpenaiGpt4oMiniTranscribe = "openai/gpt-4o-mini-transcribe";

    /// <summary><c>openai/gpt-4o-transcribe</c>.</summary>
    public const string OpenaiGpt4oTranscribe = "openai/gpt-4o-transcribe";

    /// <summary><c>openai/gpt-live-transcribe</c>.</summary>
    public const string OpenaiGptLiveTranscribe = "openai/gpt-live-transcribe";

    /// <summary><c>openai/gpt-realtime-whisper</c>.</summary>
    public const string OpenaiGptRealtimeWhisper = "openai/gpt-realtime-whisper";

    /// <summary><c>openai/gpt-transcribe</c>.</summary>
    public const string OpenaiGptTranscribe = "openai/gpt-transcribe";

    /// <summary><c>openai/whisper-1</c>.</summary>
    public const string OpenaiWhisper1 = "openai/whisper-1";

    /// <summary><c>spacexai/grok-stt</c>.</summary>
    public const string SpacexaiGrokStt = "spacexai/grok-stt";
}
