// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenResponses;

/// <summary>The converted prompt: input items, joined system instructions, and warnings.</summary>
internal sealed class OpenResponsesInputResult
{
    public OpenResponsesInputResult(JsonArray input, string? instructions, List<CallWarning> warnings)
    {
        Input = input;
        Instructions = instructions;
        Warnings = warnings;
    }

    /// <summary>The <c>input</c> items.</summary>
    public JsonArray Input { get; }

    /// <summary>System messages joined with newlines, or null when there are none.</summary>
    public string? Instructions { get; }

    public List<CallWarning> Warnings { get; }
}

/// <summary>Converts a prompt into Open Responses input items. Maps to <c>convertToOpenResponsesInput</c>.</summary>
internal static class OpenResponsesInput
{
    public static OpenResponsesInputResult Convert(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName = OpenResponsesProvider.ProviderName,
        bool strictResponseInput = false,
        OpenResponsesExtensionRegistry? extensionRegistry = null,
        IReadOnlyDictionary<string, LanguageModelTool>? providerToolsByName = null)
    {
        providerToolsByName ??= new Dictionary<string, LanguageModelTool>(StringComparer.Ordinal);
        var input = new JsonArray();
        var warnings = new List<CallWarning>();
        var systemMessages = new List<string>();
        var replayedExtensionItems = new HashSet<string>(StringComparer.Ordinal);

        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            switch (message)
            {
                case SystemModelMessage system:
                    systemMessages.Add(system.Content);
                    break;
                case UserModelMessage user:
                    input.Add(new JsonObject { ["type"] = "message", ["role"] = "user", ["content"] = ConvertUser(user, providerOptionsName) });
                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, input, warnings, replayedExtensionItems, providerOptionsName, strictResponseInput, extensionRegistry, providerToolsByName);
                    break;
                case ToolModelMessage tool:
                    ConvertTool(tool, input, warnings, replayedExtensionItems, providerOptionsName, extensionRegistry, providerToolsByName);
                    break;
            }
        }

        return new OpenResponsesInputResult(input, systemMessages.Count > 0 ? string.Join("\n", systemMessages) : null, warnings);
    }

    private static JsonArray ConvertUser(UserModelMessage user, string providerOptionsName)
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

        return content;
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
                ["detail"] = ImageDetail(file.ProviderOptions, providerOptionsName),
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
        var data = file.Data ?? Array.Empty<byte>();
        return "data:" + MediaTypes.ResolveFullMediaType(file.MediaType, data) + ";base64," + System.Convert.ToBase64String(data);
    }

    private static string ImageDetail(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        var detail = ProviderData(providerOptions, providerOptionsName) is { } data
            && data.TryGetProperty("imageDetail", out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        return detail == "low" || detail == "high" || detail == "auto" ? detail : "auto";
    }

    private static void ConvertAssistant(
        AssistantModelMessage assistant,
        JsonArray input,
        List<CallWarning> warnings,
        HashSet<string> replayedExtensionItems,
        string providerOptionsName,
        bool strictResponseInput,
        OpenResponsesExtensionRegistry? extensionRegistry,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName)
    {
        var content = new JsonArray();
        string? messageId = null;

        void Flush()
        {
            if (content.Count == 0)
            {
                return;
            }

            if (strictResponseInput && messageId == null)
            {
                // Assistant text without an item id is sent as an easy input message.
                input.Add(new JsonObject
                {
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["content"] = string.Concat(content.Select(part => (string?)part!["text"])),
                });
            }
            else if (strictResponseInput)
            {
                foreach (var part in content)
                {
                    part!["annotations"] ??= new JsonArray();
                    part["logprobs"] ??= new JsonArray();
                }

                input.Add(new JsonObject
                {
                    ["id"] = messageId,
                    ["type"] = "message",
                    ["status"] = "completed",
                    ["role"] = "assistant",
                    ["content"] = content,
                });
            }
            else
            {
                var item = new JsonObject { ["type"] = "message", ["role"] = "assistant", ["content"] = content };
                if (messageId != null)
                {
                    item["id"] = messageId;
                }

                input.Add(item);
            }

            content = new JsonArray();
            messageId = null;
        }

        foreach (var part in assistant.Parts)
        {
            var providerData = ProviderData(part.ProviderOptions, providerOptionsName);
            if (GetExtensionReplay(providerData, extensionRegistry) is { } replay)
            {
                Flush();
                AddReplayItem(replay.Item, input, replayedExtensionItems);
                continue;
            }

            var toolName = part switch
            {
                AssistantToolCallPart toolCallPart => toolCallPart.ToolCall.ToolName,
                AssistantToolResultPart toolResultPart => toolResultPart.ToolName,
                _ => null,
            };
            if (toolName != null && TryEncodeProviderToolPart(part, toolName, providerToolsByName, extensionRegistry, input, warnings))
            {
                Flush();
                continue;
            }

            switch (part)
            {
                case AssistantReasoningPart reasoning when reasoning.Text.Length > 0 || providerData != null:
                    Flush();
                    AddReasoning(input, ReasoningItem(reasoning.Text, providerData));
                    break;
                case AssistantTextPart text:
                    var itemId = ItemId(providerData);
                    if (content.Count > 0 && messageId != itemId)
                    {
                        Flush();
                    }

                    messageId = itemId;
                    var outputText = new JsonObject { ["type"] = "output_text", ["text"] = text.Text };
                    if (Annotations(Child(providerData, "annotations")) is { } annotations)
                    {
                        outputText["annotations"] = annotations;
                    }

                    content.Add(outputText);
                    break;
                case AssistantToolCallPart toolCall:
                    Flush();
                    var call = toolCall.ToolCall;
                    var item = new JsonObject { ["type"] = "function_call" };
                    if (ItemId(providerData) is { } callItemId)
                    {
                        item["id"] = callItemId;
                    }

                    item["call_id"] = call.ToolCallId;
                    item["name"] = call.ToolName;
                    item["arguments"] = call.ArgumentsJson;
                    input.Add(item);
                    break;
            }
        }

        Flush();
    }

    /// <summary>Adds a reasoning item, or appends its content to the previous item when both have the same id.</summary>
    private static void AddReasoning(JsonArray input, JsonObject item)
    {
        if (item["id"] is { } id
            && input.Count > 0
            && input[input.Count - 1] is JsonObject previous
            && (string?)previous["type"] == "reasoning"
            && (string?)previous["id"] == (string?)id)
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

    private static JsonObject ReasoningItem(string text, JsonElement? providerData)
    {
        var item = new JsonObject
        {
            ["type"] = "reasoning",
            ["summary"] = TypedTexts(Child(providerData, "reasoningSummary"), "summary_text") ?? new JsonArray(),
        };
        if (ItemId(providerData) is { } itemId)
        {
            item["id"] = itemId;
        }

        var hasReasoningContent = providerData is { } data && data.TryGetProperty("reasoningContent", out _);
        if (TypedTexts(Child(providerData, "reasoningContent"), "reasoning_text") is { } content)
        {
            item["content"] = content;
        }
        else if (!hasReasoningContent && text.Length > 0)
        {
            item["content"] = new JsonArray(new JsonObject { ["type"] = "reasoning_text", ["text"] = text });
        }

        if (Child(providerData, "reasoningEncryptedContent") is { ValueKind: JsonValueKind.String } encrypted)
        {
            item["encrypted_content"] = encrypted.GetString();
        }

        return item;
    }

    /// <summary>Copies an array whose parts all have <paramref name="type"/> and a string <c>text</c>, or returns null.</summary>
    private static JsonArray? TypedTexts(JsonElement? value, string type)
    {
        if (value is not { ValueKind: JsonValueKind.Array } array)
        {
            return null;
        }

        var parts = new JsonArray();
        foreach (var part in array.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object
                || !part.TryGetProperty("type", out var partType)
                || partType.ValueKind != JsonValueKind.String
                || partType.GetString() != type
                || !part.TryGetProperty("text", out var text)
                || text.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            parts.Add(new JsonObject { ["type"] = type, ["text"] = text.GetString() });
        }

        return parts;
    }

    /// <summary>Copies the <c>url_citation</c> annotations, or returns null when any annotation is not a valid citation.</summary>
    private static JsonArray? Annotations(JsonElement? value)
    {
        if (value is not { ValueKind: JsonValueKind.Array } array)
        {
            return null;
        }

        var annotations = new JsonArray();
        foreach (var annotation in array.EnumerateArray())
        {
            if (Child(annotation, "type") is not { ValueKind: JsonValueKind.String } type
                || type.GetString() != "url_citation"
                || Child(annotation, "start_index") is not { ValueKind: JsonValueKind.Number } start
                || Child(annotation, "end_index") is not { ValueKind: JsonValueKind.Number } end
                || Child(annotation, "url") is not { ValueKind: JsonValueKind.String } url
                || Child(annotation, "title") is not { ValueKind: JsonValueKind.String } title)
            {
                return null;
            }

            annotations.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = JsonNode.Parse(start.GetRawText()),
                ["end_index"] = JsonNode.Parse(end.GetRawText()),
                ["url"] = url.GetString(),
                ["title"] = title.GetString(),
            });
        }

        return annotations;
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
        if (GetExtensionReplay(ProviderData(tool.ProviderMetadata, providerOptionsName), extensionRegistry) is { } replay)
        {
            AddReplayItem(replay.Item, input, replayedExtensionItems);
            return;
        }

        var resultPart = new AssistantToolResultPart(tool.ToolCallId, tool.ToolName, tool);
        if (TryEncodeProviderToolPart(resultPart, tool.ToolName, providerToolsByName, extensionRegistry, input, warnings))
        {
            return;
        }

        input.Add(new JsonObject
        {
            ["type"] = "function_call_output",
            ["call_id"] = tool.ToolCallId,
            ["output"] = ToolOutput(tool),
        });
    }

    /// <summary>
    /// Encodes a tool call or tool result of a provider-defined tool through its extension and adds the items. Returns false
    /// when the part does not belong to a registered provider tool. A part that the extension cannot encode is dropped with a warning.
    /// </summary>
    private static bool TryEncodeProviderToolPart(
        AssistantContentPart part,
        string toolName,
        IReadOnlyDictionary<string, LanguageModelTool> providerToolsByName,
        OpenResponsesExtensionRegistry? extensionRegistry,
        JsonArray input,
        List<CallWarning> warnings)
    {
        if (!providerToolsByName.TryGetValue(toolName, out var providerTool)
            || extensionRegistry == null
            || providerTool.ProviderToolId == null
            || !extensionRegistry.ByProviderToolId.TryGetValue(providerTool.ProviderToolId, out var extension))
        {
            return false;
        }

        List<JsonObject>? encoded = null;
        if (extension.EncodeInputItem != null && extension.ItemTypes != null)
        {
            try
            {
                var items = extension.EncodeInputItem(new OpenResponsesEncodeInputItemContext(part, providerTool));
                if (items != null
                    && items.Count > 0
                    && items.All(item => item != null && OpenResponsesJson.IsExtensionItem(item) && extension.ItemTypes.Contains((string)item["type"]!)))
                {
                    encoded = items.Select(item => item.DeepClone().AsObject()).ToList();
                }
            }
            catch (Exception)
            {
                // Encoding failures are reported as unsupported below.
            }
        }

        if (encoded == null)
        {
            warnings.Add(new CallWarning("unsupported", "provider-defined tool " + providerTool.ProviderToolId + " " + part.Type + " history", null));
            return true;
        }

        foreach (var item in encoded)
        {
            input.Add(item);
        }

        return true;
    }

    /// <summary>Adds an original extension item once per prompt.</summary>
    private static void AddReplayItem(JsonObject? item, JsonArray input, HashSet<string> replayed)
    {
        if (item != null && replayed.Add((string)item["type"]! + ":" + (string)item["id"]!))
        {
            input.Add(item.DeepClone());
        }
    }

    /// <summary>
    /// Reads the <c>openResponsesExtension</c> reference of a part. Returns null when the part has none or its extension is not
    /// registered. The result holds the original item when the part stores it, and no item when it only references one.
    /// </summary>
    private static ExtensionReplay? GetExtensionReplay(JsonElement? providerData, OpenResponsesExtensionRegistry? extensionRegistry)
    {
        if (extensionRegistry == null
            || Child(providerData, "openResponsesExtension") is not { ValueKind: JsonValueKind.Object } extensionData
            || Child(extensionData, "id") is not { ValueKind: JsonValueKind.String } id
            || !extensionRegistry.ByExtensionId.TryGetValue(id.GetString()!, out var extension))
        {
            return null;
        }

        if (Child(extensionData, "item") is { ValueKind: JsonValueKind.Object } itemElement
            && JsonNode.Parse(itemElement.GetRawText()) is JsonObject item
            && OpenResponsesJson.IsExtensionItem(item)
            && extension.ItemTypes != null
            && extension.ItemTypes.Contains((string)item["type"]!))
        {
            return new ExtensionReplay(item);
        }

        return Child(extensionData, "itemId") is { ValueKind: JsonValueKind.String } ? new ExtensionReplay(null) : null;
    }

    private static JsonNode ToolOutput(ToolModelMessage tool)
    {
        return tool.OutputType == "execution-denied" && string.IsNullOrEmpty(tool.OutputJson)
            ? "Tool call execution denied."
            : tool.OutputJson;
    }

    private static string? ItemId(JsonElement? providerData)
    {
        return Child(providerData, "itemId") is { ValueKind: JsonValueKind.String } itemId ? itemId.GetString() : null;
    }

    private static JsonElement? Child(JsonElement? element, string name)
    {
        return element is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var child) ? child : null;
    }

    private static JsonElement? ProviderData(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        return providerOptions != null
            && providerOptions.TryGetValue(providerOptionsName, out var data)
            && data.ValueKind == JsonValueKind.Object
                ? data
                : null;
    }

    private static JsonElement? ProviderData(JsonElement? providerMetadata, string providerOptionsName)
    {
        return Child(providerMetadata, providerOptionsName) is { ValueKind: JsonValueKind.Object } data ? data : null;
    }

    private sealed class ExtensionReplay
    {
        public ExtensionReplay(JsonObject? item)
        {
            Item = item;
        }

        public JsonObject? Item { get; }
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
