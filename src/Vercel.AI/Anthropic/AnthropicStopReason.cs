// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Maps an Anthropic <c>stop_reason</c> onto the SDK finish reason.</summary>
public static class AnthropicStopReason
{
    /// <summary>
    /// Maps a stop reason. <c>tool_use</c> is <see cref="FinishReason.Stop"/> when the JSON response tool produced the payload.
    /// </summary>
    public static FinishReason Map(string? finishReason, bool isJsonResponseFromTool = false)
    {
        switch (finishReason)
        {
            case "pause_turn":
            case "end_turn":
            case "stop_sequence":
                return FinishReason.Stop;
            case "refusal":
                return FinishReason.ContentFilter;
            case "tool_use":
                return isJsonResponseFromTool ? FinishReason.Stop : FinishReason.ToolCalls;
            case "max_tokens":
            case "model_context_window_exceeded":
                return FinishReason.Length;
            default:
                return FinishReason.Other;
        }
    }
}
