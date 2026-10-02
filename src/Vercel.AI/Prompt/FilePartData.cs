// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Prompt;

/// <summary>A file-part value before it is converted to the v4 provider shape.</summary>
public sealed class FilePartInput
{
    private FilePartInput()
    {
    }

    /// <summary>Tagged type, when this value is already tagged.</summary>
    public string? Type { get; private set; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Bytes { get; private set; }

    /// <summary>True when <see cref="Bytes"/> came from an ArrayBuffer and should be copied.</summary>
    public bool IsArrayBuffer { get; private set; }

    /// <summary>Inline base64 or any non-URL string.</summary>
    public string? TextData { get; private set; }

    /// <summary>URL instance.</summary>
    public Uri? Url { get; private set; }

    /// <summary>URL string that has not been parsed yet.</summary>
    public string? UrlText { get; private set; }

    /// <summary>Provider reference.</summary>
    public IReadOnlyDictionary<string, string>? Reference { get; private set; }

    /// <summary>Text payload.</summary>
    public string? Text { get; private set; }

    /// <summary>Creates inline bytes.</summary>
    public static FilePartInput FromBytes(byte[] bytes)
    {
        return new FilePartInput { Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes)) };
    }

    /// <summary>Creates an ArrayBuffer payload. The bytes are copied during conversion.</summary>
    public static FilePartInput FromArrayBuffer(byte[] bytes)
    {
        return new FilePartInput { Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes)), IsArrayBuffer = true };
    }

    /// <summary>Creates a string payload. URLs and data URLs are detected during conversion.</summary>
    public static FilePartInput FromString(string value)
    {
        return new FilePartInput { TextData = value ?? throw new ArgumentNullException(nameof(value)) };
    }

    /// <summary>Creates a URL payload.</summary>
    public static FilePartInput FromUrl(Uri url)
    {
        return new FilePartInput { Url = url ?? throw new ArgumentNullException(nameof(url)) };
    }

    /// <summary>Creates a provider reference.</summary>
    public static FilePartInput FromReference(IReadOnlyDictionary<string, string> reference)
    {
        return new FilePartInput { Reference = reference ?? throw new ArgumentNullException(nameof(reference)) };
    }

    /// <summary>Creates tagged inline bytes.</summary>
    public static FilePartInput TaggedData(byte[] bytes)
    {
        return new FilePartInput { Type = "data", Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes)) };
    }

    /// <summary>Creates tagged inline bytes copied from an ArrayBuffer.</summary>
    public static FilePartInput TaggedDataBuffer(byte[] bytes)
    {
        return new FilePartInput { Type = "data", Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes)), IsArrayBuffer = true };
    }

    /// <summary>Creates tagged inline text. A data URL is rejected.</summary>
    public static FilePartInput TaggedData(string data)
    {
        return new FilePartInput { Type = "data", TextData = data ?? throw new ArgumentNullException(nameof(data)) };
    }

    /// <summary>Creates a tagged URL.</summary>
    public static FilePartInput TaggedUrl(Uri url)
    {
        return new FilePartInput { Type = "url", Url = url ?? throw new ArgumentNullException(nameof(url)) };
    }

    /// <summary>Creates a tagged provider reference.</summary>
    public static FilePartInput TaggedReference(IReadOnlyDictionary<string, string> reference)
    {
        return new FilePartInput { Type = "reference", Reference = reference ?? throw new ArgumentNullException(nameof(reference)) };
    }

    /// <summary>Creates a tagged text part.</summary>
    public static FilePartInput TaggedText(string text)
    {
        return new FilePartInput { Type = "text", Text = text ?? string.Empty };
    }
}

/// <summary>A file part in the v4 provider prompt. Maps to the result of <c>convertToLanguageModelV4FilePart</c>.</summary>
public sealed class LanguageModelFilePart
{
    /// <summary>Creates a part.</summary>
    public LanguageModelFilePart(string dataType, string? mediaType)
    {
        DataType = dataType;
        MediaType = mediaType;
    }

