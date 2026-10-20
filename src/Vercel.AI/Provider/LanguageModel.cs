// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;

namespace Vercel.AI.Provider;

/// <summary>Why a language-model call stopped.</summary>
public enum FinishReason
{
    /// <summary>The model finished naturally.</summary>
    Stop,

    /// <summary>The output token limit was reached.</summary>
    Length,

    /// <summary>The provider filtered the content.</summary>
    ContentFilter,

    /// <summary>The model called one or more tools.</summary>
    ToolCalls,

    /// <summary>The call failed.</summary>
    Error,

    /// <summary>A provider-specific reason.</summary>
    Other,
}

/// <summary>Token counts for a model call.</summary>
public sealed class LanguageModelUsage
{
    /// <summary>Creates usage counts.</summary>
    public LanguageModelUsage(
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        int? cacheReadTokens = null,
        int? cacheWriteTokens = null,
        int? reasoningTokens = null,
        JsonElement? raw = null,
        int? noCacheInputTokens = null,
        int? textTokens = null)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens ?? ((inputTokens ?? 0) + (outputTokens ?? 0));
        CacheReadTokens = cacheReadTokens;
        CacheWriteTokens = cacheWriteTokens;
        ReasoningTokens = reasoningTokens;
        Raw = raw;
        NoCacheInputTokens = noCacheInputTokens;
        TextTokens = textTokens;
    }

    private LanguageModelUsage()
    {
    }

    /// <summary>Prompt tokens.</summary>
    public int? InputTokens { get; }

    /// <summary>Generated tokens.</summary>
    public int? OutputTokens { get; }

    /// <summary>Input plus output tokens.</summary>
    public int? TotalTokens { get; }

    /// <summary>Input tokens read from cache, when the provider reports them.</summary>
    public int? CacheReadTokens { get; }

    /// <summary>Input tokens written to cache, when the provider reports them.</summary>
    public int? CacheWriteTokens { get; }

    /// <summary>Output tokens spent on reasoning, when the provider reports them.</summary>
    public int? ReasoningTokens { get; }

    /// <summary>Provider usage object, unchanged.</summary>
    public JsonElement? Raw { get; }

    /// <summary>Input tokens that were not served from cache, when the provider reports them.</summary>
    public int? NoCacheInputTokens { get; }

    /// <summary>Output tokens that were not reasoning tokens, when the provider reports them.</summary>
    public int? TextTokens { get; }

    /// <summary>Adds two usage values. <see cref="Raw"/> survives only when one side is <see cref="Null"/>.</summary>
    public static LanguageModelUsage Add(LanguageModelUsage left, LanguageModelUsage right)
    {
        return new LanguageModelUsage(
            Sum(left.InputTokens, right.InputTokens),
            Sum(left.OutputTokens, right.OutputTokens),
            Sum(left.TotalTokens, right.TotalTokens),
            Sum(left.CacheReadTokens, right.CacheReadTokens),
            Sum(left.CacheWriteTokens, right.CacheWriteTokens),
            Sum(left.ReasoningTokens, right.ReasoningTokens),
            raw: left.IsNull ? right.Raw : right.IsNull ? left.Raw : null,
            noCacheInputTokens: Sum(left.NoCacheInputTokens, right.NoCacheInputTokens),
            textTokens: Sum(left.TextTokens, right.TextTokens));
    }

    /// <summary>Zero usage.</summary>
    public static LanguageModelUsage Empty { get; } = new(0, 0, 0);

    /// <summary>Usage with every count and <see cref="Raw"/> unset. It is the start value for adding steps.</summary>
    public static LanguageModelUsage Null { get; } = new();

    private bool IsNull =>
        InputTokens is null
        && NoCacheInputTokens is null
        && CacheReadTokens is null
        && CacheWriteTokens is null
        && OutputTokens is null
        && TextTokens is null
        && ReasoningTokens is null
        && TotalTokens is null
        && Raw is null;

    private static int? Sum(int? left, int? right)
    {
        if (left is null && right is null)
        {
            return null;
        }

        return (left ?? 0) + (right ?? 0);
    }
}

/// <summary>A warning produced while calling a model.</summary>
public sealed class CallWarning
{
    /// <summary>Creates a warning.</summary>
    public CallWarning(string type, string message)
    {
        Type = type;
        Message = message;
    }

    /// <summary>Creates an <c>unsupported</c> or <c>compatibility</c> warning about a feature.</summary>
    /// <param name="type">Warning category, such as <c>unsupported</c>.</param>
    /// <param name="feature">The feature the warning is about.</param>
    /// <param name="details">Additional details. When omitted, <see cref="Message"/> is the feature name.</param>
    public CallWarning(string type, string feature, string? details)
    {
        Type = type;
        Feature = feature;
        Message = details ?? feature;
    }

    /// <summary>Warning category, such as <c>unsupported</c>.</summary>
    public string Type { get; }

    /// <summary>The feature that is unsupported or used in compatibility mode, or <see langword="null"/> when the provider did not set one.</summary>
    public string? Feature { get; }

    /// <summary>Human-readable warning. For a feature warning, this is its details text.</summary>
    public string Message { get; }
}

/// <summary>Content the model generated.</summary>
public abstract class GeneratedContent
{
    /// <summary>Creates content of the given type.</summary>
    protected GeneratedContent(string type)
    {
        Type = type;
    }

    /// <summary>Content type name from the V4 specification.</summary>
    public string Type { get; }
}

/// <summary>Generated text.</summary>
public class GeneratedText : GeneratedContent
{
    /// <summary>Creates text content.</summary>
    public GeneratedText(string text, IReadOnlyList<Citation>? citations = null, JsonElement? providerMetadata = null)
        : base("text")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Citations = citations;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Generated text.</summary>
    public string Text { get; }

    /// <summary>
    /// References supporting this text, or null when the provider sent none. They are separate from the retrieved
    /// <see cref="GeneratedSource"/> parts.
    /// </summary>
    public IReadOnlyList<Citation>? Citations { get; }

    /// <summary>Provider metadata for this text, keyed by provider name, such as the message <c>itemId</c>.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A function tool call.</summary>
public class GeneratedToolCall : GeneratedContent
{
    /// <summary>Creates a tool call.</summary>
    public GeneratedToolCall(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata = null)
        : base("tool-call")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        ArgumentsJson = argumentsJson ?? "{}";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider id for this call.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON object arguments.</summary>
    public string ArgumentsJson { get; }

    /// <summary>Provider-specific metadata for this call, such as a thought signature.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>True when the provider executed the tool, so the caller has nothing to run.</summary>
    public bool ProviderExecuted { get; init; }

    /// <summary>True when the tool is not known up front, such as a provider-executed MCP tool. Maps to the call's <c>dynamic</c>.</summary>
    public bool Dynamic { get; init; }

    /// <summary>
    /// True when the call failed to parse: its tool is unknown, its input did not parse, or the input did not match the tool's
    /// input schema. Such a call is <see cref="Dynamic"/> and is never executed. Maps to <c>invalid</c>.
    /// </summary>
    public bool Invalid { get; init; }

    /// <summary>Why the call is <see cref="Invalid"/>. Maps to <c>error</c>.</summary>
    public Exception? Error { get; init; }

    /// <summary>Display title of the tool, when the tool has one. Maps to <c>title</c>.</summary>
    public string? Title { get; init; }

    /// <summary>Metadata of the tool, kept apart from <see cref="ProviderMetadata"/>. Maps to <c>toolMetadata</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
}

/// <summary>
/// A request to approve a tool call, such as one a provider-executed MCP tool needs. A model returns it with
/// <see cref="ToolCallId"/> only; the generation loop fills in <see cref="ToolCall"/>.
/// </summary>
public sealed class GeneratedToolApprovalRequest : GeneratedContent
{
    /// <summary>Creates an approval request.</summary>
    public GeneratedToolApprovalRequest(string approvalId, string toolCallId)
        : base("tool-approval-request")
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
    }

