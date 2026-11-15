// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure Speech transcription provider options. Maps to <c>azure-speech-transcription-model-options.ts</c>.</summary>
internal static class AzureSpeechTranscriptionModelOptions
{
    /// <summary>
    /// Shape of the Speech options. Strict objects reject unknown keys, such as <c>diarization.maxSpeakers</c>, which
    /// MAI-Transcribe-2 does not support, instead of silently dropping them. Maps to <c>azureSpeechTranscriptionModelOptionsShape</c>.
    /// </summary>
    public static Dictionary<string, OptionValidator> AzureSpeechTranscriptionModelOptionsShape()
    {
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["timestamps"] = ProviderOptionShape.Enum("word", "segment", "none"),
            ["transcribeStyle"] = ProviderOptionShape.Enum("verbatim", "clean"),
            ["locales"] = ProviderOptionShape.ArrayOf(ProviderOptionShape.String(), length: 1),
            ["diarization"] = ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal) { ["enabled"] = ProviderOptionShape.Boolean() },
                true,
                "enabled"),
            ["phraseList"] = ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal) { ["phrases"] = ProviderOptionShape.ArrayOf(ProviderOptionShape.String()) },
                true,
                "phrases"),
        };
    }
}

/// <summary>Options of the Azure Speech transcription API. Maps to <c>AzureTranscriptionModelSpeechOptions</c>.</summary>
internal sealed class AzureTranscriptionModelSpeechOptions
{
    /// <summary>Timing granularity. Defaults to <c>segment</c> so results include timed segments (Azure's own default is <c>none</c>).</summary>
    public string? Timestamps { get; init; }

    /// <summary><c>verbatim</c> keeps fillers and false starts, <c>clean</c> removes them. Azure defaults to <c>verbatim</c>.</summary>
    public string? TranscribeStyle { get; init; }

    /// <summary>Forces a single language, for example <c>["en"]</c>. Omit for automatic language detection and code switching.</summary>
    public JsonNode? Locales { get; init; }

    /// <summary>Speaker diarization. Speaker IDs are available in provider metadata.</summary>
    public JsonNode? Diarization { get; init; }

    /// <summary>Keyword biasing for names and domain terminology.</summary>
    public JsonNode? PhraseList { get; init; }

    /// <summary>Reads the Speech options out of the picked <c>azure</c> options.</summary>
    public static AzureTranscriptionModelSpeechOptions From(JsonObject options)
    {
        return new AzureTranscriptionModelSpeechOptions
        {
            Timestamps = options["timestamps"]?.GetValue<string>(),
            TranscribeStyle = options["transcribeStyle"]?.GetValue<string>(),
            Locales = options["locales"]?.DeepClone(),
            Diarization = options["diarization"]?.DeepClone(),
            PhraseList = options["phraseList"]?.DeepClone(),
        };
    }
}
