// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Anthropic model limits that Bedrock Converse has to honor.</summary>
public sealed class AmazonBedrockModelCapabilities
{
    /// <summary>Creates a capability set.</summary>
    public AmazonBedrockModelCapabilities(
        int maxOutputTokens,
        bool supportsStructuredOutput,
        bool supportsAdaptiveThinking,
        bool rejectsSamplingParameters,
        bool supportsXhighEffort,
        bool rejectsThinkingDisabledAboveHighEffort,
        bool rejectsThinkingDisabled,
        bool rejectsForcedToolUse,
        bool isKnownModel)
    {
        MaxOutputTokens = maxOutputTokens;
        SupportsStructuredOutput = supportsStructuredOutput;
        SupportsAdaptiveThinking = supportsAdaptiveThinking;
        RejectsSamplingParameters = rejectsSamplingParameters;
        SupportsXhighEffort = supportsXhighEffort;
        RejectsThinkingDisabledAboveHighEffort = rejectsThinkingDisabledAboveHighEffort;
        RejectsThinkingDisabled = rejectsThinkingDisabled;
        RejectsForcedToolUse = rejectsForcedToolUse;
        IsKnownModel = isKnownModel;
    }

    /// <summary>Maximum output tokens for the model family.</summary>
    public int MaxOutputTokens { get; }

    /// <summary>Whether native structured output is available on the Anthropic API.</summary>
    public bool SupportsStructuredOutput { get; }

    /// <summary>Whether adaptive thinking is available.</summary>
    public bool SupportsAdaptiveThinking { get; }

    /// <summary>Whether temperature, top-p, and top-k are rejected.</summary>
    public bool RejectsSamplingParameters { get; }

    /// <summary>Whether <c>xhigh</c> effort is accepted.</summary>
    public bool SupportsXhighEffort { get; }

    /// <summary>Whether disabled thinking above high effort is rejected.</summary>
    public bool RejectsThinkingDisabledAboveHighEffort { get; }

    /// <summary>Whether thinking cannot be disabled.</summary>
    public bool RejectsThinkingDisabled { get; }

    /// <summary>Whether forced tool choice is rejected.</summary>
    public bool RejectsForcedToolUse { get; }

    /// <summary>Whether the id matched a known Claude family.</summary>
    public bool IsKnownModel { get; }
}

/// <summary>Resolves Claude family capabilities from a Bedrock model id.</summary>
public static class AmazonBedrockModelCapabilitiesResolver
{
    /// <summary>Returns capabilities for <paramref name="modelId"/>.</summary>
    public static AmazonBedrockModelCapabilities Resolve(string modelId)
    {
        modelId = modelId ?? string.Empty;
        if (modelId.IndexOf("claude-opus-5-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, true, true, true);
        }

        if (modelId.IndexOf("claude-opus-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, true, false, false);
        }

        if (modelId.IndexOf("claude-fable-5-1", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, true, true);
        }

        if (modelId.IndexOf("claude-fable-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, true, false);
        }

        if (modelId.IndexOf("claude-opus-4-8", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-7", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-sonnet-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, false, false);
        }

        if (modelId.IndexOf("claude-sonnet-4-6", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-6", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-sonnet-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-haiku-4-5", StringComparison.Ordinal) >= 0)
        {
            return Known(64000, true, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-opus-4-1", StringComparison.Ordinal) >= 0)
        {
            return Known(32000, true, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-sonnet-4(?:-|@)", RegexOptions.CultureInvariant))
        {
            return Known(64000, false, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-opus-4(?:-|@)", RegexOptions.CultureInvariant))
        {
            return Known(32000, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-3-haiku", StringComparison.Ordinal) >= 0)
        {
            return Known(4096, false, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-(?:instant(?:-|$)|v?2(?=$|[-.:])|3(?=$|[-.]))", RegexOptions.CultureInvariant))
        {
            return new AmazonBedrockModelCapabilities(4096, false, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-", StringComparison.Ordinal) >= 0)
        {
            return new AmazonBedrockModelCapabilities(128000, true, true, true, true, true, false, false, false);
        }

        return new AmazonBedrockModelCapabilities(4096, false, false, false, false, false, false, false, false);
    }

    private static AmazonBedrockModelCapabilities Known(
        int maxOutputTokens,
        bool supportsStructuredOutput,
        bool supportsAdaptiveThinking,
        bool rejectsSamplingParameters,
        bool supportsXhighEffort,
        bool rejectsThinkingDisabledAboveHighEffort,
        bool rejectsThinkingDisabled,
        bool rejectsForcedToolUse)
    {
        return new AmazonBedrockModelCapabilities(
            maxOutputTokens,
            supportsStructuredOutput,
            supportsAdaptiveThinking,
            rejectsSamplingParameters,
            supportsXhighEffort,
            rejectsThinkingDisabledAboveHighEffort,
            rejectsThinkingDisabled,
            rejectsForcedToolUse,
            true);
    }
}
