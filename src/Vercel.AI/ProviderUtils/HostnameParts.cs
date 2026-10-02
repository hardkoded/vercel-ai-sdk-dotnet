// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>Checks a value before it is concatenated into a generated host.</summary>
public static class HostnameParts
{
    // One ASCII DNS label. `$` can succeed before a trailing newline, so the match must cover the whole value.
    private const string SingleLabelPattern = @"^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$";

    /// <summary>
    /// Returns whether <paramref name="value"/> is one DNS label: 1–63 letters, digits, or hyphens, with no leading or trailing hyphen.
    /// </summary>
    public static bool IsValidHostnamePart(string? value)
    {
        if (value is null)
        {
            return false;
        }

        var match = Regex.Match(
            value,
            SingleLabelPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));
        return match.Success && match.Value == value;
    }
}
