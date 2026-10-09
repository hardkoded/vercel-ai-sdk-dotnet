// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Anthropic;

/// <summary>What a Claude model id accepts on the Messages API.</summary>
public sealed class AnthropicModelCapabilities
{
    /// <summary>Creates a capability set.</summary>
    public AnthropicModelCapabilities(
        int maxOutputTokens,
        bool supportsStructuredOutput,
        bool supportsAdaptiveThinking,
        bool rejectsSamplingParameters,
        bool supportsXhighEffort,
        bool rejectsThinkingDisabledAboveHighEffort,
        bool rejectsThinkingDisabled,
        bool rejectsBudgetThinking,
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
        RejectsBudgetThinking = rejectsBudgetThinking;
        RejectsForcedToolUse = rejectsForcedToolUse;
        IsKnownModel = isKnownModel;
    }

    /// <summary>Model output token ceiling.</summary>
    public int MaxOutputTokens { get; }

    /// <summary>Native <c>output_config.format</c> is available.</summary>
    public bool SupportsStructuredOutput { get; }

    /// <summary>Thinking is adaptive rather than budget-based.</summary>
    public bool SupportsAdaptiveThinking { get; }

    /// <summary>Temperature, top-p, and top-k are rejected.</summary>
    public bool RejectsSamplingParameters { get; }

    /// <summary>Effort <c>xhigh</c> is accepted.</summary>
    public bool SupportsXhighEffort { get; }

    /// <summary>Thinking cannot be disabled at effort above <c>high</c>.</summary>
    public bool RejectsThinkingDisabledAboveHighEffort { get; }

    /// <summary>The model always thinks and rejects <c>disabled</c> thinking.</summary>
    public bool RejectsThinkingDisabled { get; }

    /// <summary>Budget-based thinking is rejected and sent as adaptive thinking.</summary>
    public bool RejectsBudgetThinking { get; }

    /// <summary>Forced tool choice is rejected.</summary>
    public bool RejectsForcedToolUse { get; }

    /// <summary>The id is a recognized Claude model rather than a forward-compatible guess.</summary>
    public bool IsKnownModel { get; }

    /// <summary>Resolves capabilities from a model id, including Bedrock-style prefixes.</summary>
    public static AnthropicModelCapabilities Get(string modelId)
    {
        if (modelId.IndexOf("claude-opus-5-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, true, true, true, true);
        }

        if (modelId.IndexOf("claude-opus-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, true, false, false, false);
        }

        if (modelId.IndexOf("claude-fable-5-1", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, true, true, true);
        }

        if (modelId.IndexOf("claude-fable-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, true, true, false);
        }

        if (modelId.IndexOf("claude-haiku-5-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, true, false, true, false);
        }

        if (modelId.IndexOf("claude-opus-4-8", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-7", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-sonnet-5", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, true, true, false, false, false, false);
        }

        if (modelId.IndexOf("claude-sonnet-4-6", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-6", StringComparison.Ordinal) >= 0)
        {
            return Known(128000, true, true, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-sonnet-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-haiku-4-5", StringComparison.Ordinal) >= 0)
        {
            return Known(64000, true, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-opus-4-1", StringComparison.Ordinal) >= 0)
        {
            return Known(32000, true, false, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-sonnet-4(?:-|@)", RegexOptions.CultureInvariant))
        {
            return Known(64000, false, false, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-opus-4(?:-|@)", RegexOptions.CultureInvariant))
        {
            return Known(32000, false, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-3-haiku", StringComparison.Ordinal) >= 0)
        {
            return Known(4096, false, false, false, false, false, false, false, false);
        }

        if (Regex.IsMatch(modelId, "claude-(?:instant(?:-|$)|v?2(?=$|[-.:])|3(?=$|[-.]))", RegexOptions.CultureInvariant))
        {
            return Unknown(4096, false, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-", StringComparison.Ordinal) >= 0)
        {
            return Unknown(128000, true, true, true, true, true, false, false, false);
        }

        return Unknown(4096, false, false, false, false, false, false, false, false);
    }

    private static AnthropicModelCapabilities Known(
        int max,
        bool structured,
        bool adaptive,
        bool rejectSampling,
        bool xhigh,
        bool rejectDisabledAboveHigh,
        bool rejectDisabled,
        bool rejectBudget,
        bool rejectForced)
    {
        return new AnthropicModelCapabilities(max, structured, adaptive, rejectSampling, xhigh, rejectDisabledAboveHigh, rejectDisabled, rejectBudget, rejectForced, true);
    }

    private static AnthropicModelCapabilities Unknown(
        int max,
        bool structured,
        bool adaptive,
        bool rejectSampling,
        bool xhigh,
        bool rejectDisabledAboveHigh,
        bool rejectDisabled,
        bool rejectBudget,
        bool rejectForced)
    {
        return new AnthropicModelCapabilities(max, structured, adaptive, rejectSampling, xhigh, rejectDisabledAboveHigh, rejectDisabled, rejectBudget, rejectForced, false);
    }
}
