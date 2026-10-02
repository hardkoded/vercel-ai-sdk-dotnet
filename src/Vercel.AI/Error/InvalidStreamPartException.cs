// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A language-model stream part failed validation.</summary>
public sealed class InvalidStreamPartException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidStreamPartError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidStreamPartError";

    /// <summary>Creates the exception.</summary>
    public InvalidStreamPartException(LanguageModelStreamPart chunk, string message)
        : base(ErrorName, message, null)
    {
        SetMarker(TypeMarker);
        Chunk = chunk;
    }

    /// <summary>Stream part that failed validation.</summary>
    public LanguageModelStreamPart Chunk { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
