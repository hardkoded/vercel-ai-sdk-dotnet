// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Alibaba;

/// <summary>Reads <c>cacheControl</c> from <c>alibaba</c> provider options and counts breakpoints. Maps to <c>CacheControlValidator</c>.</summary>
internal sealed class AlibabaCacheControlValidator
{
    // Alibaba allows a maximum of 4 cache breakpoints per request
    private const int MaxCacheBreakpoints = 4;

    private readonly List<CallWarning> _warnings = new List<CallWarning>();
    private int _breakpointCount;

    /// <summary>Warnings collected while reading cache controls. Maps to <c>getWarnings</c>.</summary>
    public IReadOnlyList<CallWarning> Warnings => _warnings;

    /// <summary>Returns the cache control of <paramref name="providerOptions"/>, or null when there is none. Maps to <c>getCacheControl</c>.</summary>
    public JsonNode? GetCacheControl(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var value = Read(providerOptions);
        if (value == null)
        {
            return null;
        }

        _breakpointCount++;
        if (_breakpointCount > MaxCacheBreakpoints)
        {
            _warnings.Add(new CallWarning(
                "other",
                "Max breakpoint limit exceeded. Only the last " + MaxCacheBreakpoints + " cache markers will take effect."));
        }

        return value;
    }

    // The value passes through as is. The Alibaba API validates it.
    private static JsonNode? Read(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions == null
            || !providerOptions.TryGetValue(AlibabaProvider.ProviderId, out var alibaba)
            || alibaba.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!alibaba.TryGetProperty("cacheControl", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            if (!alibaba.TryGetProperty("cache_control", out value))
            {
                return null;
            }
        }

        return IsFalsy(value) ? null : JsonNode.Parse(value.GetRawText());
    }

    private static bool IsFalsy(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null or JsonValueKind.False => true,
            JsonValueKind.String => value.GetString()!.Length == 0,
            JsonValueKind.Number => value.GetDouble() == 0,
            _ => false,
        };
    }
}
