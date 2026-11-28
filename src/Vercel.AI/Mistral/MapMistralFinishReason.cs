// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Maps Mistral finish reasons. Maps to <c>map-mistral-finish-reason.ts</c>.</summary>
internal static class MapMistralFinishReason
{
    /// <summary>Maps a Mistral finish reason to the unified one. Maps to <c>mapMistralFinishReason</c>.</summary>
    public static FinishReason Map(string? finishReason)
    {
        return finishReason switch
        {
            "stop" => FinishReason.Stop,
            "length" or "model_length" => FinishReason.Length,
            "tool_calls" => FinishReason.ToolCalls,
            "error" => FinishReason.Error,
            _ => FinishReason.Other,
        };
    }
}
