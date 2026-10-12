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
                    input.Add(new JsonObject { ["type"] = "message", ["role"] = "user", ["content"] = ConvertUser(user, providerOptionsName) });
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

    private static void ConvertAssistant(AssistantModelMessage assistant, JsonArray input, string providerOptionsName, bool strictResponseInput)
    {
        var reasoningData = ProviderData(assistant.ReasoningProviderOptions, providerOptionsName);
        if (!string.IsNullOrEmpty(assistant.Reasoning) || reasoningData != null)
        {
            input.Add(ReasoningItem(assistant.Reasoning ?? string.Empty, reasoningData));
        }

        if (!string.IsNullOrEmpty(assistant.Text))
        {
            // Assistant text carries no item id, so strict mode sends an easy input message.
            input.Add(strictResponseInput
                ? new JsonObject { ["type"] = "message", ["role"] = "assistant", ["content"] = assistant.Text }
                : new JsonObject
                {
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["content"] = new JsonArray(new JsonObject { ["type"] = "output_text", ["text"] = assistant.Text }),
                });
        }

        foreach (var call in assistant.ToolCalls)
        {
            var item = new JsonObject { ["type"] = "function_call" };
            if (ItemId(ProviderData(call.ProviderMetadata, providerOptionsName)) is { } itemId)
            {
                item["id"] = itemId;
            }

            item["call_id"] = call.ToolCallId;
            item["name"] = call.ToolName;
            item["arguments"] = call.ArgumentsJson;
            input.Add(item);
        }
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
}
