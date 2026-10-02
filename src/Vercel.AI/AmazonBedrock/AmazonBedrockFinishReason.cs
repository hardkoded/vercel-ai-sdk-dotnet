// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Maps a Converse <c>stopReason</c> onto the SDK finish reason.</summary>
public static class AmazonBedrockFinishReason
{
    /// <summary>
    /// Maps <paramref name="stopReason"/>. A JSON response tool reports <c>tool_use</c>
    /// but finishes as <see cref="FinishReason.Stop"/> when <paramref name="isJsonResponseFromTool"/> is true.
    /// </summary>
    public static FinishReason Map(string? stopReason, bool isJsonResponseFromTool)
    {
        switch (stopReason)
        {
            case "stop_sequence":
            case "end_turn":
                return FinishReason.Stop;
            case "max_tokens":
                return FinishReason.Length;
            case "content_filtered":
            case "guardrail_intervened":
                return FinishReason.ContentFilter;
            case "tool_use":
                return isJsonResponseFromTool ? FinishReason.Stop : FinishReason.ToolCalls;
            default:
                return FinishReason.Other;
        }
    }
}
