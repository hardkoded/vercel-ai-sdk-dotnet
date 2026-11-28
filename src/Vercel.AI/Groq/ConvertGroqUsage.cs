// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>Groq usage conversion. Maps to <c>convert-groq-usage.ts</c>.</summary>
internal static class ConvertGroqUsage
{
    /// <summary>Converts a Groq usage object. Maps to <c>convertGroqUsage</c>.</summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } element)
        {
            return LanguageModelUsage.Null;
        }

        var promptTokens = ReadNumber(element, "prompt_tokens") ?? 0;
        var cacheReadTokens = Child(element, "prompt_tokens_details") is { } promptDetails
            ? ReadNumber(promptDetails, "cached_tokens")
            : null;
        var completionTokens = ReadNumber(element, "completion_tokens") ?? 0;
        var reasoningTokens = Child(element, "completion_tokens_details") is { } completionDetails
            ? ReadNumber(completionDetails, "reasoning_tokens")
            : null;
        var textTokens =
            reasoningTokens != null
                ? Math.Max(0, completionTokens - reasoningTokens.Value)
                : completionTokens;

        return new LanguageModelUsage(
            promptTokens,
            completionTokens,
            ReadNumber(element, "total_tokens"),
            cacheReadTokens: cacheReadTokens,
            cacheWriteTokens: null,
            reasoningTokens: reasoningTokens,
            raw: element.Clone(),
            noCacheInputTokens: cacheReadTokens != null
                ? promptTokens - cacheReadTokens.Value
                : promptTokens,
            textTokens: textTokens
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
