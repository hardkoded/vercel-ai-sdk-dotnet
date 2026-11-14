// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenAI;

/// <summary>
/// Option names of the OpenAI image API. Routers that also call the OpenAI image API (Azure) accept these options under their own
/// provider options key. Maps to <c>openaiImageModelGenerationOptionsShape</c> and <c>openaiImageModelEditOptionsShape</c> in
/// <c>openai-image-model-options.ts</c>.
/// </summary>
internal static class OpenAIImageModelOptions
{
    // Shared by generations and edits: the base object of the upstream shape.
    private static readonly string[] BaseKeys =
    {
        "quality", "background", "outputFormat", "complexity", "outputCompression", "user", "safetyIdentifier",
    };

    private static readonly string[] GenerationKeys = BaseKeys.Concat(new[] { "style", "frameCount", "frameDurationMs", "moderation" }).ToArray();

    private static readonly string[] EditKeys = BaseKeys.Concat(new[] { "inputFidelity", "markup", "moderation" }).ToArray();

    /// <summary>Keys of the image generation options. Maps to <c>openaiImageModelGenerationOptionsShape</c>.</summary>
    public static IReadOnlyList<string> GenerationOptionsShape()
    {
        return GenerationKeys;
    }

    /// <summary>Keys of the image edit options. Maps to <c>openaiImageModelEditOptionsShape</c>.</summary>
    public static IReadOnlyList<string> EditOptionsShape()
    {
        return EditKeys;
    }
}
