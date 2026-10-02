// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>One file uploaded as part of a skill.</summary>
public sealed class OpenAISkillFile
{
    /// <summary>Creates a skill file. <paramref name="path"/> is the multipart filename.</summary>
    public OpenAISkillFile(string path, byte[] data, string mediaType)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MediaType = mediaType;
    }

    /// <summary>Relative path sent as the filename.</summary>
    public string Path { get; }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}

/// <summary>Skill upload result.</summary>
public sealed class OpenAISkillUpload
{
    internal OpenAISkillUpload(string id, string? name, string? description, int? latestVersion, int? defaultVersion, long? createdAt, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Id = id;
        Name = name;
        Description = description;
        LatestVersion = latestVersion;
        DefaultVersion = defaultVersion;
        CreatedAt = createdAt;
        Warnings = warnings;
    }

    /// <summary>Skill id.</summary>
    public string Id { get; }

    /// <summary>Skill name.</summary>
    public string? Name { get; }

    /// <summary>Skill description.</summary>
    public string? Description { get; }

    /// <summary>Latest version.</summary>
    public int? LatestVersion { get; }

    /// <summary>Default version.</summary>
    public int? DefaultVersion { get; }

    /// <summary>Creation time as unix seconds.</summary>
    public long? CreatedAt { get; }

    /// <summary>Warnings produced while preparing the upload.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI skill store.</summary>
public sealed class OpenAISkillStore : ISkillStore
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a skill store.</summary>
    public OpenAISkillStore(OpenAIProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>Provider id.</summary>
    public string Provider => _provider.Name + ".skills";

    /// <inheritdoc />
    public async Task<UploadedSkill> UploadSkillAsync(string name, string instructions, CancellationToken cancellationToken)
    {
        var file = new OpenAISkillFile("SKILL.md", Encoding.UTF8.GetBytes(instructions ?? string.Empty), "text/markdown");
        var result = await UploadAsync(new[] { file }, null, cancellationToken).ConfigureAwait(false);
        return new UploadedSkill(result.Id, result.Name ?? name);
    }

    /// <summary>Uploads skill files as <c>files[]</c>. <paramref name="displayTitle"/> is warned and omitted.</summary>
    public async Task<OpenAISkillUpload> UploadAsync(IReadOnlyList<OpenAISkillFile> files, string? displayTitle, CancellationToken cancellationToken)
    {
        var warnings = new List<OpenAICallWarning>();
        if (displayTitle != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "displayTitle", null));
        }

        using var content = new MultipartFormDataContent();
        foreach (var file in files)
        {
            var bytes = new ByteArrayContent(file.Data);
            bytes.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(file.MediaType) ? "application/octet-stream" : file.MediaType);
            content.Add(bytes, "files[]", file.Path);
        }

        var raw = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "skills"),
            content,
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(raw));
        var root = document.RootElement;
        var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
        var name = root.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String ? nameElement.GetString() : null;
        var description = root.TryGetProperty("description", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String
            ? descriptionElement.GetString()
            : null;
        int? latest = root.TryGetProperty("latest_version", out var latestElement) && latestElement.TryGetInt32(out var latestValue) ? latestValue : null;
        int? defaultVersion = root.TryGetProperty("default_version", out var defaultElement) && defaultElement.TryGetInt32(out var defaultValue) ? defaultValue : null;
        long? created = OpenAIJson.Unix(root, "created_at");
        return new OpenAISkillUpload(id, name, description, latest, defaultVersion, created, warnings);
    }
}
