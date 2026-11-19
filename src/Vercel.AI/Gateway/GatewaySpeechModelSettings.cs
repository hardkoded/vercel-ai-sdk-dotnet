// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewaySpeechModelId</c>; any other string is accepted too.</summary>
public static class GatewaySpeechModelId
{
    /// <summary><c>elevenlabs/eleven-flash-v2</c>.</summary>
    public const string ElevenlabsElevenFlashV2 = "elevenlabs/eleven-flash-v2";

    /// <summary><c>elevenlabs/eleven-flash-v2.5</c>.</summary>
    public const string ElevenlabsElevenFlashV25 = "elevenlabs/eleven-flash-v2.5";

    /// <summary><c>elevenlabs/eleven-multilingual-v2</c>.</summary>
    public const string ElevenlabsElevenMultilingualV2 = "elevenlabs/eleven-multilingual-v2";

    /// <summary><c>elevenlabs/eleven-v3</c>.</summary>
    public const string ElevenlabsElevenV3 = "elevenlabs/eleven-v3";

    /// <summary><c>elevenlabs/eleven-v3-conversational</c>.</summary>
    public const string ElevenlabsElevenV3Conversational = "elevenlabs/eleven-v3-conversational";

    /// <summary><c>elevenlabs/eleven-v4</c>.</summary>
    public const string ElevenlabsElevenV4 = "elevenlabs/eleven-v4";

    /// <summary><c>elevenlabs/eleven-v4-turbo</c>.</summary>
    public const string ElevenlabsElevenV4Turbo = "elevenlabs/eleven-v4-turbo";

    /// <summary><c>elevenlabs/music-v2</c>.</summary>
    public const string ElevenlabsMusicV2 = "elevenlabs/music-v2";

    /// <summary><c>elevenlabs/music-v2.5</c>.</summary>
    public const string ElevenlabsMusicV25 = "elevenlabs/music-v2.5";

    /// <summary><c>elevenlabs/sound-effects-v2</c>.</summary>
    public const string ElevenlabsSoundEffectsV2 = "elevenlabs/sound-effects-v2";

    /// <summary><c>fish-audio/drama-3</c>.</summary>
    public const string FishAudioDrama3 = "fish-audio/drama-3";

    /// <summary><c>fish-audio/s1</c>.</summary>
    public const string FishAudioS1 = "fish-audio/s1";

    /// <summary><c>fish-audio/s2-pro</c>.</summary>
    public const string FishAudioS2Pro = "fish-audio/s2-pro";

    /// <summary><c>fish-audio/s2.1-pro</c>.</summary>
    public const string FishAudioS21Pro = "fish-audio/s2.1-pro";

    /// <summary><c>google/gemini-3.8-flash-lite-tts</c>.</summary>
    public const string GoogleGemini38FlashLiteTts = "google/gemini-3.8-flash-lite-tts";

    /// <summary><c>google/gemini-3.8-flash-tts</c>.</summary>
    public const string GoogleGemini38FlashTts = "google/gemini-3.8-flash-tts";

    /// <summary><c>microsoft/mai-voice-2</c>.</summary>
    public const string MicrosoftMaiVoice2 = "microsoft/mai-voice-2";

    /// <summary><c>microsoft/mai-voice-2-flash</c>.</summary>
    public const string MicrosoftMaiVoice2Flash = "microsoft/mai-voice-2-flash";

    /// <summary><c>microsoft/mai-voice-2.1</c>.</summary>
    public const string MicrosoftMaiVoice21 = "microsoft/mai-voice-2.1";

    /// <summary><c>microsoft/mai-voice-2.1-flash</c>.</summary>
    public const string MicrosoftMaiVoice21Flash = "microsoft/mai-voice-2.1-flash";

    /// <summary><c>mistral/voxtral-mini-tts-3</c>.</summary>
    public const string MistralVoxtralMiniTts3 = "mistral/voxtral-mini-tts-3";

    /// <summary><c>openai/tts-1</c>.</summary>
    public const string OpenaiTts1 = "openai/tts-1";

    /// <summary><c>openai/tts-1-hd</c>.</summary>
    public const string OpenaiTts1Hd = "openai/tts-1-hd";

    /// <summary><c>spacexai/grok-tts</c>.</summary>
    public const string SpacexaiGrokTts = "spacexai/grok-tts";
}
