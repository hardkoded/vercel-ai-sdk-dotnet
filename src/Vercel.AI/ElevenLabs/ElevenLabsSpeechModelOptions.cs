// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.ElevenLabs;

/// <summary>
/// Validation of <c>providerOptions.elevenlabs</c> for speech models. Maps to <c>elevenLabsSpeechModelOptionsSchema</c>
/// and <c>compositionPlanSchema</c> of <c>elevenlabs-speech-model-options.ts</c>. Unknown keys are ignored, like zod.
/// </summary>
internal static class ElevenLabsSpeechModelOptions
{
    private const string Parameter = "providerOptions.elevenlabs";

    private static readonly string[] Levels = ["low", "medium", "high"];

    private static readonly string[] ConditionStrengths = ["low", "medium", "high", "xhigh"];

    private static readonly string[] TextNormalizations = ["auto", "on", "off"];

    /// <summary>Validates the <c>elevenlabs</c> entry of the provider options. Maps to <c>parseProviderOptions</c> with <c>elevenLabsSpeechModelOptionsSchema</c>.</summary>
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

        Boolean(options, "withTimestamps");
        Text(options, "languageCode", 0);
        if (Optional(options, "voiceSettings", out var voiceSettings))
        {
            Object(voiceSettings, "voiceSettings");
            Number(voiceSettings, "stability", "voiceSettings.stability", 0, 1);
            Number(voiceSettings, "similarityBoost", "voiceSettings.similarityBoost", 0, 1);
            Number(voiceSettings, "style", "voiceSettings.style", 0, 1);
            Boolean(voiceSettings, "useSpeakerBoost", "voiceSettings.useSpeakerBoost");
        }

        if (Optional(options, "pronunciationDictionaryLocators", out var locators))
        {
            foreach (var locator in Array(locators, "pronunciationDictionaryLocators", 0, 3))
            {
                Object(locator, "pronunciationDictionaryLocators[]");
                Required(locator, "pronunciationDictionaryId", "pronunciationDictionaryLocators[]");
                Text(locator, "pronunciationDictionaryId", 0, "pronunciationDictionaryLocators[]");
                Text(locator, "versionId", 0, "pronunciationDictionaryLocators[]");
            }
        }

        Number(options, "seed", "seed", 0, 4294967295);
        Text(options, "previousText", 0);
        Text(options, "nextText", 0);
        Strings(options, "previousRequestIds", 3);
        Strings(options, "nextRequestIds", 3);
        if (Optional(options, "applyTextNormalization", out var normalization))
        {
            OneOf(normalization, "applyTextNormalization", TextNormalizations);
        }

        Boolean(options, "applyLanguageTextNormalization");
        Boolean(options, "enableLogging");
        ValidateStreaming(options);
        ValidateDialogue(options);
        ValidateInputs(options);
        Integer(options, "musicLengthMs", "musicLengthMs", 3000, 600000);
        Boolean(options, "forceInstrumental");
        if (Optional(options, "compositionPlan", out var plan))
        {
            ValidateCompositionPlan(plan);
        }

