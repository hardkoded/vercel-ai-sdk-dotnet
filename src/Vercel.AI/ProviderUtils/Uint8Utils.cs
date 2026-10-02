// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Base64 conversions. Maps to <c>uint8-utils</c>.</summary>
public static class Uint8Utils
{
    /// <summary>Decodes standard or base64url text the way <c>atob</c> does, including missing padding.</summary>
    public static byte[] ConvertBase64ToUint8Array(string base64String)
    {
        if (base64String is null)
        {
            throw new ArgumentNullException(nameof(base64String));
        }

        if (base64String.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var translated = base64String.Replace('-', '+').Replace('_', '/');
        return Atob(translated);
    }

    /// <summary>Encodes bytes as standard base64.</summary>
    public static string ConvertUint8ArrayToBase64(byte[] array)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (array.Length == 0)
        {
            return string.Empty;
        }

        return Convert.ToBase64String(array);
    }

    /// <summary>Returns base64 text unchanged, or encodes a byte array.</summary>
    public static string ConvertToBase64(object value)
    {
        if (value is byte[] bytes)
        {
            return ConvertUint8ArrayToBase64(bytes);
        }

        if (value is string text)
        {
            return text;
        }

        throw new ArgumentException("Value must be a string or a byte array.", nameof(value));
    }

    private static byte[] Atob(string value)
    {
        var remainder = value.Length % 4;
        if (remainder == 1)
        {
            value = value.Substring(0, value.Length - 1);
            remainder = value.Length % 4;
        }

        if (remainder > 0)
        {
            value = value + new string('=', 4 - remainder);
        }

        return Convert.FromBase64String(value);
    }
}
