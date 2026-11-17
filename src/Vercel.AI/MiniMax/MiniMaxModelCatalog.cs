// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.MiniMax;

/// <summary>The request settings of one MiniMax video model (upstream <c>getMiniMaxVideoModelEntry</c>).</summary>
internal sealed class MiniMaxVideoModelEntry
{
    /// <summary>Creates an entry.</summary>
    public MiniMaxVideoModelEntry(
        IReadOnlyList<string> supportedResolutions,
        string defaultResolution,
        int minDurationSeconds,
        bool referenceImage
    )
    {
        SupportedResolutions = supportedResolutions;
        DefaultResolution = defaultResolution;
        MinDurationSeconds = minDurationSeconds;
        ReferenceImage = referenceImage;
    }

    /// <summary>Resolution tiers the model serves (<c>requestConfig.video.supportedResolutions</c>).</summary>
    public IReadOnlyList<string> SupportedResolutions { get; }

    /// <summary>Tier used when none is requested (<c>requestConfig.video.defaultResolution</c>).</summary>
    public string DefaultResolution { get; }

    /// <summary>Shortest duration the API accepts (<c>requestConfig.video.minDurationSeconds</c>).</summary>
    public int MinDurationSeconds { get; }

    /// <summary>Whether the model takes reference inputs (<c>capabilities.video.inputs.referenceImage</c>).</summary>
    public bool ReferenceImage { get; }
}

/// <summary>MiniMax model catalog (upstream <c>minimax-model-catalog.ts</c>), limited to what the video model reads.</summary>
internal static class MiniMaxModelCatalog
{
    /// <summary>Maps to <c>getMiniMaxVideoModelEntry</c>.</summary>
    public static MiniMaxVideoModelEntry GetMiniMaxVideoModelEntry(string modelId)
    {
        var (supported, defaultResolution) = modelId switch
        {
            "MiniMax-H3" => (new[] { "768P", "2K" }, "2K"),
            "MiniMax-H3-Max" => (new[] { "480P", "768P" }, "768P"),
            _ => (MiniMaxVideoModelOptions.MiniMaxVideoResolutions.ToArray(), "2K"),
        };
        return new MiniMaxVideoModelEntry(
            supported,
            defaultResolution,
            modelId == "MiniMax-H3" ? 4 : 5,
            modelId != "MiniMax-H3-Max"
        );
    }
}
