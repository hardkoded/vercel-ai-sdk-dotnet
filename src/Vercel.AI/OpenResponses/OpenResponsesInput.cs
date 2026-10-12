// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenResponses;

/// <summary>The request <c>input</c>, the <c>instructions</c>, and the warnings found while converting a prompt.</summary>
internal sealed class OpenResponsesInputResult
{
    public OpenResponsesInputResult(JsonArray input, string? instructions, List<CallWarning> warnings)
    {
        Input = input;
        Instructions = instructions;
        Warnings = warnings;
    }

    public JsonArray Input { get; }

    public string? Instructions { get; }

    public List<CallWarning> Warnings { get; }
}

/// <summary>Converts a prompt into Open Responses input items. Maps to <c>convertToOpenResponsesInput</c>.</summary>
internal static class OpenResponsesInput
{
    public static OpenResponsesInputResult Convert(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName,
        OpenResponsesExtensionRegistry? extensionRegistry,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName,
        bool strictResponseInput)
    {
        var input = new JsonArray();
        var warnings = new List<CallWarning>();
        var systemMessages = new List<string>();
        var replayedExtensionItems = new HashSet<string>(StringComparer.Ordinal);

        foreach (var message in prompt)
        {
            if (message is SystemModelMessage system)
            {
                systemMessages.Add(system.Content);
            }
            else if (message is UserModelMessage user)
            {
                input.Add(ConvertUser(user, providerOptionsName));
            }
            else if (message is AssistantModelMessage assistant)
            {
                ConvertAssistant(assistant, input, warnings, replayedExtensionItems, providerOptionsName, extensionRegistry, providerToolsByName, strictResponseInput);
            }
            else if (message is ToolModelMessage tool)
            {
                ConvertTool(tool, input, warnings, replayedExtensionItems, providerOptionsName, extensionRegistry, providerToolsByName);
            }
        }

        return new OpenResponsesInputResult(input, systemMessages.Count > 0 ? string.Join("\n", systemMessages) : null, warnings);
    }

