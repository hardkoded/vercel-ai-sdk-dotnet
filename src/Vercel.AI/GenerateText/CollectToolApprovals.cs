// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>One part of an approval history message.</summary>
public abstract class ApprovalHistoryPart
{
    /// <summary>Creates a part.</summary>
    protected ApprovalHistoryPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }
}

/// <summary>A tool call stored in assistant history.</summary>
public sealed class ApprovalToolCallPart : ApprovalHistoryPart
{
    /// <summary>Creates a tool call part.</summary>
    public ApprovalToolCallPart(string toolCallId, string toolName, JsonElement input)
        : base("tool-call")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        Input = input;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Tool input.</summary>
    public JsonElement Input { get; }
}

/// <summary>A request that a tool call be approved.</summary>
public sealed class ApprovalRequestPart : ApprovalHistoryPart
{
    /// <summary>Creates an approval request part.</summary>
    public ApprovalRequestPart(string approvalId, string toolCallId)
        : base("tool-approval-request")
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
    }

    /// <summary>Approval id.</summary>
    public string ApprovalId { get; }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }
}

/// <summary>The caller's decision for an approval request.</summary>
public sealed class ApprovalResponsePart : ApprovalHistoryPart
{
    /// <summary>Creates an approval response part.</summary>
    public ApprovalResponsePart(string approvalId, bool approved, string? reason = null)
        : base("tool-approval-response")
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        Approved = approved;
        Reason = reason;
    }

    /// <summary>Approval id.</summary>
    public string ApprovalId { get; }

    /// <summary>Whether the call was approved.</summary>
    public bool Approved { get; }

    /// <summary>Denial reason, when the caller sent one.</summary>
    public string? Reason { get; }
}

/// <summary>A tool result stored beside an approval response.</summary>
public sealed class ApprovalToolResultPart : ApprovalHistoryPart
{
    /// <summary>Creates a tool result part.</summary>
    public ApprovalToolResultPart(string toolCallId, string toolName, string outputType, JsonElement? outputValue = null, string? reason = null)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
        OutputValue = outputValue;
        Reason = reason;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Output type, such as <c>text</c> or <c>execution-denied</c>.</summary>
    public string OutputType { get; }

    /// <summary>Output value for text and JSON results.</summary>
    public JsonElement? OutputValue { get; }

    /// <summary>Denial reason.</summary>
    public string? Reason { get; }
}

/// <summary>A message in the approval history.</summary>
public sealed class ApprovalHistoryMessage
{
    /// <summary>Creates a message. <paramref name="parts"/> is null when the message content is a string.</summary>
    public ApprovalHistoryMessage(string role, string? text = null, IReadOnlyList<ApprovalHistoryPart>? parts = null)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Text = text;
        Parts = parts;
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>String content.</summary>
    public string? Text { get; }

    /// <summary>Structured content. Null means the message is not a part list.</summary>
    public IReadOnlyList<ApprovalHistoryPart>? Parts { get; }
}

/// <summary>One approval that still needs execution or denial handling.</summary>
public sealed class CollectedToolApproval
{
    /// <summary>Creates a collected approval.</summary>
    public CollectedToolApproval(
        ApprovalRequestPart approvalRequest,
        ApprovalResponsePart approvalResponse,
        ApprovalToolCallPart toolCall,
        ApprovalToolResultPart? existingToolResult = null)
    {
        ApprovalRequest = approvalRequest ?? throw new ArgumentNullException(nameof(approvalRequest));
        ApprovalResponse = approvalResponse ?? throw new ArgumentNullException(nameof(approvalResponse));
        ToolCall = toolCall ?? throw new ArgumentNullException(nameof(toolCall));
        ExistingToolResult = existingToolResult;
    }

    /// <summary>Matching approval request.</summary>
    public ApprovalRequestPart ApprovalRequest { get; }

    /// <summary>Approval response.</summary>
    public ApprovalResponsePart ApprovalResponse { get; }

    /// <summary>Tool call the approval refers to.</summary>
    public ApprovalToolCallPart ToolCall { get; }

    /// <summary>Existing tool result, when a denied call already has an execution-denied output.</summary>
    public ApprovalToolResultPart? ExistingToolResult { get; }
}

