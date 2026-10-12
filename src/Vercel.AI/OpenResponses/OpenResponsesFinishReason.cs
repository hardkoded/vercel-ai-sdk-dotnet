// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenResponses;

/// <summary>Maps an Open Responses incomplete reason to a finish reason. Maps to <c>mapOpenResponsesFinishReason</c>.</summary>
internal static class OpenResponsesFinishReason
{
    public static FinishReason Map(string? finishReason, bool hasToolCalls)
    {
        switch (finishReason)
        {
            case null:
                return hasToolCalls ? FinishReason.ToolCalls : FinishReason.Stop;
            case "max_output_tokens":
                return FinishReason.Length;
            case "content_filter":
                return FinishReason.ContentFilter;
            default:
                return hasToolCalls ? FinishReason.ToolCalls : FinishReason.Other;
        }
    }
}
