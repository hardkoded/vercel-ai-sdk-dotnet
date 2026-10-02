// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace Vercel.AI.Util;

/// <summary>Deep equality for parsed JSON-like data. Maps to <c>isDeepEqualData</c>.</summary>
public static class DataEquality
{
    /// <summary>
    /// Returns whether <paramref name="left"/> and <paramref name="right"/> are deeply equal.
    /// Dictionaries of different runtime types are not equal. Delegates are compared by reference.
    /// </summary>
    public static bool IsDeepEqualData(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left is Delegate || right is Delegate)
        {
            return false;
        }

        if (left is DateTime leftDate && right is DateTime rightDate)
        {
            return leftDate == rightDate;
        }

        if (left is DateTime || right is DateTime)
        {
            return false;
        }

        var leftList = AsList(left);
        var rightList = AsList(right);
        if (leftList is not null || rightList is not null)
        {
            if (leftList is null || rightList is null || leftList.Count != rightList.Count)
            {
                return false;
            }

            for (var i = 0; i < leftList.Count; i++)
            {
                if (!IsDeepEqualData(leftList[i], rightList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        var leftMap = AsMap(left);
        var rightMap = AsMap(right);
        if (leftMap is not null || rightMap is not null)
        {
            if (leftMap is null || rightMap is null || left.GetType() != right.GetType())
            {
                return false;
            }

            if (leftMap.Count != rightMap.Count)
            {
                return false;
            }

            foreach (var pair in leftMap)
            {
                if (!rightMap.TryGetValue(pair.Key, out var other) || !IsDeepEqualData(pair.Value, other))
                {
                    return false;
                }
            }

            return true;
        }

        if (IsPrimitive(left) && IsPrimitive(right))
        {
            return Equals(left, right);
        }

        return false;
    }

    private static bool IsPrimitive(object value)
    {
        return value is string
            || value is bool
            || value is byte
            || value is sbyte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal
            || value is char
            || value is JsonUndefined;
    }

    private static IList? AsList(object value)
    {
        if (value is string || value is IDictionary || value is IReadOnlyDictionary<string, object?>)
        {
            return null;
        }

        return value as IList;
    }

    private static IReadOnlyDictionary<string, object?>? AsMap(object value)
    {
        return value as IReadOnlyDictionary<string, object?>;
    }
}

/// <summary>Deep object merge. Maps to <c>mergeObjects</c>.</summary>
public static class ObjectMerge
{
    /// <summary>
    /// Deep-merges <paramref name="overrides"/> onto <paramref name="baseObject"/>.
    /// Nested dictionaries merge. Arrays, dates, and regular expressions are replaced.
    /// <see cref="JsonUndefined"/> override values are skipped.
    /// Keys named <c>__proto__</c>, <c>constructor</c>, and <c>prototype</c> are ignored.
    /// A null argument maps to JavaScript <c>undefined</c>.
    /// </summary>
    public static IDictionary<string, object?>? MergeObjects(
        IDictionary<string, object?>? baseObject,
        IDictionary<string, object?>? overrides)
    {
        if (baseObject is null && overrides is null)
        {
            return null;
        }

        if (baseObject is null)
        {
            return overrides;
        }

        if (overrides is null)
        {
            return baseObject;
        }

        var result = new Dictionary<string, object?>(baseObject.Count, StringComparer.Ordinal);
        foreach (var pair in baseObject)
        {
            result[pair.Key] = pair.Value;
        }

        foreach (var pair in overrides)
        {
            if (IsDangerousKey(pair.Key) || pair.Value is JsonUndefined)
            {
                continue;
            }

            if (IsMergeable(pair.Value)
                && result.TryGetValue(pair.Key, out var existing)
                && IsMergeable(existing))
            {
                result[pair.Key] = MergeObjects(
                    (IDictionary<string, object?>)existing!,
                    (IDictionary<string, object?>)pair.Value!);
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

    private static bool IsMergeable(object? value)
    {
        return value is IDictionary<string, object?> && value is not string;
    }
}

/// <summary>Own-key lookup. Maps to <c>getOwn</c>.</summary>
public static class OwnProperties
{
    /// <summary>
    /// Returns the own value stored at <paramref name="key"/>, or <c>null</c> when the map
    /// is null or the key is absent. Lookup does not walk a prototype chain.
    /// </summary>
    public static object? GetOwn(IReadOnlyDictionary<string, object?>? map, string key)
    {
        if (map is null || key is null)
        {
            return null;
        }

        return map.TryGetValue(key, out var value) ? value : null;
    }
}

/// <summary>
/// A string-keyed map that stores every key, including <c>__proto__</c>, <c>constructor</c>,
/// and <c>prototype</c>. Maps to <c>createIdMap</c>.
/// </summary>
public sealed class IdMap<T>
    where T : class
{
    private readonly Dictionary<string, T> _values = new Dictionary<string, T>(StringComparer.Ordinal);
    private readonly List<string> _order = new List<string>();

    /// <summary>Stores <paramref name="value"/> at <paramref name="key"/>, including prototype-like names.</summary>
    public void Set(string key, T value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (!_values.ContainsKey(key))
        {
            _order.Add(key);
        }

        _values[key] = value;
    }

    /// <summary>Returns the stored value, or <c>default</c> when <paramref name="key"/> is absent.</summary>
    public T? Get(string key)
    {
        if (key is null)
        {
            return default;
        }

        return _values.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>Keys in insertion order.</summary>
    public IReadOnlyList<string> Keys
    {
        get { return _order; }
    }
}
