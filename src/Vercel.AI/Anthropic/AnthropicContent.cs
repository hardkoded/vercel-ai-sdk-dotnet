// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Generated text that also carries Anthropic provider metadata, such as citations.</summary>
public sealed class AnthropicText : GeneratedContent
{
    /// <summary>Creates text content.</summary>
    public AnthropicText(string text, JsonElement? providerMetadata)
        : base("text")
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Generated text.</summary>
    public string Text { get; }

    /// <summary>Provider metadata, when the block has citations or a compaction marker.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Reasoning content with a thinking signature or redacted payload.</summary>
public sealed class AnthropicReasoningContent : GeneratedContent
{
    /// <summary>Creates reasoning content.</summary>
    public AnthropicReasoningContent(string text, JsonElement providerMetadata)
        : base("reasoning")
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Thinking text. Empty for redacted thinking.</summary>
    public string Text { get; }

    /// <summary>Signature or redacted data.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>A provider block the V4 content list has no dedicated type for.</summary>
public sealed class AnthropicCustomContent : GeneratedContent
{
    /// <summary>Creates custom content.</summary>
    public AnthropicCustomContent(string kind, JsonElement? providerMetadata)
        : base("custom")
    {
        Kind = kind ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Custom kind, such as <c>anthropic.container_upload</c>.</summary>
    public string Kind { get; }

    /// <summary>Provider metadata for the custom block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A provider-executed tool result returned inside a Messages response.</summary>
public sealed class AnthropicToolResultContent : GeneratedContent
{
    /// <summary>Creates a tool result.</summary>
    public AnthropicToolResultContent(string toolCallId, string toolName, string resultJson, bool isError, JsonElement? providerMetadata)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ResultJson = resultJson ?? "null";
        IsError = isError;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result.</summary>
    public string ResultJson { get; }

    /// <summary>Whether the tool failed.</summary>
    public bool IsError { get; }

    /// <summary>Provider metadata, including caller information.</summary>
    public JsonElement? ProviderMetadata { get; }
}
