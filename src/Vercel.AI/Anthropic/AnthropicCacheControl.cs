// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Reads <c>cache_control</c> and enforces Anthropic's four-breakpoint limit.</summary>
public sealed class AnthropicCacheControlValidator
{
    private const int MaxBreakpoints = 4;
    private readonly List<AnthropicWarning> _warnings = new List<AnthropicWarning>();
    private readonly string _providerOptionsName;
    private int _breakpoints;

    /// <summary>Creates a validator. <paramref name="providerOptionsName"/> is read before <c>anthropic</c>.</summary>
    public AnthropicCacheControlValidator(string providerOptionsName = "anthropic")
    {
        _providerOptionsName = providerOptionsName;
    }

    /// <summary>Warnings collected while reading cache controls.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings
    {
        get { return _warnings; }
    }

    /// <summary>Returns the cache control object when this context may cache it.</summary>
    public JsonObject? Get(JsonElement? providerMetadata, string contextType, bool canCache)
    {
        var value = Read(providerMetadata, _providerOptionsName);
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
        return Read(providerMetadata, "anthropic");
    }

    /// <summary>Reads the cache control from <paramref name="providerOptionsName"/>, falling back to the <c>anthropic</c> namespace.</summary>
    public static JsonObject? Read(JsonElement? providerMetadata, string providerOptionsName)
    {
        JsonElement? custom = null;
        if (providerOptionsName != "anthropic" && providerMetadata != null && providerMetadata.Value.ValueKind == JsonValueKind.Object)
        {
            custom = AnthropicJson.Property(providerMetadata.Value, providerOptionsName);
        }

        foreach (var namespaceOptions in new[] { custom, AnthropicJson.Anthropic(providerMetadata) })
        {
            if (namespaceOptions == null)
            {
                continue;
            }

            var value = AnthropicJson.Property(namespaceOptions.Value, "cacheControl", "cache_control");
            if (value != null && value.Value.ValueKind == JsonValueKind.Object)
            {
                return JsonNode.Parse(value.Value.GetRawText()) as JsonObject;
            }
        }

        return null;
    }
}
