// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenAI;

/// <summary>
/// OpenAI speech model options, read from the <c>openai</c> provider options. Routers that also call the OpenAI
/// speech API (Azure) accept the same options under their own key. Maps to <c>OpenAISpeechModelOptions</c>.
/// </summary>
public sealed class OpenAISpeechModelOptions
{
    private static readonly string[] ShapeKeys = ["instructions", "speed"];

    /// <summary>Control the voice of the generated audio, for example <c>Speak in a slow and steady tone</c>.</summary>
    public string? Instructions { get; set; }

    /// <summary>Speed of the generated audio, from 0.25 to 4.0.</summary>
    public double? Speed { get; set; }

    /// <summary>The option keys. Maps to <c>openaiSpeechModelOptionsShape</c>.</summary>
    public static IReadOnlyList<string> Shape()
    {
        return ShapeKeys;
    }
}
