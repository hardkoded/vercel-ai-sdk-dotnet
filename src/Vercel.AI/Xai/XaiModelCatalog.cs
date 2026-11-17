// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Xai;

/// <summary>Known options accepted under <c>providerOptions.xai</c>. Maps to <c>xaiProviderOptions</c>.</summary>
internal static class XaiProviderOptions
{
    /// <summary>Maps to <c>xaiProviderOptions.language</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["reasoningEffort"] = new("Reasoning effort for configurable models."),
            ["reasoningSummary"] = new("Deprecated summary level; xAI ignores this setting."),
            ["logprobs"] = new("Return token log probabilities."),
            ["topLogprobs"] = new("Number of token log probabilities to return."),
            ["minP"] = new("Minimum probability sampling threshold."),
            ["maxTurns"] = new("Maximum number of agentic tool-calling turns."),
            ["parallelToolCalls"] = new("Allow parallel tool calls."),
            ["promptCacheKey"] = new("Cache affinity key for shared prompt prefixes."),
            ["safetyIdentifier"] = new("End-user identifier for policy violations."),
            ["serviceTier"] = new("Default or priority processing tier."),
            ["store"] = new("Store the input and response for later retrieval."),
            ["previousResponseId"] = new("Previous response ID to continue."),
            ["user"] = new("End-user identifier for abuse monitoring."),
            ["include"] = new("Additional output data to return."),
        };

    /// <summary>Maps to <c>xaiProviderOptions.image</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Image =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["aspect_ratio"] = new("Output aspect ratio, unless aspectRatio is provided."),
            ["output_format"] = new("Output image encoding."),
            ["sync_mode"] = new("Wait for image generation to complete."),
            ["resolution"] = new("Output image resolution."),
            ["quality"] = new("Output image quality."),
            ["user"] = new("End-user identifier."),
        };

    /// <summary>Maps to <c>xaiProviderOptions.speech</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Speech =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["sampleRate"] = new("Output audio sample rate."),
            ["bitRate"] = new("MP3 output bit rate."),
            ["optimizeStreamingLatency"] = new("Trade quality for lower latency."),
            ["textNormalization"] = new("Normalize written text before synthesis."),
            ["withTimestamps"] = new("Return character timings and audio duration."),
            ["replace"] = new("Spoken substitutions and pronunciation hints."),
        };

    /// <summary>Maps to <c>xaiProviderOptions.transcription</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Transcription =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["audioFormat"] = new("Encoding of raw input audio."),
            ["sampleRate"] = new("Input audio sample rate."),
            ["language"] = new("Language for inverse text normalization."),
            ["format"] = new("Enable inverse text normalization."),
            ["multichannel"] = new("Transcribe each audio channel separately."),
            ["channels"] = new("Number of interleaved input audio channels."),
            ["diarize"] = new("Enable speaker diarization."),
            ["keyterm"] = new("Terms to bias transcription toward."),
            ["fillerWords"] = new("Include filler words in the transcript."),
            ["vadThreshold"] = new("Voice activity detection threshold."),
            ["streaming"] = new("WebSocket streaming transcription settings."),
        };
}

/// <summary>Maps to the <c>portable</c> member of <c>LanguageCapabilities.reasoning</c>: the SDK portable-reasoning policy, independent of native support.</summary>
internal sealed class XaiPortableReasoningCapabilities
{
    /// <summary>Maps to <c>configurable</c>.</summary>
    public bool Configurable { get; init; }

    /// <summary>Maps to <c>none</c>.</summary>
    public bool None { get; init; }

    /// <summary>Maps to <c>xhigh</c>.</summary>
    public bool Xhigh { get; init; }
}

/// <summary>Maps to <c>LanguageCapabilities.reasoning</c> of <c>xai-model-catalog.ts</c>.</summary>
internal sealed class XaiLanguageReasoningCapabilities
{
    /// <summary>Maps to <c>support</c>.</summary>
    public ModelCapability Support { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>efforts</c>. Null means the catalog does not declare an exhaustive list.</summary>
    public IReadOnlyList<string>? Efforts { get; init; }

    /// <summary>Maps to <c>portable</c>.</summary>
    public XaiPortableReasoningCapabilities Portable { get; init; } = new();
}

/// <summary>Maps to <c>LanguageCapabilities</c> of <c>xai-model-catalog.ts</c>.</summary>
internal sealed class XaiLanguageCapabilities
{
    /// <summary>Maps to <c>inputs</c>.</summary>
    public LanguageInputCapabilities Inputs { get; init; } = new();

