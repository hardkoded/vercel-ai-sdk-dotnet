// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Prompt;

/// <summary>A tool choice sent to a language model. Maps to the result of <c>prepareToolChoice</c>.</summary>
public sealed class PreparedToolChoice
{
    /// <summary>Creates a tool choice.</summary>
    public PreparedToolChoice(string type, string? toolName = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ToolName = toolName;
    }

    /// <summary>Choice type: auto, none, required, or tool.</summary>
    public string Type { get; }

    /// <summary>Tool name when <see cref="Type"/> is <c>tool</c>.</summary>
    public string? ToolName { get; }
}

/// <summary>Tool-choice normalization. Maps to <c>prepareToolChoice</c>.</summary>
public static class ToolChoices
{
    /// <summary>Returns auto when <paramref name="toolChoice"/> is null.</summary>
    public static PreparedToolChoice PrepareToolChoice(ToolChoice? toolChoice)
    {
        if (toolChoice is null)
        {
            return new PreparedToolChoice("auto");
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new PreparedToolChoice("tool", named.ToolName);
        }

        return new PreparedToolChoice(toolChoice.Type);
    }

    /// <summary>Returns auto when <paramref name="toolChoice"/> is null. String values are auto, none, or required.</summary>
    public static PreparedToolChoice PrepareToolChoice(string? toolChoice)
    {
        if (toolChoice is null)
        {
            return new PreparedToolChoice("auto");
        }

        return new PreparedToolChoice(toolChoice);
    }
}
