// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Normalizes a JSON Schema to the subset Moonshot's MFJS validator accepts.
/// The root must be an object schema, tuple <c>items</c> become <c>prefixItems</c>,
/// and a sibling <c>type</c> moves into <c>anyOf</c> branches.
/// </summary>
public static class MoonshotJsonSchema
{
    private static readonly string[] ArrayKeys = { "allOf", "anyOf", "oneOf", "prefixItems" };

    private static readonly string[] MapKeys = { "properties", "patternProperties", "$defs", "dependentSchemas" };

    private static readonly string[] SingleKeys = { "additionalProperties", "propertyNames", "items", "contains", "not", "if", "then", "else" };

    /// <summary>The functionality name used when the root schema is not an object.</summary>
    public const string RootFunctionality = "tool parameters must be a JSON Schema object with type \"object\" for moonshotai (MFJS)";

    /// <summary>Returns a normalized copy of <paramref name="schema"/>.</summary>
    public static JsonNode Normalize(JsonNode schema)
    {
        if (schema == null)
        {
            throw new UnsupportedFunctionalityException(RootFunctionality);
        }

        return Normalize(schema, true);
    }

    private static JsonNode Normalize(JsonNode definition, bool isRoot)
    {
        if (definition is not JsonObject source)
        {
            if (isRoot)
            {
                throw new UnsupportedFunctionalityException(RootFunctionality);
            }

            return definition.DeepClone();
        }

        if (isRoot && !string.Equals(ReadString(source["type"]), "object", StringComparison.Ordinal))
        {
            throw new UnsupportedFunctionalityException(RootFunctionality);
        }

        var result = (JsonObject)source.DeepClone();
        if (result["items"] is JsonArray tuple)
        {
            var combined = new JsonArray();
            if (result["prefixItems"] is JsonArray existing)
            {
                foreach (var item in existing)
                {
                    combined.Add(item == null ? null : Normalize(item, false));
                }
            }

            foreach (var item in tuple)
            {
                combined.Add(item == null ? null : Normalize(item, false));
            }

            result.Remove("items");
            result["prefixItems"] = combined;
        }
        else if (result["items"] is JsonObject)
        {
            result["items"] = Normalize(result["items"]!, false);
        }

        if (ReadString(result["type"]) != null && result["anyOf"] is JsonArray anyOf)
        {
            var parentType = ReadString(result["type"])!;
            result.Remove("type");
            var rewritten = new JsonArray();
            foreach (var branch in anyOf)
            {
                if (branch is JsonObject branchObject && branchObject["type"] == null)
                {
                    var copy = new JsonObject { ["type"] = parentType };
                    foreach (var property in branchObject)
                    {
                        copy[property.Key] = property.Value == null ? null : property.Value.DeepClone();
                    }

                    rewritten.Add(copy);
                }
                else
                {
                    rewritten.Add(branch == null ? null : branch.DeepClone());
                }
            }

            result["anyOf"] = rewritten;
        }

        foreach (var key in ArrayKeys)
        {
            if (result[key] is JsonArray array)
            {
                var normalized = new JsonArray();
                foreach (var item in array)
                {
                    normalized.Add(item == null ? null : Normalize(item, false));
                }

                result[key] = normalized;
            }
        }

        foreach (var key in MapKeys)
        {
            if (result[key] is JsonObject map)
            {
                var normalized = new JsonObject();
                foreach (var property in map)
                {
                    normalized[property.Key] = property.Value == null ? null : Normalize(property.Value, false);
                }

                result[key] = normalized;
            }
        }

        foreach (var key in SingleKeys)
        {
            var value = result[key];
            if (value is JsonObject || IsBoolean(value))
            {
                result[key] = Normalize(value!, false);
            }
        }

        return result;
    }

    private static string? ReadString(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static bool IsBoolean(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<bool>(out _);
    }
}
