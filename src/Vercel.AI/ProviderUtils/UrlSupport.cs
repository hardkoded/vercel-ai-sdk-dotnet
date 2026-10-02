// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// A URL pattern with JavaScript <c>RegExp</c> global and sticky state.
/// Maps to the regular expressions accepted by <c>isUrlSupported</c>.
/// </summary>
public sealed class UrlPattern
{
    private readonly Regex _regex;
    private readonly Func<string, bool>? _test;

    /// <summary>Creates a pattern.</summary>
    public UrlPattern(string pattern, bool global = false, bool sticky = false, Func<string, bool>? test = null)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Global = global;
        Sticky = sticky;
        _regex = new Regex(pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
        _test = test;
    }

    /// <summary>Source pattern.</summary>
    public string Pattern { get; }

    /// <summary>True when the pattern is global (<c>/g</c>).</summary>
    public bool Global { get; }

    /// <summary>True when the pattern is sticky (<c>/y</c>).</summary>
    public bool Sticky { get; }

    /// <summary>True when lastIndex must not be changed, matching <c>Object.freeze</c> on a non-stateful expression.</summary>
    public bool Frozen { get; set; }

    /// <summary>Caller-owned match index.</summary>
    public int LastIndex { get; set; }

    /// <summary>Tests <paramref name="value"/> and may update <see cref="LastIndex"/> when the override does.</summary>
    public bool Test(string value)
    {
        if (_test != null)
        {
            return _test(value);
        }

        return _regex.IsMatch(value);
    }
}

/// <summary>Checks native URL support. Maps to <c>isUrlSupported</c>.</summary>
public static class UrlSupport
{
    /// <summary>
    /// True when <paramref name="url"/> matches a pattern for <paramref name="mediaType"/>,
    /// a <c>type/*</c> prefix, or <c>*</c>.
    /// </summary>
    public static bool IsUrlSupported(string mediaType, string url, IReadOnlyDictionary<string, UrlPattern[]> supportedUrls)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        if (supportedUrls is null)
        {
            throw new ArgumentNullException(nameof(supportedUrls));
        }

        url = url.ToLowerInvariant();
        mediaType = mediaType.ToLowerInvariant();
        var topLevelOnly = mediaType.IndexOf('/') < 0;
        foreach (var pair in supportedUrls)
        {
            var key = pair.Key.ToLowerInvariant();
            var prefix = key == "*" || key == "*/*" ? string.Empty : key.Replace("*", string.Empty);
            var matchesType = prefix.Length == 0
                || (topLevelOnly ? mediaType + "/" == prefix : mediaType.StartsWith(prefix));
            if (!matchesType || pair.Value is null)
            {
                continue;
            }

            foreach (var pattern in pair.Value)
            {
                if (TestFromStart(pattern, url))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TestFromStart(UrlPattern pattern, string value)
    {
        if (!pattern.Global && !pattern.Sticky)
        {
            return pattern.Test(value);
        }

        var lastIndex = pattern.LastIndex;
        pattern.LastIndex = 0;
        try
        {
            return pattern.Test(value);
        }
        finally
        {
            pattern.LastIndex = lastIndex;
        }
    }
}
