// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Xai;

/// <summary>Maps an xAI Chat Completions finish reason. Maps to <c>mapXaiFinishReason</c> in <c>map-xai-finish-reason.ts</c>.</summary>
internal static class MapXaiFinishReason
{
    /// <summary>Returns the unified finish reason name.</summary>
    public static string Map(string? finishReason)
    {
        return finishReason switch
        {
            "stop" => "stop",
            "length" => "length",
            "tool_calls" or "function_call" => "tool-calls",
            "content_filter" => "content-filter",
            _ => "other",
        };
    }
}
