// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>JSON Schema after OpenAI compatibility rewrites, plus the warnings those rewrites produced.</summary>
public sealed class NormalizedOpenAIJsonSchema
{
    /// <summary>Creates a normalized schema.</summary>
    public NormalizedOpenAIJsonSchema(JsonNode schema, IReadOnlyList<CallWarning> warnings)
    {
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        Warnings = warnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>Schema safe to send to OpenAI.</summary>
    public JsonNode Schema { get; }

    /// <summary>Compatibility warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Removes JSON Schema keywords OpenAI structured outputs reject.</summary>
public static class NormalizeOpenAIJsonSchema
{
    /// <summary>
    /// Drops string <c>propertyNames</c> and regex lookaround <c>pattern</c> values.
    /// Non-string <c>propertyNames</c> schemas throw.
    /// </summary>
    public static NormalizedOpenAIJsonSchema Normalize(JsonNode schema)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        var clone = JsonNode.Parse(schema.ToJsonString()) ?? JsonNode.Parse("{}")!;
        var removedPropertyNames = false;
        var removedLookaround = false;
        Walk(clone, ref removedPropertyNames, ref removedLookaround);
        var warnings = new List<CallWarning>();
        if (removedPropertyNames)
        {
            warnings.Add(new OpenAICallWarning(
                "compatibility",
                "JSON Schema propertyNames",
                "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints.",
                null));
        }

        if (removedLookaround)
        {
            warnings.Add(new OpenAICallWarning(
                "compatibility",
                "JSON Schema pattern with regex lookaround",
                "OpenAI does not support regex lookaround in JSON Schema patterns. The pattern was removed before sending the schema, so OpenAI will not enforce that constraint.",
                null));
        }

        return new NormalizedOpenAIJsonSchema(clone, warnings);
    }

    private static void Walk(JsonNode? node, ref bool removedPropertyNames, ref bool removedLookaround)
    {
        if (node is JsonArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                Walk(array[i], ref removedPropertyNames, ref removedLookaround);
            }

            return;
        }

        if (node is not JsonObject obj)
        {
            return;
        }

        if (obj["propertyNames"] is JsonNode propertyNames)
        {
            if (!IsStringSchema(propertyNames))
            {
                throw new AiSdkException("JSON Schema propertyNames that does not use a string schema");
            }

            obj.Remove("propertyNames");
            removedPropertyNames = true;
        }

        if (obj["pattern"] is JsonValue patternValue
            && patternValue.TryGetValue<string>(out var pattern)
            && pattern != null
            && ContainsRegexLookaround(pattern))
        {
            obj.Remove("pattern");
            removedLookaround = true;
        }

        foreach (var pair in obj.ToList())
        {
            Walk(pair.Value, ref removedPropertyNames, ref removedLookaround);
        }
    }

    private static bool IsStringSchema(JsonNode node)
    {
        if (node is not JsonObject obj)
        {
            return false;
        }

        return obj["type"] is JsonValue type && type.TryGetValue<string>(out var text) && text == "string";
    }

    private static bool ContainsRegexLookaround(string pattern)
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
                var lookaround = index + 2 < pattern.Length ? pattern[index + 2] : '\0';
                if (lookaround == '=' || lookaround == '!')
                {
                    return true;
                }

                if (lookaround == '<' && index + 3 < pattern.Length && (pattern[index + 3] == '=' || pattern[index + 3] == '!'))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
