// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Ui;

public static partial class UIMessageCompletion
{
    /// <summary>
    /// Checks if the last message is an assistant message with completed tool calls.
    /// The last step of the message must have at least one tool invocation and
    /// all tool invocations must have a result. Completed model text may follow
    /// the tool invocation because client-side tool results update parts in place,
    /// but trailing text without a completed stream state prevents resubmission.
    /// Maps to <c>lastAssistantMessageIsCompleteWithToolCalls</c>.
    /// </summary>
    public static bool LastAssistantMessageIsCompleteWithToolCalls(
        IReadOnlyList<UIMessage> messages
    )
    {
        var lastStepParts = LastStepParts(messages);
        if (lastStepParts is null)
        {
            return false;
        }

        var lastToolInvocationIndex = -1;
        var tools = new List<ToolUIPart>();
        for (var i = 0; i < lastStepParts.Count; i++)
        {
            if (lastStepParts[i] is ToolUIPart { ProviderExecuted: not true } tool)
            {
                tools.Add(tool);
                lastToolInvocationIndex = i;
            }
        }

        return tools.Count > 0
            && !lastStepParts
                .Skip(lastToolInvocationIndex + 1)
                .Any(part => part is TextUIPart { State: not "done" })
            && tools.All(part =>
                (part.State == ToolUIPartState.OutputAvailable && part.Preliminary != true)
                || part.State == ToolUIPartState.OutputError
            );
    }
}
