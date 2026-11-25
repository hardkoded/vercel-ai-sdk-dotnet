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

/// <summary>Known options accepted under <c>providerOptions.elevenlabs</c>. Maps to <c>elevenLabsProviderOptions</c>.</summary>
internal static class ElevenLabsProviderOptions
{
    /// <summary>Maps to <c>elevenLabsProviderOptions.speech</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Speech =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["withTimestamps"] = new(
                "Return character alignment for speech or lyric word timestamps for music."
            ),
            ["languageCode"] = new("Language code."),
            ["voiceSettings"] = new("Voice settings."),
            ["pronunciationDictionaryLocators"] = new("Pronunciation dictionary locators."),
            ["seed"] = new("Seed."),
            ["previousText"] = new("Previous text."),
            ["nextText"] = new("Next text."),
            ["previousRequestIds"] = new("Previous request ids."),
            ["nextRequestIds"] = new("Next request ids."),
            ["applyTextNormalization"] = new("Apply text normalization."),
            ["applyLanguageTextNormalization"] = new("Apply language text normalization."),
            ["enableLogging"] = new("Enable logging."),
            ["streaming"] = new("WebSocket speech settings; not used by non-streaming generation."),
            ["dialogue"] = new("Voice settings for streaming text-to-dialogue."),
            ["inputs"] = new("Text and voice pairs for non-streaming text-to-dialogue."),
            ["musicLengthMs"] = new("Music length ms."),
            ["forceInstrumental"] = new("Force instrumental."),
            ["compositionPlan"] = new("Native music composition plan; replaces the text prompt."),
            ["durationSeconds"] = new("Duration seconds."),
            ["promptInfluence"] = new("Prompt influence."),
            ["loop"] = new("Loop."),
        };

    /// <summary>Maps to <c>elevenLabsProviderOptions.transcription</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Transcription =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["languageCode"] = new("Language hint for transcription."),
            ["tagAudioEvents"] = new("Include audio event tags in batch transcripts."),
            ["numSpeakers"] = new("Expected speaker count for batch transcription."),
            ["timestampsGranularity"] = new("Timestamp precision for batch transcription."),
            ["diarize"] = new("Identify speakers in batch transcripts."),
            ["fileFormat"] = new("Input audio format for batch transcription."),
            ["sourceUrl"] = new("Transcribe a remote audio source in batch mode."),
            ["streaming"] = new("Scribe realtime transcription settings."),
        };
}

/// <summary>Maps to the <c>speech</c> capabilities of <c>getElevenLabsSpeechModelEntry</c>.</summary>
internal sealed class ElevenLabsSpeechCapabilities
{
    /// <summary>Maps to <c>instructions</c>.</summary>
    public ModelCapability Instructions { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>speech</c>.</summary>
    public bool Speech { get; init; }

    /// <summary>Maps to <c>streaming</c>.</summary>
    public ModelCapability Streaming { get; init; } = ModelCapability.Unknown;
}

/// <summary>The catalog entry of an ElevenLabs speech model. Maps to the result of <c>getElevenLabsSpeechModelEntry</c>.</summary>
internal sealed class ElevenLabsSpeechModelEntry
{
    internal ElevenLabsSpeechModelEntry(
        ElevenLabsSpeechCapabilities capabilities,
        IReadOnlyDictionary<string, ModelCapability> providerOptions,
        ElevenLabsSpeechRequestConfig requestConfig
    )
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
        RequestConfig = requestConfig;
    }

    /// <summary>Maps to <c>capabilities.speech</c>.</summary>
    public ElevenLabsSpeechCapabilities Capabilities { get; }

    /// <summary>Maps to <c>providerOptions.speech</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }

    /// <summary>Maps to <c>requestConfig.speech</c>.</summary>
    public ElevenLabsSpeechRequestConfig RequestConfig { get; }
}

