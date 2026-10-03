// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.OpenAI;

/// <summary>Request limits derived from an OpenAI model id.</summary>
public sealed class OpenAILanguageModelCapabilities
{
    internal OpenAILanguageModelCapabilities(
        bool isReasoningModel,
        string systemMessageMode,
        bool supportsFlexProcessing,
        bool supportsPriorityProcessing,
        bool supportsConfigurationUpdate,
        bool supportsAsyncToolCalling,
        IReadOnlyList<string>? supportedReasoningEfforts,
        bool supportsNonReasoningParameters)
    {
        IsReasoningModel = isReasoningModel;
        SystemMessageMode = systemMessageMode;
        SupportsFlexProcessing = supportsFlexProcessing;
        SupportsPriorityProcessing = supportsPriorityProcessing;
        SupportsConfigurationUpdate = supportsConfigurationUpdate;
        SupportsAsyncToolCalling = supportsAsyncToolCalling;
        SupportedReasoningEfforts = supportedReasoningEfforts;
        SupportsNonReasoningParameters = supportsNonReasoningParameters;
    }

    /// <summary>Whether sampling parameters such as temperature are limited.</summary>
    public bool IsReasoningModel { get; }

    /// <summary><c>system</c>, <c>developer</c>, or <c>remove</c>.</summary>
    public string SystemMessageMode { get; }

    /// <summary>Whether <c>service_tier: flex</c> is accepted.</summary>
    public bool SupportsFlexProcessing { get; }

    /// <summary>Whether <c>service_tier</c> <c>priority</c> or <c>fast</c> is accepted.</summary>
    public bool SupportsPriorityProcessing { get; }

    /// <summary>Whether GPT-6 configuration updates are accepted.</summary>
    public bool SupportsConfigurationUpdate { get; }

    /// <summary>Whether async tool calling is accepted.</summary>
    public bool SupportsAsyncToolCalling { get; }

    /// <summary>Allowed reasoning efforts. Null when the family does not constrain them.</summary>
    public IReadOnlyList<string>? SupportedReasoningEfforts { get; }

    /// <summary>Whether temperature and top_p stay when reasoning effort is <c>none</c>.</summary>
    public bool SupportsNonReasoningParameters { get; }

    /// <summary>Classifies <paramref name="modelId"/> the way the OpenAI provider does.</summary>
    public static OpenAILanguageModelCapabilities ForModel(string modelId)
    {
        var oSeries = GetOSeriesVersion(modelId);
        var gpt = GetGptVersion(modelId);
        var variant = gpt?.Variant;
        var isGptChat = gpt != null
            && gpt.Value.Minor == null
            && variant != null
            && variant.StartsWith("chat", StringComparison.Ordinal);
        var isGptNano = variant != null && variant.StartsWith("nano", StringComparison.Ordinal);
        var isGpt6OrLater = gpt != null && gpt.Value.Major >= 6;
        var isGpt6SolOrLuna = modelId == "gpt-6-sol" || modelId == "gpt-6-luna";
        var supportsFlex = (oSeries != null && oSeries >= 3) || (gpt != null && gpt.Value.Major >= 5 && !isGptChat);
        var supportsPriority = modelId.StartsWith("gpt-4", StringComparison.Ordinal)
            || (gpt != null && gpt.Value.Major >= 5 && !isGptNano && !isGptChat)
            || (oSeries != null && oSeries >= 3);
        var isReasoning = oSeries != null || (gpt != null && gpt.Value.Major >= 5 && !isGptChat);
        var supportsNonReasoning = !isGpt6OrLater
            && gpt != null
            && (gpt.Value.Major > 5 || (gpt.Value.Major == 5 && (gpt.Value.Minor ?? 0) >= 1));
        IReadOnlyList<string>? efforts = isGpt6SolOrLuna
            ? new[] { "none", "low", "medium", "high", "xhigh", "max" }
            : isGpt6OrLater
                ? new[] { "low", "medium", "high", "xhigh", "max" }
                : null;
        return new OpenAILanguageModelCapabilities(
            isReasoning,
            isReasoning ? "developer" : "system",
            supportsFlex,
            supportsPriority,
            isGpt6OrLater,
            isGpt6OrLater,
            efforts,
            supportsNonReasoning);
    }

    private static int? GetOSeriesVersion(string modelId)
    {
        var match = Regex.Match(modelId, "^o(\\d+)(?:-|$)");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    private static (int Major, int? Minor, string? Variant)? GetGptVersion(string modelId)
    {
        var match = Regex.Match(modelId, "^gpt-(\\d+)(?:\\.(\\d+))?(?:-(.+))?$");
        if (!match.Success)
        {
            return null;
        }

        int? minor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : null;
        var variant = match.Groups[3].Success ? match.Groups[3].Value : null;
        return (int.Parse(match.Groups[1].Value), minor, variant);
    }
}
