// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.HuggingFace;

/// <summary>Generated text that carries the Responses output item id.</summary>
public sealed class HuggingFaceText : GeneratedText
{
    /// <summary>Creates text.</summary>
    public HuggingFaceText(string text, JsonElement? providerMetadata)
        : base(text)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata, such as <c>huggingface.itemId</c>.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Reasoning text that carries the Responses output item id.</summary>
public sealed class HuggingFaceReasoning : GeneratedReasoning
{
    /// <summary>Creates reasoning.</summary>
    public HuggingFaceReasoning(string text, JsonElement? providerMetadata)
        : base(text)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata, such as <c>huggingface.itemId</c>.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A tool call that Hugging Face ran itself, such as an MCP call.</summary>
public sealed class HuggingFaceToolCall : GeneratedToolCall
{
    /// <summary>Creates a tool call.</summary>
    public HuggingFaceToolCall(string toolCallId, string toolName, string argumentsJson, bool providerExecuted)
        : base(toolCallId, toolName, argumentsJson)
    {
        ProviderExecuted = providerExecuted;
    }

    /// <summary>True when Hugging Face executed the tool.</summary>
    public bool ProviderExecuted { get; }
}

/// <summary>A tool result returned in the response output.</summary>
public sealed class HuggingFaceToolResult : GeneratedContent
{
    /// <summary>Creates a tool result.</summary>
    public HuggingFaceToolResult(string toolCallId, string toolName, JsonElement result)
        : base("tool-result")
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        Result = result;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Result value.</summary>
    public JsonElement Result { get; }
}
