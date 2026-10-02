// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>
/// An argument passed to an AI SDK function is invalid.
/// Maps to the AI-package <c>InvalidArgumentError</c>.
/// </summary>
public sealed class InvalidArgumentException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidArgumentError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidArgumentError";

    /// <summary>Creates the exception.</summary>
    public InvalidArgumentException(string parameter, object? value, string message)
        : base(ErrorName, "Invalid argument for parameter " + parameter + ": " + message, null)
    {
        SetMarker(TypeMarker);
        Parameter = parameter;
        Value = value;
    }

    /// <summary>Parameter that was rejected.</summary>
    public string Parameter { get; }

    /// <summary>Value that was rejected.</summary>
    public object? Value { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
