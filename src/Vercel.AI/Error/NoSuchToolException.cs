// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>The model called a tool that is not available.</summary>
public sealed class NoSuchToolException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoSuchToolError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoSuchToolError";

    /// <summary>Creates the exception. A null <paramref name="availableTools"/> means no tools were offered.</summary>
    public NoSuchToolException(string toolName, IReadOnlyList<string>? availableTools = null, string? message = null)
        : base(ErrorName, message ?? DefaultMessage(toolName, availableTools), null)
    {
        SetMarker(TypeMarker);
        ToolName = toolName;
        AvailableTools = availableTools;
    }

    /// <summary>Tool name the model tried to call.</summary>
    public string ToolName { get; }

    /// <summary>Tools that were available. Null when none were offered.</summary>
    public IReadOnlyList<string>? AvailableTools { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string DefaultMessage(string toolName, IReadOnlyList<string>? availableTools)
    {
        var availability = availableTools == null
            ? "No tools are available."
            : "Available tools: " + string.Join(", ", availableTools) + ".";
        return "Model tried to call unavailable tool '" + toolName + "'. " + availability;
    }
}
