// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.OpenAI;

/// <summary>Capability flags for an OpenAI chat model id.</summary>
public sealed class OpenAILanguageModelCapabilities
{
    internal OpenAILanguageModelCapabilities(
        bool isReasoningModel,
        string systemMessageMode,
        bool supportsFlexProcessing,
        bool supportsPriorityProcessing,
        bool supportsNonReasoningParameters,
        IReadOnlyList<string>? supportedReasoningEfforts)
    {
        IsReasoningModel = isReasoningModel;
        SystemMessageMode = systemMessageMode;
        SupportsFlexProcessing = supportsFlexProcessing;
        SupportsPriorityProcessing = supportsPriorityProcessing;
        SupportsNonReasoningParameters = supportsNonReasoningParameters;
        SupportedReasoningEfforts = supportedReasoningEfforts;
    }

    /// <summary>Whether sampling parameters are stripped for this model.</summary>
    public bool IsReasoningModel { get; }

    /// <summary><c>system</c>, <c>developer</c>, or <c>remove</c>.</summary>
    public string SystemMessageMode { get; }

    /// <summary>Whether <c>service_tier: flex</c> is accepted.</summary>
    public bool SupportsFlexProcessing { get; }

    /// <summary>Whether priority and fast service tiers are accepted.</summary>
    public bool SupportsPriorityProcessing { get; }

    /// <summary>Whether temperature, topP, and logprobs stay when reasoning effort is <c>none</c>.</summary>
    public bool SupportsNonReasoningParameters { get; }

    /// <summary>Allowed reasoning efforts, or null when the model does not constrain them.</summary>
    public IReadOnlyList<string>? SupportedReasoningEfforts { get; }

    private static readonly Regex OSeries = new Regex("^o(\\d+)(?:-|$)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex Gpt = new Regex("^gpt-(\\d+)(?:\\.(\\d+))?(?:-(.+))?$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>Resolves capabilities from a model id the same way the OpenAI provider does.</summary>
    public static OpenAILanguageModelCapabilities Get(string modelId)
    {
        var oSeries = OSeriesVersion(modelId);
        var gpt = GptVersion(modelId);
        var isGptChat = gpt != null && gpt.Minor == null && (gpt.Variant != null && gpt.Variant.StartsWith("chat", StringComparison.Ordinal));
        var isGptNano = gpt != null && gpt.Variant != null && gpt.Variant.StartsWith("nano", StringComparison.Ordinal);
        var isGpt6 = gpt != null && gpt.Major >= 6;
        var isGpt6SolOrLuna = modelId == "gpt-6-sol" || modelId == "gpt-6-luna";
        var supportsFlex = (oSeries != null && oSeries.Value >= 3) || (gpt != null && gpt.Major >= 5 && !isGptChat);
        var supportsPriority = modelId.StartsWith("gpt-4", StringComparison.Ordinal)
            || (gpt != null && gpt.Major >= 5 && !isGptNano && !isGptChat)
            || (oSeries != null && oSeries.Value >= 3);
        var isReasoning = oSeries != null || (gpt != null && gpt.Major >= 5 && !isGptChat);
        var supportsNonReasoning = !isGpt6 && gpt != null && (gpt.Major > 5 || (gpt.Major == 5 && (gpt.Minor ?? 0) >= 1));
        IReadOnlyList<string>? efforts = null;
        if (isGpt6SolOrLuna)
        {
            efforts = new[] { "none", "low", "medium", "high", "xhigh", "max" };
        }
        else if (isGpt6)
        {
            efforts = new[] { "low", "medium", "high", "xhigh", "max" };
        }

        return new OpenAILanguageModelCapabilities(
            isReasoning,
            isReasoning ? "developer" : "system",
            supportsFlex,
            supportsPriority,
            supportsNonReasoning,
            efforts);
    }

    private static int? OSeriesVersion(string modelId)
    {
        var match = OSeries.Match(modelId ?? string.Empty);
        if (!match.Success)
        {
            return null;
        }

        return int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static GptVersionInfo? GptVersion(string modelId)
    {
        var match = Gpt.Match(modelId ?? string.Empty);
        if (!match.Success)
        {
            return null;
        }

        int? minor = match.Groups[2].Success
            ? int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)
            : (int?)null;
        var variant = match.Groups[3].Success ? match.Groups[3].Value : null;
        return new GptVersionInfo(
            int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
            minor,
            variant);
    }

    private sealed class GptVersionInfo
    {
        public GptVersionInfo(int major, int? minor, string? variant)
        {
            Major = major;
            Minor = minor;
            Variant = variant;
        }

        public int Major { get; }

        public int? Minor { get; }

        public string? Variant { get; }
    }
}
