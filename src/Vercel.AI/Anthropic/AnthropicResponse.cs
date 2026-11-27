// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
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
    public AnthropicParseContext(
        bool usesJsonResponseTool,
        string providerOptionsName,
        bool usedCustomProviderKey,
        IReadOnlyList<CallWarning>? warnings = null
    )
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

    /// <summary>
    /// Marks <c>code_execution</c> server tool calls as dynamic. Set when a web tool that dynamic filtering runs through code execution
    /// is sent without the code execution tool, which the API then allows implicitly.
    /// </summary>
    public bool MarkCodeExecutionDynamic { get; init; }

    /// <summary>
    /// The effective prepared tool definitions that computer toolset member calls are checked against.
    /// Without them, a toolset member call is rejected as undeclared.
    /// </summary>
    public System.Text.Json.Nodes.JsonArray? PreparedTools { get; init; }

    /// <summary>The caller's name for the declared computer toolset. Defaults to <c>computer</c>.</summary>
    public string? ComputerToolsetName { get; init; }

    /// <summary>Id factory for citation sources. Defaults to <c>source-n</c>.</summary>
    public Func<string>? GenerateId { get; set; }

    /// <summary>The documents of the prompt that have citations enabled. A citation refers to one by its document index.</summary>
    internal IReadOnlyList<CitationDocument> CitationDocuments { get; init; } =
        Array.Empty<CitationDocument>();
}

