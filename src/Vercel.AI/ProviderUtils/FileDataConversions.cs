// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;

namespace Vercel.AI.ProviderUtils;

/// <summary>Converts file payloads to the forms providers send.</summary>
public static class FileDataConversions
{
    /// <summary>
    /// Returns the bytes of an inline <c>data</c> or <c>text</c> file part. Base64 data is decoded and text is UTF-8 encoded.
    /// Maps to <c>convertInlineFileDataToUint8Array</c>.
    /// </summary>
    public static byte[] ConvertInlineFileDataToUint8Array(LanguageModelFilePart part)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        switch (part.DataType)
        {
            case "text":
                return Encoding.UTF8.GetBytes(part.Text ?? string.Empty);
            case "data":
                return part.Bytes ?? ByteEncoding.FromBase64(part.Base64 ?? string.Empty);
            default:
                throw new ArgumentException("File part of type '" + part.DataType + "' is not inline data.", nameof(part));
        }
    }

    /// <summary>
    /// Returns the URL of a URL file, or a base64 data URI for a file with bytes.
    /// Maps to <c>convertImageModelFileToDataUri</c>.
    /// </summary>
    public static string ConvertImageModelFileToDataUri(ImageModelFile file)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Url != null)
        {
            return file.Url;
        }

        return "data:" + file.MediaType + ";base64," + ByteEncoding.ToBase64(file.Data ?? Array.Empty<byte>());
    }
}
