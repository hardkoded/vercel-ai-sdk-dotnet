// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A tool approval response names an approval id that is not in the message history.</summary>
public sealed class InvalidToolApprovalException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidToolApprovalError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidToolApprovalError";

    /// <summary>Creates the exception.</summary>
    public InvalidToolApprovalException(string approvalId)
        : base(
            ErrorName,
            "Tool approval response references unknown approvalId: \"" + approvalId + "\". "
                + "No matching tool-approval-request found in message history.",
            null)
    {
        SetMarker(TypeMarker);
        ApprovalId = approvalId;
    }

    /// <summary>Approval id that was not found.</summary>
    public string ApprovalId { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
