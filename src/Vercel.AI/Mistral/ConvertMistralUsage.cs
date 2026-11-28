// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Converts Mistral usage. Maps to <c>convert-mistral-usage.ts</c>.</summary>
internal static class ConvertMistralUsage
{
    /// <summary>Converts a Mistral usage object. A null usage gives empty usage. Maps to <c>convertMistralUsage</c>.</summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } value)
        {
            return LanguageModelUsage.Null;
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

/// <summary>Validates Mistral usage. Maps to <c>mistralUsageSchema</c>.</summary>
internal static class MistralUsageSchema
{
    private static readonly string[] RequiredNumbers =
    {
        "prompt_tokens",
        "completion_tokens",
        "total_tokens",
    };

    private static readonly string[] OptionalNumbers =
    {
        "prompt_audio_seconds",
        "request_count",
        "num_cached_tokens",
    };

    /// <summary>
    /// Checks a usage object and returns it unchanged, so extra fields stay in <c>raw</c>. Throws
    /// <see cref="TypeValidationException"/> when a known field has the wrong type.
    /// </summary>
    public static JsonElement Parse(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("usage must be an object.", usage);
        }

        foreach (var name in RequiredNumbers)
        {
            if (
                !usage.TryGetProperty(name, out var value)
                || value.ValueKind != JsonValueKind.Number
            )
            {
                throw Invalid(name + " must be a number.", usage);
            }
        }

        foreach (var name in OptionalNumbers)
        {
            Nullish(usage, name, JsonValueKind.Number);
        }

        Nullish(usage, "service_tier", JsonValueKind.String);
        foreach (var name in new[] { "prompt_tokens_details", "prompt_token_details" })
        {
            if (Nullish(usage, name, JsonValueKind.Object) is { } details)
            {
                Nullish(details, "cached_tokens", JsonValueKind.Number, name + ".");
                Nullish(details, "audio_tokens", JsonValueKind.Number, name + ".");
                Nullish(details, "messages", JsonValueKind.Array, name + ".");
            }
        }

        if (Nullish(usage, "completion_tokens_details", JsonValueKind.Object) is { } completion)
        {
            Nullish(
                completion,
                "reasoning_tokens",
                JsonValueKind.Number,
                "completion_tokens_details."
            );
        }

        return usage;
    }

    private static JsonElement? Nullish(
        JsonElement owner,
        string name,
        JsonValueKind kind,
        string path = ""
    )
    {
        if (!owner.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == kind
            ? value
            : throw Invalid(path + name + " has the wrong type.", owner);
    }

    private static TypeValidationException Invalid(string message, JsonElement value) =>
        new("Invalid Mistral usage: " + message, value.Clone());
}
