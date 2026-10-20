// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>
/// Converts one file part for a provider that speaks the Messages protocol. Receives the part in the SDK message JSON shape.
/// Returns the image or video block without <c>cache_control</c>, or <see langword="null"/> to use the default conversion.
/// </summary>
public delegate JsonObject? AnthropicMediaPartConverter(JsonElement part);

/// <summary>Optional settings that let a Messages-compatible provider adjust the shared Anthropic adapter.</summary>
public sealed class AnthropicModelSettings
{
    /// <summary>Custom media conversion. <see langword="null"/> uses the default.</summary>
    public AnthropicMediaPartConverter? ConvertMediaPart { get; init; }

    /// <summary>Maximum temperature. Defaults to 1.</summary>
    public double? MaxTemperature { get; init; }

    /// <summary>Keeps <c>temperature</c> and <c>topP</c> when thinking is on.</summary>
    public bool SupportsTemperatureAndTopPWithThinking { get; init; }

    /// <summary>Provider-options namespace read for cache control before <c>anthropic</c>. Defaults to <c>anthropic</c>.</summary>
    public string? CacheControlProviderOptionsName { get; init; }
}
