// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Xai;

/// <summary>Converts a prompt into xAI Responses input items. Maps to <c>convertToXaiResponsesInput</c>.</summary>
internal static class XaiResponsesInput
{
    private static readonly string[] ImageDetails = { "low", "high", "auto" };

    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, List<CallWarning> warnings)
    {
        var input = new JsonArray();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    input.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    input.Add(ConvertUser(user, warnings));
                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, input, warnings);
                    break;
                case ToolModelMessage tool:
                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = tool.ToolCallId,
                        ["output"] = ToolOutput(tool),
                    });
                    break;
                default:
                    warnings.Add(new CallWarning("other", "unsupported message role"));
                    break;
            }
        }

        return input;
    }

    private static JsonObject ConvertUser(UserModelMessage user, List<CallWarning> warnings)
    {
        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart text:
                    parts.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
                    break;
                case FileContentPart file:
                    parts.Add(ConvertFile(file));
                    break;
                default:
                    warnings.Add(new CallWarning("other", "xAI Responses API does not support this content type in user messages"));
                    break;
            }
        }

        return new JsonObject { ["role"] = "user", ["content"] = parts };
    }

    private static JsonObject ConvertFile(FileContentPart file)
    {
        if (file.ProviderReference != null)
        {
            return new JsonObject
            {
                ["type"] = "input_file",
                ["file_id"] = ProviderReferences.ResolveProviderReference(file.ProviderReference, XaiProvider.ProviderId),
            };
        }

        if (MediaTypes.GetTopLevelMediaType(file.MediaType) == "image")
        {
            var image = new JsonObject
            {
                ["type"] = "input_image",
                ["image_url"] = file.Url != null
                    ? file.Url
                    : "data:" + MediaTypes.ResolveFullMediaType(file.MediaType, file.Data) + ";base64," + System.Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
            };
            if (ImageDetail(file) is { } detail)
            {
                image["detail"] = detail;
            }

            return image;
        }

        // xAI takes non-image documents by URL or by a Files API reference. It does not take inline bytes.
        if (file.Url != null)
        {
            return new JsonObject { ["type"] = "input_file", ["file_url"] = file.Url };
        }

        var functionality = "file part media type " + file.MediaType + " as inline data (xAI Responses requires a URL or a Files API reference for non-image files)";
        throw new UnsupportedFunctionalityException(functionality, functionality);
    }

    private static string? ImageDetail(FileContentPart file)
    {
        if (file.ProviderOptions == null
            || !file.ProviderOptions.TryGetValue(XaiProvider.ProviderId, out var options)
            || options.ValueKind != JsonValueKind.Object
            || !options.TryGetProperty("imageDetail", out var value)
            || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var detail = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (detail == null || Array.IndexOf(ImageDetails, detail) < 0)
        {
            throw new InvalidArgumentError("providerOptions.xai.imageDetail", null, "expected one of low, high, auto.");
        }

        return detail;
    }

    private static void ConvertAssistant(AssistantModelMessage assistant, JsonArray input, List<CallWarning> warnings)
    {
        foreach (var part in assistant.Parts)
        {
            switch (part)
            {
                case AssistantTextPart text:
                    var message = new JsonObject { ["role"] = "assistant", ["content"] = text.Text };
                    if (XaiString(part, "itemId") is { } textItemId)
                    {
                        message["id"] = textItemId;
                    }

                    input.Add(message);
                    break;
                case AssistantToolCallPart toolCall:
                    // xAI ran provider-executed tools itself, so they are not sent back.
                    if (toolCall.ToolCall.ProviderExecuted)
                    {
                        break;
                    }

                    var call = toolCall.ToolCall;
                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call",
                        ["id"] = XaiString(part, "itemId") ?? call.ToolCallId,
                        ["call_id"] = call.ToolCallId,
                        ["name"] = call.ToolName,
                        ["arguments"] = string.IsNullOrEmpty(call.ArgumentsJson) ? "{}" : call.ArgumentsJson,
                        ["status"] = "completed",
                    });
                    break;
                case AssistantToolResultPart:
                    break;
                case AssistantReasoningPart reasoning:
                    ConvertReasoning(reasoning, input, warnings);
                    break;
                default:
                    warnings.Add(new CallWarning("other", "xAI Responses API does not support " + part.Type + " in assistant messages"));
                    break;
            }
        }
    }

    private static void ConvertReasoning(AssistantReasoningPart part, JsonArray input, List<CallWarning> warnings)
    {
        var itemId = XaiString(part, "itemId");
        var encryptedContent = XaiString(part, "reasoningEncryptedContent");
        if (itemId == null && encryptedContent == null)
        {
            warnings.Add(new CallWarning("other", "Reasoning parts without itemId or encrypted content cannot be sent back to xAI. Skipping."));
            return;
        }

        var summary = new JsonArray();
        if (part.Text.Length > 0)
        {
            summary.Add(new JsonObject { ["type"] = "summary_text", ["text"] = part.Text });
        }

        var reasoning = new JsonObject
        {
            ["type"] = "reasoning",
            ["id"] = itemId ?? string.Empty,
            ["summary"] = summary,
            ["status"] = "completed",
        };
        if (encryptedContent != null)
        {
            reasoning["encrypted_content"] = encryptedContent;
        }

        input.Add(reasoning);
    }

    /// <summary>A string under the part's <c>xai</c> provider options, or null when it is missing or not a string.</summary>
    private static string? XaiString(AssistantContentPart part, string name)
    {
        return part.ProviderOptions != null && part.ProviderOptions.TryGetValue(XaiProvider.ProviderId, out var options)
            ? OpenAI.OpenAIJson.String(options, name)
            : null;
    }

    private static string ToolOutput(ToolModelMessage tool)
    {
        return tool.OutputType == "execution-denied" && string.IsNullOrEmpty(tool.OutputJson)
            ? "tool execution denied"
            : tool.OutputJson;
    }
}