    private static JsonObject ConvertUser(UserModelMessage user, string providerOptionsName)
    {
        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                content.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                if (file.ProviderReference != null)
                {
                    throw new UnsupportedFunctionalityException("file parts with provider references", "Open Responses does not support file parts with provider references.");
                }

                if (MediaTypes.GetTopLevelMediaType(file.MediaType) == "image")
                {
                    content.Add(new JsonObject
                    {
                        ["type"] = "input_image",
                        ["image_url"] = file.Url ?? DataUri(file),
                        ["detail"] = ImageDetail(file.ProviderOptions, providerOptionsName),
                    });
                }
                else if (file.Url != null)
                {
                    content.Add(new JsonObject { ["type"] = "input_file", ["file_url"] = file.Url });
                }
                else
                {
                    content.Add(new JsonObject
                    {
                        ["type"] = "input_file",
                        ["filename"] = file.FileName ?? "data",
                        ["file_data"] = DataUri(file),
                    });
                }
            }
        }

        return new JsonObject { ["type"] = "message", ["role"] = "user", ["content"] = content };
    }

    private static string DataUri(FileContentPart file)
    {
        var data = file.Data ?? Array.Empty<byte>();
        return "data:" + MediaTypes.ResolveFullMediaType(file.MediaType, data) + ";base64," + System.Convert.ToBase64String(data);
    }

    private static void ConvertAssistant(
        AssistantModelMessage assistant,
        JsonArray input,
        List<CallWarning> warnings,
        HashSet<string> replayedExtensionItems,
        string providerOptionsName,
        OpenResponsesExtensionRegistry? extensionRegistry,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName,
        bool strictResponseInput)
    {
        var assistantContent = new JsonArray();
        string? assistantMessageId = null;

        void Flush()
        {
            if (assistantContent.Count == 0)
            {
                return;
            }

            if (strictResponseInput && assistantMessageId == null)
            {
                var joined = string.Concat(assistantContent.Select(part => (string?)part!["type"] == "output_text" ? (string?)part["text"] : (string?)part["refusal"]));
                input.Add(new JsonObject { ["type"] = "message", ["role"] = "assistant", ["content"] = joined });
            }
            else if (strictResponseInput)
            {
                var strictContent = new JsonArray();
                foreach (var part in assistantContent)
                {
                    var copy = part!.DeepClone().AsObject();
                    if ((string?)copy["type"] == "output_text")
                    {
                        copy["annotations"] ??= new JsonArray();
                        copy["logprobs"] ??= new JsonArray();
                    }

                    strictContent.Add(copy);
                }

                input.Add(new JsonObject
                {
                    ["id"] = assistantMessageId,
                    ["type"] = "message",
                    ["status"] = "completed",
                    ["role"] = "assistant",
                    ["content"] = strictContent,
                });
            }
            else
            {
                var item = new JsonObject { ["type"] = "message", ["role"] = "assistant", ["content"] = assistantContent };
                if (assistantMessageId != null)
                {
                    item["id"] = assistantMessageId;
                }

                input.Add(item);
            }

            assistantContent = new JsonArray();
            assistantMessageId = null;
        }

        foreach (var part in AssistantParts(assistant))
        {
            var replay = GetExtensionReplay(part.ProviderOptions, providerOptionsName, extensionRegistry);
            if (replay != null)
            {
                Flush();
                AddReplayItem(replay.Item, input, replayedExtensionItems);
                continue;
            }

            if (part is AssistantToolCallPart || part is AssistantToolResultPart)
            {
                var toolName = part is AssistantToolCallPart call ? call.ToolName : ((AssistantToolResultPart)part).ToolName;
                if (TryEncodeProviderToolPart(part, toolName, providerToolsByName, extensionRegistry, out var providerTool, out var encoded))
                {
                    Flush();
                    if (encoded == null)
                    {
                        warnings.Add(new CallWarning("unsupported", "provider-defined tool " + providerTool!.ProviderToolId + " " + part.Type + " history"));
                    }
                    else
                    {
                        foreach (var item in encoded)
                        {
                            input.Add(item);
                        }
                    }

                    continue;
                }
            }

            switch (part)
            {
                case AssistantReasoningPart reasoning:
                    Flush();
                    AddReasoning(input, reasoning, providerOptionsName);
                    break;

                case AssistantTextPart text:
                {
                    var data = ProviderData(text.ProviderOptions, providerOptionsName);
                    var itemId = GetString(data, "itemId");
                    var annotations = ParseAnnotations(data?["annotations"]);
                    if (assistantContent.Count > 0 && assistantMessageId != itemId)
                    {
                        Flush();
                    }

                    assistantMessageId = itemId;
                    var output = new JsonObject { ["type"] = "output_text", ["text"] = text.Text };
                    if (annotations != null)
                    {
                        output["annotations"] = annotations;
                    }

                    assistantContent.Add(output);
                    break;
                }

                case AssistantToolCallPart toolCall:
                {
                    Flush();
                    var itemId = GetString(ProviderData(toolCall.ProviderOptions, providerOptionsName), "itemId");
                    var item = new JsonObject();
                    item["type"] = "function_call";
                    if (itemId != null)
                    {
                        item["id"] = itemId;
                    }

                    item["call_id"] = toolCall.ToolCallId;
                    item["name"] = toolCall.ToolName;
                    item["arguments"] = toolCall.InputJson;
                    input.Add(item);
                    break;
                }
            }
        }

        Flush();
    }

    private static void AddReasoning(JsonArray input, AssistantReasoningPart part, string providerOptionsName)
    {
        var data = ProviderData(part.ProviderOptions, providerOptionsName);
        var itemId = GetString(data, "itemId");
        var summary = ParseTextParts(data?["reasoningSummary"], "summary_text");
        var content = ParseTextParts(data?["reasoningContent"], "reasoning_text");
        var hasReasoningContent = data != null && data.ContainsKey("reasoningContent");
        var encrypted = GetString(data, "reasoningEncryptedContent");

        var item = new JsonObject { ["type"] = "reasoning", ["summary"] = summary ?? new JsonArray() };
        if (itemId != null)
        {
            item["id"] = itemId;
        }

        if (content != null)
        {
            item["content"] = content;
        }
        else if (!hasReasoningContent && part.Text.Length > 0)
        {
            item["content"] = new JsonArray(new JsonObject { ["type"] = "reasoning_text", ["text"] = part.Text });
        }

        if (encrypted != null)
        {
            item["encrypted_content"] = encrypted;
        }

        var previous = input.Count > 0 ? input[input.Count - 1] as JsonObject : null;
        if (itemId != null && previous != null && (string?)previous["type"] == "reasoning" && (string?)previous["id"] == itemId)
        {
            if (item["content"] is JsonArray more)
            {
                var merged = previous["content"] is JsonArray existing ? existing.DeepClone().AsArray() : new JsonArray();
                foreach (var entry in more)
                {
                    merged.Add(entry!.DeepClone());
                }

                previous["content"] = merged;
            }
        }
        else
        {
            input.Add(item);
        }
    }

    private static void ConvertTool(
        ToolModelMessage tool,
        JsonArray input,
        List<CallWarning> warnings,
        HashSet<string> replayedExtensionItems,
        string providerOptionsName,
        OpenResponsesExtensionRegistry? extensionRegistry,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName)
    {
        Dictionary<string, JsonElement>? metadata = null;
        if (tool.ProviderMetadata is { ValueKind: JsonValueKind.Object } bag)
        {
            metadata = bag.EnumerateObject().ToDictionary(property => property.Name, property => property.Value);
        }

        var replay = GetExtensionReplay(metadata, providerOptionsName, extensionRegistry);
        if (replay != null)
        {
            AddReplayItem(replay.Item, input, replayedExtensionItems);
            return;
        }

        var resultPart = new AssistantToolResultPart(tool.ToolCallId, tool.ToolName, tool);
        if (TryEncodeProviderToolPart(resultPart, tool.ToolName, providerToolsByName, extensionRegistry, out var providerTool, out var encoded))
        {
            if (encoded == null)
            {
                warnings.Add(new CallWarning("unsupported", "provider-defined tool " + providerTool!.ProviderToolId + " tool-result history"));
            }
            else
            {
                foreach (var item in encoded)
                {
                    input.Add(item);
                }
            }

            return;
        }

        string output;
        switch (tool.OutputType)
        {
            case "execution-denied":
                output = string.IsNullOrEmpty(tool.OutputJson) ? "Tool call execution denied." : tool.OutputJson;
                break;
            default:
                output = tool.OutputJson;
                break;
        }

        input.Add(new JsonObject { ["type"] = "function_call_output", ["call_id"] = tool.ToolCallId, ["output"] = output });
    }

    private static bool TryEncodeProviderToolPart(
        AssistantContentPart part,
        string toolName,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName,
        OpenResponsesExtensionRegistry? extensionRegistry,
        out LanguageModelTool? providerTool,
        out List<JsonObject>? encoded)
    {
        encoded = null;
        providerTool = null;
        if (!providerToolsByName.TryGetValue(toolName, out providerTool)
            || extensionRegistry == null
            || providerTool.ProviderToolId == null
            || !extensionRegistry.ByProviderToolId.TryGetValue(providerTool.ProviderToolId, out var extension))
        {
            return false;
        }

        if (extension.EncodeInputItem == null || extension.ItemTypes == null)
        {
            return true;
        }

        try
        {
            var items = extension.EncodeInputItem(new OpenResponsesEncodeInputItemContext(part, providerTool));
            if (items == null || items.Count == 0)
            {
                return true;
            }

            foreach (var item in items)
            {
                if (item == null || !OpenResponsesJson.IsExtensionItem(item) || !extension.ItemTypes.Contains((string)item["type"]!))
                {
                    return true;
                }
            }

            encoded = items.Select(item => item.DeepClone().AsObject()).ToList();
        }
        catch (Exception)
        {
            encoded = null;
        }

        return true;
    }

    private static void AddReplayItem(JsonObject? item, JsonArray input, HashSet<string> replayed)
    {
        if (item == null)
        {
            return;
        }

        var key = (string)item["type"]! + ":" + (string)item["id"]!;
        if (replayed.Add(key))
        {
            input.Add(item.DeepClone());
        }
    }

    private sealed class ExtensionReplay
    {
        public JsonObject? Item { get; set; }
    }

    private static ExtensionReplay? GetExtensionReplay(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName,
        OpenResponsesExtensionRegistry? extensionRegistry)
    {
        var extensionData = ProviderData(providerOptions, providerOptionsName)?["openResponsesExtension"] as JsonObject;
        if (extensionData == null || extensionRegistry == null)
        {
            return null;
        }

        var id = GetString(extensionData, "id");
        if (id == null || !extensionRegistry.ByExtensionId.TryGetValue(id, out var extension))
        {
            return null;
        }

        if (extensionData["item"] is JsonObject item
            && OpenResponsesJson.IsExtensionItem(item)
            && extension.ItemTypes != null
            && extension.ItemTypes.Contains((string)item["type"]!))
        {
            return new ExtensionReplay { Item = item };
        }

        return GetString(extensionData, "itemId") != null ? new ExtensionReplay() : null;
    }

    /// <summary>The ordered parts of an assistant message, built from the flat fields when the message has no parts.</summary>
    private static IReadOnlyList<AssistantContentPart> AssistantParts(AssistantModelMessage assistant)
    {
        if (assistant.Parts != null)
        {
            return assistant.Parts;
        }

        var parts = new List<AssistantContentPart>();
        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            parts.Add(new AssistantReasoningPart(assistant.Reasoning!, assistant.ReasoningProviderOptions));
        }

        if (!string.IsNullOrEmpty(assistant.Text))
        {
            parts.Add(new AssistantTextPart(assistant.Text!));
        }

        foreach (var call in assistant.ToolCalls)
        {
            Dictionary<string, JsonElement>? options = null;
            if (call.ProviderMetadata is { ValueKind: JsonValueKind.Object } bag)
            {
                options = bag.EnumerateObject().ToDictionary(property => property.Name, property => property.Value);
            }

            parts.Add(new AssistantToolCallPart(call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderExecuted, options));
        }

        return parts;
    }

    private static JsonObject? ProviderData(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        if (providerOptions != null
            && providerOptions.TryGetValue(providerOptionsName, out var value)
            && value.ValueKind == JsonValueKind.Object)
        {
            return JsonNode.Parse(value.GetRawText())!.AsObject();
        }

        return null;
    }

    private static string? GetString(JsonObject? data, string name)
    {
        if (data != null && data.TryGetPropertyValue(name, out var node) && node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static string ImageDetail(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        var detail = GetString(ProviderData(providerOptions, providerOptionsName), "imageDetail");
        return detail == "low" || detail == "high" || detail == "auto" ? detail : "auto";
    }

    private static JsonArray? ParseTextParts(JsonNode? value, string type)
    {
        if (value is not JsonArray array)
        {
            return null;
        }

        var result = new JsonArray();
        foreach (var entry in array)
        {
            if (entry is not JsonObject item || GetString(item, "type") != type || GetString(item, "text") == null)
            {
                return null;
            }

            result.Add(new JsonObject { ["type"] = type, ["text"] = GetString(item, "text") });
        }

        return result;
    }

    private static JsonArray? ParseAnnotations(JsonNode? value)
    {
        if (value is not JsonArray array)
        {
            return null;
        }

        var result = new JsonArray();
        foreach (var entry in array)
        {
            if (entry is not JsonObject item
                || GetString(item, "type") != "url_citation"
                || !IsNumber(item["start_index"])
                || !IsNumber(item["end_index"])
                || GetString(item, "url") == null
                || GetString(item, "title") == null)
            {
                return null;
            }

            result.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = item["start_index"]!.DeepClone(),
                ["end_index"] = item["end_index"]!.DeepClone(),
                ["url"] = GetString(item, "url"),
                ["title"] = GetString(item, "title"),
            });
        }

        return result;
    }

    private static bool IsNumber(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<double>(out _);
    }
}

/// <summary>JSON shape checks for Open Responses extension records.</summary>
internal static class OpenResponsesJson
{
    public static bool IsNamespacedType(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) && text.Contains(':');
    }

    public static bool IsExtensionRecord(JsonObject value)
    {
        return IsNamespacedType(value["type"]);
    }

    public static bool IsExtensionItem(JsonObject value)
    {
        return IsExtensionRecord(value) && IsString(value["id"]) && IsString(value["status"]);
    }

    public static bool IsExtensionEvent(JsonObject value)
    {
        return IsExtensionRecord(value) && value["sequence_number"] is JsonValue number && number.TryGetValue<double>(out _);
    }

    private static bool IsString(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out _);
    }
}
