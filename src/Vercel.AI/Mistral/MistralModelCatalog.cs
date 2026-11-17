// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Known options accepted under <c>providerOptions.mistral</c>. Maps to <c>mistralProviderOptions</c>.</summary>
public static class MistralProviderOptions
{
    /// <summary>Language options (<c>mistralProviderOptions.language</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["safePrompt"] = new("Inject a safety prompt. Chat API only."),
            ["documentImageLimit"] = new("Maximum document images. Chat API only."),
            ["documentPageLimit"] = new("Maximum document pages. Chat API only."),
            ["structuredOutputs"] = new("Whether to send a JSON response schema."),
            ["strictJsonSchema"] = new("Whether to use strict JSON schema validation."),
            ["parallelToolCalls"] = new("Allow parallel function calls. Chat API only."),
            ["promptCacheKey"] = new("Cache affinity key. Chat API only."),
            ["reasoningEffort"] = new("Reasoning effort for configurable reasoning models."),
        };

    /// <summary>Embedding options (<c>mistralProviderOptions.embedding</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Embedding =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["metadata"] = new("Metadata attached to the embedding request."),
            ["outputDimension"] = new("Output embedding dimensions."),
            ["outputDtype"] = new("Output embedding data type."),
        };

    /// <summary>Transcription options (<c>mistralProviderOptions.transcription</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Transcription =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["language"] = new("Language of the audio."),
            ["returnLanguage"] = new("Return the detected language."),
            ["temperature"] = new("Sampling temperature."),
            ["timestampGranularities"] = new("Timestamp granularities to return."),
            ["diarize"] = new("Identify speakers in the transcription."),
            ["contextBias"] = new("Vocabulary and context to guide transcription."),
            ["targetStreamingDelayMs"] = new(
                "Target transcription delay in milliseconds. Realtime only."
            ),
            ["speakerReferences"] = new("Named audio clips for known speakers. Realtime only."),
            ["turnDetection"] = new("Turn detection settings. Realtime only."),
        };

    /// <summary>Speech options (<c>mistralProviderOptions.speech</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Speech =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["refAudio"] = new("Reference audio for voice cloning, overriding voice."),
            ["expressiveness"] = new("Speech expressiveness, from 1 to 5."),
            ["phonemeHints"] = new("Pronunciation hints as IPA phonemes."),
        };
}

/// <summary>Reasoning support of a model. Maps to the catalog's <c>ReasoningCapabilities</c>.</summary>
public sealed class MistralReasoningCapabilities
{
    /// <summary>Creates reasoning capabilities. <see cref="Support"/> is true when any effort is accepted.</summary>
    public MistralReasoningCapabilities(IReadOnlyList<string> efforts)
    {
        Efforts = efforts;
        Support = efforts.Count > 0;
    }

    /// <summary>Whether the model accepts reasoning configuration.</summary>
    public bool Support { get; }

    /// <summary>The accepted reasoning efforts.</summary>
    public IReadOnlyList<string> Efforts { get; }
}

/// <summary>Conversations API capabilities. Maps to <c>capabilities.language.endpoints.conversation</c>.</summary>
public sealed class MistralConversationEndpointCapabilities
{
    /// <summary>Reasoning support of the Conversations API.</summary>
    public MistralReasoningCapabilities Reasoning { get; init; } = new([]);
}

/// <summary>Per-endpoint capabilities. Maps to <c>capabilities.language.endpoints</c>.</summary>
public sealed class MistralLanguageEndpointCapabilities
{
    /// <summary>The Conversations API.</summary>
    public MistralConversationEndpointCapabilities Conversation { get; init; } = new();
}

/// <summary>Language capabilities of a Mistral model. Maps to the catalog's <c>LanguageCapabilities</c>.</summary>
public sealed class MistralLanguageCapabilities
{
    /// <summary>Upstream <c>inputs</c>.</summary>
    public LanguageInputCapabilities Inputs { get; init; } = new();

    /// <summary>Upstream <c>objectGeneration</c>.</summary>
    public ModelCapability ObjectGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>tools</c>.</summary>
    public LanguageToolCapabilities Tools { get; init; } = new();

    /// <summary>Upstream <c>reasoning</c>, the Chat API.</summary>
    public MistralReasoningCapabilities Reasoning { get; init; } = new([]);

    /// <summary>Upstream <c>sampling</c>.</summary>
    public LanguageSamplingCapabilities Sampling { get; init; } = new();

    /// <summary>Upstream <c>endpoints</c>.</summary>
    public MistralLanguageEndpointCapabilities Endpoints { get; init; } = new();
}

