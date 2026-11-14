// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Azure;

/// <summary>
/// Azure Speech text to speech options, read from the <c>azure</c> provider options. Maps to
/// <c>AzureSpeechModelSpeechOptions</c> in <c>azure-speech-speech-model-options.ts</c>.
/// </summary>
public sealed class AzureSpeechSpeechModelOptions
{
    private static readonly string[] ShapeKeys = ["style", "styleDegree"];

    /// <summary>
    /// Speaking style applied with <c>mstts:express-as</c>, for example <c>excited</c> or <c>whispering</c>.
    /// Supported styles vary by voice.
    /// </summary>
    public string? Style { get; set; }

    /// <summary>Intensity of <see cref="Style"/>, from 0.01 to 2. Azure defaults to 1.</summary>
    public double? StyleDegree { get; set; }

    /// <summary>The option keys. Maps to <c>azureSpeechSpeechModelOptionsShape</c>.</summary>
    public static IReadOnlyList<string> Shape()
    {
        return ShapeKeys;
    }
}
