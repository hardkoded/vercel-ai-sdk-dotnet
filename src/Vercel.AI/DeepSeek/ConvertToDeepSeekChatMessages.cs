// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>Converts a prompt to DeepSeek chat messages. Maps to <c>convert-to-deepseek-chat-messages.ts</c>.</summary>
internal static class ConvertToDeepSeekChatMessages
{
    private const int MaxImageUrlLength = 8192;

    private static readonly string[] SupportedImageMediaTypes =
    [
        "image/gif",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
    ];

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>The converted messages and the warnings the conversion raised.</summary>
    internal sealed class Converted
    {
        public Converted(JsonArray messages, List<CallWarning> warnings)
        {
            Messages = messages;
            Warnings = warnings;
        }

        public JsonArray Messages { get; }

        public List<CallWarning> Warnings { get; }
    }

    /// <summary>Maps to <c>convertToDeepSeekChatMessages</c>.</summary>
    /// <exception cref="InvalidPromptException">A message option or an image part cannot be sent.</exception>
    /// <exception cref="UnsupportedFunctionalityException">An image media type or a prefix completion is not supported.</exception>
    public static Converted Convert(
        IReadOnlyList<ModelMessage> prompt,
        bool jsonResponse,
        JsonElement? jsonSchema,
        string modelId,
        string providerOptionsName = DeepSeekProvider.ProviderId,
        bool supportsAssistantPrefixCompletion = false,
        bool supportsStructuredOutputs = false
    )
    {
        var isDeepSeekV4 = IsDeepSeekV4Model.Check(modelId);
        var messages = new JsonArray();
        var warnings = new List<CallWarning>();
        var hasSchema =
            jsonSchema is { } schema
            && schema.ValueKind != JsonValueKind.Undefined
            && schema.ValueKind != JsonValueKind.Null;

        // Inject a system message if the response format is JSON.
        if (jsonResponse)
        {
            if (!hasSchema)
            {
                messages.Add(new JsonObject { ["role"] = "system", ["content"] = "Return JSON." });
            }
            else if (!supportsStructuredOutputs)
            {
                messages.Add(
                    new JsonObject
                    {
                        ["role"] = "system",
                        ["content"] =
                            "Return JSON that conforms to the following schema: "
                            + jsonSchema!.Value.GetRawText(),
                    }
                );
                warnings.Add(
                    new CallWarning(
                        "compatibility",
                        "responseFormat JSON schema",
                        "JSON response schema is injected into the system message."
                    )
                );
            }
        }

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
            var message = prompt[index];
            var options = DeepSeekMessageProviderOptions.Parse(
                MessageProviderOptions(message),
                providerOptionsName
            );
            if (options?.Prefix == true && message is not AssistantModelMessage)
            {
                throw new InvalidPromptException(
                    "DeepSeek assistant prefix completion requires `prefix: true` on an assistant message."
                );
            }

            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(
                        WithName(
                            new JsonObject { ["role"] = "system", ["content"] = system.Content },
                            options
                        )
                    );
                    break;
                case UserModelMessage user:
                    messages.Add(
                        WithName(ConvertUser(user, providerOptionsName, warnings), options)
                    );
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(
                        ConvertAssistant(
                            assistant,
                            options,
                            index,
                            prompt.Count,
                            lastUserMessageIndex,
                            isDeepSeekV4,
                            supportsAssistantPrefixCompletion
                        )
                    );
                    break;
                case ToolApprovalResponseModelMessage:
                    break;
                case ToolModelMessage tool:
                    if (options?.Name != null)
                    {
                        warnings.Add(
                            new CallWarning("unsupported", "message name on tool messages", null)
                        );
                    }

                    foreach (var result in tool.Results)
                    {
                        messages.Add(
                            new JsonObject
                            {
                                ["role"] = "tool",
                                ["tool_call_id"] = result.ToolCallId,
                                ["content"] = ToolContent(result, providerOptionsName, warnings),
                            }
                        );
                    }

                    break;
                default:
                    warnings.Add(
                        new CallWarning("unsupported", "message role: " + message.Role, null)
                    );
                    break;
            }
        }

        return new Converted(messages, warnings);
    }

    /// <summary>
    /// Returns the tool call arguments as JSON text: valid JSON is kept as it is and any other text is sent as a JSON string.
    /// Maps to <c>serializeDeepSeekToolCallArguments</c>.
    /// </summary>
    public static string SerializeDeepSeekToolCallArguments(string input)
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

    private static IReadOnlyDictionary<string, JsonElement>? MessageProviderOptions(
        ModelMessage message
    )
    {
        return message switch
        {
            SystemModelMessage system => system.ProviderOptions,
            UserModelMessage user => user.ProviderOptions,
            AssistantModelMessage assistant => assistant.ProviderOptions,
            ToolModelMessage tool => tool.MessageProviderOptions,
            _ => null,
        };
    }

    private static JsonObject WithName(
        JsonObject message,
        DeepSeekAssistantMessageProviderOptions? options
    )
    {
        if (options?.Name != null)
        {
            message["name"] = options.Name;
        }

        return message;
    }

    private static bool IsImage(FileContentPart file)
    {
        return MediaTypes.GetTopLevelMediaType(file.MediaType) == "image"
            && (file.ProviderReference != null || file.Url != null || file.Data != null);
    }

    private static JsonObject ConvertUser(
        UserModelMessage user,
        string providerOptionsName,
        List<CallWarning> warnings
    )
    {
        if (!user.Content.Any(part => part is FileContentPart file && IsImage(file)))
        {
            var text = new System.Text.StringBuilder();
            foreach (var part in user.Content)
            {
                if (part is TextContentPart textPart)
                {
                    text.Append(textPart.Text);
                }
                else
                {
                    warnings.Add(
                        new CallWarning("unsupported", "user message part type: " + part.Type, null)
                    );
                }
            }

            return new JsonObject { ["role"] = "user", ["content"] = text.ToString() };
        }

        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart)
            {
                content.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
            }
            else if (
                part is FileContentPart file
                && MediaTypes.GetTopLevelMediaType(file.MediaType) == "image"
            )
            {
                var filePartOptions = DeepSeekFilePartProviderOptions.Parse(
                    file.ProviderOptions,
                    providerOptionsName
                );
                if (file.ProviderReference != null)
                {
                    content.Add(
                        new JsonObject
                        {
                            ["type"] = "file",
                            ["file_id"] = ProviderReferences.ResolveProviderReference(
                                file.ProviderReference,
                                DeepSeekProvider.ProviderId
                            ),
                        }
                    );
                }
                else if (file.Url != null || file.Data != null)
                {
                    var mediaType = ResolveDeepSeekImageMediaType(file);
                    if (file.Url != null)
                    {
                        ValidateDeepSeekImageUrl(file.Url);
                        if (filePartOptions?.FileData == true)
                        {
                            throw new InvalidPromptException(
                                "DeepSeek `fileData` image parts require inline data, not a URL."
                            );
                        }

                        content.Add(ImageUrl(file.Url, filePartOptions));
                    }
                    else
                    {
                        var dataUrl = DataUrl(mediaType, file.Data!);
                        if (filePartOptions?.FileData == true)
                        {
                            if (filePartOptions.ImageDetail != null)
                            {
                                throw new InvalidPromptException(
                                    "DeepSeek `imageDetail` cannot be combined with `fileData`."
                                );
                            }

                            var filePart = new JsonObject
                            {
                                ["type"] = "file",
                                ["file_data"] = dataUrl,
                            };
                            if (file.FileName != null)
                            {
                                filePart["filename"] = file.FileName;
                            }

                            content.Add(filePart);
                        }
                        else
                        {
                            content.Add(ImageUrl(dataUrl, filePartOptions));
                        }
                    }
                }
                else
                {
                    warnings.Add(
                        new CallWarning("unsupported", "user message part type: " + part.Type, null)
                    );
                }
            }
            else
            {
                warnings.Add(
                    new CallWarning("unsupported", "user message part type: " + part.Type, null)
                );
            }
        }

        return new JsonObject { ["role"] = "user", ["content"] = content };
    }

    private static JsonObject ImageUrl(string url, DeepSeekFilePartProviderOptions? options)
    {
        var imageUrl = new JsonObject { ["url"] = url };
        if (options?.ImageDetail != null)
        {
            imageUrl["detail"] = options.ImageDetail;
        }

        return new JsonObject { ["type"] = "image_url", ["image_url"] = imageUrl };
    }

    private static string DataUrl(string mediaType, byte[] data)
    {
        return "data:"
            + (mediaType == "image/jpg" ? "image/jpeg" : mediaType)
            + ";base64,"
            + System.Convert.ToBase64String(data);
    }

    private static string ResolveDeepSeekImageMediaType(FileContentPart file)
    {
        var resolved = MediaTypes.ResolveFullMediaType(file.MediaType, file.Data);
        if (Array.IndexOf(SupportedImageMediaTypes, resolved) < 0)
        {
            throw new UnsupportedFunctionalityException(
                "DeepSeek image media type " + resolved,
                "'DeepSeek image media type "
                    + resolved
                    + "' functionality not supported. DeepSeek supports JPEG, PNG, GIF, and WebP image inputs."
            );
        }

        return resolved;
    }

    private static void ValidateDeepSeekImageUrl(string url)
    {
        if (url.Length > MaxImageUrlLength)
        {
            throw new InvalidPromptException(
                "DeepSeek image URLs must not exceed 8192 characters."
            );
        }
    }

    private static JsonObject ConvertAssistant(
        AssistantModelMessage assistant,
        DeepSeekAssistantMessageProviderOptions? options,
        int index,
        int promptCount,
        int lastUserMessageIndex,
        bool isDeepSeekV4,
        bool supportsAssistantPrefixCompletion
    )
    {
        if (options?.Prefix == true)
        {
            if (index != promptCount - 1)
            {
                throw new InvalidPromptException(
                    "DeepSeek assistant prefix completion requires the prefixed assistant message to be the final message."
                );
            }

            if (!supportsAssistantPrefixCompletion)
            {
                throw new UnsupportedFunctionalityException(
                    "DeepSeek assistant prefix completion",
                    "DeepSeek assistant prefix completion requires a beta base URL ending in `/beta`."
                );
            }
        }

        var text = new System.Text.StringBuilder();
        string? reasoning = null;
        var toolCalls = new JsonArray();
        foreach (var part in assistant.Parts)
        {
            switch (part)
            {
                case AssistantTextPart textPart:
                    text.Append(textPart.Text);
                    break;
                case AssistantReasoningPart reasoningPart:
                    // R1 must not receive prior reasoning; V4 requires it.
                    if (index <= lastUserMessageIndex && !isDeepSeekV4)
                    {
                        break;
                    }

                    reasoning =
                        reasoning == null ? reasoningPart.Text : reasoning + reasoningPart.Text;
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
                                ["arguments"] = SerializeDeepSeekToolCallArguments(
                                    call.ArgumentsJson
                                ),
                            },
                        }
                    );
                    break;
            }
        }

        // V4 demands the field on every assistant turn. Back-fill an empty string when the source message had no
        // reasoning part at all.
        var message = new JsonObject { ["role"] = "assistant", ["content"] = text.ToString() };
        WithName(message, options);
        if (options?.Prefix == true)
        {
            message["prefix"] = true;
        }

        var reasoningContent = reasoning ?? (isDeepSeekV4 ? string.Empty : null);
        if (reasoningContent != null)
        {
            message["reasoning_content"] = reasoningContent;
        }

        if (toolCalls.Count > 0)
        {
            message["tool_calls"] = toolCalls;
        }

        return message;
    }

    private static JsonNode ToolContent(
        ToolModelMessage result,
        string providerOptionsName,
        List<CallWarning> warnings
    )
    {
        switch (result.OutputType)
        {
            case "text":
            case "error-text":
                return result.OutputJson;
            case "execution-denied":
                return result.ExecutionDeniedReason ?? "Tool call execution denied.";
        }

        if (
            result.OutputContent == null
            || !result.OutputContent.Any(part => part is FileContentPart file && IsImage(file))
        )
        {
            return Stringify(result.OutputJson);
        }

        var content = new JsonArray();
        foreach (var part in result.OutputContent)
        {
            if (part is TextContentPart text)
            {
                content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file && IsImage(file))
            {
                if (file.ProviderReference != null)
                {
                    content.Add(
                        new JsonObject
                        {
                            ["type"] = "file",
                            ["file_id"] = ProviderReferences.ResolveProviderReference(
                                file.ProviderReference,
                                DeepSeekProvider.ProviderId
                            ),
                        }
                    );
                    continue;
                }

                var filePartOptions = DeepSeekFilePartProviderOptions.Parse(
                    file.ProviderOptions,
                    providerOptionsName
                );
                var mediaType = ResolveDeepSeekImageMediaType(file);
                string url;
                if (file.Url != null)
                {
                    url = file.Url;
                    ValidateDeepSeekImageUrl(url);
                }
                else
                {
                    url = DataUrl(mediaType, file.Data!);
                }

                content.Add(ImageUrl(url, filePartOptions));
            }
            else
            {
                warnings.Add(
                    new CallWarning(
                        "unsupported",
                        "tool result content part type: " + part.Type,
                        null
                    )
                );
            }
        }

        return content;
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
}
