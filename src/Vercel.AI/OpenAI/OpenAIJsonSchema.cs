// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.OpenAI;

/// <summary>JSON Schema adjusted for OpenAI structured outputs.</summary>
public sealed class OpenAIJsonSchemaResult
{
    internal OpenAIJsonSchemaResult(JsonNode schema, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Schema = schema;
        Warnings = warnings;
    }

    /// <summary>Schema with unsupported keywords removed.</summary>
    public JsonNode Schema { get; }

    /// <summary>Warnings for keywords that were removed.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>
/// Removes JSON Schema keywords OpenAI does not enforce: string <c>propertyNames</c>
/// and regex lookaround in <c>pattern</c>. A singleton local reference <c>allOf</c> becomes
/// a direct <c>$ref</c>, and at the schema root the referenced definition is expanded.
/// </summary>
public static class OpenAIJsonSchema
{
    /// <summary>The warning OpenAI emits when <c>propertyNames</c> is removed.</summary>
    public const string PropertyNamesDetails =
        "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints.";

    /// <summary>The warning OpenAI emits when a lookaround pattern is removed.</summary>
    public const string LookaroundDetails =
        "OpenAI does not support regex lookaround in JSON Schema patterns. The pattern was removed before sending the schema, so OpenAI will not enforce that constraint.";

    /// <summary>Returns a copy of <paramref name="schema"/> safe to send to OpenAI.</summary>
    public static OpenAIJsonSchemaResult Normalize(JsonElement schema)
    {
        var clone = JsonNode.Parse(schema.GetRawText()) ?? JsonNode.Parse("{}")!;
        return NormalizeNode(clone);
    }

    /// <summary>Returns a copy of <paramref name="schema"/> safe to send to OpenAI.</summary>
    public static OpenAIJsonSchemaResult Normalize(JsonNode schema)
    {
        var clone = schema.DeepClone();
        return NormalizeNode(clone);
    }

    private static OpenAIJsonSchemaResult NormalizeNode(JsonNode clone)
    {
        var state = new NormalizeState();
        var normalized = NormalizeDefinition(clone, state, isRoot: true);
        var warnings = new List<OpenAICallWarning>();
        if (state.RemovedPropertyNames)
        {
            warnings.Add(
                new OpenAICallWarning(
                    "compatibility",
                    "JSON Schema propertyNames",
                    PropertyNamesDetails
                )
            );
        }

        if (state.RemovedLookaround)
        {
            warnings.Add(
                new OpenAICallWarning(
                    "compatibility",
                    "JSON Schema pattern with regex lookaround",
                    LookaroundDetails
                )
            );
        }

        return new OpenAIJsonSchemaResult(normalized, warnings);
    }

    private sealed class NormalizeState
    {
        public bool RemovedPropertyNames { get; set; }

        public bool RemovedLookaround { get; set; }
    }

    // Maps to normalizeDefinition: a boolean schema is returned as it is.
    private static JsonNode NormalizeDefinition(
        JsonNode? definition,
        NormalizeState state,
        bool isRoot = false
    )
    {
        return definition is JsonObject obj ? NormalizeSchema(obj, state, isRoot) : definition!;
    }

    // Maps to normalizeSchema. Only the keywords that hold sub-schemas are walked.
    private static JsonObject NormalizeSchema(JsonObject schema, NormalizeState state, bool isRoot)
    {
        if (
            schema.TryGetPropertyValue("propertyNames", out var propertyNames)
            && propertyNames != null
        )
        {
            // The pinned upstream requires an explicit string schema.
            if (propertyNames is not JsonObject nameSchema || !IsString(nameSchema["type"]))
            {
                throw new Provider.AiSdkException(
                    "JSON Schema propertyNames that does not use a string schema"
                );
            }

            state.RemovedPropertyNames = true;
        }

        var normalized = new JsonObject();
        foreach (var property in schema)
        {
            if (property.Key != "propertyNames")
            {
                normalized[property.Key] = property.Value?.DeepClone();
            }
        }

        if (
            normalized["pattern"] is JsonValue patternValue
            && patternValue.TryGetValue<string>(out var pattern)
            && ContainsRegexLookaround(pattern)
        )
        {
            normalized.Remove("pattern");
            state.RemovedLookaround = true;
        }

        foreach (var keyword in SchemaRecordKeywords)
        {
            if (normalized[keyword] is JsonObject record)
            {
                normalized[keyword] = NormalizeRecord(record, state);
            }
        }

        foreach (var keyword in SchemaKeywords)
        {
            if (normalized[keyword] is JsonObject child)
            {
                normalized[keyword] = NormalizeDefinition(child, state);
            }
        }

        if (normalized["items"] is JsonArray tuple)
        {
            normalized["items"] = NormalizeArray(tuple, state);
        }
        else if (normalized["items"] is JsonObject itemSchema)
        {
            normalized["items"] = NormalizeDefinition(itemSchema, state);
        }

        foreach (var keyword in SchemaArrayKeywords)
        {
            if (normalized[keyword] is JsonArray list)
            {
                normalized[keyword] = NormalizeArray(list, state);
            }
        }

        if (normalized["dependencies"] is JsonObject dependencies)
        {
            var normalizedDependencies = new JsonObject();
            foreach (var dependency in dependencies)
            {
                normalizedDependencies[dependency.Key] = dependency.Value
                    is JsonObject dependencySchema
                    ? NormalizeDefinition(dependencySchema, state)
                    : dependency.Value?.DeepClone();
            }

            normalized["dependencies"] = normalizedDependencies;
        }

        var reference = GetSingletonReference(normalized);
        if (reference == null)
        {
            return normalized;
        }

        if (!isRoot)
        {
            // A one-item allOf has the same validation behavior as its reference.
            normalized.Remove("allOf");
            normalized["$ref"] = reference;
            return normalized;
        }

        // OpenAI requires an object at the schema root, so a local root reference
        // must be expanded instead of being sent as a direct $ref.
        var referencedSchema = GetLocalReferenceSchema(reference, normalized);
        if (referencedSchema == null)
        {
            return normalized;
        }

        normalized.Remove("allOf");
        // The root overlays the referenced schema, as upstream does. A root key such as `required` replaces the
        // definition's own (https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/1960).
        var expanded = (JsonObject)referencedSchema.DeepClone();
        foreach (var property in normalized)
        {
            expanded[property.Key] = property.Value?.DeepClone();
        }

        return expanded;
    }

