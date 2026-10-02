// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>
/// Tracks Anthropic prompt-cache breakpoints. At most four breakpoints are sent;
/// later breakpoints and cache control on non-cacheable blocks are dropped.
/// </summary>
public sealed class AnthropicCacheControlValidator
{
    private const int MaxBreakpoints = 4;

    private readonly List<AnthropicWarning> _warnings = new List<AnthropicWarning>();

    private int _breakpointCount;

    /// <summary>Warnings produced while reading cache control.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings
    {
        get { return _warnings; }
    }

    /// <summary>Reads cache control for one block, or returns null when it must be ignored.</summary>
    public JsonNode? GetCacheControl(JsonElement providerOptions, string contextType, bool canCache)
    {
        var cacheControl = AnthropicJson.CacheControl(providerOptions);
        if (cacheControl == null)
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

        _breakpointCount++;
        if (_breakpointCount > MaxBreakpoints)
        {
            _warnings.Add(new AnthropicWarning(
                "unsupported",
                "cacheControl breakpoint limit",
                "Maximum " + MaxBreakpoints + " cache breakpoints exceeded (found " + _breakpointCount + "). This breakpoint will be ignored."));
            return null;
        }

        return cacheControl;
    }
}
