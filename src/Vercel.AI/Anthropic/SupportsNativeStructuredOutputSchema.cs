// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Anthropic;

/// <summary>Whether Anthropic's native structured outputs can represent a schema.</summary>
internal static class SupportsNativeStructuredOutputSchemaChecker
{
    // Anthropic rejects schemas with more than 24 optional properties in total,
    // and grammar compilation fails earlier as optional properties add up: from 13
    // on Bedrock and the Anthropic API, from 11 on Vertex (compilation timeout).
    private const int MaxOptionalProperties = 10;

    private static readonly string[] SubschemaKeywords =
    [
        "additionalProperties",
        "contains",
        "else",
        "if",
        "items",
        "not",
        "then",
    ];

    private static readonly string[] SubschemaListKeywords = ["allOf", "anyOf", "oneOf", "prefixItems"];

    private static readonly string[] SubschemaMapKeywords = ["dependentSchemas", "patternProperties", "properties"];

    /// <summary>
    /// Whether Anthropic's native structured outputs (<c>output_config.format</c>) can
    /// represent a schema without rejecting it or losing data.
    /// Rejected: self-referencing schemas, and schemas with too many optional
    /// properties. Lossy: open objects (maps, free-form objects), which native
    /// structured outputs close with <c>additionalProperties: false</c>, so the model
    /// can only return <c>{}</c> for them.
    /// </summary>
    public static bool SupportsNativeStructuredOutputSchema(JsonElement schema)
    {
        if (HasCircularSchemaReferenceChecker.HasCircularSchemaReference(schema))
        {
            return false;
        }

        var stats = GetSchemaStats(schema, schema, new Dictionary<string, SchemaStats>(StringComparer.Ordinal));
        return !stats.HasOpenObject && stats.OptionalProperties <= MaxOptionalProperties;
    }

    private static SchemaStats GetSchemaStats(JsonElement node, JsonElement root, Dictionary<string, SchemaStats> refStats)
    {
        var stats = new SchemaStats();
        if (node.ValueKind != JsonValueKind.Object)
        {
            return stats;
        }

        void Add(JsonElement child)
        {
            var childStats = GetSchemaStats(child, root, refStats);
            stats.OptionalProperties += childStats.OptionalProperties;
            stats.HasOpenObject |= childStats.HasOpenObject;
        }

        // references are counted at every use; the schema has no cycles here:
        if (node.TryGetProperty("$ref", out var reference)
            && reference.ValueKind == JsonValueKind.String
            && reference.GetString()!.StartsWith("#", StringComparison.Ordinal))
        {
            var pointer = reference.GetString()!;
            if (!refStats.TryGetValue(pointer, out var resolved))
            {
                var target = HasCircularSchemaReferenceChecker.ResolveSchemaPointer(root, pointer);
                resolved = target == null ? new SchemaStats() : GetSchemaStats(target.Value, root, refStats);
                refStats[pointer] = resolved;
            }

            stats.OptionalProperties += resolved.OptionalProperties;
            stats.HasOpenObject |= resolved.HasOpenObject;
        }

        if (IsOpenObject(node))
        {
            stats.HasOpenObject = true;
        }

        if (node.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            var required = new HashSet<string>(StringComparer.Ordinal);
            if (node.TryGetProperty("required", out var requiredValue) && requiredValue.ValueKind == JsonValueKind.Array)
            {
                foreach (var name in requiredValue.EnumerateArray())
                {
                    if (name.ValueKind == JsonValueKind.String)
                    {
                        required.Add(name.GetString()!);
                    }
                }
            }

            stats.OptionalProperties += properties.EnumerateObject().Count(property => !required.Contains(property.Name));
        }

        foreach (var keyword in SubschemaKeywords)
        {
            if (!node.TryGetProperty(keyword, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    Add(item);
                }
            }
            else
            {
                Add(value);
            }
        }

        foreach (var keyword in SubschemaListKeywords)
        {
            if (node.TryGetProperty(keyword, out var value) && value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    Add(item);
                }
            }
        }

        foreach (var keyword in SubschemaMapKeywords)
        {
            if (node.TryGetProperty(keyword, out var value) && value.ValueKind == JsonValueKind.Object)
            {
                foreach (var entry in value.EnumerateObject())
                {
                    Add(entry.Value);
                }
            }
        }

        return stats;
    }

    private static bool IsOpenObject(JsonElement node)
    {
        var hasAdditionalProperties = node.TryGetProperty("additionalProperties", out var additionalProperties);
        if (hasAdditionalProperties
            && (additionalProperties.ValueKind == JsonValueKind.True || additionalProperties.ValueKind == JsonValueKind.Object))
        {
            return true;
        }

        if (node.TryGetProperty("patternProperties", out var patternProperties)
            && patternProperties.ValueKind == JsonValueKind.Object
            && patternProperties.EnumerateObject().Any())
        {
            return true;
        }

        var hasProperties = node.TryGetProperty("properties", out var properties) && properties.ValueKind != JsonValueKind.Null;
        var isObjectType = node.TryGetProperty("type", out var type)
            && (type.ValueKind == JsonValueKind.String && type.GetString() == "object"
                || type.ValueKind == JsonValueKind.Array && type.EnumerateArray().Any(item => item.ValueKind == JsonValueKind.String && item.GetString() == "object"));
        var closed = hasAdditionalProperties && additionalProperties.ValueKind == JsonValueKind.False;
        return isObjectType && !hasProperties && !closed;
    }

    private sealed class SchemaStats
    {
        public int OptionalProperties { get; set; }

        public bool HasOpenObject { get; set; }
    }
}
