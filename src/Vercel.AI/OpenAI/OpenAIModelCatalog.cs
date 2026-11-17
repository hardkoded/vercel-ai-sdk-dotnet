// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Capabilities of an OpenAI transcription model. <see langword="null"/> means unknown, as upstream's <c>'unknown'</c>.</summary>
internal sealed class OpenAITranscriptionCapabilities
{
    internal OpenAITranscriptionCapabilities(
        bool? transcription,
        bool? streaming,
        bool? streamingDelay
    )
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
    internal OpenAITranscriptionModelEntry(
        OpenAITranscriptionCapabilities capabilities,
        string languageField,
        string? responseFormat = null,
        string? chunkingStrategy = null
    )
    {
        ChunkingStrategy = chunkingStrategy;
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

    /// <summary>The chunking strategy the model requests by default, or <see langword="null"/> for none. Maps to <c>requestConfig.transcription.chunkingStrategy</c>.</summary>
    public string? ChunkingStrategy { get; }
}

/// <summary>A catalog entry of an OpenAI realtime model. Maps to <c>OpenAIRealtimeModelEntry</c>.</summary>
internal sealed class OpenAIRealtimeModelEntry
{
    internal OpenAIRealtimeModelEntry(
        RealtimeCapabilities capabilities,
        string api,
        string startup,
        string finalization
    )
    {
        Capabilities = capabilities;
        Api = api;
        Startup = startup;
        Finalization = finalization;
    }

    /// <summary>Model capabilities. Maps to <c>capabilities.realtime</c>.</summary>
    public RealtimeCapabilities Capabilities { get; }

    /// <summary>The API that serves the model: <c>live</c> or <c>realtime</c>. Maps to <c>requestConfig.realtime.api</c>.</summary>
    public string Api { get; }

    /// <summary>How a session starts: <c>session-start</c> or <c>session-update</c>. Maps to <c>requestConfig.realtime.startup</c>.</summary>
    public string Startup { get; }

    /// <summary>How a session ends: <c>session-close</c> or <c>transport-close</c>. Maps to <c>requestConfig.realtime.finalization</c>.</summary>
    public string Finalization { get; }
}

/// <summary>The capabilities of the Live realtime API with its startup and finalization. Maps to <c>openaiLiveRealtimeCapabilities</c>.</summary>
internal sealed class OpenAILiveRealtimeCapabilities
{
    internal OpenAILiveRealtimeCapabilities(OpenAIRealtimeModelEntry entry)
    {
        Conversation = entry.Capabilities.Conversation;
        Transports = entry.Capabilities.Transports;
        Connections = entry.Capabilities.Connections;
        Startup = entry.Startup;
        Finalization = entry.Finalization;
    }

    /// <summary>Conversation mode.</summary>
    public string Conversation { get; }

    /// <summary>Transports.</summary>
    public IReadOnlyList<string> Transports { get; }

    /// <summary>Connection kinds.</summary>
    public IReadOnlyList<string> Connections { get; }

    /// <summary>How a session starts.</summary>
    public string Startup { get; }

    /// <summary>How a session ends.</summary>
    public string Finalization { get; }
}

/// <summary>OpenAI model catalog. Maps to <c>openai-model-catalog.ts</c>.</summary>
internal static class OpenAIModelCatalog
{
    private static readonly OpenAIRealtimeModelEntry LiveModelEntry = new(
        new RealtimeCapabilities
        {
            Conversation = "continuous",
            Transports = ["websocket", "webrtc"],
            Connections = ["server-websocket", "webrtc"],
        },
        "live",
        "session-start",
        "session-close"
    );

    private static readonly OpenAIRealtimeModelEntry RealtimeModelEntry = new(
        new RealtimeCapabilities
        {
            Conversation = "turn-based",
            Transports = ["websocket"],
            Connections = ["client-secret-websocket", "server-websocket"],
        },
        "realtime",
        "session-update",
        "transport-close"
    );

    private static readonly Dictionary<string, OpenAIRealtimeModelEntry> RealtimeEntries = new(
        StringComparer.Ordinal
    )
    {
        ["gpt-live-1"] = LiveModelEntry,
        ["gpt-realtime"] = RealtimeModelEntry,
    };

    /// <summary>The Live realtime capabilities derived from the catalog. Maps to <c>openaiLiveRealtimeCapabilities</c>.</summary>
    internal static OpenAILiveRealtimeCapabilities OpenAILiveRealtimeCapabilities { get; } =
        new(LiveModelEntry);

    /// <summary>
    /// Returns the realtime entry of a model id. An unknown id, or one the requested <paramref name="api"/> does not serve,
    /// resolves to that API's default entry; with no <paramref name="api"/> it resolves to the Realtime entry.
    /// Maps to <c>getOpenAIRealtimeModelEntry</c>.
    /// </summary>
    public static OpenAIRealtimeModelEntry GetOpenAIRealtimeModelEntry(
        string modelId,
        string? api = null
    )
    {
        if (
            RealtimeEntries.TryGetValue(modelId, out var entry) && (api == null || entry.Api == api)
        )
        {
            return entry;
        }

        return api == "live" ? LiveModelEntry : RealtimeModelEntry;
    }

    private static readonly string[] TranscriptionSnapshotModelIds =
    {
        "gpt-transcribe",
        "gpt-live-transcribe",
        "gpt-realtime-whisper",
    };

    private static readonly Dictionary<string, OpenAITranscriptionModelEntry> TranscriptionEntries =
        new Dictionary<string, OpenAITranscriptionModelEntry>(StringComparer.Ordinal)
        {
            ["whisper-1"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(true, false, false),
                "language",
                "verbose_json"
            ),
            ["gpt-4o-mini-transcribe"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(true, false, false),
                "language"
            ),
            ["gpt-4o-transcribe"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(true, false, false),
                "language"
            ),
            ["gpt-4o-transcribe-diarize"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(true, false, false),
                "language",
                "diarized_json",
                "auto"
            ),
            ["gpt-transcribe"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(true, true, false),
                "languages"
            ),
            ["gpt-live-transcribe"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(false, true, true),
                "languages"
            ),
            ["gpt-realtime-whisper"] = new OpenAITranscriptionModelEntry(
                new OpenAITranscriptionCapabilities(false, true, true),
                "language"
            ),
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

        return new OpenAITranscriptionModelEntry(
            new OpenAITranscriptionCapabilities(null, null, null),
            "language"
        );
    }
}