    private static readonly string[] SchemaRecordKeywords =
    [
        "properties",
        "patternProperties",
        "definitions",
        "$defs",
    ];

    private static readonly string[] SchemaKeywords =
    [
        "additionalProperties",
        "additionalItems",
        "contains",
        "not",
        "if",
        "then",
        "else",
    ];

    private static readonly string[] SchemaArrayKeywords = ["allOf", "anyOf", "oneOf"];

    private static JsonObject NormalizeRecord(JsonObject schemas, NormalizeState state)
    {
        var result = new JsonObject();
        foreach (var entry in schemas)
        {
            result[entry.Key] = NormalizeDefinition(entry.Value, state)?.DeepClone();
        }

        return result;
    }

    private static JsonArray NormalizeArray(JsonArray schemas, NormalizeState state)
    {
        var result = new JsonArray();
        foreach (var entry in schemas)
        {
            result.Add(NormalizeDefinition(entry, state)?.DeepClone());
        }

        return result;
    }

    // Do not rewrite intersections: only a single reference is equivalent to a direct $ref.
    private static string? GetSingletonReference(JsonObject schema)
    {
        return
            schema["allOf"] is JsonArray { Count: 1 } allOf
            && allOf[0] is JsonObject { Count: 1 } only
            && only["$ref"] is JsonValue value
            && value.TryGetValue<string>(out var reference)
            ? reference
            : null;
    }

    private static readonly Regex LocalReference = new Regex(
        "^#/(definitions|\\$defs)/(.+)\\z",
        RegexOptions.CultureInvariant | RegexOptions.Singleline,
        TimeSpan.FromMilliseconds(100)
    );

    private static JsonObject? GetLocalReferenceSchema(string reference, JsonObject normalized)
    {
        var match = LocalReference.Match(reference);
        if (!match.Success)
        {
            return null;
        }

        var name = match.Groups[2].Value.Replace("~1", "/").Replace("~0", "~");
        return
            normalized[match.Groups[1].Value] is JsonObject definitions
            && definitions[name] is JsonObject definition
            ? definition
            : null;
    }

    private static bool IsString(JsonNode? type)
    {
        return type is JsonValue value
            && value.TryGetValue<string>(out var text)
            && text == "string";
    }

    /// <summary>True when <paramref name="pattern"/> contains an unescaped regex lookaround.</summary>
    public static bool ContainsRegexLookaround(string pattern)
    {
        var escaped = false;
        var inCharacterClass = false;
        for (var index = 0; index < pattern.Length; index++)
        {
            var character = pattern[index];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\')
            {
                escaped = true;
                continue;
            }

            if (character == '[')
            {
                inCharacterClass = true;
                continue;
            }

            if (character == ']')
            {
                inCharacterClass = false;
                continue;
            }

            if (
                !inCharacterClass
                && character == '('
                && index + 1 < pattern.Length
                && pattern[index + 1] == '?'
            )
            {
                var lookaroundPrefix = index + 2 < pattern.Length ? pattern[index + 2] : '\0';
                if (lookaroundPrefix == '=' || lookaroundPrefix == '!')
                {
                    return true;
                }

                if (
                    lookaroundPrefix == '<'
                    && index + 3 < pattern.Length
                    && (pattern[index + 3] == '=' || pattern[index + 3] == '!')
                )
                {
                    return true;
                }
            }
        }

        return false;
    }
}
