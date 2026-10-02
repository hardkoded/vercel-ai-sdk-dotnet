// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.DeepSeek;

/// <summary>DeepSeek model-id helpers.</summary>
public static class DeepSeekModels
{
    /// <summary>
    /// True for V4 ids and the unversioned <c>deepseek-flash</c> and <c>deepseek-pro</c> aliases.
    /// Legacy <c>deepseek-chat</c> and <c>deepseek-reasoner</c> are not V4.
    /// </summary>
    public static bool IsV4(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return false;
        }

        return modelId.IndexOf("deepseek-v4", StringComparison.Ordinal) >= 0
            || modelId.StartsWith("deepseek-flash", StringComparison.Ordinal)
            || modelId.StartsWith("deepseek-pro", StringComparison.Ordinal);
    }
}
