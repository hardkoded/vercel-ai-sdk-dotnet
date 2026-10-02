// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A tool approval signature could not be verified.</summary>
public sealed class InvalidToolApprovalSignatureException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidToolApprovalSignatureError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidToolApprovalSignatureError";

    /// <summary>Creates the exception.</summary>
    public InvalidToolApprovalSignatureException(string approvalId, string toolCallId, string reason)
        : base(
            ErrorName,
            "Tool approval signature verification failed for approval \"" + approvalId
                + "\" (tool call \"" + toolCallId + "\"): " + reason,
            null)
    {
        SetMarker(TypeMarker);
        ApprovalId = approvalId;
        ToolCallId = toolCallId;
    }

    /// <summary>Approval id that failed verification.</summary>
    public string ApprovalId { get; }

    /// <summary>Tool call id that failed verification.</summary>
    public string ToolCallId { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
