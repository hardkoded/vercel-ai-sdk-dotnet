// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>
/// Relaxes a JSON Schema so it can be sent as Anthropic <c>output_config.format.schema</c>.
/// The caller's original schema is not modified.
/// </summary>
public static class AnthropicJsonSchema
{
    private static readonly string[] SupportedFormats =
    {
        "date-time", "time", "date", "duration", "email", "hostname", "uri", "ipv4", "ipv6", "uuid",
    };

    private static readonly string[] ConstraintKeys =
    {
        "minimum", "maximum", "exclusiveMinimum", "exclusiveMaximum", "multipleOf",
        "minLength", "maxLength", "pattern", "minItems", "maxItems", "uniqueItems",
        "minProperties", "maxProperties", "not",
    };

    /// <summary>Returns a schema Anthropic's constrained decoder accepts.</summary>
    public static JsonNode Sanitize(JsonElement schema)
    {
        return SanitizeNode(schema);
    }

    /// <summary>Returns a schema Anthropic's constrained decoder accepts.</summary>
    public static JsonNode Sanitize(JsonNode schema)
    {
        return SanitizeNode(JsonDocument.Parse(schema.ToJsonString()).RootElement);
    }

    private static JsonNode SanitizeNode(JsonElement schema)
    {
        if (schema.ValueKind == JsonValueKind.True || schema.ValueKind == JsonValueKind.False)
        {
            return JsonValue.Create(schema.GetBoolean())!;
        }

        if (schema.ValueKind != JsonValueKind.Object)
        {
            return JsonNode.Parse(schema.GetRawText()) ?? new JsonObject();
        }

        if (schema.TryGetProperty("$ref", out var reference) && reference.ValueKind != JsonValueKind.Null)
        {
            return new JsonObject { ["$ref"] = JsonNode.Parse(reference.GetRawText()) };
        }

        var result = new JsonObject();
        Copy(schema, result, "$schema");
        Copy(schema, result, "$id");
        Copy(schema, result, "title");
        Copy(schema, result, "description");
        Copy(schema, result, "default");
        Copy(schema, result, "const");
        Copy(schema, result, "enum");
        Copy(schema, result, "type");

        if (schema.TryGetProperty("anyOf", out var anyOf) && anyOf.ValueKind == JsonValueKind.Array)
        {
            result["anyOf"] = MapArray(anyOf);
        }
        else if (schema.TryGetProperty("oneOf", out var oneOf) && oneOf.ValueKind == JsonValueKind.Array)
        {
            result["anyOf"] = MapArray(oneOf);
        }

        if (schema.TryGetProperty("allOf", out var allOf) && allOf.ValueKind == JsonValueKind.Array)
        {
            result["allOf"] = MapArray(allOf);
        }

        if (schema.TryGetProperty("definitions", out var definitions) && definitions.ValueKind == JsonValueKind.Object)
        {
            result["definitions"] = MapObject(definitions);
        }

        if (schema.TryGetProperty("$defs", out var defs) && defs.ValueKind == JsonValueKind.Object)
        {
            result["$defs"] = MapObject(defs);
        }

        var type = schema.TryGetProperty("type", out var typeValue) && typeValue.ValueKind == JsonValueKind.String ? typeValue.GetString() : null;
        var hasProperties = schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object;
        if (type == "object" || hasProperties)
        {
            if (hasProperties)
            {
                result["properties"] = MapObject(properties);
            }

            result["additionalProperties"] = false;
            Copy(schema, result, "required");
        }

        if (schema.TryGetProperty("items", out var items) && items.ValueKind != JsonValueKind.Null)
        {
            result["items"] = items.ValueKind == JsonValueKind.Array ? MapArray(items) : SanitizeNode(items);
        }

        if (schema.TryGetProperty("format", out var format) && format.ValueKind == JsonValueKind.String && IsSupportedFormat(format.GetString()))
        {
            result["format"] = format.GetString();
        }

        var constraint = DescribeConstraints(schema);
        if (constraint != null)
        {
            var existing = result["description"]?.GetValue<string>();
            result["description"] = existing == null ? constraint : existing + "\n" + constraint;
        }

        return result;
    }

    private static JsonArray MapArray(JsonElement array)
    {
        var result = new JsonArray();
        foreach (var item in array.EnumerateArray())
        {
            result.Add(SanitizeNode(item));
        }

        return result;
    }

    private static JsonObject MapObject(JsonElement element)
    {
        var result = new JsonObject();
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = SanitizeNode(property.Value);
        }

        return result;
    }

    private static void Copy(JsonElement source, JsonObject target, string name)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined)
        {
            target[name] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static bool IsSupportedFormat(string? format)
    {
        if (format == null)
        {
            return false;
        }

        foreach (var supported in SupportedFormats)
        {
            if (supported == format)
            {
                return true;
            }
        }

        return false;
    }

    private static string? DescribeConstraints(JsonElement schema)
    {
        var parts = new List<string>();
        foreach (var key in ConstraintKeys)
        {
            if (!schema.TryGetProperty(key, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.False)
            {
                continue;
            }

            parts.Add(FormatName(key) + ": " + FormatValue(value));
        }

        if (schema.TryGetProperty("format", out var format) && format.ValueKind == JsonValueKind.String && !IsSupportedFormat(format.GetString()))
        {
            parts.Add("format: " + format.GetString());
        }

        return parts.Count == 0 ? null : string.Join("; ", parts) + ".";
    }

    private static string FormatName(string key)
    {
        var builder = new StringBuilder();
        foreach (var character in key)
        {
            if (char.IsUpper(character))
            {
                builder.Append(' ');
                builder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static string FormatValue(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
    }
}
