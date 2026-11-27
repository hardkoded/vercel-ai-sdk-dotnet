// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>JSON helpers for Anthropic request and response bodies.</summary>
internal static class AnthropicJson
{
    private static readonly System.Text.RegularExpressions.Regex s_surrogatePair = new(
        @"(?<!\\)((?:\\\\)*)\\u(D[89AB][0-9A-F]{2})\\u(D[C-F][0-9A-F]{2})",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | System.Text.RegularExpressions.RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    public static JsonObject Object()
    {
        return new JsonObject();
    }

    public static void Set(JsonObject target, string name, JsonNode? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    public static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    public static void Set(JsonObject target, string name, bool? value)
    {
        if (value != null)
        {
            target[name] = value.Value;
        }
    }

    public static void Set(JsonObject target, string name, int? value)
    {
        if (value != null)
        {
            target[name] = value.Value;
        }
    }

    public static void Set(JsonObject target, string name, double? value)
    {
        if (value != null)
        {
            target[name] = value.Value;
        }
    }

    public static JsonNode? Clone(JsonNode? node)
    {
        return node?.DeepClone();
    }

    public static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static JsonObject ParseObject(string json)
    {
        return JsonNode.Parse(json) as JsonObject ?? new JsonObject();
    }

    public static bool IsObject(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Object;
    }

    public static bool TryGet(JsonElement element, string name, out JsonElement value)
    {
        value = default;
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value);
    }

    public static JsonElement? Property(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var name in names)
        {
            if (
                element.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null
                && value.ValueKind != JsonValueKind.Undefined
            )
            {
                return value;
            }
        }

        return null;
    }

    public static string? String(JsonElement element, params string[] names)
    {
        var value = Property(element, names);
        if (value == null)
        {
            return null;
        }

        return value.Value.ValueKind == JsonValueKind.String
            ? value.Value.GetString()
            : value.Value.ToString();
    }

    public static bool? Bool(JsonElement element, params string[] names)
    {
        var value = Property(element, names);
        if (value == null)
        {
            return null;
        }

        if (value.Value.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        if (value.Value.ValueKind == JsonValueKind.False)
        {
            return false;
        }

        return null;
    }

    public static int? Int(JsonElement element, params string[] names)
    {
        var value = Property(element, names);
        if (value == null || value.Value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.Value.TryGetInt32(out var number) ? number : null;
    }

    public static double? Double(JsonElement element, params string[] names)
    {
        var value = Property(element, names);
        if (value == null || value.Value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.Value.GetDouble();
    }

    public static JsonElement? Anthropic(JsonElement? providerOptions)
    {
        if (providerOptions == null || providerOptions.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return Property(providerOptions.Value, "anthropic");
    }

    public static JsonNode Node(JsonElement element)
    {
        return JsonNode.Parse(element.GetRawText()) ?? new JsonObject();
    }

    // Maps JSON.stringify: no whitespace, and non-ASCII text is not escaped.
    public static string Compact(JsonNode node)
    {
        return UnescapeSurrogatePairs(
            node.ToJsonString(
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }
            )
        );
    }

    // Maps JSON.stringify: no whitespace, and non-ASCII text is not escaped.
    public static string Compact(JsonElement element)
    {
        using var stream = new System.IO.MemoryStream();
        using (
            var writer = new Utf8JsonWriter(
                stream,
                new JsonWriterOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }
            )
        )
        {
            element.WriteTo(writer);
        }

        return UnescapeSurrogatePairs(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }

    // The encoder of System.Text.Json always escapes characters outside the basic plane, such as emoji.
    // JSON.stringify keeps them. An escape that follows an odd number of backslashes is text, not an escape.
    private static string UnescapeSurrogatePairs(string json)
    {
        return s_surrogatePair.Replace(json, match =>
            match.Groups[1].Value
            + new string(
                new[]
                {
                    (char)System.Convert.ToInt32(match.Groups[2].Value, 16),
                    (char)System.Convert.ToInt32(match.Groups[3].Value, 16),
                }
            )
        );
    }

    public static bool LooksLikeUrl(string value)
    {
        return value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
    }

    public static string FormatNumber(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }
}
