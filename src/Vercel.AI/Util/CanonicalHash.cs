// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>Canonical JSON and its SHA-256 digest. Maps to <c>canonicalJSON</c> and <c>hashCanonical</c>.</summary>
public static class CanonicalHash
{
    /// <summary>
    /// Serializes <paramref name="value"/> with object keys sorted at every level.
    /// <see cref="JsonUndefined"/> serializes as a missing result (<c>null</c> in C#), matching
    /// <c>JSON.stringify(undefined)</c>. Undefined array elements become JSON <c>null</c>.
    /// </summary>
    public static string? CanonicalJson(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is JsonUndefined)
        {
            return null;
        }

        if (value is string || IsNumber(value) || value is bool)
        {
            return JsonSerializer.Serialize(value);
        }

        if (value is IList list && value is not string)
        {
            var parts = new string[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                parts[i] = element is JsonUndefined ? "null" : CanonicalJson(element) ?? "undefined";
            }

            return "[" + string.Join(",", parts) + "]";
        }

        if (value is IDictionary dictionary)
        {
            var keys = new List<string>(dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string key)
                {
                    keys.Add(key);
                }
            }

            keys.Sort(StringComparer.Ordinal);
            var entries = new string[keys.Count];
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var nested = CanonicalJson(dictionary[key]);
                entries[i] = JsonSerializer.Serialize(key) + ":" + (nested ?? "undefined");
            }

            return "{" + string.Join(",", entries) + "}";
        }

        return JsonSerializer.Serialize(value);
    }

    /// <summary>SHA-256 of <see cref="CanonicalJson"/>, encoded as unpadded base64url.</summary>
    public static string HashCanonical(object? value)
    {
        var canonical = CanonicalJson(value) ?? "undefined";
        using (var sha = SHA256.Create())
        {
            var digest = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            return ToBase64Url(digest);
        }
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static bool IsNumber(object value)
    {
        return value is byte
            || value is sbyte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal;
    }
}