    /// <summary>Approval id the response must carry.</summary>
    public string ApprovalId { get; }

    /// <summary>Id of the tool call that needs approval.</summary>
    public string ToolCallId { get; }

    /// <summary>The tool call that needs approval, once the generation loop resolved it.</summary>
    public GeneratedToolCall? ToolCall { get; init; }
}

/// <summary>Reasoning text kept separate from the answer.</summary>
public class GeneratedReasoning : GeneratedContent
{
    /// <summary>Creates reasoning content.</summary>
    public GeneratedReasoning(string text)
        : this(text, null)
    {
    }

    /// <summary>Creates reasoning content that carries provider metadata, such as an <c>itemId</c> or encrypted content.</summary>
    public GeneratedReasoning(string text, JsonElement? providerMetadata)
        : base("reasoning")
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Reasoning text.</summary>
    public string Text { get; }

    /// <summary>Provider metadata for this reasoning, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A URL source cited by the model.</summary>
public sealed class GeneratedSource : GeneratedContent
{
    /// <summary>Creates a source.</summary>
    public GeneratedSource(string id, string url, string? title, JsonElement? providerMetadata = null)
        : base("source")
    {
        Id = id;
        Url = url;
        Title = title;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Source id. Search results keep the provider result id so citations can be correlated.</summary>
    public string Id { get; }

    /// <summary>Source URL.</summary>
    public string Url { get; }

    /// <summary>Optional title.</summary>
    public string? Title { get; }

    /// <summary>Provider-specific source metadata, such as a snippet or search-result id.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A document source, such as a retrieved file or a cited document.</summary>
public sealed class GeneratedDocumentSource : GeneratedContent
{
    /// <summary>Creates a document source.</summary>
    public GeneratedDocumentSource(string id, string mediaType, string title, string? filename = null, JsonElement? providerMetadata = null)
        : base("source")
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Filename = filename;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Source id.</summary>
    public string Id { get; }

    /// <summary>IANA media type of the document.</summary>
    public string MediaType { get; }

    /// <summary>Document title.</summary>
    public string Title { get; }

    /// <summary>Optional file name.</summary>
    public string? Filename { get; }

    /// <summary>Provider-specific source metadata, such as a file id or a page range.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>
/// A reference that supports generated text: the source it points at, plus the optional range of the text it supports
/// and the cited passage. It is not a second copy of the retrieved sources.
/// </summary>
public sealed class Citation
{
    /// <summary>Creates a citation for a URL source.</summary>
    public Citation(GeneratedSource source, int? startIndex = null, int? endIndex = null, string? citedText = null)
        : this((GeneratedContent)source, startIndex, endIndex, citedText)
    {
    }

    /// <summary>Creates a citation for a document source.</summary>
    public Citation(GeneratedDocumentSource source, int? startIndex = null, int? endIndex = null, string? citedText = null)
        : this((GeneratedContent)source, startIndex, endIndex, citedText)
    {
    }

    private Citation(GeneratedContent source, int? startIndex, int? endIndex, string? citedText)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        StartIndex = startIndex;
        EndIndex = endIndex;
        CitedText = citedText;
    }

    /// <summary>The referenced source: a <see cref="GeneratedSource"/> (URL) or a <see cref="GeneratedDocumentSource"/>.</summary>
    public GeneratedContent Source { get; }

    /// <summary>Start offset in the containing generated text, when the provider supplied one.</summary>
    public int? StartIndex { get; }

    /// <summary>Exclusive end offset in the containing generated text, when the provider supplied one.</summary>
    public int? EndIndex { get; }

    /// <summary>The supporting passage from the source, when the provider supplied one.</summary>
    public string? CitedText { get; }
}

/// <summary>A tool result: one the provider produced itself, such as a web or file search, or one the generation loop produced by running a client tool.</summary>
public sealed class GeneratedToolResult : GeneratedContent
{
    /// <summary>Creates a provider-executed tool result.</summary>
    public GeneratedToolResult(string toolCallId, string toolName, string resultJson, bool isError = false)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ResultJson = resultJson ?? "null";
        IsError = isError;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result.</summary>
    public string ResultJson { get; }

    /// <summary>Whether the tool failed.</summary>
    public bool IsError { get; }

    /// <summary>Provider-specific metadata for this result, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; init; }
}

/// <summary>
/// A provider-specific content part, identified by <see cref="Kind"/>. It carries no data of its own; the provider keeps it in
/// <see cref="ProviderMetadata"/>.
/// </summary>
public sealed class GeneratedCustom : GeneratedContent
{
    /// <summary>Creates a custom content part.</summary>
    public GeneratedCustom(string kind, JsonElement? providerMetadata = null)
        : base("custom")
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider-scoped kind, such as <c>open-responses.extension-replay</c>.</summary>
    public string Kind { get; }

    /// <summary>Provider metadata for this part, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A generated file.</summary>
public class GeneratedFile : GeneratedContent
{
    /// <summary>Creates file content.</summary>
    public GeneratedFile(byte[] data, string mediaType)
        : base("file")
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MediaType = mediaType;
    }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}

/// <summary>How the model should choose tools. Maps to <c>toolChoice</c>.</summary>
public abstract class ToolChoice
{
    private ToolChoice(string type)
    {
        Type = type;
    }

    /// <summary>Choice type: auto, none, required, or tool.</summary>
    public string Type { get; }

    /// <summary>The model may call tools.</summary>
    public static ToolChoice Auto { get; } = new AutoChoice();

    /// <summary>The model must not call tools.</summary>
    public static ToolChoice None { get; } = new NoneChoice();

    /// <summary>The model must call a tool.</summary>
    public static ToolChoice Required { get; } = new RequiredChoice();

    /// <summary>The model must call <paramref name="toolName"/>.</summary>
    public static ToolChoice Tool(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new ArgumentException("Tool name is required.", nameof(toolName));
        }

        return new NamedChoice(toolName);
    }

    private sealed class AutoChoice : ToolChoice
    {
        public AutoChoice()
            : base("auto")
        {
        }
    }

    private sealed class NoneChoice : ToolChoice
    {
        public NoneChoice()
            : base("none")
        {
        }
    }

    private sealed class RequiredChoice : ToolChoice
    {
        public RequiredChoice()
            : base("required")
        {
        }
    }

    /// <summary>A named tool choice.</summary>
    public sealed class NamedChoice : ToolChoice
    {
        internal NamedChoice(string toolName)
            : base("tool")
        {
            ToolName = toolName;
        }

        /// <summary>Required tool name.</summary>
        public string ToolName { get; }
    }
}

/// <summary>A function tool definition sent to the model.</summary>
public sealed class LanguageModelTool
{
    /// <summary>Creates a function tool.</summary>
    public LanguageModelTool(string name, string? description, JsonElement inputSchema, bool? strict = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        InputSchema = inputSchema;
        Strict = strict;
    }

    private LanguageModelTool(string providerToolId, string name, JsonElement args)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ProviderToolId = providerToolId ?? throw new ArgumentNullException(nameof(providerToolId));
        Args = args;
    }

