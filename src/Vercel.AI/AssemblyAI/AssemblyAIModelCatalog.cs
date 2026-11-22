// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AssemblyAI;

/// <summary>Known options accepted under <c>providerOptions.assemblyai</c>. Maps to <c>assemblyaiProviderOptions</c>.</summary>
internal static class AssemblyAIProviderOptions
{
    /// <summary>Maps to <c>assemblyaiProviderOptions.speech</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Speech =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["bitRate"] = new("MP3 bit rate in bits per second. MP3 output only."),
            ["container"] = new(
                "Output container: `'raw'` for the encoded samples alone or `'wav'` for a WAV file. Requires a `pcm_*` encoding."
            ),
            ["encoding"] = new(
                "Audio encoding of the output (`'pcm_s16le'`, `'pcm_mulaw'`, `'pcm_alaw'`, or `'mp3'`). Overrides the encoding derived from `outputFormat`."
            ),
            ["sampleRate"] = new(
                "Sample rate of the output audio in Hz (8000, 16000, 22050, 24000, 44100, 48000)."
            ),
        };

    /// <summary>Maps to <c>assemblyaiProviderOptions.transcription</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Transcription =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["audioEndAt"] = new("End time of the audio in milliseconds."),
            ["audioStartFrom"] = new("Start time of the audio in milliseconds."),
            ["autoChapters"] = new(
                "Whether to automatically generate chapters for the transcription."
            ),
            ["autoHighlights"] = new(
                "Whether to automatically generate highlights for the transcription."
            ),
            ["boostParam"] = new(
                "Boost parameter for word boost (used with `wordBoost`). Allowed values: 'low', 'default', 'high'."
            ),
            ["contentSafety"] = new("Whether to enable content safety filtering."),
            ["contentSafetyConfidence"] = new(
                "Confidence threshold for content safety filtering (25-100)."
            ),
            ["customSpelling"] = new("Custom spelling rules for the transcription."),
            ["disfluencies"] = new(
                "Whether to include filler words (um, uh, etc.) in the transcription."
            ),
            ["domain"] = new(
                "Enable a domain-specific model to improve accuracy for specialized terminology. Currently supports `'medical-v1'` (Medical Mode)."
            ),
            ["entityDetection"] = new("Whether to enable entity detection."),
            ["filterProfanity"] = new("Whether to filter profanity from the transcription."),
            ["formatText"] = new("Whether to format text with punctuation and capitalization."),
            ["iabCategories"] = new("Whether to enable IAB categories detection."),
            ["keytermsPrompt"] = new(
                "Domain-specific keyterms to boost recognition for (max 6 words per phrase). Replaces `wordBoost` for newer models: supported by `assemblyai-stt-3-7` and `universal-3-5-pro` (and `universal-2` when metaphone is enabled for the account)."
            ),
            ["languageCode"] = new("Language code for the transcription."),
            ["languageConfidenceThreshold"] = new("Confidence threshold for language detection."),
            ["languageDetection"] = new("Whether to enable language detection."),
            ["languageDetectionOptions"] = new("Options for automatic language detection."),
            ["multichannel"] = new("Whether to process audio as multichannel."),
            ["prompt"] = new(
                "Provide natural-language context (up to 1,500 words) to steer the model. Only supported by `assemblyai-stt-3-7` and `universal-3-5-pro`."
            ),
            ["punctuate"] = new("Whether to add punctuation to the transcription."),
            ["redactPii"] = new("Whether to redact personally identifiable information (PII)."),
            ["redactPiiAudio"] = new("Whether to redact PII in the audio file."),
            ["redactPiiAudioOptions"] = new(
                "Options for PII-redacted audio files. Requires `redactPiiAudio`."
            ),
            ["redactPiiAudioQuality"] = new("Audio format for PII redaction."),
            ["redactPiiPolicies"] = new("List of PII types to redact."),
            ["redactPiiReturnUnredacted"] = new(
                "Return the original unredacted transcript alongside the redacted one. Requires `redactPii`."
            ),
            ["redactPiiSub"] = new("Substitution method for redacted PII."),
            ["redactStaticEntities"] = new(
                "Map of user-defined labels to exact terms to redact, e.g. `{ INTERNAL_TOOL: ['Bearclaw'] }`. Applied on top of standard PII redaction using `redactPiiSub`. Requires `redactPii`."
            ),
            ["removeAudioTags"] = new(
                "Remove inline annotations from rich transcripts. `'all'` removes all inline annotations; `'speaker'` removes only speaker cues. AssemblyAI-STT-3.7 and Universal-3 Pro models."
            ),
            ["sentimentAnalysis"] = new("Whether to enable sentiment analysis."),
            ["speakerLabels"] = new("Whether to identify different speakers in the audio."),
            ["speakerOptions"] = new(
                "Options for speaker diarization, e.g. a range of possible speakers."
            ),
            ["speakersExpected"] = new("Number of speakers expected in the audio."),
            ["speechThreshold"] = new("Threshold for speech detection (0-1)."),
            ["summarization"] = new("Whether to generate a summary of the transcription."),
            ["summaryModel"] = new("Model to use for summarization."),
            ["summaryType"] = new("Type of summary to generate."),
            ["temperature"] = new(
                "Sampling temperature (0-1) controlling randomness. AssemblyAI-STT-3.7 and Universal-3 Pro models."
            ),
            ["webhookAuthHeaderName"] = new(
                "Name of the authentication header for webhook requests."
            ),
            ["webhookAuthHeaderValue"] = new(
                "Value of the authentication header for webhook requests."
            ),
            ["webhookUrl"] = new("URL to send webhook notifications to."),
            ["wordBoost"] = new("List of words to boost recognition for."),
        };
}

