// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>Start of a streamed Groq tool-call argument.</summary>
public sealed class GroqToolInputStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input start.</summary>
    public GroqToolInputStartStreamPart(string id, string toolName)
        : base("tool-input-start")
    {
        Id = id ?? string.Empty;
        ToolName = toolName ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }
}

/// <summary>One fragment of a streamed Groq tool-call argument.</summary>
public sealed class GroqToolInputDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input delta.</summary>
    public GroqToolInputDeltaStreamPart(string id, string delta)
        : base("tool-input-delta")
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Argument fragment, including an empty string when the provider sent one.</summary>
    public string Delta { get; }
}

/// <summary>End of a streamed Groq tool-call argument.</summary>
public sealed class GroqToolInputEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input end.</summary>
    public GroqToolInputEndStreamPart(string id)
        : base("tool-input-end")
    {
        Id = id ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }
}
