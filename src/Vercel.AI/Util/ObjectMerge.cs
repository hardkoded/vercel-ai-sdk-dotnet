// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Text.RegularExpressions;

namespace Vercel.AI.Util;

/// <summary>Deep-merges JSON objects. Arrays, dates, and regular expressions are replaced.</summary>
public static class ObjectMerge
{
    /// <summary>
    /// Merges <paramref name="overrides"/> onto a copy of <paramref name="baseObject"/>.
    /// Undefined override values are skipped. <c>__proto__</c>, <c>constructor</c>, and <c>prototype</c> are ignored.
    /// Returns <c>null</c> when both inputs are null.
    /// </summary>
    public static IDictionary<string, object>? MergeObjects(IDictionary<string, object>? baseObject, IDictionary<string, object>? overrides)
    {
        if (baseObject == null && overrides == null)
        {
            return null;
        }

        if (baseObject == null)
        {
            return overrides;
        }

        if (overrides == null)
        {
            return baseObject;
        }

        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var pair in baseObject)
        {
            result[pair.Key] = pair.Value;
        }

        foreach (var pair in overrides)
        {
            if (IsDangerousKey(pair.Key) || pair.Value is JsUndefined)
            {
                continue;
            }

            object? baseValue;
            result.TryGetValue(pair.Key, out baseValue);
            var overrideMap = AsMergeableMap(pair.Value);
            var baseMap = AsMergeableMap(baseValue);
            if (overrideMap != null && baseMap != null)
            {
                var merged = MergeObjects(baseMap, overrideMap);
                if (merged != null)
                {
                    result[pair.Key] = merged;
                }
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    private static bool IsDangerousKey(string key)
    {
        return key == "__proto__" || key == "constructor" || key == "prototype";
    }

    private static IDictionary<string, object>? AsMergeableMap(object? value)
    {
        if (value == null || value is JsUndefined || value is string || value is IList || value is DateTime || value is Delegate || value is Regex)
        {
            return null;
        }

        return value as IDictionary<string, object>;
    }
}
