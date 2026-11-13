// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Serialization;

namespace Vercel.AI.MiniMax;

/// <summary>Provider options for MiniMax image generation and editing. Pass them under <c>minimax</c> (upstream <c>MiniMaxImageModelOptions</c>).</summary>
public sealed class MiniMaxImageModelOptions
{
    /// <summary>Output areas accepted by <see cref="Area"/>.</summary>
    public static IReadOnlyList<string> MiniMaxImageAreas { get; } = new[] { "1K", "2K", "4K" };

    /// <summary>Output formats accepted by <see cref="OutputFormat"/>.</summary>
    public static IReadOnlyList<string> MiniMaxImageOutputFormats { get; } = new[] { "jpeg", "jpg", "png", "webp" };

    /// <summary>Output area when a pixel size is not supplied: <c>1K</c>, <c>2K</c>, or <c>4K</c>. Defaults to 4K.</summary>
    [JsonPropertyName("area")]
    public string? Area { get; set; }

    /// <summary>Disable prompt enhancement for text-to-image requests.</summary>
    [JsonPropertyName("disablePromptEnhanced")]
    public bool? DisablePromptEnhanced { get; set; }

    /// <summary>Compression quality for JPEG and WebP output, from 0 to 100.</summary>
    [JsonPropertyName("outputCompression")]
    public int? OutputCompression { get; set; }

    /// <summary>Generated image format: <c>jpeg</c>, <c>jpg</c>, <c>png</c>, or <c>webp</c>. Defaults to JPEG.</summary>
    [JsonPropertyName("outputFormat")]
    public string? OutputFormat { get; set; }
}
