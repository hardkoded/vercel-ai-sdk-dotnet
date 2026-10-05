// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>URLs Gemini can read without downloading the bytes.</summary>
public static class GoogleFileUrls
{
    private static readonly Regex[] YouTube =
    {
        new(@"^https://(?:www\.)?youtube\.com/watch\?v=[\w-]+(?:&[\w=&.-]*)?$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)),
        new(@"^https://youtu\.be/[\w-]+(?:\?[\w=&.-]*)?$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)),
    };

    /// <summary>True for Generative Language file URLs and public YouTube watch URLs.</summary>
    public static bool IsSupported(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (url!.StartsWith("https://generativelanguage.googleapis.com/v1beta/files/", StringComparison.Ordinal))
        {
            return true;
        }

        for (var i = 0; i < YouTube.Length; i++)
        {
            if (YouTube[i].IsMatch(url))
            {
                return true;
            }
        }

        return false;
    }
}
