// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>
/// Validates a tool call input against the JSON Schema of a tool. Covers the keywords tool schemas use:
/// <c>type</c>, <c>enum</c>, <c>const</c>, <c>properties</c>, <c>required</c>, <c>additionalProperties</c>, <c>items</c>,
/// <c>minItems</c>, <c>maxItems</c>, <c>minLength</c>, <c>maxLength</c>, <c>minimum</c>, <c>maximum</c>,
/// <c>anyOf</c>, <c>oneOf</c> and <c>allOf</c>. Other keywords are ignored.
/// </summary>
public static class ToolInputSchemaValidator
{
    /// <summary>Returns the validation errors of <paramref name="value"/>, or an empty list when it is valid.</summary>
    public static IReadOnlyList<string> Validate(JsonElement schema, JsonElement value)
    {
        var errors = new List<string>();
        Check(schema, value, string.Empty, errors);
        return errors;
    }

    private static void Check(JsonElement schema, JsonElement value, string path, List<string> errors)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var at = path.Length == 0 ? "input" : path;

        if (schema.TryGetProperty("type", out var type) && !MatchesType(type, value))
        {
            errors.Add(at + ": expected " + Describe(type) + ", received " + Kind(value));
            return;
        }

        if (schema.TryGetProperty("const", out var constant) && !JsonEquals(constant, value))
        {
            errors.Add(at + ": expected the constant " + constant.GetRawText());
        }

        if (schema.TryGetProperty("enum", out var allowed) && allowed.ValueKind == JsonValueKind.Array)
        {
            var found = false;
            foreach (var option in allowed.EnumerateArray())
            {
                if (JsonEquals(option, value))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                errors.Add(at + ": expected one of " + allowed.GetRawText());
            }
        }

        CheckCombinators(schema, value, path, at, errors);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                CheckObject(schema, value, path, errors);
                break;
            case JsonValueKind.Array:
                CheckArray(schema, value, path, at, errors);
                break;
            case JsonValueKind.String:
                var text = value.GetString()!;
                if (TryGetNumber(schema, "minLength", out var minLength) && text.Length < minLength)
                {
                    errors.Add(at + ": expected at least " + minLength.ToString(CultureInfo.InvariantCulture) + " characters");
                }

                if (TryGetNumber(schema, "maxLength", out var maxLength) && text.Length > maxLength)
                {
                    errors.Add(at + ": expected at most " + maxLength.ToString(CultureInfo.InvariantCulture) + " characters");
                }

                break;
            case JsonValueKind.Number:
                var number = value.GetDouble();
                if (TryGetNumber(schema, "minimum", out var minimum) && number < minimum)
                {
                    errors.Add(at + ": expected a number >= " + minimum.ToString(CultureInfo.InvariantCulture));
                }

                if (TryGetNumber(schema, "maximum", out var maximum) && number > maximum)
                {
                    errors.Add(at + ": expected a number <= " + maximum.ToString(CultureInfo.InvariantCulture));
                }

