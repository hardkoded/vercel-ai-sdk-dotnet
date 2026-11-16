// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Converts Mistral usage. Maps to <c>convert-mistral-usage.ts</c>.</summary>
internal static class ConvertMistralUsage
{
    /// <summary>Converts a Mistral usage object. A null usage gives empty usage. Maps to <c>convertMistralUsage</c>.</summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } value)
        {
            return LanguageModelUsage.Empty;
        }

        var promptTokens = Count(value, "prompt_tokens") ?? 0;
        var completionTokens = Count(value, "completion_tokens") ?? 0;
        var cacheReadTokens = Count(value, "num_cached_tokens")
            ?? Details(value, "prompt_tokens_details")
            ?? Details(value, "prompt_token_details")
            ?? 0;

        return new LanguageModelUsage(
            promptTokens,
            completionTokens,
            null,
            cacheReadTokens == 0 ? null : cacheReadTokens,
            null,
            null,
            value.Clone(),
            promptTokens - cacheReadTokens,
            completionTokens
        );
    }

    private static int? Details(JsonElement usage, string name)
    {
        return
            usage.TryGetProperty(name, out var details) && details.ValueKind == JsonValueKind.Object
            ? Count(details, "cached_tokens")
            : null;
    }

    private static int? Count(JsonElement owner, string name)
    {
        return
            owner.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }
}
