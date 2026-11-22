// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Fal;

/// <summary>Maps to <c>requestConfig.video</c> of the Fal video model entry.</summary>
internal sealed class FalVideoRequestConfig
{
    /// <summary>The model id without its <c>fal-ai/</c> or <c>fal/</c> prefix. Maps to <c>normalizedModelId</c>.</summary>
    public string NormalizedModelId { get; init; } = "";
}

/// <summary>Maps to the Fal video model entry. Only <c>requestConfig</c> is ported.</summary>
internal sealed class FalVideoModelEntry
{
    /// <summary>Maps to <c>requestConfig.video</c>.</summary>
    public FalVideoRequestConfig Video { get; init; } = new();
}

/// <summary>Maps to <c>fal-model-catalog.ts</c>.</summary>
internal static class FalModelCatalog
{
    private static readonly Regex FalAiPrefix = new(
        "^fal-ai/",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );
    private static readonly Regex FalPrefix = new(
        "^fal/",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>Returns the entry of a video model. Maps to <c>getFalVideoModelEntry</c>.</summary>
    internal static FalVideoModelEntry GetFalVideoModelEntry(string modelId)
    {
        return new FalVideoModelEntry
        {
            Video = new FalVideoRequestConfig
            {
                NormalizedModelId = FalPrefix.Replace(FalAiPrefix.Replace(modelId, ""), ""),
            },
        };
    }
}