    /// <summary>
    /// Creates a provider-defined tool. Maps to <c>{ type: 'provider', id, name, args }</c>.
    /// <paramref name="id"/> is <c>&lt;provider&gt;.&lt;tool&gt;</c>. <paramref name="args"/> must be a JSON object.
    /// </summary>
    public static LanguageModelTool ProviderDefined(string id, string name, JsonElement args)
    {
        return new LanguageModelTool(id, name, args);
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Provider-defined tool id, or null for a function tool.</summary>
    public string? ProviderToolId { get; }

    /// <summary>Whether this is a provider-defined tool rather than a function tool.</summary>
    public bool IsProviderDefined => ProviderToolId != null;

    /// <summary>Arguments of a provider-defined tool. Undefined for a function tool.</summary>
    public JsonElement Args { get; }

    /// <summary>Tool description.</summary>
    public string? Description { get; }

    /// <summary>JSON Schema for the arguments.</summary>
    public JsonElement InputSchema { get; }

    /// <summary>Responses strict mode. Null means unset, which Responses sends as false.</summary>
    public bool? Strict { get; }

    /// <summary>
    /// JSON Schema for the tool result. OpenAI Responses parses the result as JSON, so text results of this tool are sent as JSON string literals.
    /// </summary>
    public JsonElement? OutputSchema { get; init; }

    /// <summary>
    /// Provider options of the tool, keyed by provider name, such as <c>{ "openai": { "omitFromRequestTools": true } }</c>.
    /// Maps to the tool's <c>providerOptions</c>.
    /// </summary>
    public JsonElement? ProviderOptions { get; init; }
}

/// <summary>A prompt message.</summary>
public abstract class ModelMessage
{
    /// <summary>Creates a message with a role.</summary>
    protected ModelMessage(string role)
    {
        Role = role;
    }

    /// <summary>Message role.</summary>
    public string Role { get; }
}

/// <summary>System instructions.</summary>
public sealed class SystemModelMessage : ModelMessage
{
    /// <summary>Creates a system message.</summary>
    public SystemModelMessage(string content)
        : this(content, null)
    {
    }

    /// <summary>Creates a system message with provider options, such as an OpenAI <c>reasoningEffortUpdate</c>.</summary>
    public SystemModelMessage(string content, IReadOnlyDictionary<string, JsonElement>? providerOptions)
        : base("system")
    {
        Content = content ?? string.Empty;
        ProviderOptions = providerOptions;
    }

    /// <summary>Instruction text.</summary>
    public string Content { get; }

    /// <summary>Provider-specific options for this message.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; }
}

/// <summary>A user content part.</summary>
public abstract class UserContentPart
{
    /// <summary>Creates a part.</summary>
    protected UserContentPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }
}

/// <summary>Text in a user message.</summary>
public sealed class TextContentPart : UserContentPart
{
    /// <summary>Creates a text part.</summary>
    public TextContentPart(string text)
        : base("text")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }

    /// <summary>Provider-specific options for this part, such as the Amazon Bedrock <c>guardContent</c> or <c>cachePoint</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
}

/// <summary>A file or image in a user message.</summary>
public sealed class FileContentPart : UserContentPart
{
    /// <summary>Creates a file part.</summary>
    public FileContentPart(string mediaType, string? url, byte[]? data, string? fileName)
        : base("file")
    {
        MediaType = mediaType;
        Url = url;
        Data = data;
        FileName = fileName;
    }

    private FileContentPart(
        string mediaType,
        IReadOnlyDictionary<string, string> providerReference,
        string? fileName,
        IReadOnlyDictionary<string, JsonElement>? providerOptions)
        : this(mediaType, null, null, fileName)
    {
        ProviderReference = providerReference ?? throw new ArgumentNullException(nameof(providerReference));
        ProviderOptions = providerOptions;
    }

    /// <summary>
    /// Creates a file part that refers to an uploaded file. Maps to <c>data: { type: "reference", reference }</c>.
    /// </summary>
    /// <param name="mediaType">IANA media type.</param>
    /// <param name="providerReference">Map from provider name to the provider's file id.</param>
    /// <param name="fileName">Optional file name.</param>
    /// <param name="providerOptions">Provider-specific options for this part.</param>
    public static FileContentPart FromProviderReference(
        string mediaType,
        IReadOnlyDictionary<string, string> providerReference,
        string? fileName = null,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
    {
        return new FileContentPart(mediaType, providerReference, fileName, providerOptions);
    }

    /// <summary>Map from provider name to an uploaded file id, used when there is no URL or inline data.</summary>
    public IReadOnlyDictionary<string, string>? ProviderReference { get; }

    /// <summary>Provider-specific options for this part, such as the xAI <c>imageDetail</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Remote URL, when the file is not inlined.</summary>
    public string? Url { get; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; }
}

/// <summary>A user message.</summary>
public sealed class UserModelMessage : ModelMessage
{
    /// <summary>Creates a user message from text.</summary>
    public UserModelMessage(string text)
        : this(new UserContentPart[] { new TextContentPart(text) })
    {
    }

    /// <summary>Creates a user message from parts.</summary>
    public UserModelMessage(IReadOnlyList<UserContentPart> content)
        : base("user")
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>User content parts.</summary>
    public IReadOnlyList<UserContentPart> Content { get; }

    /// <summary>Provider-specific options for this message, such as an Amazon Bedrock <c>cachePoint</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
}

/// <summary>One part of an assistant message, kept in the order the model produced it.</summary>
public abstract class AssistantContentPart
{
    /// <summary>Creates a part.</summary>
    protected AssistantContentPart(string type, IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        Type = type;
        ProviderOptions = providerOptions;
    }

    /// <summary>Part type: <c>text</c>, <c>file</c>, <c>reasoning</c>, <c>reasoning-file</c>, <c>tool-call</c>, <c>tool-result</c>, or <c>custom</c>.</summary>
    public string Type { get; }

