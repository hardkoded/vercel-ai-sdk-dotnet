// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Normalized Converse token usage.</summary>
public sealed class AmazonBedrockUsageConversion
{
    /// <summary>Creates a conversion result.</summary>
    public AmazonBedrockUsageConversion(
        int? inputTotal,
        int? noCache,
        int? cacheRead,
        int? cacheWrite,
        int? outputTotal,
        int? outputText,
        int? outputReasoning,
        JsonElement? raw)
    {
        InputTotal = inputTotal;
        NoCache = noCache;
        CacheRead = cacheRead;
        CacheWrite = cacheWrite;
        OutputTotal = outputTotal;
        OutputText = outputText;
        OutputReasoning = outputReasoning;
        Raw = raw;
    }

    /// <summary>Input tokens including cache read and cache write.</summary>
    public int? InputTotal { get; }

    /// <summary>Input tokens that were not served from or written to cache.</summary>
    public int? NoCache { get; }

    /// <summary>Cache-read input tokens. Zero when the provider omitted them.</summary>
    public int? CacheRead { get; }

    /// <summary>Cache-write input tokens. Zero when the provider omitted them.</summary>
    public int? CacheWrite { get; }

    /// <summary>Output tokens.</summary>
    public int? OutputTotal { get; }

    /// <summary>Output tokens that were not reasoning tokens.</summary>
    public int? OutputText { get; }

    /// <summary>Reasoning tokens. Null when Bedrock does not report them.</summary>
    public int? OutputReasoning { get; }

    /// <summary>Provider usage object, unchanged.</summary>
    public JsonElement? Raw { get; }

    /// <summary>Maps this conversion onto <see cref="LanguageModelUsage"/>.</summary>
    public LanguageModelUsage ToLanguageModelUsage()
    {
        int? reportedTotal = null;
        if (Raw is { } raw && raw.ValueKind == JsonValueKind.Object && raw.TryGetProperty("totalTokens", out var total) && total.ValueKind == JsonValueKind.Number)
        {
            reportedTotal = total.GetInt32();
        }

        return new LanguageModelUsage(InputTotal, OutputTotal, reportedTotal, CacheRead, CacheWrite, OutputReasoning, Raw);
    }
}

/// <summary>Converts a Converse <c>usage</c> object.</summary>
public static class AmazonBedrockUsage
{
    /// <summary>
    /// Converts <paramref name="usage"/>. Null and undefined usage return null counts.
    /// Cache tokens that are missing or JSON null count as zero, and the raw object is preserved.
    /// </summary>
    public static AmazonBedrockUsageConversion Convert(JsonElement? usage)
    {
        if (usage == null || usage.Value.ValueKind == JsonValueKind.Null || usage.Value.ValueKind == JsonValueKind.Undefined)
        {
            return new AmazonBedrockUsageConversion(null, null, null, null, null, null, null, null);
        }

        var element = usage.Value;
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new AmazonBedrockResponseException("Invalid JSON response: \"usage\" must be an object.");
        }

        ValidateOptionalNumber(element, "inputTokens");
        ValidateOptionalNumber(element, "outputTokens");
        ValidateOptionalNumber(element, "totalTokens");
        ValidateOptionalNumber(element, "cacheReadInputTokens", allowNull: true);
        ValidateOptionalNumber(element, "cacheWriteInputTokens", allowNull: true);
        ValidateCacheDetails(element);

        var input = ReadInt(element, "inputTokens") ?? 0;
        var output = ReadInt(element, "outputTokens") ?? 0;
        var cacheRead = ReadNullableInt(element, "cacheReadInputTokens") ?? 0;
        var cacheWrite = ReadNullableInt(element, "cacheWriteInputTokens") ?? 0;
        return new AmazonBedrockUsageConversion(
            input + cacheRead + cacheWrite,
            input,
            cacheRead,
            cacheWrite,
            output,
            output,
            null,
            element.Clone());
    }

    /// <summary>Converts a usage JSON object.</summary>
    public static AmazonBedrockUsageConversion Convert(JsonObject? usage)
    {
        if (usage == null)
        {
            return Convert((JsonElement?)null);
        }

        using var document = JsonDocument.Parse(usage.ToJsonString());
        return Convert(document.RootElement);
    }

    private static void ValidateCacheDetails(JsonElement usage)
    {
        if (!usage.TryGetProperty("cacheDetails", out var details) || details.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (details.ValueKind != JsonValueKind.Array)
        {
            throw new AmazonBedrockResponseException("Invalid JSON response: \"cacheDetails\" must be an array.");
        }

        foreach (var item in details.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new AmazonBedrockResponseException("Invalid JSON response: \"cacheDetails\" entries must be objects.");
            }

            if (!item.TryGetProperty("inputTokens", out var tokens) || tokens.ValueKind != JsonValueKind.Number)
            {
                throw new AmazonBedrockResponseException("Invalid JSON response: \"inputTokens\" must be a number.");
            }

            if (!item.TryGetProperty("ttl", out var ttl) || ttl.ValueKind != JsonValueKind.String)
            {
                throw new AmazonBedrockResponseException("Invalid JSON response: \"ttl\" must be a string.");
            }
        }
    }

    private static void ValidateOptionalNumber(JsonElement usage, string name, bool allowNull = false)
    {
        if (!usage.TryGetProperty(name, out var value))
        {
            return;
        }

        if (allowNull && value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new AmazonBedrockResponseException("Invalid JSON response: \"" + name + "\" must be a number.");
        }
    }

    private static int? ReadInt(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.GetInt32();
    }

    private static int? ReadNullableInt(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new AmazonBedrockResponseException("Invalid JSON response: \"" + name + "\" must be a number.");
        }

        return value.GetInt32();
    }
}

/// <summary>A Converse response failed schema checks.</summary>
public sealed class AmazonBedrockResponseException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    public AmazonBedrockResponseException(string message)
        : base(message)
    {
    }
}
