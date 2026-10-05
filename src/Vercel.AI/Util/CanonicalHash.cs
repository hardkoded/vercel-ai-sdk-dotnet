// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>Canonical JSON and its SHA-256 base64url digest.</summary>
public static class CanonicalHash
{
    /// <summary>
    /// Serializes <paramref name="value"/> with object keys sorted.
    /// <see cref="JsUndefined.Value"/> returns <c>null</c>, matching <c>JSON.stringify(undefined)</c>.
    /// </summary>
    public static string? CanonicalJson(object? value)
    {
        if (value is JsUndefined)
        {
            return null;
        }

        if (value == null || IsPrimitive(value))
        {
            return JsonSerializer.Serialize(value);
        }

        var list = value as IList;
        if (list != null && !(value is string))
        {
            var parts = new string[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                var canonical = element is JsUndefined ? "null" : CanonicalJson(element);
                parts[i] = canonical ?? "null";
            }

            return "[" + string.Join(",", parts) + "]";
        }

        var dictionary = value as IDictionary;
        if (dictionary != null)
        {
            var keys = new List<string>();
            foreach (var key in dictionary.Keys)
            {
                keys.Add(Convert.ToString(key, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
            }

            keys.Sort(StringComparer.Ordinal);
            var entries = new List<string>(keys.Count);
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var inner = CanonicalJson(dictionary[key]);
                if (inner == null)
                {
                    inner = "undefined";
                }

                entries.Add(JsonSerializer.Serialize(key) + ":" + inner);
            }

            return "{" + string.Join(",", entries) + "}";
        }

        return JsonSerializer.Serialize(value);
    }

    /// <summary>SHA-256 digest of <see cref="CanonicalJson"/>, encoded as base64url without padding.</summary>
    public static string HashCanonical(object? value)
    {
        var json = CanonicalJson(value) ?? "undefined";
        using (var sha = SHA256.Create())
        {
            var digest = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            return ToBase64Url(digest);
        }
    }

    /// <summary>Encodes <paramref name="bytes"/> as base64url without padding.</summary>
    public static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static bool IsPrimitive(object value)
    {
        return value is string || value is bool || value is byte || value is sbyte
            || value is short || value is ushort || value is int || value is uint
            || value is long || value is ulong || value is float || value is double
            || value is decimal;
    }
}
