// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.GenerateText;

/// <summary>Canonical JSON and SHA-256 digests used by tool fingerprints and approval signatures.</summary>
public static class CanonicalHash
{
    /// <summary>Serializes <paramref name="value"/> with sorted object keys.</summary>
    public static string CanonicalJson(JsonElement value)
    {
        return Write(value);
    }

    /// <summary>SHA-256 digest of <see cref="CanonicalJson"/>, encoded as unpadded base64url.</summary>
    public static string HashCanonical(JsonElement value)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Write(value)));
        return ToBase64Url(hash);
    }

    /// <summary>Encodes <paramref name="bytes"/> as unpadded base64url.</summary>
    public static string ToBase64Url(byte[] bytes)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    internal static byte[] FromBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        return Convert.FromBase64String(padded);
    }

    private static string Write(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return "null";
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.String:
                return JsonSerializer.Serialize(value.GetString());
            case JsonValueKind.Number:
                return value.GetRawText();
            case JsonValueKind.Array:
                var items = new List<string>();
                foreach (var item in value.EnumerateArray())
                {
                    items.Add(Write(item));
                }

                return "[" + string.Join(",", items) + "]";
            case JsonValueKind.Object:
                var properties = new List<JsonProperty>();
                foreach (var property in value.EnumerateObject())
                {
                    properties.Add(property);
                }

                properties.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));
                var parts = new List<string>(properties.Count);
                foreach (var property in properties)
                {
                    parts.Add(JsonSerializer.Serialize(property.Name) + ":" + Write(property.Value));
                }

                return "{" + string.Join(",", parts) + "}";
            default:
                return "null";
        }
    }
}
