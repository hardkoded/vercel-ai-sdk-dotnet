// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// The approval of each tool call as it arrives in the model stream. Maps to the <c>tool-call</c> case of
/// <c>executeToolsFromStream</c> in <c>execute-tools-from-stream.ts</c>.
/// </summary>
internal static class ExecuteToolsFromStream
{
    /// <summary>
    /// Resolves the approval of one tool call. A call that is invalid or has no known tool, and a call that does not need approval,
    /// has no request and does not use an approval id. A user approval or an automatic denial blocks the call.
    /// </summary>
    internal static async Task<ToolCallApproval> ResolveToolCallApprovalAsync(
        GenerateTextOptions options,
        GeneratedToolCall toolCall,
        IReadOnlyList<ModelMessage> messages
    )
    {
        if (toolCall.Invalid || Generation.FindTool(options, toolCall.ToolName) == null)
        {
            return ToolCallApproval.None;
        }

        var status = await ToolApprovalResolution
            .ResolveToolApprovalAsync(
                options.Tools,
                toolCall,
                options.ToolApproval,
                messages,
                options.ToolsContext,
                options.RuntimeContext
            )
            .ConfigureAwait(false);
        if (status.Type == ToolApprovalStatus.NotApplicableType)
        {
            return ToolCallApproval.None;
        }

        var approvalId = Generation.NewApprovalId(options);
        var signature = Generation.SignApproval(options, approvalId, toolCall);
        switch (status.Type)
        {
            case ToolApprovalStatus.UserApprovalType:
                return new ToolCallApproval(
                    new GeneratedToolApprovalRequest(approvalId, toolCall.ToolCallId)
                    {
                        ToolCall = toolCall,
                        Reason = status.Reason,
                        Signature = signature,
                    },
                    null,
                    true
                );
            case ToolApprovalStatus.ApprovedType:
            case ToolApprovalStatus.DeniedType:
                var approved = status.Type == ToolApprovalStatus.ApprovedType;
                return new ToolCallApproval(
                    new GeneratedToolApprovalRequest(approvalId, toolCall.ToolCallId)
                    {
                        ToolCall = toolCall,
                        IsAutomatic = true,
                        Signature = signature,
                    },
                    new GeneratedToolApprovalResponse(
                        approvalId,
                        toolCall,
                        approved,
                        status.Reason,
                        toolCall.ProviderExecuted
                    ),
                    !approved
                );
            default:
                return ToolCallApproval.None;
        }
    }

    /// <summary>The approval of one tool call. .NET-only: upstream enqueues these parts inside the transformer.</summary>
    internal sealed record ToolCallApproval(
        GeneratedToolApprovalRequest? Request,
        GeneratedToolApprovalResponse? Response,
        bool Blocked
    )
    {
        internal static readonly ToolCallApproval None = new(null, null, false);
    }
}
