// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Creates random ids. Maps to <c>createIdGenerator</c> and <c>generateId</c>.</summary>
public static class IdGenerators
{
    /// <summary>Default alphabet. Not cryptographically secure.</summary>
    public const string DefaultAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly object Gate = new object();
    private static readonly Random Random = new Random();

    /// <summary>Generates a 16-character id. Maps to <c>generateId</c>.</summary>
    public static string GenerateId()
    {
        return CreateIdGenerator()();
    }

    /// <summary>Creates an id generator. Maps to <c>createIdGenerator</c>.</summary>
    public static Func<string> CreateIdGenerator(
        string? prefix = null,
        int size = 16,
        string? alphabet = null,
        string separator = "-")
    {
        var characters = alphabet ?? DefaultAlphabet;
        if (prefix != null && characters.IndexOf(separator) >= 0)
        {
            throw new InvalidArgumentError(
                "separator",
                "The separator \"" + separator + "\" must not be part of the alphabet \"" + characters + "\".");
        }

        string Next()
        {
            var buffer = new char[size];
            var length = characters.Length;
            lock (Gate)
            {
                for (var i = 0; i < size; i++)
                {
                    buffer[i] = characters[Random.Next(length)];
                }
            }

            var body = new string(buffer);
            return prefix is null ? body : prefix + separator + body;
        }

        return Next;
    }
}
