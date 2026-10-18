// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>The model called a tool that is not available. Maps to <c>NoSuchToolError</c>.</summary>
public sealed class NoSuchToolError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_NoSuchToolError";

    /// <summary>Creates the error. <paramref name="message"/> defaults to the upstream format.</summary>
    public NoSuchToolError(string toolName, IReadOnlyList<string>? availableTools = null, string? message = null)
        : base(
            "AI_NoSuchToolError",
            message ?? "Model tried to call unavailable tool '" + toolName + "'. " +
                (availableTools == null ? "No tools are available." : "Available tools: " + string.Join(", ", availableTools) + "."))
    {
        ToolName = toolName;
        AvailableTools = availableTools;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Name of the tool that was called.</summary>
    public string ToolName { get; }

    /// <summary>Names of the available tools, or null when no tools were set.</summary>
    public IReadOnlyList<string>? AvailableTools { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
