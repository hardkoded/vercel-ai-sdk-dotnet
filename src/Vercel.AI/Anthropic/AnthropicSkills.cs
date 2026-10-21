// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>A skill file sent as multipart <c>files[]</c>.</summary>
public sealed class AnthropicSkillFile
{
    /// <summary>Creates a skill file.</summary>
    public AnthropicSkillFile(string path, byte[] data)
    {
        Path = path ?? "skill";
        Data = data ?? Array.Empty<byte>();
    }

    /// <summary>Relative path inside the skill.</summary>
    public string Path { get; }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }
}

/// <summary>Result of uploading a skill.</summary>
public sealed class AnthropicSkillUpload
{
    /// <summary>Creates an upload result.</summary>
    public AnthropicSkillUpload(string id, string? name, IReadOnlyDictionary<string, string> providerReference)
    {
        Id = id;
        Name = name;
        ProviderReference = providerReference ?? throw new ArgumentNullException(nameof(providerReference));
    }

    /// <summary>Skill id.</summary>
    public string Id { get; }

    /// <summary>Skill name from the version metadata, when present.</summary>
    public string? Name { get; }

    /// <summary>Map from provider name to the skill id. <c>anthropic</c> is set to the skill id.</summary>
    public IReadOnlyDictionary<string, string> ProviderReference { get; }
}

/// <summary>Anthropic Skills API.</summary>
public sealed class AnthropicSkills
{
    private readonly ProviderHttp _http;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;
    private readonly Func<string> _baseUrl;

    /// <summary>Creates a skills client.</summary>
    public AnthropicSkills(ProviderHttp http, Func<string> baseUrl, Func<IReadOnlyDictionary<string, string?>> headers, string provider)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Provider = provider ?? "anthropic.skills";
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Uploads skill files, then reads the latest version metadata.</summary>
    public async Task<AnthropicSkillUpload> UploadSkillAsync(IReadOnlyList<AnthropicSkillFile> files, string? displayTitle, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        if (!string.IsNullOrEmpty(displayTitle))
        {
            form.Add(new StringContent(displayTitle!), "display_title");
        }

        foreach (var file in files)
        {
            var content = new ByteArrayContent(file.Data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            MultipartForm.AddFile(form, content, "files[]", file.Path);
        }

        var headers = WithBeta(_headers());
        var created = await _http.SendBytesAsync(HttpMethod.Post, new Uri(ApiKeys.Combine(_baseUrl(), "skills").AbsoluteUri), form, headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(created));
        var root = document.RootElement;
        var id = AnthropicJson.String(root, "id") ?? string.Empty;
        var version = AnthropicJson.String(root, "latest_version") ?? string.Empty;
        var versionUri = ApiKeys.Combine(_baseUrl(), "skills/" + PathSegments.Encode(id) + "/versions/" + PathSegments.Encode(version));
        var metadataResponse = await _http.SendJsonStringAsync(HttpMethod.Get, versionUri, null, headers, cancellationToken).ConfigureAwait(false);
        string? name = null;
        var metadata = metadataResponse.Body;
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            using var versionDocument = JsonDocument.Parse(metadata);
            name = AnthropicJson.String(versionDocument.RootElement, "name");
        }

        return new AnthropicSkillUpload(id, name, new Dictionary<string, string> { ["anthropic"] = id });
    }

    private static Dictionary<string, string?> WithBeta(IReadOnlyDictionary<string, string?> headers)
    {
        var copy = new Dictionary<string, string?>();
        foreach (var pair in headers)
        {
            copy[pair.Key] = pair.Value;
        }

        copy["anthropic-beta"] = "skills-2025-10-02";
        return copy;
    }
}