/// <summary>ElevenLabs model catalog. Maps to <c>elevenlabs-model-catalog.ts</c>.</summary>
internal static class ElevenLabsModelCatalog
{
    /// <summary>Maps to <c>elevenLabsSpeechModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> ElevenLabsSpeechModelIds =
    [
        "eleven_v4",
        "eleven_v4_turbo",
        "eleven_v3",
        "eleven_v3_conversational",
        "eleven_multilingual_v2",
        "eleven_flash_v2_5",
        "eleven_flash_v2",
        "eleven_turbo_v2_5",
        "eleven_turbo_v2",
        "eleven_monolingual_v1",
        "eleven_multilingual_v1",
        "music_v2_5",
        "music_v2",
        "music_v1",
        "eleven_text_to_sound_v2",
        "eleven_text_to_sound_v3",
    ];

    /// <summary>Maps to <c>elevenLabsTranscriptionModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> ElevenLabsTranscriptionModelIds =
    [
        "scribe_v1",
        "scribe_v1_experimental",
        "scribe_v2",
        "scribe_v2_realtime",
    ];

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

    /// <summary>Returns the entry of a speech model. Maps to <c>getElevenLabsSpeechModelEntry</c>.</summary>
    internal static ElevenLabsSpeechModelEntry GetElevenLabsSpeechModelEntry(string modelId)
    {
        var music = modelId.StartsWith("music_", StringComparison.Ordinal);
        var soundEffects = modelId.StartsWith("eleven_text_to_sound", StringComparison.Ordinal);
        var dialogue =
            modelId.StartsWith("eleven_v3", StringComparison.Ordinal)
            || modelId.StartsWith("eleven_v4", StringComparison.Ordinal);
        var tts = !music && !soundEffects;
        var known = ElevenLabsSpeechModelIds.Contains(modelId);
        var defaultDialogue = modelId == "eleven_v4";
        var singleVoiceDialogue = modelId == "eleven_v3_conversational";
        var streamingDisabled = modelId is "eleven_monolingual_v1" or "eleven_multilingual_v1";
        var support = known ? ModelCapability.Supported : ModelCapability.Unknown;

        var streaming =
            music || soundEffects || streamingDisabled ? ModelCapability.Unsupported
            : dialogue ? ModelCapability.When("Requires providerOptions.elevenlabs.dialogue.")
            : ModelCapability.Supported;

        var providerOptions = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var key in ElevenLabsProviderOptions.Speech.Keys)
        {
            providerOptions[key] = support;
        }

        foreach (var key in ElevenLabsTextToSpeechOnlyOptions)
        {
            providerOptions[key] = tts ? support : ModelCapability.Unsupported;
        }

        foreach (var key in ElevenLabsMusicOnlyOptions)
        {
            providerOptions[key] = ModelCapability.From(music);
        }

        foreach (var key in ElevenLabsSoundEffectsOnlyOptions)
        {
            providerOptions[key] = ModelCapability.From(soundEffects);
        }

        providerOptions["seed"] = ModelCapability.From(!soundEffects);
        providerOptions["withTimestamps"] = ModelCapability.From(!soundEffects);
        providerOptions["dialogue"] = dialogue
            ? ModelCapability.When(
                "Streaming dialogue only; eleven_v3_conversational accepts one voice."
            )
            : ModelCapability.Unsupported;
        providerOptions["streaming"] = tts
            ? ModelCapability.When("Streaming speech only.")
            : ModelCapability.Unsupported;
        providerOptions["inputs"] = tts
            ? ModelCapability.When("Non-streaming text-to-dialogue only.")
            : ModelCapability.Unsupported;
        providerOptions["compositionPlan"] = music
            ? ModelCapability.When(
                "Music only; cannot be combined with musicLengthMs or forceInstrumental."
            )
            : ModelCapability.Unsupported;

        return new ElevenLabsSpeechModelEntry(
            new ElevenLabsSpeechCapabilities
            {
                Instructions = ModelCapability.Unsupported,
                Speech = true,
                Streaming = streaming,
            },
            providerOptions,
            new ElevenLabsSpeechRequestConfig(
                music: music,
                soundEffects: soundEffects,
                dialogue: dialogue,
                defaultDialogue: defaultDialogue,
                singleVoiceDialogue: singleVoiceDialogue,
                incrementalDialogue: modelId.StartsWith("eleven_v4", StringComparison.Ordinal)
            )
        );
    }
}
