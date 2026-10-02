// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Finds where a searched string may start. Maps to <c>getPotentialStartIndex</c>.</summary>
public static class TextIndex
{
    /// <summary>
    /// Returns the index of <paramref name="searchedText"/> in <paramref name="text"/>,
    /// or the index of the longest suffix of <paramref name="text"/> that is a prefix of
    /// <paramref name="searchedText"/>. Returns <c>null</c> when the search text is empty or nothing matches.
    /// </summary>
    public static int? GetPotentialStartIndex(string text, string searchedText)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (searchedText is null)
        {
            throw new ArgumentNullException(nameof(searchedText));
        }

        if (searchedText.Length == 0)
        {
            return null;
        }

        var directIndex = text.IndexOf(searchedText, StringComparison.Ordinal);
        if (directIndex >= 0)
        {
            return directIndex;
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

/// <summary>Splits a sequence into chunks. Maps to <c>splitArray</c>.</summary>
public static class Arrays
{
    /// <summary>Splits <paramref name="values"/> into chunks of <paramref name="chunkSize"/>.</summary>
    /// <exception cref="InvalidArgumentException"><paramref name="chunkSize"/> is not greater than zero.</exception>
    public static List<List<T>> SplitArray<T>(IReadOnlyList<T> values, int chunkSize)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (chunkSize <= 0)
        {
            throw new InvalidArgumentException("chunkSize", chunkSize, "chunkSize must be greater than 0");
        }

        var result = new List<List<T>>();
        for (var i = 0; i < values.Count; i += chunkSize)
        {
            var count = Math.Min(chunkSize, values.Count - i);
            var chunk = new List<T>(count);
            for (var j = 0; j < count; j++)
            {
                chunk.Add(values[i + j]);
            }

            result.Add(chunk);
        }

        return result;
    }
}

/// <summary>Case-insensitive header map. Maps to the Web <c>Headers</c> object used by <c>prepareHeaders</c>.</summary>
public sealed class HeaderMap
{
    private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates an empty header map.</summary>
    public HeaderMap()
    {
    }

    /// <summary>Creates a map from <paramref name="headers"/>.</summary>
    public HeaderMap(IEnumerable<KeyValuePair<string, string>>? headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            _values[pair.Key] = pair.Value;
        }
    }

    /// <summary>Returns the header value, or <c>null</c> when it is absent.</summary>
    public string? Get(string name)
    {
        return name is not null && _values.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>Returns whether <paramref name="name"/> is present.</summary>
    public bool Has(string name)
    {
        return name is not null && _values.ContainsKey(name);
    }

    /// <summary>Sets <paramref name="name"/> to <paramref name="value"/>.</summary>
    public void Set(string name, string value)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _values[name] = value;
    }
}

/// <summary>Fills default headers without overwriting existing ones. Maps to <c>prepareHeaders</c>.</summary>
public static class HttpHeaderPreparation
{
    /// <summary>
    /// Copies <paramref name="headers"/> and sets each default that is not already present.
    /// Header names are compared without regard to case.
    /// </summary>
    public static HeaderMap PrepareHeaders(
        IEnumerable<KeyValuePair<string, string>>? headers,
        IReadOnlyDictionary<string, string> defaultHeaders)
    {
        if (defaultHeaders is null)
        {
            throw new ArgumentNullException(nameof(defaultHeaders));
        }

        var result = new HeaderMap(headers);
        foreach (var pair in defaultHeaders)
        {
            if (!result.Has(pair.Key))
            {
                result.Set(pair.Key, pair.Value);
            }
        }

        return result;
    }
}
