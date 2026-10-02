// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>Settings passed to <see cref="CallSettings.PrepareLanguageModelCallOptions"/>.</summary>
public sealed class LanguageModelCallSettings
{
    /// <summary>Maximum output tokens. Non-integers are rejected.</summary>
    public double? MaxOutputTokens { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-p nucleus sampling.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k sampling.</summary>
    public double? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Random seed. Non-integers are rejected.</summary>
    public double? Seed { get; set; }

    /// <summary>Reasoning effort, when the provider supports it.</summary>
    public string? Reasoning { get; set; }
}

/// <summary>Validates language-model call settings. Maps to <c>prepareLanguageModelCallOptions</c>.</summary>
public static class CallSettings
{
    /// <summary>Returns a copy of <paramref name="settings"/> after validating it.</summary>
    public static LanguageModelCallSettings PrepareLanguageModelCallOptions(LanguageModelCallSettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (settings.MaxOutputTokens is { } maxOutputTokens)
        {
            if (!IsInteger(maxOutputTokens))
            {
                throw new Error.InvalidArgumentException("maxOutputTokens", maxOutputTokens, "maxOutputTokens must be an integer");
            }

            if (maxOutputTokens < 1)
            {
                throw new Error.InvalidArgumentException("maxOutputTokens", maxOutputTokens, "maxOutputTokens must be >= 1");
            }
        }

        RequireNumber(settings.Temperature, "temperature");
        RequireNumber(settings.TopP, "topP");
        RequireNumber(settings.TopK, "topK");
        RequireNumber(settings.PresencePenalty, "presencePenalty");
        RequireNumber(settings.FrequencyPenalty, "frequencyPenalty");
        if (settings.Seed is { } seed && !IsInteger(seed))
        {
            throw new Error.InvalidArgumentException("seed", seed, "seed must be an integer");
        }

        return Copy(settings);
    }

    /// <summary>Deprecated alias of <see cref="PrepareLanguageModelCallOptions"/>.</summary>
    public static LanguageModelCallSettings PrepareCallSettings(LanguageModelCallSettings settings)
    {
        return PrepareLanguageModelCallOptions(settings);
    }

    private static void RequireNumber(double? value, string parameter)
    {
        if (value is { } number && (double.IsNaN(number) || double.IsInfinity(number)))
        {
            throw new Error.InvalidArgumentException(parameter, number, parameter + " must be a number");
        }
    }

    private static bool IsInteger(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value);
    }

    private static LanguageModelCallSettings Copy(LanguageModelCallSettings settings)
    {
        return new LanguageModelCallSettings
        {
            MaxOutputTokens = settings.MaxOutputTokens,
            Temperature = settings.Temperature,
            TopP = settings.TopP,
            TopK = settings.TopK,
            PresencePenalty = settings.PresencePenalty,
            FrequencyPenalty = settings.FrequencyPenalty,
            StopSequences = settings.StopSequences,
            Seed = settings.Seed,
            Reasoning = settings.Reasoning,
        };
    }
}
