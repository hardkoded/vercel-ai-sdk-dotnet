// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>An approval request refers to a tool call that is not in the message history.</summary>
public sealed class ToolCallNotFoundForApprovalException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_ToolCallNotFoundForApprovalError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_ToolCallNotFoundForApprovalError";

    /// <summary>Creates the exception.</summary>
    public ToolCallNotFoundForApprovalException(string toolCallId, string approvalId)
        : base(ErrorName, "Tool call \"" + toolCallId + "\" not found for approval request \"" + approvalId + "\".", null)
    {
        SetMarker(TypeMarker);
        ToolCallId = toolCallId;
        ApprovalId = approvalId;
    }

    /// <summary>Tool call id that was not found.</summary>
    public string ToolCallId { get; }

    /// <summary>Approval request that named the missing call.</summary>
    public string ApprovalId { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
