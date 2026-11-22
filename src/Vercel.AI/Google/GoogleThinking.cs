// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Google;

/// <summary>Maps the shared reasoning option onto Gemini thinking configuration.</summary>
public static class GoogleThinking
{
    private static readonly Regex Hydrogen = new(
        @"(^|[/.])gemini-4-(?:hydrogen|neon)(?:-|\z)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );
    private static readonly Regex ModelName = new(
        @"(?:^|[/.])(gemini-[^/]*)\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );
    private static readonly Regex Gemini3Pro = new(
        @"^gemini-([0-9]+)(?:\.[0-9]+)?-pro(?:\z|-)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );
    private static readonly Regex Gemini3Flash = new(
        @"^gemini-([0-9]+)\.([0-9]+)-flash(?:\z|-(?!lite(?:-|\z)))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    /// <summary>
    /// Returns a <c>thinkingConfig</c> object, or null when reasoning is unset or <c>provider-default</c>.
    /// Provider options are applied later and override these fields.
    /// </summary>
    public static JsonObject? Resolve(
        string? reasoning,
        string modelId,
        IList<GoogleWarning> warnings
    )
    {
        if (string.IsNullOrEmpty(reasoning) || reasoning == "provider-default")
        {
            return null;
        }

        var capabilities = GoogleModelCapability.Get(modelId);
        if (
            capabilities.UsesGemini3Features
            && modelId.IndexOf("gemini-3-pro-image", StringComparison.Ordinal) < 0
        )
        {
            var level = Gemini3Level(reasoning!, modelId, warnings);
            return level == null ? null : new JsonObject { ["thinkingLevel"] = level };
        }

        return new JsonObject { ["thinkingBudget"] = Budget(reasoning!, modelId, warnings) };
    }

    private static void AddWarnings(
        IList<GoogleWarning> warnings,
        IEnumerable<ModelWarning> modelWarnings
    )
    {
        foreach (var warning in modelWarnings)
        {
            switch (warning)
            {
                case UnsupportedWarning unsupported:
                    warnings.Add(
                        GoogleWarning.Unsupported(unsupported.Feature, unsupported.Details)
                    );
                    break;
                case CompatibilityWarning compatibility:
                    warnings.Add(
                        GoogleWarning.Compatibility(
                            compatibility.Feature,
                            compatibility.Details ?? string.Empty
                        )
                    );
                    break;
            }
        }
    }

    private static string? Gemini3Level(
        string reasoning,
        string modelId,
        IList<GoogleWarning> warnings
    )
    {
        // Gemini 3 thinking cannot be fully disabled, so reasoning "none" maps to the lowest supported level.
        var modelWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(
            reasoning,
            ThinkingLevels(modelId).ToDictionary(level => level, level => level),
            modelWarnings
        );
        AddWarnings(warnings, modelWarnings);
        return effort;
    }

    /// <summary>The reasoning efforts of the model catalog entry. Empty for budget-based models.</summary>
    internal static IReadOnlyList<string> ReasoningEfforts(string modelId)
    {
        return
            GoogleModelCapability.Get(modelId).UsesGemini3Features
            && modelId.IndexOf("gemini-3-pro-image", StringComparison.Ordinal) < 0
            ? ThinkingLevels(modelId)
            : [];
    }

    // Maps to the reasoning efforts of the model catalog entry.
    private static List<string> ThinkingLevels(string modelId)
    {
        var minimum = MinimumLevel(modelId);
        var levels = new List<string>();
        if (minimum == "minimal")
        {
            levels.Add("minimal");
        }

        levels.Add("low");
        levels.Add("medium");
        levels.Add("high");
        if (IsHydrogen(modelId))
        {
            levels.Add("xhigh");
        }

        return levels;
    }

    private static bool IsHydrogen(string modelId)
    {
        return Hydrogen.IsMatch(modelId ?? string.Empty);
    }

    private static string MinimumLevel(string modelId)
    {
        if (IsHydrogen(modelId))
        {
            return "low";
        }

        var nameMatch = ModelName.Match(modelId ?? string.Empty);
        var name = nameMatch.Success ? nameMatch.Groups[1].Value : modelId ?? string.Empty;

        if (name.Equals("gemini-flash-latest", StringComparison.OrdinalIgnoreCase))
        {
            return "low";
        }

        var proMatch = Gemini3Pro.Match(name);
        if (
            proMatch.Success
            && int.TryParse(
                proMatch.Groups[1].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var proMajor
            )
            && proMajor >= 3
        )
        {
            return "low";
        }

        var match = Gemini3Flash.Match(name);
        if (!match.Success)
        {
            return "minimal";
        }

        if (
            !int.TryParse(
                match.Groups[1].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var major
            )
            || !int.TryParse(
                match.Groups[2].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var minor
            )
        )
        {
            return "minimal";
        }

        return major > 3 || (major == 3 && minor >= 7) ? "low" : "minimal";
    }

    private static int Budget(string reasoning, string modelId, IList<GoogleWarning> warnings)
    {
        // Gemini 2.5 Pro cannot disable thinking and requires a thinking budget of at least 128 tokens. Other
        // Gemini 2.5 models can disable thinking with a budget of 0.
        var minThinking =
            modelId.IndexOf("2.5-pro", StringComparison.OrdinalIgnoreCase) >= 0 ? 128 : 0;
        if (reasoning == "none" && minThinking == 0)
        {
            return 0;
        }

        var maxThinking =
            modelId.IndexOf("2.5-pro", StringComparison.OrdinalIgnoreCase) >= 0
            || modelId.IndexOf("gemini-3-pro-image", StringComparison.OrdinalIgnoreCase) >= 0
                ? 32768
                : 24576;
        var modelWarnings = new List<ModelWarning>();
        var budget = ReasoningMap.MapReasoningToProviderBudget(
            reasoning,
            65536,
            maxThinking,
            modelWarnings,
            minThinking
        );
        AddWarnings(warnings, modelWarnings);
        return budget;
    }
}
