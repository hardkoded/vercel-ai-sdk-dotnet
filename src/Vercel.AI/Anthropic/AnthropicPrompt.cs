// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic <c>system</c> and <c>messages</c> produced from an SDK prompt.</summary>
public sealed class AnthropicPromptResult
{
    /// <summary>Creates a converted prompt.</summary>
    public AnthropicPromptResult(
        JsonArray? system,
        JsonArray messages,
        IReadOnlyList<string> betas,
        IReadOnlyList<AnthropicWarning> warnings
    )
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

    /// <summary>The by-value tool definitions that ended up in <see cref="Messages"/>.</summary>
    internal IReadOnlyList<JsonObject> InlineToolDefinitions { get; set; } =
        Array.Empty<JsonObject>();
}

/// <summary>The Anthropic form of a tool that a mid-conversation tool change defines by value.</summary>
public sealed class AnthropicInlineToolResult
{
    /// <summary>Creates a converted inline tool.</summary>
    public AnthropicInlineToolResult(
        JsonObject? definition,
        IReadOnlyList<AnthropicWarning>? warnings = null,
        IReadOnlyList<string>? betas = null
    )
    {
        Definition = definition;
        Warnings = warnings ?? Array.Empty<AnthropicWarning>();
        Betas = betas ?? Array.Empty<string>();
    }

    /// <summary>The tool definition. Null when the tool is dropped.</summary>
    public JsonObject? Definition { get; }

    /// <summary>Warnings from converting the tool.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }

    /// <summary>Betas the tool requires.</summary>
    public IReadOnlyList<string> Betas { get; }
}

/// <summary>
/// Converts a tool that a mid-conversation tool change adds by value (a <c>function</c> or <c>provider</c>
/// tool object) into its Anthropic definition. Maps to <c>AnthropicInlineToolConverter</c>.
/// </summary>
public delegate AnthropicInlineToolResult AnthropicInlineToolConverter(JsonElement tool);

