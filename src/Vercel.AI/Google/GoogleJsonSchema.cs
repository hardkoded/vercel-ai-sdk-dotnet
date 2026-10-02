// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Google;

/// <summary>Rewrites JSON Schema so Gemini <c>responseJsonSchema</c> accepts it.</summary>
public static class GoogleJsonSchema
{
    /// <summary>
    /// Replaces <c>const</c> with a single-value <c>enum</c> and walks nested schemas.
    /// The input document is not modified.
    /// </summary>
    public static JsonNode Sanitize(JsonElement schema)
    {
        return SanitizeNode(JsonNode.Parse(schema.GetRawText())!);
    }

    private static JsonNode SanitizeNode(JsonNode node)
    {
        if (node is not JsonObject schema)
        {
            return node;
        }

        var result = new JsonObject();
        JsonNode? constValue = null;
        var hasConst = false;
        foreach (var pair in schema)
        {
            if (pair.Key == "const")
            {
                hasConst = true;
                constValue = pair.Value == null ? GoogleJson.Null() : JsonNode.Parse(pair.Value.ToJsonString());
                continue;
            }

            if (pair.Key == "properties" || pair.Key == "$defs")
            {
                result[pair.Key] = SanitizeMap(pair.Value as JsonObject);
                continue;
            }

            if (pair.Key == "items" || pair.Key == "additionalProperties" || pair.Key == "anyOf" || pair.Key == "oneOf")
            {
                result[pair.Key] = SanitizeDefinition(pair.Value);
                continue;
            }

            result[pair.Key] = pair.Value == null ? GoogleJson.Null() : JsonNode.Parse(pair.Value.ToJsonString());
        }

        if (hasConst)
        {
            result["enum"] = new JsonArray(constValue ?? GoogleJson.Null());
        }

        return result;
    }

    private static JsonObject SanitizeMap(JsonObject? map)
    {
        var result = new JsonObject();
        if (map == null)
        {
            return result;
        }

        foreach (var pair in map)
        {
            result[pair.Key] = SanitizeDefinition(pair.Value);
        }

        return result;
    }

    private static JsonNode? SanitizeDefinition(JsonNode? definition)
    {
        if (definition == null)
        {
            return GoogleJson.Null();
        }

        if (definition is JsonValue value && value.TryGetValue<bool>(out _))
        {
            return JsonNode.Parse(definition.ToJsonString());
        }

        if (definition is JsonArray array)
        {
            var copy = new JsonArray();
            foreach (var item in array)
            {
                copy.Add(SanitizeDefinition(item));
            }

            return copy;
        }

        if (definition is JsonObject)
        {
            return SanitizeNode(JsonNode.Parse(definition.ToJsonString())!);
        }

        return JsonNode.Parse(definition.ToJsonString());
    }
}
