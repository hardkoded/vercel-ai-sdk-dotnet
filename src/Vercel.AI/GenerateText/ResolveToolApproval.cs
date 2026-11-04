// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>The options a tool's <see cref="Tool.NeedsApprovalFunction"/> receives. Maps to the second argument of the <c>needsApproval</c> function.</summary>
public sealed class ToolNeedsApprovalOptions
{
    /// <summary>Creates the options.</summary>
    public ToolNeedsApprovalOptions(string toolCallId, IReadOnlyList<ModelMessage> messages)
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>Id of the tool call.</summary>
    public string ToolCallId { get; }

    /// <summary>
    /// Messages that were sent to the language model to initiate the response that contained the tool call.
    /// They do not include the instructions nor the assistant response that contained the tool call.
    /// </summary>
    public IReadOnlyList<ModelMessage> Messages { get; }
}

/// <summary>Resolves the approval state of a tool call. Maps to <c>resolveToolApproval</c>.</summary>
public static class ToolApprovalResolution
{
    /// <summary>
    /// Resolves the approval state for a tool call by checking the user-supplied and the tool-defined approval settings.
    /// The user-supplied settings take precedence. Without any setting, the call does not require approval.
    /// The result is never null: a null status becomes <c>not-applicable</c>.
    /// </summary>
    /// <param name="tools">Tools that are available for the model to call.</param>
    /// <param name="toolCall">A valid tool call.</param>
    /// <param name="toolApproval">User-defined approval configuration. It takes precedence over the tool-defined settings.</param>
    /// <param name="messages">Messages that were sent to the language model to initiate the response that contained the tool call.</param>
    public static async Task<ToolApprovalStatus> ResolveToolApprovalAsync(
        IReadOnlyList<Tool>? tools,
        GeneratedToolCall toolCall,
        ToolApprovalConfiguration? toolApproval,
        IReadOnlyList<ModelMessage> messages)
    {
        if (toolCall is null)
        {
            throw new ArgumentNullException(nameof(toolCall));
        }

        // user-defined generic tool approval
        if (toolApproval?.Function != null)
        {
            return Normalize(await toolApproval.Function(new GenericToolApprovalOptions(toolCall, tools, messages)).ConfigureAwait(false));
        }

        // The tool name can be client-controlled, so tools and approvals are looked up by exact name only.
        Tool? tool = null;
        if (tools != null)
        {
            foreach (var candidate in tools)
            {
                if (string.Equals(candidate.Name, toolCall.ToolName, StringComparison.Ordinal))
                {
                    tool = candidate;
                    break;
                }
            }
        }

        // assume that the input has been validated and matches the tool's input schema
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(toolCall.ArgumentsJson) ? "{}" : toolCall.ArgumentsJson);
        var input = document.RootElement.Clone();

        // user-defined per-tool approval
        if (toolApproval != null && toolApproval.TryGetValue(toolCall.ToolName, out var entry) && entry != null)
        {
            var status = entry.Function != null
                ? await entry.Function(input, new SingleToolApprovalOptions(toolCall.ToolCallId, messages)).ConfigureAwait(false)
                : entry.Status;
            return Normalize(status);
        }

        // tool-defined approval
        if (tool == null || (!tool.NeedsApproval && tool.NeedsApprovalFunction == null))
        {
            return ToolApprovalStatus.NotApplicable;
        }

        var needsApproval = tool.NeedsApprovalFunction != null
            ? await tool.NeedsApprovalFunction(input, new ToolNeedsApprovalOptions(toolCall.ToolCallId, messages)).ConfigureAwait(false)
            : tool.NeedsApproval;

        return needsApproval ? ToolApprovalStatus.UserApproval : ToolApprovalStatus.NotApplicable;
    }

    private static ToolApprovalStatus Normalize(ToolApprovalStatus? status) => status ?? ToolApprovalStatus.NotApplicable;
}
