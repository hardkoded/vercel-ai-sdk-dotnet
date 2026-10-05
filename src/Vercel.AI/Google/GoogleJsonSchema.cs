// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Google;

/// <summary>Rewrites JSON Schema so Gemini <c>responseJsonSchema</c> accepts it.</summary>
public static class GoogleJsonSchema
{
    /// <summary>
    /// Replaces each <c>const</c> with a one-value <c>enum</c> and leaves every other keyword in place.
    /// The input document is not modified.
    /// </summary>
    public static JsonNode Sanitize(JsonNode schema)
    {
        if (schema is not JsonObject source)
        {
            return schema?.DeepClone() ?? new JsonObject();
        }

        var result = new JsonObject();
        JsonNode? constValue = null;
        var hasConst = false;
        foreach (var pair in source)
        {
            if (pair.Key == "const")
            {
                hasConst = true;
                constValue = pair.Value?.DeepClone();
                continue;
            }

            result[pair.Key] = SanitizeChild(pair.Key, pair.Value);
        }

        if (hasConst)
        {
            result["enum"] = new JsonArray(constValue?.DeepClone());
        }

        return result;
    }

    /// <summary>Sanitizes a schema element.</summary>
    public static JsonNode Sanitize(JsonElement schema)
    {
        return Sanitize(JsonNode.Parse(schema.GetRawText())!);
    }

    private static JsonNode? SanitizeChild(string key, JsonNode? value)
    {
        if (value is null)
        {
            return null;
        }

        switch (key)
        {
            case "properties":
            case "$defs":
            case "definitions":
                return SanitizeMap(value);
            case "items":
                return value is JsonArray items ? SanitizeArray(items) : SanitizeNode(value);
            case "additionalProperties":
                return value is JsonValue ? value.DeepClone() : SanitizeNode(value);
            case "anyOf":
            case "oneOf":
            case "allOf":
                return value is JsonArray choices ? SanitizeArray(choices) : value.DeepClone();
            default:
                return value.DeepClone();
        }
    }

    private static JsonObject SanitizeMap(JsonNode value)
    {
        var map = new JsonObject();
        if (value is JsonObject source)
        {
            foreach (var pair in source)
            {
                map[pair.Key] = SanitizeNode(pair.Value);
            }
        }

        return map;
    }

    private static JsonArray SanitizeArray(JsonArray source)
    {
        var array = new JsonArray();
        foreach (var item in source)
        {
            array.Add(SanitizeNode(item));
        }

        return array;
    }

    private static JsonNode? SanitizeNode(JsonNode? value)
    {
        if (value is JsonObject)
        {
            return Sanitize(value);
        }

        return value?.DeepClone();
    }
}