/// <summary>Approved and denied tool approvals from the last tool message.</summary>
public sealed class CollectedToolApprovalSet
{
    /// <summary>Creates an empty or filled set.</summary>
    public CollectedToolApprovalSet(IReadOnlyList<CollectedToolApproval> approvedToolApprovals, IReadOnlyList<CollectedToolApproval> deniedToolApprovals)
    {
        ApprovedToolApprovals = approvedToolApprovals ?? Array.Empty<CollectedToolApproval>();
        DeniedToolApprovals = deniedToolApprovals ?? Array.Empty<CollectedToolApproval>();
    }

    /// <summary>Approvals the caller accepted and that have not already produced a result.</summary>
    public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }

    /// <summary>Approvals the caller denied and that still need an execution-denied result.</summary>
    public IReadOnlyList<CollectedToolApproval> DeniedToolApprovals { get; }
}

/// <summary>An approval response names an unknown approval id.</summary>
public sealed class InvalidToolApprovalException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public InvalidToolApprovalException(string approvalId)
        : base("Tool approval response references unknown approvalId: " + JsonSerializer.Serialize(approvalId))
    {
        ApprovalId = approvalId;
    }

    /// <summary>Unknown approval id.</summary>
    public string ApprovalId { get; }
}

/// <summary>An approval request points at a tool call that is not in the history.</summary>
public sealed class ToolCallNotFoundForApprovalException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ToolCallNotFoundForApprovalException(string toolCallId, string approvalId)
        : base("Tool call \"" + toolCallId + "\" not found for approval request \"" + approvalId + "\".")
    {
        ToolCallId = toolCallId;
        ApprovalId = approvalId;
    }

    /// <summary>Missing tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Approval id.</summary>
    public string ApprovalId { get; }
}

/// <summary>Collects tool approvals from the last tool message. Maps to <c>collectToolApprovals</c>.</summary>
public static class ToolApprovalCollection
{
    /// <summary>
    /// Reads approval responses from the last message when it is a tool message.
    /// Returns empty lists for every other ending.
    /// </summary>
    public static CollectedToolApprovalSet CollectToolApprovals(IReadOnlyList<ApprovalHistoryMessage> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (messages.Count == 0 || !string.Equals(messages[messages.Count - 1].Role, "tool", StringComparison.Ordinal))
        {
            return new CollectedToolApprovalSet(Array.Empty<CollectedToolApproval>(), Array.Empty<CollectedToolApproval>());
        }

        var last = messages[messages.Count - 1];
        var toolCalls = new Dictionary<string, ApprovalToolCallPart>(StringComparer.Ordinal);
        var requests = new Dictionary<string, ApprovalRequestPart>(StringComparer.Ordinal);
        foreach (var message in messages)
        {
            if (!string.Equals(message.Role, "assistant", StringComparison.Ordinal) || message.Parts is null)
            {
                continue;
            }

            foreach (var part in message.Parts)
            {
                if (part is ApprovalToolCallPart call)
                {
                    toolCalls[call.ToolCallId] = call;
                }
                else if (part is ApprovalRequestPart request)
                {
                    requests[request.ApprovalId] = request;
                }
            }
        }

        var results = new Dictionary<string, ApprovalToolResultPart>(StringComparer.Ordinal);
        var responses = new List<ApprovalResponsePart>();
        if (last.Parts != null)
        {
            foreach (var part in last.Parts)
            {
                if (part is ApprovalToolResultPart result)
                {
                    results[result.ToolCallId] = result;
                }
                else if (part is ApprovalResponsePart response)
                {
                    responses.Add(response);
                }
            }
        }

        var approved = new List<CollectedToolApproval>();
        var denied = new List<CollectedToolApproval>();
        foreach (var response in responses)
        {
            if (!requests.TryGetValue(response.ApprovalId, out var request))
            {
                throw new InvalidToolApprovalException(response.ApprovalId);
            }

            results.TryGetValue(request.ToolCallId, out var existing);
            if (existing != null && (response.Approved || !string.Equals(existing.OutputType, "execution-denied", StringComparison.Ordinal)))
            {
                continue;
            }

            if (!toolCalls.TryGetValue(request.ToolCallId, out var toolCall))
            {
                throw new ToolCallNotFoundForApprovalException(request.ToolCallId, request.ApprovalId);
            }

            var collected = new CollectedToolApproval(request, response, toolCall, existing);
            if (response.Approved)
            {
                approved.Add(collected);
            }
            else
            {
                denied.Add(collected);
            }
        }

        return new CollectedToolApprovalSet(approved, denied);
    }
}
