// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Options for <see cref="AnthropicMessages.Convert"/>.</summary>
public sealed class AnthropicConvertOptions
{
    /// <summary>When false, reasoning blocks are omitted.</summary>
    public bool SendReasoning { get; set; } = true;

    /// <summary>Warnings appended while converting.</summary>
    public List<AnthropicWarning> Warnings { get; set; } = new List<AnthropicWarning>();

    /// <summary>Shared cache-breakpoint counter. A new validator is used when this is null.</summary>
    public AnthropicCacheControlValidator? CacheControl { get; set; }

    /// <summary>Maps a custom tool name to the Anthropic provider tool name.</summary>
    public IReadOnlyDictionary<string, string>? ToolNames { get; set; }

    /// <summary>Maps a toolset member tool name to the Anthropic toolset name.</summary>
    public IReadOnlyDictionary<string, string>? ToolsetNames { get; set; }
}

/// <summary>Prompt and beta headers produced by message conversion.</summary>
public sealed class AnthropicPromptConversion
{
    /// <summary>Creates a conversion result.</summary>
    public AnthropicPromptConversion(JsonObject prompt, IReadOnlyList<string> betas)
    {
        Prompt = prompt ?? new JsonObject();
        Betas = betas ?? Array.Empty<string>();
    }

    /// <summary>Anthropic <c>system</c> and <c>messages</c>.</summary>
    public JsonObject Prompt { get; }

    /// <summary>Beta headers required by the converted prompt.</summary>
    public IReadOnlyList<string> Betas { get; }
}

