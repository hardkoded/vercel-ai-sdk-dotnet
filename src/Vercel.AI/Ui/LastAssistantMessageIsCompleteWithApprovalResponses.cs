// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Ui;

/// <summary>Checks on the last assistant message of a chat. Maps to <c>last-assistant-message-is-complete-with-*.ts</c>.</summary>
public static partial class UIMessageCompletion
{
    /// <summary>
    /// Checks if the last message is an assistant message with completed tool call approvals.
    /// The last step of the message must have at least one tool approval response and
    /// all tool approvals must have a response.
    /// Maps to <c>lastAssistantMessageIsCompleteWithApprovalResponses</c>.
    /// </summary>
    public static bool LastAssistantMessageIsCompleteWithApprovalResponses(
        IReadOnlyList<UIMessage> messages
    )
    {
        var lastStepParts = LastStepParts(messages);
        if (lastStepParts is null)
        {
            return false;
        }

        var tools = lastStepParts.OfType<ToolUIPart>().ToList();

        return tools.Any(part => part.State == ToolUIPartState.ApprovalResponded)
            && tools.All(part =>
                (part.State == ToolUIPartState.OutputAvailable && part.Preliminary != true)
                || part.State
                    is ToolUIPartState.OutputError
                        or ToolUIPartState.OutputDenied
                        or ToolUIPartState.ApprovalResponded
            );
    }

    private static IReadOnlyList<UIMessagePart>? LastStepParts(IReadOnlyList<UIMessage> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (messages.Count == 0 || messages[messages.Count - 1].Role != UIMessageRole.Assistant)
        {
            return null;
        }

        var parts = messages[messages.Count - 1].Parts;
        var lastStepStartIndex = -1;
        for (var i = 0; i < parts.Count; i++)
        {
            if (parts[i] is StepStartUIPart)
            {
                lastStepStartIndex = i;
            }
        }

        return parts.Skip(lastStepStartIndex + 1).ToList();
    }
}
