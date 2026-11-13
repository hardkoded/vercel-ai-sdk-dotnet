// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>Maps the shared reasoning option onto Gemini thinking configuration.</summary>
public static class GoogleThinking
{
    private static readonly string[] ReasoningLevels = ["minimal", "low", "medium", "high", "xhigh", "max"];
    private static readonly Regex Hydrogen = new(@"(^|[/.])gemini-4-hydrogen(?:-|\z)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex ModelName = new(@"(?:^|[/.])(gemini-[^/]*)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Gemini3Pro = new(@"^gemini-([0-9]+)(?:\.[0-9]+)?-pro(?:\z|-)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Gemini3Flash = new(@"^gemini-([0-9]+)\.([0-9]+)-flash(?:\z|-(?!lite(?:-|\z)))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Returns a <c>thinkingConfig</c> object, or null when reasoning is unset or <c>provider-default</c>.
    /// Provider options are applied later and override these fields.
    /// </summary>
    public static JsonObject? Resolve(string? reasoning, string modelId, IList<GoogleWarning> warnings)
    {
        if (string.IsNullOrEmpty(reasoning) || reasoning == "provider-default")
        {
            return null;
        }

        var capabilities = GoogleModelCapability.Get(modelId);
        if (capabilities.UsesGemini3Features && modelId.IndexOf("gemini-3-pro-image", StringComparison.Ordinal) < 0)
        {
            var level = Gemini3Level(reasoning!, modelId, warnings);
            return level == null ? null : new JsonObject { ["thinkingLevel"] = level };
        }

        if (reasoning == "none")
        {
            return new JsonObject { ["thinkingBudget"] = 0 };
        }

        var budget = Budget(reasoning!, modelId, warnings);
        return budget == null ? null : new JsonObject { ["thinkingBudget"] = budget };
    }

    private static string? Gemini3Level(string reasoning, string modelId, IList<GoogleWarning> warnings)
    {
        var levels = ThinkingLevels(modelId);
        var index = Array.IndexOf(ReasoningLevels, reasoning);
        if (index < 0 && reasoning != "none")
        {
            warnings.Add(GoogleWarning.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        // Nearest supported level: the level itself, then lower ones, then higher ones. "none" maps to the lowest level.
        var candidates = new List<string>();
        if (index >= 0)
        {
            candidates.Add(reasoning);
            for (var i = index - 1; i >= 0; i--)
            {
                candidates.Add(ReasoningLevels[i]);
            }
        }

        for (var i = index + 1; i < ReasoningLevels.Length; i++)
        {
            candidates.Add(ReasoningLevels[i]);
        }

        var mapped = candidates.FirstOrDefault(levels.Contains);
        if (mapped == null)
        {
            warnings.Add(GoogleWarning.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        if (reasoning != "none" && mapped != reasoning)
        {
            warnings.Add(GoogleWarning.Compatibility("reasoning", "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
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
        if (proMatch.Success && int.TryParse(proMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var proMajor) && proMajor >= 3)
        {
            return "low";
        }

        var match = Gemini3Flash.Match(name);
        if (!match.Success)
        {
            return "minimal";
        }

        if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
        {
            return "minimal";
        }

        return major > 3 || (major == 3 && minor >= 7) ? "low" : "minimal";
    }

    private static int? Budget(string reasoning, string modelId, IList<GoogleWarning> warnings)
    {
        double? percent = reasoning switch
        {
            "minimal" => 0.02,
            "low" => 0.1,
            "medium" => 0.3,
            "high" => 0.6,
            "xhigh" => 0.9,
            "max" => 0.95,
            _ => null,
        };
        if (percent == null)
        {
            warnings.Add(GoogleWarning.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        var maxThinking = modelId.IndexOf("2.5-pro", StringComparison.OrdinalIgnoreCase) >= 0
            || modelId.IndexOf("gemini-3-pro-image", StringComparison.OrdinalIgnoreCase) >= 0
            ? 32768
            : 24576;
        var tokens = (int)Math.Round(65536 * percent.Value);
        if (tokens > maxThinking)
        {
            tokens = maxThinking;
        }

        return tokens < 0 ? 0 : tokens;
    }
}
