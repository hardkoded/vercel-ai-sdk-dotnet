// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Port of <c>normalize-json-schema-for-mfjs.ts</c>. Normalizes a JSON Schema to the subset Moonshot's
/// MFJS validator accepts: <c>object</c> root required, tuple <c>items</c> become <c>prefixItems</c>, and
/// <c>type</c> next to <c>anyOf</c> moves into the branches below the root. Everything else passes through.
/// </summary>
internal static class MoonshotJsonSchema
{
    private const string RootError = "tool and response schemas must be a JSON Schema object with type \"object\" for moonshotai (MFJS)";

    private static readonly string[] SchemaArrayKeys = ["allOf", "anyOf", "oneOf", "prefixItems"];

    private static readonly string[] SchemaMapKeys = ["properties", "patternProperties", "$defs", "dependentSchemas"];

    private static readonly string[] SchemaSingleKeys = ["additionalProperties", "propertyNames", "items", "contains", "not", "if", "then", "else"];

    private static readonly Regex LocalRef = new(@"^#/(\$defs|definitions)/([^/]+)\z", RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>Port of <c>normalizeJsonSchemaForMFJS</c>.</summary>
    public static JsonNode? NormalizeJsonSchemaForMFJS(JsonNode? schema)
    {
        return NormalizeDefinition(schema, true);
    }

    private static UnsupportedFunctionalityException RootException()
    {
        return new UnsupportedFunctionalityException(RootError, "'" + RootError + "' functionality not supported.");
    }

    private static JsonNode? NormalizeDefinition(JsonNode? definition, bool isRoot)
    {
        if (definition is not JsonObject record)
        {
            if (isRoot)
            {
                throw RootException();
            }

            return definition?.DeepClone();
        }

        // A root union of objects (Zod z.union / discriminatedUnion of objects) is an object;
        // MFJS only needs the root to say so.
        var root = record;
        if (isRoot && !record.ContainsKey("type") && IsObjectUnion(record))
        {
            root = new JsonObject { ["type"] = "object" };
            foreach (var pair in record)
            {
                root[pair.Key] = pair.Value?.DeepClone();
            }
        }

        if (isRoot && !(root["type"] is JsonValue type && type.TryGetValue<string>(out var typeName) && typeName == "object"))
        {
            throw RootException();
        }

        var result = new JsonObject();
        foreach (var pair in root)
        {
            result[pair.Key] = pair.Value?.DeepClone();
        }

        // Existing prefixItems stay first (they are positional).
        if (result["items"] is JsonArray tuple)
        {
            var prefix = new JsonArray();
            if (result["prefixItems"] is JsonArray existing)
            {
                foreach (var item in existing)
                {
                    prefix.Add(item?.DeepClone());
                }
            }

            foreach (var item in tuple)
            {
                prefix.Add(NormalizeDefinition(item, false));
            }

            result["prefixItems"] = prefix;
            result.Remove("items");
        }
        else if (result["items"] is JsonObject items)
        {
            result["items"] = NormalizeDefinition(items, false);
        }

        // MFJS requires type to live inside anyOf items, except at the root, which
        // must keep type "object" (moving it leaves a typeless root that 400s).
        if (!isRoot
            && result["type"] is JsonValue parent
            && parent.TryGetValue<string>(out var parentType)
            && result["anyOf"] is JsonArray anyOf)
        {
            result.Remove("type");
            var branches = new JsonArray();
            foreach (var branch in anyOf)
            {
                if (branch is JsonObject branchObject && branchObject["type"] is null)
                {
                    var typed = new JsonObject { ["type"] = parentType };
                    foreach (var pair in branchObject)
                    {
                        typed[pair.Key] = pair.Value?.DeepClone();
                    }

                    branches.Add(typed);
                }
                else
                {
                    branches.Add(branch?.DeepClone());
                }
            }

            result["anyOf"] = branches;
        }

        foreach (var key in SchemaArrayKeys)
        {
            if (result[key] is JsonArray array)
            {
                var mapped = new JsonArray();
                foreach (var item in array)
                {
                    mapped.Add(NormalizeDefinition(item, false));
                }

                result[key] = mapped;
            }
        }

        foreach (var key in SchemaMapKeys)
        {
            if (result[key] is JsonObject map)
            {
                var mapped = new JsonObject();
                foreach (var pair in map)
                {
                    mapped[pair.Key] = NormalizeDefinition(pair.Value, false);
                }

                result[key] = mapped;
            }
        }

        foreach (var key in SchemaSingleKeys)
        {
            var value = result[key];
            if (value is JsonObject || (value is JsonValue single && single.TryGetValue<bool>(out _)))
            {
                result[key] = NormalizeDefinition(value, false);
            }
        }

        return result;
    }

    /// <summary>
    /// Every <c>anyOf</c>/<c>oneOf</c> on the root is a non-empty union of object schemas. Local <c>$ref</c>s
    /// (<c>#/$defs/X</c>, <c>#/definitions/X</c>) are resolved one hop. A nullable branch is not an object.
    /// </summary>
    private static bool IsObjectUnion(JsonObject definition)
    {
        var any = false;
        foreach (var key in new[] { "anyOf", "oneOf" })
        {
            if (!definition.ContainsKey(key))
            {
                continue;
            }

            var branches = definition[key];
            any = true;
            if (branches is not JsonArray array || array.Count == 0)
            {
                return false;
            }

            foreach (var branch in array)
            {
                if (!IsObjectSchema(branch, definition))
                {
                    return false;
                }
            }
        }

        return any;
    }

    private static bool IsObjectSchema(JsonNode? branch, JsonObject root)
    {
        var resolved = ResolveLocalRef(branch, root);
        if (resolved is not JsonObject schema)
        {
            return false;
        }

        var type = schema["type"];
        if (type is JsonValue value && value.TryGetValue<string>(out var name) && name == "object")
        {
            return true;
        }

        return type is null && schema["properties"] is JsonObject;
    }

    private static JsonNode? ResolveLocalRef(JsonNode? branch, JsonObject root)
    {
        if (branch is not JsonObject record
            || record["$ref"] is not JsonValue reference
            || !reference.TryGetValue<string>(out var path))
        {
            return branch;
        }

        var match = LocalRef.Match(path);
        if (!match.Success)
        {
            return branch;
        }

        return root[match.Groups[1].Value] is JsonObject defs && defs.TryGetPropertyValue(match.Groups[2].Value, out var target) ? target : branch;
    }
}
