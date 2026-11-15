// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Support declared by a provider's model catalog. Upstream <c>ModelCapability</c>
/// (<c>boolean | 'unknown' | { condition: string }</c>). Unknown is distinct from unsupported.
/// A condition describes when support is available; it is documentation, not an expression to execute.
/// </summary>
public sealed class ModelCapability
{
    /// <summary>Supported.</summary>
    public static readonly ModelCapability Supported = new(true, null, false);

    /// <summary>Not supported.</summary>
    public static readonly ModelCapability Unsupported = new(false, null, false);

    /// <summary>Support is not declared. Upstream <c>'unknown'</c>.</summary>
    public static readonly ModelCapability Unknown = new(false, null, true);

    private ModelCapability(bool supported, string? condition, bool isUnknown)
    {
        IsSupported = supported;
        Condition = condition;
        IsUnknown = isUnknown;
    }

    /// <summary>True when support is declared as <c>true</c>.</summary>
    public bool IsSupported { get; }

    /// <summary>True when support is declared as <c>'unknown'</c>.</summary>
    public bool IsUnknown { get; }

    /// <summary>When support is available, for the <c>{ condition }</c> form. Null otherwise.</summary>
    public string? Condition { get; }

    /// <summary>Maps a boolean to <see cref="Supported"/> or <see cref="Unsupported"/>.</summary>
    public static ModelCapability From(bool supported) => supported ? Supported : Unsupported;

    /// <summary>The <c>{ condition }</c> form.</summary>
    public static ModelCapability When(string condition) => new(false, condition, false);
}

/// <summary>A provider option, declared once at the provider level. Upstream <c>ProviderOptionDefinition</c>.</summary>
public sealed class ProviderOptionDefinition
{
    /// <summary>Creates a definition.</summary>
    public ProviderOptionDefinition(string description)
    {
        Description = description;
    }

    /// <summary>What the option does.</summary>
    public string Description { get; }
}

/// <summary>
/// Model metadata shared by provider implementations and generated documentation. Upstream <c>ModelCapabilities</c>.
/// These declarations describe the capabilities exposed by the provider. They do not introduce validation or change
/// request behavior. A null section is undeclared, not explicitly unsupported.
/// </summary>
public sealed class ModelCapabilities
{
    /// <summary>Upstream <c>language</c>.</summary>
    public LanguageCapabilities? Language { get; init; }

    /// <summary>Upstream <c>embedding</c>.</summary>
    public EmbeddingCapabilities? Embedding { get; init; }

    /// <summary>Upstream <c>image</c>.</summary>
    public ImageCapabilities? Image { get; init; }

    /// <summary>Upstream <c>video</c>.</summary>
    public VideoCapabilities? Video { get; init; }

    /// <summary>Upstream <c>reranking</c>.</summary>
    public RerankingCapabilities? Reranking { get; init; }

    /// <summary>Upstream <c>decision</c>.</summary>
    public DecisionCapabilities? Decision { get; init; }

    /// <summary>Upstream <c>transcription</c>.</summary>
    public TranscriptionCapabilities? Transcription { get; init; }

    /// <summary>Upstream <c>speechTranslation</c>.</summary>
    public SpeechTranslationCapabilities? SpeechTranslation { get; init; }

    /// <summary>Upstream <c>speech</c>.</summary>
    public SpeechCapabilities? Speech { get; init; }

    /// <summary>Upstream <c>realtime</c>.</summary>
    public RealtimeCapabilities? Realtime { get; init; }
}

/// <summary>Upstream <c>ModelCapabilities.language</c>.</summary>
public sealed class LanguageCapabilities
{
    /// <summary>Upstream <c>inputs</c>.</summary>
    public LanguageInputCapabilities Inputs { get; init; } = new();

    /// <summary>Upstream <c>objectGeneration</c>.</summary>
    public ModelCapability ObjectGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>tools</c>.</summary>
    public LanguageToolCapabilities Tools { get; init; } = new();

    /// <summary>Upstream <c>reasoning</c>.</summary>
    public LanguageReasoningCapabilities Reasoning { get; init; } = new();

    /// <summary>Upstream <c>sampling</c>.</summary>
    public LanguageSamplingCapabilities Sampling { get; init; } = new();
}

/// <summary>Upstream <c>ModelCapabilities.language.inputs</c>.</summary>
public sealed class LanguageInputCapabilities
{
    /// <summary>Image input.</summary>
    public ModelCapability Image { get; init; } = ModelCapability.Unknown;

