// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Anthropic;

/// <summary>Thinking and effort derived from a top-level reasoning level.</summary>
public sealed class AnthropicReasoningConfig
{
    /// <summary>Creates a reasoning mapping.</summary>
    public AnthropicReasoningConfig(string? thinkingType, int? budgetTokens, string? effort, string? display)
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
        IList<AnthropicWarning> warnings)
    {
        if (!IsCustom(reasoning))
        {
            return null;
        }

        if (reasoning == "none")
        {
            if (capabilities.RejectsThinkingDisabled)
            {
                warnings.Add(new AnthropicWarning(
                    "compatibility",
                    "reasoning",
                    "reasoning 'none' is not supported by " + modelId + "; it always uses adaptive thinking. Using effort 'low' to minimize thinking instead."));
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
        if (budget == null)
        {
            return null;
        }

        return new AnthropicReasoningConfig("enabled", budget, null, null);
    }

    private static string? MapEffort(string reasoning, bool supportsXhigh, IList<AnthropicWarning> warnings)
    {
        string? mapped;
        switch (reasoning)
        {
            case "minimal":
                mapped = "low";
                break;
            case "low":
                mapped = "low";
                break;
            case "medium":
                mapped = "medium";
                break;
            case "high":
                mapped = "high";
                break;
            case "xhigh":
                mapped = supportsXhigh ? "xhigh" : "max";
                break;
            case "max":
                mapped = "max";
                break;
            default:
                warnings.Add(new AnthropicWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(new AnthropicWarning(
                "compatibility",
                "reasoning",
                "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    /// <summary>Token budget for budget-based thinking. Clamped to at least 1024.</summary>
    public static int? MapBudget(string reasoning, int maxOutputTokens, IList<AnthropicWarning> warnings)
    {
        double? percent;
        switch (reasoning)
        {
            case "minimal":
                percent = 0.02;
                break;
            case "low":
                percent = 0.1;
                break;
            case "medium":
                percent = 0.3;
                break;
            case "high":
                percent = 0.6;
                break;
            case "xhigh":
                percent = 0.9;
                break;
            case "max":
                percent = 0.95;
                break;
            default:
                warnings.Add(new AnthropicWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        var budget = (int)Math.Round(maxOutputTokens * percent.Value, MidpointRounding.AwayFromZero);
        if (budget < 1024)
        {
            budget = 1024;
        }

        if (budget > maxOutputTokens)
        {
            budget = maxOutputTokens;
        }

        return budget;
    }
}
