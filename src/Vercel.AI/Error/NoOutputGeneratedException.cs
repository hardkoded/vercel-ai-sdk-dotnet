// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>Thrown when no model output was generated, for example because an earlier call failed.</summary>
public sealed class NoOutputGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoOutputGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoOutputGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoOutputGeneratedException(string? message = null, Exception? cause = null)
        : base(ErrorName, message ?? "No output generated.", cause)
    {
        SetMarker(TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
