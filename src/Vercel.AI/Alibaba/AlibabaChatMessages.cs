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

    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, bool preserveThinking, AlibabaCacheControlValidator? cacheControlValidator = null)
    {
        var messages = new JsonArray();
        prompt ??= Array.Empty<ModelMessage>();
        var lastUserMessageIndex = -1;
        for (var i = prompt.Count - 1; i >= 0; i--)
        {
            if (prompt[i] is UserModelMessage)
            {
                lastUserMessageIndex = i;
                break;
            }
        }

        for (var index = 0; index < prompt.Count; index++)
        {
            switch (prompt[index])
            {
                case SystemModelMessage system:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "system",
                        ["content"] = TextContent(system.Content, cacheControlValidator?.GetCacheControl(system.ProviderOptions)),
                    });
                    break;
                case UserModelMessage user:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = MapUserContent(user, cacheControlValidator, cacheControlValidator?.GetCacheControl(user.ProviderOptions)),
                    });
                    break;
                case AssistantModelMessage assistant:
                    var assistantCacheControl = cacheControlValidator?.GetCacheControl(assistant.ProviderOptions);
                    if (MapAssistant(assistant, preserveThinking || index > lastUserMessageIndex, assistantCacheControl) is { } mapped)
                    {
                        messages.Add(mapped);
                    }

                    break;
                case ToolModelMessage tool:
                    var toolCacheControl = cacheControlValidator?.GetCacheControl(tool.MessageProviderOptions);
                    var results = tool.Results;
                    for (var i = 0; i < results.Count; i++)
                    {
                        var partCacheControl = cacheControlValidator?.GetCacheControl(results[i].ProviderOptions)
                            ?? (i == results.Count - 1 ? toolCacheControl : null);
                        messages.Add(new JsonObject
                        {
                            ["role"] = "tool",
                            ["tool_call_id"] = results[i].ToolCallId,
                            ["content"] = TextContent(ToolContent(results[i]), partCacheControl),
                        });
                    }

                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + prompt[index].Role + ".");
            }
        }

        return messages;
    }

    // A plain string, or one text block that carries the cache control.
    private static JsonNode? TextContent(string text, JsonNode? cacheControl)
    {
        return cacheControl == null
            ? JsonValue.Create(text)
            : new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text, ["cache_control"] = cacheControl });
    }

    private static JsonArray MapUserContent(UserModelMessage user, AlibabaCacheControlValidator? cacheControlValidator, JsonNode? messageCacheControl)
    {
        var parts = new JsonArray();
        for (var i = 0; i < user.Content.Count; i++)
        {
            var part = user.Content[i];
            var providerOptions = part switch
            {
                TextContentPart text => text.ProviderOptions,
                FileContentPart file => file.ProviderOptions,
                _ => null,
            };
            var partCacheControl = cacheControlValidator?.GetCacheControl(providerOptions)
                ?? (i == user.Content.Count - 1 ? messageCacheControl : null);
            var mapped = part switch
            {
                TextContentPart text => new JsonObject { ["type"] = "text", ["text"] = text.Text },
                FileContentPart file => MapFile(file),
                _ => null,
            };
            if (mapped == null)
            {
                continue;
            }

            if (partCacheControl != null)
            {
                mapped["cache_control"] = partCacheControl;
            }

            parts.Add(mapped);
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
    private static JsonObject? MapAssistant(AssistantModelMessage assistant, bool includeReasoning, JsonNode? cacheControl)
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

        var reasoningOnly = text.Length == 0 && toolCalls.Count == 0;
        var message = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = reasoningOnly ? null
                : cacheControl != null ? TextContent(text.ToString(), cacheControl)
                : text.Length > 0 ? JsonValue.Create(text.ToString()) : null,
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
