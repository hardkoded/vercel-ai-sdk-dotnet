// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>A message in the <c>gen_ai.input.messages</c> SemConv format. Maps to <c>SemConvInputMessage</c>.</summary>
public sealed class SemConvInputMessage
{
    /// <summary>Creates a message.</summary>
    public SemConvInputMessage(string role, IReadOnlyList<JsonObject> parts)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>Message parts. Maps to <c>SemConvPart</c>.</summary>
    public IReadOnlyList<JsonObject> Parts { get; }

    /// <summary>The message as JSON, with properties in the order upstream writes them.</summary>
    public JsonObject ToJsonObject() =>
        new()
        {
            ["role"] = Role,
            ["parts"] = new JsonArray(Parts.Select(p => p.DeepClone()).ToArray()),
        };
}

/// <summary>A message in the <c>gen_ai.output.messages</c> SemConv format. Maps to <c>SemConvOutputMessage</c>.</summary>
public sealed class SemConvOutputMessage
{
    /// <summary>Creates a message.</summary>
    public SemConvOutputMessage(string role, IReadOnlyList<JsonObject> parts, string finishReason)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
        FinishReason = finishReason ?? throw new ArgumentNullException(nameof(finishReason));
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>Message parts. Maps to <c>SemConvPart</c>.</summary>
    public IReadOnlyList<JsonObject> Parts { get; }

    /// <summary>Finish reason in the SemConv vocabulary. Maps to <c>finish_reason</c>.</summary>
    public string FinishReason { get; }

    /// <summary>The message as JSON, with properties in the order upstream writes them.</summary>
    public JsonObject ToJsonObject() =>
        new()
        {
            ["role"] = Role,
            ["parts"] = new JsonArray(Parts.Select(p => p.DeepClone()).ToArray()),
            ["finish_reason"] = FinishReason,
        };
}

/// <summary>A part of <c>gen_ai.system_instructions</c>. Maps to <c>SemConvSystemInstruction</c>.</summary>
public sealed class SemConvSystemInstruction
{
    /// <summary>Creates a system instruction part.</summary>
    public SemConvSystemInstruction(string type, string? content)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Content = content;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Part content.</summary>
    public string? Content { get; }

    /// <summary>The part as JSON.</summary>
    public JsonObject ToJsonObject()
    {
        var result = new JsonObject { ["type"] = Type };
        if (Content is not null)
        {
            result["content"] = Content;
        }

        return result;
    }
}

public static partial class GenAiConventions
{
    private static readonly JsonSerializerOptions SemConvJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Serializes SemConv messages the way <c>JSON.stringify</c> does, without escaping non-ASCII text.</summary>
    /// <param name="messages">The SemConv values, as JSON.</param>
    internal static string StringifySemConv(IEnumerable<JsonObject> messages) =>
        // A node has one parent, so the array gets copies and the caller can serialize the same values again.
        new JsonArray(messages.Select(m => m.DeepClone()).ToArray()).ToJsonString(
            SemConvJsonOptions
        );

    /// <summary>Serializes a JSON value the way <c>JSON.stringify</c> does, without escaping non-ASCII text.</summary>
    /// <param name="node">The value.</param>
    internal static string StringifySemConv(JsonNode node) => node.ToJsonString(SemConvJsonOptions);

    /// <summary>Converts <paramref name="instructions"/> to the <c>gen_ai.system_instructions</c> SemConv format. Maps to <c>formatSystemInstructions</c>.</summary>
    /// <param name="instructions">A string, one system message, or a list of them.</param>
    public static IReadOnlyList<SemConvSystemInstruction> FormatSystemInstructions(
        Instructions instructions
    )
    {
        if (instructions is null)
        {
            throw new ArgumentNullException(nameof(instructions));
        }

        if (instructions.Text is { } text)
        {
            return [new SemConvSystemInstruction("text", text)];
        }

        return (instructions.Messages ?? [])
            .Select(message => new SemConvSystemInstruction("text", message.Content))
            .ToArray();
    }

