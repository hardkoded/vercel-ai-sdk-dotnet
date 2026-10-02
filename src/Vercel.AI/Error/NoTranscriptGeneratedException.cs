// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>No transcript was generated.</summary>
public sealed class NoTranscriptGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoTranscriptGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoTranscriptGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoTranscriptGeneratedException(IReadOnlyList<MediaResponseMetadata> responses)
        : base(ErrorName, "No transcript generated.", null)
    {
        SetMarker(TypeMarker);
        Responses = responses;
    }

    /// <summary>Response metadata for each transcription call.</summary>
    public IReadOnlyList<MediaResponseMetadata> Responses { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
