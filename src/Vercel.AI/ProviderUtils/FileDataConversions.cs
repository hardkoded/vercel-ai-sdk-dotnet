// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.ProviderUtils;

/// <summary>Inline file data passed to providers.</summary>
public sealed class InlineFileData
{
    /// <summary>Creates inline data. <paramref name="type"/> is <c>text</c>, <c>data</c>, or <c>stream</c>.</summary>
    public InlineFileData(string type, string? text = null, object? data = null, Stream? stream = null, Action<Exception>? onCancel = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Text = text;
        Data = data;
        Stream = stream;
        OnCancel = onCancel;
    }

    /// <summary>Data kind.</summary>
    public string Type { get; }

    /// <summary>Text payload for <c>text</c> data.</summary>
    public string? Text { get; }

    /// <summary>Byte array, or a base64 string, for <c>data</c>.</summary>
    public object? Data { get; }

    /// <summary>Stream payload.</summary>
    public Stream? Stream { get; }

    /// <summary>Called when a stream payload is rejected.</summary>
    public Action<Exception>? OnCancel { get; }
}

/// <summary>Image file passed to an image model.</summary>
public sealed class ImageModelFile
{
    /// <summary>Creates a URL file.</summary>
    public static ImageModelFile FromUrl(string url)
    {
        return new ImageModelFile("url", url, null, null);
    }

    /// <summary>Creates a file from base64 text or bytes.</summary>
    public static ImageModelFile File(string mediaType, object data)
    {
        return new ImageModelFile("file", null, mediaType, data);
    }

    private ImageModelFile(string type, string? url, string? mediaType, object? data)
    {
        Type = type;
        Url = url;
        MediaType = mediaType;
        Data = data;
    }

    /// <summary><c>url</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>URL when <see cref="Type"/> is <c>url</c>.</summary>
    public string? Url { get; }

    /// <summary>Media type when <see cref="Type"/> is <c>file</c>.</summary>
    public string? MediaType { get; }

    /// <summary>Base64 text or raw bytes.</summary>
    public object? Data { get; }
}

/// <summary>Converts inline files. Maps to <c>convertInlineFileDataToUint8Array</c> and <c>convertImageModelFileToDataUri</c>.</summary>
public static class FileDataConversions
{
    /// <summary>Converts text, bytes, or base64 into a byte array. Streams are rejected.</summary>
    public static byte[] ConvertInlineFileDataToUint8Array(InlineFileData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.Type == "stream")
        {
            var error = new UnsupportedFunctionalityError("streaming file upload");
            try
            {
                data.OnCancel?.Invoke(error);
            }
            catch (Exception)
            {
            }

            throw error;
        }

        if (data.Type == "text")
        {
            return Encoding.UTF8.GetBytes(data.Text ?? string.Empty);
        }

        if (data.Data is byte[] bytes)
        {
            return bytes;
        }

        if (data.Data is string text)
        {
            return Uint8Utils.ConvertBase64ToUint8Array(text);
        }

        throw new ArgumentException("Inline file data must be text, bytes, or base64.", nameof(data));
    }

    /// <summary>Returns a URL or a <c>data:</c> URI.</summary>
    public static string ConvertImageModelFileToDataUri(ImageModelFile file)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Type == "url")
        {
            return file.Url ?? string.Empty;
        }

        var payload = file.Data is string text
            ? text
            : Uint8Utils.ConvertUint8ArrayToBase64((byte[])file.Data!);
        return "data:" + file.MediaType + ";base64," + payload;
    }
}
