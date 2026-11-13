// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Deepgram;

/// <summary>Capabilities of a Deepgram transcription model.</summary>
public sealed class DeepgramTranscriptionCapabilities
{
    internal DeepgramTranscriptionCapabilities(bool streaming)
    {
        Streaming = streaming;
    }

    /// <summary>True when the model streams over <c>/v1/listen</c>. Flux models use <c>/v2/listen</c> and are not supported.</summary>
    public bool Streaming { get; }
}

/// <summary>Deepgram model catalog. Maps to <c>deepgram-model-catalog.ts</c>.</summary>
public static class DeepgramModelCatalog
{
    /// <summary>Returns the capabilities of a transcription model id.</summary>
    public static DeepgramTranscriptionCapabilities GetDeepgramTranscriptionModelEntry(string modelId)
    {
        return new DeepgramTranscriptionCapabilities(!(modelId ?? string.Empty).StartsWith("flux", StringComparison.Ordinal));
    }
}
