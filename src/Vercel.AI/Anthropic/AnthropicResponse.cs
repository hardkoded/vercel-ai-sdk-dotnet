// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Settings for mapping an Anthropic Messages response onto SDK content.</summary>
public sealed class AnthropicParseContext
{
    /// <summary>Creates parse settings.</summary>
    public AnthropicParseContext(bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey, IReadOnlyList<CallWarning>? warnings = null)
    {
        UsesJsonResponseTool = usesJsonResponseTool;
        ProviderOptionsName = providerOptionsName ?? "anthropic";
        UsedCustomProviderKey = usedCustomProviderKey;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        ToolNames = new Dictionary<string, string>();
    }

    /// <summary>True when structured output was requested through the json tool.</summary>
    public bool UsesJsonResponseTool { get; }

    /// <summary>Provider-options key.</summary>
    public string ProviderOptionsName { get; }

    /// <summary>True when a custom provider-options key was present.</summary>
    public bool UsedCustomProviderKey { get; }

    /// <summary>Warnings from request preparation.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Maps an API tool name back to the caller's tool name.</summary>
    public Dictionary<string, string> ToolNames { get; }

    /// <summary>Id factory for citation sources. Defaults to <c>source-n</c>.</summary>
    public Func<string>? GenerateId { get; set; }
}

