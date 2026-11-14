// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Azure;

/// <summary>Request routing of an Azure-owned model. Maps to <c>AzureRequestConfig</c> in <c>azure-model-catalog.ts</c>.</summary>
public sealed class AzureModelRequestConfig
{
    internal AzureModelRequestConfig(bool maiImage, string? speechVoiceModel, string? transcribeName, bool transcribeSupportsTimestamps, bool maiStreamingTranscription)
    {
        MaiImage = maiImage;
        SpeechVoiceModel = speechVoiceModel;
        TranscribeName = transcribeName;
        TranscribeSupportsTimestamps = transcribeSupportsTimestamps;
        MaiStreamingTranscription = maiStreamingTranscription;
    }

    /// <summary>True when the model uses the MAI image API. Maps to <c>image.api === 'mai'</c>.</summary>
    public bool MaiImage { get; }

    /// <summary>Voice model name for the Azure Speech API, for example <c>MAI-Voice-2</c>. Maps to <c>speech.voiceModel</c>.</summary>
    public string? SpeechVoiceModel { get; }

    /// <summary>Model name for the Azure Speech transcription API. Maps to <c>transcription.name</c>.</summary>
    public string? TranscribeName { get; }

    /// <summary>True when the Speech transcription model returns timestamps. Maps to <c>transcription.supportsTimestamps</c>.</summary>
    public bool TranscribeSupportsTimestamps { get; }

    /// <summary>True when the model uses MAI streaming transcription. Maps to <c>transcription.api === 'mai'</c>.</summary>
    public bool MaiStreamingTranscription { get; }
}

/// <summary>Azure-owned models. Maps to <c>azureModelCatalog</c> in <c>azure-model-catalog.ts</c>.</summary>
public static class AzureModelCatalog
{
    private static readonly Dictionary<string, AzureModelRequestConfig> Entries = new(StringComparer.Ordinal)
    {
        ["mai-image-2.5"] = Image(),
        ["mai-image-2.5-flash"] = Image(),
        ["mai-image-2.5-pro"] = Image(),
        ["mai-image-2.6"] = Image(),
        ["mai-image-2.6-flash"] = Image(),
        ["mai-voice-2.1-flash"] = Speech("MAI-Voice-2.1-Flash"),
        ["mai-voice-2.1"] = Speech("MAI-Voice-2.1"),
        ["mai-voice-2-flash"] = Speech("MAI-Voice-2-Flash"),
        ["mai-voice-2"] = Speech("MAI-Voice-2"),
        ["mai-transcribe-2"] = new AzureModelRequestConfig(false, null, "MAI-Transcribe-2", true, false),
        ["mai-transcribe-1.5"] = new AzureModelRequestConfig(false, null, "MAI-Transcribe-1.5", false, false),
        ["mai-transcribe-2-streaming"] = new AzureModelRequestConfig(false, null, null, false, true),
    };

    /// <summary>Returns the entry for <paramref name="modelId"/>, case-insensitively, or null. Maps to <c>getAzureModelEntry</c>.</summary>
    public static AzureModelRequestConfig? GetAzureModelEntry(string modelId)
    {
        return Entries.TryGetValue(modelId.ToLowerInvariant(), out var entry) ? entry : null;
    }

    private static AzureModelRequestConfig Image()
    {
        return new AzureModelRequestConfig(true, null, null, false, false);
    }

    private static AzureModelRequestConfig Speech(string voiceModel)
    {
        return new AzureModelRequestConfig(false, voiceModel, null, false, false);
    }
}
