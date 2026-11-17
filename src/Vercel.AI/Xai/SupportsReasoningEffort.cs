// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Xai;

/// <summary>Maps to <c>supports-reasoning-effort.ts</c>: compatibility projections for existing internal consumers.</summary>
internal static class XaiReasoningEffort
{
    /// <summary>False for models that reject the reasoning effort parameter. Maps to <c>supportsReasoningEffort</c>.</summary>
    public static bool SupportsReasoningEffort(string modelId)
    {
        return XaiModelCatalog
            .GetXaiLanguageModelEntry(modelId)
            .Capabilities.Reasoning.Portable.Configurable;
    }

    /// <summary>False for models that reject the <c>none</c> reasoning effort. Maps to <c>supportsNoneReasoningEffort</c>.</summary>
    public static bool SupportsNoneReasoningEffort(string modelId)
    {
        return XaiModelCatalog
            .GetXaiLanguageModelEntry(modelId)
            .Capabilities.Reasoning.Portable.None;
    }
}
