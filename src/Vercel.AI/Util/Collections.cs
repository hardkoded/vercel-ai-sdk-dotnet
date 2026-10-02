// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Array and property helpers from the AI SDK util package.</summary>
public static class Collections
{
    /// <summary>Splits <paramref name="array"/> into chunks of <paramref name="chunkSize"/>.</summary>
    public static List<List<T>> SplitArray<T>(IReadOnlyList<T>? array, int chunkSize)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (chunkSize <= 0)
        {
            throw new InvalidArgumentError("chunkSize", chunkSize, "chunkSize must be greater than 0");
        }

        var result = new List<List<T>>();
        for (var i = 0; i < array.Count; i += chunkSize)
        {
            var chunk = new List<T>();
            var end = Math.Min(array.Count, i + chunkSize);
            for (var j = i; j < end; j++)
            {
                chunk.Add(array[j]);
            }

            result.Add(chunk);
        }

        return result;
    }

    /// <summary>
    /// Returns an own property value. Inherited names such as <c>constructor</c> are not returned.
    /// Null and undefined objects return <c>null</c>.
    /// </summary>
    public static object? GetOwn(DataObject? obj, string? key)
    {
        if (obj == null || key == null)
        {
            return null;
        }

        object value;
        return obj.Properties.TryGetValue(key, out value) ? value : null;
    }

    /// <summary>Returns an own dictionary entry, or <c>null</c> when the key is absent.</summary>
    public static object? GetOwn(IDictionary<string, object>? obj, string? key)
    {
        if (obj == null || key == null)
        {
            return null;
        }

        object value;
        return obj.TryGetValue(key, out value) ? value : null;
    }

    /// <summary>
    /// Largest index where <paramref name="searchedText"/> occurs, or where a suffix of
    /// <paramref name="text"/> is a prefix of <paramref name="searchedText"/>. Empty search text returns <c>null</c>.
    /// </summary>
    public static int? GetPotentialStartIndex(string? text, string? searchedText)
    {
        if (text == null)
        {
            text = string.Empty;
        }

        if (string.IsNullOrEmpty(searchedText))
        {
            return null;
        }

        var direct = text.IndexOf(searchedText, StringComparison.Ordinal);
        if (direct >= 0)
        {
            return direct;
        }

        for (var i = text.Length - 1; i >= 0; i--)
        {
            var suffix = text.Substring(i);
            if (searchedText.StartsWith(suffix, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return null;
    }
}

/// <summary>
/// String-keyed map with no inherited members. Missing keys, including <c>__proto__</c>, return the default value.
/// </summary>
public sealed class IdMap<T>
{
    private readonly Dictionary<string, T> _items = new Dictionary<string, T>(StringComparer.Ordinal);
    private readonly List<string> _order = new List<string>();

    /// <summary>No prototype object is consulted. Always <c>null</c>.</summary>
    public object? Prototype
    {
        get { return null; }
    }

    /// <summary>Own keys in insertion order.</summary>
    public IReadOnlyList<string> Keys
    {
        get { return _order; }
    }

    /// <summary>Stores <paramref name="value"/> under <paramref name="key"/> as an own entry.</summary>
    public void Set(string key, T value)
    {
        if (!_items.ContainsKey(key))
        {
            _order.Add(key);
        }

        _items[key] = value;
    }

    /// <summary>Returns the own value, or <c>default</c> when <paramref name="key"/> is absent.</summary>
    public T? Get(string? key)
    {
        T value;
        return key != null && _items.TryGetValue(key, out value) ? value : default(T);
    }

    /// <summary>Returns whether <paramref name="key"/> is an own key.</summary>
    public bool Has(string? key)
    {
        return key != null && _items.ContainsKey(key);
    }
}
