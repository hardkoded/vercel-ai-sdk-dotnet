// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Mistral tool-call ids on Bedrock must be exactly nine alphanumeric characters.</summary>
public static class AmazonBedrockToolIds
{
    private const string Base62Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int NormalizedLength = 9;

    /// <summary>True when <paramref name="modelId"/> is a Mistral model, including region-prefixed ids.</summary>
    public static bool IsMistralModel(string modelId)
    {
        return modelId != null && modelId.IndexOf("mistral.", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Returns <paramref name="toolCallId"/> unchanged for other models.
    /// For Mistral, keeps a valid nine-character alphanumeric id and hashes any other id into one.
    /// </summary>
    public static string Normalize(string toolCallId, bool isMistral)
    {
        if (!isMistral || toolCallId == null)
        {
            return toolCallId ?? string.Empty;
        }

        return IsValidMistralId(toolCallId) ? toolCallId : ConvertToBase62Hash(toolCallId);
    }

    private static bool IsValidMistralId(string value)
    {
        if (value.Length != NormalizedLength)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!((character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z')
                || (character >= '0' && character <= '9')))
            {
                return false;
            }
        }

        return true;
    }

    // FNV-1a 64-bit is deterministic across runtimes and reaches the full nine-character base62 space.
    private static string ConvertToBase62Hash(string value)
    {
        var hash = 14695981039346656037UL;
        foreach (var character in value)
        {
            hash ^= character;
            unchecked
            {
                hash *= 1099511628211UL;
            }
        }

        var space = 1UL;
        for (var i = 0; i < NormalizedLength; i++)
        {
            space *= (ulong)Base62Characters.Length;
        }

        var remaining = hash % space;
        var result = new char[NormalizedLength];
        for (var i = NormalizedLength - 1; i >= 0; i--)
        {
            result[i] = Base62Characters[(int)(remaining % (ulong)Base62Characters.Length)];
            remaining /= (ulong)Base62Characters.Length;
        }

        return new string(result);
    }
}
