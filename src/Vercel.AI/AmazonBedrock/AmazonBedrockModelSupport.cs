// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Detects Anthropic models and the Converse features they accept.</summary>
public static class AmazonBedrockModelSupport
{
    private static readonly string[] ModelsWithoutStrictToolSupport =
    {
        "claude-opus-4-7",
        "claude-opus-4-8",
        "claude-opus-5",
        "claude-fable-5",
        "claude-sonnet-5",
    };

    private static readonly string[] ModelsWithoutReliableNativeStructuredOutput =
    {
        "claude-opus-4-7",
        "claude-opus-4-8",
        "claude-opus-5",
        "claude-fable-5",
        "claude-sonnet-5",
        "claude-sonnet-4-6",
        "claude-haiku-4-5",
    };

    /// <summary>
    /// An application inference profile is Anthropic when <paramref name="modelFamily"/> says so,
    /// the id contains <c>anthropic</c>, or a reasoning budget is set on an application inference profile.
    /// </summary>
    public static bool IsAnthropicModel(string modelId, string? modelFamily, int? reasoningBudgetTokens)
    {
        if (string.Equals(modelFamily, "anthropic", StringComparison.Ordinal))
        {
            return true;
        }

        if (modelId != null && modelId.IndexOf("anthropic", StringComparison.Ordinal) >= 0)
        {
            return true;
        }

        return modelId != null
            && modelId.IndexOf(":application-inference-profile/", StringComparison.Ordinal) >= 0
            && reasoningBudgetTokens != null;
    }

    /// <summary>Whether tool <c>strict</c> is accepted for <paramref name="modelId"/>.</summary>
    public static bool SupportsStrictTools(string modelId)
    {
        return !Matches(modelId, ModelsWithoutStrictToolSupport);
    }

    /// <summary>Whether native structured output is reliable for <paramref name="modelId"/>.</summary>
    public static bool SupportsNativeStructuredOutput(string modelId)
    {
        return !Matches(modelId, ModelsWithoutReliableNativeStructuredOutput);
    }

    private static bool Matches(string modelId, string[] models)
    {
        if (modelId == null)
        {
            return false;
        }

        foreach (var model in models)
        {
            if (modelId.IndexOf(model, StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
