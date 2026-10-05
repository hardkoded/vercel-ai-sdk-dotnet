// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Maps Gemini finish reasons onto the shared finish-reason enum.</summary>
public static class GoogleFinishReason
{
    /// <summary>
    /// Maps <paramref name="finishReason"/>. <c>STOP</c> becomes <see cref="FinishReason.ToolCalls"/> when the response contains a client tool call.
    /// </summary>
    public static FinishReason Map(string? finishReason, bool hasToolCalls)
    {
        switch (finishReason)
        {
            case "STOP":
                return hasToolCalls ? FinishReason.ToolCalls : FinishReason.Stop;
            case "MAX_TOKENS":
                return FinishReason.Length;
            case "IMAGE_SAFETY":
            case "RECITATION":
            case "SAFETY":
            case "BLOCKLIST":
            case "PROHIBITED_CONTENT":
            case "SPII":
                return FinishReason.ContentFilter;
            case "MALFORMED_FUNCTION_CALL":
                return FinishReason.Error;
            default:
                return FinishReason.Other;
        }
    }

    /// <summary>
    /// A prompt block is terminal when the reason is present and is not an unspecified placeholder.
    /// </summary>
    public static bool IsConfirmedPromptBlock(string? blockReason)
    {
        return !string.IsNullOrEmpty(blockReason)
            && blockReason != "BLOCK_REASON_UNSPECIFIED"
            && blockReason != "BLOCKED_REASON_UNSPECIFIED";
    }
}
