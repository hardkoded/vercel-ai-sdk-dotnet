// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Moonshot AI chat message conversion. Maps to <c>convert-to-moonshotai-chat-messages.ts</c>. The chat completions
/// API accepts text, <c>image_url</c>, and <c>video_url</c> content parts only. Anything else (audio, PDF, other file
/// types) throws here rather than being rejected by the API with a 400.
/// </summary>
internal static class MoonshotChatMessages
{
    private static readonly string[] SupportedImageMediaTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp",
        "image/heic",
        "image/heif",
    ];

    private static readonly string[] SupportedVideoMediaTypes =
    [
        "video/mp4",
        "video/mpeg",
        "video/mov",
        "video/avi",
        "video/x-flv",
        "video/mpg",
        "video/webm",
        "video/wmv",
        "video/3gpp",
    ];

    /// <summary>The converted messages and the warnings the conversion raised.</summary>
    internal sealed class Result
    {
        public Result(JsonArray messages, List<CallWarning> warnings)
        {
            Messages = messages;
            Warnings = warnings;
        }

        /// <summary>The Moonshot AI chat messages.</summary>
        public JsonArray Messages { get; }

        /// <summary>The warnings of the conversion.</summary>
        public List<CallWarning> Warnings { get; }
    }

    /// <summary>Maps to <c>convertToMoonshotAIChatMessages</c>. <paramref name="responseFormatType"/> is <c>responseFormat?.type</c>.</summary>
    public static Result ConvertToMoonshotAIChatMessages(
        IReadOnlyList<ModelMessage> prompt,
        string? modelId = null,
        string providerOptionsName = "moonshotai",
        string? responseFormatType = null
    )
    {
        var messages = new JsonArray();
        var warnings = new List<CallWarning>();
        bool? dynamicTools = modelId == null ? null : MoonshotModelCatalog.DynamicTools(modelId);
        var flat = ToolModelMessage.Flatten(prompt);

        for (var index = 0; index < flat.Count; index++)
        {
            var message = flat[index];
            var options = MoonshotChatOptions.ParseMessageProviderOptions(
                ProviderOptionsOf(message),
                providerOptionsName
            );

            if (options?.Partial == true && message is not AssistantModelMessage)
            {
                throw new InvalidPromptException(
                    "Moonshot AI Partial Mode requires `partial: true` on an assistant message."
                );
            }

            if (options?.Tools != null && message is not SystemModelMessage)
            {
                throw new InvalidPromptException(
                    "Moonshot dynamic tools must be configured on a system message."
                );
            }

            switch (message)
            {
                case SystemModelMessage system:
                    if (options?.Tools is { Count: > 0 } dynamic)
                    {
                        if (system.Content.Length > 0)
                        {
                            throw new InvalidPromptException(
                                "A Moonshot dynamic-tool system message must use empty content because the API forbids content alongside tools."
                            );
                        }

                        if (dynamicTools == false)
                        {
                            warnings.Add(
                                new CallWarning(
                                    "unsupported",
                                    $"dynamic tool loading for model \"{modelId}\"",
                                    "Moonshot documents dynamic tool loading only for Kimi K3. The dynamic system message has been omitted."
                                )
                            );
                            break;
                        }

                        var prepared = MoonshotPrepareTools.PrepareTools(
                            modelId ?? "custom-model",
                            dynamic
                        );
                        warnings.AddRange(prepared.ToolWarnings);
                        messages.Add(
                            new JsonObject
                            {
                                ["role"] = "system",
                                ["tools"] = prepared.Tools ?? new JsonArray(),
                            }
                        );
                        break;
                    }

                    messages.Add(
                        WithName(
                            new JsonObject { ["role"] = "system", ["content"] = system.Content },
                            options
                        )
                    );
                    break;
                case UserModelMessage user:
                    messages.Add(
                        WithName(
                            new JsonObject
                            {
                                ["role"] = "user",
                                ["content"] = ConvertUserContent(user),
                            },
                            options
                        )
                    );
                    break;
                case AssistantModelMessage assistant:
                    if (options?.Partial == true)
                    {
                        if (index != flat.Count - 1)
                        {
                            throw new InvalidPromptException(
                                "Moonshot AI Partial Mode requires the partial assistant message to be the final message."
                            );
                        }

                        if (responseFormatType == "json_object")
                        {
                            throw new InvalidPromptException(
                                "Moonshot AI Partial Mode cannot be combined with JSON object response format."
                            );
                        }
                    }

                    messages.Add(ConvertAssistant(assistant, options));
                    break;
                case ToolModelMessage tool:
                    if (options?.Name != null)
                    {
                        warnings.Add(
                            new CallWarning("unsupported", "message name on tool messages", null)
                        );
                    }

                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "tool",
                            ["tool_call_id"] = tool.ToolCallId,
                            ["content"] = tool.OutputType switch
                            {
                                "execution-denied" => tool.ExecutionDeniedReason
                                    ?? "Tool call execution denied.",
                                _ => tool.OutputJson,
                            },
                        }
                    );
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + message?.Role + ".");
            }
        }

        return new Result(messages, warnings);
    }

    private static IReadOnlyDictionary<string, System.Text.Json.JsonElement>? ProviderOptionsOf(
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
        MoonshotAIMessageProviderOptions? options
    )
    {
        if (options?.Name != null)
        {
            message["name"] = options.Name;
        }

        return message;
    }

    private static JsonObject ConvertAssistant(
        AssistantModelMessage assistant,
        MoonshotAIMessageProviderOptions? options
    )
    {
        var text = assistant.Text ?? string.Empty;
        var reasoning = assistant.Reasoning ?? string.Empty;
        var message = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] =
                assistant.ToolCalls.Count > 0 && text.Length == 0 ? null : JsonValue.Create(text),
        };
        WithName(message, options);
        if (options?.Partial == true)
        {
            message["partial"] = true;
        }

        if (reasoning.Length > 0)
        {
            message["reasoning_content"] = reasoning;
        }

        if (assistant.ToolCalls.Count > 0)
        {
            var calls = new JsonArray();
            foreach (var call in assistant.ToolCalls)
            {
                calls.Add(
                    new JsonObject
                    {
                        ["id"] = call.ToolCallId,
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = call.ToolName,
                            ["arguments"] = OpenAICompatibleChat.SerializeToolCallArguments(
                                call.ArgumentsJson ?? string.Empty
                            ),
                        },
                    }
                );
            }

            message["tool_calls"] = calls;
        }

        return message;
    }

    private static JsonNode ConvertUserContent(UserModelMessage user)
    {
        if (user.Content.Count == 1 && user.Content[0] is TextContentPart only)
        {
            return only.Text ?? string.Empty;
        }

        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart text:
                    parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;
                case FileContentPart file:
                    parts.Add(ConvertFile(file));
                    break;
            }
        }

        return parts;
    }

    private static JsonObject ConvertFile(FileContentPart file)
    {
        var mediaType = file.MediaType ?? string.Empty;
        var topLevel = MediaTypes.GetTopLevelMediaType(mediaType);

        if (file.ProviderReference != null)
        {
            if (topLevel != "image" && topLevel != "video")
            {
                throw Unsupported("file part media type " + mediaType);
            }

            var reference = ProviderReferences.ResolveProviderReference(
                file.ProviderReference,
                MoonshotProvider.ProviderId
            );
            if (!reference.StartsWith("ms://", StringComparison.Ordinal))
            {
                throw Unsupported("Moonshot file provider references without an ms:// URL");
            }

            return MediaPart(topLevel, reference);
        }

        if (file.Text != null)
        {
            if (topLevel == "text")
            {
                return new JsonObject { ["type"] = "text", ["text"] = file.Text };
            }

            throw Unsupported("file part media type " + mediaType);
        }

        if (topLevel == "image")
        {
            return MediaPart("image", FormatMediaUrl(file, SupportedImageMediaTypes, "image"));
        }

        if (topLevel == "video")
        {
            return MediaPart("video", FormatMediaUrl(file, SupportedVideoMediaTypes, "video"));
        }

        if (topLevel == "text")
        {
            return new JsonObject
            {
                ["type"] = "text",
                ["text"] = file.Data == null ? file.Url : Encoding.UTF8.GetString(file.Data),
            };
        }

        throw Unsupported("file part media type " + mediaType);
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }

    private static JsonObject MediaPart(string topLevel, string url)
    {
        var key = topLevel + "_url";
        return new JsonObject
        {
            ["type"] = key,
            [key] = new JsonObject { ["url"] = url },
        };
    }

    private static string FormatMediaUrl(
        FileContentPart file,
        string[] supportedMediaTypes,
        string topLevelMediaType
    )
    {
        var isUrl = file.Url != null && file.Data == null;
        var mediaType = isUrl
            ? file.MediaType
            : MediaTypes.ResolveFullMediaType(file.MediaType, file.Data);

        if (
            Array.IndexOf(supportedMediaTypes, mediaType) < 0
            && !(isUrl && (mediaType == topLevelMediaType || mediaType == topLevelMediaType + "/*"))
        )
        {
            throw Unsupported("file part media type " + mediaType);
        }

        return isUrl
            ? file.Url!
            : $"data:{mediaType};base64,{Convert.ToBase64String(file.Data ?? [])}";
    }
}
