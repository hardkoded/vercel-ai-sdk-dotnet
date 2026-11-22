// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Anthropic;

/// <summary>Thinking and effort derived from a top-level reasoning level.</summary>
public sealed class AnthropicReasoningConfig
{
    /// <summary>Creates a reasoning mapping.</summary>
    public AnthropicReasoningConfig(
        string? thinkingType,
        int? budgetTokens,
        string? effort,
        string? display
    )
    {
        ThinkingType = thinkingType;
        BudgetTokens = budgetTokens;
        Effort = effort;
        Display = display;
    }

    /// <summary><c>enabled</c>, <c>adaptive</c>, or <c>disabled</c>.</summary>
    public string? ThinkingType { get; }

    /// <summary>Budget for <c>enabled</c> thinking.</summary>
    public int? BudgetTokens { get; }

    /// <summary>Effort level, when adaptive thinking uses one.</summary>
    public string? Effort { get; }

    /// <summary>Thinking display mode.</summary>
    public string? Display { get; }
}

/// <summary>Maps SDK reasoning levels onto Anthropic thinking and effort.</summary>
public static class AnthropicReasoning
{
    /// <summary>True when reasoning asks for a concrete mode rather than the provider default.</summary>
    public static bool IsCustom(string? reasoning)
    {
        return !string.IsNullOrEmpty(reasoning) && reasoning != "provider-default";
    }

    /// <summary>Maps a reasoning level. Returns null for <c>provider-default</c>.</summary>
    public static AnthropicReasoningConfig? Resolve(
        string? reasoning,
        string modelId,
        AnthropicModelCapabilities capabilities,
        IList<AnthropicWarning> warnings
    )
    {
        if (!IsCustom(reasoning))
        {
            return null;
        }

        if (reasoning == "none")
        {
            if (capabilities.SupportsBetweenToolsThinking)
            {
                warnings.Add(
                    new AnthropicWarning(
                        "compatibility",
                        "reasoning",
                        "reasoning 'none' is not supported by "
                            + modelId
                            + "; thinking cannot be disabled. Using 'between_tools' thinking, the lowest thinking setting, instead."
                    )
                );
                return new AnthropicReasoningConfig("between_tools", null, null, null);
            }

            if (capabilities.RejectsThinkingDisabled)
            {
                warnings.Add(
                    new AnthropicWarning(
                        "compatibility",
                        "reasoning",
                        "reasoning 'none' is not supported by "
                            + modelId
                            + "; it always uses adaptive thinking. Using effort 'low' to minimize thinking instead."
                    )
                );
                return new AnthropicReasoningConfig(null, null, "low", null);
            }

            return new AnthropicReasoningConfig("disabled", null, null, null);
        }

        if (capabilities.SupportsAdaptiveThinking)
        {
            var effort = MapEffort(reasoning!, capabilities.SupportsXhighEffort, warnings);
            return new AnthropicReasoningConfig("adaptive", null, effort, "summarized");
        }

        var budget = MapBudget(reasoning!, capabilities.MaxOutputTokens, warnings);
        return new AnthropicReasoningConfig("enabled", budget, null, null);
    }

    private static string? MapEffort(
        string reasoning,
        bool supportsXhigh,
        IList<AnthropicWarning> warnings
    )
    {
        var effortMap = new Dictionary<string, string>
        {
            { "low", "low" },
            { "medium", "medium" },
            { "high", "high" },
            { "max", "max" },
        };
        if (supportsXhigh)
        {
            effortMap["xhigh"] = "xhigh";
        }

        var modelWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(reasoning, effortMap, modelWarnings);
        foreach (var warning in modelWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(
                    new AnthropicWarning(
                        callWarning.Type,
                        callWarning.Feature!,
                        callWarning.Message
                    )
                );
            }
        }

        return effort;
    }

    /// <summary>Token budget for budget-based thinking. Clamped to at least 1024.</summary>
    public static int MapBudget(
        string reasoning,
        int maxOutputTokens,
        IList<AnthropicWarning> warnings
    )
    {
        var modelWarnings = new List<ModelWarning>();
        var budget = ReasoningMap.MapReasoningToProviderBudget(
            reasoning,
            maxOutputTokens,
            maxOutputTokens,
            modelWarnings
        );
        foreach (var warning in modelWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(
                    new AnthropicWarning(
                        callWarning.Type,
                        callWarning.Feature!,
                        callWarning.Message
                    )
                );
            }
        }

        return budget;
    }
}