/// <summary>Maps an Anthropic Messages JSON body onto a generate result.</summary>
public static class AnthropicResponse
{
    // JSON.stringify leaves quotes, non-ASCII, and `+` unescaped in the text, so the tool call inputs do too.
    private static readonly JsonSerializerOptions s_compact = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Parses a Messages response body.</summary>
    public static LanguageModelGenerateResult Parse(
        string json,
        AnthropicParseContext context,
        IReadOnlyDictionary<string, string>? headers = null,
        LanguageModelStreamRequest? request = null
    )
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return Parse(document.RootElement, context, json, headers, request);
    }

    /// <summary>Parses a Messages response object.</summary>
    public static LanguageModelGenerateResult Parse(
        JsonElement root,
        AnthropicParseContext context,
        string? raw = null,
        IReadOnlyDictionary<string, string>? headers = null,
        LanguageModelStreamRequest? request = null
    )
    {
        var content = new List<GeneratedContent>();
        var mcpNames = new Dictionary<string, McpToolCall>();
        var serverToolCalls = new Dictionary<string, string>();
        var jsonTool = false;
        var idFactory = context.GenerateId ?? SequentialId();
        if (root.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            var hasWebSearchResults = HasWebSearchResults(parts);
            // The JSON response tool call becomes the text, so other text is dropped,
            // unless the model never called the tool and answered in text instead.
            var fallbackText = context.UsesJsonResponseTool
                ? GetJsonResponseToolFallbackText(parts, StopReason(root))
                : null;
            var position = 0;
            foreach (var part in parts.EnumerateArray())
            {
                MapPart(
                    part,
                    content,
                    context,
                    mcpNames,
                    serverToolCalls,
                    idFactory,
                    hasWebSearchResults,
                    fallbackText == null || fallbackText.Contains(position),
                    ref jsonTool
                );
                position++;
            }
        }

        var stop =
            root.TryGetProperty("stop_reason", out var stopElement)
            && stopElement.ValueKind == JsonValueKind.String
                ? stopElement.GetString()
                : null;
        var usage =
            root.TryGetProperty("usage", out var usageElement)
            && usageElement.ValueKind == JsonValueKind.Object
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
            headers,
            null,
            request
        );
    }

    private static string? StopReason(JsonElement root)
    {
        return
            root.TryGetProperty("stop_reason", out var stop)
            && stop.ValueKind == JsonValueKind.String
            ? stop.GetString()
            : null;
    }

    /// <summary>
    /// Positions of the text parts to return when the JSON response tool is in use: the text after
    /// the last tool call or result, and only if the tool was never called.
    /// </summary>
    private static HashSet<int> GetJsonResponseToolFallbackText(
        JsonElement parts,
        string? stopReason
    )
    {
        var text = new HashSet<int>();
        if (stopReason == "pause_turn" || stopReason == "tool_use")
        {
            return text;
        }

        var position = 0;
        foreach (var part in parts.EnumerateArray())
        {
            var type = AnthropicJson.String(part, "type");
            if (type == "tool_use" && AnthropicJson.String(part, "name") == "json")
            {
                return [];
            }

            if (type == "text")
            {
                text.Add(position);
            }
            else if (type != "thinking" && type != "redacted_thinking")
            {
                text.Clear();
            }

            position++;
        }

        return text;
    }

    private static bool HasWebSearchResults(JsonElement parts)
    {
        foreach (var part in parts.EnumerateArray())
        {
            if (
                AnthropicJson.String(part, "type") == "web_search_tool_result"
                && part.TryGetProperty("content", out var body)
                && body.ValueKind == JsonValueKind.Array
                && body.GetArrayLength() > 0
            )
            {
                return true;
            }
        }

        return false;
    }

    private static void MapPart(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Dictionary<string, McpToolCall> mcpNames,
        Dictionary<string, string> serverToolCalls,
        Func<string> generateId,
        bool hasWebSearchResults,
        bool includeText,
        ref bool jsonTool
    )
    {
        var type = AnthropicJson.String(part, "type");
        switch (type)
        {
            case "text":
                if (includeText)
                {
                    MapText(part, content, context, generateId, hasWebSearchResults);
                }

                break;
            case "thinking":
                content.Add(
                    new AnthropicReasoningContent(
                        AnthropicJson.String(part, "thinking") ?? string.Empty,
                        ObjectElement(
                            new JsonObject
                            {
                                ["anthropic"] = new JsonObject
                                {
                                    ["signature"] = AnthropicJson.String(part, "signature"),
                                },
                            }
                        )
                    )
                );
                break;
            case "redacted_thinking":
                content.Add(
                    new AnthropicReasoningContent(
                        string.Empty,
                        ObjectElement(
                            new JsonObject
                            {
                                ["anthropic"] = new JsonObject
                                {
                                    ["redactedData"] = AnthropicJson.String(part, "data"),
                                },
                            }
                        )
                    )
                );
                break;
            case "container_upload":
                content.Add(
                    new AnthropicCustomContent(
                        "anthropic.container_upload",
                        ObjectElement(
                            new JsonObject
                            {
                                ["anthropic"] = new JsonObject
                                {
                                    ["fileId"] = AnthropicJson.String(part, "file_id"),
                                },
                            }
                        )
                    )
                );
                break;
            case "compaction":
                var compaction = AnthropicJson.String(part, "content");
                if (!string.IsNullOrEmpty(compaction))
                {
                    var compactionMetadata = new JsonObject { ["type"] = "compaction" };
                    var signature = AnthropicJson.String(part, "signature");
                    if (signature != null)
                    {
                        compactionMetadata["signature"] = signature;
                    }

                    content.Add(
                        new AnthropicText(
                            compaction!,
                            ObjectElement(new JsonObject { ["anthropic"] = compactionMetadata })
                        )
                    );
                }

                break;
            case "tool_use":
                MapToolUse(part, content, context, ref jsonTool);
                break;
            case "server_tool_use":
                MapServerTool(part, content, context, serverToolCalls);
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
            case "tool_search_tool_result":
                MapToolSearch(part, content, context, serverToolCalls);
                break;
            case "advisor_tool_result":
                MapAdvisor(part, content, context);
                break;
            case "fallback":
                break;
        }
    }

    private static void MapText(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Func<string> generateId,
        bool hasWebSearchResults
    )
    {
        var text = AnthropicJson.String(part, "text") ?? string.Empty;
        var citations = new List<JsonElement>();
        if (
            part.TryGetProperty("citations", out var citationArray)
            && citationArray.ValueKind == JsonValueKind.Array
        )
        {
            citations.AddRange(citationArray.EnumerateArray());
        }

        if (citations.Count > 0)
        {
            content.Add(
                new AnthropicText(
                    text,
                    AnthropicCitations.TextMetadata(citations),
                    AnthropicCitations.MapAll(citations, context.CitationDocuments)
                )
            );
        }
        else
        {
            content.Add(new GeneratedText(text));
        }

        foreach (var citation in citations)
        {
            // Search results are already sources. Only a citation without them falls back to a source.
            if (
                hasWebSearchResults
                && AnthropicJson.String(citation, "type") == "web_search_result_location"
            )
            {
                continue;
            }

            var source = CitationSource(citation, context.CitationDocuments, generateId);
            if (source != null)
            {
                content.Add(source);
            }
        }
    }

    private static GeneratedContent? CitationSource(
        JsonElement citation,
        IReadOnlyList<CitationDocument> documents,
        Func<string> generateId
    )
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
            return new GeneratedSource(
                generateId(),
                AnthropicJson.String(citation, "url") ?? string.Empty,
                AnthropicJson.String(citation, "title"),
                ObjectElement(metadata)
            );
        }

        if (type != "page_location" && type != "char_location")
        {
            return null;
        }

        var documentIndex =
            citation.TryGetProperty("document_index", out var indexValue)
            && indexValue.ValueKind == JsonValueKind.Number
                ? indexValue.GetInt32()
                : -1;
        if (documentIndex < 0 || documentIndex >= documents.Count)
        {
            return null;
        }

        var document = documents[documentIndex];
        var documentMetadata = new JsonObject
        {
            ["citedText"] = AnthropicJson.String(citation, "cited_text"),
        };
        if (type == "page_location")
        {
            documentMetadata["startPageNumber"] = Number(citation, "start_page_number");
            documentMetadata["endPageNumber"] = Number(citation, "end_page_number");
        }
        else
        {
            documentMetadata["startCharIndex"] = Number(citation, "start_char_index");
            documentMetadata["endCharIndex"] = Number(citation, "end_char_index");
        }

        return new GeneratedDocumentSource(
            generateId(),
            document.MediaType,
            AnthropicJson.String(citation, "document_title") ?? document.Title,
            document.Filename,
            ObjectElement(new JsonObject { ["anthropic"] = documentMetadata })
        );
    }

    private static JsonNode? Number(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? AnthropicJson.Node(value)
            : null;
    }

    private static void MapToolUse(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        ref bool jsonTool
    )
    {
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var input = part.TryGetProperty("input", out var inputElement) ? inputElement : default;
        if (context.UsesJsonResponseTool && name == "json")
        {
            jsonTool = true;
            content.Add(
                new GeneratedText(
                    input.ValueKind == JsonValueKind.Undefined
                        ? "null"
                        : AnthropicJson.Compact(input)
                )
            );
            return;
        }

        var toolset = AnthropicJson.String(part, "toolset_name");
        JsonElement? metadata = CallerMetadata(part);
        if (toolset != null)
        {
            AnthropicToolset.Validate(toolset, name, context.PreparedTools);
            var memberInput = AnthropicToolset.MemberInput(
                name,
                input.ValueKind == JsonValueKind.Undefined
                    ? null
                    : JsonNode.Parse(input.GetRawText())
            );
            var toolsetMetadata = new JsonObject { ["toolsetName"] = toolset };
            if (
                metadata != null
                && AnthropicJson.Node(metadata.Value) is JsonObject callerMetadata
                && callerMetadata["anthropic"] is JsonObject callerInfo
            )
            {
                foreach (var property in callerInfo)
                {
                    toolsetMetadata[property.Key] = property.Value?.DeepClone();
                }
            }

            content.Add(
                new GeneratedToolCall(
                    id,
                    context.ComputerToolsetName ?? toolset,
                    memberInput,
                    ObjectElement(new JsonObject { ["anthropic"] = toolsetMetadata })
                )
            );
            return;
        }

        content.Add(
            new GeneratedToolCall(
                id,
                CustomName(context, name),
                input.ValueKind == JsonValueKind.Undefined ? "{}" : AnthropicJson.Compact(input),
                metadata
            )
        );
    }

    private static void MapServerTool(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Dictionary<string, string> serverToolCalls
    )
    {
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var input = part.TryGetProperty("input", out var inputElement)
            ? AnthropicJson.Node(inputElement)
            : new JsonObject();
        string toolName;
        if (name == "text_editor_code_execution" || name == "bash_code_execution")
        {
            toolName = "code_execution";
            input = WithType(name, input);
        }
        else if (
            name == "code_execution"
            && input is JsonObject code
            && code["code"] != null
            && code["type"] == null
        )
        {
            toolName = "code_execution";
            input = WithType("programmatic-tool-call", code);
        }
        else if (
            name == "web_search"
            || name == "code_execution"
            || name == "web_fetch"
            || name == "tool_search_tool_regex"
            || name == "tool_search_tool_bm25"
            || name == "advisor"
        )
        {
            toolName = name;
        }
        else
        {
            return;
        }

        if (name == "tool_search_tool_regex" || name == "tool_search_tool_bm25")
        {
            serverToolCalls[id] = name;
        }

        content.Add(
            new GeneratedToolCall(
                id,
                CustomName(context, toolName),
                input.ToJsonString(s_compact),
                CallerMetadata(part)
            )
            {
                ProviderExecuted = true,
                Dynamic =
                    context.MarkCodeExecutionDynamic
                    && (
                        name == "code_execution"
                        || name == "text_editor_code_execution"
                        || name == "bash_code_execution"
                    ),
            }
        );
    }

    private static void MapMcpUse(
        JsonElement part,
        List<GeneratedContent> content,
        Dictionary<string, McpToolCall> mcpNames
    )
    {
        var id = AnthropicJson.String(part, "id") ?? "tool";
        var name = AnthropicJson.String(part, "name") ?? string.Empty;
        var metadata = ObjectElement(
            new JsonObject
            {
                ["anthropic"] = new JsonObject
                {
                    ["type"] = "mcp-tool-use",
                    ["serverName"] = AnthropicJson.String(part, "server_name"),
                },
            }
        );
        mcpNames[id] = new McpToolCall(name, metadata);
        var input = part.TryGetProperty("input", out var inputElement)
            ? AnthropicJson.Compact(inputElement)
            : "{}";
        content.Add(
            new GeneratedToolCall(id, name, input, metadata)
            {
                ProviderExecuted = true,
                Dynamic = true,
            }
        );
    }

    private static void MapMcpResult(
        JsonElement part,
        List<GeneratedContent> content,
        Dictionary<string, McpToolCall> mcpNames
    )
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        mcpNames.TryGetValue(id, out var call);

        var result = part.TryGetProperty("content", out var body) ? body.GetRawText() : "null";
        content.Add(
            new AnthropicToolResultContent(
                id,
                call.Name ?? string.Empty,
                result,
                AnthropicJson.Bool(part, "is_error") == true,
                call.Metadata
            )
            {
                Dynamic = true,
            }
        );
    }

    private static void MapWebSearch(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Func<string> generateId
    )
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
                    ["pageAge"] =
                        result.TryGetProperty("page_age", out var age)
                        && age.ValueKind != JsonValueKind.Undefined
                            ? AnthropicJson.Node(age)
                            : null,
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

            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    results.ToJsonString(),
                    false,
                    CallerMetadata(part)
                )
            );
            foreach (var result in body.EnumerateArray())
            {
                var metadata = ObjectElement(
                    new JsonObject
                    {
                        ["anthropic"] = new JsonObject
                        {
                            ["pageAge"] =
                                result.TryGetProperty("page_age", out var age)
                                && age.ValueKind != JsonValueKind.Null
                                    ? AnthropicJson.Node(age)
                                    : null,
                        },
                    }
                );
                content.Add(
                    new GeneratedSource(
                        generateId(),
                        AnthropicJson.String(result, "url") ?? string.Empty,
                        AnthropicJson.String(result, "title"),
                        metadata
                    )
                );
            }
        }
        else
        {
            var error = new JsonObject
            {
                ["type"] = "web_search_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    error.ToJsonString(),
                    true,
                    CallerMetadata(part)
                )
            );
        }
    }

    private static void MapWebFetch(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context
    )
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
            var fetched = new JsonObject
            {
                ["type"] = "web_fetch_result",
                ["url"] = AnthropicJson.String(body, "url"),
                ["retrievedAt"] = Copy(body, "retrieved_at"),
            };
            if (body.TryGetProperty("content", out var document))
            {
                var documentNode = new JsonObject { ["type"] = Copy(document, "type") };
                SetIfPresent(documentNode, "title", document, "title");
                SetIfPresent(documentNode, "citations", document, "citations");
                if (document.TryGetProperty("source", out var source))
                {
                    documentNode["source"] = new JsonObject
                    {
                        ["type"] = Copy(source, "type"),
                        ["mediaType"] = Copy(source, "media_type"),
                        ["data"] = Copy(source, "data"),
                    };
                }

                fetched["content"] = documentNode;
            }

            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    fetched.ToJsonString(s_compact),
                    false,
                    CallerMetadata(part)
                )
            );
        }
        else if (contentType == "web_fetch_tool_result_error")
        {
            var error = new JsonObject
            {
                ["type"] = "web_fetch_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    error.ToJsonString(),
                    true,
                    CallerMetadata(part)
                )
            );
        }
    }

    private static void MapCodeExecution(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context
    )
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        var name = CustomName(context, "code_execution");
        if (AnthropicJson.String(part, "type") != "code_execution_tool_result")
        {
            // code execution 20250825: the result is passed through.
            content.Add(new AnthropicToolResultContent(id, name, body.GetRawText(), false, null));
            return;
        }

        var type = AnthropicJson.String(body, "type");
        if (type == "code_execution_result" || type == "encrypted_code_execution_result")
        {
            var result = new JsonObject { ["type"] = type };
            if (type == "code_execution_result")
            {
                result["stdout"] = Copy(body, "stdout");
            }
            else
            {
                result["encrypted_stdout"] = Copy(body, "encrypted_stdout");
            }

            result["stderr"] = Copy(body, "stderr");
            result["return_code"] = Copy(body, "return_code");
            result["content"] = Copy(body, "content") ?? new JsonArray();
            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    result.ToJsonString(s_compact),
                    false,
                    null
                )
            );
        }
        else if (type == "code_execution_tool_result_error")
        {
            var error = new JsonObject
            {
                ["type"] = "code_execution_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(
                new AnthropicToolResultContent(id, name, error.ToJsonString(s_compact), true, null)
            );
        }
    }

    private static void MapToolSearch(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context,
        Dictionary<string, string> serverToolCalls
    )
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        if (!serverToolCalls.TryGetValue(id, out var providerToolName))
        {
            // The call was made in an earlier response, so the custom name tells which variant is in use.
            providerToolName =
                CustomName(context, "tool_search_tool_bm25") != "tool_search_tool_bm25"
                    ? "tool_search_tool_bm25"
                    : "tool_search_tool_regex";
        }

        var name = CustomName(context, providerToolName);
        if (AnthropicJson.String(body, "type") == "tool_search_tool_search_result")
        {
            var references = new JsonArray();
            if (
                body.TryGetProperty("tool_references", out var toolReferences)
                && toolReferences.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var reference in toolReferences.EnumerateArray())
                {
                    references.Add(
                        new JsonObject
                        {
                            ["type"] = Copy(reference, "type"),
                            ["toolName"] = Copy(reference, "tool_name"),
                        }
                    );
                }
            }

            content.Add(
                new AnthropicToolResultContent(
                    id,
                    name,
                    references.ToJsonString(s_compact),
                    false,
                    null
                )
            );
        }
        else
        {
            var error = new JsonObject
            {
                ["type"] = "tool_search_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
            content.Add(
                new AnthropicToolResultContent(id, name, error.ToJsonString(s_compact), true, null)
            );
        }
    }

    private static void MapAdvisor(
        JsonElement part,
        List<GeneratedContent> content,
        AnthropicParseContext context
    )
    {
        var id = AnthropicJson.String(part, "tool_use_id") ?? string.Empty;
        if (!part.TryGetProperty("content", out var body))
        {
            return;
        }

        var name = CustomName(context, "advisor");
        var type = AnthropicJson.String(body, "type");
        JsonObject result;
        var isError = false;
        if (type == "advisor_result")
        {
            result = new JsonObject { ["type"] = "advisor_result", ["text"] = Copy(body, "text") };
        }
        else if (type == "advisor_redacted_result")
        {
            result = new JsonObject
            {
                ["type"] = "advisor_redacted_result",
                ["encryptedContent"] = Copy(body, "encrypted_content"),
            };
        }
        else
        {
            isError = true;
            result = new JsonObject
            {
                ["type"] = "advisor_tool_result_error",
                ["errorCode"] = AnthropicJson.String(body, "error_code"),
            };
        }

        if (!isError && AnthropicJson.String(body, "stop_reason") is { } stopReason)
        {
            result["stopReason"] = stopReason;
        }

        content.Add(
            new AnthropicToolResultContent(id, name, result.ToJsonString(s_compact), isError, null)
        );
    }

    private static string Stringify(JsonElement element)
    {
        return JsonSerializer.Serialize(element, s_compact);
    }

    private static JsonNode? Copy(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? JsonNode.Parse(value.GetRawText())
            : null;
    }

    private static void SetIfPresent(JsonObject target, string to, JsonElement element, string from)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(from, out var value)
        )
        {
            target[to] = JsonNode.Parse(value.GetRawText());
        }
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
        string? mapped;
        return context.ToolNames.TryGetValue(apiName, out mapped) ? mapped : apiName;
    }

    private static JsonElement? CallerMetadata(JsonElement part)
    {
        if (
            !part.TryGetProperty("caller", out var caller)
            || caller.ValueKind != JsonValueKind.Object
        )
        {
            return null;
        }

        var info = new JsonObject { ["type"] = AnthropicJson.String(caller, "type") };
        var toolId = AnthropicJson.String(caller, "tool_id");
        if (toolId != null)
        {
            info["toolId"] = toolId;
        }

        return ObjectElement(
            new JsonObject { ["anthropic"] = new JsonObject { ["caller"] = info } }
        );
    }

    private static JsonElement Metadata(JsonElement root, AnthropicParseContext context)
    {
        var anthropic = new JsonObject();
        if (root.TryGetProperty("usage", out var usage))
        {
            anthropic["usage"] = AnthropicJson.Node(usage);
        }

        anthropic["stopSequence"] =
            root.TryGetProperty("stop_sequence", out var sequence)
            && sequence.ValueKind == JsonValueKind.String
                ? JsonValue.Create(sequence.GetString())
                : null;
        var serviceTier =
            usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("service_tier", out var usageTier)
            && usageTier.ValueKind == JsonValueKind.String
                ? usageTier.GetString()
            : root.TryGetProperty("service_tier", out var rootTier)
            && rootTier.ValueKind == JsonValueKind.String
                ? rootTier.GetString()
            : null;
        if (serviceTier != null)
        {
            anthropic["serviceTier"] = serviceTier;
        }

        if (
            root.TryGetProperty("stop_details", out var stopDetails)
            && stopDetails.ValueKind == JsonValueKind.Object
        )
        {
            var details = new JsonObject { ["type"] = AnthropicJson.String(stopDetails, "type") };
            CopyString(stopDetails, "category", details, "category");
            CopyString(stopDetails, "explanation", details, "explanation");
            CopyString(stopDetails, "recommended_model", details, "recommendedModel");
            anthropic["stopDetails"] = details;
        }

        if (
            root.TryGetProperty("input_transformations", out var transformations)
            && transformations.ValueKind != JsonValueKind.Null
        )
        {
            anthropic["inputTransformations"] = AnthropicJson.Node(transformations);
        }

        if (
            root.TryGetProperty("safeguard_results", out var safeguardResults)
            && safeguardResults.ValueKind != JsonValueKind.Null
        )
        {
            anthropic["safeguardResults"] = AnthropicJson.Node(safeguardResults);
        }

        anthropic["iterations"] =
            usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("iterations", out var iterations)
            && iterations.ValueKind == JsonValueKind.Array
                ? MapIterations(iterations)
                : null;
        anthropic["container"] =
            root.TryGetProperty("container", out var container)
            && container.ValueKind == JsonValueKind.Object
                ? MapContainer(container)
                : null;
        anthropic["contextManagement"] =
            root.TryGetProperty("context_management", out var contextManagement)
            && contextManagement.ValueKind == JsonValueKind.Object
                ? MapContextManagement(contextManagement)
                : null;

        var metadata = new JsonObject { ["anthropic"] = anthropic };
        if (context.UsedCustomProviderKey && context.ProviderOptionsName != "anthropic")
        {
            metadata[context.ProviderOptionsName] = anthropic.DeepClone();
        }

        return ObjectElement(metadata);
    }

    private static JsonArray MapIterations(JsonElement iterations)
    {
        var mapped = new JsonArray();
        foreach (var iteration in iterations.EnumerateArray())
        {
            var item = new JsonObject { ["type"] = AnthropicJson.String(iteration, "type") };
            CopyString(iteration, "model", item, "model");
            item["inputTokens"] = iteration.TryGetProperty("input_tokens", out var input)
                ? AnthropicJson.Node(input)
                : null;
            item["outputTokens"] = iteration.TryGetProperty("output_tokens", out var output)
                ? AnthropicJson.Node(output)
                : null;
            CopyTruthyNumber(
                iteration,
                "cache_creation_input_tokens",
                item,
                "cacheCreationInputTokens"
            );
            CopyTruthyNumber(iteration, "cache_read_input_tokens", item, "cacheReadInputTokens");
            mapped.Add(item);
        }

        return mapped;
    }

    private static JsonObject MapContainer(JsonElement container)
    {
        JsonNode? skills = null;
        if (
            container.TryGetProperty("skills", out var skillArray)
            && skillArray.ValueKind == JsonValueKind.Array
        )
        {
            var mapped = new JsonArray();
            foreach (var skill in skillArray.EnumerateArray())
            {
                mapped.Add(
                    new JsonObject
                    {
                        ["type"] = AnthropicJson.String(skill, "type"),
                        ["skillId"] = AnthropicJson.String(skill, "skill_id"),
                        ["version"] = AnthropicJson.String(skill, "version"),
                    }
                );
            }

            skills = mapped;
        }

        return new JsonObject
        {
            ["expiresAt"] = AnthropicJson.String(container, "expires_at"),
            ["id"] = AnthropicJson.String(container, "id"),
            ["skills"] = skills,
        };
    }

    private static JsonObject MapContextManagement(JsonElement contextManagement)
    {
        var edits = new JsonArray();
        if (
            contextManagement.TryGetProperty("applied_edits", out var applied)
            && applied.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var edit in applied.EnumerateArray())
            {
                var type = AnthropicJson.String(edit, "type");
                var mapped = new JsonObject { ["type"] = type };
                switch (type)
                {
                    case "clear_tool_uses_20250919":
                        mapped["clearedToolUses"] = edit.TryGetProperty(
                            "cleared_tool_uses",
                            out var uses
                        )
                            ? AnthropicJson.Node(uses)
                            : null;
                        mapped["clearedInputTokens"] = edit.TryGetProperty(
                            "cleared_input_tokens",
                            out var toolTokens
                        )
                            ? AnthropicJson.Node(toolTokens)
                            : null;
                        break;
                    case "clear_thinking_20251015":
                        mapped["clearedThinkingTurns"] = edit.TryGetProperty(
                            "cleared_thinking_turns",
                            out var turns
                        )
                            ? AnthropicJson.Node(turns)
                            : null;
                        mapped["clearedInputTokens"] = edit.TryGetProperty(
                            "cleared_input_tokens",
                            out var thinkingTokens
                        )
                            ? AnthropicJson.Node(thinkingTokens)
                            : null;
                        break;
                    case "compact_20260112":
                        break;
                    default:
                        // An edit of an unknown type is left out.
                        continue;
                }

                edits.Add(mapped);
            }
        }

        return new JsonObject { ["appliedEdits"] = edits };
    }

    private static void CopyString(
        JsonElement source,
        string name,
        JsonObject target,
        string targetName
    )
    {
        if (AnthropicJson.String(source, name) is { } value)
        {
            target[targetName] = value;
        }
    }

    private static void CopyTruthyNumber(
        JsonElement source,
        string name,
        JsonObject target,
        string targetName
    )
    {
        if (
            source.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.GetDouble() != 0
        )
        {
            target[targetName] = AnthropicJson.Node(value);
        }
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

    private readonly record struct McpToolCall(string Name, JsonElement Metadata);
}
