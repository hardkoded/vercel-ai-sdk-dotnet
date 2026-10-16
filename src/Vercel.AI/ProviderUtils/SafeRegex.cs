// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>Runs a regex on outside text. A match timeout counts as no match.</summary>
internal static class SafeRegex
{
    /// <summary>Match timeout for regexes that run on large outside text, such as an image or a model response.</summary>
    internal static readonly TimeSpan LargeInputTimeout = TimeSpan.FromSeconds(5);

    internal static bool IsMatch(Regex regex, string input)
    {
        try
        {
            return regex.IsMatch(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    internal static Match Match(Regex regex, string input)
    {
        try
        {
            return regex.Match(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return System.Text.RegularExpressions.Match.Empty;
        }
    }
}
