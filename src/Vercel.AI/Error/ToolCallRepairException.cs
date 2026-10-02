// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>Repairing a failed tool call itself failed.</summary>
public sealed class ToolCallRepairException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_ToolCallRepairError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_ToolCallRepairError";

    /// <summary>
    /// Creates the exception.
    /// <paramref name="originalError"/> is the <see cref="NoSuchToolException"/> or <see cref="InvalidToolInputException"/> that repair tried to fix.
    /// </summary>
    public ToolCallRepairException(object? cause, AiSdkException originalError, string? message = null)
        : base(ErrorName, message ?? ("Error repairing tool call: " + ErrorMessages.GetErrorMessage(cause)), cause)
    {
        SetMarker(TypeMarker);
        OriginalError = originalError;
    }

    /// <summary>Tool error that repair attempted to fix.</summary>
    public AiSdkException OriginalError { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
