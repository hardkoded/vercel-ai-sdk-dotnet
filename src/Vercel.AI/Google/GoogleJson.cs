// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Vercel.AI.Google;

/// <summary>JSON settings shared by Gemini request builders.</summary>
public static class GoogleJson
{
    /// <summary>
    /// Serializes like <c>JSON.stringify</c>: no null properties, and non-ASCII characters stay unescaped.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Serializes metadata and keeps explicit nulls.</summary>
    public static JsonSerializerOptions Metadata { get; } = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Writes a node with <see cref="Options"/>.</summary>
    public static string Write(JsonNode node)
    {
        return node.ToJsonString(Options);
    }

    /// <summary>Parses a JSON object, or returns an empty object.</summary>
    public static JsonObject ParseObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        return JsonNode.Parse(json) as JsonObject ?? new JsonObject();
    }

    /// <summary>Assigns a property when the value is present.</summary>
    public static void Set(JsonObject target, string name, JsonNode? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    /// <summary>Assigns a string when it is non-null.</summary>
    public static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    /// <summary>Assigns a number when it is present.</summary>
    public static void Set(JsonObject target, string name, double? value)
    {
        if (value is { } number)
        {
            target[name] = number;
        }
    }

    /// <summary>Assigns an integer when it is present.</summary>
    public static void Set(JsonObject target, string name, int? value)
    {
        if (value is { } number)
        {
            target[name] = number;
        }
    }

    /// <summary>Assigns a boolean when it is present.</summary>
    public static void Set(JsonObject target, string name, bool? value)
    {
        if (value is { } flag)
        {
            target[name] = flag;
        }
    }

    /// <summary>Clones a JSON element into a node.</summary>
    public static JsonNode? Clone(JsonElement? element)
    {
        if (element is not { } value || value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return null;
        }

        return JsonNode.Parse(value.GetRawText());
    }

    /// <summary>Reads a string property.</summary>
    public static string? String(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : property.GetRawText();
    }

    /// <summary>Reads an object property.</summary>
    public static bool TryObject(JsonElement element, string name, out JsonElement value)
    {
        value = default;
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out value)
            && value.ValueKind == JsonValueKind.Object;
    }
}