/// <summary>Transcription capabilities of a Mistral model. Maps to the catalog's <c>TranscriptionCapabilities</c>.</summary>
public sealed class MistralTranscriptionCapabilities
{
    /// <summary>Upstream <c>transcription</c>.</summary>
    public ModelCapability Transcription { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>streaming</c>.</summary>
    public ModelCapability Streaming { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>duration</c>.</summary>
    public ModelCapability Duration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>segments</c>.</summary>
    public ModelCapability Segments { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>language</c>.</summary>
    public ModelCapability Language { get; init; } = ModelCapability.Unknown;

    /// <summary>Whether a pinned language can be combined with timestamps.</summary>
    public bool LanguageWithTimestamps { get; init; }

    /// <summary>Upstream <c>timestamps</c>.</summary>
    public ModelCapability Timestamps { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>diarization</c>.</summary>
    public ModelCapability Diarization { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>contextBias</c>.</summary>
    public ModelCapability ContextBias { get; init; } = ModelCapability.Unknown;
}

/// <summary>Speech capabilities of a Mistral model. Maps to the catalog's <c>SpeechCapabilities</c>.</summary>
public sealed class MistralSpeechCapabilities
{
    /// <summary>Upstream <c>instructions</c>.</summary>
    public ModelCapability Instructions { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>savedVoices</c>.</summary>
    public ModelCapability SavedVoices { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>referenceAudio</c>.</summary>
    public ModelCapability ReferenceAudio { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>nonStreaming</c>.</summary>
    public bool NonStreaming { get; init; }

    /// <summary>Upstream <c>outputFormats</c>.</summary>
    public IReadOnlyList<string> OutputFormats { get; init; } = [];
}

/// <summary>Language model entry of the Mistral catalog. Maps to <c>MistralLanguageModelEntry</c>.</summary>
public sealed class MistralLanguageModelEntry
{
    /// <summary>Upstream <c>capabilities.language</c>.</summary>
    public MistralLanguageCapabilities Language { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.language</c>: support of each option in <see cref="MistralProviderOptions.Language"/>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>Transcription model entry of the Mistral catalog. Maps to <c>getMistralTranscriptionModelEntry</c>'s result.</summary>
public sealed class MistralTranscriptionModelEntry
{
    /// <summary>Upstream <c>capabilities.transcription</c>.</summary>
    public MistralTranscriptionCapabilities Transcription { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.transcription</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>Speech model entry of the Mistral catalog. Maps to <c>getMistralSpeechModelEntry</c>'s result.</summary>
public sealed class MistralSpeechModelEntry
{
    /// <summary>Upstream <c>capabilities.speech</c>.</summary>
    public MistralSpeechCapabilities Speech { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.speech</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>Embedding model entry of the Mistral catalog.</summary>
public sealed class MistralEmbeddingModelEntry
{
    /// <summary>Upstream <c>capabilities.embedding</c>.</summary>
    public EmbeddingCapabilities Embedding { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.embedding</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>
/// An entry of <see cref="MistralModelCatalog.Entries"/>. Maps to <c>MistralModelEntry</c>. Exactly the sections the
/// model declares are set.
/// </summary>
public sealed class MistralModelEntry
{
    /// <summary>Language section, or null.</summary>
    public MistralLanguageModelEntry? Language { get; init; }

    /// <summary>Transcription section, or null.</summary>
    public MistralTranscriptionModelEntry? Transcription { get; init; }

    /// <summary>Speech section, or null.</summary>
    public MistralSpeechModelEntry? Speech { get; init; }

    /// <summary>Embedding section, or null.</summary>
    public MistralEmbeddingModelEntry? Embedding { get; init; }
}

/// <summary>Mistral model catalog. Maps to <c>mistral-model-catalog.ts</c>.</summary>
public static class MistralModelCatalog
{
    // Preserve the provider page's documented model set and image-input exceptions. Maps to languageModelDefinitions.
    // A null image marks a known ID without declared capabilities, which retains the custom-model fallback.
    private static readonly (string ModelId, bool? Image)[] LanguageModelDefinitions =
    [
        ("labs-leanstral-1-5", true),
        ("magistral-medium-latest", true),
        ("magistral-small-latest", true),
        ("ministral-14b-latest", true),
        ("ministral-8b-latest", true),
        ("mistral-large-latest", true),
        ("mistral-medium-latest", true),
        ("mistral-small-latest", true),
        ("voxtral-small-latest", false),
        ("zai-glm-5-2", false),
        ("codestral-2508", null),
        ("codestral-latest", null),
        ("glm-5-2", null),
        ("labs-leanstral-1-5-1", null),
        ("ministral-14b-2512", null),
        ("ministral-3b-2512", null),
        ("ministral-3b-latest", null),
        ("ministral-8b-2512", null),
        ("mistral-code-fim-latest", null),
        ("mistral-code-latest", null),
        ("mistral-large-2512", null),
        ("mistral-large-4", null),
        ("mistral-large-4-0", null),
        ("mistral-medium", null),
        ("mistral-medium-2604", null),
        ("mistral-medium-3", null),
        ("mistral-medium-3-5", null),
        ("mistral-medium-3.5", null),
        ("mistral-small-2603", null),
        ("mistral-vibe-cli-fast", null),
        ("mistral-vibe-cli-latest", null),
        ("mistral-vibe-cli-with-tools", null),
        ("voxtral-small-2507", null),
        ("zai-glm-5", null),
        ("zai-glm-5-3", null),
        ("zai-glm-5-3-flash", null),
        ("zai-glm-latest", null),
    ];

    private static readonly string[] TranscriptionModelIds =
    [
        "voxtral-mini-transcribe-3-0",
        "voxtral-small-transcribe-3-0",
        "voxtral-mini-latest",
        "voxtral-mini-2602",
        "voxtral-mini-transcribe-3",
        "voxtral-mini-transcribe-latest",
        "voxtral-mini-transcribe-realtime-3",
        "voxtral-mini-transcribe-realtime-3-0",
        "voxtral-small-transcribe-3",
        "voxtral-small-transcribe-latest",
    ];

    // Only the first three declare capabilities.
    private const int DeclaredTranscriptionModels = 3;

    private static readonly string[] SpeechModelIds =
    [
        "voxtral-mini-tts-3-0",
        "voxtral-mini-tts-2603",
        "voxtral-mini-tts-3",
        "voxtral-mini-tts-latest",
    ];

    // Only the first two declare capabilities.
    private const int DeclaredSpeechModels = 2;

    private static readonly string[] EmbeddingModelIds =
    [
        "mistral-embed",
        "codestral-embed",
        "codestral-embed-2505",
        "mistral-embed-2312",
    ];

    /// <summary>Known language model ids. Maps to <c>MistralModelIds['language']</c>.</summary>
    public static IReadOnlyList<string> LanguageModelIds { get; } =
    [.. LanguageModelDefinitions.Select(d => d.ModelId)];

    /// <summary>Known embedding model ids. Maps to <c>MistralModelIds['embedding']</c>.</summary>
    public static IReadOnlyList<string> EmbeddingModelIdList => EmbeddingModelIds;

    /// <summary>Known speech model ids. Maps to <c>MistralModelIds['speech']</c>.</summary>
    public static IReadOnlyList<string> SpeechModelIdList => SpeechModelIds;

    /// <summary>Known transcription model ids. Maps to <c>MistralModelIds['transcription']</c>.</summary>
    public static IReadOnlyList<string> TranscriptionModelIdList => TranscriptionModelIds;

    // Known non-reasoning models from https://api.mistral.ai/v1/models (2026-10-06).
    // Unknown IDs inherit current reasoning configuration support.
    private static readonly HashSet<string> ModelsWithoutReasoningEffort = new(
        StringComparer.Ordinal
    )
    {
        "codestral-2508",
        "codestral-latest",
        "ministral-14b-2512",
        "ministral-14b-latest",
        "ministral-3b-2512",
        "ministral-3b-latest",
        "ministral-8b-2512",
        "ministral-8b-latest",
        "mistral-code-fim-latest",
        "mistral-code-latest",
        "mistral-large-latest",
        "mistral-large-2512",
        "voxtral-small-2507",
        "voxtral-small-latest",
    };

    // GLM-5.3 models accept only low, high, and max reasoning effort and cannot disable reasoning.
    // Omitting reasoning_effort uses max. https://docs.mistral.ai/studio/conversations/reasoning (2026-10-26)
    private static readonly HashSet<string> ModelsWithLowHighMaxReasoningEffort = new(
        StringComparer.Ordinal
    )
    {
        "zai-glm-5",
        "zai-glm-5-3",
        "zai-glm-5-3-flash",
        "zai-glm-latest",
    };

    // Mistral Large 4 Chat models accept low, medium, and high reasoning effort and cannot disable reasoning (see #24350).
    private static readonly HashSet<string> ModelsWithLowMediumHighReasoningEffort = new(
        StringComparer.Ordinal
    )
    {
        "mistral-large-4",
        "mistral-large-4-0",
    };

    // https://api.mistral.ai/v1/models (2026-10-06)
    private static readonly HashSet<string> ReasoningEffortModelIds = new(StringComparer.Ordinal)
    {
        "glm-5-2",
        "labs-leanstral-1-5",
        "labs-leanstral-1-5-1",
        "magistral-medium-latest",
        "magistral-small-latest",
        "mistral-large-4",
        "mistral-large-4-0",
        "mistral-medium",
        "mistral-medium-2604",
        "mistral-medium-3",
        "mistral-medium-3-5",
        "mistral-medium-3.5",
        "mistral-medium-latest",
        "mistral-small-2603",
        "mistral-small-latest",
        "mistral-vibe-cli-fast",
        "mistral-vibe-cli-latest",
        "mistral-vibe-cli-with-tools",
        "zai-glm-5-2",
    };

    /// <summary>Entries of every model id that declares capabilities. Maps to <c>mistralModelCatalog</c>; renamed because C# does not allow a member named like its class.</summary>
    public static IReadOnlyDictionary<string, MistralModelEntry> Entries { get; } = BuildEntries();

    /// <summary>Returns the entry of a language model. Any other model id resolves to an entry with unknown support. Maps to <c>getMistralLanguageModelEntry</c>.</summary>
    public static MistralLanguageModelEntry GetMistralLanguageModelEntry(string modelId) =>
        Entries.TryGetValue(modelId, out var entry) && entry.Language is { } language
            ? language
            : CreateLanguageModelEntry(modelId, null);

    /// <summary>Returns the entry of a transcription model. Maps to <c>getMistralTranscriptionModelEntry</c>.</summary>
    public static MistralTranscriptionModelEntry GetMistralTranscriptionModelEntry(
        string modelId
    ) =>
        Entries.TryGetValue(modelId, out var entry) && entry.Transcription is { } transcription
            ? transcription
            : CreateTranscriptionModelEntry(modelId, false);

    /// <summary>Returns the entry of a speech model. Maps to <c>getMistralSpeechModelEntry</c>.</summary>
    public static MistralSpeechModelEntry GetMistralSpeechModelEntry(string modelId) =>
        Entries.TryGetValue(modelId, out var entry) && entry.Speech is { } speech
            ? speech
            : CreateSpeechModelEntry(false);

    private static Dictionary<string, MistralModelEntry> BuildEntries()
    {
        var entries = new Dictionary<string, MistralModelEntry>(StringComparer.Ordinal);
        foreach (var (modelId, image) in LanguageModelDefinitions)
        {
            if (image is not null)
            {
                entries[modelId] = new MistralModelEntry
                {
                    Language = CreateLanguageModelEntry(modelId, image),
                };
            }
        }

        for (var i = 0; i < DeclaredTranscriptionModels; i++)
        {
            entries[TranscriptionModelIds[i]] = new MistralModelEntry
            {
                Transcription = CreateTranscriptionModelEntry(TranscriptionModelIds[i], true),
            };
        }

        for (var i = 0; i < DeclaredSpeechModels; i++)
        {
            entries[SpeechModelIds[i]] = new MistralModelEntry
            {
                Speech = CreateSpeechModelEntry(true),
            };
        }

        entries["mistral-embed"] = new MistralModelEntry
        {
            Embedding = CreateEmbeddingModelEntry(1024, false),
        };

        // https://docs.mistral.ai/studio/knowledge-rag/embeddings/code_embeddings
        entries["codestral-embed"] = new MistralModelEntry
        {
            Embedding = CreateEmbeddingModelEntry(1536, true),
        };

        return entries;
    }

    private static MistralReasoningCapabilities ReasoningCapabilities(
        IReadOnlyList<string> efforts
    ) => new(efforts);

    private static MistralLanguageModelEntry CreateLanguageModelEntry(string modelId, bool? image)
    {
        var reasoning = ReasoningCapabilities(
            ModelsWithoutReasoningEffort.Contains(modelId) ? []
            : ModelsWithLowHighMaxReasoningEffort.Contains(modelId) ? ["low", "high", "max"]
            : ModelsWithLowMediumHighReasoningEffort.Contains(modelId) ? ["low", "medium", "high"]
            : ["none", "high"]
        );
        var conversationReasoning = ReasoningCapabilities(
            ReasoningEffortModelIds.Contains(modelId) ? ["none", "high"] : []
        );
        var support = image is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        var chatOnly = ModelCapability.When("Only supported on the Chat API.");
        return new MistralLanguageModelEntry
        {
            Language = new MistralLanguageCapabilities
            {
                Inputs = new LanguageInputCapabilities
                {
                    Image = image is { } hasImage
                        ? ModelCapability.From(hasImage)
                        : ModelCapability.Unknown,
                    Audio = ModelCapability.Unknown,
                },
                ObjectGeneration = support,
                Tools = new LanguageToolCapabilities { Usage = support, Streaming = support },
                Reasoning = reasoning,
                Sampling = new LanguageSamplingCapabilities
                {
                    Temperature = ModelCapability.Unknown,
                    TopP = ModelCapability.Unknown,
                    Logprobs = ModelCapability.Unsupported,
                },
                Endpoints = new MistralLanguageEndpointCapabilities
                {
                    Conversation = new MistralConversationEndpointCapabilities
                    {
                        Reasoning = conversationReasoning,
                    },
                },
            },
            ProviderOptions = DeclareProviderOptions(
                MistralProviderOptions.Language,
                support,
                new Dictionary<string, ModelCapability>
                {
                    ["safePrompt"] = chatOnly,
                    ["documentImageLimit"] = chatOnly,
                    ["documentPageLimit"] = chatOnly,
                    ["parallelToolCalls"] = chatOnly,
                    ["promptCacheKey"] = chatOnly,
                    ["reasoningEffort"] =
                        reasoning.Support && !conversationReasoning.Support
                            ? chatOnly
                            : ModelCapability.From(reasoning.Support),
                }
            ),
        };
    }

    private static MistralTranscriptionModelEntry CreateTranscriptionModelEntry(
        string modelId,
        bool declared
    )
    {
        // Only these legacy IDs reject pinned language combined with timestamps.
        // Future and custom IDs keep the existing permissive request behavior.
        var languageWithTimestamps =
            modelId is not ("voxtral-mini-2507" or "voxtral-mini-2602" or "voxtral-mini-latest");
        var support = declared ? ModelCapability.Supported : ModelCapability.Unknown;
        var pinnedLanguage = ModelCapability.When("Only supported when language is not pinned.");
        return new MistralTranscriptionModelEntry
        {
            Transcription = new MistralTranscriptionCapabilities
            {
                Transcription = support,
                Streaming = ModelCapability.Unsupported,
                Duration = support,
                Segments = support,
                Language = support,
                LanguageWithTimestamps = languageWithTimestamps,
                Timestamps = support,
                Diarization = support,
                ContextBias = support,
            },
            ProviderOptions = DeclareProviderOptions(
                MistralProviderOptions.Transcription,
                support,
                new Dictionary<string, ModelCapability>
                {
                    ["targetStreamingDelayMs"] = ModelCapability.Unsupported,
                    ["speakerReferences"] = ModelCapability.Unsupported,
                    ["turnDetection"] = ModelCapability.Unsupported,
                    ["returnLanguage"] = pinnedLanguage,
                    ["timestampGranularities"] = languageWithTimestamps ? support : pinnedLanguage,
                }
            ),
        };
    }

    private static MistralSpeechModelEntry CreateSpeechModelEntry(bool declared)
    {
        var support = declared ? ModelCapability.Supported : ModelCapability.Unknown;
        return new MistralSpeechModelEntry
        {
            Speech = new MistralSpeechCapabilities
            {
                Instructions = ModelCapability.Unsupported,
                SavedVoices = support,
                ReferenceAudio = support,
                NonStreaming = true,
                OutputFormats = ["pcm", "wav", "mp3", "flac", "opus"],
            },
            ProviderOptions = DeclareProviderOptions(
                MistralProviderOptions.Speech,
                support,
                new Dictionary<string, ModelCapability>()
            ),
        };
    }

    private static MistralEmbeddingModelEntry CreateEmbeddingModelEntry(
        int defaultDimensions,
        bool customDimensions
    )
    {
        var custom = ModelCapability.From(customDimensions);
        return new MistralEmbeddingModelEntry
        {
            Embedding = new EmbeddingCapabilities
            {
                DefaultDimensions = defaultDimensions,
                CustomDimensions = custom,
            },
            ProviderOptions = DeclareProviderOptions(
                MistralProviderOptions.Embedding,
                ModelCapability.Supported,
                new Dictionary<string, ModelCapability>
                {
                    ["outputDimension"] = custom,
                    ["outputDtype"] = custom,
                }
            ),
        };
    }

    private static Dictionary<string, ModelCapability> DeclareProviderOptions(
        IReadOnlyDictionary<string, ProviderOptionDefinition> definitions,
        ModelCapability support,
        IReadOnlyDictionary<string, ModelCapability> exceptions
    )
    {
        var result = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var option in definitions.Keys)
        {
            result[option] = exceptions.TryGetValue(option, out var exception)
                ? exception
                : support;
        }

        return result;
    }
}