    /// <summary>Maps to <c>objectGeneration</c>.</summary>
    public ModelCapability ObjectGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>tools</c>.</summary>
    public LanguageToolCapabilities Tools { get; init; } = new();

    /// <summary>Maps to <c>reasoning</c>.</summary>
    public XaiLanguageReasoningCapabilities Reasoning { get; init; } = new();

    /// <summary>Maps to <c>sampling</c>.</summary>
    public LanguageSamplingCapabilities Sampling { get; init; } = new();
}

/// <summary>Maps to <c>ImageCapabilities</c> of <c>xai-model-catalog.ts</c>.</summary>
internal sealed class XaiImageCapabilities
{
    /// <summary>Maps to <c>sizes.description</c>.</summary>
    public string SizesDescription { get; init; } = "";

    /// <summary>Maps to <c>fileInput</c>.</summary>
    public ModelCapability FileInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>maskInput</c>.</summary>
    public ModelCapability MaskInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>maxImagesPerCall</c>.</summary>
    public int MaxImagesPerCall { get; init; }

    /// <summary>Maps to <c>resolutions</c>.</summary>
    public IReadOnlyList<string>? Resolutions { get; init; }

    /// <summary>Maps to <c>aspectRatios</c>.</summary>
    public IReadOnlyList<string>? AspectRatios { get; init; }
}

/// <summary>Maps to <c>SpeechCapabilities</c> of <c>xai-model-catalog.ts</c>.</summary>
internal sealed class XaiSpeechCapabilities
{
    /// <summary>Maps to <c>instructions</c>.</summary>
    public ModelCapability Instructions { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>language</c>.</summary>
    public bool Language { get; init; }

    /// <summary>Maps to <c>speed</c>.</summary>
    public bool Speed { get; init; }

    /// <summary>Maps to <c>outputFormats</c>.</summary>
    public IReadOnlyList<string> OutputFormats { get; init; } = [];
}

/// <summary>Maps to <c>TranscriptionCapabilities</c> of <c>xai-model-catalog.ts</c>.</summary>
internal sealed class XaiTranscriptionCapabilities
{
    internal XaiTranscriptionCapabilities(ModelCapability support)
    {
        Transcription = support;
        Streaming = support;
        Duration = support;
        Segments = support;
        Language = support;
        WordTimestamps = support;
        Diarization = support;
        Multichannel = support;
    }

    /// <summary>Request/response transcription support.</summary>
    public ModelCapability Transcription { get; }

    /// <summary>Streaming transcription support.</summary>
    public ModelCapability Streaming { get; }

    /// <summary>Whether the result reports the audio duration.</summary>
    public ModelCapability Duration { get; }

    /// <summary>Whether the result carries segments.</summary>
    public ModelCapability Segments { get; }

    /// <summary>Whether the result reports the language.</summary>
    public ModelCapability Language { get; }

    /// <summary>Whether the result carries word timestamps.</summary>
    public ModelCapability WordTimestamps { get; }

    /// <summary>Whether speaker diarization is supported.</summary>
    public ModelCapability Diarization { get; }

    /// <summary>Whether multichannel transcription is supported.</summary>
    public ModelCapability Multichannel { get; }
}

/// <summary>Maps to <c>XaiRequestConfig.speech</c>.</summary>
internal sealed class XaiSpeechRequestConfig
{
    /// <summary>Maps to <c>voice</c>.</summary>
    public string Voice { get; init; } = "";

    /// <summary>Maps to <c>language</c>.</summary>
    public string Language { get; init; } = "";

    /// <summary>Maps to <c>outputFormat</c>.</summary>
    public string OutputFormat { get; init; } = "";
}

/// <summary>Maps to <c>XaiRequestConfig.transcription</c>.</summary>
internal sealed class XaiTranscriptionRequestConfig
{
    /// <summary>The <c>model</c> request field, or <see langword="null"/> for the empty model id. Maps to <c>model</c>.</summary>
    public string? Model { get; init; }
}

/// <summary>Maps to <c>XaiRequestConfig</c>.</summary>
internal sealed class XaiRequestConfig
{
    /// <summary>Maps to <c>speech</c>.</summary>
    public XaiSpeechRequestConfig? Speech { get; init; }

