// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>Checks whether a model can read a URL natively. Maps to <c>isUrlSupported</c>.</summary>
public static class UrlSupport
{
    /// <summary>A model that supports no URLs, such as <c>supportedUrls: {}</c> upstream.</summary>
    public static readonly IReadOnlyDictionary<string, string> None = new Dictionary<string, string>();

    /// <summary>
    /// True when a pattern in <paramref name="supportedUrls"/> (regular expression text per media type) matches <paramref name="url"/>.
    /// See <see cref="IsUrlSupported(string, string, IReadOnlyDictionary{string, IReadOnlyList{Regex}})"/>.
    /// </summary>
    public static bool IsUrlSupported(string mediaType, string url, IReadOnlyDictionary<string, string> supportedUrls)
    {
        if (supportedUrls is null)
        {
            throw new ArgumentNullException(nameof(supportedUrls));
        }

        var compiled = new Dictionary<string, IReadOnlyList<Regex>>(StringComparer.Ordinal);
        foreach (var pair in supportedUrls)
        {
            compiled[pair.Key] = new[] { new Regex(pair.Value, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)) };
        }

        return IsUrlSupported(mediaType, url, compiled);
    }

    /// <summary>
    /// True when a pattern registered for <paramref name="mediaType"/>, its <c>type/*</c> prefix, or <c>*</c> matches <paramref name="url"/>.
    /// The media type is compared in lower case; the URL keeps its case, so patterns decide case sensitivity. A top-level-only media type such as <c>image</c> only matches <c>image/*</c>.
    /// </summary>
    public static bool IsUrlSupported(string mediaType, string url, IReadOnlyDictionary<string, IReadOnlyList<Regex>> supportedUrls)
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

        mediaType = mediaType.ToLowerInvariant();
        var topLevelOnly = mediaType.IndexOf('/') < 0;
        foreach (var pair in supportedUrls)
        {
            var key = pair.Key.ToLowerInvariant();
            var prefix = key == "*" || key == "*/*" ? string.Empty : RemoveFirstStar(key);
            var matchesType = prefix.Length == 0
                || (topLevelOnly ? mediaType + "/" == prefix : mediaType.StartsWith(prefix, StringComparison.Ordinal));
            if (!matchesType || pair.Value is null)
            {
                continue;
            }

            foreach (var pattern in pair.Value)
            {
                if (SafeRegex.IsMatch(pattern, url))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string RemoveFirstStar(string value)
    {
        var star = value.IndexOf('*');
        return star < 0 ? value : value.Remove(star, 1);
    }
}
