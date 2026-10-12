// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;

namespace Vercel.AI.ProviderUtils;

/// <summary>Detects IANA media types from file signatures. Maps to <c>detectMediaType</c>.</summary>
public static class MediaTypes
{
    /// <summary>Largest ID3v2 tag skipped before the audio frame.</summary>
    public const int MaxId3TagBytes = 128 * 1024;

    private const int DefaultSniffBytes = 18;
    private const int MaxSignatureBytes = 12;
    private const int Id3ScanBytes = MaxId3TagBytes + MaxSignatureBytes;

    private static readonly Signature[] ImageSignatures =
    {
        Sig("image/gif", 0x47, 0x49, 0x46, 0x38, 0x37, 0x61),
        Sig("image/gif", 0x47, 0x49, 0x46, 0x38, 0x39, 0x61),
        Sig("image/png", 0x89, 0x50, 0x4e, 0x47),
        Sig("image/jpeg", 0xff, 0xd8),
        Sig("image/webp", 0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x57, 0x45, 0x42, 0x50),
        Sig("image/bmp", 0x42, 0x4d, null, null, null, null, 0x00, 0x00, 0x00, 0x00),
        Sig("image/tiff", 0x49, 0x49, 0x2a, 0x00),
        Sig("image/tiff", 0x4d, 0x4d, 0x00, 0x2a),
        Sig("image/avif", 0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66),
        Sig("image/heic", 0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63),
    };

    private static readonly Signature[] DocumentSignatures =
    {
        Sig("application/pdf", 0x25, 0x50, 0x44, 0x46),
    };

    private static readonly Signature[] AudioSignaturesWithoutMp4 =
    {
        Sig("audio/aac", 0xff, 0xf0),
        Sig("audio/aac", 0xff, 0xf1),
        Sig("audio/aac", 0xff, 0xf8),
        Sig("audio/aac", 0xff, 0xf9),
        Sig("audio/mpeg", 0xff, 0xfb),
        Sig("audio/mpeg", 0xff, 0xfa),
        Sig("audio/mpeg", 0xff, 0xf3),
        Sig("audio/mpeg", 0xff, 0xf2),
        Sig("audio/mpeg", 0xff, 0xe3),
        Sig("audio/mpeg", 0xff, 0xe2),
        Sig("audio/wav", 0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x57, 0x41, 0x56, 0x45),
        Sig("audio/ogg", 0x4f, 0x67, 0x67, 0x53),
        Sig("audio/flac", 0x66, 0x4c, 0x61, 0x43),
        Sig("audio/aac", 0x40, 0x15, 0x00, 0x00),
        Sig("audio/webm", 0x1a, 0x45, 0xdf, 0xa3),
    };

    private static readonly Signature[] AudioSignatures = Concat(
        AudioSignaturesWithoutMp4,
        new[] { Sig("audio/mp4", 0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70) });

    private static readonly Signature[] VideoSignatures =
    {
        Sig("video/mp4", 0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70),
        Sig("video/webm", 0x1a, 0x45, 0xdf, 0xa3),
        Sig("video/quicktime", 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70, 0x71, 0x74),
        Sig("video/x-msvideo", 0x52, 0x49, 0x46, 0x46),
    };

