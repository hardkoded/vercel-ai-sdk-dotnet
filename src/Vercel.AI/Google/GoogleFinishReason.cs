// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Maps Gemini finish reasons onto the SDK finish reason.</summary>
public static class GoogleFinishReason
{
    /// <summary>
    /// Maps <paramref name="finishReason"/>. <c>STOP</c> is tool-calls only when the response
    /// contains a client-executed function call.
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

    internal static bool IsConfirmedPromptBlock(string? blockReason)
    {
        return !string.IsNullOrEmpty(blockReason)
            && blockReason != "BLOCK_REASON_UNSPECIFIED"
            && blockReason != "BLOCKED_REASON_UNSPECIFIED";
    }
}
