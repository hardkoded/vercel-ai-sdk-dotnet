// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI;

/// <summary>Keeps the context properties that telemetry may include. Maps to <c>telemetry/filter-included-context.ts</c>.</summary>
internal static class FilterIncludedContextHelper
{
    /// <summary>
    /// Returns a shallow copy of the runtime context with only the top-level properties marked for telemetry inclusion.
    /// Maps to <c>filterIncludedContext</c>.
    /// </summary>
    internal static IReadOnlyDictionary<string, object?> FilterIncludedContext(
        IReadOnlyDictionary<string, object?>? context,
        IReadOnlyDictionary<string, bool>? includeContext
    )
    {
        var result = new Dictionary<string, object?>();
        if (context == null)
        {
            return result;
        }

        foreach (var entry in context)
        {
            if (
                includeContext != null
                && includeContext.TryGetValue(entry.Key, out var include)
                && include
            )
            {
                result[entry.Key] = entry.Value;
            }
        }

        return result;
    }

    /// <summary>Filters the context of every tool. Maps to <c>filterToolsContext</c>.</summary>
    internal static IReadOnlyDictionary<string, JsonElement> FilterToolsContext(
        IReadOnlyDictionary<string, JsonElement>? toolsContext,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, bool>>? includeToolsContext
    )
    {
        var result = new Dictionary<string, JsonElement>();
        if (includeToolsContext == null || toolsContext == null)
        {
            return result;
        }

        foreach (var entry in toolsContext)
        {
            result[entry.Key] = FilterToolContext(entry.Key, entry.Value, includeToolsContext);
        }

        return result;
    }

    /// <summary>Filters the context of one tool. Maps to <c>filterToolContext</c>.</summary>
    internal static JsonElement FilterToolContext(
        string toolName,
        JsonElement toolContext,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, bool>>? includeToolsContext
    )
    {
        IReadOnlyDictionary<string, bool>? include = null;
        includeToolsContext?.TryGetValue(toolName, out include);

        var filtered = new Dictionary<string, JsonElement>();
        if (toolContext.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in toolContext.EnumerateObject())
            {
                if (include != null && include.TryGetValue(property.Name, out var on) && on)
                {
                    filtered[property.Name] = property.Value.Clone();
                }
            }
        }

        return JsonSerializer.SerializeToElement(filtered);
    }
}
