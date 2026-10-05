// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Model call options validated by <see cref="LanguageModelCallPreparation"/>.</summary>
public sealed class LanguageModelCallOptionsInput
{
    /// <summary>Maximum output tokens. Must be an integer greater than or equal to 1 when set.</summary>
    public double? MaxOutputTokens { get; set; }

    /// <summary>Sampling temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Nucleus sampling probability.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k cutoff.</summary>
    public double? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Random seed. Must be an integer when set.</summary>
    public double? Seed { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Provider reasoning options.</summary>
    public object? Reasoning { get; set; }
}

/// <summary>Validates language-model call options. <see cref="PrepareCallSettings"/> is the deprecated alias.</summary>
public static class LanguageModelCallPreparation
{
    /// <summary>Deprecated alias of <see cref="PrepareLanguageModelCallOptions"/>.</summary>
    public static LanguageModelCallOptionsInput PrepareCallSettings(LanguageModelCallOptionsInput options)
    {
        return PrepareLanguageModelCallOptions(options);
    }

    /// <summary>Validates <paramref name="options"/> and returns a copy of the normalized values.</summary>
    public static LanguageModelCallOptionsInput PrepareLanguageModelCallOptions(LanguageModelCallOptionsInput options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.MaxOutputTokens != null)
        {
            if (!IsInteger(options.MaxOutputTokens.Value))
            {
                throw new InvalidArgumentError("maxOutputTokens", options.MaxOutputTokens, "maxOutputTokens must be an integer");
            }

            if (options.MaxOutputTokens.Value < 1)
            {
                throw new InvalidArgumentError("maxOutputTokens", options.MaxOutputTokens, "maxOutputTokens must be >= 1");
            }
        }

        RequireNumber("temperature", options.Temperature);
        RequireNumber("topP", options.TopP);
        RequireNumber("topK", options.TopK);
        RequireNumber("presencePenalty", options.PresencePenalty);
        RequireNumber("frequencyPenalty", options.FrequencyPenalty);
        if (options.Seed != null && !IsInteger(options.Seed.Value))
        {
            throw new InvalidArgumentError("seed", options.Seed, "seed must be an integer");
        }

        return new LanguageModelCallOptionsInput
        {
            MaxOutputTokens = options.MaxOutputTokens,
            Temperature = options.Temperature,
            TopP = options.TopP,
            TopK = options.TopK,
            PresencePenalty = options.PresencePenalty,
            FrequencyPenalty = options.FrequencyPenalty,
            StopSequences = options.StopSequences,
            Seed = options.Seed,
            Reasoning = options.Reasoning,
        };
    }

    private static void RequireNumber(string parameter, double? value)
    {
        if (value != null && double.IsNaN(value.Value))
        {
            throw new InvalidArgumentError(parameter, value, parameter + " must be a number");
        }
    }

    private static bool IsInteger(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value);
    }
}
