// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.DeepSeek;

/// <summary>Uploads images to <c>POST /files</c> with purpose <c>user_data</c>.</summary>
public sealed class DeepSeekFileStore : Provider.IFileStore, Operations.IFileStore
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

    /// <summary>Provider id.</summary>
    public string Provider => "deepseek.files";

    /// <summary>Files specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        var result = await UploadAsync(data ?? Array.Empty<byte>(), mediaType, fileName, ExpiresAfterSeconds, null, cancellationToken).ConfigureAwait(false);
        return new UploadedFile(result.ProviderReference.Id, result.Filename ?? fileName);
    }

    /// <summary>
    /// Uploads inline data. Streams are rejected. <c>deepseek.expiresAfter</c> overrides <see cref="ExpiresAfterSeconds"/>.
    /// </summary>
    public Task<UploadFileModelResult> UploadFileAsync(UploadFileCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        return UploadAsync(InlineBytes(call.Data), call.MediaType, call.Filename, ExpiresAfter(call.ProviderOptions) ?? ExpiresAfterSeconds, call.Headers, cancellationToken);
    }

    private async Task<UploadFileModelResult> UploadAsync(byte[] data, string? mediaType, string? fileName, int? expiresAfter, IReadOnlyDictionary<string, string>? callHeaders, CancellationToken cancellationToken)
    {
        Validate(data.LongLength, fileName, mediaType ?? string.Empty, data);
        if (expiresAfter is { } expiry && (expiry < MinExpirySeconds || expiry > MaxExpirySeconds))
        {
            throw ExpiryError();
        }

        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(data);
        file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType);
        form.Add(file, "file", string.IsNullOrEmpty(fileName) ? "file" : fileName);
        form.Add(new StringContent("user_data"), "purpose");
        if (expiresAfter is { } seconds)
        {
            form.Add(new StringContent("created_at"), "expires_after[anchor]");
            form.Add(new StringContent(seconds.ToString(CultureInfo.InvariantCulture)), "expires_after[seconds]");
        }

        var headers = _provider.CreateHeaders(Headers);
        if (callHeaders != null)
        {
            foreach (var header in callHeaders)
            {
                headers[header.Key] = header.Value;
            }
        }

        var uri = new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/files");
        var response = await _provider.PostMultipartAsync(uri, form, headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String)
        {
            throw InvalidResponse("id", response.Body, root);
        }

        var metadata = new JsonObject();
        AddLiteral(metadata, root, "object", "object", "file", response.Body);
        AddFilename(metadata, root, response.Body);
        AddLiteral(metadata, root, "purpose", "purpose", "user_data", response.Body);
        AddCount(metadata, root, "bytes", "bytes", response.Body);
        AddCount(metadata, root, "created_at", "createdAt", response.Body);
        AddCount(metadata, root, "expires_at", "expiresAt", response.Body);
        var returnedName = metadata["filename"]?.GetValue<string>();
        return new UploadFileModelResult(
            new ProviderFileReference(id.GetString()!, "deepseek"),
            mediaType,
            string.IsNullOrEmpty(returnedName) ? (string.IsNullOrEmpty(fileName) ? null : fileName) : returnedName,
            providerMetadata: JsonSerializer.SerializeToElement(new JsonObject { ["deepseek"] = metadata }));
    }

    private static byte[] InlineBytes(UploadData data)
    {
        switch (data.Type)
        {
            case "data":
                return data.Data as byte[] ?? ByteEncoding.FromBase64(data.Data as string ?? string.Empty);
            case "text":
                return Encoding.UTF8.GetBytes(data.Data as string ?? string.Empty);
            default:
                throw new UnsupportedFunctionalityException("stream file data", "DeepSeek file uploads do not support stream data. Pass bytes or base64 data instead.");
        }
    }

    private static int? ExpiresAfter(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } options
            || !options.TryGetProperty("deepseek", out var deepseek) || deepseek.ValueKind != JsonValueKind.Object
            || !deepseek.TryGetProperty("expiresAfter", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var seconds) ? seconds : throw ExpiryError();
    }

    private static AiSdkException ExpiryError()
    {
        return new AiSdkException("DeepSeek expiresAfter must be an integer from " + MinExpirySeconds.ToString(CultureInfo.InvariantCulture) + " to " + MaxExpirySeconds.ToString(CultureInfo.InvariantCulture) + ".");
    }

    private static void AddLiteral(JsonObject metadata, JsonElement root, string field, string name, string expected, string body)
    {
        if (!root.TryGetProperty(field, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.String || value.GetString() != expected)
        {
            throw InvalidResponse(field, body, root);
        }

        metadata[name] = expected;
    }

    private static void AddFilename(JsonObject metadata, JsonElement root, string body)
    {
        if (!root.TryGetProperty("filename", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw InvalidResponse("filename", body, root);
        }

        metadata["filename"] = value.GetString();
    }

    private static void AddCount(JsonObject metadata, JsonElement root, string field, string name, string body)
    {
        if (!root.TryGetProperty(field, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var count) || count < 0)
        {
            throw InvalidResponse(field, body, root);
        }

        metadata[name] = count;
    }

    private static ApiException InvalidResponse(string field, string body, JsonElement root)
    {
        var cause = new TypeValidationException("Invalid DeepSeek file response: \"" + field + "\" is missing or has an unexpected value.", root.Clone());
        return new ApiException("Invalid JSON response", 200, body, cause);
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