    /// <summary>Converts a prompt to the <c>gen_ai.input.messages</c> SemConv format, keeping the order the messages were sent in. Maps to <c>formatInputMessages</c>.</summary>
    /// <param name="prompt">The prompt.</param>
    public static IReadOnlyList<SemConvInputMessage> FormatInputMessages(
        IReadOnlyList<ModelMessage> prompt
    )
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        return ConvertMessages(prompt);
    }

    /// <summary>
    /// Converts a prompt string and messages to the <c>gen_ai.input.messages</c> SemConv format, keeping message order.
    /// Maps to <c>formatModelMessages</c>.
    /// </summary>
    /// <param name="prompt">A prompt string, which becomes a user message.</param>
    /// <param name="messages">The messages.</param>
    public static IReadOnlyList<SemConvInputMessage> FormatModelMessages(
        string? prompt,
        IReadOnlyList<ModelMessage>? messages
    ) => FormatModelMessages(prompt, null, messages);

    /// <summary>
    /// Converts a prompt, given as a string or as messages, and messages to the <c>gen_ai.input.messages</c> SemConv format,
    /// keeping message order. Maps to <c>formatModelMessages</c>.
    /// </summary>
    /// <param name="prompt">A prompt string, which becomes a user message.</param>
    /// <param name="promptMessages">A prompt given as messages. Used when <paramref name="prompt"/> is null.</param>
    /// <param name="messages">The messages, after the prompt.</param>
    public static IReadOnlyList<SemConvInputMessage> FormatModelMessages(
        string? prompt,
        IReadOnlyList<ModelMessage>? promptMessages,
        IReadOnlyList<ModelMessage>? messages
    )
    {
        var result = new List<SemConvInputMessage>();
        if (prompt is not null)
        {
            result.Add(new SemConvInputMessage("user", [TextPart(prompt)]));
        }
        else if (promptMessages is not null)
        {
            result.AddRange(ConvertMessages(promptMessages));
        }

        if (messages is not null)
        {
            result.AddRange(ConvertMessages(messages));
        }

        return result;
    }

    /// <summary>Returns the first system message of a prompt, or null when it has none. Maps to <c>extractSystemFromPrompt</c>.</summary>
    /// <param name="prompt">The prompt.</param>
    public static string? ExtractSystemFromPrompt(IReadOnlyList<ModelMessage> prompt)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        return prompt.OfType<SystemModelMessage>().FirstOrDefault()?.Content;
    }

    /// <summary>
    /// Converts the content a model call or step produced to the <c>gen_ai.output.messages</c> SemConv format, keeping the order the
    /// parts were produced in. Maps to <c>formatOutputContent</c>.
    /// </summary>
    /// <param name="content">The generated content.</param>
    /// <param name="finishReason">The finish reason, in the <c>FormatFinishReason</c> vocabulary, such as <c>tool-calls</c>.</param>
    public static IReadOnlyList<SemConvOutputMessage> FormatOutputContent(
        IReadOnlyList<GeneratedContent> content,
        string finishReason
    )
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var parts = new List<JsonObject>();
        foreach (var part in content)
        {
            switch (part)
            {
                case GeneratedText text:
                    // Skip empty text without provider metadata, the same as response messages.
                    if (text.Text.Length > 0 || text.ProviderMetadata is not null)
                    {
                        parts.Add(TextPart(text.Text));
                    }

                    break;
                case GeneratedReasoning reasoning:
                    parts.Add(ReasoningPart(reasoning.Text));
                    break;
                case GeneratedFile file:
                    parts.Add(
                        ConvertFileData(Convert.ToBase64String(file.Data), null, file.MediaType)
                    );
                    break;
                case GeneratedFileUrl fileUrl:
                    parts.Add(ConvertFileData(null, fileUrl.Url.ToString(), fileUrl.MediaType));
                    break;
                case GeneratedReasoningFile reasoningFile:
                    parts.Add(
                        ConvertFileData(
                            Convert.ToBase64String(reasoningFile.Data),
                            null,
                            reasoningFile.MediaType
                        )
                    );
                    break;
                case GeneratedCustom custom:
                    parts.Add(CustomPart(custom.Kind));
                    break;
                case GeneratedToolCall call:
                    parts.Add(ToolCallPart(call.ToolCallId, call.ToolName, call.ArgumentsJson));
                    break;
                case GeneratedToolResult { IsError: true } failed:
                    parts.Add(
                        ToolCallResponsePart(
                            failed.ToolCallId,
                            failed.ProviderExecuted
                                ? ParseJson(failed.ResultJson)
                                : (JsonNode?)(
                                    failed.Error is { } error
                                        ? ErrorMessage.ToErrorString(error)
                                        : ErrorMessage.GetErrorMessage(failed.ResultJson)
                                )
                        )
                    );
                    break;
                case GeneratedToolResult result:
                    parts.Add(
                        ToolCallResponsePart(result.ToolCallId, ParseJson(result.ResultJson))
                    );
                    break;
                case GeneratedToolApprovalRequest request:
                    parts.Add(
                        ToolApprovalRequestPart(
                            request.ApprovalId,
                            request.ToolCall?.ToolCallId ?? request.ToolCallId,
                            request.Reason
                        )
                    );
                    break;
                case GeneratedToolApprovalResponse response:
                    parts.Add(
                        ToolApprovalResponsePart(
                            response.ApprovalId,
                            response.Approved,
                            response.Reason
                        )
                    );
                    break;
                case GeneratedSource:
                case GeneratedDocumentSource:
                    // Sources are response metadata, not message parts.
                    break;
                default:
                    parts.Add(new JsonObject { ["type"] = part.Type });
                    break;
            }
        }

        return [new SemConvOutputMessage("assistant", parts, MapFinishReason(finishReason))];
    }

    /// <summary>
    /// Converts step result data to the <c>gen_ai.output.messages</c> SemConv format. Maps to <c>formatOutputMessages</c>.
    /// </summary>
    /// <remarks>Use <see cref="FormatOutputContent"/> instead. This method groups parts by type and does not keep the order they were produced in.</remarks>
    /// <param name="finishReason">The finish reason, such as <c>tool-calls</c>.</param>
    /// <param name="text">The text.</param>
    /// <param name="reasoning">The reasoning texts.</param>
    /// <param name="toolCalls">The tool calls.</param>
    /// <param name="toolResults">The tool results.</param>
    /// <param name="files">The files.</param>
    [Obsolete(
        "Use FormatOutputContent instead. This method groups parts by type and does not keep the order they were produced in."
    )]
    public static IReadOnlyList<SemConvOutputMessage> FormatOutputMessages(
        string finishReason,
        string? text = null,
        IReadOnlyList<string?>? reasoning = null,
        IReadOnlyList<GeneratedToolCall>? toolCalls = null,
        IReadOnlyList<GeneratedToolResult>? toolResults = null,
        IReadOnlyList<GeneratedFile>? files = null
    )
    {
        var parts = new List<JsonObject>();
        if (reasoning is not null)
        {
            foreach (var item in reasoning)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    parts.Add(ReasoningPart(item!));
                }
            }
        }

        if (!string.IsNullOrEmpty(text))
        {
            parts.Add(TextPart(text!));
        }

        foreach (var call in toolCalls ?? [])
        {
            parts.Add(ToolCallPart(call.ToolCallId, call.ToolName, call.ArgumentsJson));
        }

        foreach (var result in toolResults ?? [])
        {
            parts.Add(ToolCallResponsePart(result.ToolCallId, ParseJson(result.ResultJson)));
        }

        foreach (var file in files ?? [])
        {
            parts.Add(
                new JsonObject
                {
                    ["type"] = "blob",
                    ["modality"] = GetModality(file.MediaType),
                    ["mime_type"] = file.MediaType,
                    ["content"] = Convert.ToBase64String(file.Data),
                }
            );
        }

        return [new SemConvOutputMessage("assistant", parts, MapFinishReason(finishReason))];
    }

    /// <summary>Converts a <c>generateObject</c> result to the <c>gen_ai.output.messages</c> SemConv format. Maps to <c>formatObjectOutputMessages</c>.</summary>
    /// <param name="objectText">The JSON text of the object.</param>
    /// <param name="finishReason">The finish reason, such as <c>stop</c>.</param>
    public static IReadOnlyList<SemConvOutputMessage> FormatObjectOutputMessages(
        string objectText,
        string finishReason
    ) =>
        [
            new SemConvOutputMessage(
                "assistant",
                [TextPart(objectText)],
                MapFinishReason(finishReason)
            ),
        ];

    private static string MapFinishReason(string reason) =>
        reason switch
        {
            "stop" => "stop",
            "length" => "length",
            "content-filter" => "content_filter",
            "tool-calls" => "tool_call",
            "error" => "error",
            "other" => "stop",
            "unknown" => "stop",
            _ => reason,
        };

    private static string GetModality(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return "image";
        }

        if (mediaType!.StartsWith("video/", StringComparison.Ordinal))
        {
            return "video";
        }

        return mediaType.StartsWith("audio/", StringComparison.Ordinal) ? "audio" : "image";
    }

    // Parses JSON text. Text that is not JSON stays a string, as a tool result of type text does.
    private static JsonNode? ParseJson(string? json)
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return JsonValue.Create(json);
        }
    }

    private static JsonObject TextPart(string content) =>
        new() { ["type"] = "text", ["content"] = content };

    private static JsonObject ReasoningPart(string content) =>
        new() { ["type"] = "reasoning", ["content"] = content };

    private static JsonObject CustomPart(string kind) =>
        new() { ["type"] = "custom", ["kind"] = kind };

    private static JsonObject ToolCallPart(string id, string name, string argumentsJson) =>
        new()
        {
            ["type"] = "tool_call",
            ["id"] = id,
            ["name"] = name,
            ["arguments"] = ParseJson(argumentsJson),
        };

    private static JsonObject ToolCallResponsePart(string id, JsonNode? response) =>
        new()
        {
            ["type"] = "tool_call_response",
            ["id"] = id,
            ["response"] = response,
        };

    private static JsonObject ToolApprovalRequestPart(
        string approvalId,
        string toolCallId,
        string? reason
    )
    {
        var part = new JsonObject
        {
            ["type"] = "tool_approval_request",
            ["approval_id"] = approvalId,
            ["tool_call_id"] = toolCallId,
        };
        if (reason is not null)
        {
            part["reason"] = reason;
        }

        return part;
    }

    private static JsonObject ToolApprovalResponsePart(
        string approvalId,
        bool approved,
        string? reason
    )
    {
        var part = new JsonObject
        {
            ["type"] = "tool_approval_response",
            ["approval_id"] = approvalId,
            ["approved"] = approved,
        };
        if (reason is not null)
        {
            part["reason"] = reason;
        }

        return part;
    }

    // Maps to convertFileDataToSemConv. A reference has no data to show, so it becomes an empty blob like upstream.
    private static JsonObject ConvertFileData(string? base64, string? url, string? mediaType)
    {
        if (
            url is not null
            || (
                base64 is not null
                && (
                    base64.StartsWith("http://", StringComparison.Ordinal)
                    || base64.StartsWith("https://", StringComparison.Ordinal)
                )
            )
        )
        {
            return new JsonObject
            {
                ["type"] = "uri",
                ["modality"] = GetModality(mediaType),
                ["mime_type"] = mediaType,
                ["uri"] = url ?? base64,
            };
        }

        return new JsonObject
        {
            ["type"] = "blob",
            ["modality"] = GetModality(mediaType),
            ["mime_type"] = mediaType,
            ["content"] = base64 ?? string.Empty,
        };
    }

    // Maps to convertToolResultOutputToResponse.
    private static JsonNode? ConvertToolResultOutputToResponse(ToolModelMessage output)
    {
        if (output.OutputContent is not null)
        {
            return new JsonObject
            {
                ["type"] = "content",
                ["value"] = ParseJson(output.OutputJson),
            };
        }

        switch (output.OutputType)
        {
            case "text":
            case "error-text":
                return output.OutputJson;
            case "json":
            case "error-json":
                return ParseJson(output.OutputJson);
            case "execution-denied":
                var denied = new JsonObject { ["denied"] = true };
                if (output.ExecutionDeniedReason is { } reason)
                {
                    denied["reason"] = reason;
                }

                return denied;
            default:
                return ParseJson(output.OutputJson);
        }
    }

    private static JsonObject ConvertUserPart(UserContentPart part) =>
        part switch
        {
            TextContentPart text => TextPart(text.Text),
            FileContentPart file => ConvertFileData(
                file.Data is { } data ? Convert.ToBase64String(data) : file.Text,
                file.Url,
                file.MediaType
            ),
            _ => new JsonObject { ["type"] = part.Type },
        };

    private static JsonObject ConvertAssistantPart(AssistantContentPart part) =>
        part switch
        {
            AssistantTextPart text => TextPart(text.Text),
            AssistantReasoningPart reasoning => ReasoningPart(reasoning.Text),
            AssistantFilePart file => ConvertFileData(
                file.Data is { } data ? Convert.ToBase64String(data) : null,
                file.Url,
                file.MediaType
            ),
            AssistantCustomPart custom => CustomPart(custom.Kind),
            AssistantToolCallPart call => ToolCallPart(
                call.ToolCall.ToolCallId,
                call.ToolCall.ToolName,
                call.ToolCall.ArgumentsJson
            ),
            AssistantToolResultPart result => ToolCallResponsePart(
                result.ToolCallId,
                ConvertToolResultOutputToResponse(result.Output)
            ),
            AssistantToolApprovalRequestPart request => ToolApprovalRequestPart(
                request.ApprovalId,
                request.ToolCallId,
                request.Reason
            ),
            _ => new JsonObject { ["type"] = part.Type },
        };

    // Maps to convertModelMessageToSemConv. Consecutive tool messages form one message, as upstream's tool message holds many parts.
    private static IReadOnlyList<SemConvInputMessage> ConvertMessages(
        IReadOnlyList<ModelMessage> messages
    )
    {
        var result = new List<SemConvInputMessage>();
        List<JsonObject>? toolParts = null;
        foreach (var message in ToolModelMessage.Flatten(messages))
        {
            if (message is ToolModelMessage or ToolApprovalResponseModelMessage)
            {
                if (toolParts is null)
                {
                    toolParts = [];
                    result.Add(new SemConvInputMessage("tool", toolParts));
                }

                if (message is ToolModelMessage toolMessage)
                {
                    toolParts.Add(
                        ToolCallResponsePart(
                            toolMessage.ToolCallId,
                            ConvertToolResultOutputToResponse(toolMessage)
                        )
                    );
                }
                else if (message is ToolApprovalResponseModelMessage approval)
                {
                    toolParts.Add(
                        ToolApprovalResponsePart(
                            approval.ApprovalId,
                            approval.Approved,
                            approval.Reason
                        )
                    );
                    if (approval.DeniedToolResult is { } denied)
                    {
                        toolParts.Add(
                            ToolCallResponsePart(
                                denied.ToolCallId,
                                ConvertToolResultOutputToResponse(denied)
                            )
                        );
                    }
                }

                continue;
            }

            toolParts = null;
            result.Add(ConvertMessage(message));
        }

        return result;
    }

    private static SemConvInputMessage ConvertMessage(ModelMessage message)
    {
        switch (message)
        {
            case SystemModelMessage system:
                return new SemConvInputMessage("system", [TextPart(system.Content)]);
            case UserModelMessage user:
                return new SemConvInputMessage(
                    "user",
                    user.Content.Select(ConvertUserPart).ToArray()
                );
            case AssistantModelMessage assistant:
                return new SemConvInputMessage(
                    "assistant",
                    assistant.Parts.Select(ConvertAssistantPart).ToArray()
                );
            default:
                return new SemConvInputMessage(message.Role, []);
        }
    }

    /// <summary>Redacts and converts a tool offered to the model for <c>gen_ai.tool.definitions</c>.</summary>
    internal static JsonObject ToToolDefinition(LanguageModelTool tool)
    {
        if (tool.IsProviderDefined)
        {
            var definition = new JsonObject
            {
                ["type"] = "provider",
                ["id"] = tool.ProviderToolId,
                ["name"] = tool.Name,
            };
            if (tool.RedactTelemetryArgs() is { } args)
            {
                definition["args"] = JsonNode.Parse(args.GetRawText());
            }

            return ToolDefinitionRedaction.RedactToolDefinition(definition);
        }

        var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
        if (tool.Description is not null)
        {
            function["description"] = tool.Description;
        }

        if (tool.InputSchema.ValueKind != JsonValueKind.Undefined)
        {
            function["inputSchema"] = JsonNode.Parse(tool.InputSchema.GetRawText());
        }

        if (tool.Strict is { } strict)
        {
            function["strict"] = strict;
        }

        return function;
    }
}
