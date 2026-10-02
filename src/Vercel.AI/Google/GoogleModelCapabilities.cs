// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>Which Gemini request features a model id accepts.</summary>
public sealed class GoogleModelCapabilities
{
    internal GoogleModelCapabilities(bool supportsGemini2Tools, bool supportsFileSearch, bool usesGemini3Features)
    {
        SupportsGemini2Tools = supportsGemini2Tools;
        SupportsFileSearch = supportsFileSearch;
        UsesGemini3Features = usesGemini3Features;
    }

    /// <summary>Gemini 2 tool surface, including Google Search and code execution.</summary>
    public bool SupportsGemini2Tools { get; }

    /// <summary>File Search tool.</summary>
    public bool SupportsFileSearch { get; }

    /// <summary>Gemini 3 request shape, including combined tools and thought-signature replay.</summary>
    public bool UsesGemini3Features { get; }
}

/// <summary>Classifies Gemini model ids the same way the upstream provider does.</summary>
public static class GoogleModelCapabilitiesMap
{
    private static readonly Regex Gemini1 = new Regex(@"(^|/)gemini-1(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex Gemini2 = new Regex(@"(^|/)gemini-2(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex Gemini25 = new Regex(@"(^|/)gemini-2\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex Gemini = new Regex(@"(^|/)gemini-", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex GeminiPro = new Regex(@"(^|/)gemini-pro(?:-vision)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex Robotics = new Regex(@"(^|/)gemini-robotics-er-1\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Classifies <paramref name="modelId"/>. Unknown Gemini ids inherit the newest behavior.</summary>
    public static GoogleModelCapabilities Get(string modelId)
    {
        var id = modelId ?? string.Empty;
        var isGemini = Gemini.IsMatch(id);
        var isGemini2 = Gemini2.IsMatch(id);
        var preGemini2 = Gemini1.IsMatch(id) || GeminiPro.IsMatch(id) || Robotics.IsMatch(id);
        var knownOlder = preGemini2 || isGemini2;
        var gemini3 = isGemini && !knownOlder;
        return new GoogleModelCapabilities(
            (isGemini && !preGemini2) || id.ToLowerInvariant().IndexOf("nano-banana", StringComparison.Ordinal) >= 0,
            Gemini25.IsMatch(id) || gemini3,
            gemini3);
    }
}
