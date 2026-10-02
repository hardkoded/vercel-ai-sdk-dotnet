// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>No speech translation was generated.</summary>
public sealed class NoTranslationGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoTranslationGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoTranslationGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoTranslationGeneratedException(MediaResponseMetadata response)
        : base(ErrorName, "No translation generated.", null)
    {
        SetMarker(TypeMarker);
        Response = response;
    }

    /// <summary>Response metadata for the translation call.</summary>
    public MediaResponseMetadata Response { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