        Number(options, "durationSeconds", "durationSeconds", 0.5, 30);
        Number(options, "promptInfluence", "promptInfluence", 0, 1);
        Boolean(options, "loop");
    }

    private static void ValidateStreaming(JsonElement options)
    {
        if (!Optional(options, "streaming", out var streaming))
        {
            return;
        }

        Object(streaming, "streaming");
        Boolean(streaming, "autoMode", "streaming.autoMode");
        if (Optional(streaming, "chunkLengthSchedule", out var schedule))
        {
            foreach (var item in Array(schedule, "streaming.chunkLengthSchedule", 1, null))
            {
                InRange(item, "streaming.chunkLengthSchedule[]", 50, 500, false);
            }
        }

        Boolean(streaming, "enableSsmlParsing", "streaming.enableSsmlParsing");
        Integer(streaming, "inactivityTimeout", "streaming.inactivityTimeout", 1, 180);
        Text(streaming, "outputFormat", 1, "streaming");
        Boolean(streaming, "syncAlignment", "streaming.syncAlignment");
    }

    private static void ValidateDialogue(JsonElement options)
    {
        if (!Optional(options, "dialogue", out var dialogue))
        {
            return;
        }

        Object(dialogue, "dialogue");
        Required(dialogue, "voices", "dialogue");
        foreach (var voice in Array(dialogue.GetProperty("voices"), "dialogue.voices", 1, 10))
        {
            if (voice.ValueKind != JsonValueKind.String || voice.GetString()!.Length == 0)
            {
                throw Invalid("dialogue.voices[]", "expected a non-empty string");
            }
        }

        if (Optional(dialogue, "voiceSettings", out var voiceSettings))
        {
            Object(voiceSettings, "dialogue.voiceSettings");
            Number(voiceSettings, "stability", "dialogue.voiceSettings.stability", 0, 1);
        }
    }

    private static void ValidateInputs(JsonElement options)
    {
        if (!Optional(options, "inputs", out var inputs))
        {
            return;
        }

        foreach (var input in Array(inputs, "inputs", 1, null))
        {
            Object(input, "inputs[]");
            Required(input, "text", "inputs[]");
            Required(input, "voiceId", "inputs[]");
            Text(input, "text", 1, "inputs[]");
            Text(input, "voiceId", 1, "inputs[]");
        }
    }

    /// <summary>Maps to <c>compositionPlanSchema</c>: a <c>music_v1</c> prompt plan or a chunked plan.</summary>
    private static void ValidateCompositionPlan(JsonElement plan)
    {
        Object(plan, "compositionPlan");
        if (plan.TryGetProperty("sections", out _) || !plan.TryGetProperty("chunks", out _))
        {
            ValidateMusicPrompt(plan);
        }
        else
        {
            ValidateChunkedPlan(plan);
        }
    }

    private static void ValidateMusicPrompt(JsonElement plan)
    {
        Styles(plan, "positive_global_styles", true);
        Styles(plan, "negative_global_styles", true);
        Required(plan, "sections", "compositionPlan");
        foreach (
            var section in Array(plan.GetProperty("sections"), "compositionPlan.sections", 0, null)
        )
        {
            const string at = "compositionPlan.sections[]";
            Object(section, at);
            Required(section, "section_name", at);
            Text(section, "section_name", 1, at, 100);
            Styles(section, "positive_local_styles", true);
            Styles(section, "negative_local_styles", true);
            Required(section, "duration_ms", at);
            Integer(section, "duration_ms", at + ".duration_ms", 3000, 120000);
            Required(section, "lines", at);
            var lines = Array(section.GetProperty("lines"), at + ".lines", 0, 30);
            foreach (var line in lines)
            {
                if (line.ValueKind != JsonValueKind.String || line.GetString()!.Length > 200)
                {
                    throw Invalid(at + ".lines[]", "expected a string of at most 200 characters");
                }
            }

            if (Optional(section, "source_from", out var source))
            {
                Object(source, at + ".source_from");
                Required(source, "song_id", at + ".source_from");
                Text(source, "song_id", 0, at + ".source_from");
                Required(source, "range", at + ".source_from");
                TimeRange(source.GetProperty("range"), at + ".source_from.range");
                if (Optional(source, "negative_ranges", out var negatives))
                {
                    foreach (
                        var range in Array(negatives, at + ".source_from.negative_ranges", 0, null)
                    )
                    {
                        TimeRange(range, at + ".source_from.negative_ranges[]");
                    }
                }
            }
        }
    }

    private static void ValidateChunkedPlan(JsonElement plan)
    {
        foreach (var chunk in Array(plan.GetProperty("chunks"), "compositionPlan.chunks", 0, null))
        {
            const string at = "compositionPlan.chunks[]";
            Object(chunk, at);

            // z.union([generationChunk, audioRefChunk]): a chunk is one or the other.
            if (chunk.TryGetProperty("song_id", out _) && !chunk.TryGetProperty("text", out _))
            {
                AudioRefChunk(chunk, at);
                continue;
            }

            Required(chunk, "text", at);
            Text(chunk, "text", 0, at);
            Required(chunk, "duration_ms", at);
            Integer(chunk, "duration_ms", at + ".duration_ms", 3000, 120000);
            Styles(chunk, "positive_styles", true);
            Styles(chunk, "negative_styles", false);
            if (Optional(chunk, "context_adherence", out var adherence))
            {
                OneOf(adherence, at + ".context_adherence", Levels);
            }

            if (Optional(chunk, "conditioning_ref", out var reference))
            {
                AudioRefChunk(reference, at + ".conditioning_ref");
            }

            if (Optional(chunk, "condition_strength", out var strength))
            {
                OneOf(strength, at + ".condition_strength", ConditionStrengths);
            }
        }
    }

    private static void AudioRefChunk(JsonElement chunk, string at)
    {
        Object(chunk, at);
        Required(chunk, "song_id", at);
        Text(chunk, "song_id", 0, at);
        Required(chunk, "range", at);
        TimeRange(chunk.GetProperty("range"), at + ".range");
    }

    private static void TimeRange(JsonElement range, string at)
    {
        Object(range, at);
        Required(range, "start_ms", at);
        Required(range, "end_ms", at);
        Integer(range, "start_ms", at + ".start_ms", 0, null);
        Integer(range, "end_ms", at + ".end_ms", 0, null);
    }

    private static void Styles(JsonElement parent, string name, bool required)
    {
        if (required)
        {
            Required(parent, name, "compositionPlan");
        }

        if (!Optional(parent, name, out var styles))
        {
            return;
        }

        foreach (var style in Array(styles, name, 0, null))
        {
            if (style.ValueKind != JsonValueKind.String)
            {
                throw Invalid(name + "[]", "expected a string");
            }
        }
    }

    private static void Strings(JsonElement parent, string name, int max)
    {
        if (!Optional(parent, name, out var value))
        {
            return;
        }

        foreach (var item in Array(value, name, 0, max))
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw Invalid(name + "[]", "expected a string");
            }
        }
    }

    private static bool Optional(JsonElement parent, string name, out JsonElement value)
    {
        if (!parent.TryGetProperty(name, out value) || value.ValueKind == JsonValueKind.Undefined)
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            throw Invalid(name, "expected a value, received null");
        }

        return true;
    }

    private static void Required(JsonElement parent, string name, string at)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            throw Invalid(at + "." + name, "required");
        }
    }

    private static void Object(JsonElement value, string at)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(at, "expected an object");
        }
    }

    private static IEnumerable<JsonElement> Array(JsonElement value, string at, int min, int? max)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(at, "expected an array");
        }

        var length = value.GetArrayLength();
        if (length < min || (max != null && length > max))
        {
            throw Invalid(
                at,
                max == null
                    ? $"expected at least {min} item(s)"
                    : $"expected {min} to {max} item(s)"
            );
        }

        return value.EnumerateArray();
    }

    private static void Boolean(JsonElement parent, string name, string? at = null)
    {
        if (
            Optional(parent, name, out var value)
            && value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
        )
        {
            throw Invalid(at ?? name, "expected a boolean");
        }
    }

    private static void Text(
        JsonElement parent,
        string name,
        int minLength,
        string? at = null,
        int? maxLength = null
    )
    {
        if (!Optional(parent, name, out var value))
        {
            return;
        }

        var path = at == null ? name : at + "." + name;
        if (value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(path, "expected a string");
        }

        var length = value.GetString()!.Length;
        if (length < minLength || (maxLength != null && length > maxLength))
        {
            throw Invalid(path, "string length out of range");
        }
    }

    private static void Number(JsonElement parent, string name, string at, double min, double max)
    {
        if (Optional(parent, name, out var value))
        {
            InRange(value, at, min, max, false);
        }
    }

    private static void Integer(JsonElement parent, string name, string at, double min, double? max)
    {
        if (Optional(parent, name, out var value))
        {
            InRange(value, at, min, max, true);
        }
    }

    private static void InRange(JsonElement value, string at, double min, double? max, bool integer)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw Invalid(at, "expected a number");
        }

        var number = value.GetDouble();
        if (integer && number != System.Math.Floor(number))
        {
            throw Invalid(at, "expected an integer");
        }

        if (number < min || (max != null && number > max))
        {
            throw Invalid(
                at,
                max == null ? $"expected at least {min}" : $"expected {min} to {max}"
            );
        }
    }

    private static void OneOf(JsonElement value, string at, string[] allowed)
    {
        if (
            value.ValueKind != JsonValueKind.String
            || System.Array.IndexOf(allowed, value.GetString()) < 0
        )
        {
            throw Invalid(at, "expected one of " + string.Join(", ", allowed));
        }
    }

    private static InvalidArgumentError Invalid(string at, string reason) =>
        new(Parameter, null, "invalid elevenlabs speech options: " + at + ": " + reason);
}
