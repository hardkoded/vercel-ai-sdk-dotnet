// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Random id generator. The id is not cryptographically secure.
/// A prefix is joined with <c>separator</c>. The separator must not appear in the alphabet.
/// </summary>
public static class IdGenerator
{
    /// <summary>Default alphabet, letters and digits.</summary>
    public const string DefaultAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly Func<string> Default = Create();
    private static readonly object Gate = new object();
    private static readonly Random Random = new Random();

    /// <summary>Creates a generator. <paramref name="size"/> is the length of the random part.</summary>
    public static Func<string> Create(string? prefix = null, int size = 16, string? alphabet = null, string separator = "-")
    {
        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        var symbols = alphabet ?? DefaultAlphabet;
        if (symbols.Length == 0)
        {
            throw new ArgumentException("Alphabet must not be empty.", nameof(alphabet));
        }

        if (prefix != null && symbols.IndexOf(separator, StringComparison.Ordinal) >= 0)
        {
            throw new ArgumentException(
                "The separator \"" + separator + "\" must not be part of the alphabet \"" + symbols + "\".",
                nameof(separator));
        }

        var length = symbols.Length;
        Func<string> random = () =>
        {
            var chars = new char[size];
            lock (Gate)
            {
                for (var index = 0; index < size; index++)
                {
                    chars[index] = symbols[Random.Next(length)];
                }
            }

            return new string(chars);
        };

        if (prefix == null)
        {
            return random;
        }

        return () => prefix + separator + random();
    }

    /// <summary>Generates a 16-character id from the default alphabet.</summary>
    public static string Generate()
    {
        return Default();
    }
}