                break;
        }
    }

    private static void CheckCombinators(JsonElement schema, JsonElement value, string path, string at, List<string> errors)
    {
        if (schema.TryGetProperty("allOf", out var allOf) && allOf.ValueKind == JsonValueKind.Array)
        {
            foreach (var branch in allOf.EnumerateArray())
            {
                Check(branch, value, path, errors);
            }
        }

        foreach (var keyword in new[] { "anyOf", "oneOf" })
        {
            if (!schema.TryGetProperty(keyword, out var branches) || branches.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var matches = 0;
            foreach (var branch in branches.EnumerateArray())
            {
                var branchErrors = new List<string>();
                Check(branch, value, path, branchErrors);
                if (branchErrors.Count == 0)
                {
                    matches++;
                }
            }

            if (matches == 0 || (keyword == "oneOf" && matches > 1))
            {
                errors.Add(at + ": expected " + (keyword == "oneOf" ? "exactly one" : "at least one") + " of the " + keyword + " schemas to match");
            }
        }
    }

    private static void CheckObject(JsonElement schema, JsonElement value, string path, List<string> errors)
    {
        var hasProperties = schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object;

        if (schema.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.Array)
        {
            foreach (var name in required.EnumerateArray())
            {
                if (name.ValueKind == JsonValueKind.String && !value.TryGetProperty(name.GetString()!, out _))
                {
                    errors.Add(Child(path, name.GetString()!) + ": required property is missing");
                }
            }
        }

        foreach (var property in value.EnumerateObject())
        {
            if (hasProperties && properties.TryGetProperty(property.Name, out var propertySchema))
            {
                Check(propertySchema, property.Value, Child(path, property.Name), errors);
            }
            else if (schema.TryGetProperty("additionalProperties", out var additional))
            {
                if (additional.ValueKind == JsonValueKind.False)
                {
                    errors.Add(Child(path, property.Name) + ": unrecognized property");
                }
                else if (additional.ValueKind == JsonValueKind.Object)
                {
                    Check(additional, property.Value, Child(path, property.Name), errors);
                }
            }
        }
    }

    private static void CheckArray(JsonElement schema, JsonElement value, string path, string at, List<string> errors)
    {
        var length = value.GetArrayLength();
        if (TryGetNumber(schema, "minItems", out var minItems) && length < minItems)
        {
            errors.Add(at + ": expected at least " + minItems.ToString(CultureInfo.InvariantCulture) + " items");
        }

        if (TryGetNumber(schema, "maxItems", out var maxItems) && length > maxItems)
        {
            errors.Add(at + ": expected at most " + maxItems.ToString(CultureInfo.InvariantCulture) + " items");
        }

        if (schema.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Object)
        {
            var index = 0;
            foreach (var item in value.EnumerateArray())
            {
                Check(items, item, path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", errors);
                index++;
            }
        }
    }

    private static string Child(string path, string name) => path.Length == 0 ? name : path + "." + name;

    private static bool TryGetNumber(JsonElement schema, string keyword, out double number)
    {
        number = 0;
        return schema.TryGetProperty(keyword, out var element) && element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out number);
    }

    private static bool MatchesType(JsonElement type, JsonElement value)
    {
        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var option in type.EnumerateArray())
            {
                if (MatchesType(option, value))
                {
                    return true;
                }
            }

            return false;
        }

        if (type.ValueKind != JsonValueKind.String)
        {
            return true;
        }

        switch (type.GetString())
        {
            case "object": return value.ValueKind == JsonValueKind.Object;
            case "array": return value.ValueKind == JsonValueKind.Array;
            case "string": return value.ValueKind == JsonValueKind.String;
            case "boolean": return value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False;
            case "null": return value.ValueKind == JsonValueKind.Null;
            case "number": return value.ValueKind == JsonValueKind.Number;
            case "integer":
                return value.ValueKind == JsonValueKind.Number
                    && (value.TryGetInt64(out _) || (value.TryGetDouble(out var d) && Math.Floor(d) == d && !double.IsInfinity(d)));
            default: return true;
        }
    }

    private static string Describe(JsonElement type) =>
        type.ValueKind == JsonValueKind.String ? type.GetString()! : type.GetRawText();

    private static string Kind(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False: return "boolean";
            case JsonValueKind.Undefined: return "undefined";
            default: return value.ValueKind.ToString().ToLowerInvariant();
        }
    }

    private static bool JsonEquals(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            return false;
        }

        switch (left.ValueKind)
        {
            case JsonValueKind.String: return left.GetString() == right.GetString();
            case JsonValueKind.Number: return left.GetDouble() == right.GetDouble();
            case JsonValueKind.Array:
            case JsonValueKind.Object: return left.GetRawText() == right.GetRawText();
            default: return true;
        }
    }
}
