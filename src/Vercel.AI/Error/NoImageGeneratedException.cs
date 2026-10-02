// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>No image could be generated or the image response could not be parsed.</summary>
public sealed class NoImageGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoImageGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoImageGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoImageGeneratedException(
        string? message = null,
        Exception? cause = null,
        IReadOnlyList<ImageModelCall>? calls = null,
        IReadOnlyList<MediaResponseMetadata>? responses = null)
        : base(ErrorName, message ?? "No image generated.", cause)
    {
        SetMarker(TypeMarker);
        Calls = calls;
        Responses = responses;
    }

    /// <summary>Underlying image-model calls, when the caller recorded them.</summary>
    public IReadOnlyList<ImageModelCall>? Calls { get; }

    /// <summary>Response metadata for each call, when the caller recorded it.</summary>
    public IReadOnlyList<MediaResponseMetadata>? Responses { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
