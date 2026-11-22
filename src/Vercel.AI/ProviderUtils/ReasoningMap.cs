// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>Maps reasoning levels onto provider effort and token budgets.</summary>
public static class ReasoningMap
{
    private static readonly Dictionary<string, double> DefaultBudgetPercentages = new Dictionary<
        string,
        double
    >
    {
        { "minimal", 0.02 },
        { "low", 0.1 },
        { "medium", 0.3 },
        { "high", 0.6 },
        { "xhigh", 0.9 },
        { "max", 0.95 },
    };

    private static readonly string[] ReasoningLevels =
    [
        "minimal",
        "low",
        "medium",
        "high",
        "xhigh",
        "max",
    ];

    /// <summary>Converts a warning produced while mapping reasoning into a call warning that keeps its feature.</summary>
    internal static CallWarning? ToCallWarning(ModelWarning warning)
    {
        return warning switch
        {
            UnsupportedWarning unsupported => new CallWarning(
                unsupported.Type,
                unsupported.Feature,
                unsupported.Details
            ),
            CompatibilityWarning compatibility => new CallWarning(
                compatibility.Type,
                compatibility.Feature,
                compatibility.Details
            ),
            _ => null,
        };
    }

    /// <summary>True for every reasoning value except omitted and <c>provider-default</c>. Maps to <c>isCustomReasoning</c>.</summary>
    public static bool IsCustomReasoning(string? reasoning)
    {
        return reasoning != null && reasoning != "provider-default";
    }

    /// <summary>
    /// Maps a reasoning value to a provider effort string. A <paramref name="providerOptionsEffort"/> is returned
    /// unchanged. Otherwise an omitted or <c>provider-default</c> reasoning returns <c>null</c>. An unsupported value maps
    /// to the next lower supported level, or the next higher one if there is no lower level. <c>none</c> is never a
    /// fallback, so an unsupported <c>none</c> maps to the lowest supported level. Maps to <c>mapReasoningToProviderEffort</c>.
    /// </summary>
    public static string? MapReasoningToProviderEffort(
        string? reasoning,
        IReadOnlyDictionary<string, string> effortMap,
        IList<ModelWarning> warnings,
        string? providerOptionsEffort = null
    )
    {
        if (effortMap is null)
        {
            throw new ArgumentNullException(nameof(effortMap));
        }

        if (warnings is null)
        {
            throw new ArgumentNullException(nameof(warnings));
        }

        if (providerOptionsEffort is not null)
        {
            return providerOptionsEffort;
        }

        if (!IsCustomReasoning(reasoning))
        {
            return null;
        }

        // IndexOf is -1 for "none", so every level is a higher fallback.
        var index = Array.IndexOf(ReasoningLevels, reasoning);
        var candidates = new List<string> { reasoning! };
        for (var i = Math.Max(index, 0) - 1; i >= 0; i--)
        {
            candidates.Add(ReasoningLevels[i]);
        }

        for (var i = index + 1; i < ReasoningLevels.Length; i++)
        {
            candidates.Add(ReasoningLevels[i]);
        }

        string? mapped = null;
        foreach (var candidate in candidates)
        {
            if (effortMap.TryGetValue(candidate, out var value) && value is not null)
            {
                mapped = value;
                break;
            }
        }

        if (mapped is null)
        {
            warnings.Add(
                new UnsupportedWarning(
                    "reasoning",
                    "reasoning \"" + reasoning + "\" is not supported by this model."
                )
            );
            return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(
                new CompatibilityWarning(
                    "reasoning",
                    "reasoning \""
                        + reasoning
                        + "\" is not directly supported by this model. mapped to effort \""
                        + mapped
                        + "\"."
                )
            );
        }

        return mapped;
    }

    /// <summary>
    /// Maps a reasoning value to a token budget. Levels missing from <paramref name="budgetPercentages"/> use the default
    /// percentages. Handle <c>none</c> before calling when the model can disable reasoning. Otherwise <c>none</c> maps to
    /// <paramref name="minReasoningBudget"/> with a compatibility warning. Maps to <c>mapReasoningToProviderBudget</c>.
    /// </summary>
    public static int MapReasoningToProviderBudget(
        string reasoning,
        int maxOutputTokens,
        int maxReasoningBudget,
        IList<ModelWarning> warnings,
        int minReasoningBudget = 1024,
        IReadOnlyDictionary<string, double>? budgetPercentages = null
    )
    {
        if (warnings is null)
        {
            throw new ArgumentNullException(nameof(warnings));
        }

        if (reasoning == "none")
        {
            warnings.Add(
                new CompatibilityWarning(
                    "reasoning",
                    "reasoning \"none\" is not supported by this model. mapped to the minimum reasoning budget of "
                        + minReasoningBudget.ToString(
                            System.Globalization.CultureInfo.InvariantCulture
                        )
                        + " tokens."
                )
            );
            return minReasoningBudget;
        }

        double pct;
        if (budgetPercentages is null || !budgetPercentages.TryGetValue(reasoning, out pct))
        {
            pct = DefaultBudgetPercentages[reasoning];
        }

        var scaled = (int)Math.Round(maxOutputTokens * pct, MidpointRounding.AwayFromZero);
        return Math.Min(maxReasoningBudget, Math.Max(minReasoningBudget, scaled));
    }
}
