// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Serialization;

namespace Vercel.AI.MiniMax;

/// <summary>Provider options for MiniMax video generation. Pass them under <c>minimax</c>.</summary>
public sealed class MiniMaxVideoModelOptions
{
    /// <summary>Aspect ratios supported by the MiniMax video models (<c>minimaxVideoRatios</c>).</summary>
    public static IReadOnlyList<string> MiniMaxVideoRatios { get; } = new[] { "adaptive", "21:9", "16:9", "4:3", "1:1", "3:4", "9:16" };

    /// <summary>Output resolutions supported by MiniMax video models (<c>minimaxVideoResolutions</c>). Availability depends on the selected model.</summary>
    public static IReadOnlyList<string> MiniMaxVideoResolutions { get; } = new[] { "480P", "768P", "2K" };

    /// <summary>Output resolution. One of <see cref="MiniMaxVideoResolutions"/>.</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    /// <summary>Aspect ratio of the generated video. One of <see cref="MiniMaxVideoRatios"/>. Overrides the top-level aspect ratio.</summary>
    [JsonPropertyName("ratio")]
    public string? Ratio { get; set; }

    /// <summary>Reference audio URLs for reference-to-video generation.</summary>
    [JsonPropertyName("referenceAudioUrls")]
    public IList<string>? ReferenceAudioUrls { get; set; }

    /// <summary>Whether to embed an AIGC watermark in the output. Defaults to <c>false</c>.</summary>
    [JsonPropertyName("aigcWatermark")]
    public bool? AigcWatermark { get; set; }

    /// <summary>Interval in milliseconds between task status polls. Default: 10000.</summary>
    [JsonPropertyName("pollIntervalMs")]
    public int? PollIntervalMs { get; set; }

    /// <summary>Maximum time in milliseconds to poll before timing out. Default: 600000.</summary>
    [JsonPropertyName("pollTimeoutMs")]
    public int? PollTimeoutMs { get; set; }
}
