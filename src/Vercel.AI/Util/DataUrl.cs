// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.RegularExpressions;

namespace Vercel.AI.Util;

/// <summary>Reads text from <c>text/*</c> data URLs.</summary>
public static class DataUrl
{
    private static readonly Regex CharsetPattern = new Regex(
        "(?:^|;)\\s*charset\\s*=\\s*(?:\"([^\"]+)\"|([^;\\s]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Decodes a <c>text/*</c> data URL. Without a charset the decoded bytes are returned as Latin-1 text.
    /// </summary>
    public static string GetTextFromDataUrl(string? dataUrl)
    {
        if (dataUrl == null)
        {
            throw new InvalidArgumentError("dataUrl", dataUrl, "Invalid data URL format");
        }

        var comma = dataUrl.IndexOf(',');
        var header = comma < 0 ? dataUrl : dataUrl.Substring(0, comma);
        string? base64Content = comma < 0 ? null : dataUrl.Substring(comma + 1);
        string? mediaType = header.Split(';')[0];
        var colon = mediaType.IndexOf(':');
        mediaType = colon < 0 ? null : mediaType.Substring(colon + 1);
        var charsetMatch = CharsetPattern.Match(header);
        string? charset = null;
        if (charsetMatch.Success)
        {
            charset = charsetMatch.Groups[1].Success ? charsetMatch.Groups[1].Value : charsetMatch.Groups[2].Value;
        }

        if (string.IsNullOrEmpty(mediaType) || base64Content == null)
        {
            throw new InvalidArgumentError("dataUrl", dataUrl, "Invalid data URL format");
        }

        try
        {
            var bytes = Convert.FromBase64String(base64Content);
            if (charset == null)
            {
                return Encoding.GetEncoding("iso-8859-1").GetString(bytes);
            }

            return Encoding.GetEncoding(charset).GetString(bytes);
        }
        catch (InvalidArgumentError)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidArgumentError("dataUrl", dataUrl, "Error decoding data URL");
        }
    }
}
