// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Xai;

/// <summary>Capabilities of an xAI transcription model. <see langword="null"/> means unknown, as upstream's <c>'unknown'</c>.</summary>
internal sealed class XaiTranscriptionCapabilities
{
    internal XaiTranscriptionCapabilities(bool? support)
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
    public bool? Transcription { get; }

    /// <summary>Streaming transcription support.</summary>
    public bool? Streaming { get; }

    /// <summary>Whether the result reports the audio duration.</summary>
    public bool? Duration { get; }

    /// <summary>Whether the result carries segments.</summary>
    public bool? Segments { get; }

    /// <summary>Whether the result reports the language.</summary>
    public bool? Language { get; }

    /// <summary>Whether the result carries word timestamps.</summary>
    public bool? WordTimestamps { get; }

    /// <summary>Whether speaker diarization is supported.</summary>
    public bool? Diarization { get; }

    /// <summary>Whether multichannel transcription is supported.</summary>
    public bool? Multichannel { get; }
}

/// <summary>A catalog entry of an xAI transcription model. Maps to the result of <c>getXaiTranscriptionModelEntry</c>.</summary>
internal sealed class XaiTranscriptionModelEntry
{
    internal XaiTranscriptionModelEntry(XaiTranscriptionCapabilities capabilities, IReadOnlyDictionary<string, XaiProviderOptionSupport> providerOptions, string? model)
    {
        Capabilities = capabilities;
        ProviderOptions = providerOptions;
        Model = model;
    }

    /// <summary>Model capabilities. Maps to <c>capabilities.transcription</c>.</summary>
    public XaiTranscriptionCapabilities Capabilities { get; }

    /// <summary>Support and condition of each transcription provider option. Maps to <c>providerOptions.transcription</c>.</summary>
    public IReadOnlyDictionary<string, XaiProviderOptionSupport> ProviderOptions { get; }

    /// <summary>The <c>model</c> request field, or <see langword="null"/> for the empty model id. Maps to <c>requestConfig.transcription.model</c>.</summary>
    public string? Model { get; }
}

/// <summary>Support of one provider option. Maps to <c>declareProviderOptions</c> entries.</summary>
internal sealed class XaiProviderOptionSupport
{
    internal XaiProviderOptionSupport(string description, bool? support, string? condition)
    {
        Description = description;
        Support = support;
        Condition = condition;
    }

    /// <summary>What the option does.</summary>
    public string Description { get; }

    /// <summary>True when supported (conditionally, when <see cref="Condition"/> is set), <see langword="null"/> when unknown. A conditional option is an exception to the model-wide support, as in <c>declareProviderOptions</c>.</summary>
    public bool? Support { get; }

    /// <summary>When the option applies, if conditional.</summary>
    public string? Condition { get; }
}

/// <summary>xAI model catalog. Maps to <c>xai-model-catalog.ts</c>. Only the transcription entries are ported.</summary>
internal static class XaiModelCatalog
{
    private static readonly string[] KnownTranscriptionModelIds = { "grok-voice-transcribe-2.0" };

    private static readonly (string Name, string Description, string? Condition)[] TranscriptionOptions =
    {
        ("audioFormat", "Encoding of raw input audio.", "Opus encoding is supported for streaming requests only."),
        ("sampleRate", "Input audio sample rate.", null),
        ("language", "Language for inverse text normalization.", null),
        ("format", "Enable inverse text normalization.", "Requires language and a request/response transcription request."),
        ("multichannel", "Transcribe each audio channel separately.", null),
        ("channels", "Number of interleaved input audio channels.", null),
        ("diarize", "Enable speaker diarization.", null),
        ("keyterm", "Terms to bias transcription toward.", null),
        ("fillerWords", "Include filler words in the transcript.", null),
        ("vadThreshold", "Voice activity detection threshold.", null),
        ("streaming", "WebSocket streaming transcription settings.", "Only supported for streaming transcription."),
    };

    /// <summary>
    /// Entry of a transcription model. A declared model reports its capabilities as supported; any other model id
    /// reports them as unknown but is still sent as the <c>model</c> field. Maps to <c>getXaiTranscriptionModelEntry</c>.
    /// </summary>
    internal static XaiTranscriptionModelEntry GetXaiTranscriptionModelEntry(string modelId)
    {
        // The empty model id is the default model and is declared.
        var declared = modelId.Length == 0 || Array.IndexOf(KnownTranscriptionModelIds, modelId) >= 0;
        return CreateTranscriptionModelEntry(modelId, declared);
    }

    private static XaiTranscriptionModelEntry CreateTranscriptionModelEntry(string modelId, bool declared)
    {
        bool? support = declared ? true : null;
        var options = new Dictionary<string, XaiProviderOptionSupport>(StringComparer.Ordinal);
        foreach (var (name, description, condition) in TranscriptionOptions)
        {
            options[name] = new XaiProviderOptionSupport(description, condition is null ? support : true, condition);
        }

        return new XaiTranscriptionModelEntry(new XaiTranscriptionCapabilities(support), options, modelId.Length > 0 ? modelId : null);
    }
}
