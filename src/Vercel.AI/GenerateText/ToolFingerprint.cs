// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.GenerateText;

/// <summary>A tool definition pinned by <see cref="ToolFingerprints.FingerprintTools"/>.</summary>
public sealed class FingerprintTool
{
    /// <summary>Creates a tool definition.</summary>
    public FingerprintTool(JsonElement inputSchema, string? description = null, bool descriptionIsFunction = false, string? title = null)
    {
        InputSchema = inputSchema;
        Description = description;
        DescriptionIsFunction = descriptionIsFunction;
        Title = title;
    }

    /// <summary>JSON Schema for the tool input.</summary>
    public JsonElement InputSchema { get; }

    /// <summary>String description. Ignored when <see cref="DescriptionIsFunction"/> is true.</summary>
    public string? Description { get; }

    /// <summary>True when the description is a function. Only the presence of a function is pinned.</summary>
    public bool DescriptionIsFunction { get; }

    /// <summary>Tool title.</summary>
    public string? Title { get; }
}

/// <summary>Added, removed, and changed tool names.</summary>
public sealed class ToolDrift
{
    /// <summary>Creates a drift report.</summary>
    public ToolDrift(IReadOnlyList<string> added, IReadOnlyList<string> removed, IReadOnlyList<string> changed)
    {
        Added = added ?? Array.Empty<string>();
        Removed = removed ?? Array.Empty<string>();
        Changed = changed ?? Array.Empty<string>();
    }

    /// <summary>Tools present only in the current map.</summary>
    public IReadOnlyList<string> Added { get; }

    /// <summary>Tools present only in the baseline.</summary>
    public IReadOnlyList<string> Removed { get; }

    /// <summary>Tools whose digest changed.</summary>
    public IReadOnlyList<string> Changed { get; }
}

/// <summary>Tool-definition fingerprints. Maps to <c>fingerprintTools</c> and <c>detectToolDrift</c>.</summary>
public static class ToolFingerprints
{
    /// <summary>Returns a stable digest for each tool. Function descriptions hash as a single placeholder.</summary>
    public static IReadOnlyDictionary<string, string> FingerprintTools(IReadOnlyDictionary<string, FingerprintTool> tools)
    {
        if (tools is null)
        {
            throw new ArgumentNullException(nameof(tools));
        }

        var digests = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in tools)
        {
            digests.Add(pair.Key, Digest(pair.Value));
        }

        return digests;
    }

    /// <summary>Diffs two fingerprint maps. Names are compared as own keys.</summary>
    public static ToolDrift DetectToolDrift(IReadOnlyDictionary<string, string> current, IReadOnlyDictionary<string, string> baseline)
    {
        if (current is null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        if (baseline is null)
        {
            throw new ArgumentNullException(nameof(baseline));
        }

        var added = new List<string>();
        var changed = new List<string>();
        foreach (var pair in current)
        {
            if (!baseline.ContainsKey(pair.Key))
            {
                added.Add(pair.Key);
            }
            else if (!string.Equals(pair.Value, baseline[pair.Key], StringComparison.Ordinal))
            {
                changed.Add(pair.Key);
            }
        }

        var removed = new List<string>();
        foreach (var pair in baseline)
        {
            if (!current.ContainsKey(pair.Key))
            {
                removed.Add(pair.Key);
            }
        }

        return new ToolDrift(added, removed, changed);
    }

    private static string Digest(FingerprintTool tool)
    {
        var description = tool.DescriptionIsFunction
            ? "{\"type\":\"function\"}"
            : tool.Description is null
                ? "{\"type\":\"none\"}"
                : "{\"type\":\"string\",\"value\":" + JsonSerializer.Serialize(tool.Description) + "}";
        var canonical = "{\"description\":" + description + ",\"inputSchema\":" + CanonicalHash.CanonicalJson(tool.InputSchema) + ",\"title\":" + (tool.Title is null ? "null" : JsonSerializer.Serialize(tool.Title)) + "}";
        using var sha = SHA256.Create();
        return CanonicalHash.ToBase64Url(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
    }
}