/// <summary>Maps to <c>requestConfig.transcription</c> of <c>getAssemblyAITranscriptionModelEntry</c>.</summary>
internal sealed class AssemblyAITranscriptionRequestConfig
{
    /// <summary>True when AssemblyAI has deprecated the model. Maps to <c>deprecated</c>.</summary>
    public bool Deprecated { get; init; }

    /// <summary>Warning that points to a newer model, or null. Maps to <c>replacementWarning</c>.</summary>
    public string? ReplacementWarning { get; init; }
}

/// <summary>Maps to the request configuration of an AssemblyAI catalog entry.</summary>
internal sealed class AssemblyAIRequestConfig
{
    /// <summary>Maps to <c>transcription</c>.</summary>
    public AssemblyAITranscriptionRequestConfig? Transcription { get; init; }
}

/// <summary>AssemblyAI model catalog. Maps to <c>assemblyai-model-catalog.ts</c>.</summary>
internal static class AssemblyAIModelCatalog
{
    /// <summary>Known transcription model ids. Maps to <c>assemblyaiTranscriptionModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> AssemblyaiTranscriptionModelIds =
    [
        "assemblyai-stt-3-7",
        "universal-2",
        "universal-3-5-pro",
    ];

    /// <summary>Known speech model ids. Maps to <c>assemblyaiSpeechModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> AssemblyaiSpeechModelIds = ["assemblyai-tts-1"];

    // Maps to `modelPolicies`.
    internal static readonly IReadOnlyDictionary<string, TranscriptionModelPolicy> ModelPolicies =
        new Dictionary<string, TranscriptionModelPolicy>(StringComparer.Ordinal)
        {
            ["assemblyai-stt-3-7"] = new(Prompted: true),
            ["universal-2"] = new(
                ReplacementWarning: "'assemblyai-stt-3-7' is AssemblyAI's latest flagship model; 'universal-2' remains available for languages it does not support."
            ),
            ["universal-3-5-pro"] = new(Prompted: true, Deprecated: true),
        };

    /// <summary>Maps to <c>assemblyaiModelCatalog</c>, keyed by model id.</summary>
    internal static readonly IReadOnlyDictionary<
        string,
        ModelEntry<AssemblyAIRequestConfig>
    > AssemblyaiModelCatalog = CreateCatalog();

