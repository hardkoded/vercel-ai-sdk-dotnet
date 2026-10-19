// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Ui;

/// <summary>The role of a <see cref="UIMessage"/>.</summary>
public enum UIMessageRole
{
    /// <summary>System message. Maps to <c>'system'</c>.</summary>
    System,

    /// <summary>User message. Maps to <c>'user'</c>.</summary>
    User,

    /// <summary>Assistant message. Maps to <c>'assistant'</c>.</summary>
    Assistant,
}

/// <summary>
/// A message shown in a chat UI, made of parts. Maps to <c>UIMessage</c>.
/// </summary>
public sealed class UIMessage
{
    /// <summary>Creates a UI message.</summary>
    public UIMessage(UIMessageRole role, IReadOnlyList<UIMessagePart> parts)
    {
        Role = role;
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
    }

    /// <summary>A unique identifier for the message.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The role of the message.</summary>
    public UIMessageRole Role { get; }

    /// <summary>The metadata of the message.</summary>
    public JsonElement? Metadata { get; init; }

    /// <summary>The parts of the message.</summary>
    public IReadOnlyList<UIMessagePart> Parts { get; }
}

/// <summary>A part of a <see cref="UIMessage"/>. Maps to <c>UIMessagePart</c>.</summary>
public abstract class UIMessagePart
{
    /// <summary>Creates a part.</summary>
    protected UIMessagePart(string type)
    {
        Type = type;
    }

    /// <summary>The part type, such as <c>text</c>, <c>tool-get-weather</c>, or <c>data-chart</c>.</summary>
    public string Type { get; }
}

/// <summary>A text part. Maps to <c>TextUIPart</c>.</summary>
public sealed class TextUIPart : UIMessagePart
{
    /// <summary>Creates a text part.</summary>
    public TextUIPart(string text)
        : base("text")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>The text content.</summary>
    public string Text { get; }

