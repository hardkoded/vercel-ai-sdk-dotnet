// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>A Zod number check. Maps to one entry of <c>ZodNumberDef.checks</c>.</summary>
public sealed class NumberCheck
{
    /// <summary>Creates a check. <paramref name="kind"/> is <c>int</c>, <c>min</c>, <c>max</c>, or <c>multipleOf</c>.</summary>
    public NumberCheck(string kind, double value = 0, bool inclusive = true)
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
        Value = value;
        Inclusive = inclusive;
    }

    /// <summary>Check kind.</summary>
    public string Kind { get; }

    /// <summary>Bound or multiple.</summary>
    public double Value { get; }

    /// <summary>True for an inclusive min or max.</summary>
    public bool Inclusive { get; }
}

/// <summary>JSON Schema helpers that do not require a Zod runtime.</summary>
public static class JsonSchemas
{
    /// <summary>
    /// Sets <c>additionalProperties</c> to false on object schemas, in place.
    /// Schema-valued <c>additionalProperties</c> are visited instead. Maps to <c>addAdditionalPropertiesToJsonSchema</c>.
    /// </summary>
    public static JsonNode AddAdditionalPropertiesToJsonSchema(JsonNode schema)
    {
        if (schema is null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (schema is not JsonObject jsonSchema)
        {
            return schema;
        }

        if (IsObjectType(jsonSchema["type"]))
        {
            var additional = jsonSchema["additionalProperties"];
            jsonSchema["additionalProperties"] = additional != null && additional is not JsonValue
                ? Visit(additional)
                : false;

            if (jsonSchema["properties"] is JsonObject properties)
            {
                var keys = new List<string>();
                foreach (var pair in properties)
                {
                    keys.Add(pair.Key);
                }

                foreach (var key in keys)
                {
                    var child = properties[key];
                    if (child != null)
                    {
                        Assign(properties, key, Visit(child));
                    }
                }
            }
        }

        ReplaceChild(jsonSchema, "items");
        ReplaceArray(jsonSchema, "anyOf");
        ReplaceArray(jsonSchema, "allOf");
        ReplaceArray(jsonSchema, "oneOf");

        if (jsonSchema["definitions"] is JsonObject definitions)
        {
            var keys = new List<string>();
            foreach (var pair in definitions)
            {
                keys.Add(pair.Key);
            }

            foreach (var key in keys)
            {
                var child = definitions[key];
                if (child != null)
                {
                    Assign(definitions, key, Visit(child));
                }
            }
        }

        return jsonSchema;
    }

    /// <summary>Converts number checks to a JSON Schema object. Maps to <c>parseNumberDef</c>.</summary>
    public static JsonObject ParseNumberDef(IReadOnlyList<NumberCheck>? checks)
    {
        var result = new JsonObject
        {
            ["type"] = "number",
        };

        if (checks is null)
        {
            return result;
        }

        foreach (var check in checks)
        {
            if (check.Kind == "int")
            {
                result["type"] = "integer";
            }
            else if (check.Kind == "min")
            {
                if (check.Inclusive)
                {
                    result["minimum"] = check.Value;
                }
                else
                {
                    result["exclusiveMinimum"] = check.Value;
                }
            }
            else if (check.Kind == "max")
            {
                if (check.Inclusive)
                {
                    result["maximum"] = check.Value;
                }
                else
                {
                    result["exclusiveMaximum"] = check.Value;
                }
            }
            else if (check.Kind == "multipleOf")
            {
                result["multipleOf"] = check.Value;
            }
        }

        return result;
    }

    private static JsonNode Visit(JsonNode definition)
    {
        if (definition is JsonValue)
        {
            return definition;
        }

        return AddAdditionalPropertiesToJsonSchema(definition);
    }

    private static bool IsObjectType(JsonNode? type)
    {
        if (type is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text == "object";
        }

        if (type is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item is JsonValue itemValue && itemValue.TryGetValue<string>(out var name) && name == "object")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ReplaceChild(JsonObject schema, string name)
    {
        var child = schema[name];
        if (child is null)
        {
            return;
        }

        if (child is JsonArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                var item = array[i];
                if (item != null)
                {
                    Assign(array, i, Visit(item));
                }
            }

            return;
        }

        Assign(schema, name, Visit(child));
    }

    private static void ReplaceArray(JsonObject schema, string name)
    {
        if (schema[name] is not JsonArray array)
        {
            return;
        }

        for (var i = 0; i < array.Count; i++)
        {
            var item = array[i];
            if (item != null)
            {
                Assign(array, i, Visit(item));
            }
        }
    }

    private static void Assign(JsonObject parent, string name, JsonNode node)
    {
        if (ReferenceEquals(parent[name], node))
        {
            return;
        }

        parent[name] = node;
    }

    private static void Assign(JsonArray parent, int index, JsonNode node)
    {
        if (ReferenceEquals(parent[index], node))
        {
            return;
        }

        parent[index] = node;
    }
}
