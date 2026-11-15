// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenAI;

/// <summary>Capabilities of an OpenAI transcription model. <see langword="null"/> means unknown, as upstream's <c>'unknown'</c>.</summary>
internal sealed class OpenAITranscriptionCapabilities
{
    internal OpenAITranscriptionCapabilities(bool? transcription, bool? streaming, bool? streamingDelay)
    {
        Transcription = transcription;
        Streaming = streaming;
        StreamingDelay = streamingDelay;
    }

    /// <summary>True when the model supports the REST transcription endpoint.</summary>
    public bool? Transcription { get; }

    /// <summary>True when the model supports realtime streaming transcription.</summary>
    public bool? Streaming { get; }

    /// <summary>True when the realtime session accepts a <c>delay</c> setting.</summary>
    public bool? StreamingDelay { get; }
}

/// <summary>A catalog entry of an OpenAI transcription model. Maps to <c>OpenAITranscriptionModelEntry</c>.</summary>
internal sealed class OpenAITranscriptionModelEntry
{
    internal OpenAITranscriptionModelEntry(OpenAITranscriptionCapabilities capabilities, string languageField, string? responseFormat = null)
    {
        Capabilities = capabilities;
        ResponseFormat = responseFormat;
        LanguageField = languageField;
    }

    /// <summary>Model capabilities.</summary>
    public OpenAITranscriptionCapabilities Capabilities { get; }

    /// <summary>The request field that carries a language hint: <c>language</c> or <c>languages</c>. Maps to <c>requestConfig.transcription.languageField</c>.</summary>
    public string LanguageField { get; }

    /// <summary>The response format the model requests by default, or <see langword="null"/> for the provider default JSON. Maps to <c>requestConfig.transcription.responseFormat</c>.</summary>
    public string? ResponseFormat { get; }
}

/// <summary>OpenAI model catalog. Maps to <c>openai-model-catalog.ts</c>.</summary>
internal static class OpenAIModelCatalog
{
    private static readonly string[] TranscriptionSnapshotModelIds = { "gpt-transcribe", "gpt-live-transcribe", "gpt-realtime-whisper" };

    private static readonly Dictionary<string, OpenAITranscriptionModelEntry> TranscriptionEntries = new Dictionary<string, OpenAITranscriptionModelEntry>(StringComparer.Ordinal)
    {
        ["whisper-1"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(true, false, false), "language", "verbose_json"),
        ["gpt-4o-mini-transcribe"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(true, false, false), "language"),
        ["gpt-4o-transcribe"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(true, false, false), "language"),
        ["gpt-4o-transcribe-diarize"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(true, false, false), "language", "diarized_json"),
        ["gpt-transcribe"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(true, true, false), "languages"),
        ["gpt-live-transcribe"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(false, true, true), "languages"),
        ["gpt-realtime-whisper"] = new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(false, true, true), "language"),
    };

    /// <summary>Returns the entry of a transcription model id. Dated snapshots resolve to their base model. Maps to <c>getOpenAITranscriptionModelEntry</c>.</summary>
    public static OpenAITranscriptionModelEntry GetOpenAITranscriptionModelEntry(string modelId)
    {
        if (TranscriptionEntries.TryGetValue(modelId, out var entry))
        {
            return entry;
        }

        foreach (var baseModelId in TranscriptionSnapshotModelIds)
        {
            if (modelId.StartsWith(baseModelId + "-", StringComparison.Ordinal))
            {
                return TranscriptionEntries[baseModelId];
            }
        }

        return new OpenAITranscriptionModelEntry(new OpenAITranscriptionCapabilities(null, null, null), "language");
    }
}