    /// <summary>Returns the entry of a transcription model. Any other model id resolves to an entry with unknown support. Maps to <c>getAssemblyAITranscriptionModelEntry</c>.</summary>
    internal static ModelEntry<AssemblyAIRequestConfig> GetAssemblyAITranscriptionModelEntry(
        string modelId
    )
    {
        var known = ModelPolicies.TryGetValue(modelId, out var policy);
        var prompted = policy?.Prompted ?? false;
        var support = known ? ModelCapability.Supported : ModelCapability.Unknown;
        var options = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var key in AssemblyAIProviderOptions.Transcription.Keys)
        {
            options[key] = support;
        }

        options["prompt"] = Declared(known, prompted);
        options["temperature"] = Declared(known, prompted);
        options["removeAudioTags"] = Declared(known, prompted);
        options["wordBoost"] = Declared(known, !prompted);
        options["boostParam"] = Declared(known, !prompted);
        options["keytermsPrompt"] = prompted
            ? ModelCapability.Supported
            : ModelCapability.When(
                "Supported by Universal-2 when enabled for the account; use AssemblyAI-STT-3.7 otherwise."
            );
        options["domain"] = ModelCapability.When(
            "Requires a model and account with Medical Mode enabled."
        );
        options["redactPiiReturnUnredacted"] = ModelCapability.When("Requires redactPii.");
        options["redactStaticEntities"] = ModelCapability.When("Requires redactPii.");
        options["redactPiiAudioOptions"] = ModelCapability.When("Requires redactPiiAudio.");

        return new ModelEntry<AssemblyAIRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Transcription = new TranscriptionCapabilities
                {
                    Transcription = ModelCapability.Supported,
                    Streaming = ModelCapability.Unsupported,
                    Duration = ModelCapability.Supported,
                    Segments = ModelCapability.Supported,
                    Language = ModelCapability.Supported,
                },
            },
            ProviderOptions = new ModelProviderOptions { Transcription = options },
            RequestConfig = new AssemblyAIRequestConfig
            {
                Transcription = new AssemblyAITranscriptionRequestConfig
                {
                    Deprecated = policy?.Deprecated ?? false,
                    ReplacementWarning = policy?.ReplacementWarning,
                },
            },
        };
    }

    /// <summary>Returns the entry of a speech model. Any other model id resolves to an entry with unknown support. Maps to <c>getAssemblyAISpeechModelEntry</c>.</summary>
    internal static ModelEntry<AssemblyAIRequestConfig> GetAssemblyAISpeechModelEntry(
        string modelId
    )
    {
        var known = AssemblyaiSpeechModelIds.Contains(modelId);
        var support = known ? ModelCapability.Supported : ModelCapability.Unknown;
        var options = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var key in AssemblyAIProviderOptions.Speech.Keys)
        {
            options[key] = support;
        }

        options["bitRate"] = ModelCapability.When("MP3 output only.");
        options["container"] = ModelCapability.When("Requires a pcm_* encoding.");

        return new ModelEntry<AssemblyAIRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Speech = new SpeechCapabilities { Instructions = ModelCapability.Unsupported },
            },
            ProviderOptions = new ModelProviderOptions { Speech = options },
        };
    }

    private static ModelCapability Declared(bool known, bool supported)
    {
        return known ? ModelCapability.From(supported) : ModelCapability.Unknown;
    }

    private static Dictionary<string, ModelEntry<AssemblyAIRequestConfig>> CreateCatalog()
    {
        var catalog = new Dictionary<string, ModelEntry<AssemblyAIRequestConfig>>(
            StringComparer.Ordinal
        );
        foreach (var id in AssemblyaiSpeechModelIds)
        {
            catalog[id] = GetAssemblyAISpeechModelEntry(id);
        }

        foreach (var id in AssemblyaiTranscriptionModelIds)
        {
            catalog[id] = GetAssemblyAITranscriptionModelEntry(id);
        }

        return catalog;
    }

    internal sealed record TranscriptionModelPolicy(
        bool Deprecated = false,
        string? ReplacementWarning = null,
        bool Prompted = false
    );
}
