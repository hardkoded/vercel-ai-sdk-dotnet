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

namespace Vercel.AI.Cohere;

/// <summary>Converts a prompt to Cohere chat messages and documents. Maps to <c>convert-to-cohere-chat-prompt.ts</c>.</summary>
internal static class ConvertToCohereChatPrompt
{
    private static readonly JsonSerializerOptions s_compactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Returns the tool call arguments as JSON text: valid JSON is kept as it is and any other text is sent as a JSON string.
    /// Maps to <c>serializeCohereToolCallArguments</c>.
    /// </summary>
    internal static string SerializeCohereToolCallArguments(string input)
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

    /// <summary>Converts the prompt. Maps to <c>convertToCohereChatPrompt</c>.</summary>
    internal static (JsonArray Messages, JsonArray Documents) Convert(
        IReadOnlyList<ModelMessage> prompt
    )
    {
        var messages = new JsonArray();
        var documents = new JsonArray();
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
                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "user",
                            ["content"] = UserContent(user, documents),
                        }
                    );
                    break;
                case AssistantModelMessage assistant when assistant.ToolCalls.Count > 0:
                    var toolCalls = new JsonArray();
                    foreach (var call in assistant.ToolCalls)
                    {
                        toolCalls.Add(
                            new JsonObject
                            {
                                ["id"] = call.ToolCallId,
                                ["type"] = "function",
                                ["function"] = new JsonObject
                                {
                                    ["name"] = call.ToolName,
                                    ["arguments"] = SerializeCohereToolCallArguments(
                                        call.ArgumentsJson
                                    ),
                                },
                            }
                        );
                    }

                    messages.Add(
                        new JsonObject { ["role"] = "assistant", ["tool_calls"] = toolCalls }
                    );
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "assistant",
                            ["content"] = assistant.Text ?? string.Empty,
                        }
                    );
                    break;
                case ToolModelMessage tool:
                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "tool",
                            ["content"] = tool.OutputJson,
                            ["tool_call_id"] = tool.ToolCallId,
                        }
                    );
                    break;
            }
        }

        return (messages, documents);
    }

    // Images stay inline. Other files are sent as RAG documents, so a message without images is plain text.
    private static JsonNode UserContent(UserModelMessage user, JsonArray documents)
    {
        var parts = new JsonArray();
        var text = new StringBuilder();
        var hasImage = false;
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart && textPart.Text.Length > 0)
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
                text.Append(textPart.Text);
            }
            else if (
                part is FileContentPart file
                && MediaTypes.GetTopLevelMediaType(file.MediaType) == "image"
            )
            {
                hasImage = true;
                var url = file.Url
                    ?? "data:"
                        + MediaTypes.ResolveFullMediaType(file.MediaType, file.Data)
                        + ";base64,"
                        + System.Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
                parts.Add(
                    new JsonObject
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new JsonObject { ["url"] = url },
                    }
                );
            }
            else if (part is FileContentPart document)
            {
                if (document.Url != null)
                {
                    throw new UnsupportedFunctionalityException(
                        "File URL data",
                        "URLs should be downloaded by the AI SDK and not reach this point. This indicates a configuration issue."
                    );
                }

                var data = new JsonObject
                {
                    ["text"] = Encoding.UTF8.GetString(document.Data ?? Array.Empty<byte>()),
                };
                if (document.FileName != null)
                {
                    data["title"] = document.FileName;
                }

                documents.Add(new JsonObject { ["data"] = data });
            }
        }

        return hasImage ? parts : JsonValue.Create(text.ToString())!;
    }
}
