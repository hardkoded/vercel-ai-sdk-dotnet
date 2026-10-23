// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.RegularExpressions;

namespace Vercel.AI.WebFetch;

internal enum WebFetchContentKind
{
    Html,
    Text,
    Unsupported,
}

/// <summary>Decides how to handle a response body and decodes it. Maps to <c>content-type.ts</c>.</summary>
internal static class WebFetchContentType
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

    private static readonly HashSet<string> TextMediaTypes = new(StringComparer.Ordinal)
    {
        "application/ecmascript",
        "application/javascript",
        "application/json",
        "application/ld+json",
        "application/toml",
        "application/x-javascript",
        "application/x-ndjson",
        "application/x-yaml",
        "application/xml",
        "application/yaml",
    };

    private static readonly Regex CharsetInContentType = new(@";\s*charset\s*=\s*""?([^"";\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout);

    private static readonly Regex CharsetInMetaTag = new(@"<meta[^>]+charset\s*=\s*[""']?\s*([\w.:-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout);

    public static string? GetMediaType(string? contentType)
    {
        var semicolon = contentType?.IndexOf(';') ?? -1;
        var mediaType = (semicolon < 0 ? contentType : contentType!.Substring(0, semicolon))?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(mediaType) ? null : mediaType;
    }

    /// <summary>Uses the Content-Type header and falls back to sniffing when the server omits it or sends a generic type.</summary>
    public static WebFetchContentKind Classify(string? mediaType, byte[] body)
    {
        // Without a specific type, sniff the content.
        if (mediaType == null || mediaType == "application/octet-stream")
        {
            return LooksLikeHtml(body) ? WebFetchContentKind.Html : WebFetchContentKind.Unsupported;
        }

        if (mediaType == "text/html" || mediaType == "application/xhtml+xml")
        {
            return WebFetchContentKind.Html;
        }

        if (mediaType.StartsWith("text/", StringComparison.Ordinal)
            || TextMediaTypes.Contains(mediaType)
            || mediaType.EndsWith("+json", StringComparison.Ordinal)
            || mediaType.EndsWith("+xml", StringComparison.Ordinal))
        {
            return WebFetchContentKind.Text;
        }

        return WebFetchContentKind.Unsupported;
    }

    /// <summary>
    /// Decodes a text response. The byte order mark takes precedence, then the Content-Type charset, then (for HTML) a
    /// <c>&lt;meta charset&gt;</c> declaration. Unknown or missing encodings fall back to UTF-8.
    /// </summary>
    public static string DecodeText(byte[] body, string? contentType, bool isHtml)
    {
        var bomLength = 0;
        var charset = GetCharsetFromBom(body, out bomLength)
            ?? GetCharsetFromContentType(contentType)
            ?? (isHtml ? GetCharsetFromMetaTag(body) : null)
            ?? "utf-8";

        Encoding encoding;
        try
        {
            encoding = GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            encoding = new UTF8Encoding(false);
            bomLength = body.Length >= 3 && body[0] == 0xef && body[1] == 0xbb && body[2] == 0xbf ? 3 : 0;
        }

        return encoding.GetString(body, bomLength, body.Length - bomLength);
    }

    private static bool LooksLikeHtml(byte[] body)
    {
        var start = new UTF8Encoding(false).GetString(body, 0, Math.Min(body.Length, 512)).TrimStart().ToLowerInvariant();
        return start.StartsWith("<!doctype html", StringComparison.Ordinal) || start.StartsWith("<html", StringComparison.Ordinal);
    }

    private static string? GetCharsetFromContentType(string? contentType)
    {
        if (contentType == null)
        {
            return null;
        }

        var match = CharsetInContentType.Match(contentType);
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    private static string? GetCharsetFromBom(byte[] body, out int bomLength)
    {
        bomLength = 0;
        if (body.Length >= 3 && body[0] == 0xef && body[1] == 0xbb && body[2] == 0xbf)
        {
            bomLength = 3;
            return "utf-8";
        }

        if (body.Length >= 2 && body[0] == 0xfe && body[1] == 0xff)
        {
            bomLength = 2;
            return "utf-16be";
        }

        if (body.Length >= 2 && body[0] == 0xff && body[1] == 0xfe)
        {
            bomLength = 2;
            return "utf-16le";
        }

        return null;
    }

    // Simplified version of the WHATWG encoding prescan of the first 1024 bytes.
    private static string? GetCharsetFromMetaTag(byte[] body)
    {
        var head = GetEncoding("windows-1252").GetString(body, 0, Math.Min(body.Length, 1024));
        var match = CharsetInMetaTag.Match(head);
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    private static Encoding GetEncoding(string charset)
    {
        EncodingProviderRegistration.Ensure();

        // The WHATWG encoding standard decodes the Latin-1 labels as windows-1252.
        if (charset == "iso-8859-1" || charset == "latin1" || charset == "us-ascii" || charset == "ascii")
        {
            charset = "windows-1252";
        }

        if (charset == "utf-8" || charset == "utf8")
        {
            return new UTF8Encoding(false);
        }

        return Encoding.GetEncoding(charset, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
    }

    private static class EncodingProviderRegistration
    {
        private static readonly object Gate = new();
        private static bool registered;

        public static void Ensure()
        {
            if (registered)
            {
                return;
            }

            lock (Gate)
            {
                if (!registered)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    registered = true;
                }
            }
        }
    }
}
