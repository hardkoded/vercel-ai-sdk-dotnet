// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>Converts a prompt into DeepSeek Responses input items. Maps to <c>convertToDeepSeekResponsesInput</c>.</summary>
internal static class DeepSeekResponsesInput
{
    private const int MaxImageUrlLength = 8192;
    private static readonly string[] ImageDetails = { "low", "high", "original", "auto" };
    private static readonly string[] SupportedImageMediaTypes = { "image/gif", "image/jpeg", "image/jpg", "image/png", "image/webp" };

    /// <exception cref="InvalidPromptException">An image URL is longer than 8192 characters.</exception>
    /// <exception cref="UnsupportedFunctionalityException">An image part cannot be sent.</exception>
    /// <exception cref="InvalidArgumentError">The provider options of an image part are invalid.</exception>
    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, string providerOptionsName, List<CallWarning> warnings)
    {
        var input = new JsonArray();
        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            switch (message)
            {
                case SystemModelMessage system:
                    input.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    input.Add(ConvertUser(user, providerOptionsName, warnings));
                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, providerOptionsName, input, warnings);
                    break;
                case ToolModelMessage tool:
                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = tool.ToolCallId,
                        ["output"] = ToolOutput(tool, providerOptionsName, warnings),
                    });
                    break;
            }
        }

        return input;
    }

    private static JsonObject ConvertUser(UserModelMessage user, string providerOptionsName, List<CallWarning> warnings)
    {
        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                content.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file && IsImage(file))
            {
                content.Add(ConvertImage(file, providerOptionsName));
            }
            else
            {
                warnings.Add(new CallWarning("unsupported", "user message part type: " + part.Type, null));
            }
        }

        return new JsonObject { ["role"] = "user", ["content"] = content };
    }

    private static bool IsImage(FileContentPart file)
    {
        return MediaTypes.GetTopLevelMediaType(file.MediaType) == "image"
            && (file.ProviderReference != null || file.Url != null || file.Data != null);
    }

    private static void ConvertAssistant(AssistantModelMessage assistant, string providerOptionsName, JsonArray input, List<CallWarning> warnings)
    {
        foreach (var part in assistant.Parts)
        {
            switch (part)
            {
                case AssistantTextPart text:
                    if (text.Text.Length > 0)
                    {
                        input.Add(new JsonObject { ["role"] = "assistant", ["content"] = text.Text });
                    }

                    break;
                case AssistantReasoningPart reasoning:
                    var item = new JsonObject { ["type"] = "reasoning" };
                    if (Option(part, providerOptionsName, "itemId") is { ValueKind: JsonValueKind.String } itemId)
                    {
                        item["id"] = itemId.GetString();
                    }

                    item["content"] = new JsonArray(new JsonObject { ["type"] = "reasoning_text", ["text"] = reasoning.Text });
                    input.Add(item);
                    break;
                case AssistantToolCallPart toolCall:
                    var call = toolCall.ToolCall;
                    if (call.ProviderExecuted)
                    {
                        if (Option(part, providerOptionsName, "webSearchCall") is { ValueKind: JsonValueKind.Object } webSearchCall
                            && OpenAI.OpenAIJson.String(webSearchCall, "type") == "web_search_call")
                        {
                            input.Add(JsonNode.Parse(webSearchCall.GetRawText()));
                        }

                        break;
                    }

                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call",
                        ["call_id"] = call.ToolCallId,
                        ["name"] = call.ToolName,
                        ["arguments"] = call.ArgumentsJson,
                    });
                    break;
                case AssistantToolResultPart:
                    break;
                default:
                    warnings.Add(new CallWarning("unsupported", "assistant message part type: " + part.Type, null));
                    break;
            }
        }
    }

    private static JsonNode ToolOutput(ToolModelMessage tool, string providerOptionsName, List<CallWarning> warnings)
    {
        switch (tool.OutputType)
        {
            case "execution-denied":
                return string.IsNullOrEmpty(tool.OutputJson) ? "Tool call execution denied." : tool.OutputJson;
            case "text":
            case "error-text":
                return tool.OutputJson;
        }

        if (tool.OutputContent == null)
        {
            return tool.OutputJson;
        }

        var value = new JsonArray();
        foreach (var item in tool.OutputContent)
        {
            if (item is TextContentPart text)
            {
                value.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
            }
            else if (item is FileContentPart file && IsImage(file))
            {
                value.Add(ConvertImage(file, providerOptionsName));
            }
            else
            {
                warnings.Add(new CallWarning("unsupported", "tool result content part type: " + item.Type, null));
            }
        }

        return value;
    }

    private static JsonObject ConvertImage(FileContentPart file, string providerOptionsName)
    {
        if (file.ProviderReference != null)
        {
            return new JsonObject
            {
                ["type"] = "input_image",
                ["file_id"] = ProviderReferences.ResolveProviderReference(file.ProviderReference, DeepSeekProvider.ProviderId),
            };
        }

        var options = FilePartOptions(file, providerOptionsName);
        if (options.FileData)
        {
            const string functionality = "DeepSeek `fileData` image parts with the Responses API";
            throw new UnsupportedFunctionalityException(functionality, functionality);
        }

        var image = new JsonObject { ["type"] = "input_image" };
        if (file.Url != null)
        {
            if (file.Url.Length > MaxImageUrlLength)
            {
                throw new InvalidPromptException("DeepSeek image URLs must not exceed 8192 characters.");
            }

            image["image_url"] = file.Url;
        }
        else
        {
            var mediaType = MediaTypes.ResolveFullMediaType(file.MediaType, file.Data);
            if (Array.IndexOf(SupportedImageMediaTypes, mediaType) < 0)
            {
                throw new UnsupportedFunctionalityException(
                    "DeepSeek image media type " + mediaType,
                    "'DeepSeek image media type " + mediaType + "' functionality not supported. DeepSeek supports JPEG, PNG, GIF, and WebP image inputs.");
            }

            image["image_url"] = "data:" + (mediaType == "image/jpg" ? "image/jpeg" : mediaType) + ";base64," + System.Convert.ToBase64String(file.Data!);
        }

        if (options.ImageDetail != null)
        {
            image["detail"] = options.ImageDetail;
        }

        return image;
    }

    private static (string? ImageDetail, bool FileData) FilePartOptions(FileContentPart file, string providerOptionsName)
    {
        if (file.ProviderOptions == null
            || !file.ProviderOptions.TryGetValue(providerOptionsName, out var options)
            || options.ValueKind == JsonValueKind.Null)
        {
            return (null, false);
        }

        var scope = "providerOptions." + providerOptionsName;
        if (options.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentError(scope, null, "expected an object.");
        }

        string? detail = null;
        var fileData = false;
        if (options.TryGetProperty("imageDetail", out var detailValue) && detailValue.ValueKind != JsonValueKind.Null)
        {
            detail = detailValue.ValueKind == JsonValueKind.String ? detailValue.GetString() : null;
            if (detail == null || Array.IndexOf(ImageDetails, detail) < 0)
            {
                throw new InvalidArgumentError(scope + ".imageDetail", null, "expected one of low, high, original, auto.");
            }
        }

        if (options.TryGetProperty("fileData", out var fileDataValue) && fileDataValue.ValueKind != JsonValueKind.Null)
        {
            if (fileDataValue.ValueKind != JsonValueKind.True)
            {
                throw new InvalidArgumentError(scope + ".fileData", null, "expected true.");
            }

            fileData = true;
        }

        return (detail, fileData);
    }

    private static JsonElement? Option(AssistantContentPart part, string providerOptionsName, string name)
    {
        return part.ProviderOptions != null
            && part.ProviderOptions.TryGetValue(providerOptionsName, out var options)
            && options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty(name, out var value)
                ? value
                : null;
    }
}
