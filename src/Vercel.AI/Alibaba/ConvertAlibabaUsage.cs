// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Alibaba;

/// <summary>Maps to <c>convert-alibaba-usage.ts</c>.</summary>
internal static class ConvertAlibabaUsage
{
    /// <summary>
    /// Converts the usage object of a response or stream chunk. Fields the conversion does not map stay in
    /// <see cref="LanguageModelUsage.Raw"/>. Maps to <c>convertAlibabaUsage</c>.
    /// </summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } element)
        {
            return LanguageModelUsage.Null;
        }

        var promptTokens = ReadNumber(element, "prompt_tokens") ?? 0;
        var completionTokens = ReadNumber(element, "completion_tokens") ?? 0;
        var cacheReadTokens = ReadNestedNumber(element, "prompt_tokens_details", "cached_tokens")
            ?? 0;
        var cacheWriteTokens = ReadNestedNumber(
            element,
            "prompt_tokens_details",
            "cache_creation_input_tokens"
        ) ?? 0;
        var reasoningTokens = ReadNestedNumber(
            element,
            "completion_tokens_details",
            "reasoning_tokens"
        ) ?? 0;
        return new LanguageModelUsage(
            promptTokens,
            completionTokens,
            ReadNumber(element, "total_tokens"),
            cacheReadTokens: cacheReadTokens,
            cacheWriteTokens: cacheWriteTokens,
            reasoningTokens: reasoningTokens,
            raw: element.Clone(),
            // Alibaba counts both cache reads and cache writes inside prompt_tokens.
            noCacheInputTokens: promptTokens - cacheReadTokens - cacheWriteTokens,
            textTokens: Math.Max(0, completionTokens - reasoningTokens)
        );
    }

    private static int? ReadNumber(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static int? ReadNestedNumber(JsonElement element, string outer, string name)
    {
        return element.TryGetProperty(outer, out var inner) ? ReadNumber(inner, name) : null;
    }
}