    /// <summary>The state of the text part: <c>streaming</c> or <c>done</c>.</summary>
    public string? State { get; init; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A provider-specific part. Maps to <c>CustomContentUIPart</c>.</summary>
public sealed class CustomContentUIPart : UIMessagePart
{
    /// <summary>Creates a custom part.</summary>
    public CustomContentUIPart(string kind)
        : base("custom")
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
    }

    /// <summary>The kind of custom content, in the format <c>{provider}.{provider-type}</c>.</summary>
    public string Kind { get; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A reasoning part. Maps to <c>ReasoningUIPart</c>.</summary>
public sealed class ReasoningUIPart : UIMessagePart
{
    /// <summary>Creates a reasoning part.</summary>
    public ReasoningUIPart(string text)
        : base("reasoning")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>The reasoning part ID.</summary>
    public string? Id { get; init; }

    /// <summary>The reasoning text.</summary>
    public string Text { get; }

    /// <summary>The state of the reasoning part: <c>streaming</c> or <c>done</c>.</summary>
    public string? State { get; init; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A source that is a URL. Maps to <c>SourceUrlUIPart</c>.</summary>
public sealed class SourceUrlUIPart : UIMessagePart
{
    /// <summary>Creates a source URL part.</summary>
    public SourceUrlUIPart(string sourceId, string url)
        : base("source-url")
    {
        SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
        Url = url ?? throw new ArgumentNullException(nameof(url));
    }

    /// <summary>The source id.</summary>
    public string SourceId { get; }

    /// <summary>The URL.</summary>
    public string Url { get; }

    /// <summary>The title.</summary>
    public string? Title { get; init; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A source that is a document. Maps to <c>SourceDocumentUIPart</c>.</summary>
public sealed class SourceDocumentUIPart : UIMessagePart
{
    /// <summary>Creates a source document part.</summary>
    public SourceDocumentUIPart(string sourceId, string mediaType, string title)
        : base("source-document")
    {
        SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    /// <summary>The source id.</summary>
    public string SourceId { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>The title.</summary>
    public string Title { get; }

    /// <summary>The file name.</summary>
    public string? Filename { get; init; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A file part. Maps to <c>FileUIPart</c>.</summary>
public sealed class FileUIPart : UIMessagePart
{
    /// <summary>Creates a file part.</summary>
    public FileUIPart(string mediaType, string url)
        : base("file")
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Url = url ?? throw new ArgumentNullException(nameof(url));
    }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>The file name.</summary>
    public string? Filename { get; init; }

    /// <summary>The URL, which can be a data URL.</summary>
    public string Url { get; }

    /// <summary>Map from provider name to an uploaded file id. It takes precedence over <see cref="Url"/>.</summary>
    public IReadOnlyDictionary<string, string>? ProviderReference { get; init; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>A file produced as reasoning. Maps to <c>ReasoningFileUIPart</c>.</summary>
public sealed class ReasoningFileUIPart : UIMessagePart
{
    /// <summary>Creates a reasoning file part.</summary>
    public ReasoningFileUIPart(string mediaType, string url)
        : base("reasoning-file")
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Url = url ?? throw new ArgumentNullException(nameof(url));
    }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>The URL, which can be a data URL.</summary>
    public string Url { get; }

    /// <summary>The provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
}

/// <summary>Marks the start of a step. Maps to <c>StepStartUIPart</c>.</summary>
public sealed class StepStartUIPart : UIMessagePart
{
    /// <summary>Creates a step-start part.</summary>
    public StepStartUIPart()
        : base("step-start")
    {
    }
}

/// <summary>A data part. Its type is <c>data-{name}</c>. Maps to <c>DataUIPart</c>.</summary>
public sealed class DataUIPart : UIMessagePart
{
    /// <summary>Creates a data part named <paramref name="name"/>.</summary>
    public DataUIPart(string name, JsonElement data)
        : base("data-" + (name ?? throw new ArgumentNullException(nameof(name))))
    {
        Name = name;
        Data = data;
    }

    /// <summary>The data name, without the <c>data-</c> prefix.</summary>
    public string Name { get; }

    /// <summary>The part id.</summary>
    public string? Id { get; init; }

    /// <summary>The data.</summary>
    public JsonElement Data { get; }
}

/// <summary>The state of a <see cref="ToolUIPart"/>.</summary>
public enum ToolUIPartState
{
    /// <summary>The input is still streaming. Maps to <c>'input-streaming'</c>.</summary>
    InputStreaming,

    /// <summary>The input is complete. Maps to <c>'input-available'</c>.</summary>
    InputAvailable,

    /// <summary>The call waits for an approval. Maps to <c>'approval-requested'</c>.</summary>
    ApprovalRequested,

    /// <summary>The approval was answered. Maps to <c>'approval-responded'</c>.</summary>
    ApprovalResponded,

    /// <summary>The output is available. Maps to <c>'output-available'</c>.</summary>
    OutputAvailable,

    /// <summary>The call failed. Maps to <c>'output-error'</c>.</summary>
    OutputError,

    /// <summary>The approval was denied. Maps to <c>'output-denied'</c>.</summary>
    OutputDenied,
}

/// <summary>The approval of a tool call in a <see cref="ToolUIPart"/>.</summary>
public sealed class ToolUIApproval
{
    /// <summary>Creates an approval.</summary>
    public ToolUIApproval(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    /// <summary>The approval id.</summary>
    public string Id { get; }

    /// <summary>Whether the call was approved. Null while the approval is requested.</summary>
    public bool? Approved { get; init; }

    /// <summary>Why the approval was requested.</summary>
    public string? RequestReason { get; init; }

    /// <summary>Why the call was approved or denied.</summary>
    public string? Reason { get; init; }

    /// <summary>Whether the approval was decided automatically.</summary>
    public bool? IsAutomatic { get; init; }
}

/// <summary>
/// A tool invocation part. Its type is <c>tool-{name}</c>, or <c>dynamic-tool</c> for a tool whose name is not known up front.
/// Maps to <c>ToolUIPart</c> and <c>DynamicToolUIPart</c>.
/// </summary>
public sealed class ToolUIPart : UIMessagePart
{
    /// <summary>Creates a tool part for the tool <paramref name="toolName"/>.</summary>
    public ToolUIPart(string toolName, string toolCallId, ToolUIPartState state, bool isDynamic = false)
        : base(isDynamic ? "dynamic-tool" : "tool-" + (toolName ?? throw new ArgumentNullException(nameof(toolName))))
    {
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        State = state;
        IsDynamic = isDynamic;
    }

    /// <summary>The tool name.</summary>
    public string ToolName { get; }

    /// <summary>The tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>The state of the invocation.</summary>
    public ToolUIPartState State { get; }

    /// <summary>True for a <c>dynamic-tool</c> part.</summary>
    public bool IsDynamic { get; }

    /// <summary>
    /// For a <c>dynamic-tool</c> part, false when the part came from a static tool that is no longer available.
    /// Maps to <c>dynamic: false</c>.
    /// </summary>
    public bool? Dynamic { get; init; }

    /// <summary>The title of the tool.</summary>
    public string? Title { get; init; }

    /// <summary>Whether the provider executed the tool.</summary>
    public bool? ProviderExecuted { get; init; }

    /// <summary>The tool input. Null when it is undefined.</summary>
    public JsonElement? Input { get; init; }

    /// <summary>The raw input of a failed or streaming call.</summary>
    public JsonElement? RawInput { get; init; }

    /// <summary>The tool output. Null when it is undefined.</summary>
    public JsonElement? Output { get; init; }

    /// <summary>The error text of a failed call.</summary>
    public string? ErrorText { get; init; }

    /// <summary>True when the output is a preliminary result.</summary>
    public bool? Preliminary { get; init; }

    /// <summary>The provider metadata of the call.</summary>
    public IReadOnlyDictionary<string, JsonElement>? CallProviderMetadata { get; init; }

    /// <summary>The provider metadata of the result.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ResultProviderMetadata { get; init; }

    /// <summary>The approval of the call.</summary>
    public ToolUIApproval? Approval { get; init; }
}
