// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>A file reference that carries an OpenAI file id.</summary>
public sealed class OpenAIFileReference
{
    /// <summary>Creates a reference.</summary>
    public OpenAIFileReference(string? openAI)
    {
        OpenAI = openAI;
    }

    /// <summary>OpenAI file id.</summary>
    public string? OpenAI { get; }
}

/// <summary>Metadata returned by the OpenAI Files API.</summary>
public sealed class OpenAIFileMetadata
{
    internal OpenAIFileMetadata(string id, string? filename, string? purpose, int? bytes, DateTimeOffset? createdAt, string? status, DateTimeOffset? expiresAt)
    {
        Id = id;
        Filename = filename;
        Purpose = purpose;
        Bytes = bytes;
        CreatedAt = createdAt;
        Status = status;
        ExpiresAt = expiresAt;
    }

    /// <summary>File id.</summary>
    public string Id { get; }

    /// <summary>File name.</summary>
    public string? Filename { get; }

    /// <summary>Upload purpose.</summary>
    public string? Purpose { get; }

    /// <summary>Size in bytes.</summary>
    public int? Bytes { get; }

    /// <summary>Creation time.</summary>
    public DateTimeOffset? CreatedAt { get; }

    /// <summary>Provider status.</summary>
    public string? Status { get; }

    /// <summary>Expiry time. Null when the upload has none.</summary>
    public DateTimeOffset? ExpiresAt { get; }
}

/// <summary>OpenAI file store.</summary>
public sealed class OpenAIFileStore : IFileStore
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a file store.</summary>
    public OpenAIFileStore(OpenAIProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>Provider id.</summary>
    public string Provider => _provider.Name + ".files";

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        var metadata = await UploadAsync(fileName, data, mediaType, "assistants", null, cancellationToken).ConfigureAwait(false);
        return new UploadedFile(metadata.Id, metadata.Filename ?? fileName);
    }

    /// <summary>Uploads a file. <paramref name="expiresAfterSeconds"/> sends <c>expires_after</c>.</summary>
    public async Task<OpenAIFileMetadata> UploadAsync(
        string fileName,
        byte[] data,
        string mediaType,
        string? purpose,
        int? expiresAfterSeconds,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(data);
        file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType);
        content.Add(file, "file", fileName);
        content.Add(new StringContent(string.IsNullOrEmpty(purpose) ? "assistants" : purpose!), "purpose");
        if (expiresAfterSeconds != null)
        {
            content.Add(new StringContent("created_at"), "expires_after[anchor]");
            content.Add(new StringContent(expiresAfterSeconds.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "expires_after[seconds]");
        }

        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "files"),
            content,
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
        return Read(document.RootElement);
    }

    /// <summary>Reads file metadata.</summary>
    public async Task<OpenAIFileMetadata> GetMetadataAsync(OpenAIFileReference file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "files/" + OpenAIJson.EncodePathSegment(id)),
            null,
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Read(document.RootElement);
    }

    /// <summary>Downloads file bytes. The media type has parameters removed.</summary>
    public async Task<(byte[] Data, string? MediaType)> DownloadAsync(OpenAIFileReference file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "files/" + OpenAIJson.EncodePathSegment(id) + "/content"));
        OpenAIJson.ApplyHeaders(request, _provider.CreateOpenAIHeaders());
        using var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw ProviderHttp.MapStatus((int)response.StatusCode, Encoding.UTF8.GetString(bytes));
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        return (bytes, mediaType);
    }

    /// <summary>Deletes a file.</summary>
    public async Task DeleteAsync(OpenAIFileReference file, CancellationToken cancellationToken)
    {
        var id = RequireId(file);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Delete,
            ApiKeys.Combine(_provider.Options.BaseUrl, "files/" + OpenAIJson.EncodePathSegment(id)),
            null,
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        _ = document;
    }

    /// <summary>Returns the OpenAI file id or throws when it is missing.</summary>
    public static string RequireId(OpenAIFileReference file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.OpenAI))
        {
            throw new ArgumentException("file reference is missing an 'openai' file id.", "file");
        }

        return file.OpenAI!;
    }

    private static OpenAIFileMetadata Read(JsonElement root)
    {
        var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
        var filename = root.TryGetProperty("filename", out var filenameElement) && filenameElement.ValueKind == JsonValueKind.String
            ? filenameElement.GetString()
            : null;
        var purpose = root.TryGetProperty("purpose", out var purposeElement) && purposeElement.ValueKind == JsonValueKind.String
            ? purposeElement.GetString()
            : null;
        int? bytes = root.TryGetProperty("bytes", out var bytesElement) && bytesElement.TryGetInt32(out var size) ? size : null;
        var status = root.TryGetProperty("status", out var statusElement) && statusElement.ValueKind == JsonValueKind.String
            ? statusElement.GetString()
            : null;
        DateTimeOffset? expires = null;
        if (root.TryGetProperty("expires_at", out var expiresElement) && expiresElement.ValueKind == JsonValueKind.Number && expiresElement.TryGetInt64(out var expiresAt) && expiresAt != 0)
        {
            expires = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
        }

        return new OpenAIFileMetadata(id, filename, purpose, bytes, OpenAIJson.UnixSeconds(OpenAIJson.Unix(root, "created_at")), status, expires);
    }
}
