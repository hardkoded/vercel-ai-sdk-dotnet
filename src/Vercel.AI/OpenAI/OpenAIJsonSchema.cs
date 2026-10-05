// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

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
/// and regex lookaround in <c>pattern</c>.
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
        var removedPropertyNames = false;
        var removedLookaround = false;
        Walk(clone, ref removedPropertyNames, ref removedLookaround);
        var warnings = new List<OpenAICallWarning>();
        if (removedPropertyNames)
        {
            warnings.Add(new OpenAICallWarning("compatibility", "JSON Schema propertyNames", PropertyNamesDetails));
        }

        if (removedLookaround)
        {
            warnings.Add(new OpenAICallWarning("compatibility", "JSON Schema pattern with regex lookaround", LookaroundDetails));
        }

        return new OpenAIJsonSchemaResult(clone, warnings);
    }

    private static void Walk(JsonNode? node, ref bool removedPropertyNames, ref bool removedLookaround)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("propertyNames", out var propertyNames) && propertyNames != null)
            {
                if (propertyNames is not JsonObject nameSchema || !IsStringSchema(nameSchema))
                {
                    throw new Provider.AiSdkException("JSON Schema propertyNames that does not use a string schema");
                }

                obj.Remove("propertyNames");
                removedPropertyNames = true;
            }

            if (obj["pattern"] is JsonValue patternValue
                && patternValue.TryGetValue<string>(out var pattern)
                && ContainsRegexLookaround(pattern))
            {
                obj.Remove("pattern");
                removedLookaround = true;
            }

            foreach (var property in obj.ToList())
            {
                Walk(property.Value, ref removedPropertyNames, ref removedLookaround);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                Walk(item, ref removedPropertyNames, ref removedLookaround);
            }
        }
    }

    private static bool IsStringSchema(JsonObject schema)
    {
        return schema["type"] is JsonValue type && type.TryGetValue<string>(out var text) && text == "string";
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

            if (!inCharacterClass && character == '(' && index + 1 < pattern.Length && pattern[index + 1] == '?')
            {
                var lookaroundPrefix = index + 2 < pattern.Length ? pattern[index + 2] : '\0';
                if (lookaroundPrefix == '=' || lookaroundPrefix == '!')
                {
                    return true;
                }

                if (lookaroundPrefix == '<'
                    && index + 3 < pattern.Length
                    && (pattern[index + 3] == '=' || pattern[index + 3] == '!'))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
