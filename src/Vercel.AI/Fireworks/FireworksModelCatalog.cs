// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Fireworks;

/// <summary>Per-model Fireworks facts. Maps to <c>fireworks-model-catalog.ts</c>.</summary>
internal static class FireworksModelCatalog
{
    // Model families that always reason and reject reasoning effort 'none'.
    private static readonly Regex ModelsWithoutNoneReasoningEffort = new Regex(
        "^(?:gpt-oss|glm-5p3|muse-spark)",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    // Model families that reject reasoning effort 'minimal'.
    private static readonly Regex ModelsWithoutMinimalReasoningEffort = new Regex(
        "^(?:nemotron|inkling)",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>
    /// Fireworks accepts every reasoning effort for most models, so reasoning values are sent directly,
    /// except to model families known to reject them. Maps to <c>getFireworksReasoningEffortMap</c>.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> GetFireworksReasoningEffortMap(
        string modelId
    )
    {
        var modelName = modelId.Substring(modelId.LastIndexOf('/') + 1);
        var map = new Dictionary<string, string>();
        if (!ModelsWithoutNoneReasoningEffort.IsMatch(modelName))
        {
            map["none"] = "none";
        }

        if (!ModelsWithoutMinimalReasoningEffort.IsMatch(modelName))
        {
            map["minimal"] = "minimal";
        }

        foreach (var effort in new[] { "low", "medium", "high", "xhigh", "max" })
        {
            map[effort] = effort;
        }

        return map;
    }
}
