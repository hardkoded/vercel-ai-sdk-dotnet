// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>Which Gemini request features a model id accepts.</summary>
public sealed class GoogleModelCapabilities
{
    /// <summary>Creates a capability set.</summary>
    public GoogleModelCapabilities(bool supportsGemini2Tools, bool supportsFileSearch, bool usesGemini3Features)
    {
        SupportsGemini2Tools = supportsGemini2Tools;
        SupportsFileSearch = supportsFileSearch;
        UsesGemini3Features = usesGemini3Features;
    }

    /// <summary>Google Search, URL context, code execution, and similar tools.</summary>
    public bool SupportsGemini2Tools { get; }

    /// <summary>File Search tool.</summary>
    public bool SupportsFileSearch { get; }

    /// <summary>Gemini 3 request shape, including combined tools and thought-signature handling.</summary>
    public bool UsesGemini3Features { get; }
}

/// <summary>Classifies Gemini model ids. Unknown future Gemini ids inherit the newest behavior.</summary>
public static class GoogleModelCapability
{
    private static readonly Regex Gemini1 = new(@"(^|/)gemini-1(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Gemini2 = new(@"(^|/)gemini-2(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Gemini25 = new(@"(^|/)gemini-2\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Gemini = new(@"(^|/)gemini-", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex GeminiPro = new(@"(^|/)gemini-pro(?:-vision)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Robotics = new(@"(^|/)gemini-robotics-er-1\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>Classifies <paramref name="modelId"/>.</summary>
    public static GoogleModelCapabilities Get(string modelId)
    {
        var id = modelId ?? string.Empty;
        var isGemini = Gemini.IsMatch(id);
        var isGemini2 = Gemini2.IsMatch(id);
        var knownOlder = IsKnownPreGemini2(id) || isGemini2;
        var usesGemini3 = isGemini && !knownOlder;
        var gemini2Tools = (isGemini && !IsKnownPreGemini2(id)) || id.IndexOf("nano-banana", StringComparison.OrdinalIgnoreCase) >= 0;
        return new GoogleModelCapabilities(gemini2Tools, Gemini25.IsMatch(id) || usesGemini3, usesGemini3);
    }

    /// <summary>True when the id is a Gemma model.</summary>
    public static bool IsGemma(string modelId)
    {
        return modelId != null && modelId.StartsWith("gemma-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnownPreGemini2(string modelId)
    {
        return Gemini1.IsMatch(modelId) || GeminiPro.IsMatch(modelId) || Robotics.IsMatch(modelId);
    }
}
