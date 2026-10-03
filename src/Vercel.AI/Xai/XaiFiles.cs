// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>Bytes, base64, or a stream uploaded to xAI.</summary>
public sealed class XaiFileUpload
{
    /// <summary>Inline file bytes.</summary>
    public byte[]? Bytes { get; set; }

    /// <summary>Base64 file contents. Decoded before they are sent.</summary>
    public string? Base64 { get; set; }

    /// <summary>Streamed file contents. Fields are written before these bytes.</summary>
    public Stream? Stream { get; set; }

    /// <summary>Called when <see cref="ExpiresAfter"/> is rejected before any request.</summary>
    public Action<Exception>? CancelStream { get; set; }

    /// <summary>File media type.</summary>
    public string? MediaType { get; set; }

    /// <summary>File name. Omitted names are sent as <c>blob</c>.</summary>
    public string? FileName { get; set; }

    /// <summary><c>team_id</c>, when set.</summary>
    public string? TeamId { get; set; }

    /// <summary>TTL in seconds. Integers from 3600 through 2592000 are accepted.</summary>
    public double? ExpiresAfter { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Metadata stored under <c>providerMetadata.xai</c>. Null fields are omitted.</summary>
public sealed class XaiFileMetadata
{
    /// <summary>Creates metadata from the fields the response actually returned.</summary>
    public XaiFileMetadata(IReadOnlyDictionary<string, object> values)
    {
        Values = values ?? new Dictionary<string, object>();
    }

    /// <summary>Present response fields.</summary>
    public IReadOnlyDictionary<string, object> Values { get; }
}

/// <summary>Result of an xAI file upload or metadata read.</summary>
public sealed class XaiFileResult
{
    /// <summary>Creates a file result.</summary>
    public XaiFileResult(
        IReadOnlyDictionary<string, string> providerReference,
        XaiFileMetadata metadata,
        string? fileName,
        string? mediaType,
        long? byteSize,
        DateTimeOffset? createdAt,
        DateTimeOffset? expiresAt)
    {
        ProviderReference = providerReference ?? new Dictionary<string, string>();
        Metadata = metadata ?? new XaiFileMetadata(new Dictionary<string, object>());
        FileName = fileName;
        MediaType = mediaType;
        ByteSize = byteSize;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Warnings = Array.Empty<string>();
    }

    /// <summary>Always empty.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>Reference containing the <c>xai</c> file id.</summary>
    public IReadOnlyDictionary<string, string> ProviderReference { get; }

    /// <summary>Response fields with nulls removed. <c>createdAt</c> is unix seconds.</summary>
    public XaiFileMetadata Metadata { get; }

    /// <summary>File name from the response, or the name that was uploaded.</summary>
    public string? FileName { get; }

    /// <summary>Media type supplied by the caller.</summary>
    public string? MediaType { get; }

    /// <summary>Size in bytes, when the response includes it.</summary>
    public long? ByteSize { get; }

    /// <summary>Creation time.</summary>
    public DateTimeOffset? CreatedAt { get; }

    /// <summary>Expiration time.</summary>
    public DateTimeOffset? ExpiresAt { get; }
}

/// <summary>Downloaded xAI file bytes.</summary>
public sealed class XaiFileDownload
{
    /// <summary>Creates a download.</summary>
    public XaiFileDownload(Stream content, string? mediaType)
    {
        Content = content ?? Stream.Null;
        MediaType = mediaType;
        Warnings = Array.Empty<string>();
    }

    /// <summary>Always empty.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>File contents.</summary>
    public Stream Content { get; }

    /// <summary>Content type with parameters removed. Null when the response has none.</summary>
    public string? MediaType { get; }
}

/// <summary>Result of deleting an xAI file.</summary>
public sealed class XaiFileDeleteResult
{
    /// <summary>Creates a delete result.</summary>
    public XaiFileDeleteResult(IReadOnlyDictionary<string, string> providerReference, bool deleted)
    {
        ProviderReference = providerReference ?? new Dictionary<string, string>();
        Deleted = deleted;
        Warnings = Array.Empty<string>();
    }

    /// <summary>Always empty.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>Reference containing the deleted <c>xai</c> file id.</summary>
    public IReadOnlyDictionary<string, string> ProviderReference { get; }

    /// <summary>Whether the provider deleted the file.</summary>
    public bool Deleted { get; }
}

/// <summary>xAI Files API at <c>{base}/files</c>.</summary>
public sealed class XaiFiles : IFileStore
{
    /// <summary>Shortest accepted <c>expires_after</c>, in seconds.</summary>
    public const int MinimumExpiresAfter = 3600;

    /// <summary>Longest accepted <c>expires_after</c>, in seconds.</summary>
    public const int MaximumExpiresAfter = 2592000;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a file client.</summary>
    public XaiFiles(HttpClient httpClient, string baseUrl, Func<IReadOnlyDictionary<string, string?>>? headers, string? provider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? XaiProvider.DefaultBaseUrl : baseUrl;
        _headers = headers ?? (() => new Dictionary<string, string?>());
        Provider = string.IsNullOrEmpty(provider) ? "xai.files" : provider!;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <inheritdoc />
    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        var result = await UploadAsync(new XaiFileUpload { Bytes = data, FileName = fileName, MediaType = mediaType }, cancellationToken).ConfigureAwait(false);
        string? id = null;
        result.ProviderReference.TryGetValue("xai", out id);
        return new UploadedFile(id ?? string.Empty, result.FileName);
    }

    /// <summary>Posts multipart file contents to <c>/files</c>.</summary>
    public async Task<XaiFileResult> UploadAsync(XaiFileUpload upload, CancellationToken cancellationToken)
    {
        if (upload == null)
        {
            throw new ArgumentNullException(nameof(upload));
        }

        try
        {
            ValidateExpiresAfter(upload.ExpiresAfter);
        }
        catch (Exception exception)
        {
            if (upload.Stream != null && upload.CancelStream != null)
            {
                upload.CancelStream(exception);
            }

            throw;
        }

        var bytes = await ReadAsync(upload, cancellationToken).ConfigureAwait(false);
        var fileName = string.IsNullOrEmpty(upload.FileName) ? "blob" : upload.FileName!;
        var parts = new List<MultipartPart>();
        if (upload.ExpiresAfter is { } expires)
        {
            parts.Add(MultipartPart.Field("expires_after", ((long)expires).ToString(CultureInfo.InvariantCulture)));
        }

        if (upload.TeamId != null)
        {
            parts.Add(MultipartPart.Field("team_id", upload.TeamId));
        }

        parts.Add(MultipartPart.File("file", fileName, upload.MediaType ?? "application/octet-stream", bytes));
        var body = MultipartPart.Encode(parts);
        var headers = ProviderExchange.Merge(_headers(), upload.Headers);
        var response = await ProviderExchange.SendAsync(
            _httpClient,
            HttpMethod.Post,
            ApiKeys.Combine(_baseUrl, "/files"),
            body,
            headers,
            cancellationToken).ConfigureAwait(false);
        return ReadFile(response.Body, fileName, upload.MediaType);
    }

    /// <summary>Reads file metadata.</summary>
    public async Task<XaiFileResult> GetMetadataAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        var response = await ProviderExchange.SendAsync(
            _httpClient,
            HttpMethod.Get,
            ApiKeys.Combine(_baseUrl, "/files/" + EncodePathSegment(id)),
            null,
            _headers(),
            cancellationToken).ConfigureAwait(false);
        return ReadFile(response.Body, null, null);
    }

    /// <summary>Downloads file bytes.</summary>
    public async Task<XaiFileDownload> DownloadAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        var response = await ProviderExchange.SendAsync(
            _httpClient,
            HttpMethod.Get,
            ApiKeys.Combine(_baseUrl, "/files/" + EncodePathSegment(id) + "/content"),
            null,
            _headers(),
            cancellationToken).ConfigureAwait(false);
        string? contentType = null;
        string? mediaType = null;
        if (response.Headers.TryGetValue("Content-Type", out contentType) && !string.IsNullOrWhiteSpace(contentType))
        {
            var separator = contentType.IndexOf(';');
            mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            if (mediaType.Length == 0)
            {
                mediaType = null;
            }
        }

        return new XaiFileDownload(new MemoryStream(response.Bytes, writable: false), mediaType);
    }