    /// <summary>Audio input.</summary>
    public ModelCapability Audio { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.language.tools</c>.</summary>
public sealed class LanguageToolCapabilities
{
    /// <summary>Tool usage.</summary>
    public ModelCapability Usage { get; init; } = ModelCapability.Unknown;

    /// <summary>Tool streaming.</summary>
    public ModelCapability Streaming { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.language.reasoning</c>.</summary>
public sealed class LanguageReasoningCapabilities
{
    /// <summary>Reasoning support.</summary>
    public ModelCapability Support { get; init; } = ModelCapability.Unknown;

    /// <summary>Null means the provider does not declare an exhaustive list.</summary>
    public IReadOnlyList<string>? Efforts { get; init; }
}

/// <summary>Upstream <c>ModelCapabilities.language.sampling</c>.</summary>
public sealed class LanguageSamplingCapabilities
{
    /// <summary>Temperature.</summary>
    public ModelCapability Temperature { get; init; } = ModelCapability.Unknown;

    /// <summary>Top-p.</summary>
    public ModelCapability TopP { get; init; } = ModelCapability.Unknown;

    /// <summary>Logprobs.</summary>
    public ModelCapability Logprobs { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.embedding</c>.</summary>
public sealed class EmbeddingCapabilities
{
    /// <summary>Upstream <c>defaultDimensions</c>.</summary>
    public int DefaultDimensions { get; init; }

    /// <summary>Upstream <c>customDimensions</c>.</summary>
    public ModelCapability CustomDimensions { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.image</c>.</summary>
public sealed class ImageCapabilities
{
    /// <summary>Upstream <c>sizes</c>, when it is a list. Null when <see cref="SizesDescription"/> is used.</summary>
    public IReadOnlyList<string>? Sizes { get; init; }

    /// <summary>Upstream <c>sizes</c>, when it is <c>{ description }</c>. Null when <see cref="Sizes"/> is used.</summary>
    public string? SizesDescription { get; init; }

    /// <summary>Upstream <c>fileInput</c>.</summary>
    public ModelCapability FileInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>maskInput</c>.</summary>
    public ModelCapability MaskInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>maxImagesPerCall</c>.</summary>
    public int? MaxImagesPerCall { get; init; }
}

/// <summary>Upstream <c>ModelCapabilities.video</c>.</summary>
public sealed class VideoCapabilities
{
    /// <summary>Upstream <c>inputs</c>.</summary>
    public VideoInputCapabilities Inputs { get; init; } = new();

    /// <summary>Audio generation.</summary>
    public ModelCapability GenerateAudio { get; init; } = ModelCapability.Unknown;

    /// <summary>Aspect ratio.</summary>
    public ModelCapability AspectRatio { get; init; } = ModelCapability.Unknown;

    /// <summary>Resolution.</summary>
    public ModelCapability Resolution { get; init; } = ModelCapability.Unknown;

    /// <summary>Duration.</summary>
    public ModelCapability Duration { get; init; } = ModelCapability.Unknown;

    /// <summary>Frames per second.</summary>
    public ModelCapability Fps { get; init; } = ModelCapability.Unknown;

    /// <summary>Seed.</summary>
    public ModelCapability Seed { get; init; } = ModelCapability.Unknown;

    /// <summary>Null means the catalog does not declare a fixed limit.</summary>
    public int? MaxVideosPerCall { get; init; }
}

/// <summary>Upstream <c>ModelCapabilities.video.inputs</c>.</summary>
public sealed class VideoInputCapabilities
{
    /// <summary>Text input.</summary>
    public ModelCapability Text { get; init; } = ModelCapability.Unknown;

    /// <summary>The single starting image passed through the <c>image</c> option.</summary>
    public ModelCapability Image { get; init; } = ModelCapability.Unknown;

    /// <summary>Role-tagged first frame passed through the <c>frameImages</c> option.</summary>
    public ModelCapability FirstFrame { get; init; } = ModelCapability.Unknown;

    /// <summary>Role-tagged last frame passed through the <c>frameImages</c> option.</summary>
    public ModelCapability LastFrame { get; init; } = ModelCapability.Unknown;

    /// <summary>Reference images passed through the <c>inputReferences</c> option.</summary>
    public ModelCapability ReferenceImage { get; init; } = ModelCapability.Unknown;

    /// <summary>Reference videos passed through the <c>inputReferences</c> option.</summary>
    public ModelCapability ReferenceVideo { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.reranking</c>.</summary>
public sealed class RerankingCapabilities
{
    /// <summary>Input support exposed by the provider, including compatibility conversions.</summary>
    public RerankingDocumentCapabilities Documents { get; init; } = new();

    /// <summary>Upstream <c>topN</c>.</summary>
    public ModelCapability TopN { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.reranking.documents</c>.</summary>
public sealed class RerankingDocumentCapabilities
{
    /// <summary>Text documents.</summary>
    public ModelCapability Text { get; init; } = ModelCapability.Unknown;

    /// <summary>Object documents.</summary>
    public ModelCapability Object { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.decision</c>.</summary>
public sealed class DecisionCapabilities
{
    /// <summary>Upstream <c>questionTypes</c>.</summary>
    public DecisionQuestionTypeCapabilities QuestionTypes { get; init; } = new();

    /// <summary>Upstream <c>inputs</c>.</summary>
    public DecisionInputCapabilities Inputs { get; init; } = new();

    /// <summary>Upstream <c>reasoning</c>.</summary>
    public ModelCapability Reasoning { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>maxOutputTokens</c>.</summary>
    public ModelCapability MaxOutputTokens { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.decision.questionTypes</c>.</summary>
public sealed class DecisionQuestionTypeCapabilities
{
    /// <summary>Upstream <c>boolean</c>.</summary>
    public ModelCapability Boolean { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>choice</c>.</summary>
    public ModelCapability Choice { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>score</c>.</summary>
    public ModelCapability Score { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>box</c>.</summary>
    public ModelCapability Box { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.decision.inputs</c>.</summary>
public sealed class DecisionInputCapabilities
{
    /// <summary>Image input.</summary>
    public ModelCapability Image { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.transcription</c>.</summary>
public sealed class TranscriptionCapabilities
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

    /// <summary>Upstream <c>streamingDelay</c>. Null when omitted.</summary>
    public ModelCapability? StreamingDelay { get; init; }
}

/// <summary>Upstream <c>ModelCapabilities.speechTranslation</c>.</summary>
public sealed class SpeechTranslationCapabilities
{
    /// <summary>Upstream <c>translatedAudio</c>.</summary>
    public ModelCapability TranslatedAudio { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>translatedText</c>.</summary>
    public ModelCapability TranslatedText { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>sourceTranscript</c>.</summary>
    public ModelCapability SourceTranscript { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.speech</c>.</summary>
public sealed class SpeechCapabilities
{
    /// <summary>Upstream <c>instructions</c>.</summary>
    public ModelCapability Instructions { get; init; } = ModelCapability.Unknown;
}

/// <summary>Upstream <c>ModelCapabilities.realtime</c>.</summary>
public sealed class RealtimeCapabilities
{
    /// <summary>Upstream <c>conversation</c>: <c>continuous</c> or <c>turn-based</c>.</summary>
    public string Conversation { get; init; } = "continuous";

    /// <summary>Upstream <c>transports</c>: <c>websocket</c> or <c>webrtc</c>.</summary>
    public IReadOnlyList<string> Transports { get; init; } = [];

    /// <summary>Upstream <c>connections</c>: <c>client-secret-websocket</c>, <c>server-websocket</c> or <c>webrtc</c>.</summary>
    public IReadOnlyList<string> Connections { get; init; } = [];
}

/// <summary>
/// The provider options a provider accepts, grouped by the operation they configure. Upstream
/// <c>ProviderOptionDefinitions</c>. Section names match <see cref="ModelCapabilities"/>; option names match the keys
/// accepted under <c>providerOptions.&lt;provider&gt;</c> for that operation.
/// </summary>
public sealed class ProviderOptionDefinitions
{
    /// <summary>Upstream <c>language</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Language { get; init; }

    /// <summary>Upstream <c>embedding</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Embedding { get; init; }

    /// <summary>Upstream <c>image</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Image { get; init; }

    /// <summary>Upstream <c>video</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Video { get; init; }

    /// <summary>Upstream <c>reranking</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Reranking { get; init; }

    /// <summary>Upstream <c>decision</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Decision { get; init; }

    /// <summary>Upstream <c>transcription</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Transcription { get; init; }

    /// <summary>Upstream <c>speechTranslation</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? SpeechTranslation { get; init; }

    /// <summary>Upstream <c>speech</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Speech { get; init; }

    /// <summary>Upstream <c>realtime</c>.</summary>
    public IReadOnlyDictionary<string, ProviderOptionDefinition>? Realtime { get; init; }
}

/// <summary>
/// Per-model support for the options in the provider's <see cref="ProviderOptionDefinitions"/>, keyed by the same
/// sections and option names. Upstream <c>ModelProviderOptions</c>.
/// </summary>
public sealed class ModelProviderOptions
{
    /// <summary>Upstream <c>language</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Language { get; init; }

    /// <summary>Upstream <c>embedding</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Embedding { get; init; }

    /// <summary>Upstream <c>image</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Image { get; init; }

    /// <summary>Upstream <c>video</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Video { get; init; }

    /// <summary>Upstream <c>reranking</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Reranking { get; init; }

    /// <summary>Upstream <c>decision</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Decision { get; init; }

    /// <summary>Upstream <c>transcription</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Transcription { get; init; }

    /// <summary>Upstream <c>speechTranslation</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? SpeechTranslation { get; init; }

    /// <summary>Upstream <c>speech</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Speech { get; init; }

    /// <summary>Upstream <c>realtime</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability>? Realtime { get; init; }
}

/// <summary>
/// One provider-owned model record, which can declare several operations. Upstream <c>ModelEntry&lt;RequestConfig&gt;</c>.
/// <typeparamref name="TRequestConfig"/> is a provider-defined type for request encoding and defaults. Feature support
/// belongs in <see cref="Capabilities"/>, including provider-specific features.
/// </summary>
/// <typeparam name="TRequestConfig">The provider-defined request configuration type.</typeparam>
public sealed class ModelEntry<TRequestConfig>
    where TRequestConfig : class
{
    /// <summary>Upstream <c>capabilities</c>.</summary>
    public ModelCapabilities Capabilities { get; init; } = new();

    /// <summary>Omitted sections or options are undeclared, not explicitly unsupported.</summary>
    public ModelProviderOptions? ProviderOptions { get; init; }

    /// <summary>Upstream <c>requestConfig</c>.</summary>
    public TRequestConfig? RequestConfig { get; init; }
}
