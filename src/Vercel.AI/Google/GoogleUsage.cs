// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
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

    /// <summary>
    /// Sums the usage metadata of two requests, such as the hops of a long decode response that was continued automatically.
    /// Numeric counts add, a missing count is the other side, and other fields come from <paramref name="b"/>.
    /// </summary>
    public static JsonElement? Add(JsonElement? a, JsonElement? b)
    {
        var left = IsObject(a) ? a!.Value : (JsonElement?)null;
        var right = IsObject(b) ? b!.Value : (JsonElement?)null;
        if (left == null || right == null)
        {
            return right ?? left;
        }

        var sum = new JsonObject();
        foreach (var property in left.Value.EnumerateObject())
        {
            sum[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        }

        foreach (var property in right.Value.EnumerateObject())
        {
            sum[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        }

        foreach (var name in CountFields)
        {
            var total = AddCounts(left.Value, right.Value, name);
            if (total != null)
            {
                sum[name] = total;
            }
        }

        foreach (var name in DetailFields)
        {
            var total = AddDetails(left.Value, right.Value, name);
            if (total != null)
            {
                sum[name] = total;
            }
        }

        return JsonSerializer.SerializeToElement(sum);
    }

    private static readonly string[] CountFields =
    {
        "promptTokenCount",
        "candidatesTokenCount",
        "toolUsePromptTokenCount",
        "totalTokenCount",
        "cachedContentTokenCount",
        "thoughtsTokenCount",
    };

    private static readonly string[] DetailFields =
    {
        "promptTokensDetails",
        "cacheTokensDetails",
        "candidatesTokensDetails",
        "toolUsePromptTokensDetails",
    };

    private static bool IsObject(JsonElement? element)
    {
        return element is { ValueKind: JsonValueKind.Object };
    }

    private static long? Number(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var count) ? count : null;
    }

    private static JsonNode? AddCounts(JsonElement a, JsonElement b, string name)
    {
        var left = Number(a, name);
        var right = Number(b, name);
        if (left == null && right == null)
        {
            return null;
        }

        return JsonValue.Create((left ?? 0) + (right ?? 0));
    }

    private static JsonNode? AddDetails(JsonElement a, JsonElement b, string name)
    {
        var hasLeft = a.TryGetProperty(name, out var left) && left.ValueKind == JsonValueKind.Array;
        var hasRight = b.TryGetProperty(name, out var right) && right.ValueKind == JsonValueKind.Array;
        if (!hasLeft || !hasRight)
        {
            return null;
        }

        var totals = new List<KeyValuePair<string, long>>();
        foreach (var item in left.EnumerateArray().Concat(right.EnumerateArray()))
        {
            var modality = item.TryGetProperty("modality", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() ?? string.Empty : string.Empty;
            var count = Number(item, "tokenCount") ?? 0;
            var index = totals.FindIndex(entry => entry.Key == modality);
            if (index < 0)
            {
                totals.Add(new KeyValuePair<string, long>(modality, count));
            }
            else
            {
                totals[index] = new KeyValuePair<string, long>(modality, totals[index].Value + count);
            }
        }

        var result = new JsonArray();
        foreach (var entry in totals)
        {
            result.Add(new JsonObject { ["modality"] = entry.Key, ["tokenCount"] = entry.Value });
        }

        return result;
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
