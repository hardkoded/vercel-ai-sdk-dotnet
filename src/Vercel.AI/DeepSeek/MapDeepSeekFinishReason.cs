// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.DeepSeek;

/// <summary>Maps to <c>map-deepseek-finish-reason.ts</c>.</summary>
internal static class MapDeepSeekFinishReason
{
    /// <summary>Maps the provider finish reason. Maps to <c>mapDeepSeekFinishReason</c>.</summary>
    public static FinishReason Map(string? finishReason)
    {
        return finishReason switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "content_filter" => FinishReason.ContentFilter,
            "tool_calls" => FinishReason.ToolCalls,
            "insufficient_system_resource" => FinishReason.Error,
            _ => FinishReason.Other,
        };
    }
}
