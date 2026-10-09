// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>Maps reasoning levels onto provider effort and token budgets.</summary>
public static class ReasoningMap
{
    private static readonly Dictionary<string, double> DefaultBudgetPercentages = new Dictionary<string, double>
    {
        { "minimal", 0.02 },
        { "low", 0.1 },
        { "medium", 0.3 },
        { "high", 0.6 },
        { "xhigh", 0.9 },
        { "max", 0.95 },
    };

    /// <summary>True for every reasoning value except omitted and <c>provider-default</c>. Maps to <c>isCustomReasoning</c>.</summary>
    public static bool IsCustomReasoning(string? reasoning)
    {
        return reasoning != null && reasoning != "provider-default";
    }

    /// <summary>Maps a reasoning level through <paramref name="effortMap"/>. Maps to <c>mapReasoningToProviderEffort</c>.</summary>
    public static string? MapReasoningToProviderEffort(
        string reasoning,
        IReadOnlyDictionary<string, string> effortMap,
        IList<ModelWarning> warnings)
    {
        if (effortMap is null)
        {
            throw new ArgumentNullException(nameof(effortMap));
        }

        if (warnings is null)
        {
            throw new ArgumentNullException(nameof(warnings));
        }

        string? mapped;
        if (!effortMap.TryGetValue(reasoning, out mapped) || mapped is null)
        {
            warnings.Add(new UnsupportedWarning("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(new CompatibilityWarning(
                "reasoning",
                "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    /// <summary>Maps a reasoning level to a token budget. Maps to <c>mapReasoningToProviderBudget</c>.</summary>
    public static int? MapReasoningToProviderBudget(
        string reasoning,
        int maxOutputTokens,
        int maxReasoningBudget,
        IList<ModelWarning> warnings,
        int minReasoningBudget = 1024,
        IReadOnlyDictionary<string, double>? budgetPercentages = null)
    {
        if (warnings is null)
        {
            throw new ArgumentNullException(nameof(warnings));
        }

        var percentages = budgetPercentages ?? DefaultBudgetPercentages;
        double pct;
        if (!percentages.TryGetValue(reasoning, out pct))
        {
            warnings.Add(new UnsupportedWarning("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        var scaled = (int)Math.Round(maxOutputTokens * pct, MidpointRounding.AwayFromZero);
        return Math.Min(maxReasoningBudget, Math.Max(minReasoningBudget, scaled));
    }
}
