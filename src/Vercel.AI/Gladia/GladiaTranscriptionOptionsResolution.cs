// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.Gladia;

/// <summary>Language configuration resolved from the current and the legacy options. Maps to <c>ResolvedLanguageConfig</c>.</summary>
internal sealed class ResolvedLanguageConfig
{
    /// <summary>Languages to transcribe, or null when not set.</summary>
    public string[]? Languages { get; init; }

    /// <summary>Whether to detect the language on each utterance, or null when not set.</summary>
    public bool? CodeSwitching { get; init; }
}

/// <summary>Vocabulary resolved from the current and the legacy options. Maps to the result of <c>resolveCustomVocabulary</c>.</summary>
internal sealed class ResolvedCustomVocabulary
{
    /// <summary>The <c>customVocabulary</c> flag, or null when not set.</summary>
    public bool? CustomVocabulary { get; init; }

    /// <summary>Vocabulary entries taken from a legacy <c>customVocabulary</c> array.</summary>
    public JsonArray? Vocabulary { get; init; }
}

/// <summary>Resolves the legacy Gladia transcription options. Maps to <c>gladia-transcription-options-resolution.ts</c>.</summary>
internal static class GladiaTranscriptionOptionsResolution
{
    /// <summary>Maps to <c>resolveLanguageConfig</c>.</summary>
    public static ResolvedLanguageConfig? ResolveLanguageConfig(JsonElement options)
    {
        if (Present(options, "languageConfig", out var languageConfig))
        {
            return new ResolvedLanguageConfig
            {
                Languages = Present(languageConfig, "languages", out var configured) ? Strings(configured) : null,
                CodeSwitching = Present(languageConfig, "codeSwitching", out var switching) ? switching.GetBoolean() : null,
            };
        }

        bool? codeSwitching = Present(options, "enableCodeSwitching", out var enabled) ? enabled.GetBoolean() : null;
        string[]? languages;
        if (codeSwitching == true
            && Present(options, "codeSwitchingConfig", out var codeSwitchingConfig)
            && Present(codeSwitchingConfig, "languages", out var candidates))
        {
            languages = Strings(candidates);
        }
        else if (Present(options, "language", out var language))
        {
            languages = [language.GetString()!];
        }
        else if (Present(options, "detectLanguage", out var detect) && detect.GetBoolean())
        {
            languages = [];
        }
        else
        {
            languages = null;
        }

        return languages == null && codeSwitching == null
            ? null
            : new ResolvedLanguageConfig { Languages = languages, CodeSwitching = codeSwitching };
    }

    /// <summary>Maps to <c>resolveCustomVocabulary</c>.</summary>
    public static ResolvedCustomVocabulary ResolveCustomVocabulary(JsonElement options, List<ModelWarning>? warnings = null)
    {
        warnings ??= [];
        if (Present(options, "customVocabulary", out var customVocabulary) && customVocabulary.ValueKind == JsonValueKind.Array)
        {
            warnings.Add(new DeprecatedWarning(
                "customVocabulary",
                "Use customVocabulary: true with customVocabularyConfig instead of an array."));
            if (Present(options, "customVocabularyConfig", out var config) && Present(config, "vocabulary", out _))
            {
                warnings.Add(new OtherWarning("customVocabularyConfig.vocabulary overrides the deprecated customVocabulary array."));
                return new ResolvedCustomVocabulary { CustomVocabulary = true };
            }

            var vocabulary = new JsonArray();
            var total = 0;
            foreach (var item in customVocabulary.EnumerateArray())
            {
                total++;
                if (IsValidEntry(item))
                {
                    vocabulary.Add(JsonNode.Parse(item.GetRawText()));
                }
            }

            if (vocabulary.Count != total)
            {
                warnings.Add(new OtherWarning(
                    "Invalid customVocabulary entries were ignored; use strings or objects with a string value and correctly typed optional fields."));
            }

            return vocabulary.Count > 0 ? new ResolvedCustomVocabulary { CustomVocabulary = true, Vocabulary = vocabulary } : new ResolvedCustomVocabulary();
        }

        return new ResolvedCustomVocabulary
        {
            CustomVocabulary = customVocabulary.ValueKind is JsonValueKind.True or JsonValueKind.False ? customVocabulary.GetBoolean() : null,
        };
    }

    private static bool IsValidEntry(JsonElement item)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            return true;
        }

        if (item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty("value", out var value)
            || value.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        if (Present(item, "intensity", out var intensity) && intensity.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (Present(item, "pronunciations", out var pronunciations)
            && (pronunciations.ValueKind != JsonValueKind.Array || pronunciations.EnumerateArray().Any(entry => entry.ValueKind != JsonValueKind.String)))
        {
            return false;
        }

        return !Present(item, "language", out var language) || language.ValueKind == JsonValueKind.String;
    }

    private static string[] Strings(JsonElement array)
    {
        return array.EnumerateArray().Select(entry => entry.GetString()!).ToArray();
    }

    private static bool Present(JsonElement element, string name, out JsonElement value)
    {
        value = default;
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out value)
            && value.ValueKind != JsonValueKind.Null;
    }
}
