// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>One part of a Bedrock prompt message.</summary>
public abstract class AmazonBedrockPromptPart
{
    /// <summary>Creates a part.</summary>
    protected AmazonBedrockPromptPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Provider metadata for this part, keyed by provider name.</summary>
    public JsonElement? ProviderOptions { get; set; }
}

/// <summary>Text content.</summary>
public sealed class AmazonBedrockTextPart : AmazonBedrockPromptPart
{
    /// <summary>Creates a text part.</summary>
    public AmazonBedrockTextPart(string text)
        : base("text")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>A file, image, video, or document part.</summary>
public sealed class AmazonBedrockFilePart : AmazonBedrockPromptPart
{
    /// <summary>Creates a file part.</summary>
    public AmazonBedrockFilePart(string mediaType)
        : base("file")
    {
        MediaType = mediaType ?? "application/octet-stream";
    }

    /// <summary>IANA media type, or a top-level type such as <c>image</c>.</summary>
    public string MediaType { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; set; }

    /// <summary>Inline bytes. Encoded as base64.</summary>
    public byte[]? Bytes { get; set; }

    /// <summary>Already-encoded base64 payload.</summary>
    public string? Base64 { get; set; }

    /// <summary>UTF-8 text document body.</summary>
    public string? Text { get; set; }

    /// <summary>Remote URL. Only <c>s3:</c> URLs are accepted for images and video.</summary>
    public string? Url { get; set; }

    /// <summary>Whether this part is a provider reference.</summary>
    public bool IsReference { get; set; }
}

/// <summary>Assistant reasoning that may carry a Bedrock signature.</summary>
public sealed class AmazonBedrockReasoningPart : AmazonBedrockPromptPart
{
    /// <summary>Creates a reasoning part.</summary>
    public AmazonBedrockReasoningPart(string text)
        : base("reasoning")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Reasoning text.</summary>
    public string Text { get; }
}

/// <summary>An assistant tool call.</summary>
public sealed class AmazonBedrockToolCallPart : AmazonBedrockPromptPart
{
    /// <summary>Creates a tool call.</summary>
    public AmazonBedrockToolCallPart(string toolCallId, string toolName, JsonNode? input)
        : base("tool-call")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        Input = input;
    }

    /// <summary>Provider tool-call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name. Invalid characters are removed before the request is sent.</summary>
    public string ToolName { get; }

    /// <summary>Tool input. Non-objects are wrapped as <c>rawInvalidInput</c>.</summary>
    public JsonNode? Input { get; }
}

/// <summary>A tool result.</summary>
public sealed class AmazonBedrockToolResultPart : AmazonBedrockPromptPart
{
    /// <summary>Creates a tool result.</summary>
    public AmazonBedrockToolResultPart(string toolCallId, string toolName)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        OutputType = "json";
    }

    /// <summary>Matching tool-call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary><c>text</c>, <c>json</c>, <c>error-text</c>, <c>error-json</c>, <c>content</c>, or <c>execution-denied</c>.</summary>
    public string OutputType { get; set; }

    /// <summary>JSON or text output value.</summary>
    public JsonNode? OutputValue { get; set; }

    /// <summary>Reason used when execution was denied.</summary>
    public string? DenialReason { get; set; }

    /// <summary>Content parts when <see cref="OutputType"/> is <c>content</c>.</summary>
    public IReadOnlyList<AmazonBedrockPromptPart>? Content { get; set; }
}

/// <summary>A prompt message converted into Converse blocks.</summary>
public sealed class AmazonBedrockPromptMessage
{
    /// <summary>Creates a message.</summary>
    public AmazonBedrockPromptMessage(string role)
    {
        Role = role ?? "user";
        Parts = Array.Empty<AmazonBedrockPromptPart>();
    }

    /// <summary><c>system</c>, <c>user</c>, <c>assistant</c>, or <c>tool</c>.</summary>
    public string Role { get; }

    /// <summary>System instruction text.</summary>
    public string? SystemText { get; set; }

    /// <summary>Content parts for user, assistant, and tool messages.</summary>
    public IReadOnlyList<AmazonBedrockPromptPart> Parts { get; set; }

    /// <summary>Provider metadata for the whole message.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Creates a system message.</summary>
    public static AmazonBedrockPromptMessage System(string text, JsonElement? providerOptions = null)
    {
        return new AmazonBedrockPromptMessage("system")
        {
            SystemText = text ?? string.Empty,
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a user message.</summary>
    public static AmazonBedrockPromptMessage User(IReadOnlyList<AmazonBedrockPromptPart> parts, JsonElement? providerOptions = null)
    {
        return new AmazonBedrockPromptMessage("user")
        {
            Parts = parts ?? Array.Empty<AmazonBedrockPromptPart>(),
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates an assistant message.</summary>
    public static AmazonBedrockPromptMessage Assistant(IReadOnlyList<AmazonBedrockPromptPart> parts, JsonElement? providerOptions = null)
    {
        return new AmazonBedrockPromptMessage("assistant")
        {
            Parts = parts ?? Array.Empty<AmazonBedrockPromptPart>(),
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a tool message.</summary>
    public static AmazonBedrockPromptMessage Tool(IReadOnlyList<AmazonBedrockPromptPart> parts, JsonElement? providerOptions = null)
    {
        return new AmazonBedrockPromptMessage("tool")
        {
            Parts = parts ?? Array.Empty<AmazonBedrockPromptPart>(),
            ProviderOptions = providerOptions,
        };
    }
}
