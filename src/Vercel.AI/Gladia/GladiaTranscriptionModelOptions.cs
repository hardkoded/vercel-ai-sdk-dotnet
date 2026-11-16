// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gladia;

/// <summary>Gladia transcription provider options. Maps to <c>gladia-transcription-model-options.ts</c>.</summary>
internal static class GladiaTranscriptionModelOptions
{
    /// <summary>
    /// Shape of the <c>gladia</c> provider options (camelCase keys). Maps to <c>gladiaTranscriptionModelOptionsSchema</c>.
    /// Source of truth: https://api.gladia.io/openapi.json, InitTranscriptionRequest.
    /// </summary>
    public static Dictionary<string, OptionValidator> GladiaTranscriptionModelOptionsShape()
    {
        var text = ProviderOptionShape.String();
        var flag = ProviderOptionShape.Boolean();
        var number = ProviderOptionShape.Number();
        var strings = ProviderOptionShape.ArrayOf(text);
        var vocabularyEntry = ProviderOptionShape.Union(
            text,
            ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["value"] = text,
                    ["intensity"] = ProviderOptionShape.Nullish(number),
                    ["pronunciations"] = ProviderOptionShape.Nullish(strings),
                    ["language"] = ProviderOptionShape.Nullish(text),
                },
                false,
                "value"));
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["contextPrompt"] = ProviderOptionShape.Nullish(text),
            ["customVocabulary"] = ProviderOptionShape.Nullish(ProviderOptionShape.Union(flag, ProviderOptionShape.ArrayOf(_ => null))),
            ["customVocabularyConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["vocabulary"] = ProviderOptionShape.ArrayOf(vocabularyEntry),
                    ["defaultIntensity"] = ProviderOptionShape.Nullish(number),
                },
                false,
                "vocabulary")),
            ["detectLanguage"] = ProviderOptionShape.Nullish(flag),
            ["enableCodeSwitching"] = ProviderOptionShape.Nullish(flag),
            ["codeSwitchingConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["languages"] = ProviderOptionShape.Nullish(strings),
                })),
            ["language"] = ProviderOptionShape.Nullish(text),
            ["languageConfig"] = ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["languages"] = strings,
                    ["codeSwitching"] = flag,
                }),
            ["callback"] = ProviderOptionShape.Nullish(flag),
            ["callbackConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["url"] = text,
                    ["method"] = ProviderOptionShape.Nullish(ProviderOptionShape.Enum("POST", "PUT")),
                },
                false,
                "url")),
            ["subtitles"] = ProviderOptionShape.Nullish(flag),
            ["subtitlesConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["formats"] = ProviderOptionShape.Nullish(ProviderOptionShape.ArrayOf(ProviderOptionShape.Enum("srt", "vtt"))),
                    ["minimumDuration"] = ProviderOptionShape.Nullish(number),
                    ["maximumDuration"] = ProviderOptionShape.Nullish(number),
                    ["maximumCharactersPerRow"] = ProviderOptionShape.Nullish(number),
                    ["maximumRowsPerCaption"] = ProviderOptionShape.Nullish(number),
                    ["style"] = ProviderOptionShape.Nullish(ProviderOptionShape.Enum("default", "compliance")),
                })),
            ["diarization"] = ProviderOptionShape.Nullish(flag),
            ["diarizationConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["numberOfSpeakers"] = ProviderOptionShape.Nullish(number),
                    ["minSpeakers"] = ProviderOptionShape.Nullish(number),
                    ["maxSpeakers"] = ProviderOptionShape.Nullish(number),
                    ["enhanced"] = ProviderOptionShape.Nullish(flag),
                })),
            ["translation"] = ProviderOptionShape.Nullish(flag),
            ["translationConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["targetLanguages"] = strings,
                    ["model"] = ProviderOptionShape.Nullish(ProviderOptionShape.Enum("base", "batch", "enhanced")),
                    ["matchOriginalUtterances"] = ProviderOptionShape.Nullish(flag),
                    ["lipsync"] = flag,
                    ["contextAdaptation"] = flag,
                    ["context"] = text,
                    ["informal"] = flag,
                },
                false,
                "targetLanguages")),
            ["summarization"] = ProviderOptionShape.Nullish(flag),
            ["summarizationConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["type"] = ProviderOptionShape.Nullish(ProviderOptionShape.Enum("general", "bullet_points", "concise")),
                })),
            ["moderation"] = ProviderOptionShape.Nullish(flag),
            ["namedEntityRecognition"] = ProviderOptionShape.Nullish(flag),
            ["chapterization"] = ProviderOptionShape.Nullish(flag),
            ["nameConsistency"] = ProviderOptionShape.Nullish(flag),
            ["customSpelling"] = ProviderOptionShape.Nullish(flag),
            ["customSpellingConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["spellingDictionary"] = value => value.ValueKind != System.Text.Json.JsonValueKind.Object
                        ? "expected object"
                        : value.EnumerateObject().Select(entry => strings(entry.Value)).FirstOrDefault(error => error != null),
                },
                false,
                "spellingDictionary")),
            ["structuredDataExtraction"] = ProviderOptionShape.Nullish(flag),
            ["structuredDataExtractionConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["classes"] = strings,
                },
                false,
                "classes")),
            ["sentimentAnalysis"] = ProviderOptionShape.Nullish(flag),
            ["audioToLlm"] = ProviderOptionShape.Nullish(flag),
            ["audioToLlmConfig"] = ProviderOptionShape.Nullish(ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["prompts"] = strings,
                    ["model"] = text,
                },
                false,
                "prompts")),
            ["piiRedaction"] = flag,
            ["piiRedactionConfig"] = ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["entityTypes"] = strings,
                    ["processedTextType"] = ProviderOptionShape.Enum("MARKER", "MASK"),
                }),
            ["customMetadata"] = ProviderOptionShape.Nullish(value => value.ValueKind == System.Text.Json.JsonValueKind.Object ? null : "expected object"),
            ["sentences"] = ProviderOptionShape.Nullish(flag),
            ["displayMode"] = ProviderOptionShape.Nullish(flag),
            ["punctuationEnhanced"] = ProviderOptionShape.Nullish(flag),
            ["pollIntervalMs"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number(min: 1, integer: true)),
            ["pollTimeoutMs"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number(min: 1, integer: true)),
        };
    }
}
