// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>
/// Groq token usage. Cache writes are never reported. Text tokens equal completion tokens
/// when reasoning tokens are absent, and uncached input equals the prompt when cache reads are absent.
/// </summary>
public sealed class GroqUsage
{
    private GroqUsage(
        int? inputTotal,
        int? noCache,
        int? cacheRead,
        int? outputTotal,
        int? text,
        int? reasoning,
        JsonElement? raw)
    {
        InputTotal = inputTotal;
        NoCache = noCache;
        CacheRead = cacheRead;
        OutputTotal = outputTotal;
        Text = text;
        Reasoning = reasoning;
        Raw = raw;
    }

    /// <summary>Prompt tokens. Null when the provider sent no usage object.</summary>
    public int? InputTotal { get; }

    /// <summary>Prompt tokens that were not read from cache.</summary>
    public int? NoCache { get; }

    /// <summary>Cached prompt tokens, including zero. Null when the provider omitted them.</summary>
    public int? CacheRead { get; }

    /// <summary>Always null. Groq does not report cache writes.</summary>
    public int? CacheWrite
    {
        get { return null; }
    }

    /// <summary>Completion tokens. Null when the provider sent no usage object.</summary>
    public int? OutputTotal { get; }

    /// <summary>Completion tokens that were not reasoning tokens.</summary>
    public int? Text { get; }

    /// <summary>Reasoning tokens, including zero. Null when the provider omitted them.</summary>
    public int? Reasoning { get; }

    /// <summary>Provider usage object, unchanged. Null when usage was null.</summary>
    public JsonElement? Raw { get; }

    /// <summary>Converts a Groq usage object. A null element matches a missing usage payload.</summary>
    public static GroqUsage Convert(JsonElement? usage)
    {
        if (usage is null || usage.Value.ValueKind == JsonValueKind.Null || usage.Value.ValueKind == JsonValueKind.Undefined)
        {
            return new GroqUsage(null, null, null, null, null, null, null);
        }

        var element = usage.Value;
        var prompt = ReadInt(element, "prompt_tokens") ?? 0;
        var completion = ReadInt(element, "completion_tokens") ?? 0;
        int? cacheRead = null;
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("prompt_tokens_details", out var promptDetails)
            && promptDetails.ValueKind == JsonValueKind.Object
            && HasNumber(promptDetails, "cached_tokens", out var cached))
        {
            cacheRead = cached;
        }

        int? reasoning = null;
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("completion_tokens_details", out var completionDetails)
            && completionDetails.ValueKind == JsonValueKind.Object
            && HasNumber(completionDetails, "reasoning_tokens", out var reasoningTokens))
        {
            reasoning = reasoningTokens;
        }

        var noCache = cacheRead is null ? prompt : prompt - cacheRead.Value;
        var text = reasoning is null ? completion : Math.Max(0, completion - reasoning.Value);
        return new GroqUsage(prompt, noCache, cacheRead, completion, text, reasoning, element.Clone());
    }

    /// <summary>Projects the counts that <see cref="LanguageModelUsage"/> can store.</summary>
    public LanguageModelUsage ToLanguageModelUsage()
    {
        return new LanguageModelUsage(InputTotal, OutputTotal, null, CacheRead, null, Reasoning, Raw);
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        if (HasNumber(element, name, out var number))
        {
            return number;
        }

        return null;
    }

    private static bool HasNumber(JsonElement element, string name, out int number)
    {
        number = 0;
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out number))
        {
            return true;
        }

        return false;
    }
}
