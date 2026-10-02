// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic <c>system</c> and <c>messages</c> produced from an SDK prompt.</summary>
public sealed class AnthropicPromptResult
{
    /// <summary>Creates a converted prompt.</summary>
    public AnthropicPromptResult(JsonArray? system, JsonArray messages, IReadOnlyList<string> betas, IReadOnlyList<AnthropicWarning> warnings)
    {
        System = system;
        Messages = messages;
        Betas = betas;
        Warnings = warnings;
    }

    /// <summary>Top-level system blocks. Null when the prompt has no system text.</summary>
    public JsonArray? System { get; }

    /// <summary>Conversation messages.</summary>
    public JsonArray Messages { get; }

    /// <summary>Betas required by the prompt.</summary>
    public IReadOnlyList<string> Betas { get; }

    /// <summary>Warnings produced while converting.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }
}

/// <summary>Converts SDK prompt messages into Anthropic message blocks.</summary>
public static class AnthropicPrompt
{
    /// <summary>Converts a prompt JSON array that uses the SDK message shape.</summary>
    public static AnthropicPromptResult Convert(
        JsonElement prompt,
        bool sendReasoning = true,
        AnthropicCacheControlValidator? cacheControl = null,
        IReadOnlyDictionary<string, string>? providerToolNames = null,
        IReadOnlyDictionary<string, string>? toolsetNames = null)
    {
        var warnings = new List<AnthropicWarning>();
        var betas = new List<string>();
        var ownsValidator = cacheControl == null;
        var validator = cacheControl ?? new AnthropicCacheControlValidator();
        var names = providerToolNames ?? new Dictionary<string, string>();
        var toolsets = toolsetNames ?? new Dictionary<string, string>();
        var blocks = Group(prompt);
        JsonArray? system = null;
        var messages = new JsonArray();

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var isLast = i == blocks.Count - 1;
            if (block.Role == "system")
            {
                ConvertSystem(block, i == 0, ref system, messages, warnings, betas, validator, names);
            }
            else if (block.Role == "user")
            {
                messages.Add(new JsonObject { ["role"] = "user", ["content"] = ConvertUser(block, warnings, betas, validator) });
            }
            else
            {
                var content = ConvertAssistant(block, isLast, sendReasoning, warnings, betas, validator, names, toolsets);
                if (content.Count > 0)
                {
                    messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content });
                }
            }
        }

        if (ownsValidator)
        {
            foreach (var warning in validator.Warnings)
            {
                warnings.Add(warning);
            }
        }

        return new AnthropicPromptResult(system, messages, betas, warnings);
    }

    private static void ConvertSystem(
        PromptBlock block,
        bool hoist,
        ref JsonArray? system,
        JsonArray messages,
        List<AnthropicWarning> warnings,
        List<string> betas,
        AnthropicCacheControlValidator validator,
        IReadOnlyDictionary<string, string> names)
    {
        var converted = new List<JsonObject>();
        var toolChangeCount = 0;
        var hasInlineOptions = false;
        foreach (var message in block.Messages)
        {
            var text = message.ValueKind == JsonValueKind.String
                ? message.GetString() ?? string.Empty
                : AnthropicJson.String(message, "content") ?? string.Empty;
            var options = Provider(message);
            var toolChanges = options == null ? default : AnthropicJson.Property(options.Value, "toolChanges");
            var clearAt = options == null ? null : AnthropicJson.String(options.Value, "clearAt");
            var effort = options == null ? null : AnthropicJson.String(options.Value, "effort");
            var changes = toolChanges != null && toolChanges.Value.ValueKind == JsonValueKind.Array ? toolChanges.Value.GetArrayLength() : 0;
            toolChangeCount += changes;
            if (clearAt != null || effort != null)
            {
                hasInlineOptions = true;
            }

            var content = new JsonArray();
            if (text.Length > 0 || (changes == 0 && clearAt == null && effort == null))
            {
                var part = new JsonObject { ["type"] = "text", ["text"] = text };
                AnthropicJson.Set(part, "cache_control", validator.Get(Metadata(message), "system message", true));
                content.Add(part);
            }

            if (toolChanges != null && toolChanges.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var change in toolChanges.Value.EnumerateArray())
                {
                    var toolName = AnthropicJson.String(change, "toolName") ?? string.Empty;
                    content.Add(new JsonObject
                    {
                        ["type"] = AnthropicJson.String(change, "type"),
                        ["tool"] = new JsonObject { ["type"] = "tool_reference", ["name"] = ToProviderName(names, toolName) },
                    });
                }
            }

            var node = new JsonObject { ["content"] = content };
            AnthropicJson.Set(node, "clear_at", clearAt);
            if (effort != null)
            {
                node["output_config"] = new JsonObject { ["effort"] = effort };
            }

            converted.Add(node);
        }

        var shouldHoist = hoist && toolChangeCount == 0 && !hasInlineOptions;
        if (!shouldHoist && system == null && toolChangeCount == 0 && !hasInlineOptions)
        {
            shouldHoist = true;
        }

        if (IsInitial(hoist, system, toolChangeCount, hasInlineOptions))
        {
            if (toolChangeCount > 0)
            {
                warnings.Add(new AnthropicWarning(
                    "other",
                    message: "tool changes on the initial system message are not supported by Anthropic. Configure the initial tool set via the tools option instead. The tool changes have been ignored."));
            }

            if (hasInlineOptions)
            {
                warnings.Add(new AnthropicWarning(
                    "other",
                    message: "clearAt and effort on the initial system message are not supported by Anthropic. These options have been ignored."));
            }

            var parts = system ?? new JsonArray();
            foreach (var message in converted)
            {
                if (message["content"] is JsonArray content)
                {
                    foreach (var part in content)
                    {
                        if (part is JsonObject obj && obj["type"]?.GetValue<string>() == "text")
                        {
                            parts.Add(obj.DeepClone());
                        }
                    }
                }
            }

            system = parts.Count == 0 ? system : parts;
            return;
        }

        AddBeta(betas, "mid-conversation-system-2026-04-07");
        foreach (var message in converted)
        {
            var inline = new JsonObject { ["role"] = "system", ["content"] = message["content"]!.DeepClone() };
            if (message["clear_at"] != null)
            {
                inline["clear_at"] = message["clear_at"]!.DeepClone();
                AddBeta(betas, "mid-conversation-system-clear-at-2026-08-21");
            }

            if (message["output_config"] != null)
            {
                inline["output_config"] = message["output_config"]!.DeepClone();
                AddBeta(betas, "mid-conversation-output-config-2026-07-01");
            }

            messages.Add(inline);
            if (message["content"] is JsonArray content)
            {
                foreach (var part in content)
                {
                    var type = part?["type"]?.GetValue<string>();
                    if (type == "tool_addition" || type == "tool_removal")
                    {
                        AddBeta(betas, "mid-conversation-tool-changes-2026-07-01");
                        break;
                    }
                }
            }
        }
    }

    private static bool IsInitial(bool firstBlock, JsonArray? system, int toolChanges, bool inlineOptions)
    {
        if (firstBlock)
        {
            return true;
        }

        return system == null && toolChanges == 0 && !inlineOptions;
    }

    private static JsonArray ConvertUser(PromptBlock block, List<AnthropicWarning> warnings, List<string> betas, AnthropicCacheControlValidator validator)
    {
        var content = new JsonArray();
        foreach (var message in block.Messages)
        {
            var role = AnthropicJson.String(message, "role") ?? "user";
            if (role == "tool")
            {
                AppendToolResults(message, content, warnings, betas, validator);
                continue;
            }

            if (!message.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var index = 0;
            var count = parts.GetArrayLength();
            foreach (var part in parts.EnumerateArray())
            {
                var isLast = index == count - 1;
                index++;
                var cache = validator.Get(Metadata(part), "user message part", true)
                    ?? (isLast ? validator.Get(Metadata(message), "user message", true) : null);
                var type = AnthropicJson.String(part, "type");
                if (type == "text")
                {
                    var text = new JsonObject { ["type"] = "text", ["text"] = AnthropicJson.String(part, "text") ?? string.Empty };
                    AnthropicJson.Set(text, "cache_control", cache);
                    content.Add(text);
                }
                else if (type == "file")
                {
                    content.Add(ConvertFile(part, cache, betas));
                }
            }
        }

        return content;
    }

    private static void AppendToolResults(JsonElement message, JsonArray content, List<AnthropicWarning> warnings, List<string> betas, AnthropicCacheControlValidator validator)
    {
        if (!message.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        var count = parts.GetArrayLength();
        foreach (var part in parts.EnumerateArray())
        {
            if (AnthropicJson.String(part, "type") == "tool-approval-response")
            {
                index++;
                continue;
            }

            var isLast = index == count - 1;
            index++;
            JsonElement? outputOptions = null;
            if (part.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Object)
            {
                outputOptions = Metadata(output);
                if (outputOptions == null && output.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        outputOptions = Metadata(item);
                        if (outputOptions != null)
                        {
                            break;
                        }
                    }
                }
            }

            var cache = validator.Get(Metadata(part), "tool result part", true)
                ?? validator.Get(outputOptions, "tool result output", true)
                ?? (isLast ? validator.Get(Metadata(message), "tool result", true) : null);
            var result = new JsonObject
            {
                ["type"] = "tool_result",
                ["tool_use_id"] = AnthropicJson.String(part, "toolCallId") ?? string.Empty,
            };
            if (part.TryGetProperty("output", out var body))
            {
                var outputType = AnthropicJson.String(body, "type");
                if (outputType == "error-json" || outputType == "error-text")
                {
                    result["is_error"] = true;
                }

                if (outputType == "content" && body.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
                {
                    var nested = new JsonArray();
                    foreach (var item in value.EnumerateArray())
                    {
                        var itemType = AnthropicJson.String(item, "type");
                        if (itemType == "text")
                        {
                            nested.Add(new JsonObject { ["type"] = "text", ["text"] = AnthropicJson.String(item, "text") ?? string.Empty });
                        }
                        else if (itemType == "file" || itemType == "tool-reference" || itemType == "custom")
                        {
                            if (itemType == "custom" || AnthropicJson.String(item, "type") == "tool-reference")
                            {
                                nested.Add(JsonNode.Parse(item.GetRawText()));
                            }
                            else
                            {
                                nested.Add(ConvertFile(item, null, betas));
                            }
                        }
                    }

                    result["content"] = nested;
                }
                else if (body.TryGetProperty("value", out var jsonValue))
                {
                    result["content"] = jsonValue.ValueKind == JsonValueKind.String ? jsonValue.GetString() : jsonValue.GetRawText();
                }
                else if (AnthropicJson.String(body, "text") != null)
                {
                    result["content"] = AnthropicJson.String(body, "text");
                }
            }

            AnthropicJson.Set(result, "cache_control", cache);
            content.Add(result);
        }
    }

    private static JsonObject ConvertFile(JsonElement part, JsonObject? cache, List<string> betas)
    {
        var mediaType = AnthropicJson.String(part, "mediaType") ?? string.Empty;
        var filename = AnthropicJson.String(part, "filename");
        var options = Provider(part);
        var citations = options != null && AnthropicJson.Property(options.Value, "citations") is { } citation && AnthropicJson.Bool(citation, "enabled") == true;
        var title = options == null ? null : AnthropicJson.String(options.Value, "title");
        var context = options == null ? null : AnthropicJson.String(options.Value, "context");
        var container = options != null && AnthropicJson.Bool(options.Value, "containerUpload") == true;
        if (!part.TryGetProperty("data", out var data))
        {
            throw new AiSdkException("Unsupported media type: " + mediaType);
        }

        var dataType = AnthropicJson.String(data, "type");
        if (dataType == "reference")
        {
            var reference = data.TryGetProperty("reference", out var referenceValue) ? referenceValue : default;
            if (reference.ValueKind != JsonValueKind.Object || !reference.TryGetProperty("anthropic", out var fileId) || fileId.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Provider reference does not contain an anthropic file id.");
            }

            AddBeta(betas, "files-api-2025-04-14");
            if (container)
            {
                return new JsonObject { ["type"] = "container_upload", ["file_id"] = fileId.GetString() };
            }

            var kind = TopLevel(mediaType) == "image" ? "image" : "document";
            var node = new JsonObject
            {
                ["type"] = kind,
                ["source"] = new JsonObject { ["type"] = "file", ["file_id"] = fileId.GetString() },
            };
            AnthropicJson.Set(node, "cache_control", cache);
            return node;
        }

        if (dataType == "text")
        {
            var node = Document(new JsonObject
            {
                ["type"] = "text",
                ["media_type"] = "text/plain",
                ["data"] = AnthropicJson.String(data, "text") ?? string.Empty,
            }, title ?? filename, context, citations, cache);
            return node;
        }

        string? url = null;
        string? base64 = null;
        if (dataType == "url")
        {
            url = data.TryGetProperty("url", out var urlValue) ? urlValue.ToString().Trim('"') : null;
        }
        else
        {
            base64 = data.TryGetProperty("data", out var raw) ? raw.ValueKind == JsonValueKind.String ? raw.GetString() : raw.GetRawText() : null;
            if (base64 != null && AnthropicJson.LooksLikeUrl(base64))
            {
                url = base64;
                base64 = null;
            }
        }

        var top = TopLevel(mediaType);
        if (top == "image")
        {
            var source = url != null
                ? new JsonObject { ["type"] = "url", ["url"] = url }
                : new JsonObject { ["type"] = "base64", ["media_type"] = ResolveMedia(mediaType, base64), ["data"] = base64 ?? string.Empty };
            var image = new JsonObject { ["type"] = "image", ["source"] = source };
            AnthropicJson.Set(image, "cache_control", cache);
            return image;
        }

        var resolved = url != null ? mediaType : ResolveMedia(mediaType, base64);
        if (top == "application" && (url != null ? mediaType == "application/pdf" : resolved == "application/pdf"))
        {
            AddBeta(betas, "pdfs-2024-09-25");
            var source = url != null
                ? new JsonObject { ["type"] = "url", ["url"] = url }
                : new JsonObject { ["type"] = "base64", ["media_type"] = "application/pdf", ["data"] = base64 ?? string.Empty };
            return Document(source, title ?? filename, context, citations, cache);
        }

        if (mediaType == "text/plain")
        {
            JsonObject source;
            if (url != null)
            {
                source = new JsonObject { ["type"] = "url", ["url"] = url };
            }
            else
            {
                source = new JsonObject { ["type"] = "text", ["media_type"] = "text/plain", ["data"] = DecodeText(base64) };
            }

            return Document(source, title ?? filename, context, citations, cache);
        }

        throw new AiSdkException("Unsupported functionality: media type: " + mediaType);
    }

    private static JsonObject Document(JsonObject source, string? title, string? context, bool citations, JsonObject? cache)
    {
        var node = new JsonObject { ["type"] = "document", ["source"] = source };
        AnthropicJson.Set(node, "title", title);
        AnthropicJson.Set(node, "context", context);
        if (citations)
        {
            node["citations"] = new JsonObject { ["enabled"] = true };
        }

        AnthropicJson.Set(node, "cache_control", cache);
        return node;
    }

    private static JsonArray ConvertAssistant(
        PromptBlock block,
        bool isLastBlock,
        bool sendReasoning,
        List<AnthropicWarning> warnings,
        List<string> betas,
        AnthropicCacheControlValidator validator,
        IReadOnlyDictionary<string, string> names,
        IReadOnlyDictionary<string, string> toolsets)
    {
        var content = new JsonArray();
        for (var j = 0; j < block.Messages.Count; j++)
        {
            var message = block.Messages[j];
            var isLastMessage = j == block.Messages.Count - 1;
            if (!message.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var index = 0;
            var count = parts.GetArrayLength();
            foreach (var part in parts.EnumerateArray())
            {
                var isLastPart = index == count - 1;
                index++;
                var type = AnthropicJson.String(part, "type");
                JsonObject? cache = null;
                if (type != "reasoning")
                {
                    cache = validator.Get(Metadata(part), "assistant message part", true)
                        ?? (isLastPart ? validator.Get(Metadata(message), "assistant message", true) : null);
                }
                if (type == "text")
                {
                    var metadata = Provider(part);
                    var metaType = metadata == null ? null : AnthropicJson.String(metadata.Value, "type");
                    if (metaType == "compaction")
                    {
                        var text = AnthropicJson.String(part, "text") ?? string.Empty;
                        if (text.Length == 0)
                        {
                            continue;
                        }

                        var compaction = new JsonObject { ["type"] = "compaction", ["content"] = text };
                        AnthropicJson.Set(compaction, "cache_control", cache);
                        content.Add(compaction);
                    }
                    else
                    {
                        var text = AnthropicJson.String(part, "text") ?? string.Empty;
                        if (isLastBlock && isLastMessage && isLastPart)
                        {
                            text = text.Trim();
                        }

                        var node = new JsonObject { ["type"] = "text", ["text"] = text };
                        if (metadata != null && AnthropicJson.Property(metadata.Value, "citations") is { } citations)
                        {
                            node["citations"] = AnthropicJson.Node(citations);
                        }

                        AnthropicJson.Set(node, "cache_control", cache);
                        content.Add(node);
                    }
                }
                else if (type == "reasoning")
                {
                    if (!sendReasoning)
                    {
                        warnings.Add(new AnthropicWarning("other", message: "sending reasoning content is disabled for this model"));
                        continue;
                    }

                    var metadata = Provider(part);
                    var signature = metadata == null ? null : AnthropicJson.String(metadata.Value, "signature");
                    var redacted = metadata == null ? null : AnthropicJson.String(metadata.Value, "redactedData");
                    if (signature != null)
                    {
                        validator.Get(Metadata(part), "thinking block", false);
                        content.Add(new JsonObject
                        {
                            ["type"] = "thinking",
                            ["thinking"] = AnthropicJson.String(part, "text") ?? string.Empty,
                            ["signature"] = signature,
                        });
                    }
                    else if (redacted != null)
                    {
                        validator.Get(Metadata(part), "redacted thinking block", false);
                        content.Add(new JsonObject { ["type"] = "redacted_thinking", ["data"] = redacted });
                    }
                    else
                    {
                        warnings.Add(new AnthropicWarning("other", message: "unsupported reasoning metadata"));
                    }
                }
                else if (type == "tool-call")
                {
                    content.Add(ConvertToolCall(part, cache, warnings, names, toolsets));
                }
            }
        }

        return MoveToolUses(content);
    }

    private static JsonObject ConvertToolCall(
        JsonElement part,
        JsonObject? cache,
        List<AnthropicWarning> warnings,
        IReadOnlyDictionary<string, string> names,
        IReadOnlyDictionary<string, string> toolsets)
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
        var input = ToToolInput(part);
        var metadata = Provider(part);
        var toolset = metadata == null ? null : AnthropicJson.String(metadata.Value, "toolsetName");
        if (toolset == null && toolsets.TryGetValue(toolName, out var mappedToolset))
        {
            toolset = mappedToolset;
        }

        if (toolset != null && input is JsonObject member)
        {
            var action = member["action"]?.GetValue<string>();
            if (string.IsNullOrEmpty(action))
            {
                warnings.Add(new AnthropicWarning("other", message: "toolset tool call for tool " + toolName + " is missing the action"));
                return new JsonObject { ["type"] = "text", ["text"] = string.Empty };
            }

            member.Remove("action");
            var toolsetCall = new JsonObject
            {
                ["type"] = "tool_use",
                ["id"] = id,
                ["name"] = action,
                ["toolset_name"] = toolset,
                ["input"] = member,
            };
            AnthropicJson.Set(toolsetCall, "cache_control", cache);
            return toolsetCall;
        }

        var call = new JsonObject
        {
            ["type"] = "tool_use",
            ["id"] = id,
            ["name"] = ToProviderName(names, toolName),
            ["input"] = input ?? new JsonObject(),
        };
        AnthropicJson.Set(call, "cache_control", cache);
        return call;
    }

    private static JsonNode ToToolInput(JsonElement part)
    {
        if (!part.TryGetProperty("input", out var input))
        {
            return new JsonObject();
        }

        if (input.ValueKind == JsonValueKind.Object)
        {
            return AnthropicJson.Node(input);
        }

        if (input.ValueKind == JsonValueKind.String)
        {
            var text = input.GetString() ?? string.Empty;
            if (text.Length > 0 && text[0] == '{')
            {
                try
                {
                    var parsed = JsonNode.Parse(text);
                    if (parsed is JsonObject)
                    {
                        return parsed;
                    }

                    return new JsonObject { ["rawInvalidInput"] = parsed };
                }
                catch (JsonException)
                {
                    return new JsonObject { ["rawInvalidInput"] = text };
                }
            }

            return new JsonObject { ["rawInvalidInput"] = text };
        }

        return new JsonObject { ["rawInvalidInput"] = AnthropicJson.Node(input) };
    }

    private static JsonArray MoveToolUses(JsonArray content)
    {
        var result = new JsonArray();
        var segment = new JsonArray();
        foreach (var part in content)
        {
            var type = part?["type"]?.GetValue<string>();
            if (type == "thinking" || type == "redacted_thinking")
            {
                Flush(result, segment);
                result.Add(part!.DeepClone());
            }
            else if (part != null)
            {
                segment.Add(part.DeepClone());
            }
        }

        Flush(result, segment);
        return result;
    }

    private static void Flush(JsonArray result, JsonArray segment)
    {
        foreach (var part in segment)
        {
            if (part?["type"]?.GetValue<string>() != "tool_use")
            {
                result.Add(part!.DeepClone());
            }
        }

        foreach (var part in segment)
        {
            if (part?["type"]?.GetValue<string>() == "tool_use")
            {
                result.Add(part.DeepClone());
            }
        }

        segment.Clear();
    }

    private static string ToProviderName(IReadOnlyDictionary<string, string> names, string toolName)
    {
        string mapped;
        return names.TryGetValue(toolName, out mapped) ? mapped : toolName;
    }

    private static string TopLevel(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private static string ResolveMedia(string mediaType, string? data)
    {
        if (mediaType == "image" || mediaType == "image/*" || mediaType == "application")
        {
            var detected = Detect(data);
            if (detected != null)
            {
                return detected;
            }
        }

        return mediaType;
    }

    private static string? Detect(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(data);
        }
        catch (FormatException)
        {
            return null;
        }

        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            return "image/png";
        }

        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
        {
            return "application/pdf";
        }

        return null;
    }

    private static string DecodeText(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }
        catch (FormatException)
        {
            return data;
        }
    }

    private static JsonElement? Provider(JsonElement element)
    {
        return AnthropicJson.Anthropic(Metadata(element));
    }

    private static JsonElement? Metadata(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty("providerOptions", out var options) ? options : (JsonElement?)null;
    }

    private static void AddBeta(List<string> betas, string beta)
    {
        if (!betas.Contains(beta))
        {
            betas.Add(beta);
        }
    }

    private static List<PromptBlock> Group(JsonElement prompt)
    {
        var blocks = new List<PromptBlock>();
        PromptBlock? current = null;
        if (prompt.ValueKind != JsonValueKind.Array)
        {
            return blocks;
        }

        foreach (var message in prompt.EnumerateArray())
        {
            var role = message.ValueKind == JsonValueKind.String ? "system" : AnthropicJson.String(message, "role");
            var blockRole = role == "tool" ? "user" : role == "assistant" ? "assistant" : role == "system" ? "system" : "user";
            if (current == null || current.Role != blockRole)
            {
                current = new PromptBlock(blockRole);
                blocks.Add(current);
            }

            current.Messages.Add(message);
        }

        return blocks;
    }

    private sealed class PromptBlock
    {
        public PromptBlock(string role)
        {
            Role = role;
        }

        public string Role { get; }

        public List<JsonElement> Messages { get; } = new List<JsonElement>();
    }
}
