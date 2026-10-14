// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.QuiverAI;

/// <summary>
/// A reference image accepted by <c>providerOptions.quiverai.referenceImages</c>: <c>{ "url" }</c> or <c>{ "base64" }</c>.
/// Exactly one of <see cref="Url"/> and <see cref="Base64"/> is set. Serializes to that JSON shape.
/// </summary>
public sealed class QuiverAIImageReference
{
    private const int MaxReferenceBase64Length = 16_777_216;
    private const int MaxReferenceBytes = 12_582_912;

    private static readonly HashSet<string> SupportedMediaTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "image/gif",
        "image/jpeg",
        "image/png",
        "image/svg+xml",
        "image/webp",
    };

    private static readonly Regex Scheme = new Regex("^[a-z][a-z\\d+.-]*://", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex DataUrl = new Regex("^data:([^;,]+);base64,(.+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static readonly Regex SvgStart = new Regex("^(?:<\\?xml[\\s\\S]*?\\?>\\s*)?(?:<!--[\\s\\S]*?-->\\s*)?(?:<!DOCTYPE[\\s\\S]*?>\\s*)?<svg[\\s>]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex SvgClose = new Regex("</svg>\\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex SvgSelfClose = new Regex("<svg(?:\\s[^>]*)?/>\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private QuiverAIImageReference(string? url, string? base64)
    {
        Url = url;
        Base64 = base64;
    }

    /// <summary>HTTP or HTTPS image URL, or <see langword="null"/> for a base64 reference.</summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; }

    /// <summary>Base64-encoded image data without a data URL prefix, or <see langword="null"/> for a URL reference.</summary>
    [JsonPropertyName("base64")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Base64 { get; }

    /// <summary>Returns a URL reference for an HTTP or HTTPS URL.</summary>
    public static QuiverAIImageReference Prepare(Uri input)
    {
        return new QuiverAIImageReference(ValidateImageUrl(input.ToString()), null);
    }

    /// <summary>
    /// Returns a URL reference for a URL, or a base64 reference for a base64 string or a base64 data URL.
    /// Throws <see cref="InvalidArgumentException"/> for other schemes, invalid base64, or unsupported images.
    /// </summary>
    public static QuiverAIImageReference Prepare(string input)
    {
        if (Scheme.IsMatch(input))
        {
            return new QuiverAIImageReference(ValidateImageUrl(input), null);
        }

        if (input.StartsWith("data:", StringComparison.Ordinal))
        {
            var match = DataUrl.Match(input);
            if (!match.Success || !SupportedMediaTypes.Contains(match.Groups[1].Value))
            {
                throw new InvalidArgumentException("input", input, "QuiverAI reference image data URLs must use base64 encoding and a supported image media type.");
            }

            ValidateReferenceBase64(match.Groups[2].Value, "input");
            return new QuiverAIImageReference(null, match.Groups[2].Value);
        }

        ValidateReferenceBase64(input, "input");
        return new QuiverAIImageReference(null, input);
    }

    /// <summary>Returns a base64 reference for PNG, JPEG, WebP, GIF, or SVG bytes.</summary>
    public static QuiverAIImageReference Prepare(byte[] input)
    {
        ValidateReferenceBytes(input, "input");
        return new QuiverAIImageReference(null, Convert.ToBase64String(input));
    }

    internal static string ValidateImageUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            throw new InvalidArgumentException("url", url, "QuiverAI image URLs must be valid HTTP or HTTPS URLs.");
        }

        if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidArgumentException("url", url, "QuiverAI image URLs must use HTTP or HTTPS.");
        }

        return parsed.AbsoluteUri;
    }

    internal static void ValidateReferenceBase64(string base64, string argument)
    {
        if (base64.Length == 0 || base64.Length > MaxReferenceBase64Length)
        {
            throw new InvalidArgumentException(argument, base64, "QuiverAI reference images must contain 1-" + MaxReferenceBase64Length + " base64 characters.");
        }

        byte[] data;
        try
        {
            data = ByteEncoding.FromBase64(base64);
        }
        catch (FormatException)
        {
            throw new InvalidArgumentException(argument, base64, "QuiverAI reference image data must be valid base64.");
        }

        ValidateReferenceBytes(data, argument);
    }

    private static void ValidateReferenceBytes(byte[] data, string argument)
    {
        if (data.Length == 0 || data.Length > MaxReferenceBytes)
        {
            throw new InvalidArgumentException(argument, null, "QuiverAI reference images must decode to 1-" + MaxReferenceBytes + " bytes.");
        }

        var mediaType = IsSvg(data) ? "image/svg+xml" : MediaTypes.DetectMediaType(data, "image");
        if (mediaType == null || !SupportedMediaTypes.Contains(mediaType))
        {
            throw new InvalidArgumentException(argument, null, "QuiverAI reference images must be PNG, JPEG, WebP, GIF, or SVG data.");
        }
    }

    private static bool IsSvg(byte[] data)
    {
        string text;
        try
        {
            text = new UTF8Encoding(false, true).GetString(data);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        var normalized = (text.StartsWith("\uFEFF", StringComparison.Ordinal) ? text.Substring(1) : text).Trim();
        return SvgStart.IsMatch(normalized) && (SvgClose.IsMatch(normalized) || SvgSelfClose.IsMatch(normalized));
    }
}
