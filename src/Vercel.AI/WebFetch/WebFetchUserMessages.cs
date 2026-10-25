// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.WebFetch;

/// <summary>Checks whether a URL appears in a user message. Maps to <c>is-url-in-user-messages.ts</c>.</summary>
internal static class WebFetchUserMessages
{
    private static readonly Regex UrlPattern = new(@"https?://[^\s<>""'`]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Only user messages count. Assistant messages, tool calls, and tool results are excluded because the model controls
    /// their content, directly or through tool inputs that are reflected in outputs. Otherwise, a model could make a URL
    /// fetchable by first writing it, e.g. a URL that encodes private data in its query string.
    /// </summary>
    public static bool IsUrlInUserMessages(string url, IReadOnlyList<ModelMessage> messages)
    {
        var texts = new List<string>();
        foreach (var message in messages)
        {
            if (message is not UserModelMessage user)
            {
                continue;
            }

            foreach (var part in user.Content)
            {
                if (part is TextContentPart text)
                {
                    texts.Add(text.Text);
                }
            }
        }

        var requested = NormalizeUrl(url);
        if (requested == null)
        {
            return false;
        }

        // Compare whole parsed URLs, not substrings: a substring check would let `https://example.com` match
        // `https://example.com.attacker.org/`.
        foreach (var text in texts)
        {
            foreach (var candidate in ExtractUrls(text))
            {
                if (string.Equals(NormalizeUrl(candidate), requested, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Parses a URL and removes the fragment, which is not sent to the server.</summary>
    internal static string? NormalizeUrl(string url)
    {
        Uri parsed;
        if (!Uri.TryCreate(url, UriKind.Absolute, out parsed!))
        {
            return null;
        }

        return new Uri(parsed.GetLeftPart(UriPartial.Query)).AbsoluteUri;
    }

    // Finds http(s) URLs in text. Punctuation at the end of a match, as in "see https://example.com/docs.", is also tried without it.
    private static IEnumerable<string> ExtractUrls(string text)
    {
        foreach (Match found in UrlPattern.Matches(text))
        {
            var match = found.Value;
            yield return match;

            var end = match.Length;
            while (end > 0 && ".,;:!?)]}".IndexOf(match[end - 1]) >= 0)
            {
                end--;
            }

            if (end < match.Length)
            {
                yield return match.Substring(0, end);
            }
        }
    }
}
