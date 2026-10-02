// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.RegularExpressions;

namespace Vercel.AI.Util;

/// <summary>Data URL helpers. Maps to <c>getTextFromDataUrl</c>.</summary>
public static class DataUrls
{
    private static readonly Regex CharsetPattern = new Regex(
        "(?:^|;)\\s*charset\\s*=\\s*(?:\"([^\"]+)\"|([^;\\s]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Decodes a <c>text/*</c> data URL. Without a charset the bytes are read as Latin-1,
    /// matching <c>atob</c>. A declared charset is used to decode the bytes.
    /// </summary>
    /// <exception cref="InvalidArgumentException">The URL is malformed or cannot be decoded.</exception>
    public static string GetTextFromDataUrl(string dataUrl)
    {
        if (dataUrl is null)
        {
            throw new ArgumentNullException(nameof(dataUrl));
        }

        var comma = dataUrl.IndexOf(',');
        var header = comma >= 0 ? dataUrl.Substring(0, comma) : dataUrl;
        string? payload = comma >= 0 ? dataUrl.Substring(comma + 1) : null;
        var mediaTypePart = header.Split(';')[0];
        var colon = mediaTypePart.IndexOf(':');
        string? mediaType = colon >= 0 ? mediaTypePart.Substring(colon + 1) : null;
        if (mediaType is null || payload is null)
        {
            throw new InvalidArgumentException(dataUrlParameter, dataUrl, "Invalid data URL format");
        }

        var charsetMatch = CharsetPattern.Match(header);
        string? charset = null;
        if (charsetMatch.Success)
        {
            charset = charsetMatch.Groups[1].Success
                ? charsetMatch.Groups[1].Value
                : charsetMatch.Groups[2].Value;
        }

        try
        {
            var bytes = Convert.FromBase64String(payload);
            if (charset is null)
            {
                return Encoding.GetEncoding("iso-8859-1").GetString(bytes);
            }

            return Encoding.GetEncoding(charset).GetString(bytes);
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is NotSupportedException)
        {
            throw new InvalidArgumentException(dataUrlParameter, dataUrl, "Error decoding data URL");
        }
    }

    private const string dataUrlParameter = "dataUrl";
}