/// <summary>Converts SDK prompt messages into Anthropic message blocks.</summary>
public static class AnthropicPrompt
{
    /// <summary>Converts SDK prompt messages. Maps to <c>convertToAnthropicPrompt</c>.</summary>
    public static AnthropicPromptResult Convert(
        IReadOnlyList<ModelMessage> prompt,
        bool sendReasoning = true,
        AnthropicCacheControlValidator? cacheControl = null,
        IReadOnlyDictionary<string, string>? providerToolNames = null,
        IReadOnlyDictionary<string, string>? toolsetNames = null,
        AnthropicMediaPartConverter? convertMediaPart = null,
        AnthropicInlineToolConverter? convertInlineTool = null
    )
    {
        if (prompt == null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        return ConvertJson(
            ToJson(prompt),
            sendReasoning,
            cacheControl,
            providerToolNames,
            toolsetNames,
            convertMediaPart,
            convertInlineTool
        );
    }

    /// <summary>Converts a prompt JSON array that uses the SDK message shape.</summary>
    internal static AnthropicPromptResult ConvertJson(
        JsonElement prompt,
        bool sendReasoning = true,
        AnthropicCacheControlValidator? cacheControl = null,
        IReadOnlyDictionary<string, string>? providerToolNames = null,
        IReadOnlyDictionary<string, string>? toolsetNames = null,
        AnthropicMediaPartConverter? convertMediaPart = null,
        AnthropicInlineToolConverter? convertInlineTool = null
    )
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
        var emittedInlineDefinitions = new List<JsonObject>();
        var lastUserMessageIndex = -1;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var isLast = i == blocks.Count - 1;
            if (block.Role == "system")
            {
                ConvertSystem(
                    block,
                    i == 0,
                    ref system,
                    messages,
                    warnings,
                    betas,
                    validator,
                    names,
                    convertInlineTool,
                    emittedInlineDefinitions
                );
            }
            else if (block.Role == "user")
            {
                if (block.Messages.Any(HasUserContent))
                {
                    lastUserMessageIndex = messages.Count;
                }

                messages.Add(
                    new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = ConvertUser(
                            block,
                            warnings,
                            betas,
                            validator,
                            convertMediaPart,
                            toolsets
                        ),
                    }
                );
            }
            else
            {
                var content = ConvertAssistant(
                    block,
                    isLast,
                    sendReasoning,
                    warnings,
                    betas,
                    validator,
                    names,
                    toolsets
                );
                if (content.Count > 0)
                {
                    messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content });
                }
            }
        }

        NormalizeOrphanedCallers(messages, lastUserMessageIndex, warnings);

        if (ownsValidator)
        {
            foreach (var warning in validator.Warnings)
            {
                warnings.Add(warning);
            }
        }

        return new AnthropicPromptResult(system, messages, betas, warnings)
        {
            InlineToolDefinitions = emittedInlineDefinitions,
        };
    }

    // Only a user message (not a tool result message) ends a turn.
    private static bool HasUserContent(JsonElement message)
    {
        return AnthropicJson.String(message, "role") == "user"
            && message.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.Array
            && content.GetArrayLength() > 0;
    }

    // Pruning can remove a code execution call while retaining tool calls that reference it.
    // Check the converted blocks, since unsupported source calls may also have been omitted
    // during conversion. Only history before a subsequent user message is normalized: tool
    // result messages do not end a turn, so their caller metadata must remain intact for
    // Anthropic to resume an active code execution.
    private static void NormalizeOrphanedCallers(
        JsonArray messages,
        int lastUserMessageIndex,
        List<AnthropicWarning> warnings
    )
    {
        var codeExecutionToolCallIds = new HashSet<string>();
        foreach (var message in messages)
        {
            if (message?["role"]?.GetValue<string>() != "assistant")
            {
                continue;
            }

            foreach (var part in (JsonArray)message["content"]!)
            {
                if (
                    part?["type"]?.GetValue<string>() == "server_tool_use"
                    && part["name"]?.GetValue<string>() == "code_execution"
                    && part["id"]?.GetValue<string>() is { } sourceId
                )
                {
                    codeExecutionToolCallIds.Add(sourceId);
                }
            }
        }

        var warnedToolCallIds = new HashSet<string>();
        for (var i = 0; i < lastUserMessageIndex; i++)
        {
            var message = messages[i];
            if (message?["role"]?.GetValue<string>() != "assistant")
            {
                continue;
            }

            foreach (var part in (JsonArray)message["content"]!)
            {
                if (part is not JsonObject block || block["caller"] is not JsonObject caller)
                {
                    continue;
                }

                var callerToolId = caller["tool_id"]?.GetValue<string>();
                if (
                    caller["type"]?.GetValue<string>() == "direct"
                    || (callerToolId != null && codeExecutionToolCallIds.Contains(callerToolId))
                )
                {
                    continue;
                }

                var toolCallId = block["id"]?.GetValue<string>()
                    ?? block["tool_use_id"]?.GetValue<string>()
                    ?? string.Empty;
                block.Remove("caller");
                if (warnedToolCallIds.Add(toolCallId))
                {
                    warnings.Add(
                        new AnthropicWarning(
                            "other",
                            message: "Omitted caller metadata for tool "
                                + toolCallId
                                + " because source code execution tool "
                                + callerToolId
                                + " is missing from the conversation history."
                        )
                    );
                }
            }
        }
    }

    private static void ConvertSystem(
        PromptBlock block,
        bool hoist,
        ref JsonArray? system,
        JsonArray messages,
        List<AnthropicWarning> warnings,
        List<string> betas,
        AnthropicCacheControlValidator validator,
        IReadOnlyDictionary<string, string> names,
        AnthropicInlineToolConverter? convertInlineTool,
        List<JsonObject> emittedInlineDefinitions
    )
    {
        var converted = new List<JsonObject>();
        var inlineDefinitions = new Dictionary<JsonObject, List<JsonObject>>();
        var toolChangeCount = 0;
        var hasInlineOptions = false;
        foreach (var message in block.Messages)
        {
            var text =
                message.ValueKind == JsonValueKind.String
                    ? message.GetString() ?? string.Empty
                    : AnthropicJson.String(message, "content") ?? string.Empty;
            var options = Provider(message);
            var toolChanges =
                options == null ? default : AnthropicJson.Property(options.Value, "toolChanges");
            var clearAt = options == null ? null : AnthropicJson.String(options.Value, "clearAt");
            var effort = options == null ? null : AnthropicJson.String(options.Value, "effort");
            var changes =
                toolChanges != null && toolChanges.Value.ValueKind == JsonValueKind.Array
                    ? toolChanges.Value.GetArrayLength()
                    : 0;

            var content = new JsonArray();
            if (text.Length > 0 || (changes == 0 && clearAt == null && effort == null))
            {
                var part = new JsonObject { ["type"] = "text", ["text"] = text };
                AnthropicJson.Set(
                    part,
                    "cache_control",
                    validator.Get(Metadata(message), "system message", true)
                );
                content.Add(part);
            }

            var messageDefinitions = new List<JsonObject>();
            var messageChangeCount = 0;
            if (toolChanges != null && toolChanges.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var change in toolChanges.Value.EnumerateArray())
                {
                    if (
                        change.ValueKind == JsonValueKind.Object
                        && change.TryGetProperty("tool", out var inlineTool)
                    )
                    {
                        if (convertInlineTool == null)
                        {
                            throw new UnsupportedFunctionalityException(
                                "tool additions by value in mid-conversation tool changes",
                                "'tool additions by value in mid-conversation tool changes' functionality not supported."
                            );
                        }

                        if (IsComputerTool(inlineTool))
                        {
                            warnings.Add(
                                new AnthropicWarning(
                                    "other",
                                    message: "The tool '"
                                        + (AnthropicJson.String(inlineTool, "name") ?? string.Empty)
                                        + "' cannot be added by value in a tool change. Declare it in the tools option and add it by reference with toolName. The tool change has been ignored."
                                )
                            );
                            continue;
                        }

                        var inline = convertInlineTool(inlineTool);
                        warnings.AddRange(inline.Warnings);
                        foreach (var beta in inline.Betas)
                        {
                            AddBeta(betas, beta);
                        }

                        if (inline.Definition == null)
                        {
                            continue;
                        }

                        content.Add(
                            new JsonObject
                            {
                                ["type"] = "tool_addition",
                                ["tool"] = new JsonObject
                                {
                                    ["type"] = "tool_definition",
                                    ["definition"] = inline.Definition,
                                },
                            }
                        );
                        messageDefinitions.Add(inline.Definition);
                        messageChangeCount++;
                        continue;
                    }

                    var toolName = AnthropicJson.String(change, "toolName") ?? string.Empty;
                    content.Add(
                        new JsonObject
                        {
                            ["type"] = AnthropicJson.String(change, "type"),
                            ["tool"] = new JsonObject
                            {
                                ["type"] = "tool_reference",
                                ["name"] = ToProviderName(names, toolName),
                            },
                        }
                    );
                    messageChangeCount++;
                }
            }

            // Every change was ignored and nothing else is left to send.
            if (content.Count == 0 && clearAt == null && effort == null)
            {
                continue;
            }

            toolChangeCount += messageChangeCount;
            if (clearAt != null || effort != null)
            {
                hasInlineOptions = true;
            }

            var node = new JsonObject { ["content"] = content };
            AnthropicJson.Set(node, "clear_at", clearAt);
            if (effort != null)
            {
                node["output_config"] = new JsonObject { ["effort"] = effort };
            }

            converted.Add(node);
            if (messageDefinitions.Count > 0)
            {
                inlineDefinitions[node] = messageDefinitions;
            }
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
                warnings.Add(
                    new AnthropicWarning(
                        "other",
                        message: "tool changes on the initial system message are not supported by Anthropic. Configure the initial tool set via the tools option instead. The tool changes have been ignored."
                    )
                );
            }

            var parts = system ?? new JsonArray();
            foreach (var message in converted)
            {
                // Effort-only messages stay in the messages array.
                var contentParts = message["content"] as JsonArray;
                if (
                    (contentParts?.Count ?? 0) == 0
                    && message["clear_at"] == null
                    && message["output_config"] != null
                )
                {
                    messages.Add(
                        new JsonObject
                        {
                            ["role"] = "system",
                            ["content"] = new JsonArray(),
                            ["output_config"] = message["output_config"]!.DeepClone(),
                        }
                    );
                    AddBeta(betas, "mid-conversation-output-config-2026-07-01");
                }
                else if (message["clear_at"] != null || message["output_config"] != null)
                {
                    warnings.Add(
                        new AnthropicWarning(
                            "other",
                            message: "clearAt and effort on this initial system message are not supported by Anthropic. Use a separate effort-only system message with empty content to set effort. These options have been ignored."
                        )
                    );
                }

                if (contentParts != null)
                {
                    foreach (var part in contentParts)
                    {
                        if (part is JsonObject obj && obj["type"]?.GetValue<string>() == "text")
                        {
                            parts.Add(obj.DeepClone());
                        }
                    }
                }
            }

            system = parts;
            return;
        }

        AddBeta(betas, "mid-conversation-system-2026-04-07");
        foreach (var message in converted)
        {
            var inline = new JsonObject
            {
                ["role"] = "system",
                ["content"] = message["content"]!.DeepClone(),
            };
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
            if (inlineDefinitions.TryGetValue(message, out var definitions))
            {
                // The inline tools beta also covers changes by reference.
                emittedInlineDefinitions.AddRange(definitions);
                AddBeta(betas, "inline-tools-2026-09-15");
            }
            else if (message["content"] is JsonArray content)
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

    private static bool IsComputerTool(JsonElement tool)
    {
        // The computer use tools cannot be defined in a message (the API rejects
        // them), they must be declared in `tools` and added by reference.
        return AnthropicJson.String(tool, "type") == "provider"
            && (AnthropicJson.String(tool, "id") ?? string.Empty).StartsWith(
                "anthropic.computer_",
                StringComparison.Ordinal
            );
    }

    /// <summary>
    /// Collects the tools that mid-conversation tool changes add by value, so they can take part in
    /// tool name mapping. Tools that cannot be added by value are left out. Maps to <c>getInlineToolsFromPrompt</c>.
    /// </summary>
    internal static IReadOnlyList<JsonElement> GetInlineTools(JsonElement prompt)
    {
        var tools = new List<JsonElement>();
        if (prompt.ValueKind != JsonValueKind.Array)
        {
            return tools;
        }

        foreach (var message in prompt.EnumerateArray())
        {
            if (
                message.ValueKind != JsonValueKind.Object
                || AnthropicJson.String(message, "role") != "system"
            )
            {
                continue;
            }

            var options = Provider(message);
            var changes =
                options == null ? default : AnthropicJson.Property(options.Value, "toolChanges");
            if (changes == null || changes.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var change in changes.Value.EnumerateArray())
            {
                if (
                    change.ValueKind == JsonValueKind.Object
                    && change.TryGetProperty("tool", out var tool)
                    && !IsComputerTool(tool)
                )
                {
                    tools.Add(tool);
                }
            }
        }

        return tools;
    }

    /// <summary>Converts SDK prompt messages to the JSON prompt shape.</summary>
    internal static JsonElement PromptJson(IReadOnlyList<ModelMessage> prompt)
    {
        return ToJson(prompt);
    }

    private static bool IsInitial(
        bool firstBlock,
        JsonArray? system,
        int toolChanges,
        bool inlineOptions
    )
    {
        if (firstBlock)
        {
            return true;
        }

        return system == null && toolChanges == 0 && !inlineOptions;
    }

    private static JsonArray ConvertUser(
        PromptBlock block,
        List<AnthropicWarning> warnings,
        List<string> betas,
        AnthropicCacheControlValidator validator,
        AnthropicMediaPartConverter? convertMediaPart,
        IReadOnlyDictionary<string, string> toolsets
    )
    {
        var content = new JsonArray();
        foreach (var message in block.Messages)
        {
            var role = AnthropicJson.String(message, "role") ?? "user";
            if (role == "tool")
            {
                AppendToolResults(message, content, warnings, betas, validator, toolsets);
                continue;
            }

            if (
                !message.TryGetProperty("content", out var parts)
                || parts.ValueKind != JsonValueKind.Array
            )
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
                    var text = new JsonObject
                    {
                        ["type"] = "text",
                        ["text"] = AnthropicJson.String(part, "text") ?? string.Empty,
                    };
                    AnthropicJson.Set(text, "cache_control", cache);
                    content.Add(text);
                }
                else if (type == "file")
                {
                    var convertedPart = convertMediaPart?.Invoke(part);
                    if (convertedPart != null)
                    {
                        AnthropicJson.Set(convertedPart, "cache_control", cache);
                        content.Add(convertedPart);
                        continue;
                    }

                    content.Add(ConvertFile(part, cache, betas));
                }
            }
        }

        return content;
    }

    private static void AppendToolResults(
        JsonElement message,
        JsonArray content,
        List<AnthropicWarning> warnings,
        List<string> betas,
        AnthropicCacheControlValidator validator,
        IReadOnlyDictionary<string, string> toolsets
    )
    {
        if (
            !message.TryGetProperty("content", out var parts)
            || parts.ValueKind != JsonValueKind.Array
        )
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
            if (
                part.TryGetProperty("output", out var output)
                && output.ValueKind == JsonValueKind.Object
            )
            {
                outputOptions = Metadata(output);
                if (
                    outputOptions == null
                    && output.TryGetProperty("value", out var value)
                    && value.ValueKind == JsonValueKind.Array
                )
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
            var resultToolset = ToolsetName(part, toolsets);
            if (resultToolset != null)
            {
                result["toolset_name"] = resultToolset;
            }

            if (part.TryGetProperty("output", out var body))
            {
                var outputType = AnthropicJson.String(body, "type");
                if (outputType == "error-json" || outputType == "error-text")
                {
                    result["is_error"] = true;
                }

                if (
                    outputType == "content"
                    && body.TryGetProperty("value", out var value)
                    && value.ValueKind == JsonValueKind.Array
                )
                {
                    result["content"] = ConvertToolContent(value, warnings, betas);
                }
                else if (outputType == "execution-denied")
                {
                    result["content"] = AnthropicJson.String(body, "reason")
                        ?? "Tool call execution denied.";
                }
                else if (body.TryGetProperty("value", out var jsonValue))
                {
                    result["content"] =
                        jsonValue.ValueKind == JsonValueKind.String
                            ? jsonValue.GetString()
                            : jsonValue.GetRawText();
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

    private static string ResolveAnthropicReference(JsonElement data)
    {
        var reference = new Dictionary<string, string>();
        if (
            data.TryGetProperty("reference", out var referenceValue)
            && referenceValue.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var entry in referenceValue.EnumerateObject())
            {
                if (entry.Value.ValueKind == JsonValueKind.String)
                {
                    reference[entry.Name] = entry.Value.GetString()!;
                }
            }
        }

        return ProviderReferences.ResolveProviderReference(reference, "anthropic");
    }

    private static JsonArray ConvertToolContent(
        JsonElement parts,
        List<AnthropicWarning> warnings,
        List<string> betas
    )
    {
        var nested = new JsonArray();
        foreach (var item in parts.EnumerateArray())
        {
            var itemType = AnthropicJson.String(item, "type");
            if (itemType == "text")
            {
                nested.Add(
                    new JsonObject
                    {
                        ["type"] = "text",
                        ["text"] = AnthropicJson.String(item, "text") ?? string.Empty,
                    }
                );
            }
            else if (itemType == "file")
            {
                var converted = ConvertToolFile(item, warnings, betas);
                if (converted != null)
                {
                    nested.Add(converted);
                }
            }
            else if (itemType == "custom")
            {
                var options = Provider(item);
                if (
                    options != null
                    && AnthropicJson.String(options.Value, "type") == "tool-reference"
                )
                {
                    nested.Add(
                        new JsonObject
                        {
                            ["type"] = "tool_reference",
                            ["tool_name"] = AnthropicJson.String(options.Value, "toolName"),
                        }
                    );
                }
                else
                {
                    warnings.Add(
                        new AnthropicWarning(
                            "other",
                            message: "unsupported custom tool content part"
                        )
                    );
                }
            }
            else
            {
                warnings.Add(
                    new AnthropicWarning(
                        "other",
                        message: "unsupported tool content part type: " + itemType
                    )
                );
            }
        }

        return nested;
    }

    private static JsonObject? ConvertToolFile(
        JsonElement item,
        List<AnthropicWarning> warnings,
        List<string> betas
    )
    {
        var mediaType = AnthropicJson.String(item, "mediaType") ?? string.Empty;
        var isImage = TopLevel(mediaType) == "image";
        var data = item.TryGetProperty("data", out var dataValue) ? dataValue : default;
        var dataType =
            data.ValueKind == JsonValueKind.Object ? AnthropicJson.String(data, "type") : null;
        if (dataType == "reference")
        {
            var fileId = ResolveAnthropicReference(data);
            AddBeta(betas, "files-api-2025-04-14");
            return new JsonObject
            {
                ["type"] = isImage ? "image" : "document",
                ["source"] = new JsonObject { ["type"] = "file", ["file_id"] = fileId },
            };
        }

        if (dataType == "url")
        {
            return new JsonObject
            {
                ["type"] = isImage ? "image" : "document",
                ["source"] = new JsonObject
                {
                    ["type"] = "url",
                    ["url"] = AnthropicJson.String(data, "url"),
                },
            };
        }

        if (dataType == "data")
        {
            var base64 = AnthropicJson.String(data, "data") ?? string.Empty;
            if (isImage)
            {
                return new JsonObject
                {
                    ["type"] = "image",
                    ["source"] = new JsonObject
                    {
                        ["type"] = "base64",
                        ["media_type"] = ResolveMedia(mediaType, base64),
                        ["data"] = base64,
                    },
                };
            }

            if (ResolveMedia(mediaType, base64) == "application/pdf")
            {
                AddBeta(betas, "pdfs-2024-09-25");
                return new JsonObject
                {
                    ["type"] = "document",
                    ["source"] = new JsonObject
                    {
                        ["type"] = "base64",
                        ["media_type"] = "application/pdf",
                        ["data"] = base64,
                    },
                };
            }

            warnings.Add(
                new AnthropicWarning(
                    "other",
                    message: "unsupported tool content part type: file with media type: "
                        + mediaType
                )
            );
            return null;
        }

        warnings.Add(
            new AnthropicWarning(
                "other",
                message: "unsupported tool content part type: file with data type: " + dataType
            )
        );
        return null;
    }

    private static JsonObject ConvertFile(JsonElement part, JsonObject? cache, List<string> betas)
    {
        var mediaType = AnthropicJson.String(part, "mediaType") ?? string.Empty;
        var filename = AnthropicJson.String(part, "filename");
        var options = Provider(part);
        var citations =
            options != null
            && AnthropicJson.Property(options.Value, "citations") is { } citation
            && AnthropicJson.Bool(citation, "enabled") == true;
        var title = options == null ? null : AnthropicJson.String(options.Value, "title");
        var context = options == null ? null : AnthropicJson.String(options.Value, "context");
        var container =
            options != null && AnthropicJson.Bool(options.Value, "containerUpload") == true;
        if (!part.TryGetProperty("data", out var data))
        {
            throw new AiSdkException("Unsupported media type: " + mediaType);
        }

        var dataType = AnthropicJson.String(data, "type");
        if (dataType == "reference")
        {
            var fileId = ResolveAnthropicReference(data);

            AddBeta(betas, "files-api-2025-04-14");
            if (container)
            {
                return new JsonObject { ["type"] = "container_upload", ["file_id"] = fileId };
            }

            var kind = TopLevel(mediaType) == "image" ? "image" : "document";
            var node = new JsonObject
            {
                ["type"] = kind,
                ["source"] = new JsonObject { ["type"] = "file", ["file_id"] = fileId },
            };
            AnthropicJson.Set(node, "cache_control", cache);
            return node;
        }

        if (dataType == "text")
        {
            var node = Document(
                new JsonObject
                {
                    ["type"] = "text",
                    ["media_type"] = "text/plain",
                    ["data"] = AnthropicJson.String(data, "text") ?? string.Empty,
                },
                title ?? filename,
                context,
                citations,
                cache
            );
            return node;
        }

        string? url = null;
        string? base64 = null;
        if (dataType == "url")
        {
            url = data.TryGetProperty("url", out var urlValue)
                ? urlValue.ToString().Trim('"')
                : null;
        }
        else
        {
            base64 = data.TryGetProperty("data", out var raw)
                ? raw.ValueKind == JsonValueKind.String
                    ? raw.GetString()
                    : raw.GetRawText()
                : null;
            if (base64 != null && AnthropicJson.LooksLikeUrl(base64))
            {
                url = base64;
                base64 = null;
            }
        }

        var top = TopLevel(mediaType);
        if (top == "image")
        {
            var source =
                url != null
                    ? new JsonObject { ["type"] = "url", ["url"] = url }
                    : new JsonObject
                    {
                        ["type"] = "base64",
                        ["media_type"] = ResolveMedia(mediaType, base64),
                        ["data"] = base64 ?? string.Empty,
                    };
            var image = new JsonObject { ["type"] = "image", ["source"] = source };
            AnthropicJson.Set(image, "cache_control", cache);
            return image;
        }

        var resolved = url != null ? mediaType : ResolveMedia(mediaType, base64);
        if (
            top == "application"
            && (url != null ? mediaType == "application/pdf" : resolved == "application/pdf")
        )
        {
            AddBeta(betas, "pdfs-2024-09-25");
            var source =
                url != null
                    ? new JsonObject { ["type"] = "url", ["url"] = url }
                    : new JsonObject
                    {
                        ["type"] = "base64",
                        ["media_type"] = "application/pdf",
                        ["data"] = base64 ?? string.Empty,
                    };
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
                source = new JsonObject
                {
                    ["type"] = "text",
                    ["media_type"] = "text/plain",
                    ["data"] = DecodeText(base64),
                };
            }

            return Document(source, title ?? filename, context, citations, cache);
        }

        throw new UnsupportedFunctionalityException(
            "media type: " + mediaType,
            "Unsupported functionality: media type: " + mediaType
        );
    }

    private static JsonObject Document(
        JsonObject source,
        string? title,
        string? context,
        bool citations,
        JsonObject? cache
    )
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
        IReadOnlyDictionary<string, string> toolsets
    )
    {
        var content = new JsonArray();
        var mcpToolUseIds = new HashSet<string>();
        for (var j = 0; j < block.Messages.Count; j++)
        {
            var message = block.Messages[j];
            var isLastMessage = j == block.Messages.Count - 1;
            if (
                !message.TryGetProperty("content", out var parts)
                || parts.ValueKind != JsonValueKind.Array
            )
            {
                continue;
            }

            // Filter before calculating the last part so message cache control
            // and assistant prefill trimming apply to content actually sent.
            var sentParts = parts
                .EnumerateArray()
                .Where(part =>
                    AnthropicJson.String(part, "type") != "text"
                    || (AnthropicJson.String(part, "text") ?? string.Empty).Length > 0
                )
                .ToList();
            var index = 0;
            var count = sentParts.Count;
            foreach (var part in sentParts)
            {
                var isLastPart = index == count - 1;
                index++;
                var type = AnthropicJson.String(part, "type");
                JsonObject? cache = null;
                if (type != "reasoning")
                {
                    cache = validator.Get(Metadata(part), "assistant message part", true)
                        ?? (
                            isLastPart
                                ? validator.Get(Metadata(message), "assistant message", true)
                                : null
                        );
                }
                if (type == "text")
                {
                    var metadata = Provider(part);
                    var metaType =
                        metadata == null ? null : AnthropicJson.String(metadata.Value, "type");
                    if (metaType == "compaction")
                    {
                        var text = AnthropicJson.String(part, "text") ?? string.Empty;
                        if (text.Length == 0)
                        {
                            continue;
                        }

                        var compaction = new JsonObject
                        {
                            ["type"] = "compaction",
                            ["content"] = text,
                        };
                        if (
                            AnthropicJson.String(metadata!.Value, "signature") is
                            { } compactionSignature
                        )
                        {
                            AddBeta(betas, "compact-2026-09-04");
                            compaction["signature"] = compactionSignature;
                        }

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
                        if (
                            metadata != null
                            && AnthropicJson.Property(metadata.Value, "citations") is { } citations
                        )
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
                        warnings.Add(
                            new AnthropicWarning(
                                "other",
                                message: "sending reasoning content is disabled for this model"
                            )
                        );
                        continue;
                    }

                    var metadata = Provider(part);
                    var signature =
                        metadata == null ? null : AnthropicJson.String(metadata.Value, "signature");
                    var redacted =
                        metadata == null
                            ? null
                            : AnthropicJson.String(metadata.Value, "redactedData");
                    if (signature != null)
                    {
                        validator.Get(Metadata(part), "thinking block", false);
                        content.Add(
                            new JsonObject
                            {
                                ["type"] = "thinking",
                                ["thinking"] = AnthropicJson.String(part, "text") ?? string.Empty,
                                ["signature"] = signature,
                            }
                        );
                    }
                    else if (redacted != null)
                    {
                        validator.Get(Metadata(part), "redacted thinking block", false);
                        content.Add(
                            new JsonObject { ["type"] = "redacted_thinking", ["data"] = redacted }
                        );
                    }
                    else
                    {
                        warnings.Add(
                            new AnthropicWarning("other", message: "unsupported reasoning metadata")
                        );
                    }
                }
                else if (type == "custom")
                {
                    if (AnthropicJson.String(part, "kind") == "anthropic.fallback")
                    {
                        AppendFallback(part, content, warnings, betas);
                    }
                }
                else if (type == "tool-call")
                {
                    AppendToolCall(part, cache, content, warnings, names, toolsets, mcpToolUseIds);
                }
                else if (type == "tool-result")
                {
                    AppendToolResult(part, cache, content, warnings, names, mcpToolUseIds);
                }
            }
        }

        return MoveToolUses(content);
    }

    private static void AppendFallback(
        JsonElement part,
        JsonArray content,
        List<AnthropicWarning> warnings,
        List<string> betas
    )
    {
        var metadata = Provider(part);
        var from = metadata == null ? null : FallbackModel(metadata.Value, "from");
        var to = metadata == null ? null : FallbackModel(metadata.Value, "to");
        if (
            metadata == null
            || AnthropicJson.String(metadata.Value, "type") != "fallback"
            || from == null
            || to == null
        )
        {
            warnings.Add(
                new AnthropicWarning(
                    "other",
                    message: "anthropic fallback metadata must include from.model and to.model"
                )
            );
            return;
        }

        AddBeta(betas, "server-side-fallback-2026-06-01");
        content.Add(
            new JsonObject
            {
                ["type"] = "fallback",
                ["from"] = new JsonObject { ["model"] = from },
                ["to"] = new JsonObject { ["model"] = to },
            }
        );
    }

    private static string? FallbackModel(JsonElement metadata, string name)
    {
        return
            metadata.TryGetProperty(name, out var side)
            && side.ValueKind == JsonValueKind.Object
            && side.TryGetProperty("model", out var model)
            && model.ValueKind == JsonValueKind.String
            ? model.GetString()
            : null;
    }

    // Maps to getAnthropicCaller: a programmatic caller needs a tool id, a direct caller needs none.
    private static JsonObject? GetAnthropicCaller(JsonElement part)
    {
        var metadata = Provider(part);
        if (
            metadata == null
            || !metadata.Value.TryGetProperty("caller", out var caller)
            || caller.ValueKind != JsonValueKind.Object
        )
        {
            return null;
        }

        var type = AnthropicJson.String(caller, "type");
        var toolId = AnthropicJson.String(caller, "toolId");
        if (
            (type == "code_execution_20250825" || type == "code_execution_20260120")
            && !string.IsNullOrEmpty(toolId)
        )
        {
            return new JsonObject { ["type"] = type, ["tool_id"] = toolId };
        }

        return type == "direct" ? new JsonObject { ["type"] = "direct" } : null;
    }

    private static void AppendToolCall(
        JsonElement part,
        JsonObject? cache,
        JsonArray content,
        List<AnthropicWarning> warnings,
        IReadOnlyDictionary<string, string> names,
        IReadOnlyDictionary<string, string> toolsets,
        HashSet<string> mcpToolUseIds
    )
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
        var caller = GetAnthropicCaller(part);
        if (AnthropicJson.Bool(part, "providerExecuted") != true)
        {
            content.Add(ConvertToolCall(part, cache, caller, warnings, names, toolsets));
            return;
        }

        var providerToolName = ToProviderName(names, toolName);
        var metadata = Provider(part);
        var hasInput = part.TryGetProperty("input", out var inputValue);
        JsonNode? rawInput = hasInput ? NodeOrNull(inputValue) : null;
        var inputType =
            inputValue.ValueKind == JsonValueKind.Object
                ? AnthropicJson.String(inputValue, "type")
                : null;
        if (metadata != null && AnthropicJson.String(metadata.Value, "type") == "mcp-tool-use")
        {
            mcpToolUseIds.Add(id);
            var serverName = AnthropicJson.String(metadata.Value, "serverName");
            if (serverName == null)
            {
                warnings.Add(
                    new AnthropicWarning(
                        "other",
                        message: "mcp tool use server name is required and must be a string"
                    )
                );
                return;
            }

            var mcpCall = new JsonObject
            {
                ["type"] = "mcp_tool_use",
                ["id"] = id,
                ["name"] = toolName,
                ["input"] = rawInput,
                ["server_name"] = serverName,
            };
            AnthropicJson.Set(mcpCall, "cache_control", cache);
            content.Add(mcpCall);
            return;
        }

        string? name = null;
        var input = rawInput;
        if (
            providerToolName == "code_execution"
            && (inputType == "bash_code_execution" || inputType == "text_editor_code_execution")
        )
        {
            // code execution subtools: map back to the subtool name
            name = inputType;
            input = WithoutType(inputValue);
        }
        else if (providerToolName == "code_execution" && inputType == "programmatic-tool-call")
        {
            // code execution 20250825 programmatic tool calling: strip the fake type
            name = "code_execution";
            input = WithoutType(inputValue);
        }
        else if (
            providerToolName == "code_execution"
            || providerToolName == "web_fetch"
            || providerToolName == "web_search"
            || providerToolName == "tool_search_tool_regex"
            || providerToolName == "tool_search_tool_bm25"
        )
        {
            name = providerToolName;
        }
        else if (providerToolName == "advisor")
        {
            // The advisor server_tool_use.input is always {}.
            name = "advisor";
            input = new JsonObject();
        }

        if (name == null)
        {
            warnings.Add(
                new AnthropicWarning(
                    "other",
                    message: "provider executed tool call for tool "
                        + toolName
                        + " is not supported"
                )
            );
            return;
        }

        var serverCall = new JsonObject
        {
            ["type"] = "server_tool_use",
            ["id"] = id,
            ["name"] = name,
            ["input"] = input,
        };
        AnthropicJson.Set(serverCall, "caller", caller);
        AnthropicJson.Set(serverCall, "cache_control", cache);
        content.Add(serverCall);
    }

    // A JSON null stays null instead of becoming an empty object.
    private static JsonNode? NodeOrNull(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null ? null : AnthropicJson.Node(element);
    }

    private static JsonObject WithoutType(JsonElement input)
    {
        var result = new JsonObject();
        foreach (var property in input.EnumerateObject())
        {
            if (property.Name != "type")
            {
                result[property.Name] = NodeOrNull(property.Value);
            }
        }

        return result;
    }

    // Maps to extractErrorValue: the error code of an error output, or null when it is not available.
    private static string? ExtractErrorCode(JsonElement? value)
    {
        if (value == null)
        {
            return null;
        }

        var element = value.Value;
        if (element.ValueKind == JsonValueKind.String)
        {
            try
            {
                using var parsed = JsonDocument.Parse(element.GetString() ?? string.Empty);
                return parsed.RootElement.ValueKind == JsonValueKind.Object
                    ? AnthropicJson.String(parsed.RootElement, "errorCode")
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        return element.ValueKind == JsonValueKind.Object
            ? AnthropicJson.String(element, "errorCode")
            : null;
    }

    private static JsonObject ToolResultBlock(
        string type,
        string toolUseId,
        JsonNode? resultContent,
        JsonObject? caller,
        JsonObject? cache
    )
    {
        var block = new JsonObject
        {
            ["type"] = type,
            ["tool_use_id"] = toolUseId,
            ["content"] = resultContent,
        };
        AnthropicJson.Set(block, "caller", caller);
        AnthropicJson.Set(block, "cache_control", cache);
        return block;
    }

    private static JsonNode? Required(JsonElement element, string name, string toolName)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
        )
        {
            return NodeOrNull(value);
        }

        throw new InvalidOperationException(
            "Type validation failed for the output of tool " + toolName + ": missing " + name
        );
    }

    private static void AppendToolResult(
        JsonElement part,
        JsonObject? cache,
        JsonArray content,
        List<AnthropicWarning> warnings,
        IReadOnlyDictionary<string, string> names,
        HashSet<string> mcpToolUseIds
    )
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
        var providerToolName = ToProviderName(names, toolName);
        var caller = GetAnthropicCaller(part);
        var output = part.TryGetProperty("output", out var outputValue) ? outputValue : default;
        var outputType = AnthropicJson.String(output, "type");
        JsonElement? value =
            output.ValueKind == JsonValueKind.Object && output.TryGetProperty("value", out var v)
                ? v
                : null;

        void UnsupportedOutput() =>
            warnings.Add(
                new AnthropicWarning(
                    "other",
                    message: "provider executed tool result output type "
                        + outputType
                        + " for tool "
                        + toolName
                        + " is not supported"
                )
            );

        if (mcpToolUseIds.Contains(id))
        {
            if (outputType != "json" && outputType != "error-json")
            {
                UnsupportedOutput();
                return;
            }

            var mcpResult = new JsonObject
            {
                ["type"] = "mcp_tool_result",
                ["tool_use_id"] = id,
                ["is_error"] = outputType == "error-json",
                ["content"] = value == null ? null : NodeOrNull(value.Value),
            };
            AnthropicJson.Set(mcpResult, "cache_control", cache);
            content.Add(mcpResult);
        }
        else if (providerToolName == "code_execution")
        {
            AppendCodeExecutionResult(
                id,
                toolName,
                outputType,
                value,
                cache,
                content,
                warnings,
                UnsupportedOutput
            );
            return;
        }

        if (providerToolName == "web_fetch")
        {
            if (outputType == "error-json")
            {
                content.Add(
                    ToolResultBlock(
                        "web_fetch_tool_result",
                        id,
                        new JsonObject
                        {
                            ["type"] = "web_fetch_tool_result_error",
                            ["error_code"] = ExtractErrorCode(value) ?? "unavailable",
                        },
                        caller,
                        cache
                    )
                );
                return;
            }

            if (outputType != "json")
            {
                UnsupportedOutput();
                return;
            }

            var fetch = value!.Value;
            var document = fetch.TryGetProperty("content", out var fetchContent)
                ? fetchContent
                : default;
            var source =
                document.ValueKind == JsonValueKind.Object
                && document.TryGetProperty("source", out var fetchSource)
                    ? fetchSource
                    : default;
            var documentNode = new JsonObject
            {
                ["type"] = "document",
                ["title"] = Required(document, "title", toolName),
            };
            if (
                document.ValueKind == JsonValueKind.Object
                && document.TryGetProperty("citations", out var citations)
            )
            {
                documentNode["citations"] = AnthropicJson.Node(citations);
            }

            documentNode["source"] = new JsonObject
            {
                ["type"] = Required(source, "type", toolName),
                ["media_type"] = Required(source, "mediaType", toolName),
                ["data"] = Required(source, "data", toolName),
            };
            content.Add(
                ToolResultBlock(
                    "web_fetch_tool_result",
                    id,
                    new JsonObject
                    {
                        ["type"] = "web_fetch_result",
                        ["url"] = Required(fetch, "url", toolName),
                        ["retrieved_at"] = Required(fetch, "retrievedAt", toolName),
                        ["content"] = documentNode,
                    },
                    caller,
                    cache
                )
            );
            return;
        }

        if (providerToolName == "web_search")
        {
            if (outputType == "error-json")
            {
                content.Add(
                    ToolResultBlock(
                        "web_search_tool_result",
                        id,
                        new JsonObject
                        {
                            ["type"] = "web_search_tool_result_error",
                            ["error_code"] = ExtractErrorCode(value) ?? "unavailable",
                        },
                        caller,
                        cache
                    )
                );
                return;
            }

            if (outputType != "json")
            {
                UnsupportedOutput();
                return;
            }

            var results = new JsonArray();
            foreach (var result in value!.Value.EnumerateArray())
            {
                results.Add(
                    new JsonObject
                    {
                        ["url"] = Required(result, "url", toolName),
                        ["title"] = Required(result, "title", toolName),
                        ["page_age"] = Required(result, "pageAge", toolName),
                        ["encrypted_content"] = Required(result, "encryptedContent", toolName),
                        ["type"] = Required(result, "type", toolName),
                    }
                );
            }

            content.Add(ToolResultBlock("web_search_tool_result", id, results, caller, cache));
            return;
        }

        if (
            providerToolName == "tool_search_tool_regex"
            || providerToolName == "tool_search_tool_bm25"
        )
        {
            if (
                outputType == "error-json"
                || (
                    outputType == "json"
                    && value!.Value.ValueKind == JsonValueKind.Object
                    && AnthropicJson.String(value.Value, "type") == "tool_search_tool_result_error"
                )
            )
            {
                content.Add(
                    ToolResultBlock(
                        "tool_search_tool_result",
                        id,
                        new JsonObject
                        {
                            ["type"] = "tool_search_tool_result_error",
                            ["error_code"] = ExtractErrorCode(value) ?? "unavailable",
                        },
                        null,
                        cache
                    )
                );
                return;
            }

            if (outputType != "json")
            {
                UnsupportedOutput();
                return;
            }

            var references = new JsonArray();
            foreach (var reference in value!.Value.EnumerateArray())
            {
                references.Add(
                    new JsonObject
                    {
                        ["type"] = "tool_reference",
                        ["tool_name"] = Required(reference, "toolName", toolName),
                    }
                );
            }

            content.Add(
                ToolResultBlock(
                    "tool_search_tool_result",
                    id,
                    new JsonObject
                    {
                        ["type"] = "tool_search_tool_search_result",
                        ["tool_references"] = references,
                    },
                    null,
                    cache
                )
            );
            return;
        }

        if (providerToolName == "advisor")
        {
            if (outputType != "json" && outputType != "error-json")
            {
                UnsupportedOutput();
                return;
            }

            var advisor = value!.Value;
            var advisorType = AnthropicJson.String(advisor, "type");
            var advisorContent = new JsonObject { ["type"] = advisorType };
            if (advisorType == "advisor_result")
            {
                advisorContent["text"] = Required(advisor, "text", toolName);
                AnthropicJson.Set(
                    advisorContent,
                    "stop_reason",
                    AnthropicJson.String(advisor, "stopReason")
                );
            }
            else if (advisorType == "advisor_redacted_result")
            {
                advisorContent["encrypted_content"] = Required(
                    advisor,
                    "encryptedContent",
                    toolName
                );
                AnthropicJson.Set(
                    advisorContent,
                    "stop_reason",
                    AnthropicJson.String(advisor, "stopReason")
                );
            }
            else
            {
                advisorContent["type"] = "advisor_tool_result_error";
                advisorContent["error_code"] = Required(advisor, "errorCode", toolName);
            }

            content.Add(ToolResultBlock("advisor_tool_result", id, advisorContent, null, cache));
            return;
        }

        warnings.Add(
            new AnthropicWarning(
                "other",
                message: "provider executed tool result for tool " + toolName + " is not supported"
            )
        );
    }

    private static void AppendCodeExecutionResult(
        string id,
        string toolName,
        string? outputType,
        JsonElement? value,
        JsonObject? cache,
        JsonArray content,
        List<AnthropicWarning> warnings,
        Action unsupportedOutput
    )
    {
        // Errors from code_execution tools, such as programmatic tool calling.
        if (outputType == "error-text" || outputType == "error-json")
        {
            JsonElement? info = null;
            if (value is { ValueKind: JsonValueKind.String } text)
            {
                try
                {
                    using var parsed = JsonDocument.Parse(text.GetString() ?? string.Empty);
                    info = parsed.RootElement.Clone();
                }
                catch (JsonException)
                {
                    info = null;
                }
            }
            else if (value is { ValueKind: JsonValueKind.Object })
            {
                info = value;
            }

            if (info is not { ValueKind: JsonValueKind.Object })
            {
                info = null;
            }

            var errorCode = (info == null ? null : AnthropicJson.String(info.Value, "errorCode"))
                ?? "unknown";
            var isCodeExecutionError =
                info != null
                && AnthropicJson.String(info.Value, "type") == "code_execution_tool_result_error";
            content.Add(
                ToolResultBlock(
                    isCodeExecutionError
                        ? "code_execution_tool_result"
                        : "bash_code_execution_tool_result",
                    id,
                    new JsonObject
                    {
                        ["type"] = isCodeExecutionError
                            ? "code_execution_tool_result_error"
                            : "bash_code_execution_tool_result_error",
                        ["error_code"] = errorCode,
                    },
                    null,
                    cache
                )
            );
            return;
        }

        if (outputType != "json")
        {
            unsupportedOutput();
            return;
        }

        if (
            value == null
            || value.Value.ValueKind != JsonValueKind.Object
            || AnthropicJson.String(value.Value, "type") is not { } resultType
        )
        {
            warnings.Add(
                new AnthropicWarning(
                    "other",
                    message: "provider executed tool result output value is not a valid code execution result for tool "
                        + toolName
                )
            );
            return;
        }

        var output = value.Value;

        // The type of the output tells code execution 20250522, 20250825 and the
        // encrypted results of implicit code execution for dynamic web tools apart.
        if (
            resultType == "code_execution_result"
            || resultType == "encrypted_code_execution_result"
        )
        {
            var inner = new JsonObject { ["type"] = resultType };
            if (resultType == "code_execution_result")
            {
                inner["stdout"] = Required(output, "stdout", toolName);
            }
            else
            {
                inner["encrypted_stdout"] = Required(output, "encrypted_stdout", toolName);
            }

            inner["stderr"] = Required(output, "stderr", toolName);
            inner["return_code"] = Required(output, "return_code", toolName);
            inner["content"] = output.TryGetProperty("content", out var files)
                ? AnthropicJson.Node(files)
                : new JsonArray();
            content.Add(ToolResultBlock("code_execution_tool_result", id, inner, null, cache));
        }
        else if (resultType == "bash_code_execution_result")
        {
            // Prompt caching requires stable key ordering.
            var inner = new JsonObject
            {
                ["type"] = resultType,
                ["stdout"] = Required(output, "stdout", toolName),
                ["stderr"] = Required(output, "stderr", toolName),
                ["return_code"] = Required(output, "return_code", toolName),
                ["content"] = Required(output, "content", toolName),
            };
            content.Add(ToolResultBlock("bash_code_execution_tool_result", id, inner, null, cache));
        }
        else if (resultType == "bash_code_execution_tool_result_error")
        {
            content.Add(
                ToolResultBlock(
                    "bash_code_execution_tool_result",
                    id,
                    AnthropicJson.Node(output),
                    null,
                    cache
                )
            );
        }
        else if (resultType.StartsWith("text_editor_code_execution_", StringComparison.Ordinal))
        {
            content.Add(
                ToolResultBlock(
                    "text_editor_code_execution_tool_result",
                    id,
                    AnthropicJson.Node(output),
                    null,
                    cache
                )
            );
        }
        else
        {
            throw new InvalidOperationException(
                "Type validation failed for the output of tool "
                    + toolName
                    + ": unknown result type "
                    + resultType
            );
        }
    }

    private static JsonObject ConvertToolCall(
        JsonElement part,
        JsonObject? cache,
        JsonObject? caller,
        List<AnthropicWarning> warnings,
        IReadOnlyDictionary<string, string> names,
        IReadOnlyDictionary<string, string> toolsets
    )
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        var id = AnthropicJson.String(part, "toolCallId") ?? string.Empty;
        var input = ToToolInput(part);
        var metadata = Provider(part);
        var toolset = ToolsetName(part, toolsets);
        if (toolset != null)
        {
            // The member call keeps its input as is. The `action` is the member tool name.
            var rawInput = input as JsonObject ?? new JsonObject();
            var action =
                rawInput["action"] is JsonValue actionValue
                && actionValue.TryGetValue<string>(out var actionText)
                    ? actionText
                    : null;
            if (action == null)
            {
                warnings.Add(
                    new AnthropicWarning(
                        "other",
                        message: "toolset tool call for tool " + toolName + " is missing the action"
                    )
                );
            }

            var toolsetCall = new JsonObject
            {
                ["type"] = "tool_use",
                ["id"] = id,
                ["name"] = action ?? toolset,
                ["toolset_name"] = toolset,
                ["input"] = rawInput,
            };
            AnthropicJson.Set(toolsetCall, "caller", caller);
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
        AnthropicJson.Set(call, "caller", caller);
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
            if (type == "thinking" || type == "redacted_thinking" || type == "fallback")
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
        string? mapped;
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
            bytes = System.Convert.FromBase64String(data);
        }
        catch (FormatException)
        {
            return null;
        }

        if (
            bytes.Length >= 8
            && bytes[0] == 0x89
            && bytes[1] == 0x50
            && bytes[2] == 0x4E
            && bytes[3] == 0x47
        )
        {
            return "image/png";
        }

        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (
            bytes.Length >= 4
            && bytes[0] == 0x25
            && bytes[1] == 0x50
            && bytes[2] == 0x44
            && bytes[3] == 0x46
        )
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
            return Encoding.UTF8.GetString(System.Convert.FromBase64String(data));
        }
        catch (FormatException)
        {
            return data;
        }
    }

    private static string? ToolsetName(
        JsonElement part,
        IReadOnlyDictionary<string, string> toolsets
    )
    {
        var toolName = AnthropicJson.String(part, "toolName") ?? string.Empty;
        if (toolsets.TryGetValue(toolName, out var mapped))
        {
            return mapped;
        }

        var metadata = Provider(part);
        return metadata == null ? null : AnthropicJson.String(metadata.Value, "toolsetName");
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

        return element.TryGetProperty("providerOptions", out var options)
            ? options
            : (JsonElement?)null;
    }

    private static void AddBeta(List<string> betas, string beta)
    {
        if (!betas.Contains(beta))
        {
            betas.Add(beta);
        }
    }

    // Writes the typed prompt in the SDK message JSON shape that ConvertJson reads.
    private static JsonElement ToJson(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            if (message is SystemModelMessage system)
            {
                var node = new JsonObject { ["role"] = "system", ["content"] = system.Content };
                AnthropicJson.Set(node, "providerOptions", Options(system.ProviderOptions));
                messages.Add(node);
            }
            else if (message is UserModelMessage user)
            {
                var parts = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        var textNode = new JsonObject { ["type"] = "text", ["text"] = text.Text };
                        AnthropicJson.Set(
                            textNode,
                            "providerOptions",
                            Options(text.ProviderOptions)
                        );
                        parts.Add(textNode);
                    }
                    else if (part is FileContentPart file)
                    {
                        parts.Add(FileJson(file));
                    }
                }

                var userNode = new JsonObject { ["role"] = "user", ["content"] = parts };
                AnthropicJson.Set(userNode, "providerOptions", Options(user.ProviderOptions));
                messages.Add(userNode);
            }
            else if (message is AssistantModelMessage assistant)
            {
                var parts = new JsonArray();
                foreach (var part in assistant.Parts)
                {
                    JsonObject? node = part switch
                    {
                        AssistantTextPart text => new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = text.Text,
                        },
                        AssistantReasoningPart reasoning => new JsonObject
                        {
                            ["type"] = "reasoning",
                            ["text"] = reasoning.Text,
                        },
                        AssistantToolCallPart call => new JsonObject
                        {
                            ["type"] = "tool-call",
                            ["toolCallId"] = call.ToolCall.ToolCallId,
                            ["toolName"] = call.ToolCall.ToolName,
                            ["input"] = ToolInput(call.ToolCall.ArgumentsJson),
                        },
                        AssistantCustomPart custom => new JsonObject
                        {
                            ["type"] = "custom",
                            ["kind"] = custom.Kind,
                        },
                        _ => null,
                    };
                    if (node != null)
                    {
                        AnthropicJson.Set(node, "providerOptions", Options(part.ProviderOptions));
                        parts.Add(node);
                    }
                }

                var assistantNode = new JsonObject { ["role"] = "assistant", ["content"] = parts };
                AnthropicJson.Set(
                    assistantNode,
                    "providerOptions",
                    Options(assistant.ProviderOptions)
                );
                messages.Add(assistantNode);
            }
            else if (message is ToolModelMessage tool)
            {
                JsonNode value;
                try
                {
                    value = JsonNode.Parse(tool.OutputJson) ?? JsonValue.Create(tool.OutputJson)!;
                }
                catch (JsonException)
                {
                    value = JsonValue.Create(tool.OutputJson)!;
                }

                var output = new JsonObject
                {
                    ["type"] = tool.IsError ? "error-json" : "json",
                    ["value"] = value,
                };
                AnthropicJson.Set(
                    output,
                    "providerOptions",
                    tool.OutputProviderOptions is { } outputOptions
                        ? AnthropicJson.Node(outputOptions)
                        : null
                );
                var result = new JsonObject
                {
                    ["type"] = "tool-result",
                    ["toolName"] = tool.ToolName,
                    ["toolCallId"] = tool.ToolCallId,
                    ["output"] = output,
                };
                AnthropicJson.Set(
                    result,
                    "providerOptions",
                    tool.ProviderMetadata is { } metadata ? AnthropicJson.Node(metadata) : null
                );
                messages.Add(
                    new JsonObject
                    {
                        ["role"] = "tool",
                        ["content"] = new JsonArray { result },
                    }
                );
            }
        }

        using var document = JsonDocument.Parse(messages.ToJsonString());
        return document.RootElement.Clone();
    }

    private static JsonObject FileJson(FileContentPart file)
    {
        var data = new JsonObject();
        if (file.ProviderReference != null)
        {
            var reference = new JsonObject();
            foreach (var pair in file.ProviderReference)
            {
                reference[pair.Key] = pair.Value;
            }

            data["type"] = "reference";
            data["reference"] = reference;
        }
        else if (!string.IsNullOrEmpty(file.Url))
        {
            data["type"] = "url";
            data["url"] = file.Url;
        }
        else if (file.Data != null)
        {
            data["type"] = "data";
            data["data"] = System.Convert.ToBase64String(file.Data);
        }

        var node = new JsonObject
        {
            ["type"] = "file",
            ["mediaType"] = file.MediaType,
            ["data"] = data,
        };
        AnthropicJson.Set(node, "filename", file.FileName);
        AnthropicJson.Set(node, "providerOptions", Options(file.ProviderOptions));
        return node;
    }

    private static JsonNode ToolInput(string argumentsJson)
    {
        try
        {
            var parsed = JsonNode.Parse(
                string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson
            );
            return parsed is JsonObject
                ? parsed
                : new JsonObject { ["rawInvalidInput"] = argumentsJson };
        }
        catch (JsonException)
        {
            return new JsonObject { ["rawInvalidInput"] = argumentsJson };
        }
    }

    private static JsonObject? Options(IReadOnlyDictionary<string, JsonElement>? options)
    {
        if (options == null)
        {
            return null;
        }

        var node = new JsonObject();
        foreach (var pair in options)
        {
            node[pair.Key] = AnthropicJson.Node(pair.Value);
        }

        return node;
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
            var role =
                message.ValueKind == JsonValueKind.String
                    ? "system"
                    : AnthropicJson.String(message, "role");
            var blockRole =
                role == "tool" ? "user"
                : role == "assistant" ? "assistant"
                : role == "system" ? "system"
                : "user";
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
