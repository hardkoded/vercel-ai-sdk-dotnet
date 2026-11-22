// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ElevenLabs;

/// <summary>Request configuration of one ElevenLabs speech model. Maps to <c>requestConfig.speech</c>.</summary>
internal sealed class ElevenLabsSpeechRequestConfig
{
    /// <summary>Creates the configuration.</summary>
    public ElevenLabsSpeechRequestConfig(
        bool music,
        bool soundEffects,
        bool dialogue,
        bool defaultDialogue,
        bool singleVoiceDialogue,
        bool incrementalDialogue
    )
    {
        Music = music;
        SoundEffects = soundEffects;
        Dialogue = dialogue;
        DefaultDialogue = defaultDialogue;
        SingleVoiceDialogue = singleVoiceDialogue;
        IncrementalDialogue = incrementalDialogue;
    }

    /// <summary>True for <c>music_*</c> models.</summary>
    public bool Music { get; }

    /// <summary>True for <c>eleven_text_to_sound*</c> models.</summary>
    public bool SoundEffects { get; }

    /// <summary>True for <c>eleven_v3*</c> and <c>eleven_v4*</c> models.</summary>
    public bool Dialogue { get; }

    /// <summary>True when a plain text call is sent as a single-input dialogue.</summary>
    public bool DefaultDialogue { get; }

    /// <summary>True when streaming dialogue accepts exactly one voice.</summary>
    public bool SingleVoiceDialogue { get; }

    /// <summary>True when streaming dialogue accepts incremental messages.</summary>
    public bool IncrementalDialogue { get; }
}

/// <summary>ElevenLabs model catalog. Maps to <c>elevenlabs-model-catalog.ts</c>.</summary>
internal static class ElevenLabsModelCatalog
{
    /// <summary>Provider options that only apply to text-to-speech. Maps to <c>elevenLabsTextToSpeechOnlyOptions</c>.</summary>
    internal static readonly IReadOnlyList<string> ElevenLabsTextToSpeechOnlyOptions =
    [
        "applyLanguageTextNormalization",
        "applyTextNormalization",
        "dialogue",
        "inputs",
        "languageCode",
        "nextRequestIds",
        "nextText",
        "previousRequestIds",
        "previousText",
        "pronunciationDictionaryLocators",
        "streaming",
        "voiceSettings",
    ];

    /// <summary>Provider options that only apply to music. Maps to <c>elevenLabsMusicOnlyOptions</c>.</summary>
    internal static readonly IReadOnlyList<string> ElevenLabsMusicOnlyOptions =
    [
        "compositionPlan",
        "forceInstrumental",
        "musicLengthMs",
    ];

    /// <summary>Provider options that only apply to sound effects. Maps to <c>elevenLabsSoundEffectsOnlyOptions</c>.</summary>
    internal static readonly IReadOnlyList<string> ElevenLabsSoundEffectsOnlyOptions =
    [
        "durationSeconds",
        "loop",
        "promptInfluence",
    ];

    /// <summary>Returns only <c>requestConfig.speech</c> of upstream <c>getElevenLabsSpeechModelEntry</c>. The full entry (capabilities, providerOptions) is not ported (#1681); internal until it is.</summary>
    internal static ElevenLabsSpeechRequestConfig GetElevenLabsSpeechModelEntry(string modelId)
    {
        return new ElevenLabsSpeechRequestConfig(
            music: modelId.StartsWith("music_", StringComparison.Ordinal),
            soundEffects: modelId.StartsWith("eleven_text_to_sound", StringComparison.Ordinal),
            dialogue: modelId.StartsWith("eleven_v3", StringComparison.Ordinal)
                || modelId.StartsWith("eleven_v4", StringComparison.Ordinal),
            defaultDialogue: modelId == "eleven_v4",
            singleVoiceDialogue: modelId == "eleven_v3_conversational",
            incrementalDialogue: modelId.StartsWith("eleven_v4", StringComparison.Ordinal)
        );
    }
}
