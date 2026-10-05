// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Token usage converted from Gemini <c>usageMetadata</c>.</summary>
public sealed class GoogleTokenUsage
{
    /// <summary>Creates converted usage. <see cref="CacheWrite"/> stays null because Gemini does not report cache writes.</summary>
    public GoogleTokenUsage(int inputTotal, int noCache, int cacheRead, int outputTotal, int text, int reasoning, JsonElement? raw)
    {
        InputTotal = inputTotal;
        NoCache = noCache;
        CacheRead = cacheRead;
        OutputTotal = outputTotal;
        Text = text;
        Reasoning = reasoning;
        Raw = raw;
    }

    /// <summary>Prompt tokens plus tool-use prompt tokens.</summary>
    public int InputTotal { get; }

    /// <summary>Input tokens that were not served from cache.</summary>
    public int NoCache { get; }

    /// <summary>Cached content tokens.</summary>
    public int CacheRead { get; }

    /// <summary>Always null. Gemini does not report cache writes.</summary>
    public int? CacheWrite
    {
        get { return null; }
    }

    /// <summary>Candidate tokens plus thought tokens.</summary>
    public int OutputTotal { get; }

    /// <summary>Candidate tokens.</summary>
    public int Text { get; }

    /// <summary>Thought tokens.</summary>
    public int Reasoning { get; }

    /// <summary>The original usage object.</summary>
    public JsonElement? Raw { get; }

    /// <summary>Maps onto the shared usage type. Cache writes stay unset.</summary>
    public LanguageModelUsage ToLanguageModelUsage()
    {
        return new LanguageModelUsage(InputTotal, OutputTotal, InputTotal + OutputTotal, CacheRead, null, Reasoning, Raw, NoCache, Text);
    }
}

/// <summary>Converts Gemini <c>usageMetadata</c>.</summary>
public static class GoogleUsage
{
    /// <summary>
    /// Adds tool-use prompt tokens to the input total and thought tokens to the output total.
    /// Returns null when <paramref name="usage"/> is missing.
    /// </summary>
    public static GoogleTokenUsage? Convert(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var prompt = Count(element, "promptTokenCount");
        var candidates = Count(element, "candidatesTokenCount");
        var toolUse = Count(element, "toolUsePromptTokenCount");
        var cached = Count(element, "cachedContentTokenCount");
        var thoughts = Count(element, "thoughtsTokenCount");
        var input = prompt + toolUse;
        return new GoogleTokenUsage(input, input - cached, cached, candidates + thoughts, candidates, thoughts, element.Clone());
    }

    private static int Count(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return 0;
        }

        return value.TryGetInt32(out var count) ? count : 0;
    }
}
