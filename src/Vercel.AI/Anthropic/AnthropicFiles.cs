// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

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
    public async Task<AnthropicUploadedFile> UploadAsync(string fileName, byte[] data, string mediaType, bool? downloadable, CancellationToken cancellationToken)
    {
        var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(data ?? Array.Empty<byte>());
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType);
        var name = string.IsNullOrEmpty(fileName) ? "blob" : fileName;
        form.Add(content, "file", name);
        content.Headers.Remove("Content-Disposition");
        content.Headers.TryAddWithoutValidation("Content-Disposition", "form-data; name=file; filename=\"" + name + "\"");
        if (downloadable != null)
        {
            form.Add(new StringContent(downloadable.Value ? "true" : "false"), "downloadable");
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        headers["anthropic-beta"] = "files-api-2025-04-14";
        var bytes = await _http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_baseUrl(), "files"), form, headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(bytes));
        var root = document.RootElement;
        return new AnthropicUploadedFile(
            AnthropicJson.String(root, "id") ?? string.Empty,
            AnthropicJson.String(root, "filename"),
            root.Clone());
    }
}

/// <summary>Files API upload result.</summary>
public sealed class AnthropicUploadedFile
{
    /// <summary>Creates an upload result.</summary>
    public AnthropicUploadedFile(string id, string? fileName, JsonElement raw)
    {
        Id = id;
        FileName = fileName;
        Raw = raw;
    }

    /// <summary>File id.</summary>
    public string Id { get; }

    /// <summary>File name.</summary>
    public string? FileName { get; }

    /// <summary>Raw response.</summary>
    public JsonElement Raw { get; }
}
