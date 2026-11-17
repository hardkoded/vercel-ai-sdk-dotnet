// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepSeek;

/// <summary>Maps to <c>convert-to-deepseek-usage.ts</c>.</summary>
internal static class ConvertToDeepSeekUsage
{
    /// <summary>Converts the usage object of a response or stream chunk. Maps to <c>convertDeepSeekUsage</c>.</summary>
    public static LanguageModelUsage ConvertDeepSeekUsage(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } element)
        {
            return LanguageModelUsage.Null;
        }

        var promptTokens = Int(element, "prompt_tokens") ?? 0;
        var completionTokens = Int(element, "completion_tokens") ?? 0;
        var cacheReadTokens = Int(element, "prompt_cache_hit_tokens")
            ?? Nested(element, "prompt_tokens_details", "cached_tokens")
            ?? 0;
        var reasoningTokens = Nested(element, "completion_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(
            promptTokens,
            completionTokens,
            null,
            cacheReadTokens: cacheReadTokens,
            reasoningTokens: reasoningTokens,
            raw: element.Clone(),
            noCacheInputTokens: promptTokens - cacheReadTokens,
            textTokens: Math.Max(0, completionTokens - reasoningTokens)
        );
    }

    private static int? Int(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static int? Nested(JsonElement element, string parent, string name)
    {
        return
            element.TryGetProperty(parent, out var details)
            && details.ValueKind == JsonValueKind.Object
            ? Int(details, name)
            : null;
    }
}
