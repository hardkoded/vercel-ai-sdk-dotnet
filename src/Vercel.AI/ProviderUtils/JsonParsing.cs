// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>JSON text could not be parsed.</summary>
public sealed class JsonParseException : Exception
{
    /// <summary>Creates the exception.</summary>
    public JsonParseException(string text, Exception innerException)
        : base("JSON parsing failed: Text: " + text + ". Error message: " + innerException.Message, innerException)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Original text.</summary>
    public string Text { get; }
}

/// <summary>A value did not match a JSON Schema.</summary>
public sealed class TypeValidationException : Exception
{
    /// <summary>Creates the exception.</summary>
    public TypeValidationException(string message, JsonElement? value)
        : base(message)
    {
        Value = value;
    }

    /// <summary>Value that failed validation.</summary>
    public JsonElement? Value { get; }
}

/// <summary>Success or failure of a JSON parse, including the raw value when parsing succeeded.</summary>
public sealed class JsonParseResult
{
    private JsonParseResult(bool success, JsonElement? value, JsonElement? rawValue, Exception? error)
    {
        Success = success;
        Value = value;
        RawValue = rawValue;
        Error = error;
    }

    /// <summary>True when parsing and schema checks succeeded.</summary>
    public bool Success { get; }

    /// <summary>Projected value.</summary>
    public JsonElement? Value { get; }

    /// <summary>Value before schema projection. Null when the text was not JSON.</summary>
    public JsonElement? RawValue { get; }

    /// <summary>Parse or validation error.</summary>
    public Exception? Error { get; }

    /// <summary>Creates a success result.</summary>
    public static JsonParseResult Ok(JsonElement value, JsonElement rawValue)
    {
        return new JsonParseResult(true, value, rawValue, null);
    }

    /// <summary>Creates a failure result.</summary>
    public static JsonParseResult Fail(Exception error, JsonElement? rawValue)
    {
        return new JsonParseResult(false, null, rawValue, error);
    }
}

/// <summary>Parses JSON and rejects <c>__proto__</c> and <c>constructor.prototype</c> payload keys.</summary>
public static class SecureJson
{
    private static readonly Regex SuspectProto = new Regex(
        "\"(?:_|\\\\u005[Ff])(?:_|\\\\u005[Ff])(?:p|\\\\u0070)(?:r|\\\\u0072)(?:o|\\\\u006[Ff])(?:t|\\\\u0074)(?:o|\\\\u006[Ff])(?:_|\\\\u005[Ff])(?:_|\\\\u005[Ff])\"\\s*:",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex SuspectConstructor = new Regex(
        "\"(?:c|\\\\u0063)(?:o|\\\\u006[Ff])(?:n|\\\\u006[Ee])(?:s|\\\\u0073)(?:t|\\\\u0074)(?:r|\\\\u0072)(?:u|\\\\u0075)(?:c|\\\\u0063)(?:t|\\\\u0074)(?:o|\\\\u006[Ff])(?:r|\\\\u0072)\"\\s*:",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    /// <summary>Parses <paramref name="text"/> and returns a detached element.</summary>
    public static JsonElement Parse(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        using (var document = JsonDocument.Parse(text))
        {
            if (SuspectProto.IsMatch(text) || SuspectConstructor.IsMatch(text))
            {
                RejectForbidden(document.RootElement);
            }

            return document.RootElement.Clone();
        }
    }

    private static void RejectForbidden(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in node.EnumerateObject())
            {
                if (property.NameEquals("__proto__"))
                {
                    throw new JsonException("Object contains forbidden prototype property");
                }

                if (property.NameEquals("constructor")
                    && property.Value.ValueKind == JsonValueKind.Object
                    && property.Value.TryGetProperty("prototype", out _))
                {
                    throw new JsonException("Object contains forbidden prototype property");
                }
            }

            foreach (var property in node.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                {
                    RejectForbidden(property.Value);
                }
            }
        }
        else if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in node.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                {
                    RejectForbidden(item);
                }
            }
        }
    }
}

