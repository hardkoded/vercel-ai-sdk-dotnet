// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Mcp;

/// <summary>Optional CSP and permission metadata for an MCP App HTML resource.</summary>
public sealed class McpAppResourceMeta
{
    /// <summary>Content security policy object. Null when the resource does not declare one.</summary>
    public JsonNode? Csp { get; set; }

    /// <summary>Permission object. Null when the resource does not declare one.</summary>
    public JsonNode? Permissions { get; set; }
}

/// <summary>HTML resource rendered as an MCP App.</summary>
public sealed class McpAppResource
{
    /// <summary>MIME type for HTML resources that hosts render as MCP Apps.</summary>
    public const string MimeTypeValue = "text/html;profile=mcp-app";

    /// <summary>Creates a resource.</summary>
    public McpAppResource(string uri, string html, string? mimeType = null)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Html = html ?? string.Empty;
        MimeType = mimeType ?? MimeTypeValue;
    }

    /// <summary>Resource URI.</summary>
    public string Uri { get; }

    /// <summary>Resource MIME type.</summary>
    public string MimeType { get; }

    /// <summary>HTML document.</summary>
    public string Html { get; }

    /// <summary>CSP and permissions. Null when the resource has no <c>_meta</c>.</summary>
    public McpAppResourceMeta? Meta { get; set; }
}

/// <summary>Stable digest of an MCP App resource's HTML, CSP, and permissions.</summary>
public static class McpAppFingerprint
{
    /// <summary>
    /// Returns the base64url SHA-256 of the canonical JSON for <paramref name="resource"/> HTML, CSP, and permissions.
    /// Object key order does not change the digest.
    /// </summary>
    public static Task<string> FingerprintAsync(McpAppResource resource)
    {
        if (resource is null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        var json = "{\"csp\":" + Canonical(resource.Meta?.Csp) + ",\"html\":" + Canonical(JsonValue.Create(resource.Html)) + ",\"permissions\":" + Canonical(resource.Meta?.Permissions) + "}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Task.FromResult(ToBase64Url(hash));
    }

    /// <summary>Returns true when <paramref name="current"/> differs from <paramref name="baseline"/>.</summary>
    public static bool DetectDrift(string current, string baseline)
    {
        return !string.Equals(current, baseline, StringComparison.Ordinal);
    }

    private static string Canonical(JsonNode? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is JsonObject obj)
        {
            var keys = new List<string>();
            foreach (var pair in obj)
            {
                keys.Add(pair.Key);
            }

            keys.Sort(StringComparer.Ordinal);
            var parts = new string[keys.Count];
            for (var index = 0; index < keys.Count; index++)
            {
                var key = keys[index];
                parts[index] = JsonSerializer.Serialize(key) + ":" + Canonical(obj[key]);
            }

            return "{" + string.Join(",", parts) + "}";
        }

        if (value is JsonArray array)
        {
            var parts = new string[array.Count];
            for (var index = 0; index < array.Count; index++)
            {
                parts[index] = Canonical(array[index]);
            }

            return "[" + string.Join(",", parts) + "]";
        }

        return value.ToJsonString();
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