    /// <summary><c>data</c>, <c>url</c>, <c>reference</c>, or <c>text</c>.</summary>
    public string DataType { get; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Bytes { get; set; }

    /// <summary>Inline base64 text.</summary>
    public string? Base64 { get; set; }

    /// <summary>URL.</summary>
    public Uri? Url { get; set; }

    /// <summary>Original URL string when parsing changed it.</summary>
    public string? OriginalUrl { get; set; }

    /// <summary>Provider reference.</summary>
    public IReadOnlyDictionary<string, string>? Reference { get; set; }

    /// <summary>Text payload.</summary>
    public string? Text { get; set; }

    /// <summary>Media type extracted from a data URL.</summary>
    public string? MediaType { get; }
}

/// <summary>File content could not be converted. Maps to <c>InvalidDataContentError</c>.</summary>
public sealed class InvalidDataContentException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public InvalidDataContentException(string message)
        : base(message)
    {
    }
}

/// <summary>File-part conversion. Maps to <c>convertToLanguageModelV4FilePart</c>.</summary>
public static class FileParts
{
    /// <summary>Converts a legacy or tagged file value into the v4 provider shape.</summary>
    public static LanguageModelFilePart ConvertToLanguageModelV4FilePart(FilePartInput content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (content.Type != null)
        {
            switch (content.Type)
            {
                case "data":
                    if (content.TextData != null && content.TextData.StartsWith("data:", StringComparison.Ordinal))
                    {
                        throw new InvalidDataContentException("Data URLs are not valid inline data. Pass them as { type: \"url\", url } instead.");
                    }

                    return Inline(content);
                case "url":
                    return FromUrl(content.Url ?? throw new InvalidDataContentException("URL file data requires a URL."));
                case "reference":
                    return new LanguageModelFilePart("reference", null) { Reference = content.Reference };
                case "text":
                    return new LanguageModelFilePart("text", null) { Text = content.Text };
                default:
                    throw new InvalidDataContentException("Unsupported file data type " + content.Type + ".");
            }
        }

        if (content.Url != null)
        {
            return FromUrl(content.Url);
        }

        if (content.TextData != null)
        {
            if (TryParseUrl(content.TextData, out var uri, out var absolute))
            {
                var part = FromUrl(uri);
                if (!string.Equals(absolute, content.TextData, StringComparison.Ordinal) && part.DataType == "url")
                {
                    part.OriginalUrl = content.TextData;
                }

                return part;
            }

            return new LanguageModelFilePart("data", null) { Base64 = content.TextData };
        }

        if (content.Reference != null)
        {
            return new LanguageModelFilePart("reference", null) { Reference = content.Reference };
        }

        return Inline(content);
    }

    private static LanguageModelFilePart Inline(FilePartInput content)
    {
        if (content.Bytes != null)
        {
            var bytes = content.IsArrayBuffer ? (byte[])content.Bytes.Clone() : content.Bytes;
            return new LanguageModelFilePart("data", null) { Bytes = bytes };
        }

        return new LanguageModelFilePart("data", null) { Base64 = content.TextData };
    }

    private static LanguageModelFilePart FromUrl(Uri url)
    {
        if (string.Equals(url.Scheme, "data", StringComparison.OrdinalIgnoreCase))
        {
            var source = string.IsNullOrEmpty(url.OriginalString) ? url.AbsoluteUri : url.OriginalString;
            var parts = DataUrls.SplitDataUrl(source);
            if (parts.MediaType == null || parts.Base64Content == null)
            {
                throw new InvalidDataContentException("Invalid data URL format in content " + source);
            }

            return new LanguageModelFilePart("data", parts.MediaType) { Base64 = parts.Base64Content };
        }

        return new LanguageModelFilePart("url", null) { Url = url };
    }

    private static bool TryParseUrl(string content, out Uri uri, out string absolute)
    {
        if (Uri.TryCreate(content, UriKind.Absolute, out var parsed) && parsed.IsAbsoluteUri && HasScheme(content))
        {
            uri = parsed;
            absolute = parsed.AbsoluteUri;
            return true;
        }

        var escaped = content.Replace(" ", "%20");
        if (!string.Equals(escaped, content, StringComparison.Ordinal) && Uri.TryCreate(escaped, UriKind.Absolute, out parsed) && HasScheme(escaped))
        {
            uri = parsed;
            absolute = parsed.AbsoluteUri;
            return true;
        }

        uri = new Uri("about:blank");
        absolute = string.Empty;
        return false;
    }

    private static bool HasScheme(string value)
    {
        var colon = value.IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        for (var i = 0; i < colon; i++)
        {
            var character = value[i];
            var ok = (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') || (i > 0 && ((character >= '0' && character <= '9') || character == '+' || character == '-' || character == '.'));
            if (!ok)
            {
                return false;
            }
        }

        return true;
    }
}