    /// <summary>Maps to <c>transcription</c>.</summary>
    public XaiTranscriptionRequestConfig? Transcription { get; init; }
}

/// <summary>Maps to <c>XaiModelEntry</c>: one record that can declare several operations.</summary>
internal sealed class XaiModelEntry
{
    /// <summary>Maps to <c>capabilities.language</c>.</summary>
    public XaiLanguageCapabilities? Language { get; init; }

    /// <summary>Maps to <c>capabilities.image</c>.</summary>
    public XaiImageCapabilities? Image { get; init; }

    /// <summary>Maps to <c>capabilities.speech</c>.</summary>
    public XaiSpeechCapabilities? Speech { get; init; }

    /// <summary>Maps to <c>capabilities.transcription</c>.</summary>
    public XaiTranscriptionCapabilities? Transcription { get; init; }

    /// <summary>Maps to <c>capabilities.realtime</c>.</summary>
    public RealtimeCapabilities? Realtime { get; init; }

    /// <summary>Maps to <c>providerOptions</c>.</summary>
    public ModelProviderOptions? ProviderOptions { get; init; }

    /// <summary>Maps to <c>requestConfig</c>.</summary>
    public XaiRequestConfig? RequestConfig { get; init; }
}

/// <summary>A catalog entry of an xAI language model. Maps to the result of <c>getXaiLanguageModelEntry</c>.</summary>
internal sealed class XaiLanguageModelEntry
{
    internal XaiLanguageModelEntry(
        XaiLanguageCapabilities capabilities,
        IReadOnlyDictionary<string, ModelCapability> providerOptions
    )
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
    }

    /// <summary>Maps to <c>capabilities.language</c>.</summary>
    public XaiLanguageCapabilities Capabilities { get; }

    /// <summary>Maps to <c>providerOptions.language</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }
}

/// <summary>A catalog entry of an xAI image model. Maps to the result of <c>getXaiImageModelEntry</c>.</summary>
internal sealed class XaiImageModelEntry
{
    internal XaiImageModelEntry(
        XaiImageCapabilities capabilities,
        IReadOnlyDictionary<string, ModelCapability> providerOptions
    )
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
    }

    /// <summary>Maps to <c>capabilities.image</c>.</summary>
    public XaiImageCapabilities Capabilities { get; }

    /// <summary>Maps to <c>providerOptions.image</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }
}

/// <summary>A catalog entry of the xAI speech model. Maps to the result of <c>getXaiSpeechModelEntry</c>.</summary>
internal sealed class XaiSpeechModelEntry
{
    internal XaiSpeechModelEntry(
        XaiSpeechCapabilities capabilities,
        IReadOnlyDictionary<string, ModelCapability> providerOptions,
        XaiSpeechRequestConfig requestConfig
    )
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
        RequestConfig = requestConfig;
    }

    /// <summary>Maps to <c>capabilities.speech</c>.</summary>
    public XaiSpeechCapabilities Capabilities { get; }

    /// <summary>Maps to <c>providerOptions.speech</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }

    /// <summary>Maps to <c>requestConfig.speech</c>.</summary>
    public XaiSpeechRequestConfig RequestConfig { get; }
}

/// <summary>A catalog entry of an xAI transcription model. Maps to the result of <c>getXaiTranscriptionModelEntry</c>.</summary>
internal sealed class XaiTranscriptionModelEntry
{
    internal XaiTranscriptionModelEntry(
        XaiTranscriptionCapabilities capabilities,
        IReadOnlyDictionary<string, ModelCapability> providerOptions,
        XaiTranscriptionRequestConfig requestConfig
    )
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
        RequestConfig = requestConfig;
    }

    /// <summary>Maps to <c>capabilities.transcription</c>.</summary>
    public XaiTranscriptionCapabilities Capabilities { get; }

    /// <summary>Maps to <c>providerOptions.transcription</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }

    /// <summary>Maps to <c>requestConfig.transcription</c>.</summary>
    public XaiTranscriptionRequestConfig RequestConfig { get; }
}

/// <summary>Declaration of one language model. Maps to <c>LanguageDefinition</c>.</summary>
internal sealed record XaiLanguageDefinition(
    bool Image,
    bool Reasoning,
    IReadOnlyList<string>? Efforts = null
);

/// <summary>Declaration of one image model. Maps to the argument of <c>createImageModelEntry</c>.</summary>
internal sealed record XaiImageDefinition(
    IReadOnlyList<string> Resolutions,
    IReadOnlyList<string> AspectRatios,
    ModelCapability Quality
);

