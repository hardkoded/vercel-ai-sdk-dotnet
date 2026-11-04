// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>The signature of a tool approval request is missing or does not match. Maps to <c>InvalidToolApprovalSignatureError</c>.</summary>
public sealed class InvalidToolApprovalSignatureError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_InvalidToolApprovalSignatureError";

    /// <summary>Creates the error.</summary>
    public InvalidToolApprovalSignatureError(string approvalId, string toolCallId, string reason)
        : base(
            "AI_InvalidToolApprovalSignatureError",
            "Tool approval signature verification failed for approval \"" + approvalId + "\" (tool call \"" + toolCallId + "\"): " + reason)
    {
        ApprovalId = approvalId;
        ToolCallId = toolCallId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Id of the approval whose signature failed.</summary>
    public string ApprovalId { get; }

    /// <summary>Id of the tool call of the approval.</summary>
    public string ToolCallId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
