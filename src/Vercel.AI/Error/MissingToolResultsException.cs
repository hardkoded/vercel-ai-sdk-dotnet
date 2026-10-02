// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A follow-up model call is missing tool results for one or more tool calls.</summary>
public sealed class MissingToolResultsException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_MissingToolResultsError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_MissingToolResultsError";

    /// <summary>Creates the exception.</summary>
    public MissingToolResultsException(IReadOnlyList<string> toolCallIds)
        : base(ErrorName, BuildMessage(toolCallIds), null)
    {
        SetMarker(TypeMarker);
        ToolCallIds = toolCallIds;
    }

    /// <summary>Tool call ids that have no result.</summary>
    public IReadOnlyList<string> ToolCallIds { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string BuildMessage(IReadOnlyList<string> toolCallIds)
    {
        var plural = toolCallIds.Count > 1;
        return "Tool result" + (plural ? "s are" : " is")
            + " missing for tool call" + (plural ? "s" : "")
            + " " + string.Join(", ", toolCallIds) + ".";
    }
}
