// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace Vercel.AI.ProviderUtils;

/// <summary>Normalizes a single value or an array. Maps to <c>asArray</c>.</summary>
public static class Arrays
{
    /// <summary>
    /// Returns an empty array for null or <see cref="JsUndefined"/>, the same array instance for an array,
    /// or a one-element array otherwise.
    /// </summary>
    public static object AsArray(object? value)
    {
        if (value is null || value is JsUndefined)
        {
            return Array.Empty<object>();
        }

        if (value is Array)
        {
            return value;
        }

        return new[] { value };
    }
}

/// <summary>Drops null and undefined values. Maps to <c>filterNullable</c>.</summary>
public static class NullableValues
{
    /// <summary>Returns the values that are not null or <see cref="JsUndefined"/>.</summary>
    public static IReadOnlyList<object?> FilterNullable(params object?[] values)
    {
        var result = new List<object?>();
        if (values is null)
        {
            return result;
        }

        foreach (var value in values)
        {
            if (value != null && value is not JsUndefined)
            {
                result.Add(value);
            }
        }

        return result;
    }
}

/// <summary>Removes a single trailing slash. Maps to <c>withoutTrailingSlash</c>.</summary>
public static class Urls
{
    /// <summary>Removes one trailing slash. Null stays null.</summary>
    public static string? WithoutTrailingSlash(string? url)
    {
        if (url is null)
        {
            return null;
        }

        if (url.Length > 0 && url[url.Length - 1] == '/')
        {
            return url.Substring(0, url.Length - 1);
        }

        return url;
    }

    /// <summary>
    /// True when <paramref name="url"/> and <paramref name="baseUrl"/> have the same origin.
    /// Invalid URLs return false. Maps to <c>isSameOrigin</c>.
    /// </summary>
    public static bool IsSameOrigin(string? url, string? baseUrl)
    {
        try
        {
            if (url is null || baseUrl is null)
            {
                return false;
            }

            return Origin(new Uri(url, UriKind.Absolute)) == Origin(new Uri(baseUrl, UriKind.Absolute));
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private static string Origin(Uri uri)
    {
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return uri.Scheme + "://" + uri.Host + port;
    }
}

/// <summary>Filename helpers. Maps to <c>stripFileExtension</c> and <c>mediaTypeToExtension</c>.</summary>
public static class FileNames
{
    /// <summary>Returns the text before the first dot.</summary>
    public static string StripFileExtension(string filename)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        var dot = filename.IndexOf('.');
        return dot < 0 ? filename : filename.Substring(0, dot);
    }

    /// <summary>Maps a media type's subtype to a file extension.</summary>
    public static string MediaTypeToExtension(string mediaType)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        var slash = mediaType.IndexOf('/');
        var subtype = slash < 0 ? string.Empty : mediaType.Substring(slash + 1).ToLowerInvariant();
        switch (subtype)
        {
            case "mpeg":
                return "mp3";
            case "x-wav":
                return "wav";
            case "opus":
                return "ogg";
            case "mp4":
            case "x-m4a":
                return "m4a";
            default:
                return subtype;
        }
    }
}

/// <summary>Object-shape checks. Maps to <c>isRecord</c>, <c>isJSONSerializable</c>, and <c>removeUndefinedEntries</c>.</summary>
public static class Records
{
    /// <summary>True for a non-null object that is not an array or a primitive.</summary>
    public static bool IsRecord(object? value)
    {
        if (value is null || value is JsUndefined || value is string)
        {
            return false;
        }

        if (value is Array)
        {
            return false;
        }

        if (value is ValueType && value is not DateTime && value is not DateTimeOffset)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// True when <paramref name="value"/> can cross a JSON serialization boundary.
    /// Plain dictionaries are accepted. Class instances, dates, and regular expressions are not.
    /// </summary>
    public static bool IsJsonSerializable(object? value)
    {
        if (value is null || value is JsUndefined)
        {
            return true;
        }

        if (value is string || value is bool)
        {
            return true;
        }

        if (IsJsonNumber(value))
        {
            return true;
        }

        if (value is Delegate || value is System.Numerics.BigInteger || value is JsSymbol)
        {
            return false;
        }

        if (value is Array array)
        {
            foreach (var item in array)
            {
                if (!IsJsonSerializable(item))
                {
                    return false;
                }
            }

            return true;
        }

        if (value is IList list && value is not IDictionary)
        {
            foreach (var item in list)
            {
                if (!IsJsonSerializable(item))
                {
                    return false;
                }
            }

            return true;
        }

        if (IsPlainDictionary(value))
        {
            foreach (var item in ((IDictionary)value).Values)
            {
                if (!IsJsonSerializable(item))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>Copies entries whose values are not null or <see cref="JsUndefined"/>.</summary>
    public static Dictionary<string, object?> RemoveUndefinedEntries(IReadOnlyDictionary<string, object?>? record)
    {
        var result = new Dictionary<string, object?>();
        if (record is null)
        {
            return result;
        }

        foreach (var pair in record)
        {
            if (pair.Value != null && pair.Value is not JsUndefined)
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    private static bool IsPlainDictionary(object value)
    {
        var type = value.GetType();
        return type == typeof(Dictionary<string, object?>) || type == typeof(Dictionary<string, object>);
    }

    private static bool IsJsonNumber(object value)
    {
        return value is byte
            || value is sbyte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal;
    }
}
