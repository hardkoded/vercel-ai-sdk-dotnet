// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>
/// Converts a Converse <c>usage</c> object. Bedrock <c>inputTokens</c> excludes cache tokens;
/// the V4 input total adds cache read and cache write.
/// </summary>
public static class AmazonBedrockUsage
{
    /// <summary>Converts <paramref name="usage"/>. A missing object leaves the token counts unset.</summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
        {
            return new LanguageModelUsage(null, null, null);
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var input = ReadInt(element, "inputTokens") ?? 0;
        var output = ReadInt(element, "outputTokens") ?? 0;
        var cacheRead = ReadCache(element, "cacheReadInputTokens");
        var cacheWrite = ReadCache(element, "cacheWriteInputTokens");
        int? total = element.TryGetProperty("totalTokens", out var totalElement) && totalElement.ValueKind == JsonValueKind.Number
            ? totalElement.GetInt32()
            : null;
        return new LanguageModelUsage(input + cacheRead + cacheWrite, output, total, cacheRead, cacheWrite, raw: element.Clone());
    }

    private static int ReadCache(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return 0;
        }

        return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : 0;
    }

    private static int? ReadInt(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.GetInt32();
    }
}
