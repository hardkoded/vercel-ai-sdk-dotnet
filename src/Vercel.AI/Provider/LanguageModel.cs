// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

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
    public LanguageModelUsage(int? inputTokens, int? outputTokens, int? totalTokens)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens ?? ((inputTokens ?? 0) + (outputTokens ?? 0));
    }

    /// <summary>Prompt tokens.</summary>
    public int? InputTokens { get; }

    /// <summary>Generated tokens.</summary>
    public int? OutputTokens { get; }

    /// <summary>Input plus output tokens.</summary>
    public int? TotalTokens { get; }

    /// <summary>Adds two usage values.</summary>
    public static LanguageModelUsage Add(LanguageModelUsage left, LanguageModelUsage right)
    {
        return new LanguageModelUsage(
            Sum(left.InputTokens, right.InputTokens),
            Sum(left.OutputTokens, right.OutputTokens),
            Sum(left.TotalTokens, right.TotalTokens));
    }

    /// <summary>Zero usage.</summary>
    public static LanguageModelUsage Empty { get; } = new(0, 0, 0);

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

    /// <summary>Warning category, such as <c>unsupported</c>.</summary>
    public string Type { get; }

    /// <summary>Human-readable warning.</summary>
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
public sealed class GeneratedText : GeneratedContent
{
    /// <summary>Creates text content.</summary>
    public GeneratedText(string text)
        : base("text")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Generated text.</summary>
    public string Text { get; }
}

/// <summary>A function tool call.</summary>
public sealed class GeneratedToolCall : GeneratedContent
{
    /// <summary>Creates a tool call.</summary>
    public GeneratedToolCall(string toolCallId, string toolName, string argumentsJson)
        : base("tool-call")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        ArgumentsJson = argumentsJson ?? "{}";
    }

    /// <summary>Provider id for this call.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON object arguments.</summary>
    public string ArgumentsJson { get; }
}

/// <summary>Reasoning text kept separate from the answer.</summary>
public sealed class GeneratedReasoning : GeneratedContent
{
    /// <summary>Creates reasoning content.</summary>
    public GeneratedReasoning(string text)
        : base("reasoning")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Reasoning text.</summary>
    public string Text { get; }
}

/// <summary>A URL source cited by the model.</summary>
public sealed class GeneratedSource : GeneratedContent
{
    /// <summary>Creates a source.</summary>
    public GeneratedSource(string id, string url, string? title)
        : base("source")
    {
        Id = id;
        Url = url;
        Title = title;
    }

    /// <summary>Source id.</summary>
    public string Id { get; }

    /// <summary>Source URL.</summary>
    public string Url { get; }

    /// <summary>Optional title.</summary>
    public string? Title { get; }
}

/// <summary>A generated file.</summary>
public sealed class GeneratedFile : GeneratedContent
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
    public LanguageModelTool(string name, string? description, JsonElement inputSchema)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        InputSchema = inputSchema;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Tool description.</summary>
    public string? Description { get; }

    /// <summary>JSON Schema for the arguments.</summary>
    public JsonElement InputSchema { get; }
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
        : base("system")
    {
        Content = content ?? string.Empty;
    }

    /// <summary>Instruction text.</summary>
    public string Content { get; }
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
}

/// <summary>An assistant message.</summary>
public sealed class AssistantModelMessage : ModelMessage
{
    /// <summary>Creates an assistant message.</summary>
    public AssistantModelMessage(string? text, IReadOnlyList<GeneratedToolCall>? toolCalls, string? reasoning)
        : base("assistant")
    {
        Text = text;
        ToolCalls = toolCalls ?? Array.Empty<GeneratedToolCall>();
        Reasoning = reasoning;
    }

    /// <summary>Assistant text.</summary>
    public string? Text { get; }

    /// <summary>Tool calls made in this turn.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Reasoning text, when the provider returned it.</summary>
    public string? Reasoning { get; }
}

/// <summary>A tool result fed back to the model.</summary>
public sealed class ToolModelMessage : ModelMessage
{
    /// <summary>Creates a tool result message.</summary>
    public ToolModelMessage(string toolCallId, string toolName, string outputJson, bool isError)
        : base("tool")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        OutputJson = outputJson ?? "null";
        IsError = isError;
    }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result.</summary>
    public string OutputJson { get; }

    /// <summary>Whether the tool failed.</summary>
    public bool IsError { get; }
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

    /// <summary>JSON schema when structured output is requested.</summary>
    public JsonElement? JsonSchema { get; set; }

    /// <summary>Schema name sent to providers that require one.</summary>
    public string? JsonSchemaName { get; set; }

    /// <summary>Tools the model may call.</summary>
    public IReadOnlyList<LanguageModelTool>? Tools { get; set; }

    /// <summary>Tool selection mode.</summary>
    public ToolChoice? ToolChoice { get; set; }

    /// <summary>Extra HTTP headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options, keyed by provider name.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
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
        string? responseId = null)
    {
        Content = content ?? Array.Empty<GeneratedContent>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        RawFinishReason = rawFinishReason;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        ResponseId = responseId;
    }

    /// <summary>Generated content parts.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

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
}

/// <summary>A text delta.</summary>
public sealed class TextDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a text delta.</summary>
    public TextDeltaStreamPart(string id, string delta)
        : base("text-delta")
    {
        Id = id;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Text block id.</summary>
    public string Id { get; }

    /// <summary>Text fragment.</summary>
    public string Delta { get; }
}

/// <summary>A reasoning delta.</summary>
public sealed class ReasoningDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a reasoning delta.</summary>
    public ReasoningDeltaStreamPart(string id, string delta)
        : base("reasoning-delta")
    {
        Id = id;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Reasoning block id.</summary>
    public string Id { get; }

    /// <summary>Reasoning fragment.</summary>
    public string Delta { get; }
}

/// <summary>A completed tool call.</summary>
public sealed class ToolCallStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-call part.</summary>
    public ToolCallStreamPart(string toolCallId, string toolName, string argumentsJson)
        : base("tool-call")
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        ArgumentsJson = argumentsJson ?? "{}";
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }
}

/// <summary>A cited URL.</summary>
public sealed class SourceStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a source part.</summary>
    public SourceStreamPart(string id, string url, string? title)
        : base("source")
    {
        Id = id;
        Url = url;
        Title = title;
    }

    /// <summary>Source id.</summary>
    public string Id { get; }

    /// <summary>Source URL.</summary>
    public string Url { get; }

    /// <summary>Optional title.</summary>
    public string? Title { get; }
}

/// <summary>The stream finished.</summary>
public sealed class FinishStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a finish part.</summary>
    public FinishStreamPart(FinishReason finishReason, LanguageModelUsage usage, string? rawFinishReason = null)
        : base("finish")
    {
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        RawFinishReason = rawFinishReason;
    }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage reported with the finish part.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider finish reason.</summary>
    public string? RawFinishReason { get; }
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

    /// <summary>Error message.</summary>
    public string Message { get; }
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

    /// <summary>Streams partial results.</summary>
    IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken);
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
