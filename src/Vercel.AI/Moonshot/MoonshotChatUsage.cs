// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
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

        Validate(element);
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

    /// <summary>
    /// Reads the cache write tokens by TTL from the <c>msh-usage-cache-write-tokens-5m</c> and
    /// <c>msh-usage-cache-write-tokens-1h</c> response headers. Maps to <c>extractCacheWriteTokensByTtl</c>.
    /// </summary>
    public static JsonObject? ExtractCacheWriteTokensByTtl(
        IReadOnlyDictionary<string, string>? headers
    )
    {
        var ttl5m = ParseTokenCountHeader(headers, "msh-usage-cache-write-tokens-5m");
        var ttl1h = ParseTokenCountHeader(headers, "msh-usage-cache-write-tokens-1h");
        if (ttl5m == null && ttl1h == null)
        {
            return null;
        }

        return new JsonObject
        {
            ["cacheWriteTokens"] = new JsonObject
            {
                ["ttl5m"] = ttl5m ?? 0,
                ["ttl1h"] = ttl1h ?? 0,
            },
        };
    }

    /// <summary>Maps to <c>parseTokenCountHeader</c>: only a string of ASCII digits counts.</summary>
    private static long? ParseTokenCountHeader(
        IReadOnlyDictionary<string, string>? headers,
        string name
    )
    {
        if (headers == null)
        {
            return null;
        }

        string? value = null;
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                break;
            }
        }

        var trimmed = value?.Trim();
        if (
            string.IsNullOrEmpty(trimmed)
            || !trimmed!.All(static c => c is >= '0' and <= '9')
            || !long.TryParse(
                trimmed,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var number
            )
        )
        {
            return null;
        }

        return number;
    }

    /// <summary>
    /// Checks the known usage fields the way <c>tokenUsageSchema</c> does: numbers or null, nested objects or null.
    /// Other fields pass through into <see cref="LanguageModelUsage.Raw"/>.
    /// </summary>
    private static void Validate(JsonElement usage)
    {
        ValidateNumbers(
            usage,
            "usage",
            "prompt_tokens",
            "completion_tokens",
            "cached_tokens",
            "total_tokens"
        );
        ValidateObject(usage, "prompt_tokens_details", "cached_tokens", "cache_write_tokens");
        ValidateObject(usage, "completion_tokens_details", "reasoning_tokens");
    }

    private static void ValidateObject(JsonElement parent, string name, params string[] numbers)
    {
        if (!parent.TryGetProperty(name, out var child) || child.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (child.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidResponseDataException(
                parent.GetRawText(),
                "usage." + name + " must be an object."
            );
        }

        ValidateNumbers(child, "usage." + name, numbers);
    }

    private static void ValidateNumbers(JsonElement parent, string path, params string[] names)
    {
        foreach (var name in names)
        {
            if (
                parent.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null
                && value.ValueKind != JsonValueKind.Number
            )
            {
                throw new InvalidResponseDataException(
                    parent.GetRawText(),
                    path + "." + name + " must be a number."
                );
            }
        }
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