/// <summary>Maps an Anthropic Messages JSON body onto a generate result.</summary>
public static class AnthropicResponse
{
    /// <summary>Parses a Messages response body.</summary>
    public static LanguageModelGenerateResult Parse(string json, AnthropicParseContext context, IReadOnlyDictionary<string, string>? headers = null)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return Parse(document.RootElement, context, json, headers);
    }

    /// <summary>Parses a Messages response object.</summary>
    public static LanguageModelGenerateResult Parse(JsonElement root, AnthropicParseContext context, string? raw = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        var content = new List<GeneratedContent>();
        var mcpNames = new Dictionary<string, string>();
        var jsonTool = false;
        var idFactory = context.GenerateId ?? SequentialId();
        if (root.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                MapPart(part, content, context, mcpNames, idFactory, ref jsonTool);
            }
        }

        var stop = root.TryGetProperty("stop_reason", out var stopElement) && stopElement.ValueKind == JsonValueKind.String
            ? stopElement.GetString()
            : null;
        var usage = root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object
            ? AnthropicUsage.Convert(usageElement)
            : LanguageModelUsage.Empty;
        var metadata = Metadata(root, context);
        return new LanguageModelGenerateResult(
            content,
            AnthropicStopReason.Map(stop, jsonTool),
            usage,
            stop,
            context.Warnings,
            root.TryGetProperty("id", out var id) ? id.GetString() : null,
            metadata,
            raw,
            root.TryGetProperty("model", out var model) ? model.GetString() : null,
            null,
            headers);
    }

    private static void MapPart(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Dictionary<string, string> mcpNames,
        Func<string> generateId,
        ref bool jsonTool)
    {
        var type = AnthropicJson.String(part, "type");
        switch (type)
        {
            case "text":
                if (!context.UsesJsonResponseTool)
                {
                    MapText(part, content, generateId);
                }

                break;
            case "thinking":
                content.Add(new AnthropicReasoningContent(
                    AnthropicJson.String(part, "thinking") ?? string.Empty,
                    ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["signature"] = AnthropicJson.String(part, "signature") } })));
                break;
            case "redacted_thinking":
                content.Add(new AnthropicReasoningContent(
                    string.Empty,
                    ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["redactedData"] = AnthropicJson.String(part, "data") } })));
                break;
            case "container_upload":
                content.Add(new AnthropicCustomContent(
                    "anthropic.container_upload",
                    ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["fileId"] = AnthropicJson.String(part, "file_id") } })));
                break;
            case "compaction":
                var compaction = AnthropicJson.String(part, "content");
                if (!string.IsNullOrEmpty(compaction))
                {
                    content.Add(new AnthropicText(compaction!, ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["type"] = "compaction" } })));
                }

                break;
            case "tool_use":
                MapToolUse(part, content, context, ref jsonTool);
                break;
            case "server_tool_use":
                MapServerTool(part, content, context);
                break;
            case "mcp_tool_use":
                MapMcpUse(part, content, mcpNames);
                break;
            case "mcp_tool_result":
                MapMcpResult(part, content, mcpNames);
                break;
            case "web_search_tool_result":
                MapWebSearch(part, content, context, generateId);
                break;
            case "web_fetch_tool_result":
                MapWebFetch(part, content, context);
                break;
            case "code_execution_tool_result":
            case "bash_code_execution_tool_result":
            case "text_editor_code_execution_tool_result":
                MapCodeExecution(part, content, context);
                break;
            case "fallback":
                break;
        }
    }

    private static void MapText(JsonElement part, List<GeneratedContent> content, Func<string> generateId)
    {
        var text = AnthropicJson.String(part, "text") ?? string.Empty;
        JsonArray? web = null;
        if (part.TryGetProperty("citations", out var citations) && citations.ValueKind == JsonValueKind.Array)
        {
            foreach (var citation in citations.EnumerateArray())
            {
                if (AnthropicJson.String(citation, "type") == "web_search_result_location")
                {
                    web = web ?? new JsonArray();
                    web.Add(AnthropicJson.Node(citation));
                }
            }
        }

        if (web != null && web.Count > 0)
        {
            content.Add(new AnthropicText(text, ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["citations"] = web } })));
        }
        else
        {
            content.Add(new GeneratedText(text));
        }

        if (part.TryGetProperty("citations", out var all) && all.ValueKind == JsonValueKind.Array)
        {
            foreach (var citation in all.EnumerateArray())
            {
                var source = CitationSource(citation, generateId);
                if (source != null)
                {
                    content.Add(source);
                }
            }
        }
    }

    private static GeneratedSource? CitationSource(JsonElement citation, Func<string> generateId)
    {
        var type = AnthropicJson.String(citation, "type");
        if (type == "web_search_result_location")
        {
            var metadata = new JsonObject
            {
                ["anthropic"] = new JsonObject
                {
                    ["citedText"] = AnthropicJson.String(citation, "cited_text"),
                    ["encryptedIndex"] = AnthropicJson.String(citation, "encrypted_index"),
                },
            };
            return new GeneratedSource(generateId(), AnthropicJson.String(citation, "url") ?? string.Empty, AnthropicJson.String(citation, "title"), ObjectElement(metadata));
        }

        return null;
    }

    private static void MapToolUse(JsonElement part, List<GeneratedContent> content, AnthropicParseContext context, ref bool jsonTool)
    {
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var input = part.TryGetProperty("input", out var inputElement) ? inputElement : default;
        if (context.UsesJsonResponseTool && name == "json")
        {
            jsonTool = true;
            content.Add(new GeneratedText(input.ValueKind == JsonValueKind.Undefined ? "null" : input.GetRawText()));
            return;
        }

        var toolset = AnthropicJson.String(part, "toolset_name");
        JsonElement? metadata = CallerMetadata(part);
        if (!string.IsNullOrEmpty(toolset))
        {
            var wrapped = new JsonObject { ["action"] = name };
            if (input.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in input.EnumerateObject())
                {
                    wrapped[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }
            }

            content.Add(new GeneratedToolCall(id, CustomName(context, toolset!), wrapped.ToJsonString(), metadata));
            return;
        }

        content.Add(new GeneratedToolCall(id, CustomName(context, name), input.ValueKind == JsonValueKind.Undefined ? "{}" : input.GetRawText(), metadata));
    }

    private static void MapServerTool(JsonElement part, List<GeneratedContent> content, AnthropicParseContext context)
    {
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var input = part.TryGetProperty("input", out var inputElement) ? AnthropicJson.Node(inputElement) : new JsonObject();
        string toolName;
        if (name == "text_editor_code_execution" || name == "bash_code_execution")
        {
            toolName = "code_execution";
            input = WithType(name, input);
        }
        else if (name == "code_execution" && input is JsonObject code && code["code"] != null && code["type"] == null)
        {
            toolName = "code_execution";
            input = WithType("programmatic-tool-call", code);
        }
        else if (name == "web_search" || name == "code_execution" || name == "web_fetch" || name == "tool_search_tool_regex" || name == "tool_search_tool_bm25" || name == "advisor")
        {
            toolName = name;
        }
        else
        {
            return;
        }

        content.Add(new GeneratedToolCall(id, CustomName(context, toolName), input.ToJsonString(), CallerMetadata(part)));
    }

    private static void MapMcpUse(JsonElement part, List<GeneratedContent> content, Dictionary<string, string> mcpNames)
    {
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        mcpNames[id] = name;
        var metadata = ObjectElement(new JsonObject
        {
            ["anthropic"] = new JsonObject
            {
                ["type"] = "mcp-tool-use",
                ["serverName"] = AnthropicJson.String(part, "server_name"),
            },
        });
        var input = part.TryGetProperty("input", out var inputElement) ? inputElement.GetRawText() : "{}";
        content.Add(new GeneratedToolCall(id, name, input, metadata));
    }

    private static void MapMcpResult(JsonElement part, List<GeneratedContent> content, Dictionary<string, string> mcpNames)
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        string? name;
        if (!mcpNames.TryGetValue(id, out name))
        {
            name = string.Empty;
        }

        var result = part.TryGetProperty("content", out var body) ? body.GetRawText() : "null";
        content.Add(new AnthropicToolResultContent(id, name, result, AnthropicJson.Bool(part, "is_error") == true, null));
    }

    private static void MapWebSearch(JsonElement part, List<GeneratedContent> content, AnthropicParseContext context, Func<string> generateId)
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        var name = CustomName(context, "web_search");
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        if (body.ValueKind == JsonValueKind.Array)
        {
            var results = new JsonArray();
            foreach (var result in body.EnumerateArray())
            {
                var item = new JsonObject
                {
                    ["url"] = AnthropicJson.String(result, "url"),
                    ["pageAge"] = result.TryGetProperty("page_age", out var age) && age.ValueKind != JsonValueKind.Undefined ? AnthropicJson.Node(age) : null,
                    ["encryptedContent"] = AnthropicJson.String(result, "encrypted_content"),
                    ["type"] = AnthropicJson.String(result, "type"),
                };
                var title = AnthropicJson.String(result, "title");
                if (title != null)
                {
                    item["title"] = title;
                }

                results.Add(item);
            }

            content.Add(new AnthropicToolResultContent(id, name, results.ToJsonString(), false, CallerMetadata(part)));
            foreach (var result in body.EnumerateArray())
            {
                var metadata = ObjectElement(new JsonObject
                {
                    ["anthropic"] = new JsonObject
                    {
                        ["pageAge"] = result.TryGetProperty("page_age", out var age) && age.ValueKind != JsonValueKind.Null ? AnthropicJson.Node(age) : null,
                    },
                });
                content.Add(new GeneratedSource(generateId(), AnthropicJson.String(result, "url") ?? string.Empty, AnthropicJson.String(result, "title"), metadata));
            }
        }
        else
        {
            var error = new JsonObject
            {
                ["type"] = "web_search_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(new AnthropicToolResultContent(id, name, error.ToJsonString(), true, CallerMetadata(part)));
        }
    }

    private static void MapWebFetch(JsonElement part, List<GeneratedContent> content, AnthropicParseContext context)
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        var name = CustomName(context, "web_fetch");
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        var contentType = AnthropicJson.String(body, "type");
        if (contentType == "web_fetch_result")
        {
            content.Add(new AnthropicToolResultContent(id, name, body.GetRawText(), false, CallerMetadata(part)));
        }
        else if (contentType == "web_fetch_tool_result_error")
        {
            var error = new JsonObject
            {
                ["type"] = "web_fetch_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(new AnthropicToolResultContent(id, name, error.ToJsonString(), true, CallerMetadata(part)));
        }
    }

    private static void MapCodeExecution(JsonElement part, List<GeneratedContent> content, AnthropicParseContext context)
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        content.Add(new AnthropicToolResultContent(id, CustomName(context, "code_execution"), body.GetRawText(), false, null));
    }

    private static JsonObject WithType(string type, JsonNode input)
    {
        var wrapped = new JsonObject { ["type"] = type };
        if (input is JsonObject source)
        {
            foreach (var property in source)
            {
                if (property.Key != "type")
                {
                    wrapped[property.Key] = property.Value?.DeepClone();
                }
            }
        }

        return wrapped;
    }

    private static string CustomName(AnthropicParseContext context, string apiName)
    {
        string mapped;
        return context.ToolNames.TryGetValue(apiName, out mapped) ? mapped : apiName;
    }

    private static JsonElement? CallerMetadata(JsonElement part)
    {
        if (!part.TryGetProperty("caller", out var caller) || caller.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var info = new JsonObject { ["type"] = AnthropicJson.String(caller, "type") };
        var toolId = AnthropicJson.String(caller, "tool_id");
        if (toolId != null)
        {
            info["toolId"] = toolId;
        }

        return ObjectElement(new JsonObject { ["anthropic"] = new JsonObject { ["caller"] = info } });
    }

    private static JsonElement Metadata(JsonElement root, AnthropicParseContext context)
    {
        var anthropic = new JsonObject();
        if (root.TryGetProperty("usage", out var usage))
        {
            anthropic["usage"] = AnthropicJson.Node(usage);
        }

        anthropic["stopSequence"] = root.TryGetProperty("stop_sequence", out var sequence) && sequence.ValueKind == JsonValueKind.String
            ? JsonValue.Create(sequence.GetString())
            : null;
        if (root.TryGetProperty("container", out var container) && container.ValueKind == JsonValueKind.Object)
        {
            var mapped = new JsonObject
            {
                ["expiresAt"] = AnthropicJson.String(container, "expires_at"),
                ["id"] = AnthropicJson.String(container, "id"),
            };
            anthropic["container"] = mapped;
        }

        var metadata = new JsonObject { ["anthropic"] = anthropic };
        if (context.UsedCustomProviderKey && context.ProviderOptionsName != "anthropic")
        {
            metadata[context.ProviderOptionsName] = anthropic.DeepClone();
        }

        return ObjectElement(metadata);
    }

    private static JsonElement ObjectElement(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    private static Func<string> SequentialId()
    {
        var n = 0;
        return () => "source-" + (n++).ToString(CultureInfo.InvariantCulture);
    }
}
