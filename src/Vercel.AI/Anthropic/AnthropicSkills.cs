// Copyright 2023 Vercel, Inc.
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
    public AnthropicSkillUpload(string id, string? name, JsonElement providerReference)
    {
        Id = id;
        Name = name;
        ProviderReference = providerReference;
    }

    /// <summary>Skill id.</summary>
    public string Id { get; }

    /// <summary>Skill name from the version metadata, when present.</summary>
    public string? Name { get; }

    /// <summary><c>providerReference.anthropic</c> set to the skill id.</summary>
    public JsonElement ProviderReference { get; }
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

    /// <summary>
    /// Encodes one URL path segment. <c>.</c> and <c>..</c> become <c>%252E</c> and <c>%252E%252E</c>
    /// so they cannot traverse directories. Other characters use URI escaping.
    /// </summary>
    public static string EncodePathSegment(string value)
    {
        if (value == ".")
        {
            return "%252E";
        }

        if (value == "..")
        {
            return "%252E%252E";
        }

        return Uri.EscapeDataString(value ?? string.Empty);
    }

    /// <summary>Uploads skill files, then reads the latest version metadata.</summary>
    public async Task<AnthropicSkillUpload> UploadSkillAsync(IReadOnlyList<AnthropicSkillFile> files, string? displayTitle, CancellationToken cancellationToken)
    {
        var form = new MultipartFormDataContent();
        if (!string.IsNullOrEmpty(displayTitle))
        {
            form.Add(new StringContent(displayTitle!), "display_title");
        }

        foreach (var file in files)
        {
            var content = new ByteArrayContent(file.Data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "files[]", file.Path);
            content.Headers.Remove("Content-Disposition");
            content.Headers.TryAddWithoutValidation("Content-Disposition", "form-data; name=\"files[]\"; filename=\"" + file.Path + "\"");
        }

        var headers = WithBeta(_headers());
        var created = await _http.SendBytesAsync(HttpMethod.Post, new Uri(ApiKeys.Combine(_baseUrl(), "skills").AbsoluteUri), form, headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(created));
        var root = document.RootElement;
        var id = AnthropicJson.String(root, "id") ?? string.Empty;
        var version = AnthropicJson.String(root, "latest_version") ?? string.Empty;
        var versionUri = ApiKeys.Combine(_baseUrl(), "skills/" + EncodePathSegment(id) + "/versions/" + EncodePathSegment(version));
        var metadataResponse = await _http.SendJsonStringAsync(HttpMethod.Get, versionUri, null, headers, cancellationToken).ConfigureAwait(false);
        string? name = null;
        var metadata = metadataResponse.Body;
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            using var versionDocument = JsonDocument.Parse(metadata);
            name = AnthropicJson.String(versionDocument.RootElement, "name");
        }

        using var reference = JsonDocument.Parse("{\"anthropic\":" + JsonSerializer.Serialize(id) + "}");
        return new AnthropicSkillUpload(id, name, reference.RootElement.Clone());
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
