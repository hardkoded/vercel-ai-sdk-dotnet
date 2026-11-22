// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Fal;

/// <summary>Provider options for Fal video generation, read from <c>providerOptions.fal</c>. Upstream <c>FalVideoModelOptions</c>.</summary>
public sealed class FalVideoModelOptions
{
    /// <summary>Video loop. Only for Luma models. Sent as <c>loop</c>.</summary>
    public bool? Loop { get; set; }

    /// <summary>Motion strength between 0 and 1. Sent as <c>motion_strength</c>.</summary>
    public double? MotionStrength { get; set; }

    /// <summary>Resolution, such as <c>480p</c>, <c>720p</c> or <c>1080p</c>.</summary>
    public string? Resolution { get; set; }

    /// <summary>Sent as <c>negative_prompt</c>.</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>Sent as <c>prompt_optimizer</c>.</summary>
    public bool? PromptOptimizer { get; set; }

    /// <summary>Every other key, sent to the API as is. Upstream's index signature.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; } =
        new Dictionary<string, JsonElement>();

    /// <summary>Reads <c>providerOptions.fal</c>. Returns null when absent. An invalid value throws <see cref="InvalidArgumentException"/>.</summary>
    internal static FalVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(FalProvider.ProviderId, out var fal)
            || fal.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (fal.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("expected an object.");
        }

        var options = new FalVideoModelOptions();
        var additional = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in fal.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "loop":
                    options.Loop = Bool(value, property.Name);
                    break;
                case "motionStrength":
                    options.MotionStrength = MotionStrengthValue(value);
                    break;
                case "resolution":
                    options.Resolution = Text(value, property.Name);
                    break;
                case "negativePrompt":
                    options.NegativePrompt = Text(value, property.Name);
                    break;
                case "promptOptimizer":
                    options.PromptOptimizer = Bool(value, property.Name);
                    break;
                default:
                    additional[property.Name] = value.Clone();
                    break;
            }
        }

        options.AdditionalProperties = additional;
        return options;
    }

    private static bool? Bool(JsonElement value, string name)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw Invalid(name + " must be a boolean."),
        };
    }

    private static string? Text(JsonElement value, string name)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => value.GetString(),
            _ => throw Invalid(name + " must be a string."),
        };
    }

    private static double? MotionStrengthValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return
            value.ValueKind == JsonValueKind.Number
            && value.GetDouble() is >= 0 and <= 1 and var number
            ? number
            : throw Invalid("motionStrength must be a number between 0 and 1.");
    }

    private static InvalidArgumentException Invalid(string message)
    {
        return new InvalidArgumentException(
            "providerOptions.fal",
            null,
            "Invalid Fal video provider options: " + message
        );
    }
}
