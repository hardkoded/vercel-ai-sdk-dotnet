// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.ElevenLabs;

/// <summary>
/// Validation of <c>providerOptions.elevenlabs</c> for transcription models. Maps to
/// <c>elevenLabsTranscriptionModelOptionsSchema</c> of <c>elevenlabs-transcription-model-options.ts</c>. Explicit
/// <c>null</c> values count as absent, like <c>nullish()</c>. Unknown keys are ignored, like zod.
/// </summary>
internal static class ElevenLabsTranscriptionModelOptions
{
    private const string Parameter = "providerOptions.elevenlabs";

    /// <summary>Validates the <c>elevenlabs</c> entry of the provider options. Maps to <c>parseProviderOptions</c> with <c>elevenLabsTranscriptionModelOptionsSchema</c>.</summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong shape or is out of range.</exception>
    internal static void Validate(JsonElement? providerOptions)
    {
        if (
            providerOptions is not { ValueKind: JsonValueKind.Object } bag
            || !bag.TryGetProperty("elevenlabs", out var options)
            || options.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return;
        }

        if (options.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("elevenlabs", "expected an object");
        }

        Text(options, "languageCode", "languageCode");
        Boolean(options, "tagAudioEvents", "tagAudioEvents");
        Number(options, "numSpeakers", "numSpeakers", 1, 32, true);
        OneOf(options, "timestampsGranularity", ["none", "word", "character"]);
        Boolean(options, "diarize", "diarize");
        OneOf(options, "fileFormat", ["pcm_s16le_16", "other"]);
        Text(options, "sourceUrl", "sourceUrl");

        if (!Optional(options, "streaming", out var streaming))
        {
            return;
        }

        if (streaming.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("streaming", "expected an object");
        }

        OneOf(streaming, "commitStrategy", ["manual", "vad"], "streaming.commitStrategy");
        Boolean(streaming, "enableLogging", "streaming.enableLogging");
        Boolean(streaming, "filterBackgroundAudio", "streaming.filterBackgroundAudio");
        Boolean(streaming, "includeLanguageDetection", "streaming.includeLanguageDetection");
        Boolean(streaming, "includeTimestamps", "streaming.includeTimestamps");
        Strings(streaming, "keyterms", "streaming.keyterms", 50, 20);
        Number(streaming, "minSilenceDurationMs", "streaming.minSilenceDurationMs", 50, 2000, true);
        Number(streaming, "minSpeechDurationMs", "streaming.minSpeechDurationMs", 50, 2000, true);
        Boolean(streaming, "noVerbatim", "streaming.noVerbatim");
        Text(streaming, "previousText", "streaming.previousText");
        Strings(streaming, "secondaryLanguages", "streaming.secondaryLanguages", null, null);
        Number(
            streaming,
            "vadSilenceThresholdSecs",
            "streaming.vadSilenceThresholdSecs",
            0.3,
            3,
            false
        );
        Number(streaming, "vadThreshold", "streaming.vadThreshold", 0.1, 0.9, false);
    }

    private static bool Optional(JsonElement parent, string name, out JsonElement value)
    {
        return parent.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;
    }

    private static void Boolean(JsonElement parent, string name, string at)
    {
        if (
            Optional(parent, name, out var value)
            && value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
        )
        {
            throw Invalid(at, "expected a boolean");
        }
    }

    private static void Text(JsonElement parent, string name, string at)
    {
        if (Optional(parent, name, out var value) && value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(at, "expected a string");
        }
    }

    private static void Number(
        JsonElement parent,
        string name,
        string at,
        double min,
        double max,
        bool integer
    )
    {
        if (!Optional(parent, name, out var value))
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw Invalid(at, "expected a number");
        }

        var number = value.GetDouble();
        if (integer && number != System.Math.Floor(number))
        {
            throw Invalid(at, "expected an integer");
        }

        if (number < min || number > max)
        {
            throw Invalid(at, $"expected {min} to {max}");
        }
    }

    private static void OneOf(JsonElement parent, string name, string[] allowed, string? at = null)
    {
        if (
            Optional(parent, name, out var value)
            && (
                value.ValueKind != JsonValueKind.String
                || System.Array.IndexOf(allowed, value.GetString()) < 0
            )
        )
        {
            throw Invalid(at ?? name, "expected one of " + string.Join(", ", allowed));
        }
    }

    private static void Strings(
        JsonElement parent,
        string name,
        string at,
        int? maxItems,
        int? maxLength
    )
    {
        if (!Optional(parent, name, out var value))
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(at, "expected an array");
        }

        if (maxItems != null && value.GetArrayLength() > maxItems)
        {
            throw Invalid(at, $"expected at most {maxItems} item(s)");
        }

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw Invalid(at + "[]", "expected a string");
            }

            if (maxLength != null && item.GetString()!.Length > maxLength)
            {
                throw Invalid(at + "[]", $"expected a string of at most {maxLength} characters");
            }
        }
    }

    private static InvalidArgumentError Invalid(string at, string reason) =>
        new(Parameter, null, "invalid elevenlabs transcription options: " + at + ": " + reason);
}
