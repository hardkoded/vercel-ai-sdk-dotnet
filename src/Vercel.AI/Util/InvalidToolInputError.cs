// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>The input of a tool call does not match the tool's input schema. Maps to <c>InvalidToolInputError</c>.</summary>
public sealed class InvalidToolInputError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_InvalidToolInputError";

    /// <summary>Creates the error. <paramref name="message"/> defaults to the upstream format.</summary>
    public InvalidToolInputError(string toolName, string toolInput, Exception? cause = null, string? message = null)
        : base("AI_InvalidToolInputError", message ?? "Invalid input for tool " + toolName + ": " + (cause?.Message ?? "unknown error"), cause)
    {
        ToolName = toolName;
        ToolInput = toolInput;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Name of the tool that was called.</summary>
    public string ToolName { get; }

    /// <summary>Raw input of the call.</summary>
    public string ToolInput { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
