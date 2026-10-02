// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.GenerateText;

/// <summary>Active-tool filtering. Maps to <c>filterActiveTools</c>.</summary>
public static class ActiveToolFilter
{
    /// <summary>
    /// Keeps tools whose names are in <paramref name="activeTools"/>.
    /// A null tool set stays null. A null active-tool list returns <paramref name="tools"/> unchanged.
    /// </summary>
    public static IReadOnlyDictionary<string, T>? FilterActiveTools<T>(
        IReadOnlyDictionary<string, T>? tools,
        IReadOnlyList<string>? activeTools)
    {
        if (tools is null || activeTools is null)
        {
            return tools;
        }

        var filtered = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var pair in tools)
        {
            if (Contains(activeTools, pair.Key))
            {
                filtered.Add(pair.Key, pair.Value);
            }
        }

        return filtered;
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        foreach (var candidate in names)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