    /// <summary>Deletes a file.</summary>
    public async Task<XaiFileDeleteResult> DeleteAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        var response = await ProviderExchange.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            ApiKeys.Combine(_baseUrl, "/files/" + EncodePathSegment(id)),
            null,
            _headers(),
            cancellationToken).ConfigureAwait(false);
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
        {
            var deletedId = ReadString(document.RootElement, "id") ?? id;
            var deleted = document.RootElement.TryGetProperty("deleted", out var flag) && flag.ValueKind == JsonValueKind.True;
            return new XaiFileDeleteResult(new Dictionary<string, string> { ["xai"] = deletedId }, deleted);
        }
    }

    /// <summary>Percent-encodes a path segment and double-encodes <c>.</c> and <c>..</c>.</summary>
    public static string EncodePathSegment(string value)
    {
        var encoded = Uri.EscapeDataString(value ?? string.Empty);
        if (encoded == ".")
        {
            return "%252E";
        }

        if (encoded == "..")
        {
            return "%252E%252E";
        }

        return encoded;
    }

    private static void ValidateExpiresAfter(double? expiresAfter)
    {
        if (expiresAfter == null)
        {
            return;
        }

        var value = expiresAfter.Value;
        if (value != Math.Truncate(value) || value < MinimumExpiresAfter || value > MaximumExpiresAfter)
        {
            throw new ArgumentException("expiresAfter must be an integer number of seconds from " + MinimumExpiresAfter.ToString(CultureInfo.InvariantCulture) + " through " + MaximumExpiresAfter.ToString(CultureInfo.InvariantCulture) + ".");
        }
    }

    private static string RequireId(IReadOnlyDictionary<string, string?> file)
    {
        if (file == null)
        {
            throw new ArgumentException("file reference is missing an 'xai' file id.");
        }

        string? id;
        if (!file.TryGetValue("xai", out id) || string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("file reference is missing an 'xai' file id.");
        }

        return id!;
    }

    private static async Task<byte[]> ReadAsync(XaiFileUpload upload, CancellationToken cancellationToken)
    {
        if (upload.Stream != null)
        {
            using (var buffer = new MemoryStream())
            {
                await upload.Stream.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
                return buffer.ToArray();
            }
        }

        if (upload.Base64 != null)
        {
            return Convert.FromBase64String(upload.Base64);
        }

        return upload.Bytes ?? Array.Empty<byte>();
    }

    private static XaiFileResult ReadFile(string json, string? uploadedName, string? mediaType)
    {
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json))
        {
            var root = document.RootElement;
            var metadata = new Dictionary<string, object>();
            var fileName = ReadString(root, "filename");
            if (fileName != null)
            {
                metadata["filename"] = fileName;
            }

            var purpose = ReadString(root, "purpose");
            if (purpose != null)
            {
                metadata["purpose"] = purpose;
            }

            long? bytes = null;
            if (root.TryGetProperty("bytes", out var bytesValue) && bytesValue.ValueKind == JsonValueKind.Number)
            {
                bytes = bytesValue.GetInt64();
                metadata["bytes"] = bytes.Value;
            }

            long? created = null;
            if (root.TryGetProperty("created_at", out var createdValue) && createdValue.ValueKind == JsonValueKind.Number)
            {
                created = createdValue.GetInt64();
                metadata["createdAt"] = created.Value;
            }

            var status = ReadString(root, "status");
            if (status != null)
            {
                metadata["status"] = status;
            }

            long? expires = null;
            if (root.TryGetProperty("expires_at", out var expiresValue) && expiresValue.ValueKind == JsonValueKind.Number)
            {
                expires = expiresValue.GetInt64();
                metadata["expiresAt"] = expires.Value;
            }

            var id = ReadString(root, "id") ?? string.Empty;
            return new XaiFileResult(
                new Dictionary<string, string> { ["xai"] = id },
                new XaiFileMetadata(metadata),
                fileName ?? uploadedName,
                mediaType,
                bytes,
                created == null ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeSeconds(created.Value),
                expires == null ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeSeconds(expires.Value));
        }
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private sealed class MultipartPart
    {
        private MultipartPart(string name, string? fileName, string? mediaType, byte[] bytes)
        {
            Name = name;
            FileName = fileName;
            MediaType = mediaType;
            Bytes = bytes;
        }

        public string Name { get; }

        public string? FileName { get; }

        public string? MediaType { get; }

        public byte[] Bytes { get; }

        public static MultipartPart Field(string name, string value)
        {
            return new MultipartPart(name, null, null, Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        public static MultipartPart File(string name, string fileName, string mediaType, byte[] bytes)
        {
            return new MultipartPart(name, fileName, mediaType, bytes ?? Array.Empty<byte>());
        }

        public static ByteArrayContent Encode(IReadOnlyList<MultipartPart> parts)
        {
            var boundary = "ai-sdk-multipart-" + Guid.NewGuid().ToString("N");
            using (var stream = new MemoryStream())
            {
                foreach (var part in parts)
                {
                    WriteAscii(stream, "--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + part.Name + "\"");
                    if (part.FileName != null)
                    {
                        WriteAscii(stream, "; filename=\"" + part.FileName + "\"");
                    }

                    WriteAscii(stream, "\r\n");
                    if (part.MediaType != null)
                    {
                        WriteAscii(stream, "Content-Type: " + part.MediaType + "\r\n");
                    }

                    WriteAscii(stream, "\r\n");
                    stream.Write(part.Bytes, 0, part.Bytes.Length);
                    WriteAscii(stream, "\r\n");
                }

                WriteAscii(stream, "--" + boundary + "--\r\n");
                var content = new ByteArrayContent(stream.ToArray());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                content.Headers.ContentType.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("boundary", boundary));
                return content;
            }
        }

        private static void WriteAscii(Stream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
