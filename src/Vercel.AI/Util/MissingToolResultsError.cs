// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A tool call has no tool result before the next user or system message. Maps to <c>MissingToolResultsError</c>.</summary>
public sealed class MissingToolResultsError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_MissingToolResultsError";

    /// <summary>Creates the error.</summary>
    public MissingToolResultsError(IReadOnlyList<string> toolCallIds)
        : base(
            "AI_MissingToolResultsError",
            "Tool result"
                + (toolCallIds.Count > 1 ? "s are" : " is")
                + " missing for tool call"
                + (toolCallIds.Count > 1 ? "s" : string.Empty)
                + " "
                + string.Join(", ", toolCallIds)
                + "."
        )
    {
        ToolCallIds = toolCallIds;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Ids of the tool calls without a result.</summary>
    public IReadOnlyList<string> ToolCallIds { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
