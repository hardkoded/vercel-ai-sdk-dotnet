// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Tagged file bytes, text, or a stream.</summary>
public sealed class UploadData
{
    /// <summary>Creates tagged upload data.</summary>
    public UploadData(string type, object? data)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Data = data;
    }

    /// <summary><c>data</c>, <c>text</c>, or <c>stream</c>.</summary>
    public string Type { get; }

    /// <summary>Bytes, base64 text, or a <see cref="UploadStream"/>.</summary>
    public object? Data { get; }
}

/// <summary>A stream the provider may take ownership of.</summary>
public sealed class UploadStream
{
    /// <summary>True after <see cref="Cancel"/>.</summary>
    public bool Cancelled { get; private set; }

    /// <summary>Reason passed to <see cref="Cancel"/>.</summary>
    public Exception? CancelError { get; private set; }

    /// <summary>Releases the stream after a failed upload.</summary>
    public void Cancel(Exception error)
    {
        Cancelled = true;
        CancelError = error;
    }
}

/// <summary>A provider file reference.</summary>
public sealed class ProviderFileReference
{
    /// <summary>Creates a reference.</summary>
    public ProviderFileReference(string id, string? provider = null)
    {
        Id = id ?? string.Empty;
        Provider = provider;
    }

    /// <summary>Provider file id.</summary>
    public string Id { get; }

    /// <summary>Provider name.</summary>
    public string? Provider { get; }
}

/// <summary>Arguments forwarded to a files API.</summary>
public sealed class UploadFileCall
{
    /// <summary>Creates an upload call.</summary>
    public UploadFileCall(UploadData data, string mediaType, string? filename, CancellationToken cancellationToken, IReadOnlyDictionary<string, string>? headers, JsonElement? providerOptions)
    {
        Data = data;
        MediaType = mediaType;
        Filename = filename;
        CancellationToken = cancellationToken;
        Headers = headers;
        ProviderOptions = providerOptions;
    }

    /// <summary>Tagged data.</summary>
    public UploadData Data { get; }

    /// <summary>Media type sent to the provider.</summary>
    public string MediaType { get; }