    /// <summary>
    /// Detects a media type from raw bytes.
    /// When <paramref name="topLevelType"/> is omitted, image, document, audio, and video signatures are considered.
    /// </summary>
    public static string? DetectMediaType(byte[] data, string? topLevelType = null)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        return Detect(maxBytes => DecodePrefix(data, maxBytes), topLevelType);
    }

    /// <summary>
    /// Detects a media type from a base64 string.
    /// When <paramref name="topLevelType"/> is omitted, image, document, audio, and video signatures are considered.
    /// </summary>
    public static string? DetectMediaType(string base64, string? topLevelType = null)
    {
        if (base64 is null)
        {
            throw new ArgumentNullException(nameof(base64));
        }

        return Detect(maxBytes => DecodePrefix(base64, maxBytes), topLevelType);
    }

    private static string? Detect(Func<int, byte[]> decodePrefix, string? topLevelType)
    {
        if (topLevelType is null)
        {
            return Match(decodePrefix, Concat(ImageSignatures, DocumentSignatures, AudioSignaturesWithoutMp4, VideoSignatures));
        }

        Signature[]? table = null;
        if (topLevelType == "image")
        {
            table = ImageSignatures;
        }
        else if (topLevelType == "audio")
        {
            table = AudioSignatures;
        }
        else if (topLevelType == "video")
        {
            table = VideoSignatures;
        }
        else if (topLevelType == "application")
        {
            table = DocumentSignatures;
        }

        if (table is null)
        {
            return null;
        }

        return Match(decodePrefix, table);
    }

    /// <summary>Returns the segment before the first slash. Maps to <c>getTopLevelMediaType</c>.</summary>
    public static string GetTopLevelMediaType(string mediaType)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    /// <summary>True for <c>type/subtype</c> when the subtype is non-empty and not <c>*</c>. Maps to <c>isFullMediaType</c>.</summary>
    public static bool IsFullMediaType(string mediaType)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        var slash = mediaType.IndexOf('/');
        if (slash < 0)
        {
            return false;
        }

        var subtype = mediaType.Substring(slash + 1);
        return subtype.Length > 0 && subtype != "*";
    }

    /// <summary>
    /// Returns <paramref name="mediaType"/> when it has a subtype. Otherwise detects the subtype from inline <paramref name="data"/>.
    /// Maps to <c>resolveFullMediaType</c>, with <paramref name="mediaType"/> and <paramref name="data"/> as the file part's
    /// <c>mediaType</c> and <c>data</c>.
    /// </summary>
    /// <exception cref="UnsupportedFunctionalityException">The subtype cannot be resolved.</exception>
    public static string ResolveFullMediaType(string mediaType, LanguageModelFilePart data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.DataType != "data")
        {
            return Resolve(mediaType, null, null);
        }

        return data.Bytes != null
            ? Resolve(mediaType, data.Bytes, null)
            : Resolve(mediaType, null, data.Base64 ?? string.Empty);
    }

    /// <summary>
    /// Returns <paramref name="mediaType"/> when it has a subtype. Otherwise detects the subtype from <paramref name="inlineData"/>,
    /// or null when the file is not inline. Maps to <c>resolveFullMediaType</c>.
    /// </summary>
    /// <exception cref="UnsupportedFunctionalityException">The subtype cannot be resolved.</exception>
    public static string ResolveFullMediaType(string mediaType, byte[]? inlineData)
    {
        return Resolve(mediaType, inlineData, null);
    }

    private static string Resolve(string mediaType, byte[]? bytes, string? base64)
    {
        if (IsFullMediaType(mediaType))
        {
            return mediaType;
        }

        string functionality;
        if (bytes != null || base64 != null)
        {
            var topLevelType = GetTopLevelMediaType(mediaType);
            var detected = bytes != null ? DetectMediaType(bytes, topLevelType) : DetectMediaType(base64!, topLevelType);
            if (detected != null)
            {
                return detected;
            }

            functionality = "file of media type \"" + mediaType + "\" must specify subtype since it could not be auto-detected";
        }
        else
        {
            functionality = "file of media type \"" + mediaType + "\" must specify subtype since it is not passed as inline bytes";
        }

        throw new UnsupportedFunctionalityException(functionality, functionality);
    }

    private static string? Match(Func<int, byte[]> decodePrefix, Signature[] signatures)
    {
        byte[] bytes;
        try
        {
            bytes = decodePrefix(DefaultSniffBytes);
        }
        catch (FormatException)
        {
            return null;
        }

        if (HasId3(bytes))
        {
            try
            {
                bytes = StripId3(decodePrefix(Id3ScanBytes));
            }
            catch (FormatException)
            {
                return null;
            }
        }

        foreach (var signature in signatures)
        {
            if (bytes.Length >= signature.Prefix.Length && Matches(bytes, signature.Prefix))
            {
                return signature.MediaType;
            }
        }

        return null;
    }

    private static bool Matches(byte[] bytes, int?[] prefix)
    {
        for (var i = 0; i < prefix.Length; i++)
        {
            var expected = prefix[i];
            if (expected != null && bytes[i] != expected.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasId3(byte[] bytes)
    {
        return bytes.Length > 10 && bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33;
    }

    private static byte[] StripId3(byte[] bytes)
    {
        var id3Size = ((bytes[6] & 0x7f) << 21)
            | ((bytes[7] & 0x7f) << 14)
            | ((bytes[8] & 0x7f) << 7)
            | (bytes[9] & 0x7f);
        var start = id3Size + 10;
        if (start >= bytes.Length)
        {
            return Array.Empty<byte>();
        }

        var rest = new byte[bytes.Length - start];
        Buffer.BlockCopy(bytes, start, rest, 0, rest.Length);
        return rest;
    }

    private static byte[] DecodePrefix(byte[] bytes, int maxBytes)
    {
        return bytes.Length > maxBytes ? Slice(bytes, maxBytes) : bytes;
    }

    private static byte[] DecodePrefix(string base64, int maxBytes)
    {
        var maxChars = ((maxBytes + 2) / 3) * 4;
        var slice = base64.Length > maxChars ? base64.Substring(0, maxChars) : base64;
        var decoded = ByteEncoding.FromBase64(slice);
        return decoded.Length > maxBytes ? Slice(decoded, maxBytes) : decoded;
    }

    private static byte[] Slice(byte[] bytes, int length)
    {
        var slice = new byte[length];
        Buffer.BlockCopy(bytes, 0, slice, 0, length);
        return slice;
    }

    private static Signature Sig(string mediaType, params int?[] prefix)
    {
        return new Signature(mediaType, prefix);
    }

    private static Signature[] Concat(params Signature[][] groups)
    {
        var count = 0;
        foreach (var group in groups)
        {
            count += group.Length;
        }

        var result = new Signature[count];
        var index = 0;
        foreach (var group in groups)
        {
            Array.Copy(group, 0, result, index, group.Length);
            index += group.Length;
        }

        return result;
    }

    private readonly struct Signature
    {
        public Signature(string mediaType, int?[] prefix)
        {
            MediaType = mediaType;
            Prefix = prefix;
        }

        public string MediaType { get; }

        public int?[] Prefix { get; }
    }
}
