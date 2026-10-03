// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Converts an Anthropic <c>usage</c> object into SDK token counts.</summary>
public static class AnthropicUsage
{
    /// <summary>
    /// Maps Anthropic usage. Cache creation and cache reads are added to the input total.
    /// Compaction and message iterations are summed when present. Advisor iterations are left out
    /// because they bill at a different rate. A <c>fallback_message</c> iteration keeps the top-level totals.
    /// </summary>
    public static LanguageModelUsage Convert(JsonElement usage, JsonElement? rawUsage = null)
    {
        var cacheWrite = NumberOrZero(usage, "cache_creation_input_tokens");
        var cacheRead = NumberOrZero(usage, "cache_read_input_tokens");
        var reasoning = ThinkingTokens(usage);
        var input = usage.TryGetProperty("input_tokens", out var inputValue) && inputValue.TryGetInt32(out var inputTokens) ? inputTokens : 0;
        var output = usage.TryGetProperty("output_tokens", out var outputValue) && outputValue.TryGetInt32(out var outputTokens) ? outputTokens : 0;

        if (usage.TryGetProperty("iterations", out var iterations) && iterations.ValueKind == JsonValueKind.Array && iterations.GetArrayLength() > 0 && !ServedByFallback(iterations))
        {
            var summedInput = 0;
            var summedOutput = 0;
            var executor = 0;
            foreach (var iteration in iterations.EnumerateArray())
            {
                var type = AnthropicJson.String(iteration, "type");
                if (type != "compaction" && type != "message")
                {
                    continue;
                }

                executor++;
                summedInput += NumberOrZero(iteration, "input_tokens");
                summedOutput += NumberOrZero(iteration, "output_tokens");
            }

            if (executor > 0)
            {
                input = summedInput;
                output = summedOutput;
            }
        }

        JsonElement raw;
        if (rawUsage is { } provided && provided.ValueKind != JsonValueKind.Undefined && provided.ValueKind != JsonValueKind.Null)
        {
            raw = provided.Clone();
        }
        else
        {
            raw = usage.Clone();
        }

        return new LanguageModelUsage(input + cacheWrite + cacheRead, output, null, cacheRead, cacheWrite, reasoning, raw);
    }

    private static bool ServedByFallback(JsonElement iterations)
    {
        foreach (var iteration in iterations.EnumerateArray())
        {
            if (AnthropicJson.String(iteration, "type") == "fallback_message")
            {
                return true;
            }
        }

        return false;
    }

    private static int NumberOrZero(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return 0;
        }

        return value.TryGetInt32(out var number) ? number : 0;
    }

    private static int? ThinkingTokens(JsonElement usage)
    {
        if (!usage.TryGetProperty("output_tokens_details", out var details) || details.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!details.TryGetProperty("thinking_tokens", out var thinking) || thinking.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return thinking.TryGetInt32(out var number) ? number : null;
    }
}