    /// <summary>Optional filename.</summary>
    public string? Filename { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Request headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Provider options. Null when the caller omitted them.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>Provider upload result.</summary>
public sealed class UploadFileModelResult
{
    /// <summary>Creates a provider result.</summary>
    public UploadFileModelResult(ProviderFileReference providerReference, string? mediaType = null, string? filename = null, long? byteSize = null, DateTime? createdAt = null, DateTime? expiresAt = null, JsonElement? providerMetadata = null, IReadOnlyList<OperationWarning>? warnings = null)
    {
        ProviderReference = providerReference;
        MediaType = mediaType;
        Filename = filename;
        ByteSize = byteSize;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        ProviderMetadata = providerMetadata;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
    }

    /// <summary>Provider reference.</summary>
    public ProviderFileReference ProviderReference { get; }

    /// <summary>Media type reported by the provider.</summary>
    public string? MediaType { get; }

    /// <summary>Filename reported by the provider.</summary>
    public string? Filename { get; }

    /// <summary>Byte size.</summary>
    public long? ByteSize { get; }

    /// <summary>Creation time.</summary>
    public DateTime? CreatedAt { get; }

    /// <summary>Expiry time.</summary>
    public DateTime? ExpiresAt { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }
}

/// <summary>Files API.</summary>
public interface IFileStore
{
    /// <summary>Uploads a file.</summary>
    Task<UploadFileModelResult> UploadFileAsync(UploadFileCall call, CancellationToken cancellationToken);
}

/// <summary>Provider that can expose a files API.</summary>
public interface IFileStoreProvider
{
    /// <summary>Returns the files API, or null when uploads are unsupported.</summary>
    IFileStore? Files();
}

/// <summary>Result of <see cref="UploadFile.UploadFileAsync"/>.</summary>
public sealed class UploadFileResult
{
    /// <summary>Creates an upload result.</summary>
    public UploadFileResult(ProviderFileReference providerReference, string? mediaType, string? filename, long? byteSize, DateTime? createdAt, DateTime? expiresAt, JsonElement? providerMetadata, IReadOnlyList<OperationWarning> warnings)
    {
        ProviderReference = providerReference;
        MediaType = mediaType;
        Filename = filename;
        ByteSize = byteSize;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        ProviderMetadata = providerMetadata;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
    }

    /// <summary>Provider reference.</summary>
    public ProviderFileReference ProviderReference { get; }

    /// <summary>Media type.</summary>
    public string? MediaType { get; }

    /// <summary>Filename.</summary>
    public string? Filename { get; }

    /// <summary>Byte size.</summary>
    public long? ByteSize { get; }

    /// <summary>Creation time.</summary>
    public DateTime? CreatedAt { get; }

    /// <summary>Expiry time.</summary>
    public DateTime? ExpiresAt { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }
}

/// <summary>Options for <see cref="UploadFile.UploadFileAsync"/>.</summary>
public sealed class UploadFileRequest
{
    /// <summary>Files API or a provider that exposes one.</summary>
    public object? Api { get; set; }

    /// <summary>Tagged data, bytes, or a string shorthand.</summary>
    public object? Data { get; set; }

    /// <summary>Optional media type. Detected when omitted.</summary>
    public string? MediaType { get; set; }

    /// <summary>Optional filename.</summary>
    public string? Filename { get; set; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Request headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider options. Null when omitted.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>True when the caller set <see cref="ProviderOptions"/>, including an explicit null.</summary>
    public bool ProviderOptionsSpecified { get; set; }
}

/// <summary>Uploads a file. Maps to <c>uploadFile</c>.</summary>
public static class UploadFile
{
    /// <summary>Uploads <see cref="UploadFileRequest.Data"/>.</summary>
    public static async Task<UploadFileResult> UploadFileAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var data = Normalize(request.Data);
        var mediaType = request.MediaType ?? DefaultMediaType(data);
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        try
        {
            var store = Resolve(request.Api);
            var result = await store.UploadFileAsync(new UploadFileCall(data, mediaType, request.Filename, token, request.Headers, request.ProviderOptionsSpecified ? request.ProviderOptions : null), token).ConfigureAwait(false);
            return new UploadFileResult(result.ProviderReference, result.MediaType, result.Filename, result.ByteSize, result.CreatedAt, result.ExpiresAt, result.ProviderMetadata, result.Warnings);
        }
        catch (Exception error)
        {
            if (data.Type == "stream" && data.Data is UploadStream stream)
            {
                try
                {
                    stream.Cancel(error);
                }
                catch (Exception)
                {
                    // A locked stream stays owned by the provider.
                }
            }

            throw;
        }
    }

    private static UploadData Normalize(object? data)
    {
        if (data is UploadData tagged)
        {
            return tagged;
        }

        if (data is byte[] || data is string)
        {
            return new UploadData("data", data);
        }

        throw new InvalidArgumentException("data", data, "data must be bytes, a string, or tagged data");
    }

    private static string DefaultMediaType(UploadData data)
    {
        if (data.Type == "text")
        {
            return "text/plain";
        }

        if (data.Type == "stream")
        {
            return "application/octet-stream";
        }

        var bytes = data.Data is byte[] raw ? raw : data.Data is string text ? DecodeSample(text) : Array.Empty<byte>();
        return MediaTypeDetector.Detect(bytes, null) ?? (LikelyText(bytes) ? "text/plain" : "application/octet-stream");
    }

    private static byte[] DecodeSample(string data)
    {
        var length = Math.Min(data.Length, ((512 + 4 + 2) / 3) * 4);
        try
        {
            return Convert.FromBase64String(data.Substring(0, length - (length % 4)));
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }

    private static bool LikelyText(byte[] bytes)
    {
        var check = Math.Min(bytes.Length, 512);
        if (check == 0)
        {
            return false;
        }

        for (var i = 0; i < check; i++)
        {
            var value = bytes[i];
            if (value == 0 || (value < 0x20 && value != 0x09 && value != 0x0A && value != 0x0D))
            {
                return false;
            }
        }

        return true;
    }

    private static IFileStore Resolve(object? api)
    {
        if (api is IFileStore store)
        {
            return store;
        }

        if (api is IFileStoreProvider provider)
        {
            var files = provider.Files();
            if (files != null)
            {
                return files;
            }
        }

        throw new InvalidOperationException("The provider does not support file uploads. Make sure it exposes a files() method.");
    }
}

/// <summary>One file inside a skill upload.</summary>
public sealed class SkillFile
{
    /// <summary>Creates a skill file.</summary>
    public SkillFile(string? path, object? data, string? mediaType = null)
    {
        Path = path;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary>Path inside the skill.</summary>
    public string? Path { get; }

    /// <summary>Tagged data or a bytes/string shorthand.</summary>
    public object? Data { get; }

    /// <summary>Optional media type.</summary>
    public string? MediaType { get; }
}

/// <summary>Normalized skill file passed to the provider.</summary>
public sealed class SkillFilePayload
{
    /// <summary>Creates a payload.</summary>
    public SkillFilePayload(string? path, UploadData data, string? mediaType)
    {
        Path = path;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary>Path inside the skill.</summary>
    public string? Path { get; }

    /// <summary>Tagged data.</summary>
    public UploadData Data { get; }

    /// <summary>Media type.</summary>
    public string? MediaType { get; }
}

/// <summary>Arguments for a skill upload.</summary>
public sealed class UploadSkillCall
{
    /// <summary>Creates a skill call.</summary>
    public UploadSkillCall(IReadOnlyList<SkillFilePayload> files, string? displayTitle, JsonElement? providerOptions)
    {
        Files = files ?? Array.Empty<SkillFilePayload>();
        DisplayTitle = displayTitle;
        ProviderOptions = providerOptions;
    }

    /// <summary>Normalized files.</summary>
    public IReadOnlyList<SkillFilePayload> Files { get; }

    /// <summary>Display title.</summary>
    public string? DisplayTitle { get; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>Skill upload result.</summary>
public sealed class UploadSkillResult
{
    /// <summary>Creates a skill result.</summary>
    public UploadSkillResult(ProviderFileReference providerReference, IReadOnlyList<OperationWarning>? warnings = null)
    {
        ProviderReference = providerReference;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
    }

    /// <summary>Provider reference.</summary>
    public ProviderFileReference ProviderReference { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }
}

/// <summary>Skills API.</summary>
public interface ISkillStore
{
    /// <summary>Uploads a skill.</summary>
    Task<UploadSkillResult> UploadSkillAsync(UploadSkillCall call, CancellationToken cancellationToken);
}

/// <summary>Provider that can expose a skills API.</summary>
public interface ISkillStoreProvider
{
    /// <summary>Returns the skills API, or null when skills are unsupported.</summary>
    ISkillStore? Skills();
}

/// <summary>Options for <see cref="UploadSkill.UploadSkillAsync"/>.</summary>
public sealed class UploadSkillRequest
{
    /// <summary>Skills API or a provider that exposes one.</summary>
    public object? Api { get; set; }

    /// <summary>Skill files.</summary>
    public IReadOnlyList<SkillFile> Files { get; set; } = Array.Empty<SkillFile>();

    /// <summary>Display title.</summary>
    public string? DisplayTitle { get; set; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; set; }
}

/// <summary>Uploads a skill. Maps to <c>uploadSkill</c>.</summary>
public static class UploadSkill
{
    /// <summary>Uploads the skill files.</summary>
    public static async Task<UploadSkillResult> UploadSkillAsync(UploadSkillRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var store = Resolve(request.Api);
        var files = new List<SkillFilePayload>();
        foreach (var file in request.Files)
        {
            var data = file.Data is UploadData tagged ? tagged : new UploadData("data", file.Data);
            files.Add(new SkillFilePayload(file.Path, data, file.MediaType));
        }

        return await store.UploadSkillAsync(new UploadSkillCall(files, request.DisplayTitle, request.ProviderOptions), cancellationToken).ConfigureAwait(false);
    }

    private static ISkillStore Resolve(object? api)
    {
        if (api is ISkillStore store)
        {
            return store;
        }

        if (api is ISkillStoreProvider provider)
        {
            var skills = provider.Skills();
            if (skills != null)
            {
                return skills;
            }
        }

        throw new InvalidOperationException("The provider does not support skills. Make sure it exposes a skills() method.");
    }
}
