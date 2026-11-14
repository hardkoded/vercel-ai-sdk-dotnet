// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Anthropic;

/// <summary>Detects cycles of local <c>$ref</c>s in a JSON schema.</summary>
internal static class HasCircularSchemaReferenceChecker
{
    // Keywords whose values are maps of name -> schema rather than a schema.
    private static readonly HashSet<string> SchemaMapKeywords = new(StringComparer.Ordinal)
    {
        "dependentSchemas",
        "patternProperties",
        "properties",
    };

    // Keywords holding instance data, which can contain a literal `$ref` key.
    private static readonly HashSet<string> DataKeywords = new(StringComparer.Ordinal)
    {
        "const",
        "default",
        "enum",
        "examples",
    };

    /// <summary>
    /// Whether a JSON schema contains a cycle of local <c>$ref</c>s, e.g. a tree node
    /// whose children reference the node itself.
    /// Anthropic's native structured outputs reject such schemas, while the JSON
    /// response tool accepts them.
    /// </summary>
    public static bool HasCircularSchemaReference(JsonElement schema)
    {
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        bool Visit(string pointer)
        {
            if (visiting.Contains(pointer))
            {
                return true;
            }

            if (visited.Contains(pointer))
            {
                return false;
            }

            var target = ResolveSchemaPointer(schema, pointer);
            if (target == null)
            {
                return false;
            }

            visiting.Add(pointer);
            var isCircular = CollectLocalRefs(target.Value, []).Any(Visit);
            visiting.Remove(pointer);
            visited.Add(pointer);

            return isCircular;
        }

        return Visit("#");
    }

    /// <summary>Resolves a local JSON pointer (<c>#/a/b</c>) against a schema. Returns null when it does not resolve.</summary>
    public static JsonElement? ResolveSchemaPointer(JsonElement schema, string pointer)
    {
        if (pointer == "#")
        {
            return schema;
        }

        if (!pointer.StartsWith("#/", StringComparison.Ordinal))
        {
            return null;
        }

        string decodedPointer;
        try
        {
            decodedPointer = Uri.UnescapeDataString(pointer.Substring(2));
        }
        catch (UriFormatException)
        {
            return null;
        }

        var current = schema;
        foreach (var rawSegment in decodedPointer.Split('/'))
        {
            var segment = rawSegment.Replace("~1", "/").Replace("~0", "~");
            if (current.ValueKind == JsonValueKind.Array)
            {
                if (!int.TryParse(segment, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index)
                    || index >= current.GetArrayLength())
                {
                    return null;
                }

                current = current[index];
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                if (!current.TryGetProperty(segment, out var next))
                {
                    return null;
                }

                current = next;
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private static List<string> CollectLocalRefs(JsonElement node, List<string> refs)
    {
        if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in node.EnumerateArray())
            {
                CollectLocalRefs(item, refs);
            }

            return refs;
        }

        if (node.ValueKind != JsonValueKind.Object)
        {
            return refs;
        }

        foreach (var property in node.EnumerateObject())
        {
            var key = property.Name;
            var value = property.Value;
            if (key == "$ref")
            {
                if (value.ValueKind == JsonValueKind.String && value.GetString()!.StartsWith("#", StringComparison.Ordinal))
                {
                    refs.Add(value.GetString()!);
                }
            }
            else if (SchemaMapKeywords.Contains(key) && value.ValueKind == JsonValueKind.Object)
            {
                foreach (var subschema in value.EnumerateObject())
                {
                    CollectLocalRefs(subschema.Value, refs);
                }
            }
            else if (
                // definitions are only reached through a `$ref`:
                key != "$defs"
                && key != "definitions"
                && !DataKeywords.Contains(key))
            {
                CollectLocalRefs(value, refs);
            }
        }

        return refs;
    }
}
