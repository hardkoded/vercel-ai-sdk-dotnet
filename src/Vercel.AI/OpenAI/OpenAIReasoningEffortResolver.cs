// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenAI;

/// <summary>Resolves portable reasoning against the efforts a model supports. Maps to <c>resolve-openai-reasoning-effort.ts</c>.</summary>
internal static class OpenAIReasoningEffortResolver
{
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

    /// <summary>Resolve portable reasoning against the efforts declared by the catalog. Maps to <c>resolveOpenAIReasoningEffort</c>.</summary>
    internal static string? ResolveOpenAIReasoningEffort(
        string? providerReasoningEffort,
        string? reasoning,
        IReadOnlyList<string>? supportedReasoningEfforts,
        IList<OpenAICallWarning> warnings
    )
    {
        if (providerReasoningEffort != null)
        {
            return providerReasoningEffort;
        }

        if (!ReasoningMap.IsCustomReasoning(reasoning))
        {
            return null;
        }

        var effortMap = new Dictionary<string, string>();
        foreach (var effort in supportedReasoningEfforts ?? AllOpenAIReasoningEfforts)
        {
            effortMap[effort] = effort;
        }

        var modelWarnings = new List<ModelWarning>();
        var mapped = ReasoningMap.MapReasoningToProviderEffort(
            reasoning!,
            effortMap,
            modelWarnings
        );
        foreach (var warning in modelWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(
                    new OpenAICallWarning(
                        callWarning.Type,
                        callWarning.Feature,
                        callWarning.Message
                    )
                );
            }
        }

        return mapped;
    }
}
