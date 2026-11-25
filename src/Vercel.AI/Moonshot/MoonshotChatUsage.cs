// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Moonshot;

/// <summary>Moonshot AI usage conversion. Maps to <c>convert-moonshotai-chat-usage.ts</c>.</summary>
internal static class MoonshotChatUsage
{
    /// <summary>Converts a Moonshot AI usage object. Maps to <c>convertMoonshotAIChatUsage</c>.</summary>
    public static LanguageModelUsage ConvertMoonshotAIChatUsage(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } element)
        {
            return LanguageModelUsage.Null;
        }

        var promptTokens = ReadNumber(element, "prompt_tokens") ?? 0;
        var completionTokens = ReadNumber(element, "completion_tokens") ?? 0;
        var details = Child(element, "prompt_tokens_details");
        var cacheReadTokens = ReadNumber(element, "cached_tokens")
            ?? (details is { } d ? ReadNumber(d, "cached_tokens") : null)
            ?? 0;
        var cacheWriteTokens = details is { } w ? ReadNumber(w, "cache_write_tokens") : null;
        var completionDetails = Child(element, "completion_tokens_details");
        var reasoningTokens = (
            completionDetails is { } c ? ReadNumber(c, "reasoning_tokens") : null
        ) ?? 0;

        return new LanguageModelUsage(
            promptTokens,
            completionTokens,
            ReadNumber(element, "total_tokens"),
            cacheReadTokens: cacheReadTokens,
            cacheWriteTokens: cacheWriteTokens,
            reasoningTokens: reasoningTokens,
            raw: element.Clone(),
            noCacheInputTokens: promptTokens - cacheReadTokens - (cacheWriteTokens ?? 0),
            textTokens: Math.Max(0, completionTokens - reasoningTokens)
        );
    }

    private static JsonElement? Child(JsonElement parent, string name)
    {
        return parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static int? ReadNumber(JsonElement parent, string name)
    {
        return
            parent.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }
}
