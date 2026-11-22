// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

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
        bool supportsNonReasoningParameters
    )
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
        var isGptChat =
            gpt != null
            && gpt.Value.Minor == null
            && variant != null
            && variant.StartsWith("chat", StringComparison.Ordinal);
        var isGptNano = variant != null && variant.StartsWith("nano", StringComparison.Ordinal);
        var isGpt6OrLater = gpt != null && gpt.Value.Major >= 6;
        var isGpt6SolOrLuna =
            modelId == "gpt-6-sol" || modelId == "gpt-6-luna" || modelId == "gpt-6.1-terra";
        // Pro, codex, and chat variants are excluded because their supported efforts are not known.
        var isStandardGpt5 =
            gpt != null
            && gpt.Value.Major == 5
            && (variant == null || StandardGpt5Variant.IsMatch(variant));
        var supportsFlex =
            (oSeries != null && oSeries >= 3)
            || (gpt != null && gpt.Value.Major >= 5 && !isGptChat);
        var supportsPriority =
            modelId.StartsWith("gpt-4", StringComparison.Ordinal)
            || (gpt != null && gpt.Value.Major >= 5 && !isGptNano && !isGptChat)
            || (oSeries != null && oSeries >= 3);
        var isReasoning = oSeries != null || (gpt != null && gpt.Value.Major >= 5 && !isGptChat);
        var supportsNonReasoning =
            isGpt6SolOrLuna
            || (
                !isGpt6OrLater
                && gpt != null
                && (gpt.Value.Major > 5 || (gpt.Value.Major == 5 && (gpt.Value.Minor ?? 0) >= 1))
            );
        IReadOnlyList<string>? efforts =
            isGpt6SolOrLuna ? new[] { "none", "low", "medium", "high", "xhigh", "max" }
            : isGpt6OrLater ? new[] { "low", "medium", "high", "xhigh", "max" }
            : isStandardGpt5 ? GetGpt5ReasoningEfforts(gpt!.Value.Minor ?? 0)
            : null;
        return new OpenAILanguageModelCapabilities(
            isReasoning,
            isReasoning ? "developer" : "system",
            supportsFlex,
            supportsPriority,
            isGpt6OrLater,
            isGpt6OrLater,
            efforts,
            supportsNonReasoning
        );
    }

    // Unknown model families send the reasoning value directly.
    private static readonly string[] AllOpenAIReasoningEfforts =
    [
        "none",
        "minimal",
        "low",
        "medium",
        "high",
        "xhigh",
        "max",
    ];

    /// <summary>
    /// Resolves the reasoning effort to send to OpenAI. A reasoning effort from provider options is sent unchanged.
    /// Otherwise the top-level reasoning value is mapped to the closest effort that the model family supports, or sent
    /// directly when the supported efforts are not known. Maps to <c>resolveOpenAIReasoningEffort</c>.
    /// </summary>
    internal static string? ResolveOpenAIReasoningEffort(
        string? providerReasoningEffort,
        string? reasoning,
        IReadOnlyList<string>? supportedReasoningEfforts,
        IList<OpenAICallWarning> warnings
    )
    {
        var modelWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(
            reasoning,
            (supportedReasoningEfforts ?? AllOpenAIReasoningEfforts).ToDictionary(
                value => value,
                value => value
            ),
            modelWarnings,
            providerReasoningEffort
        );
        foreach (var warning in modelWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(
                    new OpenAICallWarning(
                        callWarning.Type,
                        callWarning.Feature!,
                        callWarning.Message
                    )
                );
            }
        }

        return effort;
    }

    private static readonly Regex OSeries = new Regex(
        "^o([0-9]+)(?:-|\\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );
    private static readonly Regex StandardGpt5Variant = new Regex(
        "^(?:(?:mini|nano|sol|luna|terra)(?:-[0-9]{4}-[0-9]{2}-[0-9]{2})?|[0-9]{4}-[0-9]{2}-[0-9]{2})\\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );
    private static readonly Regex Gpt = new Regex(
        "^gpt-([0-9]+)(?:\\.([0-9]+))?(?:-(.+))?\\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    // A version number too large for int is not a known model, so it is treated as no match.
    private static int? GetOSeriesVersion(string modelId)
    {
        var match = OSeries.Match(modelId);
        return match.Success && TryParseInt(match.Groups[1].Value, out var version)
            ? version
            : null;
    }

    private static string[] GetGpt5ReasoningEfforts(int minor)
    {
        return minor switch
        {
            0 => ["minimal", "low", "medium", "high"],
            1 => ["none", "low", "medium", "high"],
            < 6 => ["none", "low", "medium", "high", "xhigh"],
            _ => ["none", "low", "medium", "high", "xhigh", "max"],
        };
    }

    private static (int Major, int? Minor, string? Variant)? GetGptVersion(string modelId)
    {
        var match = Gpt.Match(modelId);
        if (!match.Success || !TryParseInt(match.Groups[1].Value, out var major))
        {
            return null;
        }

        int? minor = null;
        if (match.Groups[2].Success)
        {
            if (!TryParseInt(match.Groups[2].Value, out var parsedMinor))
            {
                return null;
            }

            minor = parsedMinor;
        }

        var variant = match.Groups[3].Success ? match.Groups[3].Value : null;
        return (major, minor, variant);
    }

    private static bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
}
