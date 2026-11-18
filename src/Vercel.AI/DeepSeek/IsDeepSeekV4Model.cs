// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.DeepSeek;

/// <summary>Maps to <c>is-deepseek-v4-model.ts</c>.</summary>
internal static class IsDeepSeekV4Model
{
    /// <summary>
    /// Whether a model id uses DeepSeek V4-or-newer behavior (thinking mode on by default, <c>reasoning_content</c>
    /// required on every assistant turn). Known legacy aliases and pre-V4 version IDs retain their previous behavior;
    /// unrecognized IDs inherit the current protocol. Maps to <c>isDeepSeekV4Model</c>.
    /// </summary>
    public static bool Check(string modelId)
    {
        return DeepSeekModelCatalog
            .GetDeepSeekLanguageModelEntry(modelId)
            .Language.Reasoning.AssistantHistory;
    }
}
