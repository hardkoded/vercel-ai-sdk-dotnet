// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Converts a prompt to Mistral chat messages. Maps to <c>convert-to-mistral-chat-messages.ts</c>.</summary>
internal static class ConvertToMistralChatMessages
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Returns the tool call arguments as JSON text: valid JSON is kept as it is and any other text is sent as a JSON string.
    /// Maps to <c>serializeMistralToolCallArguments</c>.
    /// </summary>
    public static string SerializeMistralToolCallArguments(string input)
    {
        try
        {
            SecureJson.Parse(input);
            return input;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(input, CompactOptions);
        }
    }

    /// <summary>Converts the prompt. Maps to <c>convertToMistralChatMessages</c>.</summary>
    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        for (var i = 0; i < prompt.Count; i++)
        {
            var isLastMessage = i == prompt.Count - 1;
            switch (prompt[i])
            {
                case SystemModelMessage system:
                    messages.Add(
                        new JsonObject { ["role"] = "system", ["content"] = system.Content }
                    );
                    break;
                case UserModelMessage user:
                    messages.Add(
                        new JsonObject { ["role"] = "user", ["content"] = UserContent(user) }
                    );
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(Assistant(assistant, isLastMessage));
                    break;
                case ToolApprovalResponseModelMessage approval:
                    // Approval responses are skipped. A denied provider-executed call still sends its result.
                    if (approval.DeniedToolResult != null)
                    {
                        messages.Add(Tool(approval.DeniedToolResult));
                    }

                    break;
                case ToolModelMessage tool:
                    foreach (var result in tool.Results)
                    {
                        messages.Add(Tool(result));
                    }

                    break;
                default:
                    throw new InvalidOperationException("Unsupported role: " + prompt[i].Role);
            }
        }

        return messages;
    }

    /// <summary>Converts a tool result into the content of a Mistral tool message.</summary>
    public static string ToolContent(ToolModelMessage result)
    {
        switch (result.OutputType)
        {
            case "text":
            case "error-text":
                return result.OutputJson;
            case "execution-denied":
                return result.ExecutionDeniedReason ?? "Tool call execution denied.";
            default:
                return Stringify(result.OutputJson);
        }
    }

    private static JsonObject Tool(ToolModelMessage result)
    {
        return new JsonObject
        {
            ["role"] = "tool",
            ["name"] = result.ToolName,
            ["tool_call_id"] = result.ToolCallId,
            ["content"] = ToolContent(result),
        };
    }

    private static string Stringify(string json)
    {
        try
        {
            return JsonNode.Parse(json)?.ToJsonString(CompactOptions) ?? "null";
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(json, CompactOptions);
        }
    }

    private static JsonArray UserContent(UserModelMessage user)
    {
        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart text:
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;
                case FileContentPart file:
                    content.Add(File(file));
                    break;
            }
        }

        return content;
    }

    private static JsonObject File(FileContentPart part)
    {
        if (part.ProviderReference != null)
        {
            throw new UnsupportedFunctionalityException(
                "file parts with provider references",
                "'file parts with provider references' functionality not supported."
            );
        }

        if (part.Text != null)
        {
            throw new UnsupportedFunctionalityException(
                "text file parts",
                "'text file parts' functionality not supported."
            );
        }

        if (MediaTypes.GetTopLevelMediaType(part.MediaType) == "image")
        {
            return new JsonObject { ["type"] = "image_url", ["image_url"] = FormatFileUrl(part) };
        }

        if (part.Url == null)
        {
            if (MediaTypes.ResolveFullMediaType(part.MediaType, part.Data) != "application/pdf")
            {
                throw new UnsupportedFunctionalityException(
                    "Only images and PDF file parts are supported",
                    "'Only images and PDF file parts are supported' functionality not supported."
                );
            }
        }
        else if (part.MediaType != "application/pdf")
        {
            throw new UnsupportedFunctionalityException(
                "Only images and PDF file parts are supported",
                "'Only images and PDF file parts are supported' functionality not supported."
            );
        }

        return new JsonObject { ["type"] = "document_url", ["document_url"] = FormatFileUrl(part) };
    }

    private static string FormatFileUrl(FileContentPart part)
    {
        if (part.Url != null)
        {
            return part.Url;
        }

        return "data:"
            + MediaTypes.ResolveFullMediaType(part.MediaType, part.Data)
            + ";base64,"
            + System.Convert.ToBase64String(part.Data ?? Array.Empty<byte>());
    }

    private static JsonObject Assistant(AssistantModelMessage message, bool isLastMessage)
    {
        var text = new StringBuilder();
        var hasReasoning = false;
        var contentParts = new JsonArray();
        var toolCalls = new JsonArray();
        foreach (var part in message.Parts)
        {
            switch (part)
            {
                case AssistantTextPart textPart:
                    text.Append(textPart.Text);
                    contentParts.Add(
                        new JsonObject { ["type"] = "text", ["text"] = textPart.Text }
                    );
                    break;
                case AssistantToolCallPart call:
                    toolCalls.Add(
                        new JsonObject
                        {
                            ["id"] = call.ToolCall.ToolCallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject
                            {
                                ["name"] = call.ToolCall.ToolName,
                                ["arguments"] = SerializeMistralToolCallArguments(
                                    call.ToolCall.ArgumentsJson
                                ),
                            },
                        }
                    );
                    break;
                case AssistantReasoningPart reasoning:
                    hasReasoning = true;
                    contentParts.Add(Thinking(reasoning.Text));
                    break;
                default:
                    throw new InvalidOperationException(
                        "Unsupported content type in assistant message: " + part.Type
                    );
            }
        }

        var result = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = hasReasoning ? contentParts : JsonValue.Create(text.ToString()),
        };
        if (isLastMessage)
        {
            result["prefix"] = true;
        }

        if (toolCalls.Count > 0)
        {
            result["tool_calls"] = toolCalls;
        }

        return result;
    }

    /// <summary>Builds a <c>thinking</c> content part.</summary>
    public static JsonObject Thinking(string text)
    {
        return new JsonObject
        {
            ["type"] = "thinking",
            ["thinking"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text }),
            ["closed"] = true,
        };
    }
}
