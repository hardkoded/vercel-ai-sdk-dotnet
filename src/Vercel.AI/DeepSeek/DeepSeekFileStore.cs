// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepSeek;

/// <summary>Uploads images to <c>POST /files</c> with purpose <c>user_data</c>.</summary>
public sealed class DeepSeekFileStore : IFileStore
{
    /// <summary>Maximum upload size, 64 MiB.</summary>
    public const long MaxBytes = 64L * 1024L * 1024L;

    /// <summary>Maximum filename length.</summary>
    public const int MaxFileNameLength = 512;

    /// <summary>Shortest accepted expiry, in seconds.</summary>
    public const int MinExpirySeconds = 3600;

    /// <summary>Longest accepted expiry, in seconds.</summary>
    public const int MaxExpirySeconds = 2592000;

    private readonly DeepSeekProvider _provider;

    /// <summary>Creates a file store.</summary>
    public DeepSeekFileStore(DeepSeekProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>When set, sent as <c>expires_after[seconds]</c> with anchor <c>created_at</c>.</summary>
    public int? ExpiresAfterSeconds { get; set; }

    /// <summary>Headers for the next upload.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <inheritdoc />
    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        Validate(data == null ? 0 : data.LongLength, fileName, mediaType ?? string.Empty, data);
        if (ExpiresAfterSeconds is { } expiry && (expiry < MinExpirySeconds || expiry > MaxExpirySeconds))
        {
            throw new AiSdkException("DeepSeek expiresAfter must be an integer from " + MinExpirySeconds.ToString(CultureInfo.InvariantCulture) + " to " + MaxExpirySeconds.ToString(CultureInfo.InvariantCulture) + ".");
        }

        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(data ?? Array.Empty<byte>());
        file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType);
        form.Add(file, "file", string.IsNullOrEmpty(fileName) ? "file" : fileName);
        form.Add(new StringContent("user_data"), "purpose");
        if (ExpiresAfterSeconds is { } seconds)
        {
            form.Add(new StringContent("created_at"), "expires_after[anchor]");
            form.Add(new StringContent(seconds.ToString(CultureInfo.InvariantCulture)), "expires_after[seconds]");
        }

        var uri = new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/files");
        var response = await _provider.PostMultipartAsync(uri, form, _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
        using var document = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        var id = root.TryGetProperty("id", out var idElement) && idElement.ValueKind == System.Text.Json.JsonValueKind.String
            ? idElement.GetString()
            : null;
        if (string.IsNullOrEmpty(id))
        {
            throw new AiSdkException("DeepSeek file response did not contain an id.");
        }

        var returnedName = root.TryGetProperty("filename", out var nameElement) && nameElement.ValueKind == System.Text.Json.JsonValueKind.String
            ? nameElement.GetString()
            : null;
        return new UploadedFile(id!, string.IsNullOrEmpty(returnedName) ? fileName : returnedName);
    }

    /// <summary>Checks size, filename, media type, and magic bytes before any request is sent.</summary>
    public static void Validate(long length, string? fileName, string mediaType, byte[]? data)
    {
        if (length > MaxBytes)
        {
            throw new AiSdkException(
                "DeepSeek file uploads must not exceed 64 MiB ("
                + MaxBytes.ToString("N0", CultureInfo.InvariantCulture)
                + " bytes). Received "
                + length.ToString("N0", CultureInfo.InvariantCulture)
                + " bytes.");
        }

        if (fileName != null && fileName.Length > MaxFileNameLength)
        {
            throw new AiSdkException(
                "DeepSeek filenames must not exceed " + MaxFileNameLength.ToString(CultureInfo.InvariantCulture)
                + " characters. Received " + fileName.Length.ToString(CultureInfo.InvariantCulture) + " characters.");
        }

        var normalized = Normalize(mediaType);
        var detected = Detect(data);
        if (detected != null && !IsSupported(detected))
        {
            throw new AiSdkException(
                "DeepSeek file uploads support JPEG, PNG, GIF, and WebP images. Detected unsupported file content type \"" + detected + "\".");
        }

        if (IsSupported(normalized))
        {
            return;
        }

        if (!IsGeneric(normalized))
        {
            throw new AiSdkException(
                "DeepSeek file uploads support JPEG, PNG, GIF, and WebP images. Received unsupported media type \"" + (mediaType ?? string.Empty) + "\".");
        }

        if (detected != null || HasSupportedExtension(fileName))
        {
            return;
        }

        throw new AiSdkException(
            "DeepSeek file uploads support JPEG, PNG, GIF, and WebP images. Provide a supported media type or a filename ending in .jpg, .jpeg, .png, .gif, or .webp. Received \"" + (mediaType ?? string.Empty) + "\".");
    }

    private static string Normalize(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return string.Empty;
        }

        var semi = mediaType!.IndexOf(';');
        var value = semi < 0 ? mediaType : mediaType.Substring(0, semi);
        return value.Trim().ToLowerInvariant();
    }

    private static bool IsSupported(string mediaType)
    {
        return mediaType == "image/gif"
            || mediaType == "image/jpeg"
            || mediaType == "image/jpg"
            || mediaType == "image/png"
            || mediaType == "image/webp";
    }

    private static bool IsGeneric(string mediaType)
    {
        return mediaType.Length == 0
            || mediaType == "application/binary"
            || mediaType == "application/octet-stream"
            || mediaType == "binary/octet-stream"
            || mediaType == "image"
            || mediaType == "image/*";
    }

    private static bool HasSupportedExtension(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var dot = fileName!.LastIndexOf('.');
        if (dot < 0 || dot == fileName.Length - 1)
        {
            return false;
        }

        var extension = fileName.Substring(dot + 1).ToLowerInvariant();
        return extension == "gif" || extension == "jpeg" || extension == "jpg" || extension == "png" || extension == "webp";
    }

    private static string? Detect(byte[]? data)
    {
        if (data == null || data.Length < 4)
        {
            return null;
        }

        if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
        {
            return "application/pdf";
        }

        if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return "image/png";
        }

        if (data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'8')
        {
            return "image/gif";
        }

        if (data.Length >= 12
            && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F'
            && data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P')
        {
            return "image/webp";
        }

        return null;
    }
}
