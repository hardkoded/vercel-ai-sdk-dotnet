// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.QuiverAI;

/// <summary>Per-model QuiverAI facts. Maps to <c>quiverai-model-catalog.ts</c>.</summary>
internal static class QuiverAIModelCatalog
{
    /// <summary>Maps to <c>quiveraiRequestDefaults.language.structuredOutputs</c>.</summary>
    internal const bool LanguageStructuredOutputs = false;

    /// <summary>Maps to <c>quiveraiRequestDefaults.language.reasoningEfforts</c>.</summary>
    internal static readonly IReadOnlyList<string> LanguageReasoningEfforts =
    [
        "low",
        "medium",
        "high",
        "xhigh",
    ];

    /// <summary>The reasoning efforts a language model accepts. Maps to <c>getQuiverAILanguageModelEntry(modelId).capabilities.language.reasoning.efforts</c>.</summary>
    internal static IReadOnlyList<string> GetQuiverAILanguageModelReasoningEfforts(string modelId)
    {
        return LanguageReasoningEfforts;
    }
}
