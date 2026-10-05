// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.GenerateText;

/// <summary>Array output schema wrapping. Maps to the array branch of <c>getOutputStrategy</c>.</summary>
public static class OutputStrategies
{
    /// <summary>
    /// Wraps an element schema in <c>{ elements: array }</c> and lifts root <c>definitions</c> or <c>$defs</c>
    /// so root-relative references stay valid.
    /// </summary>
    public static JsonElement WrapArrayOutputSchema(JsonElement elementSchema)
    {
        JsonNode? definitions = null;
        JsonNode? defs = null;
        var items = new JsonObject();
        foreach (var property in elementSchema.EnumerateObject())
        {
            if (property.NameEquals("$schema"))
            {
                continue;
            }

            var clone = JsonNode.Parse(property.Value.GetRawText());
            if (property.NameEquals("definitions"))
            {
                definitions = clone;
                continue;
            }

            if (property.NameEquals("$defs"))
            {
                defs = clone;
                continue;
            }

            items[property.Name] = clone;
        }

        var root = new JsonObject
        {
            ["$schema"] = "http://json-schema.org/draft-07/schema#",
        };
        if (definitions != null)
        {
            root["definitions"] = definitions;
        }

        if (defs != null)
        {
            root["$defs"] = defs;
        }

        root["type"] = "object";
        root["properties"] = new JsonObject
        {
            ["elements"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = items,
            },
        };
        root["required"] = new JsonArray(JsonValue.Create("elements"));
        root["additionalProperties"] = false;
        return JsonSerializer.SerializeToElement(root);
    }
}
