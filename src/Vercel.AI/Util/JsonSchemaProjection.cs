// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Util;

/// <summary>
/// Parses a value with a JSON Schema the way a Zod object does: a property the schema does not declare is dropped.
/// A schema that allows additional properties keeps them.
/// </summary>
internal static class JsonSchemaProjection
{
    internal static JsonNode? Project(JsonElement schema, JsonNode? value)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return value;
        }

        if (value is JsonObject obj && schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            var keepsExtra = schema.TryGetProperty("additionalProperties", out var additional) && additional.ValueKind != JsonValueKind.False;
            var result = new JsonObject();
            foreach (var entry in obj)
            {
                if (properties.TryGetProperty(entry.Key, out var propertySchema))
                {
                    result[entry.Key] = Project(propertySchema, entry.Value?.DeepClone());
                }
                else if (keepsExtra)
                {
                    result[entry.Key] = entry.Value?.DeepClone();
                }
            }

            return result;
        }

        if (value is JsonArray array && schema.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Object)
        {
            var result = new JsonArray();
            foreach (var item in array)
            {
                result.Add(Project(items, item?.DeepClone()));
            }

            return result;
        }

        return value;
    }
}