    /// <summary>Provider-specific options for this part, such as an <c>itemId</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; }
}

/// <summary>Text in an assistant message.</summary>
public sealed class AssistantTextPart : AssistantContentPart
{
    /// <summary>Creates a text part.</summary>
    public AssistantTextPart(string text, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("text", providerOptions)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>Reasoning in an assistant message.</summary>
public sealed class AssistantReasoningPart : AssistantContentPart
{
    /// <summary>Creates a reasoning part.</summary>
    public AssistantReasoningPart(string text, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("reasoning", providerOptions)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Reasoning text.</summary>
    public string Text { get; }
}

/// <summary>A file in an assistant message. Maps to the assistant <c>file</c> part.</summary>
public class AssistantFilePart : AssistantContentPart
{
    /// <summary>Creates a file part.</summary>
    /// <param name="mediaType">IANA media type.</param>
    /// <param name="url">Remote URL, when the file is not inlined.</param>
    /// <param name="data">Inline bytes.</param>
    /// <param name="fileName">Optional file name.</param>
    /// <param name="providerOptions">Provider-specific options for this part.</param>
    public AssistantFilePart(
        string mediaType,
        string? url,
        byte[]? data,
        string? fileName = null,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : this("file", mediaType, url, data, fileName, providerOptions)
    {
    }

    private protected AssistantFilePart(
        string type,
        string mediaType,
        string? url,
        byte[]? data,
        string? fileName,
        IReadOnlyDictionary<string, JsonElement>? providerOptions)
        : base(type, providerOptions)
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Url = url;
        Data = data;
        FileName = fileName;
    }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Remote URL, when the file is not inlined.</summary>
    public string? Url { get; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; }
}

/// <summary>A file the model produced as part of its reasoning. Maps to the assistant <c>reasoning-file</c> part.</summary>
public sealed class AssistantReasoningFilePart : AssistantFilePart
{
    /// <summary>Creates a reasoning-file part.</summary>
    /// <param name="mediaType">IANA media type.</param>
    /// <param name="url">Remote URL, when the file is not inlined.</param>
    /// <param name="data">Inline bytes.</param>
    /// <param name="providerOptions">Provider-specific options for this part.</param>
    public AssistantReasoningFilePart(
        string mediaType,
        string? url,
        byte[]? data,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("reasoning-file", mediaType, url, data, null, providerOptions)
    {
    }
}

/// <summary>A tool call in an assistant message.</summary>
public sealed class AssistantToolCallPart : AssistantContentPart
{
    /// <summary>Creates a tool-call part. Without <paramref name="providerOptions"/>, the call's provider metadata is used.</summary>
    public AssistantToolCallPart(GeneratedToolCall toolCall, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("tool-call", providerOptions ?? ToOptions(toolCall))
    {
        ToolCall = toolCall ?? throw new ArgumentNullException(nameof(toolCall));
    }

    /// <summary>The call.</summary>
    public GeneratedToolCall ToolCall { get; }

    private static IReadOnlyDictionary<string, JsonElement>? ToOptions(GeneratedToolCall? call)
    {
        if (call?.ProviderMetadata is not { ValueKind: JsonValueKind.Object } metadata)
        {
            return null;
        }

        var options = new Dictionary<string, JsonElement>();
        foreach (var property in metadata.EnumerateObject())
        {
            options[property.Name] = property.Value.Clone();
        }

        return options;
    }
}

/// <summary>A request to approve a tool call, kept in an assistant message.</summary>
public sealed class AssistantToolApprovalRequestPart : AssistantContentPart
{
    /// <summary>Creates a tool-approval-request part.</summary>
    public AssistantToolApprovalRequestPart(string approvalId, string toolCallId, bool? isAutomatic = null)
        : base("tool-approval-request", null)
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        IsAutomatic = isAutomatic;
    }

    /// <summary>Approval id.</summary>
    public string ApprovalId { get; }

    /// <summary>Id of the tool call that needs approval.</summary>
    public string ToolCallId { get; }

    /// <summary>Whether the approval was decided automatically, when known.</summary>
    public bool? IsAutomatic { get; }
}

/// <summary>An assistant message.</summary>
public sealed class AssistantModelMessage : ModelMessage
{
    /// <summary>Creates an assistant message.</summary>
    public AssistantModelMessage(string? text, IReadOnlyList<GeneratedToolCall>? toolCalls, string? reasoning)
        : this(text, toolCalls, reasoning, null)
    {
    }

    /// <summary>
    /// Creates an assistant message whose reasoning carries provider options, such as an OpenAI
    /// <c>itemId</c> for reasoning stored by the Responses API.
    /// </summary>
    public AssistantModelMessage(
        string? text,
        IReadOnlyList<GeneratedToolCall>? toolCalls,
        string? reasoning,
        IReadOnlyDictionary<string, JsonElement>? reasoningProviderOptions)
        : this(text, toolCalls, reasoning, reasoningProviderOptions, null)
    {
    }

    /// <summary>
    /// Creates an assistant message that also keeps its content parts in order. When <paramref name="parts"/> is null,
    /// the parts are the reasoning, the text, and the tool calls, in that order.
    /// </summary>
    public AssistantModelMessage(
        string? text,
        IReadOnlyList<GeneratedToolCall>? toolCalls,
        string? reasoning,
        IReadOnlyDictionary<string, JsonElement>? reasoningProviderOptions,
        IReadOnlyList<AssistantContentPart>? parts)
        : base("assistant")
    {
        Text = text;
        ToolCalls = toolCalls ?? Array.Empty<GeneratedToolCall>();
        Reasoning = reasoning;
        ReasoningProviderOptions = reasoningProviderOptions;
        Parts = parts ?? DefaultParts(text, ToolCalls, reasoning, reasoningProviderOptions);
    }

    /// <summary>Creates an assistant message from ordered parts. The text, reasoning, and tool calls are derived from them.</summary>
    public AssistantModelMessage(IReadOnlyList<AssistantContentPart> parts)
        : this(
            JoinText(parts ?? throw new ArgumentNullException(nameof(parts)), "text"),
            ToolCallsOf(parts),
            JoinText(parts, "reasoning"),
            FirstReasoningOptions(parts),
            parts)
    {
    }

    /// <summary>
    /// Creates an assistant message from generated content, keeping order and each part's provider metadata as its provider options.
    /// This is how a generate result is replayed as history. Sources and files have no assistant part and are skipped.
    /// </summary>
    public static AssistantModelMessage FromGeneratedContent(IReadOnlyList<GeneratedContent> content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var parts = new List<AssistantContentPart>();
        foreach (var item in content)
        {
            switch (item)
            {
                case GeneratedText text:
                    parts.Add(new AssistantTextPart(text.Text, MetadataToOptions(text.ProviderMetadata)));
                    break;
                case GeneratedReasoning reasoning:
                    parts.Add(new AssistantReasoningPart(reasoning.Text, MetadataToOptions(reasoning.ProviderMetadata)));
                    break;
                case GeneratedToolCall call:
                    parts.Add(new AssistantToolCallPart(call));
                    break;
                case GeneratedToolResult result:
                    parts.Add(new AssistantToolResultPart(
                        result.ToolCallId,
                        result.ToolName,
                        new ToolModelMessage(result.ToolCallId, result.ToolName, result.ResultJson, result.IsError),
                        MetadataToOptions(result.ProviderMetadata)));
                    break;
                case GeneratedCustom custom:
                    parts.Add(new AssistantCustomPart(custom.Kind, MetadataToOptions(custom.ProviderMetadata)));
                    break;
            }
        }

        return new AssistantModelMessage(parts);
    }

    private static IReadOnlyDictionary<string, JsonElement>? MetadataToOptions(JsonElement? metadata)
    {
        if (metadata is not { ValueKind: JsonValueKind.Object } bag)
        {
            return null;
        }

        var options = new Dictionary<string, JsonElement>();
        foreach (var property in bag.EnumerateObject())
        {
            options[property.Name] = property.Value;
        }

        return options;
    }

    /// <summary>Provider-specific options for this message, such as an Amazon Bedrock <c>cachePoint</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }

    /// <summary>Assistant text.</summary>
    public string? Text { get; }

    /// <summary>Tool calls made in this turn.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Reasoning text, when the provider returned it.</summary>
    public string? Reasoning { get; }

    /// <summary>Provider options of the reasoning part, such as an OpenAI <c>itemId</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ReasoningProviderOptions { get; }

    /// <summary>The content parts in the order the model produced them, each with its own provider options.</summary>
    public IReadOnlyList<AssistantContentPart> Parts { get; }

    private static IReadOnlyList<AssistantContentPart> DefaultParts(
        string? text,
        IReadOnlyList<GeneratedToolCall> toolCalls,
        string? reasoning,
        IReadOnlyDictionary<string, JsonElement>? reasoningProviderOptions)
    {
        var parts = new List<AssistantContentPart>();
        if (!string.IsNullOrEmpty(reasoning) || reasoningProviderOptions != null)
        {
            parts.Add(new AssistantReasoningPart(reasoning ?? string.Empty, reasoningProviderOptions));
        }

        if (!string.IsNullOrEmpty(text))
        {
            parts.Add(new AssistantTextPart(text!));
        }

        foreach (var call in toolCalls)
        {
            parts.Add(new AssistantToolCallPart(call));
        }

        return parts;
    }

    private static string? JoinText(IReadOnlyList<AssistantContentPart> parts, string type)
    {
        var builder = new System.Text.StringBuilder();
        var found = false;
        foreach (var part in parts)
        {
            if (part is AssistantTextPart text && type == "text")
            {
                builder.Append(text.Text);
                found = true;
            }
            else if (part is AssistantReasoningPart reasoning && type == "reasoning")
            {
                builder.Append(reasoning.Text);
                found = true;
            }
        }

        return found ? builder.ToString() : null;
    }

    private static IReadOnlyList<GeneratedToolCall> ToolCallsOf(IReadOnlyList<AssistantContentPart> parts)
    {
        var calls = new List<GeneratedToolCall>();
        foreach (var part in parts)
        {
            if (part is AssistantToolCallPart call)
            {
                calls.Add(call.ToolCall);
            }
        }

        return calls;
    }

    private static IReadOnlyDictionary<string, JsonElement>? FirstReasoningOptions(IReadOnlyList<AssistantContentPart> parts)
    {
        foreach (var part in parts)
        {
            if (part is AssistantReasoningPart reasoning && reasoning.ProviderOptions != null)
            {
                return reasoning.ProviderOptions;
            }
        }

        return null;
    }
}

/// <summary>A provider-executed tool result in an assistant message.</summary>
public sealed class AssistantToolResultPart : AssistantContentPart
{
    /// <summary>Creates a tool-result part. <paramref name="output"/> carries the output type and value.</summary>
    public AssistantToolResultPart(
        string toolCallId,
        string toolName,
        ToolModelMessage output,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("tool-result", providerOptions)
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        Output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>The output, as a tool message with the same call id and name. Use its <see cref="ToolModelMessage.OutputType"/> and <see cref="ToolModelMessage.OutputJson"/>.</summary>
    public ToolModelMessage Output { get; }
}

/// <summary>A provider-specific part, identified by <see cref="Kind"/>.</summary>
public sealed class AssistantCustomPart : AssistantContentPart
{
    /// <summary>Creates a custom part.</summary>
    public AssistantCustomPart(string kind, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
        : base("custom", providerOptions)
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
    }

    /// <summary>Provider-scoped kind.</summary>
    public string Kind { get; }
}

/// <summary>
/// The caller's answer to a <see cref="GeneratedToolApprovalRequest"/>. Only a response with
/// <see cref="ProviderExecuted"/> reaches the model; the generation loop drops the others from the prompt.
/// </summary>
public sealed class ToolApprovalResponseModelMessage : ModelMessage
{
    /// <summary>Creates an approval response.</summary>
    public ToolApprovalResponseModelMessage(string approvalId, bool approved, string? reason = null, bool providerExecuted = false)
        : base("tool")
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        Approved = approved;
        Reason = reason;
        ProviderExecuted = providerExecuted;
    }

    /// <summary>Approval id of the request this answers.</summary>
    public string ApprovalId { get; }

    /// <summary>Whether the call was approved.</summary>
    public bool Approved { get; }

    /// <summary>Reason for the decision, when given.</summary>
    public string? Reason { get; }

    /// <summary>True when the provider runs the tool, so the model must see the response.</summary>
    public bool ProviderExecuted { get; }

    /// <summary>
    /// For a denied provider-executed call, the <c>execution-denied</c> result sent in the same tool message, after the response.
    /// The generation loop sets it while it prepares the prompt.
    /// </summary>
    public ToolModelMessage? DeniedToolResult { get; init; }
}

/// <summary>A tool result fed back to the model.</summary>
public sealed class ToolModelMessage : ModelMessage
{
    private static readonly string[] OutputTypes = { "text", "json", "error-text", "error-json", "execution-denied" };

    /// <summary>Creates a tool result message. The output type comes from <paramref name="isError"/> and from whether <paramref name="outputJson"/> is valid JSON.</summary>
    public ToolModelMessage(string toolCallId, string toolName, string outputJson, bool isError, JsonElement? providerMetadata = null)
        : this(toolCallId, toolName, outputJson, isError, null, providerMetadata)
    {
    }

    /// <summary>
    /// Creates a tool result message. <paramref name="outputType"/> is <c>text</c>, <c>json</c>, <c>error-text</c>, <c>error-json</c>,
    /// or <c>execution-denied</c>. When it is null, the type comes from <paramref name="isError"/> and from whether
    /// <paramref name="outputJson"/> is valid JSON. When it is set, it decides <see cref="IsError"/> and <paramref name="isError"/> is ignored.
    /// For <c>execution-denied</c>, <paramref name="outputJson"/> is the reason, or null for none.
    /// </summary>
    public ToolModelMessage(string toolCallId, string toolName, string? outputJson, bool isError, string? outputType, JsonElement? providerMetadata = null)
        : base("tool")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        if (outputType != null && Array.IndexOf(OutputTypes, outputType) < 0)
        {
            throw new ArgumentException("Unknown tool output type '" + outputType + "'.", nameof(outputType));
        }

        if ((outputType == "json" || outputType == "error-json") && !IsJson(outputJson))
        {
            throw new ArgumentException("Output type '" + outputType + "' needs valid JSON.", nameof(outputJson));
        }

        OutputType = outputType ?? (isError ? (IsJson(outputJson) ? "error-json" : "error-text") : (IsJson(outputJson) ? "json" : "text"));
        OutputJson = outputJson ?? (OutputType == "execution-denied" ? string.Empty : "null");
        IsError = OutputType != "text" && OutputType != "json";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>
    /// Creates a tool result whose output is a list of text and file parts, as upstream <c>output: { type: 'content' }</c>.
    /// <see cref="OutputType"/> is <c>json</c> and <see cref="OutputJson"/> is the JSON text of the parts, so a provider that
    /// does not read <see cref="OutputContent"/> sends the stringified parts, as upstream does by default.
    /// </summary>
    public ToolModelMessage(string toolCallId, string toolName, IReadOnlyList<UserContentPart> content, JsonElement? providerMetadata = null)
        : this(toolCallId, toolName, Snapshot(content), providerMetadata, true)
    {
    }

    private ToolModelMessage(string toolCallId, string toolName, UserContentPart[] snapshot, JsonElement? providerMetadata, bool snapshotted)
        : this(toolCallId, toolName, SerializeContent(snapshot), false, "json", providerMetadata)
    {
        OutputContent = snapshot;
    }

    /// <summary>
    /// Creates one tool message that carries several tool results, as upstream does for the calls of one step.
    /// Its single-result members describe the first result.
    /// </summary>
    public ToolModelMessage(IReadOnlyList<ToolModelMessage> results)
        : base("tool")
    {
        if (results is null || results.Count == 0)
        {
            throw new ArgumentException("A tool message needs at least one result.", nameof(results));
        }

        var flat = new List<ToolModelMessage>();
        foreach (var result in results)
        {
            flat.AddRange(result.Results);
        }

        var first = flat[0];
        ToolCallId = first.ToolCallId;
        ToolName = first.ToolName;
        OutputType = first.OutputType;
        OutputJson = first.OutputJson;
        OutputContent = first.OutputContent;
        IsError = first.IsError;
        ProviderMetadata = first.ProviderMetadata;
        OutputProviderOptions = first.OutputProviderOptions;
        ProviderOptions = first.ProviderOptions;
        _results = flat;
    }

    private IReadOnlyList<ToolModelMessage>? _results;

    /// <summary>The tool results this message carries, one per executed call. A single-result message lists itself.</summary>
    public IReadOnlyList<ToolModelMessage> Results => _results ??= new[] { this };

    /// <summary>Splits grouped tool messages into one message per result, in order. Other messages pass through.</summary>
    public static IReadOnlyList<ModelMessage> Flatten(IReadOnlyList<ModelMessage> messages)
    {
        var grouped = false;
        foreach (var message in messages)
        {
            if (message is ToolModelMessage { Results.Count: > 1 })
            {
                grouped = true;
                break;
            }
        }

        if (!grouped)
        {
            return messages;
        }

        var flat = new List<ModelMessage>(messages.Count);
        foreach (var message in messages)
        {
            if (message is ToolModelMessage tool)
            {
                flat.AddRange(tool.Results);
            }
            else
            {
                flat.Add(message);
            }
        }

        return flat;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result. For <c>text</c> and <c>error-text</c> it is the text itself. For <c>execution-denied</c> it is the reason, or empty for none.</summary>
    public string OutputJson { get; }

    /// <summary>Text and file parts of a multi-part output, or null for a single value. <see cref="OutputJson"/> holds the same parts as JSON text.</summary>
    public IReadOnlyList<UserContentPart>? OutputContent { get; }

    /// <summary>Whether the tool failed.</summary>
    public bool IsError { get; }

    /// <summary>Output type: <c>text</c>, <c>json</c>, <c>error-text</c>, <c>error-json</c>, or <c>execution-denied</c>.</summary>
    public string OutputType { get; }

    /// <summary>Provider metadata carried on this tool result, such as a thought signature.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Provider options set on this tool result, such as an Amazon Bedrock <c>cachePoint</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }

    /// <summary>Provider options set on the tool output itself rather than on the tool result, such as an OpenAI <c>promptCacheBreakpoint</c>.</summary>
    public JsonElement? OutputProviderOptions { get; init; }

    /// <summary>Provider options of the whole tool message, such as an Amazon Bedrock <c>cachePoint</c> added after all its results.</summary>
    public IReadOnlyDictionary<string, JsonElement>? MessageProviderOptions { get; init; }

    private static readonly JsonSerializerOptions ContentJsonOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static UserContentPart[] Snapshot(IReadOnlyList<UserContentPart> content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var copy = new UserContentPart[content.Count];
        for (var i = 0; i < copy.Length; i++)
        {
            var part = content[i];
            if (part is FileContentPart file)
            {
                var options = file.ProviderOptions == null ? null : new Dictionary<string, JsonElement>(file.ProviderOptions.ToDictionary(pair => pair.Key, pair => pair.Value));
                copy[i] = file.ProviderReference != null
                    ? FileContentPart.FromProviderReference(file.MediaType, new Dictionary<string, string>(file.ProviderReference.ToDictionary(pair => pair.Key, pair => pair.Value)), file.FileName, options)
                    : new FileContentPart(file.MediaType, file.Url, (byte[]?)file.Data?.Clone(), file.FileName) { ProviderOptions = options };
            }
            else if (part is TextContentPart)
            {
                copy[i] = part;
            }
            else
            {
                throw new ArgumentException("Tool content parts must be text or file parts.", nameof(content));
            }
        }

        return copy;
    }

    private static string SerializeContent(IReadOnlyList<UserContentPart> content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var array = new JsonArray();
        foreach (var part in content)
        {
            if (part is TextContentPart text)
            {
                array.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                var data = new JsonObject();
                if (file.ProviderReference != null)
                {
                    var reference = new JsonObject();
                    foreach (var pair in file.ProviderReference)
                    {
                        reference[pair.Key] = pair.Value;
                    }

                    data["type"] = "reference";
                    data["reference"] = reference;
                }
                else if (file.Data != null)
                {
                    data["type"] = "data";
                    data["data"] = Convert.ToBase64String(file.Data);
                }
                else if (file.Url != null)
                {
                    data["type"] = "url";
                    data["url"] = file.Url;
                }
                else
                {
                    throw new ArgumentException("A file part needs a URL, data, or a provider reference.", nameof(content));
                }

                var json = new JsonObject { ["type"] = "file", ["data"] = data, ["mediaType"] = file.MediaType };
                if (file.FileName != null)
                {
                    json["filename"] = file.FileName;
                }

                array.Add(json);
            }
            else
            {
                throw new ArgumentException("Tool content parts must be text or file parts.", nameof(content));
            }
        }

        return array.ToJsonString(ContentJsonOptions);
    }

    private static bool IsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(value!);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

/// <summary>Settings for one language-model call. Property names follow the V4 call options.</summary>
public sealed class LanguageModelCallOptions
{
    /// <summary>Prompt messages.</summary>
    public IReadOnlyList<ModelMessage> Prompt { get; set; } = Array.Empty<ModelMessage>();

    /// <summary>Maximum tokens to generate. Maps to <c>maxOutputTokens</c>.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Sampling temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Nucleus sampling.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k sampling.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Random seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Asks for a JSON response without a schema. Maps to <c>responseFormat: { type: 'json' }</c> with no schema. Providers send <c>json_object</c> or the matching format. Ignored when <see cref="JsonSchema"/> is set.</summary>
    public bool JsonResponse { get; set; }

    /// <summary>JSON schema when structured output is requested.</summary>
    public JsonElement? JsonSchema { get; set; }

    /// <summary>Schema name sent to providers that require one.</summary>
    public string? JsonSchemaName { get; set; }

    /// <summary>Response format description. Maps to <c>responseFormat.description</c>.</summary>
    public string? JsonSchemaDescription { get; set; }

    /// <summary>Tools the model may call.</summary>
    public IReadOnlyList<LanguageModelTool>? Tools { get; set; }

    /// <summary>Tool selection mode.</summary>
    public ToolChoice? ToolChoice { get; set; }

    /// <summary>Extra HTTP headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options, keyed by provider name.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>
    /// Reasoning effort for providers that accept it: <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c>, <c>high</c>, <c>xhigh</c>, or <c>max</c>.
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>When true, the stream includes one raw provider chunk for each event.</summary>
    public bool IncludeRawChunks { get; set; }

    /// <summary>Copies the options, so a provider can adjust them without changing the caller's object.</summary>
    internal LanguageModelCallOptions Copy()
    {
        return (LanguageModelCallOptions)MemberwiseClone();
    }
}

/// <summary>Result of <see cref="ILanguageModel.DoGenerateAsync"/>.</summary>
public sealed class LanguageModelGenerateResult
{
    /// <summary>Creates a generate result.</summary>
    public LanguageModelGenerateResult(
        IReadOnlyList<GeneratedContent> content,
        FinishReason finishReason,
        LanguageModelUsage usage,
        string? rawFinishReason = null,
        IReadOnlyList<CallWarning>? warnings = null,
        string? responseId = null,
        JsonElement? providerMetadata = null,
        string? rawResponse = null,
        string? responseModelId = null,
        DateTimeOffset? responseTimestamp = null,
        IReadOnlyDictionary<string, string>? responseHeaders = null)
    {
        Content = content ?? Array.Empty<GeneratedContent>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        RawFinishReason = rawFinishReason;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        ResponseId = responseId;
        ProviderMetadata = providerMetadata;
        RawResponse = rawResponse;
        ResponseModelId = responseModelId;
        ResponseTimestamp = responseTimestamp;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
    }

    /// <summary>Generated content parts.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    internal LanguageModelGenerateResult WithContent(IReadOnlyList<GeneratedContent> content)
    {
        return new LanguageModelGenerateResult(
            content,
            FinishReason,
            Usage,
            RawFinishReason,
            Warnings,
            ResponseId,
            ProviderMetadata,
            RawResponse,
            ResponseModelId,
            ResponseTimestamp,
            ResponseHeaders);
    }

    /// <summary>Normalized finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Provider finish reason, when it differs from <see cref="FinishReason"/>.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Call warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Provider response id.</summary>
    public string? ResponseId { get; }

    /// <summary>Provider metadata for this call, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Raw provider response body, when the model captured it.</summary>
    public string? RawResponse { get; }

    /// <summary>Model id reported by the provider.</summary>
    public string? ResponseModelId { get; }

    /// <summary>Provider timestamp for the response.</summary>
    public DateTimeOffset? ResponseTimestamp { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Concatenated text parts.</summary>
    public string Text
    {
        get
        {
            var parts = new List<string>();
            foreach (var part in Content)
            {
                if (part is GeneratedText text)
                {
                    parts.Add(text.Text);
                }
            }

            return string.Concat(parts);
        }
    }
}

/// <summary>One part of a language-model stream.</summary>
public abstract class LanguageModelStreamPart
{
    /// <summary>Creates a stream part.</summary>
    protected LanguageModelStreamPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>
    /// The provider chunk this part was parsed from, with every field the typed part does not model.
    /// It is set only by providers that pass chunks through unchanged, such as the Gateway.
    /// </summary>
    public JsonElement? Chunk { get; internal set; }
}

/// <summary>A text delta.</summary>
public sealed class TextDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a text delta.</summary>
    public TextDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
        : base("text-delta")
    {
        Id = id;
        Delta = delta ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Text block id.</summary>
    public string Id { get; }

    /// <summary>Text fragment.</summary>
    public string Delta { get; }

    /// <summary>Provider metadata for this delta.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A reasoning delta.</summary>
public sealed class ReasoningDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a reasoning delta.</summary>
    public ReasoningDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
        : base("reasoning-delta")
    {
        Id = id;
        Delta = delta ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Reasoning block id.</summary>
    public string Id { get; }

    /// <summary>Reasoning fragment.</summary>
    public string Delta { get; }

    /// <summary>Provider metadata for this fragment, such as a reasoning signature.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A completed tool call.</summary>
public sealed class ToolCallStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-call part.</summary>
    public ToolCallStreamPart(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata = null)
        : base("tool-call")
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        ArgumentsJson = argumentsJson ?? "{}";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }

    /// <summary>Provider metadata for this call.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>True when the provider executed the tool, so the caller has nothing to run.</summary>
    public bool ProviderExecuted { get; init; }

    /// <summary>True when the tool is not known up front, such as a provider-executed MCP tool. Maps to the call's <c>dynamic</c>.</summary>
    public bool Dynamic { get; init; }
}

/// <summary>A request to approve a tool call.</summary>
public sealed class ToolApprovalRequestStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-approval-request part.</summary>
    public ToolApprovalRequestStreamPart(string approvalId, string toolCallId)
        : base("tool-approval-request")
    {
        ApprovalId = approvalId ?? throw new ArgumentNullException(nameof(approvalId));
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
    }

    /// <summary>Approval id the response must carry.</summary>
    public string ApprovalId { get; }

    /// <summary>Id of the tool call that needs approval.</summary>
    public string ToolCallId { get; }
}

/// <summary>Marks the start of streamed tool input.</summary>
public sealed class ToolInputStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-start part.</summary>
    public ToolInputStartStreamPart(
        string id,
        string toolName,
        JsonElement? providerMetadata = null,
        bool? providerExecuted = null,
        bool? dynamic = null,
        string? title = null,
        ToolInputEncoding? inputEncoding = null)
        : base("tool-input-start")
    {
        Id = id ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        InputEncoding = inputEncoding;
        ProviderMetadata = providerMetadata;
        ProviderExecuted = providerExecuted;
        Dynamic = dynamic;
        Title = title;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Provider metadata for this tool input.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>True when the provider executes the tool.</summary>
    public bool? ProviderExecuted { get; }

    /// <summary>True for a tool the caller did not declare.</summary>
    public bool? Dynamic { get; }

    /// <summary>Optional tool title.</summary>
    public string? Title { get; }

    /// <summary>
    /// Encoding of the input deltas. Defaults to JSON when <see langword="null"/>. Text deltas concatenate
    /// to a raw string. The final tool call input must still be JSON-encoded, whatever the delta encoding.
    /// </summary>
    public ToolInputEncoding? InputEncoding { get; }
}

/// <summary>Encoding of the deltas that follow a tool-input-start part.</summary>
public enum ToolInputEncoding
{
    /// <summary>The deltas concatenate to JSON text.</summary>
    Json,

    /// <summary>The deltas concatenate to a raw string, such as OpenAI custom-tool code or HTML.</summary>
    Text,
}

/// <summary>A streamed tool-argument fragment.</summary>
public sealed class ToolInputDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-delta part.</summary>
    public ToolInputDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
        : base("tool-input-delta")
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>JSON fragment.</summary>
    public string Delta { get; }

    /// <summary>Provider metadata for this fragment.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Marks the end of streamed tool input.</summary>
public sealed class ToolInputEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-end part.</summary>
    public ToolInputEndStreamPart(string id, JsonElement? providerMetadata = null)
        : base("tool-input-end")
    {
        Id = id ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Provider metadata for this tool input.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A tool result the provider reported.</summary>
public sealed class ToolResultStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-result part.</summary>
    public ToolResultStreamPart(string toolCallId, string toolName, JsonElement result)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        Result = result;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Result value.</summary>
    public JsonElement Result { get; }

    /// <summary>Whether the tool failed. <see cref="Result"/> then holds the error.</summary>
    public bool IsError { get; init; }

    /// <summary>Provider metadata for this result, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; init; }
}

/// <summary>A cited URL.</summary>
public sealed class SourceStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a source part.</summary>
    public SourceStreamPart(string id, string url, string? title, JsonElement? providerMetadata = null)
        : base("source")
    {
        Id = id;
        Url = url;
        Title = title;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Source id.</summary>
    public string Id { get; }

    /// <summary>Source URL.</summary>
    public string Url { get; }

    /// <summary>Optional title.</summary>
    public string? Title { get; }

    /// <summary>Provider-specific source metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A retrieved document, such as a file a search tool returned.</summary>
public sealed class DocumentSourceStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a document source part.</summary>
    public DocumentSourceStreamPart(string id, string mediaType, string title, string? filename = null, JsonElement? providerMetadata = null)
        : base("source")
    {
        Id = id;
        MediaType = mediaType;
        Title = title;
        Filename = filename;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Source id.</summary>
    public string Id { get; }

    /// <summary>IANA media type of the document.</summary>
    public string MediaType { get; }

    /// <summary>Document title.</summary>
    public string Title { get; }

    /// <summary>Optional file name.</summary>
    public string? Filename { get; }

    /// <summary>Provider-specific source metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>The stream finished.</summary>
public sealed class FinishStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a finish part.</summary>
    public FinishStreamPart(
        FinishReason finishReason,
        LanguageModelUsage usage,
        string? rawFinishReason = null,
        JsonElement? providerMetadata = null)
        : base("finish")
    {
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        RawFinishReason = rawFinishReason;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage reported with the finish part.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider finish reason.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Provider metadata reported with the finish event.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A stream error from the provider.</summary>
public sealed class ErrorStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates an error part.</summary>
    public ErrorStreamPart(string message)
        : base("error")
    {
        Message = message ?? "Unknown provider error.";
    }

    /// <summary>Creates an error part that carries the provider's structured error.</summary>
    public ErrorStreamPart(ProviderStreamError error)
        : base("error")
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        Message = error.Message;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }

    /// <summary>
    /// The provider's structured error (<c>type</c>, <c>code</c>, status, retry flag, and the original <c>data</c>).
    /// Null when the provider reported only a message.
    /// </summary>
    public ProviderStreamError? Error { get; }
}

/// <summary>Marks the start of a stream and carries call warnings.</summary>
public sealed class StreamStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a stream-start part.</summary>
    public StreamStartStreamPart(IReadOnlyList<CallWarning>? warnings)
        : base("stream-start")
    {
        Warnings = warnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>Warnings discovered while preparing the request.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Marks the start of one text block.</summary>
public sealed class TextStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a text-start part.</summary>
    public TextStartStreamPart(string id, JsonElement? providerMetadata = null)
        : base("text-start")
    {
        Id = id ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Text block id.</summary>
    public string Id { get; }

    /// <summary>Provider metadata for this block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Marks the end of one text block.</summary>
public sealed class TextEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a text-end part.</summary>
    public TextEndStreamPart(string id, IReadOnlyList<Citation>? citations = null, JsonElement? providerMetadata = null)
        : base("text-end")
    {
        Id = id ?? string.Empty;
        Citations = citations;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Text block id.</summary>
    public string Id { get; }

    /// <summary>References supporting the text block with this id, or null when there are none.</summary>
    public IReadOnlyList<Citation>? Citations { get; }

    /// <summary>Provider metadata for this block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Marks the start of one reasoning block.</summary>
public sealed class ReasoningStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a reasoning-start part.</summary>
    public ReasoningStartStreamPart(string id, JsonElement? providerMetadata = null)
        : base("reasoning-start")
    {
        Id = id ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Reasoning block id.</summary>
    public string Id { get; }

    /// <summary>Provider metadata for this block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Marks the end of one reasoning block.</summary>
public sealed class ReasoningEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a reasoning-end part.</summary>
    public ReasoningEndStreamPart(string id)
        : this(id, null)
    {
    }

    /// <summary>Creates a reasoning-end part that carries provider metadata.</summary>
    public ReasoningEndStreamPart(string id, JsonElement? providerMetadata)
        : base("reasoning-end")
    {
        Id = id ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Reasoning block id.</summary>
    public string Id { get; }

    /// <summary>Provider metadata for this block, such as encrypted reasoning content.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Identifies the provider response.</summary>
public sealed class ResponseMetadataStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a response-metadata part.</summary>
    public ResponseMetadataStreamPart(string? id, string? modelId, DateTimeOffset? timestamp)
        : base("response-metadata")
    {
        Id = id;
        ModelId = modelId;
        Timestamp = timestamp;
    }

    /// <summary>Provider response id.</summary>
    public string? Id { get; }

    /// <summary>Provider model id.</summary>
    public string? ModelId { get; }

    /// <summary>Provider timestamp.</summary>
    public DateTimeOffset? Timestamp { get; }
}

/// <summary>One raw provider stream event.</summary>
public sealed class RawStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a raw part.</summary>
    public RawStreamPart(string rawJson)
        : base("raw")
    {
        RawJson = rawJson ?? string.Empty;
    }

    /// <summary>Raw event JSON.</summary>
    public string RawJson { get; }
}

/// <summary>A provider-specific part, identified by <see cref="Kind"/>.</summary>
public sealed class CustomStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a custom part.</summary>
    public CustomStreamPart(string kind, JsonElement? providerMetadata = null)
        : base("custom")
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider-scoped kind, such as <c>anthropic.message_start</c>.</summary>
    public string Kind { get; }

    /// <summary>Provider metadata for this part, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>
/// Language model specification V4. <c>doGenerate</c> is <see cref="DoGenerateAsync"/> and
/// <c>doStream</c> is <see cref="DoStreamAsync"/>.
/// </summary>
public interface ILanguageModel
{
    /// <summary>Always <c>V4</c>.</summary>
    string SpecificationVersion { get; }

    /// <summary>Provider id, such as <c>openai</c>.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Generates a complete result.</summary>
    Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken);

    /// <summary>Streams partial results. The result carries the stream with the request and response info.</summary>
    Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken);
}

/// <summary>Request info for a stream call. Maps to the <c>request</c> field of <c>doStream</c>.</summary>
public sealed class LanguageModelStreamRequest
{
    /// <summary>Creates request info.</summary>
    public LanguageModelStreamRequest(JsonElement? body = null)
    {
        Body = body;
    }

    /// <summary>Creates request info from the JSON text that was sent.</summary>
    public static LanguageModelStreamRequest FromJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new LanguageModelStreamRequest(document.RootElement.Clone());
    }

    /// <summary>Request body sent to the provider, for debugging.</summary>
    public JsonElement? Body { get; }
}

/// <summary>Response info for a stream call. Maps to the <c>response</c> field of <c>doStream</c>.</summary>
public sealed class LanguageModelStreamResponse
{
    /// <summary>Creates response info.</summary>
    public LanguageModelStreamResponse(IReadOnlyDictionary<string, string>? headers = null)
    {
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>
/// What <see cref="ILanguageModel.DoStreamAsync"/> returns: the stream plus the request and response info.
/// Maps to the <c>{ stream, request, response }</c> object returned by <c>doStream</c>.
/// Enumerating the result enumerates <see cref="Stream"/>.
/// A provider opens the HTTP response in <c>DoStreamAsync</c>. Dispose the result to release that response when you stop without enumerating the stream, or while you enumerate it.
/// Disposing is safe to repeat, and it does nothing after the stream has ended.
/// </summary>
public sealed class LanguageModelStreamResult : IAsyncEnumerable<LanguageModelStreamPart>, IAsyncDisposable, IDisposable
{
    private IDisposable? _resource;

    /// <summary>Creates a stream result.</summary>
    /// <param name="stream">The stream parts.</param>
    /// <param name="request">Request info.</param>
    /// <param name="response">Response info.</param>
    /// <param name="resource">A resource the provider opened for the stream, such as the HTTP response. The result disposes it once.</param>
    public LanguageModelStreamResult(
        IAsyncEnumerable<LanguageModelStreamPart> stream,
        LanguageModelStreamRequest? request = null,
        LanguageModelStreamResponse? response = null,
        IDisposable? resource = null)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        Request = request;
        Response = response;
        _resource = resource;
    }

    /// <summary>The stream parts.</summary>
    public IAsyncEnumerable<LanguageModelStreamPart> Stream { get; }

    /// <summary>Request info, when the model reports it.</summary>
    public LanguageModelStreamRequest? Request { get; }

    /// <summary>Response info, when the model reports it.</summary>
    public LanguageModelStreamResponse? Response { get; }

    /// <inheritdoc />
    public IAsyncEnumerator<LanguageModelStreamPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return Stream.GetAsyncEnumerator(cancellationToken);
    }

    /// <summary>
    /// Configures how awaits on the stream parts run. This instance method keeps <c>await foreach (var part in result.ConfigureAwait(false))</c>
    /// unambiguous, because the result is both an async enumerable and an async disposable.
    /// </summary>
    public System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable<LanguageModelStreamPart> ConfigureAwait(bool continueOnCapturedContext)
    {
        return Stream.ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary>Releases the resource the provider opened for this stream. Safe to call more than once.</summary>
    public void Dispose()
    {
        Interlocked.Exchange(ref _resource, null)?.Dispose();
    }

    /// <summary>Releases the resource the provider opened for this stream. Safe to call more than once.</summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }
}

/// <summary>Maps provider finish-reason strings onto <see cref="FinishReason"/>.</summary>
public static class FinishReasons
{
    /// <summary>Maps a provider finish reason.</summary>
    public static FinishReason Parse(string? raw)
    {
        if (raw == null || raw.Length == 0)
        {
            return FinishReason.Other;
        }

        switch (raw.ToUpperInvariant())
        {
            case "STOP":
            case "END_TURN":
            case "STOP_SEQUENCE":
            case "COMPLETE":
            case "COMPLETED":
                return FinishReason.Stop;
            case "LENGTH":
            case "MAX_TOKENS":
            case "MAX_OUTPUT_TOKENS":
                return FinishReason.Length;
            case "CONTENT_FILTER":
            case "CONTENT-FILTER":
            case "SAFETY":
            case "RECITATION":
                return FinishReason.ContentFilter;
            case "TOOL_CALLS":
            case "TOOL-CALLS":
            case "TOOL_USE":
            case "FUNCTION_CALL":
                return FinishReason.ToolCalls;
            case "ERROR":
                return FinishReason.Error;
            default:
                return FinishReason.Other;
        }
    }
}
