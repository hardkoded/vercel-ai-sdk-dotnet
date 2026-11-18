// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Mistral;

/// <summary>Mistral transcription turn detection settings. Maps to the <c>turnDetection</c> option.</summary>
public sealed class MistralTurnDetection
{
    /// <summary>Upstream <c>startThreshold</c>.</summary>
    public double? StartThreshold { get; init; }

    /// <summary>Upstream <c>eagerEndThreshold</c>.</summary>
    public double? EagerEndThreshold { get; init; }

    /// <summary>Upstream <c>endThreshold</c>.</summary>
    public double? EndThreshold { get; init; }

    /// <summary>Upstream <c>minSilenceMs</c>.</summary>
    public int? MinSilenceMs { get; init; }

    /// <summary>Upstream <c>endTimeoutMs</c>.</summary>
    public int? EndTimeoutMs { get; init; }
}

/// <summary>Mistral transcription options. Maps to <c>mistralTranscriptionModelOptions</c>.</summary>
public sealed class MistralTranscriptionModelOptions
{
    /// <summary>The language of the audio, such as <c>en</c>.</summary>
    public string? Language { get; init; }

    /// <summary>Return the detected language. Cannot be combined with <see cref="Language"/>.</summary>
    public bool? ReturnLanguage { get; init; }

    /// <summary>The sampling temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>The timestamp granularities to include in the response, <c>segment</c> and <c>word</c>.</summary>
    public IReadOnlyList<string>? TimestampGranularities { get; init; }

    /// <summary>Whether to identify speakers in the transcription.</summary>
    public bool? Diarize { get; init; }

    /// <summary>Words or phrases that guide the model toward correct spellings.</summary>
    public IReadOnlyList<string>? ContextBias { get; init; }

    /// <summary>Realtime only. Delay in milliseconds: 80–1200, or 2400.</summary>
    public int? TargetStreamingDelayMs { get; init; }

    /// <summary>Realtime only. Named base64-encoded audio clips for known speakers.</summary>
    public IReadOnlyDictionary<string, string>? SpeakerReferences { get; init; }

    /// <summary>Realtime only. Turn detection settings. An empty object selects the defaults.</summary>
    public MistralTurnDetection? TurnDetection { get; init; }

    /// <summary>Parses <c>providerOptions.mistral</c>. Maps to <c>parseProviderOptions</c> with the transcription schema.</summary>
    /// <exception cref="Vercel.AI.Operations.InvalidArgumentException">An option has the wrong shape or is out of range.</exception>
    public static MistralTranscriptionModelOptions? Parse(JsonElement providerOptions)
    {
        if (MistralOptionValues.Section(providerOptions) is not { } mistral)
        {
            return null;
        }

        string? language = null;
        if (MistralOptionValues.Get(mistral, "language") is { } languageValue)
        {
            language = MistralOptionValues.NonEmptyString(languageValue, "language");
        }

        bool? returnLanguage = Boolean(mistral, "returnLanguage");
        bool? diarize = Boolean(mistral, "diarize");
        double? temperature = null;
        if (MistralOptionValues.Get(mistral, "temperature") is { } temperatureValue)
        {
            if (temperatureValue.ValueKind != JsonValueKind.Number)
            {
                throw MistralOptionValues.Invalid(temperatureValue, "temperature must be a number");
            }

            temperature = temperatureValue.GetDouble();
        }

        List<string>? granularities = null;
        if (MistralOptionValues.Get(mistral, "timestampGranularities") is { } granularitiesValue)
        {
            if (
                granularitiesValue.ValueKind != JsonValueKind.Array
                || granularitiesValue.GetArrayLength() == 0
            )
            {
                throw MistralOptionValues.Invalid(
                    granularitiesValue,
                    "timestampGranularities must be a non-empty array"
                );
            }

            granularities = [];
            foreach (var item in granularitiesValue.EnumerateArray())
            {
                if (
                    item.ValueKind != JsonValueKind.String
                    || (item.GetString() != "segment" && item.GetString() != "word")
                )
                {
                    throw MistralOptionValues.Invalid(
                        item,
                        "timestampGranularities entries must be segment or word"
                    );
                }

                granularities.Add(item.GetString()!);
            }
        }

        List<string>? contextBias = null;
        if (MistralOptionValues.Get(mistral, "contextBias") is { } biasValue)
        {
            if (biasValue.ValueKind != JsonValueKind.Array)
            {
                throw MistralOptionValues.Invalid(biasValue, "contextBias must be an array");
            }

            contextBias = [];
            foreach (var item in biasValue.EnumerateArray())
            {
                contextBias.Add(MistralOptionValues.NonEmptyString(item, "contextBias"));
            }
        }

        int? delay = null;
        if (MistralOptionValues.Get(mistral, "targetStreamingDelayMs") is { } delayValue)
        {
            if (
                delayValue.ValueKind != JsonValueKind.Number
                || !delayValue.TryGetInt32(out var delayMs)
                || (delayMs != 2400 && (delayMs < 80 || delayMs > 1200))
            )
            {
                throw MistralOptionValues.Invalid(
                    delayValue,
                    "targetStreamingDelayMs must be an integer from 80 to 1200, or 2400"
                );
            }

            delay = delayMs;
        }

        Dictionary<string, string>? references = null;
        if (MistralOptionValues.Get(mistral, "speakerReferences") is { } referencesValue)
        {
            if (referencesValue.ValueKind != JsonValueKind.Object)
            {
                throw MistralOptionValues.Invalid(
                    referencesValue,
                    "speakerReferences must be an object"
                );
            }

            references = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var reference in referencesValue.EnumerateObject())
            {
                references[reference.Name] = MistralOptionValues.NonEmptyString(
                    reference.Value,
                    "speakerReferences"
                );
            }
        }

        MistralTurnDetection? turnDetection = null;
        if (MistralOptionValues.Get(mistral, "turnDetection") is { } turnValue)
        {
            if (turnValue.ValueKind != JsonValueKind.Object)
            {
                throw MistralOptionValues.Invalid(turnValue, "turnDetection must be an object");
            }

            turnDetection = new MistralTurnDetection
            {
                StartThreshold = Unit(turnValue, "startThreshold"),
                EagerEndThreshold = Unit(turnValue, "eagerEndThreshold"),
                EndThreshold = Unit(turnValue, "endThreshold"),
                MinSilenceMs = NonNegativeInt(turnValue, "minSilenceMs"),
                EndTimeoutMs = NonNegativeInt(turnValue, "endTimeoutMs"),
            };
        }

        return new MistralTranscriptionModelOptions
        {
            Language = language,
            ReturnLanguage = returnLanguage,
            Temperature = temperature,
            TimestampGranularities = granularities,
            Diarize = diarize,
            ContextBias = contextBias,
            TargetStreamingDelayMs = delay,
            SpeakerReferences = references,
            TurnDetection = turnDetection,
        };
    }

    private static bool? Boolean(JsonElement section, string name)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw MistralOptionValues.Invalid(value, name + " must be a boolean");
        }

        return value.GetBoolean();
    }

    private static double? Unit(JsonElement section, string name)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || value.GetDouble() < 0
            || value.GetDouble() > 1
        )
        {
            throw MistralOptionValues.Invalid(value, name + " must be a number from 0 to 1");
        }

        return value.GetDouble();
    }

    private static int? NonNegativeInt(JsonElement section, string name)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetInt32(out var number)
            || number < 0
        )
        {
            throw MistralOptionValues.Invalid(value, name + " must be a non-negative integer");
        }

        return number;
    }
}
