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
using Vercel.AI.Util;

namespace Vercel.AI.Groq;

/// <summary>Converts a prompt to Groq chat messages. Maps to <c>convert-to-groq-chat-messages.ts</c>.</summary>
internal static class ConvertToGroqChatMessages
{
    private static readonly JsonSerializerOptions s_compactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Maps to <c>convertToGroqChatMessages</c>.</summary>
    /// <exception cref="UnsupportedFunctionalityException">A file part cannot be sent to Groq.</exception>
    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(
                        new JsonObject { ["role"] = "system", ["content"] = system.Content }
                    );
                    break;
                case UserModelMessage user:
                    messages.Add(ConvertUser(user));
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(ConvertAssistant(assistant));
                    break;
                case ToolModelMessage tool:
                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "tool",
                            ["tool_call_id"] = tool.ToolCallId,
                            ["content"] = ToolContent(tool),
                        }
                    );
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + message.Role);
            }
        }

        return messages;
    }

    /// <summary>
    /// Returns the tool call arguments as JSON text: valid JSON is kept as it is and any other text is sent as a JSON string.
    /// Maps to <c>serializeGroqToolCallArguments</c>.
    /// </summary>
    internal static string SerializeGroqToolCallArguments(string input)
    {
        try
        {
            SecureJson.Parse(input);
            return input;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(input, s_compactOptions);
        }
    }

    private static JsonObject ConvertUser(UserModelMessage user)
    {
        if (user.Content.Count == 1 && user.Content[0] is TextContentPart only)
        {
            return new JsonObject { ["role"] = "user", ["content"] = only.Text };
        }

        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                content.Add(ConvertFile(file));
            }
        }

        return new JsonObject { ["role"] = "user", ["content"] = content };
    }

    private static JsonObject ConvertFile(FileContentPart file)
    {
        if (file.ProviderReference != null)
        {
            throw Unsupported("file parts with provider references");
        }

        if (file.Text != null)
        {
            throw Unsupported("text file parts");
        }

        if (MediaTypes.GetTopLevelMediaType(file.MediaType) != "image")
        {
            throw Unsupported("Non-image file content parts");
        }

        var url = file.Url
            ?? "data:"
                + MediaTypes.ResolveFullMediaType(file.MediaType, file.Data)
                + ";base64,"
                + System.Convert.ToBase64String(file.Data ?? []);
        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = url },
        };
    }

    private static JsonObject ConvertAssistant(AssistantModelMessage assistant)
    {
        var text = new StringBuilder();
        var reasoning = new StringBuilder();
        var toolCalls = new JsonArray();
        foreach (var part in assistant.Parts)
        {
            switch (part)
            {
                // groq supports reasoning for tool-calls in multi-turn conversations
                case AssistantReasoningPart reasoningPart:
                    reasoning.Append(reasoningPart.Text);
                    break;
                case AssistantTextPart textPart:
                    text.Append(textPart.Text);
                    break;
                case AssistantToolCallPart toolCallPart:
                    var call = toolCallPart.ToolCall;
                    toolCalls.Add(
                        new JsonObject
                        {
                            ["id"] = call.ToolCallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject
                            {
                                ["name"] = call.ToolName,
                                ["arguments"] = SerializeGroqToolCallArguments(call.ArgumentsJson),
                            },
                        }
                    );
                    break;
            }
        }

        var message = new JsonObject { ["role"] = "assistant", ["content"] = text.ToString() };
        if (reasoning.Length > 0)
        {
            message["reasoning"] = reasoning.ToString();
        }

        if (toolCalls.Count > 0)
        {
            message["tool_calls"] = toolCalls;
        }

        return message;
    }

    private static string ToolContent(ToolModelMessage tool)
    {
        switch (tool.OutputType)
        {
            case "text":
            case "error-text":
                return tool.OutputJson;
            case "execution-denied":
                return tool.ExecutionDeniedReason ?? "Tool call execution denied.";
        }

        try
        {
            return JsonNode.Parse(tool.OutputJson)?.ToJsonString(s_compactOptions) ?? "null";
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(tool.OutputJson, s_compactOptions);
        }
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }
}
