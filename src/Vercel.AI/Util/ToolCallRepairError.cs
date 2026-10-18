// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Util;

/// <summary>The repair of a tool call failed. Maps to <c>ToolCallRepairError</c>.</summary>
public sealed class ToolCallRepairError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_ToolCallRepairError";

    /// <summary>
    /// Creates the error. <paramref name="cause"/> is the failure of the repair function and
    /// <paramref name="originalError"/> the <see cref="NoSuchToolError"/> or <see cref="InvalidToolInputError"/> that started the repair.
    /// </summary>
    public ToolCallRepairError(Exception cause, AiSdkError originalError, string? message = null)
        : base("AI_ToolCallRepairError", message ?? "Error repairing tool call: " + ErrorMessage.GetErrorMessage(cause), cause)
    {
        OriginalError = originalError;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>The error that started the repair.</summary>
    public AiSdkError OriginalError { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
