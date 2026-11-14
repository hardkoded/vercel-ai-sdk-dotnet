// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic Files API upload.</summary>
public sealed class AnthropicFiles : IFileStore
{
    private readonly ProviderHttp _http;
    private readonly Func<string> _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a files client.</summary>
    public AnthropicFiles(ProviderHttp http, Func<string> baseUrl, Func<IReadOnlyDictionary<string, string?>> headers, string provider)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Provider = provider ?? "anthropic.messages";
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <inheritdoc />
    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        var result = await UploadAsync(string.IsNullOrEmpty(fileName) ? "blob" : fileName, data, mediaType, null, cancellationToken).ConfigureAwait(false);
        return new UploadedFile(result.Id, result.FileName);
    }

    /// <summary>Uploads a file and returns the parsed Files API body.</summary>
    public async Task<AnthropicUploadedFile> UploadAsync(string fileName, byte[] data, string mediaType, bool? downloadable, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(data ?? Array.Empty<byte>());
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType);
        MultipartForm.AddFile(form, content, "file", string.IsNullOrEmpty(fileName) ? "blob" : fileName);
        if (downloadable != null)
        {
            form.Add(new StringContent(downloadable.Value ? "true" : "false"), "downloadable");
        }

        var response = await _http.SendBinaryAsync(HttpMethod.Post, ApiKeys.Combine(_baseUrl(), "files"), form, GetHeaders(callHeaders), cancellationToken).ConfigureAwait(false);
        return Parse(response.Body, fileName, mediaType);
    }

    /// <summary>Reads file metadata from <c>GET /files/{id}</c>.</summary>
    public async Task<AnthropicUploadedFile> GetFileMetadataAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        var id = GetFileId(file);
        var response = await _http.SendBinaryAsync(HttpMethod.Get, ApiKeys.Combine(_baseUrl(), "files/" + PathSegments.Encode(id)), null, GetHeaders(callHeaders), cancellationToken).ConfigureAwait(false);
        return Parse(response.Body, null, null);
    }

    /// <summary>Downloads file bytes from <c>GET /files/{id}/content</c>.</summary>
    public async Task<AnthropicFileDownload> DownloadFileAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        var id = GetFileId(file);
        var response = await _http.OpenBinaryStreamAsync(HttpMethod.Get, ApiKeys.Combine(_baseUrl(), "files/" + PathSegments.Encode(id) + "/content"), GetHeaders(callHeaders), cancellationToken).ConfigureAwait(false);
        string? mediaType = null;
        if (response.Headers.TryGetValue("Content-Type", out var contentType) && !string.IsNullOrWhiteSpace(contentType))
        {
            var separator = contentType.IndexOf(';');
            mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            if (mediaType.Length == 0)
            {
                mediaType = null;
            }
        }

        return new AnthropicFileDownload(response.Body, mediaType);
    }

    /// <summary>Deletes a file with <c>DELETE /files/{id}</c>.</summary>
    public async Task<AnthropicFileDeleteResult> DeleteFileAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        var id = GetFileId(file);
        var response = await _http.SendBinaryAsync(HttpMethod.Delete, ApiKeys.Combine(_baseUrl(), "files/" + PathSegments.Encode(id)), null, GetHeaders(callHeaders), cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        return new AnthropicFileDeleteResult(new Dictionary<string, string> { ["anthropic"] = AnthropicJson.String(document.RootElement, "id") ?? throw new InvalidOperationException("Anthropic delete response is missing 'id'.") }, true);
    }

    private static string GetFileId(IReadOnlyDictionary<string, string?> file)
    {
        if (file == null || !file.TryGetValue("anthropic", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("file reference is missing an 'anthropic' file id.");
        }

        return id!;
    }

    private static AnthropicUploadedFile Parse(byte[] body, string? uploadedName, string? uploadedMediaType)
    {
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(body));
        var root = document.RootElement;
        return new AnthropicUploadedFile(
            AnthropicJson.String(root, "id") ?? string.Empty,
            AnthropicJson.String(root, "filename") ?? uploadedName,
            root.Clone(),
            uploadedMediaType);
    }

    // Provider headers, then the beta default, then the per-call headers, as upstream's combineHeaders does.
    private Dictionary<string, string?> GetHeaders(IReadOnlyDictionary<string, string?>? callHeaders)
    {
        var headers = new Dictionary<string, string?>();
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        headers["anthropic-beta"] = "files-api-2025-04-14";
        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}

/// <summary>Files API upload or metadata result.</summary>
public sealed class AnthropicUploadedFile
{
    private readonly string? _uploadedMediaType;

