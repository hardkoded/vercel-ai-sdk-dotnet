// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Mistral speech options. Maps to <c>mistralSpeechModelOptions</c>.</summary>
public sealed class MistralSpeechModelOptions
{
    /// <summary>Base64-encoded reference audio for one-off voice cloning. It takes precedence over the standard voice option.</summary>
    public string? RefAudio { get; init; }

    /// <summary>How expressive the speech is, from 1 to 5. Supported by Voxtral 3.</summary>
    public double? Expressiveness { get; init; }

    /// <summary>Pronunciation hints mapping words to lists of IPA phonemes. Supported by Voxtral 3.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? PhonemeHints { get; init; }

    /// <summary>Parses <c>providerOptions.mistral</c>. Maps to <c>parseProviderOptions</c> with the speech schema.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or is out of range.</exception>
    public static MistralSpeechModelOptions? Parse(JsonElement providerOptions)
    {
        if (MistralOptionValues.Section(providerOptions) is not { } mistral)
        {
            return null;
        }

        string? refAudio = null;
        if (MistralOptionValues.Get(mistral, "refAudio") is { } refAudioValue)
        {
            refAudio = MistralOptionValues.NonEmptyString(refAudioValue, "refAudio");
        }

        double? expressiveness = null;
        if (MistralOptionValues.Get(mistral, "expressiveness") is { } expressivenessValue)
        {
            if (
                expressivenessValue.ValueKind != JsonValueKind.Number
                || expressivenessValue.GetDouble() < 1
                || expressivenessValue.GetDouble() > 5
            )
            {
                throw MistralOptionValues.Invalid(
                    expressivenessValue,
                    "expressiveness must be a number from 1 to 5"
                );
            }

            expressiveness = expressivenessValue.GetDouble();
        }

        Dictionary<string, IReadOnlyList<string>>? phonemeHints = null;
        if (MistralOptionValues.Get(mistral, "phonemeHints") is { } hintsValue)
        {
            if (hintsValue.ValueKind != JsonValueKind.Object)
            {
                throw MistralOptionValues.Invalid(hintsValue, "phonemeHints must be an object");
            }

            phonemeHints = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            foreach (var hint in hintsValue.EnumerateObject())
            {
                if (hint.Name.Length == 0)
                {
                    throw MistralOptionValues.Invalid(
                        hintsValue,
                        "phonemeHints keys must not be empty"
                    );
                }

                if (hint.Value.ValueKind != JsonValueKind.Array || hint.Value.GetArrayLength() == 0)
                {
                    throw MistralOptionValues.Invalid(
                        hint.Value,
                        "phonemeHints values must be non-empty arrays"
                    );
                }

                var phonemes = new List<string>();
                foreach (var phoneme in hint.Value.EnumerateArray())
                {
                    phonemes.Add(MistralOptionValues.NonEmptyString(phoneme, "phonemeHints"));
                }

                phonemeHints[hint.Name] = phonemes;
            }
        }

        return new MistralSpeechModelOptions
        {
            RefAudio = refAudio,
            Expressiveness = expressiveness,
            PhonemeHints = phonemeHints,
        };
    }
}

/// <summary>Helpers that read and validate the <c>mistral</c> section of provider options.</summary>
internal static class MistralOptionValues
{
    public static JsonElement? Section(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind == JsonValueKind.Object
            && providerOptions.TryGetProperty(MistralProvider.ProviderId, out var mistral)
            && mistral.ValueKind == JsonValueKind.Object
        )
        {
            return mistral;
        }

        return null;
    }

    public static JsonElement? Get(JsonElement section, string name)
    {
        return
            section.TryGetProperty(name, out var value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            ? value
            : null;
    }

    public static string NonEmptyString(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.String || value.GetString()!.Length == 0)
        {
            throw Invalid(value, name + " must be a non-empty string");
        }

        return value.GetString()!;
    }

    public static InvalidArgumentException Invalid(JsonElement value, string reason)
    {
        return new InvalidArgumentException(
            "providerOptions",
            null,
            "invalid mistral provider options: " + reason
        );
    }
}