/// <summary>Parses JSON text, optionally checking it against a JSON Schema document.</summary>
public static class JsonParsing
{
    /// <summary>True when <see cref="SecureJson.Parse"/> accepts <paramref name="text"/>.</summary>
    public static bool IsParsable(string text)
    {
        try
        {
            SecureJson.Parse(text);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Parses <paramref name="text"/>. Schema failures throw <see cref="TypeValidationException"/>.</summary>
    public static JsonElement Parse(string text, JsonNode? schema = null)
    {
        JsonElement value;
        try
        {
            value = SecureJson.Parse(text);
        }
        catch (Exception error) when (error is not JsonParseException)
        {
            throw new JsonParseException(text, error);
        }

        if (schema is null)
        {
            return value;
        }

        if (!JsonSchemaValidator.TryApply(value, schema, out var projected, out var errorMessage))
        {
            throw new TypeValidationException(errorMessage ?? "Schema validation failed.", value);
        }

        return projected;
    }

    /// <summary>Parses <paramref name="text"/> without throwing for invalid JSON or schema mismatches.</summary>
    public static JsonParseResult SafeParse(string text, JsonNode? schema = null)
    {
        JsonElement value;
        try
        {
            value = SecureJson.Parse(text);
        }
        catch (Exception error)
        {
            var wrapped = error as JsonParseException ?? new JsonParseException(text, error);
            return JsonParseResult.Fail(wrapped, null);
        }

        if (schema is null)
        {
            return JsonParseResult.Ok(value, value);
        }

        if (!JsonSchemaValidator.TryApply(value, schema, out var projected, out var errorMessage))
        {
            return JsonParseResult.Fail(new TypeValidationException(errorMessage ?? "Schema validation failed.", value), value);
        }

        return JsonParseResult.Ok(projected, value);
    }
}

/// <summary>Applies a JSON Schema document. Unknown object properties are dropped.</summary>
public static class JsonSchemaValidator
{
    /// <summary>Projects <paramref name="value"/> through <paramref name="schema"/>.</summary>
    public static bool TryApply(JsonElement value, JsonNode schema, out JsonElement projected, out string? error)
    {
        if (schema is null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (!TryProject(value, schema, out var node, out error))
        {
            projected = default;
            return false;
        }

        projected = JsonElementFrom(node);
        return true;
    }

    private static bool TryProject(JsonElement value, JsonNode schemaNode, out JsonNode? projected, out string? error)
    {
        projected = null;
        error = null;
        if (schemaNode is not JsonObject schema)
        {
            projected = JsonNode.Parse(value.GetRawText());
            return true;
        }

        if (schema["anyOf"] is JsonArray anyOf)
        {
            foreach (var branch in anyOf)
            {
                if (branch != null && TryProject(value, branch, out var branchValue, out _))
                {
                    projected = branchValue;
                    return true;
                }
            }

            error = "Value does not match any schema in anyOf.";
            return false;
        }

        if (schema["enum"] is JsonArray enums && !MatchesEnum(value, enums))
        {
            error = "Value is not in the enum.";
            return false;
        }

        if (schema["const"] is JsonNode constant && !JsonNode.DeepEquals(constant, JsonNode.Parse(value.GetRawText())))
        {
            error = "Value does not match const.";
            return false;
        }

        if (!MatchesType(value, schema, out error))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString() ?? string.Empty;
            if (TryReadInt(schema["minLength"], out var minLength) && text.Length < minLength)
            {
                error = "String is shorter than minLength.";
                return false;
            }

            if (TryReadInt(schema["maxLength"], out var maxLength) && text.Length > maxLength)
            {
                error = "String is longer than maxLength.";
                return false;
            }
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            var number = value.GetDouble();
            if (TryReadDouble(schema["minimum"], out var minimum) && number < minimum)
            {
                error = "Number is below minimum.";
                return false;
            }

            if (TryReadDouble(schema["maximum"], out var maximum) && number > maximum)
            {
                error = "Number is above maximum.";
                return false;
            }

            if (TryReadDouble(schema["exclusiveMinimum"], out var exclusiveMinimum) && number <= exclusiveMinimum)
            {
                error = "Number is below exclusiveMinimum.";
                return false;
            }

            if (TryReadDouble(schema["exclusiveMaximum"], out var exclusiveMaximum) && number >= exclusiveMaximum)
            {
                error = "Number is above exclusiveMaximum.";
                return false;
            }
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            if (TryReadInt(schema["minItems"], out var minItems) && value.GetArrayLength() < minItems)
            {
                error = "Array is shorter than minItems.";
                return false;
            }

            if (TryReadInt(schema["maxItems"], out var maxItems) && value.GetArrayLength() > maxItems)
            {
                error = "Array is longer than maxItems.";
                return false;
            }

            var items = new JsonArray();
            if (schema["items"] is JsonObject itemSchema)
            {
                foreach (var item in value.EnumerateArray())
                {
                    if (!TryProject(item, itemSchema, out var projectedItem, out error))
                    {
                        return false;
                    }

                    items.Add(projectedItem);
                }
            }
            else if (schema["items"] is JsonArray tuple)
            {
                var index = 0;
                foreach (var item in value.EnumerateArray())
                {
                    if (index < tuple.Count && tuple[index] != null)
                    {
                        if (!TryProject(item, tuple[index]!, out var projectedItem, out error))
                        {
                            return false;
                        }

                        items.Add(projectedItem);
                    }
                    else if (schema["additionalItems"] is JsonObject additional)
                    {
                        if (!TryProject(item, additional, out var projectedItem, out error))
                        {
                            return false;
                        }

                        items.Add(projectedItem);
                    }
                    else
                    {
                        items.Add(JsonNode.Parse(item.GetRawText()));
                    }

                    index++;
                }
            }
            else
            {
                foreach (var item in value.EnumerateArray())
                {
                    items.Add(JsonNode.Parse(item.GetRawText()));
                }
            }

            projected = items;
            return true;
        }

        if (value.ValueKind == JsonValueKind.Object && schema["properties"] is JsonObject properties)
        {
            var result = new JsonObject();
            var required = new HashSet<string>(StringComparer.Ordinal);
            if (schema["required"] is JsonArray requiredArray)
            {
                foreach (var name in requiredArray)
                {
                    var text = name?.GetValue<string>();
                    if (text != null)
                    {
                        required.Add(text);
                    }
                }
            }

            foreach (var property in properties)
            {
                var present = value.TryGetProperty(property.Key, out var child);
                if (!present)
                {
                    if (required.Contains(property.Key))
                    {
                        error = "Missing required property '" + property.Key + "'.";
                        return false;
                    }

                    continue;
                }

                if (property.Value is null)
                {
                    continue;
                }

                if (!TryProject(child, property.Value, out var projectedChild, out error))
                {
                    return false;
                }

                result[property.Key] = projectedChild;
            }

            if (schema["additionalProperties"] is JsonObject additionalSchema)
            {
                foreach (var child in value.EnumerateObject())
                {
                    if (properties.ContainsKey(child.Name))
                    {
                        continue;
                    }

                    if (!TryProject(child.Value, additionalSchema, out var projectedChild, out error))
                    {
                        return false;
                    }

                    result[child.Name] = projectedChild;
                }
            }
            else if (schema["additionalProperties"] is JsonValue flag && flag.TryGetValue<bool>(out var allowed) && allowed)
            {
                foreach (var child in value.EnumerateObject())
                {
                    if (!result.ContainsKey(child.Name))
                    {
                        result[child.Name] = JsonNode.Parse(child.Value.GetRawText());
                    }
                }
            }

            projected = result;
            return true;
        }

        projected = JsonNode.Parse(value.GetRawText());
        return true;
    }

    private static bool MatchesEnum(JsonElement value, JsonArray enums)
    {
        var node = JsonNode.Parse(value.GetRawText());
        foreach (var candidate in enums)
        {
            if (JsonNode.DeepEquals(node, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesType(JsonElement value, JsonObject schema, out string? error)
    {
        error = null;
        if (schema["type"] is null)
        {
            return true;
        }

        var actual = TypeName(value);
        if (schema["type"] is JsonValue single)
        {
            var expected = single.GetValue<string>();
            if (expected == "integer")
            {
                if (value.ValueKind != JsonValueKind.Number || value.TryGetInt64(out _) == false || value.GetRawText().IndexOf('.') >= 0)
                {
                    error = "Expected integer.";
                    return false;
                }

                return true;
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                error = "Expected type " + expected + ".";
                return false;
            }

            return true;
        }

        if (schema["type"] is JsonArray types)
        {
            foreach (var type in types)
            {
                var expected = type?.GetValue<string>();
                if (expected == "integer" && value.ValueKind == JsonValueKind.Number && value.GetRawText().IndexOf('.') < 0)
                {
                    return true;
                }

                if (string.Equals(actual, expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            error = "Value type is not allowed.";
            return false;
        }

        return true;
    }

    private static string TypeName(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return "string";
            case JsonValueKind.Number:
                return "number";
            case JsonValueKind.True:
            case JsonValueKind.False:
                return "boolean";
            case JsonValueKind.Object:
                return "object";
            case JsonValueKind.Array:
                return "array";
            case JsonValueKind.Null:
                return "null";
            default:
                return "unknown";
        }
    }

    private static bool TryReadInt(JsonNode? node, out int number)
    {
        if (TryReadDouble(node, out var value) && value == Math.Floor(value) && value <= int.MaxValue && value >= int.MinValue)
        {
            number = (int)value;
            return true;
        }

        number = 0;
        return false;
    }

    private static bool TryReadDouble(JsonNode? node, out double number)
    {
        number = 0;
        if (node is not JsonValue value)
        {
            return false;
        }

        if (value.TryGetValue<int>(out var integer))
        {
            number = integer;
            return true;
        }

        if (value.TryGetValue<long>(out var longValue))
        {
            number = longValue;
            return true;
        }

        return value.TryGetValue<double>(out number);
    }

    private static JsonElement JsonElementFrom(JsonNode? node)
    {
        using (var document = JsonDocument.Parse(node?.ToJsonString() ?? "null"))
        {
            return document.RootElement.Clone();
        }
    }
}
