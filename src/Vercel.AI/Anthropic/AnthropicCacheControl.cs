// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Reads <c>cache_control</c> and enforces Anthropic's four-breakpoint limit.</summary>
public sealed class AnthropicCacheControlValidator
{
    private const int MaxBreakpoints = 4;
    private readonly List<AnthropicWarning> _warnings = new List<AnthropicWarning>();
    private int _breakpoints;

    /// <summary>Warnings collected while reading cache controls.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings
    {
        get { return _warnings; }
    }

    /// <summary>Returns the cache control object when this context may cache it.</summary>
    public JsonObject? Get(JsonElement? providerMetadata, string contextType, bool canCache)
    {
        var value = Read(providerMetadata);
        if (value == null)
        {
            return null;
        }

        if (!canCache)
        {
            _warnings.Add(new AnthropicWarning(
                "unsupported",
                "cache_control on non-cacheable context",
                "cache_control cannot be set on " + contextType + ". It will be ignored."));
            return null;
        }

        _breakpoints++;
        if (_breakpoints > MaxBreakpoints)
        {
            _warnings.Add(new AnthropicWarning(
                "unsupported",
                "cacheControl breakpoint limit",
                "Maximum " + MaxBreakpoints + " cache breakpoints exceeded (found " + _breakpoints + "). This breakpoint will be ignored."));
            return null;
        }

        return value;
    }

    /// <summary>Reads <c>cacheControl</c> or <c>cache_control</c> from Anthropic provider metadata.</summary>
    public static JsonObject? Read(JsonElement? providerMetadata)
    {
        var anthropic = AnthropicJson.Anthropic(providerMetadata);
        if (anthropic == null)
        {
            return null;
        }

        var value = AnthropicJson.Property(anthropic.Value, "cacheControl", "cache_control");
        if (value == null || value.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return JsonNode.Parse(value.Value.GetRawText()) as JsonObject;
    }
}
