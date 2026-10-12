// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenResponses;

/// <summary>The result of converting a prompt into Open Responses input items.</summary>
internal sealed class OpenResponsesInputResult
{
    public OpenResponsesInputResult(JsonArray input, string? instructions, IReadOnlyList<CallWarning> warnings)
    {
        Input = input;
        Instructions = instructions;
        Warnings = warnings;
    }

    public JsonArray Input { get; }

    public string? Instructions { get; }

    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Converts a prompt into Open Responses input items. Maps to <c>convertToOpenResponsesInput</c>.</summary>
internal static class OpenResponsesInput
{
    public static OpenResponsesInputResult Convert(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName = OpenResponsesProvider.ProviderName,
        bool strictResponseInput = false)
    {
        var input = new JsonArray();
        var warnings = new List<CallWarning>();
        var systemMessages = new List<string>();

        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    systemMessages.Add(system.Content);
                    break;
                case UserModelMessage user:
                    input.Add(ConvertUser(user, providerOptionsName));
                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, input, providerOptionsName, strictResponseInput);
                    break;
                case ToolModelMessage tool:
                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = tool.ToolCallId,
                        ["output"] = ToolOutput(tool),
                    });
                    break;
            }
        }

        return new OpenResponsesInputResult(input, systemMessages.Count > 0 ? string.Join("\n", systemMessages) : null, warnings);
    }

    private static JsonObject ConvertUser(UserModelMessage user, string providerOptionsName)
    {
        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart text:
                    content.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
                    break;
                case FileContentPart file:
                    content.Add(ConvertFile(file, providerOptionsName));
                    break;
            }
        }

        return new JsonObject { ["type"] = "message", ["role"] = "user", ["content"] = content };
    }

    private static JsonObject ConvertFile(FileContentPart file, string providerOptionsName)
    {
        if (file.ProviderReference != null)
        {
            const string functionality = "file parts with provider references";
            throw new UnsupportedFunctionalityException(functionality, "'" + functionality + "' functionality not supported.");
        }

        if (MediaTypes.GetTopLevelMediaType(file.MediaType) == "image")
        {
            return new JsonObject
            {
                ["type"] = "input_image",
                ["image_url"] = file.Url ?? DataUrl(file),
                ["detail"] = ImageDetail(file, providerOptionsName),
            };
        }

        if (file.Url != null)
        {
            return new JsonObject { ["type"] = "input_file", ["file_url"] = file.Url };
        }

        return new JsonObject
        {
            ["type"] = "input_file",
            ["filename"] = file.FileName ?? "data",
            ["file_data"] = DataUrl(file),
        };
    }

    private static string DataUrl(FileContentPart file)
    {
        var bytes = file.Data ?? Array.Empty<byte>();
        return "data:" + MediaTypes.ResolveFullMediaType(file.MediaType, bytes) + ";base64," + System.Convert.ToBase64String(bytes);
    }

    private static string ImageDetail(FileContentPart file, string providerOptionsName)
    {
        if (file.ProviderOptions != null
            && file.ProviderOptions.TryGetValue(providerOptionsName, out var options)
            && options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty("imageDetail", out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            var detail = value.GetString();
            if (detail == "low" || detail == "high" || detail == "auto")
            {
                return detail;
            }
        }

        return "auto";
    }

    private static void ConvertAssistant(AssistantModelMessage assistant, JsonArray input, string providerOptionsName, bool strictResponseInput)
    {
        var content = new List<JsonObject>();
        string? messageId = null;

        void Flush()
        {
            if (content.Count == 0)
            {
                return;
            }

            if (strictResponseInput && messageId == null)
            {
                var joined = string.Concat(content.Select(part => part["text"]!.GetValue<string>()));
                input.Add(new JsonObject { ["type"] = "message", ["role"] = "assistant", ["content"] = joined });
            }
            else if (strictResponseInput)
            {
                var strictContent = new JsonArray();
                foreach (var part in content)
                {
                    part["annotations"] ??= new JsonArray();
                    part["logprobs"] ??= new JsonArray();
                    strictContent.Add(part);
                }

                input.Add(new JsonObject
                {
                    ["id"] = messageId,
                    ["type"] = "message",
                    ["status"] = "completed",
                    ["role"] = "assistant",
                    ["content"] = strictContent,
                });
            }
            else
            {
                var item = new JsonObject { ["type"] = "message", ["role"] = "assistant" };
                var array = new JsonArray();
                foreach (var part in content)
                {
                    array.Add(part);
                }

                item["content"] = array;
                if (messageId != null)
                {
                    item["id"] = messageId;
                }

                input.Add(item);
            }

            content = new List<JsonObject>();
            messageId = null;
        }

        foreach (var part in assistant.Parts)
        {
            var data = ProviderData(part, providerOptionsName);
            switch (part)
            {
                case AssistantReasoningPart reasoning:
                    Flush();
                    AddReasoning(input, reasoning, data);
                    break;
                case AssistantTextPart text:
                    var itemId = StringOf(data, "itemId");
                    var annotations = ParseAnnotations(data);
                    if (content.Count > 0 && messageId != itemId)
                    {
                        Flush();
                    }

                    messageId = itemId;
                    var output = new JsonObject { ["type"] = "output_text", ["text"] = text.Text };
                    if (annotations != null)
                    {
                        output["annotations"] = annotations;
                    }

                    content.Add(output);
                    break;
                case AssistantToolCallPart call:
                    Flush();
                    var callItem = new JsonObject { ["type"] = "function_call" };
                    var callItemId = StringOf(data, "itemId");
                    if (callItemId != null)
                    {
                        callItem["id"] = callItemId;
                    }

                    callItem["call_id"] = call.ToolCall.ToolCallId;
                    callItem["name"] = call.ToolCall.ToolName;
                    callItem["arguments"] = call.ToolCall.ArgumentsJson;
                    input.Add(callItem);
                    break;
            }
        }

        Flush();
    }

    private static void AddReasoning(JsonArray input, AssistantReasoningPart part, JsonElement? data)
    {
        var itemId = StringOf(data, "itemId");
        var summary = ParseTextList(data, "reasoningSummary", "summary_text");
        var reasoningContent = ParseTextList(data, "reasoningContent", "reasoning_text");
        var hasReasoningContent = data is { ValueKind: JsonValueKind.Object } object1 && object1.TryGetProperty("reasoningContent", out _);
        var encrypted = StringOf(data, "reasoningEncryptedContent");

        var item = new JsonObject { ["type"] = "reasoning", ["summary"] = summary ?? new JsonArray() };
        if (itemId != null)
        {
            item["id"] = itemId;
        }

        if (reasoningContent != null)
        {
            item["content"] = reasoningContent;
        }
        else if (!hasReasoningContent && part.Text.Length > 0)
        {
            item["content"] = new JsonArray(new JsonObject { ["type"] = "reasoning_text", ["text"] = part.Text });
        }

        if (encrypted != null)
        {
            item["encrypted_content"] = encrypted;
        }

        if (itemId != null
            && input.Count > 0
            && input[input.Count - 1] is JsonObject previous
            && (string?)previous["type"] == "reasoning"
            && (string?)previous["id"] == itemId)
        {
            if (item["content"] is JsonArray added)
            {
                var merged = previous["content"] as JsonArray ?? new JsonArray();
                foreach (var entry in added)
                {
                    merged.Add(entry!.DeepClone());
                }

                previous["content"] = merged;
            }

            return;
        }

        input.Add(item);
    }

    private static string ToolOutput(ToolModelMessage tool)
    {
        switch (tool.OutputType)
        {
            case "execution-denied":
                return string.IsNullOrEmpty(tool.OutputJson) ? "Tool call execution denied." : tool.OutputJson;
            case "json":
            case "error-json":
                return JsonNode.Parse(tool.OutputJson)?.ToJsonString() ?? "null";
            default:
                return tool.OutputJson;
        }
    }

    private static JsonElement? ProviderData(AssistantContentPart part, string providerOptionsName)
    {
        if (part.ProviderOptions != null
            && part.ProviderOptions.TryGetValue(providerOptionsName, out var data)
            && data.ValueKind == JsonValueKind.Object)
        {
            return data;
        }

        return null;
    }

    private static string? StringOf(JsonElement? data, string name)
    {
        return data is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    private static JsonArray? ParseTextList(JsonElement? data, string name, string type)
    {
        if (data is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty(name, out var list)
            || list.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var result = new JsonArray();
        foreach (var entry in list.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object
                || !entry.TryGetProperty("type", out var entryType)
                || entryType.ValueKind != JsonValueKind.String
                || entryType.GetString() != type
                || !entry.TryGetProperty("text", out var text)
                || text.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            result.Add(new JsonObject { ["type"] = type, ["text"] = text.GetString() });
        }

        return result;
    }

    private static JsonArray? ParseAnnotations(JsonElement? data)
    {
        if (data is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty("annotations", out var list)
            || list.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var result = new JsonArray();
        foreach (var entry in list.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object
                || !entry.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String || type.GetString() != "url_citation"
                || !entry.TryGetProperty("start_index", out var start) || start.ValueKind != JsonValueKind.Number
                || !entry.TryGetProperty("end_index", out var end) || end.ValueKind != JsonValueKind.Number
                || !entry.TryGetProperty("url", out var url) || url.ValueKind != JsonValueKind.String
                || !entry.TryGetProperty("title", out var title) || title.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            result.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = JsonNode.Parse(start.GetRawText()),
                ["end_index"] = JsonNode.Parse(end.GetRawText()),
                ["url"] = url.GetString(),
                ["title"] = title.GetString(),
            });
        }

        return result;
    }
}
