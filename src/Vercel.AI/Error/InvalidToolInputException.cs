// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>Tool input failed validation.</summary>
public sealed class InvalidToolInputException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidToolInputError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidToolInputError";

    /// <summary>Creates the exception. A null message includes <paramref name="toolName"/> and <paramref name="cause"/>.</summary>
    public InvalidToolInputException(string toolName, string toolInput, object? cause, string? message = null)
        : base(ErrorName, message ?? ("Invalid input for tool " + toolName + ": " + ErrorMessages.GetErrorMessage(cause)), cause)
    {
        SetMarker(TypeMarker);
        ToolName = toolName;
        ToolInput = toolInput;
    }

    /// <summary>Tool that received the input.</summary>
    public string ToolName { get; }

    /// <summary>Raw tool input.</summary>
    public string ToolInput { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
