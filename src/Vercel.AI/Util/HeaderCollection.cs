// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Case-insensitive header list that preserves repeated values such as <c>Set-Cookie</c>.</summary>
public sealed class HeaderCollection
{
    private readonly List<KeyValuePair<string, string>> _items = new List<KeyValuePair<string, string>>();

    /// <summary>Creates an empty collection.</summary>
    public HeaderCollection()
    {
    }

    /// <summary>Copies <paramref name="headers"/>.</summary>
    public HeaderCollection(IEnumerable<KeyValuePair<string, string>>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            Add(header.Key, header.Value);
        }
    }

    /// <summary>Appends one header value.</summary>
    public void Add(string name, string value)
    {
        _items.Add(new KeyValuePair<string, string>(name, value));
    }

    /// <summary>Returns whether a header with <paramref name="name"/> is present.</summary>
    public bool Contains(string name)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>First value for <paramref name="name"/>, or <c>null</c>.</summary>
    public string? Get(string name)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return _items[i].Value;
            }
        }

        return null;
    }

    /// <summary>Every value for <paramref name="name"/>.</summary>
    public IReadOnlyList<string> GetAll(string name)
    {
        var values = new List<string>();
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].Key, name, StringComparison.OrdinalIgnoreCase))
            {
                values.Add(_items[i].Value);
            }
        }

        return values;
    }

    /// <summary>Header entries in insertion order.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Entries()
    {
        return _items;
    }
}

/// <summary>Fills default headers without replacing headers the caller already set.</summary>
public static class PrepareHeaders
{
    /// <summary>
    /// Copies <paramref name="headers"/> and sets each default that is not already present.
    /// Header names are compared case-insensitively.
    /// </summary>
    public static HeaderCollection Apply(HeaderCollection? headers, IReadOnlyDictionary<string, string>? defaultHeaders)
    {
        var result = new HeaderCollection(headers == null ? null : headers.Entries());
        if (defaultHeaders == null)
        {
            return result;
        }

        foreach (var header in defaultHeaders)
        {
            if (!result.Contains(header.Key))
            {
                result.Add(header.Key, header.Value);
            }
        }

        return result;
    }
}
