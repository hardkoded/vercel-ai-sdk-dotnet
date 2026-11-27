// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Alibaba;

/// <summary>Maps to <c>supports-preserved-thinking.ts</c>.</summary>
internal static class SupportsPreservedThinkingChecker
{
    // The model IDs whose row in upstream's alibaba-model-catalog.ts sets preservedThinking.
    // Unlisted model IDs do not support it.
    private static readonly string[] PreservedThinkingModelIds =
    [
        "kimi-k2.7-code",
        "qwen3.6-max-preview",
        "qwen3.6-plus",
        "qwen3.6-plus-2026-04-02",
        "qwen3.7-flash",
        "qwen3.7-flash-2026-07-15",
        "qwen3.7-max",
        "qwen3.7-max-2026-05-17",
        "qwen3.7-max-2026-05-20",
        "qwen3.7-max-2026-06-08",
        "qwen3.7-max-preview",
        "qwen3.7-plus",
        "qwen3.7-plus-2026-05-26",
        "qwen3.8-flash",
        "qwen3.8-max",
        "qwen3.8-max-0902",
    ];

    /// <summary>True when the model replays the <c>reasoning_content</c> of earlier turns. Maps to <c>supportsPreservedThinking</c>.</summary>
    public static bool SupportsPreservedThinking(string modelId)
    {
        return Array.IndexOf(PreservedThinkingModelIds, modelId) >= 0;
    }
}
