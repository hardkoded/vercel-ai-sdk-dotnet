// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>No video was generated.</summary>
public sealed class NoVideoGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoVideoGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoVideoGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoVideoGeneratedException(
        IReadOnlyList<MediaResponseMetadata> responses,
        string? message = null,
        object? cause = null)
        : base(ErrorName, message ?? "No video generated.", cause)
    {
        SetMarker(TypeMarker);
        Responses = responses;
    }

    /// <summary>Response metadata for each video call.</summary>
    public IReadOnlyList<MediaResponseMetadata> Responses { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> is this exception and still carries its responses.</summary>
    [Obsolete("Use IsInstance instead.")]
    public static bool IsNoVideoGeneratedError(object? error)
    {
        return error is NoVideoGeneratedException video && video.Name == ErrorName;
    }

    /// <summary>Returns the name, message, stack, cause, and responses.</summary>
    [Obsolete("Do not use this method. It will be removed in the next major version.")]
    public IReadOnlyDictionary<string, object?> ToJson()
    {
        return new Dictionary<string, object?>
        {
            ["name"] = Name,
            ["message"] = Message,
            ["stack"] = StackTrace,
            ["cause"] = Cause,
            ["responses"] = Responses,
        };
    }
}
