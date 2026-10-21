// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Alibaba;

/// <summary>Converts a prompt to Alibaba chat messages. Maps to <c>convertToAlibabaChatMessages</c>.</summary>
internal static class AlibabaChatMessages
{
    private const string ExecutionDeniedText = "Tool call execution denied.";

    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, bool preserveThinking)
    {
        var messages = new JsonArray();
        var flat = ToolModelMessage.Flatten(prompt ?? Array.Empty<ModelMessage>());
        var lastUserMessageIndex = -1;
        for (var i = flat.Count - 1; i >= 0; i--)
        {
            if (flat[i] is UserModelMessage)
            {
                lastUserMessageIndex = i;
                break;
            }
        }

        for (var index = 0; index < flat.Count; index++)
        {
            switch (flat[index])
            {
                case SystemModelMessage system:
                    messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    messages.Add(new JsonObject { ["role"] = "user", ["content"] = MapUserContent(user) });
                    break;
                case AssistantModelMessage assistant:
                    if (MapAssistant(assistant, preserveThinking || index > lastUserMessageIndex) is { } mapped)
                    {
                        messages.Add(mapped);
                    }

                    break;
                case ToolModelMessage tool:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = tool.ToolCallId,
                        ["content"] = ToolContent(tool),
                    });
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + flat[index].Role + ".");
            }
        }

        return messages;
    }

    private static JsonArray MapUserContent(UserModelMessage user)
    {
        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart text:
                    parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;
                case FileContentPart file:
                    parts.Add(MapFile(file));
                    break;
            }
        }

        return parts;
    }

    private static JsonObject MapFile(FileContentPart file)
    {
        if (file.Url == null && file.Data == null)
        {
            throw OpenAICompatibleChat.Unsupported("file parts with provider references");
        }

        var mediaType = file.MediaType ?? string.Empty;
        var slash = mediaType.IndexOf('/');
        if (!string.Equals(slash < 0 ? mediaType : mediaType.Substring(0, slash), "image", StringComparison.OrdinalIgnoreCase))
        {
            throw OpenAICompatibleChat.Unsupported("Only image file parts are supported");
        }

        var url = file.Url != null && file.Data == null
            ? file.Url
            : "data:" + mediaType + ";base64," + System.Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = url },
        };
    }

    // Reasoning from the current round (after the last user message) always accompanies tool calls.
    // Earlier rounds are replayed only when preserved thinking is enabled.
    private static JsonObject? MapAssistant(AssistantModelMessage assistant, bool includeReasoning)
    {
        var text = new StringBuilder();
        var reasoning = new StringBuilder();
        var toolCalls = new JsonArray();
        foreach (var part in assistant.Parts)
        {
            switch (part)
            {
                case AssistantTextPart textPart:
                    text.Append(textPart.Text);
                    break;
                case AssistantToolCallPart callPart:
                    toolCalls.Add(new JsonObject
                    {
                        ["id"] = callPart.ToolCall.ToolCallId,
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = callPart.ToolCall.ToolName,
                            ["arguments"] = callPart.ToolCall.ArgumentsJson ?? string.Empty,
                        },
                    });
                    break;
                case AssistantReasoningPart reasoningPart when includeReasoning:
                    reasoning.Append(reasoningPart.Text);
                    break;
            }
        }

        if (text.Length == 0 && toolCalls.Count == 0 && reasoning.Length == 0)
        {
            return null;
        }

        var message = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = text.Length > 0 ? JsonValue.Create(text.ToString()) : null,
        };
        if (reasoning.Length > 0)
        {
            message["reasoning_content"] = reasoning.ToString();
        }

        if (toolCalls.Count > 0)
        {
            message["tool_calls"] = toolCalls;
        }

        return message;
    }

    private static string ToolContent(ToolModelMessage tool)
    {
        return tool.OutputType == "execution-denied" && string.IsNullOrEmpty(tool.OutputJson)
            ? ExecutionDeniedText
            : tool.OutputJson;
    }
}