/// <summary>xAI model catalog. Maps to <c>xai-model-catalog.ts</c>.</summary>
internal static class XaiModelCatalog
{
    // These variants reject reasoning effort, including `none`. Multi-agent variants deliberately retain the
    // existing configurable-effort fallback.
    private static readonly Regex ModelsWithoutReasoningEffort = new(
        @"^grok-4\.20(-[0-9]{4})?-(non-)?reasoning\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    // 4.20 predates 4.5 despite its version number. Preserve this distinction.
    private static readonly Regex ModelsWithoutNoneReasoningEffort = new(
        @"^grok-(?:4\.[5-9](?![0-9])|(?:[5-9]|[1-9][0-9]+)(?:[.-]|\z))",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    // Preserve the model capabilities already declared in the provider page and shared overview. This migration does
    // not reinterpret retired model aliases. Maps to `languageDefinitions`. Known IDs without declared capabilities
    // (`grok-4.3`, `grok-latest`) retain the custom-model fallback.
    private static readonly IReadOnlyDictionary<string, XaiLanguageDefinition> LanguageDefinitions =
        new Dictionary<string, XaiLanguageDefinition>(StringComparer.Ordinal)
        {
            ["grok-4.7"] = new(true, true, ["low", "medium", "high", "xhigh"]),
            ["grok-4.6"] = new(true, true, ["low", "medium", "high", "xhigh"]),
            ["grok-4.5"] = new(true, true, ["low", "medium", "high"]),
            ["grok-4.20-reasoning"] = new(true, true),
            ["grok-4.20-non-reasoning"] = new(true, false),
            ["grok-4-1-fast-reasoning"] = new(true, true),
            ["grok-4-1-fast-non-reasoning"] = new(true, false),
            ["grok-4-1"] = new(false, false),
            ["grok-4-fast-reasoning"] = new(true, true),
            ["grok-4-fast-non-reasoning"] = new(true, false),
            ["grok-code-fast-1"] = new(false, true),
            ["grok-4"] = new(false, true),
            ["grok-3"] = new(false, false),
            ["grok-3-mini"] = new(false, true),
        };

    private static readonly string[] CommonAspectRatios =
    [
        "1:1",
        "16:9",
        "9:16",
        "4:3",
        "3:4",
        "3:2",
        "2:3",
        "2:1",
        "1:2",
        "19.5:9",
        "9:19.5",
        "20:9",
        "9:20",
    ];

    private static readonly XaiSpeechModelEntry DefaultSpeechModelEntry = new(
        new XaiSpeechCapabilities
        {
            Instructions = ModelCapability.Unsupported,
            Language = true,
            Speed = true,
            OutputFormats = ["mp3", "wav", "pcm", "mulaw", "alaw"],
        },
        DeclareProviderOptions(
            XaiProviderOptions.Speech,
            ModelCapability.Supported,
            new Dictionary<string, ModelCapability>
            {
                ["bitRate"] = ModelCapability.When("Only supported for MP3 output."),
            }
        ),
        new XaiSpeechRequestConfig
        {
            Voice = "eve",
            Language = "auto",
            OutputFormat = "mp3",
        }
    );

    private static readonly XaiTranscriptionModelEntry DefaultTranscriptionModelEntry =
        CreateTranscriptionModelEntry("", true);

    // Maps to `xaiImageModelEntries`.
    private static readonly IReadOnlyDictionary<string, XaiImageModelEntry> ImageModelEntries =
        new Dictionary<string, XaiImageModelEntry>(StringComparer.Ordinal)
        {
            ["grok-imagine-image-2.0"] = CreateImageModelEntry(
                new XaiImageDefinition(
                    ["1k", "1.5k", "2k"],
                    [.. CommonAspectRatios, "21:9", "5:2", "auto"],
                    ModelCapability.Supported
                )
            ),
            ["grok-imagine-image-pro"] = CreateImageModelEntry(
                new XaiImageDefinition(
                    ["1k", "1.5k", "2k"],
                    [.. CommonAspectRatios, "auto"],
                    ModelCapability.Unknown
                )
            ),
            ["grok-imagine-image"] = CreateImageModelEntry(
                new XaiImageDefinition(
                    ["1k"],
                    [.. CommonAspectRatios, "auto"],
                    ModelCapability.Unknown
                )
            ),
        };

    // Maps to `xaiTranscriptionModelEntries`.
    private static readonly IReadOnlyDictionary<
        string,
        XaiTranscriptionModelEntry
    > TranscriptionModelEntries = new Dictionary<string, XaiTranscriptionModelEntry>(
        StringComparer.Ordinal
    )
    {
        ["grok-voice-transcribe-2.0"] = CreateTranscriptionModelEntry(
            "grok-voice-transcribe-2.0",
            true
        ),
    };

    // Maps to `xaiRealtimeModelEntries`.
    private static readonly IReadOnlyDictionary<string, XaiModelEntry> RealtimeModelEntries =
        new Dictionary<string, XaiModelEntry>(StringComparer.Ordinal)
        {
            ["grok-voice-latest"] = new XaiModelEntry
            {
                Realtime = new RealtimeCapabilities
                {
                    Conversation = "turn-based",
                    Transports = ["websocket"],
                    Connections = ["client-secret-websocket", "server-websocket"],
                },
            },
        };

    /// <summary>
    /// Entries of every known model id. Maps to <c>xaiModelCatalog</c>; renamed because C# does not allow a member
    /// named like its class.
    /// </summary>
    public static IReadOnlyDictionary<string, XaiModelEntry> Entries { get; } = CreateEntries();

    /// <summary>Returns the entry of a language model. Any other model id resolves to an entry with unknown support. Maps to <c>getXaiLanguageModelEntry</c>.</summary>
    internal static XaiLanguageModelEntry GetXaiLanguageModelEntry(string modelId)
    {
        return
            FindModelEntry(modelId)
                is { Language: { } language, ProviderOptions.Language: { } options }
            ? new XaiLanguageModelEntry(language, options)
            : CreateLanguageModelEntry(modelId, null);
    }

    /// <summary>Returns the entry of an image model. Any other model id resolves to an entry with unknown support. Maps to <c>getXaiImageModelEntry</c>.</summary>
    internal static XaiImageModelEntry GetXaiImageModelEntry(string modelId)
    {
        return FindModelEntry(modelId) is { Image: { } image, ProviderOptions.Image: { } options }
            ? new XaiImageModelEntry(image, options)
            : CreateImageModelEntry(null);
    }

    /// <summary>Returns the entry of the speech model. Maps to <c>getXaiSpeechModelEntry</c>.</summary>
    internal static XaiSpeechModelEntry GetXaiSpeechModelEntry()
    {
        return DefaultSpeechModelEntry;
    }

    /// <summary>
    /// Entry of a transcription model. A declared model reports its capabilities as supported; any other model id
    /// reports them as unknown but is still sent as the <c>model</c> field. Maps to <c>getXaiTranscriptionModelEntry</c>.
    /// </summary>
    internal static XaiTranscriptionModelEntry GetXaiTranscriptionModelEntry(string modelId)
    {
        return
            FindModelEntry(modelId)
                is {
                    Transcription: { } transcription,
                    ProviderOptions.Transcription: { } options,
                    RequestConfig.Transcription: { } requestConfig,
                }
            ? new XaiTranscriptionModelEntry(transcription, options, requestConfig)
            : CreateTranscriptionModelEntry(modelId, false);
    }

    private static Dictionary<string, XaiModelEntry> CreateEntries()
    {
        var entries = new Dictionary<string, XaiModelEntry>(StringComparer.Ordinal);
        foreach (var (modelId, definition) in LanguageDefinitions)
        {
            var entry = CreateLanguageModelEntry(modelId, definition);
            entries[modelId] = new XaiModelEntry
            {
                Language = entry.Capabilities,
                ProviderOptions = new ModelProviderOptions { Language = entry.ProviderOptions },
            };
        }

        foreach (var (modelId, entry) in ImageModelEntries)
        {
            entries[modelId] = new XaiModelEntry
            {
                Image = entry.Capabilities,
                ProviderOptions = new ModelProviderOptions { Image = entry.ProviderOptions },
            };
        }

        // The factories use the empty model ID for both default speech and default transcription. Keep one record
        // containing both independent operations.
        entries[""] = new XaiModelEntry
        {
            Speech = DefaultSpeechModelEntry.Capabilities,
            Transcription = DefaultTranscriptionModelEntry.Capabilities,
            ProviderOptions = new ModelProviderOptions
            {
                Speech = DefaultSpeechModelEntry.ProviderOptions,
                Transcription = DefaultTranscriptionModelEntry.ProviderOptions,
            },
            RequestConfig = new XaiRequestConfig
            {
                Speech = DefaultSpeechModelEntry.RequestConfig,
                Transcription = DefaultTranscriptionModelEntry.RequestConfig,
            },
        };

        foreach (var (modelId, entry) in TranscriptionModelEntries)
        {
            entries[modelId] = new XaiModelEntry
            {
                Transcription = entry.Capabilities,
                ProviderOptions = new ModelProviderOptions
                {
                    Transcription = entry.ProviderOptions,
                },
                RequestConfig = new XaiRequestConfig { Transcription = entry.RequestConfig },
            };
        }

        foreach (var (modelId, entry) in RealtimeModelEntries)
        {
            entries[modelId] = entry;
        }

        return entries;
    }

    private static XaiModelEntry? FindModelEntry(string modelId)
    {
        return Entries.TryGetValue(modelId, out var entry) ? entry : null;
    }

    private static XaiLanguageModelEntry CreateLanguageModelEntry(
        string modelId,
        XaiLanguageDefinition? definition
    )
    {
        var configurable = !ModelsWithoutReasoningEffort.IsMatch(modelId);
        var declared = definition is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        var language = new XaiLanguageCapabilities
        {
            Inputs = new LanguageInputCapabilities
            {
                Image = definition is { } d
                    ? ModelCapability.From(d.Image)
                    : ModelCapability.Unknown,
                Audio = ModelCapability.Unknown,
            },
            ObjectGeneration = declared,
            Tools = new LanguageToolCapabilities { Usage = declared, Streaming = declared },
            Reasoning = new XaiLanguageReasoningCapabilities
            {
                Support = definition is { } r
                    ? ModelCapability.From(r.Reasoning)
                    : ModelCapability.Unknown,
                Efforts = definition?.Efforts,
                Portable = new XaiPortableReasoningCapabilities
                {
                    Configurable = configurable,
                    None = !ModelsWithoutNoneReasoningEffort.IsMatch(modelId),

                    // Keep the existing exact-ID exception. Native Grok 4.7 accepts xhigh, but portable reasoning
                    // currently maps it to high on that model.
                    Xhigh = modelId == "grok-4.6",
                },
            },
            Sampling = new LanguageSamplingCapabilities
            {
                Temperature = ModelCapability.Unknown,
                TopP = ModelCapability.Unknown,
                Logprobs = ModelCapability.Unknown,
            },
        };

        return new XaiLanguageModelEntry(
            language,
            DeclareProviderOptions(
                XaiProviderOptions.Language,
                declared,
                new Dictionary<string, ModelCapability>
                {
                    ["reasoningEffort"] = !configurable ? ModelCapability.Unsupported : declared,
                    ["reasoningSummary"] = ModelCapability.Unsupported,
                }
            )
        );
    }

    private static XaiImageModelEntry CreateImageModelEntry(XaiImageDefinition? definition)
    {
        var declared = definition is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        var image = new XaiImageCapabilities
        {
            SizesDescription =
                "Use aspectRatio and providerOptions.xai.resolution instead of size.",
            FileInput = declared,
            MaskInput = definition is not null
                ? ModelCapability.Unsupported
                : ModelCapability.Unknown,
            MaxImagesPerCall = 3,
            Resolutions = definition?.Resolutions,
            AspectRatios = definition?.AspectRatios,
        };

        return new XaiImageModelEntry(
            image,
            DeclareProviderOptions(
                XaiProviderOptions.Image,
                declared,
                new Dictionary<string, ModelCapability>
                {
                    ["quality"] = definition?.Quality ?? ModelCapability.Unknown,
                }
            )
        );
    }

    private static XaiTranscriptionModelEntry CreateTranscriptionModelEntry(
        string modelId,
        bool declared
    )
    {
        var support = declared ? ModelCapability.Supported : ModelCapability.Unknown;
        return new XaiTranscriptionModelEntry(
            new XaiTranscriptionCapabilities(support),
            DeclareProviderOptions(
                XaiProviderOptions.Transcription,
                support,
                new Dictionary<string, ModelCapability>
                {
                    ["audioFormat"] = ModelCapability.When(
                        "Opus encoding is supported for streaming requests only."
                    ),
                    ["format"] = ModelCapability.When(
                        "Requires language and a request/response transcription request."
                    ),
                    ["streaming"] = ModelCapability.When(
                        "Only supported for streaming transcription."
                    ),
                }
            ),
            new XaiTranscriptionRequestConfig { Model = modelId.Length > 0 ? modelId : null }
        );
    }

    // Maps to `declareProviderOptions`.
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