    /// <summary>Creates a result.</summary>
    public AnthropicUploadedFile(string id, string? fileName, JsonElement raw)
        : this(id, fileName, raw, null)
    {
    }

    /// <summary>Creates a result. The uploaded media type is the fallback for an upload response.</summary>
    public AnthropicUploadedFile(string id, string? fileName, JsonElement raw, string? uploadedMediaType)
    {
        Id = id;
        FileName = fileName;
        Raw = raw;
        _uploadedMediaType = uploadedMediaType;
    }

    /// <summary>File id.</summary>
    public string Id { get; }

    /// <summary>File name.</summary>
    public string? FileName { get; }

    /// <summary>Raw response.</summary>
    public JsonElement Raw { get; }

    /// <summary>Reference containing the <c>anthropic</c> file id.</summary>
    public IReadOnlyDictionary<string, string> ProviderReference => new Dictionary<string, string> { ["anthropic"] = Id };

    /// <summary><c>mime_type</c>, or the uploaded media type.</summary>
    public string? MediaType => AnthropicJson.String(Raw, "mime_type") ?? _uploadedMediaType;

    /// <summary><c>size_bytes</c>.</summary>
    public long? ByteSize => Raw.ValueKind == JsonValueKind.Object && Raw.TryGetProperty("size_bytes", out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt64() : null;

    /// <summary><c>created_at</c>.</summary>
    public DateTimeOffset? CreatedAt => ParseDate("created_at");

    /// <summary><c>expires_at</c>.</summary>
    public DateTimeOffset? ExpiresAt => ParseDate("expires_at");

    /// <summary>Always empty.</summary>
    public IReadOnlyList<ModelWarning> Warnings => Array.Empty<ModelWarning>();

    /// <summary>Metadata stored under <c>providerMetadata.anthropic</c>. Fields the response left out or set to null are omitted.</summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> ProviderMetadata
    {
        get
        {
            var metadata = new Dictionary<string, object>();
            Add(metadata, "filename", "filename");
            Add(metadata, "mimeType", "mime_type");
            Add(metadata, "sizeBytes", "size_bytes");
            Add(metadata, "createdAt", "created_at");
            Add(metadata, "downloadable", "downloadable");
            Add(metadata, "expiresAt", "expires_at");
            return new Dictionary<string, IReadOnlyDictionary<string, object>> { ["anthropic"] = metadata };
        }
    }

    private void Add(Dictionary<string, object> metadata, string key, string property)
    {
        if (Raw.ValueKind != JsonValueKind.Object || !Raw.TryGetProperty(property, out var value))
        {
            return;
        }

        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                metadata[key] = value.GetString()!;
                break;
            case JsonValueKind.Number:
                metadata[key] = value.GetInt64();
                break;
            case JsonValueKind.True:
                metadata[key] = true;
                break;
            case JsonValueKind.False:
                metadata[key] = false;
                break;
        }
    }

    private DateTimeOffset? ParseDate(string property)
    {
        var text = AnthropicJson.String(Raw, property);
        return text != null && DateTimeOffset.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var date) ? date : null;
    }
}

/// <summary>Downloaded Anthropic file bytes.</summary>
public sealed class AnthropicFileDownload
{
    /// <summary>Creates a download.</summary>
    public AnthropicFileDownload(Stream content, string? mediaType)
    {
        Content = content ?? Stream.Null;
        MediaType = mediaType;
    }

    /// <summary>Always empty.</summary>
    public IReadOnlyList<ModelWarning> Warnings => Array.Empty<ModelWarning>();

    /// <summary>File contents.</summary>
    public Stream Content { get; }

    /// <summary>Content type with parameters removed. Null when the response has none.</summary>
    public string? MediaType { get; }
}

/// <summary>Result of deleting an Anthropic file.</summary>
public sealed class AnthropicFileDeleteResult
{
    /// <summary>Creates a delete result.</summary>
    public AnthropicFileDeleteResult(IReadOnlyDictionary<string, string> providerReference, bool deleted)
    {
        ProviderReference = providerReference;
        Deleted = deleted;
    }

    /// <summary>Always empty.</summary>
    public IReadOnlyList<ModelWarning> Warnings => Array.Empty<ModelWarning>();

    /// <summary>Reference containing the deleted <c>anthropic</c> file id.</summary>
    public IReadOnlyDictionary<string, string> ProviderReference { get; }

    /// <summary>Always true after a successful delete.</summary>
    public bool Deleted { get; }
}
