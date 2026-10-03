// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>Maps the shared reasoning option onto Gemini thinking configuration.</summary>
public static class GoogleThinking
{
    private static readonly Regex Gemini3Flash = new(@"^gemini-(\d+)\.(\d+)-flash(?:$|-(?!lite(?:-|$)))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

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
        var minimum = MinimumLevel(modelId);
        if (reasoning == "none")
        {
            return minimum;
        }

        string? mapped = reasoning switch
        {
            "minimal" => minimum,
            "low" => "low",
            "medium" => "medium",
            "high" => "high",
            "xhigh" => "high",
            _ => null,
        };
        if (mapped == null)
        {
            warnings.Add(GoogleWarning.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(GoogleWarning.Compatibility("reasoning", "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    private static string MinimumLevel(string modelId)
    {
        var name = modelId ?? string.Empty;
        var slash = name.LastIndexOf('/');
        if (slash >= 0)
        {
            name = name.Substring(slash + 1);
        }

        if (name.Equals("gemini-flash-latest", StringComparison.OrdinalIgnoreCase))
        {
            return "low";
        }

        var match = Gemini3Flash.Match(name);
        if (!match.Success)
        {
            return "minimal";
        }

        var major = int.Parse(match.Groups[1].Value);
        var minor = int.Parse(match.Groups[2].Value);
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