/// <summary>Converts a V4 prompt into Anthropic Messages content blocks.</summary>
public static class AnthropicMessages
{
    /// <summary>Converts <paramref name="prompt"/>, a JSON array of V4 messages.</summary>
    public static AnthropicPromptConversion Convert(JsonElement prompt, AnthropicConvertOptions? options = null)
    {
        if (prompt.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Anthropic prompt must be an array of messages.");
        }

        var settings = options ?? new AnthropicConvertOptions();
        var warnings = settings.Warnings ?? new List<AnthropicWarning>();
        var validator = settings.CacheControl ?? new AnthropicCacheControlValidator();
        var betas = new BetaSet();
        JsonArray? system = null;
        var messages = new JsonArray();
        var blocks = Group(prompt);

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var isLast = i == blocks.Count - 1;
            if (block.Kind == "system")
            {
                ConvertSystem(block.Messages, i == 0, system == null, warnings, validator, betas, ref system, messages);
            }
            else if (block.Kind == "user")
            {
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = ConvertUser(block.Messages, warnings, validator, betas, settings),
                });
            }
            else
            {
                var content = ConvertAssistant(block.Messages, isLast, warnings, validator, betas, settings);
                if (content.Count > 0)
                {
                    messages.Add(new JsonObject
                    {
                        ["role"] = "assistant",
                        ["content"] = MoveToolUseToEnd(content),
                    });
                }
            }
        }

        var body = new JsonObject { ["messages"] = messages };
        if (system != null)
        {
            body["system"] = system;
        }

        return new AnthropicPromptConversion(body, betas.Values);
    }

    /// <summary>Builds a V4 prompt JSON array from the public call messages.</summary>
    public static JsonArray FromCall(LanguageModelCallOptions options)
    {
        var prompt = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage system)
            {
                prompt.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            }
            else if (message is UserModelMessage user)
            {
                var content = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    }
                    else if (part is FileContentPart file)
                    {
                        var data = new JsonObject();
                        if (!string.IsNullOrEmpty(file.Url))
                        {
                            data["type"] = "url";
                            data["url"] = file.Url;
                        }
                        else
                        {
                            data["type"] = "data";
                            data["data"] = file.Data == null ? string.Empty : System.Convert.ToBase64String(file.Data);
                        }

                        var node = new JsonObject
                        {
                            ["type"] = "file",
                            ["mediaType"] = file.MediaType ?? "application/octet-stream",
                            ["data"] = data,
                        };
                        AnthropicJson.Set(node, "filename", file.FileName == null ? null : JsonValue.Create(file.FileName));
                        content.Add(node);
                    }
                }

                prompt.Add(new JsonObject { ["role"] = "user", ["content"] = content });
            }
            else if (message is AssistantModelMessage assistant)
            {
                var content = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = assistant.Text });
                }

                if (!string.IsNullOrEmpty(assistant.Reasoning))
                {
                    content.Add(new JsonObject { ["type"] = "reasoning", ["text"] = assistant.Reasoning });
                }

                foreach (var call in assistant.ToolCalls)
                {
                    JsonNode input;
                    try
                    {
                        input = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson) ?? new JsonObject();
                    }
                    catch (JsonException)
                    {
                        input = new JsonObject();
                    }

                    var node = new JsonObject
                    {
                        ["type"] = "tool-call",
                        ["toolCallId"] = call.ToolCallId,
                        ["toolName"] = call.ToolName,
                        ["input"] = input,
                    };
                    if (call.ProviderMetadata is { } metadata)
                    {
                        node["providerOptions"] = AnthropicJson.Node(metadata);
                    }

                    content.Add(node);
                }

                prompt.Add(new JsonObject { ["role"] = "assistant", ["content"] = content });
            }
            else if (message is ToolModelMessage tool)
            {
                JsonNode? value;
                var outputType = tool.IsError ? "error-json" : "json";
                try
                {
                    value = JsonNode.Parse(string.IsNullOrWhiteSpace(tool.OutputJson) ? "null" : tool.OutputJson);
                }
                catch (JsonException)
                {
                    outputType = tool.IsError ? "error-text" : "text";
                    value = JsonValue.Create(tool.OutputJson);
                }

                var result = new JsonObject
                {
                    ["type"] = "tool-result",
                    ["toolName"] = tool.ToolName,
                    ["toolCallId"] = tool.ToolCallId,
                    ["output"] = new JsonObject { ["type"] = outputType, ["value"] = value },
                };
                if (tool.ProviderMetadata is { } toolMetadata)
                {
                    result["providerOptions"] = AnthropicJson.Node(toolMetadata);
                }

                prompt.Add(new JsonObject
                {
                    ["role"] = "tool",
                    ["content"] = new JsonArray { result },
                });
            }
        }

        return prompt;
    }

    private static void ConvertSystem(
        List<JsonElement> block,
        bool isFirstBlock,
        bool systemMissing,
        List<AnthropicWarning> warnings,
        AnthropicCacheControlValidator validator,
        BetaSet betas,
        ref JsonArray? system,
        JsonArray messages)
    {
        var converted = new List<SystemPiece>();
        foreach (var message in block)
        {
            var text = message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String
                ? content.GetString() ?? string.Empty
                : string.Empty;
            var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(message));
            var toolChanges = anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("toolChanges", out var changes) && changes.ValueKind == JsonValueKind.Array
                ? changes
                : default;
            var clearAt = AnthropicJson.String(anthropic, "clearAt");
            var effort = AnthropicJson.String(anthropic, "effort");
            var parts = new JsonArray();
            var toolChangeCount = toolChanges.ValueKind == JsonValueKind.Array ? toolChanges.GetArrayLength() : 0;
            if (text.Length > 0 || (toolChangeCount == 0 && clearAt == null && effort == null))
            {
                var textPart = new JsonObject { ["type"] = "text", ["text"] = text };
                AnthropicJson.Set(textPart, "cache_control", validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(message), "system message", true));
                parts.Add(textPart);
            }

            if (toolChanges.ValueKind == JsonValueKind.Array)
            {
                foreach (var change in toolChanges.EnumerateArray())
                {
                    var toolName = AnthropicJson.String(change, "toolName") ?? string.Empty;
                    parts.Add(new JsonObject
                    {
                        ["type"] = AnthropicJson.String(change, "type"),
                        ["tool"] = new JsonObject { ["type"] = "tool_reference", ["name"] = toolName },
                    });
                }
            }

            converted.Add(new SystemPiece(parts, clearAt, effort, toolChangeCount));
        }

        var toolChangeTotal = 0;
        var hasInlineOptions = false;
        foreach (var piece in converted)
        {
            toolChangeTotal += piece.ToolChangeCount;
            if (piece.ClearAt != null || piece.Effort != null)
            {
                hasInlineOptions = true;
            }
        }

        if (isFirstBlock || (systemMissing && toolChangeTotal == 0 && !hasInlineOptions))
        {
            if (toolChangeTotal > 0)
            {
                warnings.Add(new AnthropicWarning(
                    "other",
                    null,
                    "tool changes on the initial system message are not supported by Anthropic. Configure the initial tool set via the tools option instead. The tool changes have been ignored."));
            }

            foreach (var piece in converted)
            {
                if (piece.ClearAt != null || piece.Effort != null)
                {
                    warnings.Add(new AnthropicWarning(
                        "other",
                        null,
                        "clearAt and effort on the initial system message are not supported by Anthropic. These options have been ignored."));
                }
            }

            system = system ?? new JsonArray();
            foreach (var piece in converted)
            {
                foreach (var part in piece.Content)
                {
                    if (part is JsonObject obj && obj["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var type) && type == "text")
                    {
                        system.Add(part.DeepClone());
                    }
                }
            }
        }
        else
        {
            betas.Add("mid-conversation-system-2026-04-07");
            foreach (var piece in converted)
            {
                var message = new JsonObject { ["role"] = "system", ["content"] = piece.Content };
                AnthropicJson.Set(message, "clear_at", piece.ClearAt == null ? null : JsonValue.Create(piece.ClearAt));
                if (piece.Effort != null)
                {
                    message["output_config"] = new JsonObject { ["effort"] = piece.Effort };
                }

                messages.Add(message);
                if (piece.ToolChangeCount > 0)
                {
                    betas.Add("mid-conversation-tool-changes-2026-07-01");
                }

                if (piece.ClearAt != null)
                {
                    betas.Add("mid-conversation-system-clear-at-2026-08-21");
                }

                if (piece.Effort != null)
                {
                    betas.Add("mid-conversation-output-config-2026-07-01");
                }
            }
        }
    }

    private static JsonArray ConvertUser(
        List<JsonElement> block,
        List<AnthropicWarning> warnings,
        AnthropicCacheControlValidator validator,
        BetaSet betas,
        AnthropicConvertOptions settings)
    {
        var content = new JsonArray();
        foreach (var message in block)
        {
            var role = AnthropicJson.String(message, "role");
            if (role == "user")
            {
                var parts = message.GetProperty("content");
                var index = 0;
                var count = parts.GetArrayLength();
                foreach (var part in parts.EnumerateArray())
                {
                    var isLast = index == count - 1;
                    index++;
                    var cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(part), "user message part", true);
                    if (cache == null && isLast)
                    {
                        cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(message), "user message", true);
                    }

                    var type = AnthropicJson.String(part, "type");
                    if (type == "text")
                    {
                        var text = new JsonObject { ["type"] = "text", ["text"] = AnthropicJson.String(part, "text") ?? string.Empty };
                        AnthropicJson.Set(text, "cache_control", cache);
                        content.Add(text);
                    }
                    else if (type == "file")
                    {
                        ConvertFile(part, cache, betas, content);
                    }
                }
            }
            else if (role == "tool")
            {
                var parts = message.GetProperty("content");
                var index = 0;
                var count = parts.GetArrayLength();
                foreach (var part in parts.EnumerateArray())
                {
                    var isLast = index == count - 1;
                    index++;
                    if (AnthropicJson.String(part, "type") == "tool-approval-response")
                    {
                        continue;
                    }

                    content.Add(ConvertToolResult(part, isLast, message, warnings, validator, betas, settings));
                }
            }
        }

        return content;
    }

    private static void ConvertFile(JsonElement part, JsonNode? cache, BetaSet betas, JsonArray content)
    {
        var mediaType = AnthropicJson.String(part, "mediaType") ?? string.Empty;
        var data = part.GetProperty("data");
        var dataType = AnthropicJson.String(data, "type");
        var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(part));
        var filename = AnthropicJson.String(part, "filename");
        if (dataType == "reference")
        {
            var reference = data.GetProperty("reference");
            var fileId = AnthropicJson.ResolveProviderReference(reference, "anthropic");
            betas.Add("files-api-2025-04-14");
            if (AnthropicJson.Bool(anthropic, "containerUpload"))
            {
                content.Add(new JsonObject { ["type"] = "container_upload", ["file_id"] = fileId });
                return;
            }

            if (AnthropicJson.TopLevelMediaType(mediaType) == "image")
            {
                var image = new JsonObject
                {
                    ["type"] = "image",
                    ["source"] = new JsonObject { ["type"] = "file", ["file_id"] = fileId },
                };
                AnthropicJson.Set(image, "cache_control", cache);
                content.Add(image);
                return;
            }

            var document = new JsonObject
            {
                ["type"] = "document",
                ["source"] = new JsonObject { ["type"] = "file", ["file_id"] = fileId },
            };
            AnthropicJson.Set(document, "cache_control", cache);
            content.Add(document);
            return;
        }

        if (dataType == "text")
        {
            AddTextDocument(content, AnthropicJson.String(data, "text") ?? string.Empty, filename, anthropic, cache);
            return;
        }

        var top = AnthropicJson.TopLevelMediaType(mediaType);
        if (top == "image")
        {
            var image = new JsonObject { ["type"] = "image", ["source"] = FileSource(part, data, dataType, mediaType, false) };
            AnthropicJson.Set(image, "cache_control", cache);
            content.Add(image);
            return;
        }

        var resolved = dataType == "url" ? mediaType : AnthropicJson.ResolveFullMediaType(part);
        if (top == "application" && resolved == "application/pdf")
        {
            betas.Add("pdfs-2024-09-25");
            AddDocument(content, FileSource(part, data, dataType, "application/pdf", true), filename, anthropic, cache);
            return;
        }

        if (mediaType == "text/plain")
        {
            JsonObject source;
            if (dataType == "url")
            {
                source = new JsonObject { ["type"] = "url", ["url"] = UrlOf(data) };
            }
            else
            {
                var payload = AnthropicJson.DataString(data) ?? string.Empty;
                source = new JsonObject
                {
                    ["type"] = "text",
                    ["media_type"] = "text/plain",
                    ["data"] = AnthropicJson.DecodeBase64Text(payload),
                };
            }

            AddDocument(content, source, filename, anthropic, cache);
            return;
        }

        throw new AiSdkException("Unsupported functionality: media type: " + mediaType);
    }

    private static JsonObject FileSource(JsonElement part, JsonElement data, string? dataType, string mediaType, bool pdf)
    {
        if (dataType == "url")
        {
            return new JsonObject { ["type"] = "url", ["url"] = UrlOf(data) };
        }

        var payload = AnthropicJson.DataString(data) ?? string.Empty;
        var full = pdf ? "application/pdf" : AnthropicJson.ResolveFullMediaType(part);
        return new JsonObject
        {
            ["type"] = "base64",
            ["media_type"] = full.Length == 0 ? mediaType : full,
            ["data"] = payload,
        };
    }

    private static string UrlOf(JsonElement data)
    {
        var url = data.GetProperty("url");
        return url.ValueKind == JsonValueKind.String ? url.GetString() ?? string.Empty : url.ToString();
    }

    private static void AddTextDocument(JsonArray content, string text, string? filename, JsonElement anthropic, JsonNode? cache)
    {
        var source = new JsonObject
        {
            ["type"] = "text",
            ["media_type"] = "text/plain",
            ["data"] = text,
        };
        AddDocument(content, source, filename, anthropic, cache);
    }

    private static void AddDocument(JsonArray content, JsonObject source, string? filename, JsonElement anthropic, JsonNode? cache)
    {
        var title = AnthropicJson.String(anthropic, "title") ?? filename;
        var context = AnthropicJson.String(anthropic, "context");
        var document = new JsonObject { ["type"] = "document", ["source"] = source };
        AnthropicJson.Set(document, "title", title == null ? null : JsonValue.Create(title));
        AnthropicJson.Set(document, "context", string.IsNullOrEmpty(context) ? null : JsonValue.Create(context));
        if (CitationsEnabled(anthropic))
        {
            document["citations"] = new JsonObject { ["enabled"] = true };
        }

        AnthropicJson.Set(document, "cache_control", cache);
        content.Add(document);
    }

    private static bool CitationsEnabled(JsonElement anthropic)
    {
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty("citations", out var citations))
        {
            return false;
        }

        return citations.ValueKind == JsonValueKind.Object && citations.TryGetProperty("enabled", out var enabled) && enabled.ValueKind == JsonValueKind.True;
    }

    private static JsonObject ConvertToolResult(
        JsonElement part,
        bool isLast,
        JsonElement message,
        List<AnthropicWarning> warnings,
        AnthropicCacheControlValidator validator,
        BetaSet betas,
        AnthropicConvertOptions settings)
    {
        var output = part.GetProperty("output");
        var outputType = AnthropicJson.String(output, "type") ?? "json";
        JsonElement outputProviderOptions = default;
        if (output.TryGetProperty("providerOptions", out var direct))
        {
            outputProviderOptions = direct;
        }
        else if (outputType == "content" && output.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.TryGetProperty("providerOptions", out var itemOptions))
                {
                    outputProviderOptions = itemOptions;
                    break;
                }
            }
        }

        var cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(part), "tool result part", true)
            ?? validator.GetCacheControl(outputProviderOptions, "tool result output", true);
        if (cache == null && isLast)
        {
            cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(message), "tool result message", true);
        }

        JsonNode contentValue;
        if (outputType == "content")
        {
            contentValue = ToolContent(output.GetProperty("value"), warnings, betas);
        }
        else if (outputType == "text" || outputType == "error-text")
        {
            contentValue = JsonValue.Create(TextValue(output));
        }
        else if (outputType == "execution-denied")
        {
            contentValue = JsonValue.Create(AnthropicJson.String(output, "reason") ?? "Tool call execution denied.");
        }
        else
        {
            contentValue = JsonValue.Create(output.TryGetProperty("value", out var json) ? AnthropicJson.Stringify(json) : "null");
        }

        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var result = new JsonObject
        {
            ["type"] = "tool_result",
            ["tool_use_id"] = AnthropicJson.String(part, "toolCallId"),
            ["content"] = contentValue,
        };
        var toolset = ToolsetName(toolName, AnthropicJson.ProviderOptionsOf(part), settings);
        AnthropicJson.Set(result, "toolset_name", toolset == null ? null : JsonValue.Create(toolset));
        if (outputType == "error-text" || outputType == "error-json")
        {
            result["is_error"] = true;
        }

        AnthropicJson.Set(result, "cache_control", cache);
        return result;
    }

    private static string TextValue(JsonElement output)
    {
        if (!output.TryGetProperty("value", out var value))
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
    }

    private static JsonArray ToolContent(JsonElement value, List<AnthropicWarning> warnings, BetaSet betas)
    {
        var parts = new JsonArray();
        foreach (var contentPart in value.EnumerateArray())
        {
            var type = AnthropicJson.String(contentPart, "type");
            if (type == "text")
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = AnthropicJson.String(contentPart, "text") ?? string.Empty });
                continue;
            }

            if (type == "custom")
            {
                var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(contentPart));
                if (AnthropicJson.String(anthropic, "type") == "tool-reference")
                {
                    parts.Add(new JsonObject
                    {
                        ["type"] = "tool_reference",
                        ["tool_name"] = AnthropicJson.String(anthropic, "toolName"),
                    });
                }
                else
                {
                    warnings.Add(new AnthropicWarning("other", null, "unsupported custom tool content part"));
                }

                continue;
            }

            if (type == "file" && contentPart.TryGetProperty("data", out var data))
            {
                var mediaType = AnthropicJson.String(contentPart, "mediaType") ?? string.Empty;
                var dataType = AnthropicJson.String(data, "type");
                var top = AnthropicJson.TopLevelMediaType(mediaType);
                if (dataType == "url")
                {
                    var source = new JsonObject { ["type"] = "url", ["url"] = UrlOf(data) };
                    parts.Add(new JsonObject
                    {
                        ["type"] = top == "image" ? "image" : "document",
                        ["source"] = source,
                    });
                    continue;
                }

                if (dataType == "data")
                {
                    if (top == "image")
                    {
                        parts.Add(new JsonObject
                        {
                            ["type"] = "image",
                            ["source"] = new JsonObject
                            {
                                ["type"] = "base64",
                                ["media_type"] = AnthropicJson.ResolveFullMediaType(contentPart),
                                ["data"] = AnthropicJson.DataString(data),
                            },
                        });
                        continue;
                    }

                    if (AnthropicJson.ResolveFullMediaType(contentPart) == "application/pdf")
                    {
                        betas.Add("pdfs-2024-09-25");
                        parts.Add(new JsonObject
                        {
                            ["type"] = "document",
                            ["source"] = new JsonObject
                            {
                                ["type"] = "base64",
                                ["media_type"] = "application/pdf",
                                ["data"] = AnthropicJson.DataString(data),
                            },
                        });
                        continue;
                    }

                    warnings.Add(new AnthropicWarning("other", null, "unsupported tool content part type: file with media type: " + mediaType));
                    continue;
                }

                warnings.Add(new AnthropicWarning("other", null, "unsupported tool content part type: file with data type: " + dataType));
                continue;
            }

            warnings.Add(new AnthropicWarning("other", null, "unsupported tool content part type: " + type));
        }

        return parts;
    }

    private static JsonArray ConvertAssistant(
        List<JsonElement> block,
        bool isLastBlock,
        List<AnthropicWarning> warnings,
        AnthropicCacheControlValidator validator,
        BetaSet betas,
        AnthropicConvertOptions settings)
    {
        var content = new JsonArray();
        var mcpIds = new HashSet<string>(StringComparer.Ordinal);
        for (var j = 0; j < block.Count; j++)
        {
            var message = block[j];
            var isLastMessage = j == block.Count - 1;
            var parts = message.GetProperty("content");
            var index = 0;
            var count = parts.GetArrayLength();
            foreach (var part in parts.EnumerateArray())
            {
                var isLastPart = index == count - 1;
                index++;
                var type = AnthropicJson.String(part, "type");
                JsonNode? cache = null;
                if (type != "reasoning")
                {
                    cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(part), "assistant message part", true);
                    if (cache == null && isLastPart)
                    {
                        cache = validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(message), "assistant message", true);
                    }
                }
                if (type == "text")
                {
                    ConvertAssistantText(part, isLastBlock && isLastMessage && isLastPart, cache, content);
                }
                else if (type == "reasoning")
                {
                    ConvertReasoning(part, settings.SendReasoning, warnings, validator, content);
                }
                else if (type == "tool-call")
                {
                    ConvertToolCall(part, cache, warnings, mcpIds, settings, content);
                }
                else if (type == "tool-result")
                {
                    ConvertProviderToolResult(part, cache, warnings, mcpIds, settings, content);
                }
            }
        }

        return content;
    }

    private static void ConvertAssistantText(JsonElement part, bool trim, JsonNode? cache, JsonArray content)
    {
        var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(part));
        var text = AnthropicJson.String(part, "text") ?? string.Empty;
        if (AnthropicJson.String(anthropic, "type") == "compaction")
        {
            if (text.Length == 0)
            {
                return;
            }

            var compaction = new JsonObject { ["type"] = "compaction", ["content"] = text };
            AnthropicJson.Set(compaction, "cache_control", cache);
            content.Add(compaction);
            return;
        }

        var node = new JsonObject
        {
            ["type"] = "text",
            ["text"] = trim ? text.Trim() : text,
        };
        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("citations", out var citations) && citations.ValueKind == JsonValueKind.Array)
        {
            node["citations"] = AnthropicJson.Node(citations);
        }

        AnthropicJson.Set(node, "cache_control", cache);
        content.Add(node);
    }

    private static void ConvertReasoning(
        JsonElement part,
        bool sendReasoning,
        List<AnthropicWarning> warnings,
        AnthropicCacheControlValidator validator,
        JsonArray content)
    {
        if (!sendReasoning)
        {
            warnings.Add(new AnthropicWarning("other", null, "sending reasoning content is disabled for this model"));
            return;
        }

        var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(part));
        var signature = AnthropicJson.String(anthropic, "signature");
        var redacted = AnthropicJson.String(anthropic, "redactedData");
        if (signature != null)
        {
            validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(part), "thinking block", false);
            content.Add(new JsonObject
            {
                ["type"] = "thinking",
                ["thinking"] = AnthropicJson.String(part, "text") ?? string.Empty,
                ["signature"] = signature,
            });
            return;
        }

        if (redacted != null)
        {
            validator.GetCacheControl(AnthropicJson.ProviderOptionsOf(part), "redacted thinking block", false);
            content.Add(new JsonObject { ["type"] = "redacted_thinking", ["data"] = redacted });
            return;
        }

        warnings.Add(new AnthropicWarning("other", null, "unsupported reasoning metadata"));
    }

    private static void ConvertToolCall(
        JsonElement part,
        JsonNode? cache,
        List<AnthropicWarning> warnings,
        HashSet<string> mcpIds,
        AnthropicConvertOptions settings,
        JsonArray content)
    {
        var caller = Caller(AnthropicJson.ProviderOptionsOf(part));
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var providerName = ProviderToolName(toolName, settings);
        var inputElement = part.TryGetProperty("input", out var input) ? input : default;
        if (part.TryGetProperty("providerExecuted", out var executed) && executed.ValueKind == JsonValueKind.True)
        {
            var anthropic = AnthropicJson.Anthropic(AnthropicJson.ProviderOptionsOf(part));
            if (AnthropicJson.String(anthropic, "type") == "mcp-tool-use")
            {
                var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
                mcpIds.Add(id);
                var server = anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("serverName", out var serverName) && serverName.ValueKind == JsonValueKind.String
                    ? serverName.GetString()
                    : null;
                if (server == null)
                {
                    warnings.Add(new AnthropicWarning("other", null, "mcp tool use server name is required and must be a string"));
                    return;
                }

                var mcp = new JsonObject
                {
                    ["type"] = "mcp_tool_use",
                    ["id"] = id,
                    ["name"] = toolName,
                    ["input"] = inputElement.ValueKind == JsonValueKind.Undefined ? new JsonObject() : AnthropicJson.Node(inputElement),
                    ["server_name"] = server,
                };
                AnthropicJson.Set(mcp, "cache_control", cache);
                content.Add(mcp);
                return;
            }

            var codeType = inputElement.ValueKind == JsonValueKind.Object && inputElement.TryGetProperty("type", out var codeTypeElement) && codeTypeElement.ValueKind == JsonValueKind.String
                ? codeTypeElement.GetString()
                : null;
            if (providerName == "code_execution" && (codeType == "bash_code_execution" || codeType == "text_editor_code_execution"))
            {
                content.Add(ServerTool(part, codeType!, WithoutType(inputElement), caller, cache));
                return;
            }

            if (providerName == "code_execution" && codeType == "programmatic-tool-call")
            {
                content.Add(ServerTool(part, "code_execution", WithoutType(inputElement), caller, cache));
                return;
            }

            if (providerName == "code_execution" || providerName == "web_fetch" || providerName == "web_search"
                || providerName == "tool_search_tool_regex" || providerName == "tool_search_tool_bm25")
            {
                content.Add(ServerTool(part, providerName, inputElement.ValueKind == JsonValueKind.Undefined ? new JsonObject() : AnthropicJson.Node(inputElement), caller, cache));
                return;
            }

            if (providerName == "advisor")
            {
                content.Add(ServerTool(part, "advisor", new JsonObject(), caller, cache));
                return;
            }

            warnings.Add(new AnthropicWarning("other", null, "provider executed tool call for tool " + toolName + " is not supported"));
            return;
        }

        var toolset = ToolsetName(toolName, AnthropicJson.ProviderOptionsOf(part), settings);
        var toolInput = AnthropicJson.ToolInput(inputElement.ValueKind == JsonValueKind.Undefined ? default : inputElement);
        if (toolset != null)
        {
            var action = toolInput["action"]?.GetValue<string>();
            toolInput.Remove("action");
            if (string.IsNullOrEmpty(action))
            {
                warnings.Add(new AnthropicWarning("other", null, "toolset tool call for tool " + toolName + " is missing the action"));
                return;
            }

            var member = new JsonObject
            {
                ["type"] = "tool_use",
                ["id"] = AnthropicJson.String(part, "toolCallId"),
                ["name"] = action,
                ["toolset_name"] = toolset,
                ["input"] = toolInput,
            };
            AddCaller(member, caller);
            AnthropicJson.Set(member, "cache_control", cache);
            content.Add(member);
            return;
        }

        var call = new JsonObject
        {
            ["type"] = "tool_use",
            ["id"] = AnthropicJson.String(part, "toolCallId"),
            ["name"] = toolName,
            ["input"] = toolInput,
        };
        AddCaller(call, caller);
        AnthropicJson.Set(call, "cache_control", cache);
        content.Add(call);
    }

    private static JsonObject ServerTool(JsonElement part, string name, JsonNode? input, JsonObject? caller, JsonNode? cache)
    {
        var node = new JsonObject
        {
            ["type"] = "server_tool_use",
            ["id"] = AnthropicJson.String(part, "toolCallId"),
            ["name"] = name,
            ["input"] = input ?? new JsonObject(),
        };
        AddCaller(node, caller);
        AnthropicJson.Set(node, "cache_control", cache);
        return node;
    }

    private static JsonObject WithoutType(JsonElement input)
    {
        var copy = AnthropicJson.ObjectFrom(input);
        copy.Remove("type");
        return copy;
    }

    private static void ConvertProviderToolResult(
        JsonElement part,
        JsonNode? cache,
        List<AnthropicWarning> warnings,
        HashSet<string> mcpIds,
        AnthropicConvertOptions settings,
        JsonArray content)
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var providerName = ProviderToolName(toolName, settings);
        var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
        var output = part.GetProperty("output");
        var outputType = AnthropicJson.String(output, "type");
        var caller = Caller(AnthropicJson.ProviderOptionsOf(part));
        if (mcpIds.Contains(id))
        {
            if (outputType != "json" && outputType != "error-json")
            {
                warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
                return;
            }

            var mcp = new JsonObject
            {
                ["type"] = "mcp_tool_result",
                ["tool_use_id"] = id,
                ["is_error"] = outputType == "error-json",
                ["content"] = output.TryGetProperty("value", out var value) ? AnthropicJson.Node(value) : JsonValue.Create(string.Empty),
            };
            AnthropicJson.Set(mcp, "cache_control", cache);
            content.Add(mcp);
            return;
        }

        if (providerName == "web_search")
        {
            if (outputType == "error-json")
            {
                content.Add(ErrorResult("web_search_tool_result", "web_search_tool_result_error", id, output, caller, cache));
                return;
            }

            if (outputType != "json" || !output.TryGetProperty("value", out var results) || results.ValueKind != JsonValueKind.Array)
            {
                warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
                return;
            }

            var mapped = new JsonArray();
            foreach (var result in results.EnumerateArray())
            {
                var item = new JsonObject { ["type"] = AnthropicJson.String(result, "type") ?? "web_search_result" };
                AnthropicJson.CopyIfPresent(item, "url", result, "url");
                CopyNullable(item, "title", result, "title");
                CopyNullable(item, "page_age", result, "pageAge");
                AnthropicJson.CopyIfPresent(item, "encrypted_content", result, "encryptedContent");
                mapped.Add(item);
            }

            var node = new JsonObject { ["type"] = "web_search_tool_result", ["tool_use_id"] = id, ["content"] = mapped };
            AddCaller(node, caller);
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (providerName == "web_fetch")
        {
            if (outputType == "error-json")
            {
                content.Add(ErrorResult("web_fetch_tool_result", "web_fetch_tool_result_error", id, output, caller, cache));
                return;
            }

            if (outputType != "json" || !output.TryGetProperty("value", out var fetched) || fetched.ValueKind != JsonValueKind.Object)
            {
                warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
                return;
            }

            var source = fetched.GetProperty("content").GetProperty("source");
            var document = new JsonObject { ["type"] = "document" };
            CopyNullable(document, "title", fetched.GetProperty("content"), "title");
            if (fetched.GetProperty("content").TryGetProperty("citations", out var citations))
            {
                document["citations"] = AnthropicJson.Node(citations);
            }

            var sourceNode = new JsonObject { ["type"] = AnthropicJson.String(source, "type") };
            AnthropicJson.CopyIfPresent(sourceNode, "media_type", source, "mediaType");
            AnthropicJson.CopyIfPresent(sourceNode, "data", source, "data");
            document["source"] = sourceNode;
            var body = new JsonObject
            {
                ["type"] = "web_fetch_result",
                ["url"] = AnthropicJson.String(fetched, "url"),
                ["retrieved_at"] = AnthropicJson.String(fetched, "retrievedAt"),
                ["content"] = document,
            };
            var node = new JsonObject { ["type"] = "web_fetch_tool_result", ["tool_use_id"] = id, ["content"] = body };
            AddCaller(node, caller);
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (providerName == "tool_search_tool_regex" || providerName == "tool_search_tool_bm25")
        {
            if (outputType != "json" || !output.TryGetProperty("value", out var refs) || refs.ValueKind != JsonValueKind.Array)
            {
                warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
                return;
            }

            var references = new JsonArray();
            foreach (var reference in refs.EnumerateArray())
            {
                references.Add(new JsonObject
                {
                    ["type"] = "tool_reference",
                    ["tool_name"] = AnthropicJson.String(reference, "toolName"),
                });
            }

            var node = new JsonObject
            {
                ["type"] = "tool_search_tool_result",
                ["tool_use_id"] = id,
                ["content"] = new JsonObject
                {
                    ["type"] = "tool_search_tool_search_result",
                    ["tool_references"] = references,
                },
            };
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (providerName == "advisor")
        {
            if ((outputType != "json" && outputType != "error-json") || !output.TryGetProperty("value", out var advisor) || advisor.ValueKind != JsonValueKind.Object)
            {
                warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
                return;
            }

            var advisorType = AnthropicJson.String(advisor, "type");
            var advisorContent = new JsonObject { ["type"] = advisorType };
            if (advisorType == "advisor_result")
            {
                advisorContent["text"] = AnthropicJson.String(advisor, "text");
                AnthropicJson.CopyIfPresent(advisorContent, "stop_reason", advisor, "stopReason");
            }
            else if (advisorType == "advisor_redacted_result")
            {
                AnthropicJson.CopyIfPresent(advisorContent, "encrypted_content", advisor, "encryptedContent");
                AnthropicJson.CopyIfPresent(advisorContent, "stop_reason", advisor, "stopReason");
            }
            else
            {
                advisorContent["type"] = "advisor_tool_result_error";
                AnthropicJson.CopyIfPresent(advisorContent, "error_code", advisor, "errorCode");
            }

            var node = new JsonObject { ["type"] = "advisor_tool_result", ["tool_use_id"] = id, ["content"] = advisorContent };
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (providerName == "code_execution")
        {
            ConvertCodeExecution(part, output, outputType, id, cache, warnings, toolName, content);
            return;
        }

        warnings.Add(new AnthropicWarning("other", null, "provider executed tool result for tool " + toolName + " is not supported"));
    }

    private static void ConvertCodeExecution(
        JsonElement part,
        JsonElement output,
        string? outputType,
        string id,
        JsonNode? cache,
        List<AnthropicWarning> warnings,
        string toolName,
        JsonArray content)
    {
        if (outputType == "error-text" || outputType == "error-json")
        {
            var errorCode = "unknown";
            var errorType = string.Empty;
            if (output.TryGetProperty("value", out var value))
            {
                JsonElement parsed = value;
                if (value.ValueKind == JsonValueKind.String)
                {
                    try
                    {
                        using var document = JsonDocument.Parse(value.GetString() ?? "{}");
                        parsed = document.RootElement.Clone();
                    }
                    catch (JsonException)
                    {
                        parsed = default;
                    }
                }

                if (parsed.ValueKind == JsonValueKind.Object)
                {
                    errorCode = AnthropicJson.String(parsed, "errorCode") ?? "unknown";
                    errorType = AnthropicJson.String(parsed, "type") ?? string.Empty;
                }
            }

            var kind = errorType == "code_execution_tool_result_error" ? "code_execution_tool_result" : "bash_code_execution_tool_result";
            var inner = errorType == "code_execution_tool_result_error" ? "code_execution_tool_result_error" : "bash_code_execution_tool_result_error";
            var node = new JsonObject
            {
                ["type"] = kind,
                ["tool_use_id"] = id,
                ["content"] = new JsonObject { ["type"] = inner, ["error_code"] = errorCode },
            };
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (outputType != "json" || !output.TryGetProperty("value", out var body) || body.ValueKind != JsonValueKind.Object || AnthropicJson.String(body, "type") == null)
        {
            warnings.Add(new AnthropicWarning("other", null, "provider executed tool result output type " + outputType + " for tool " + toolName + " is not supported"));
            return;
        }

        var resultType = AnthropicJson.String(body, "type");
        if (resultType == "code_execution_result" || resultType == "encrypted_code_execution_result")
        {
            var inner = AnthropicJson.ObjectFrom(body);
            if (!inner.ContainsKey("content"))
            {
                inner["content"] = new JsonArray();
            }

            var node = new JsonObject { ["type"] = "code_execution_tool_result", ["tool_use_id"] = id, ["content"] = inner };
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        if (resultType == "bash_code_execution_result" || resultType == "bash_code_execution_tool_result_error")
        {
            var node = new JsonObject
            {
                ["type"] = "bash_code_execution_tool_result",
                ["tool_use_id"] = id,
                ["content"] = AnthropicJson.Node(body),
            };
            AnthropicJson.Set(node, "cache_control", cache);
            content.Add(node);
            return;
        }

        var editor = new JsonObject
        {
            ["type"] = "text_editor_code_execution_tool_result",
            ["tool_use_id"] = id,
            ["content"] = AnthropicJson.Node(body),
        };
        AnthropicJson.Set(editor, "cache_control", cache);
        content.Add(editor);
    }

    private static JsonObject ErrorResult(string type, string errorType, string id, JsonElement output, JsonObject? caller, JsonNode? cache)
    {
        var errorCode = "unavailable";
        if (output.TryGetProperty("value", out var value))
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                errorCode = AnthropicJson.String(value, "errorCode") ?? "unavailable";
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                try
                {
                    using var document = JsonDocument.Parse(value.GetString() ?? "{}");
                    errorCode = AnthropicJson.String(document.RootElement, "errorCode") ?? "unavailable";
                }
                catch (JsonException)
                {
                    errorCode = "unavailable";
                }
            }
        }

        var node = new JsonObject
        {
            ["type"] = type,
            ["tool_use_id"] = id,
            ["content"] = new JsonObject { ["type"] = errorType, ["error_code"] = errorCode },
        };
        AddCaller(node, caller);
        AnthropicJson.Set(node, "cache_control", cache);
        return node;
    }

    private static void CopyNullable(JsonObject target, string targetName, JsonElement source, string sourceName)
    {
        if (!source.TryGetProperty(sourceName, out var value))
        {
            return;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            AnthropicJson.SetNull(target, targetName);
            return;
        }

        AnthropicJson.Set(target, targetName, AnthropicJson.Node(value));
    }

    private static JsonObject? Caller(JsonElement providerOptions)
    {
        var anthropic = AnthropicJson.Anthropic(providerOptions);
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty("caller", out var caller) || caller.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var type = AnthropicJson.String(caller, "type");
        if ((type == "code_execution_20250825" || type == "code_execution_20260120") && AnthropicJson.String(caller, "toolId") is { } toolId)
        {
            return new JsonObject { ["type"] = type, ["tool_id"] = toolId };
        }

        if (type == "direct")
        {
            return new JsonObject { ["type"] = "direct" };
        }

        return null;
    }

    private static void AddCaller(JsonObject target, JsonObject? caller)
    {
        if (caller != null)
        {
            target["caller"] = caller;
        }
    }

    private static string ProviderToolName(string toolName, AnthropicConvertOptions settings)
    {
        if (settings.ToolNames != null && settings.ToolNames.TryGetValue(toolName, out var mapped) && mapped is not null)
        {
            return mapped;
        }

        return toolName;
    }

    private static string? ToolsetName(string toolName, JsonElement providerOptions, AnthropicConvertOptions settings)
    {
        if (settings.ToolsetNames != null && settings.ToolsetNames.TryGetValue(toolName, out var mapped) && mapped is not null)
        {
            return mapped;
        }

        var anthropic = AnthropicJson.Anthropic(providerOptions);
        return AnthropicJson.String(anthropic, "toolsetName");
    }

    private static JsonArray MoveToolUseToEnd(JsonArray content)
    {
        var result = new JsonArray();
        var segment = new JsonArray();
        foreach (var part in content)
        {
            var type = part is JsonObject obj ? obj["type"]?.GetValue<string>() : null;
            if (type == "thinking" || type == "redacted_thinking")
            {
                Flush(result, segment);
                segment.Clear();
                result.Add(part!.DeepClone());
            }
            else
            {
                segment.Add(part!.DeepClone());
            }
        }

        Flush(result, segment);
        return result;
    }

    private static void Flush(JsonArray result, JsonArray segment)
    {
        foreach (var part in segment)
        {
            if (part is JsonObject obj && obj["type"]?.GetValue<string>() != "tool_use")
            {
                result.Add(part.DeepClone());
            }
        }

        foreach (var part in segment)
        {
            if (part is JsonObject obj && obj["type"]?.GetValue<string>() == "tool_use")
            {
                result.Add(part.DeepClone());
            }
        }
    }

    private static List<PromptBlock> Group(JsonElement prompt)
    {
        var blocks = new List<PromptBlock>();
        PromptBlock? current = null;
        foreach (var message in prompt.EnumerateArray())
        {
            var role = AnthropicJson.String(message, "role");
            var kind = role == "assistant" ? "assistant" : role == "system" ? "system" : "user";
            if (current == null || current.Kind != kind)
            {
                current = new PromptBlock(kind);
                blocks.Add(current);
            }

            current.Messages.Add(message);
        }

        return blocks;
    }

    private sealed class PromptBlock
    {
        public PromptBlock(string kind)
        {
            Kind = kind;
        }

        public string Kind { get; }

        public List<JsonElement> Messages { get; } = new List<JsonElement>();
    }

    private sealed class SystemPiece
    {
        public SystemPiece(JsonArray content, string? clearAt, string? effort, int toolChangeCount)
        {
            Content = content;
            ClearAt = clearAt;
            Effort = effort;
            ToolChangeCount = toolChangeCount;
        }

        public JsonArray Content { get; }

        public string? ClearAt { get; }

        public string? Effort { get; }

        public int ToolChangeCount { get; }
    }

    private sealed class BetaSet
    {
        private readonly List<string> _values = new List<string>();

        private readonly HashSet<string> _seen = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<string> Values
        {
            get { return _values; }
        }

        public void Add(string beta)
        {
            if (_seen.Add(beta))
            {
                _values.Add(beta);
            }
        }
    }
}
