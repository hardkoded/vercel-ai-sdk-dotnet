// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Turns Anthropic SSE JSON events into SDK stream parts.</summary>
public sealed class AnthropicStream
{
    private readonly bool _includeRaw;
    private readonly bool _usesJsonTool;
    private readonly string _providerOptionsName;
    private readonly bool _usedCustomProviderKey;
    private readonly Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
    private bool _invalid;
    private bool _open;
    private string? _activeId;
    private string? _finish;
    private JsonElement? _usage;
    private readonly JsonObject _accumulatedUsage = new()
    {
        ["input_tokens"] = 0,
        ["output_tokens"] = 0,
        ["cache_creation_input_tokens"] = 0,
        ["cache_read_input_tokens"] = 0,
        ["iterations"] = null,
    };
    private string? _stopSequence;
    private string? _serviceTier;
    private JsonNode? _container;
    private bool _jsonTool;
    private string? _blockType;
    private JsonNode? _contextManagement;
    private JsonNode? _stopDetails;
    private JsonNode? _inputTransformations;
    private JsonNode? _safeguardResults;
    private bool _hasWebSearchResults;
    private readonly List<JsonElement> _fallbackWebCitations = [];
    private readonly Dictionary<string, string> _serverToolCalls = [];
    private readonly Dictionary<string, (string ToolName, JsonElement? Metadata)> _mcpToolCalls =
    [];
    private List<CitationDocument>? _citationDocuments;

    /// <summary>
    /// Marks <c>code_execution</c> server tool calls as dynamic. Set when a web tool that dynamic filtering runs through code execution
    /// is sent without the code execution tool, which the API then allows implicitly.
    /// </summary>
    public bool MarkCodeExecutionDynamic { get; init; }

    /// <summary>Emits a <see cref="PingStreamPart"/> for each <c>ping</c> event. Maps to <c>includePings</c>.</summary>
    public bool IncludePings { get; init; }

    /// <summary>The effective prepared tool definitions that computer toolset member calls are checked against.</summary>
    public JsonArray? PreparedTools { get; init; }

    /// <summary>The caller's name for the declared computer toolset. Defaults to <c>computer</c>.</summary>
    public string? ComputerToolsetName { get; init; }

    /// <summary>Creates the ids of source parts. Maps to <c>generateId</c>.</summary>
    public Func<string>? GenerateId { get; init; }

    /// <summary>The documents of the prompt that citations point to. Maps to <c>extractCitationDocuments</c>.</summary>
    public IReadOnlyList<CitationDocument>? CitationDocuments { get; init; }

    private static readonly HashSet<string> TextOrThinkingBlockTypes =
    [
        "redacted_thinking",
        "text",
        "thinking",
    ];

    private readonly List<HeldBackText> _heldBack = [];
    private readonly Dictionary<int, HeldBackText> _heldBackByIndex = [];
    private bool _hasOutput;
    private bool _failed;
    private FinishStreamPart? _lastFinish;
    private readonly IReadOnlyDictionary<string, string> _toolNames;

    /// <summary>Creates a stream reader.</summary>
    public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool)
        : this(includeRawChunks, usesJsonResponseTool, "anthropic", false) { }

    /// <summary>Creates a stream reader that mirrors the finish metadata under a custom provider key.</summary>
    public AnthropicStream(
        bool includeRawChunks,
        bool usesJsonResponseTool,
        string providerOptionsName,
        bool usedCustomProviderKey,
        IReadOnlyDictionary<string, string>? toolNames = null
    )
    {
        _toolNames = toolNames ?? new Dictionary<string, string>();
        _includeRaw = includeRawChunks;
        _usesJsonTool = usesJsonResponseTool;
        _providerOptionsName = providerOptionsName;
        _usedCustomProviderKey = usedCustomProviderKey;
    }

    private string CustomName(string apiName)
    {
        return _toolNames.TryGetValue(apiName, out var mapped) ? mapped : apiName;
    }

    /// <summary>Reads one SSE data payload.</summary>
    public IEnumerable<LanguageModelStreamPart> Push(string json)
    {
        foreach (var part in Transform(json))
        {
            // Tracks the partial output the interrupted finish reason needs, like upstream's second transform.
            switch (part)
            {
                case ResponseMetadataStreamPart:
                    _hasOutput = false;
                    break;
                case FinishStreamPart finish:
                    _hasOutput = false;
                    _lastFinish = finish;
                    break;
                case ErrorStreamPart:
                    _failed = true;
                    break;
                case TextDeltaStreamPart { Delta.Length: > 0 }:
                case ReasoningDeltaStreamPart { Delta.Length: > 0 }:
                case ToolInputDeltaStreamPart { Delta.Length: > 0 }:
                case ToolCallStreamPart:
                    _hasOutput = true;
                    break;
            }

            yield return part;
        }
    }

    private IEnumerable<LanguageModelStreamPart> Transform(string json)
    {
        if (_invalid)
        {
            yield break;
        }

        if (_includeRaw)
        {
            yield return new RawStreamPart(json);
        }

        JsonObject? node = null;
        var invalidJson = false;
        try
        {
            node = JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
            invalidJson = true;
        }

        if (invalidJson)
        {
            yield return new ErrorStreamPart("The stream event was not valid JSON.");
            yield break;
        }

        if (node == null)
        {
            yield break;
        }

        var type = Text(node["type"]);
        if (type == "ping")
        {
            if (IncludePings)
            {
                yield return new PingStreamPart();
            }

            yield break;
        }

        if (type == "error")
        {
            yield return new ErrorStreamPart(
                CreateAnthropicStreamError(node["error"] as JsonObject)
            );
            yield break;
        }

        if (type == "message_start")
        {
            foreach (var part in MessageStart(node))
            {
                yield return part;
            }

            yield break;
        }

        if (_invalid)
        {
            yield break;
        }

        if (type == "content_block_start")
        {
            foreach (var part in BlockStart(node))
            {
                yield return part;
            }
        }
        else if (type == "content_block_delta")
        {
            foreach (var part in BlockDelta(node))
            {
                yield return part;
            }
        }
        else if (type == "content_block_stop")
        {
            foreach (var part in BlockStop(node))
            {
                yield return part;
            }
        }
        else if (type == "message_delta")
        {
            MessageDelta(node);
        }
        else if (type == "message_stop")
        {
            _open = false;
            foreach (var part in FinishMessage())
            {
                yield return part;
            }
        }
    }

    private static ProviderStreamError CreateAnthropicStreamError(JsonObject? error)
    {
        var type = Text(error?["type"]);
        var message = Text(error?["message"]) ?? "Unknown provider error.";
        var statusCode =
            error?["statusCode"] is JsonValue explicitStatus
            && explicitStatus.TryGetValue<int>(out var status)
                ? status
                : AnthropicStreamError.StatusCode(type);
        var isRetryable =
            error?["isRetryable"] is JsonValue explicitRetryable
            && explicitRetryable.TryGetValue<bool>(out var retryable)
                ? retryable
            : type is null ? (bool?)null
            : AnthropicStreamError.StatusCode(type) is null ? null
            : AnthropicStreamError.IsRetryable(type);
        object? code = null;
        if (error?["code"] is JsonValue codeValue)
        {
            code =
                codeValue.TryGetValue<string>(out var codeText) ? codeText
                : codeValue.TryGetValue<long>(out var codeNumber) ? codeNumber
                : null;
        }

        var data = error?["data"] ?? error;
        return new ProviderStreamError(
            message,
            data?.DeepClone(),
            type,
            code,
            statusCode,
            isRetryable
        );
    }

    private IEnumerable<LanguageModelStreamPart> MessageStart(JsonObject node)
    {
        var message = node["message"] as JsonObject;
        var id = Text(message?["id"]);
        if (_open && id != _activeId)
        {
            _invalid = true;
            yield return new ErrorStreamPart(
                "Received a new message_start event while a message was still open."
            );
            yield break;
        }

        if (_open && id == _activeId)
        {
            yield break;
        }

        _open = true;
        _heldBackByIndex.Clear();
        _finish = null;
        _stopSequence = null;
        _container = null;
        _activeId = id;
        _serviceTier = Text(message?["usage"]?["service_tier"]) ?? Text(message?["service_tier"]);
        var usage = message?["usage"];
        if (usage != null)
        {
            _usage = ParseElement(usage.ToJsonString());
            _accumulatedUsage["input_tokens"] = usage["input_tokens"]?.DeepClone();
            _accumulatedUsage["cache_read_input_tokens"] = usage["cache_read_input_tokens"]
                ?.DeepClone() ?? 0;
            _accumulatedUsage["cache_creation_input_tokens"] = usage["cache_creation_input_tokens"]
                ?.DeepClone() ?? 0;
        }

        if (message?["input_transformations"] is { } inputTransformations)
        {
            _inputTransformations = inputTransformations.DeepClone();
        }

        if (message?["container"] is JsonObject startContainer)
        {
            _container = Container(startContainer, false);
        }

        if (Text(message?["stop_reason"]) is { } startStopReason)
        {
            _finish = startStopReason;
        }

        var model = Text(message?["model"]);
        DateTimeOffset? timestamp = null;
        yield return new ResponseMetadataStreamPart(id, model, timestamp);

        var metadata = new JsonObject
        {
            ["anthropic"] = new JsonObject
            {
                ["id"] = id,
                ["model"] = model,
                ["usage"] = usage?.DeepClone(),
            },
        };
        yield return new CustomStreamPart(
            "anthropic.message_start",
            ParseElement(metadata.ToJsonString())
        );

        // Pre-populated tool_use blocks (deferred tool calls) arrive in message_start.
        if (message?["content"] is JsonArray content)
        {
            foreach (var item in content)
            {
                if (item is JsonObject part && Text(part["type"]) == "tool_use")
                {
                    var toolCallId = Text(part["id"]) ?? string.Empty;
                    var toolName = Text(part["name"]) ?? string.Empty;
                    var input = Stringify(part["input"] ?? new JsonObject());
                    var callerInfo = CallerInfo(part["caller"] as JsonObject);
                    yield return new ToolInputStartStreamPart(toolCallId, CustomName(toolName));
                    yield return new ToolInputDeltaStreamPart(toolCallId, input);
                    yield return new ToolInputEndStreamPart(toolCallId);
                    yield return new ToolCallStreamPart(
                        toolCallId,
                        CustomName(toolName),
                        input,
                        callerInfo == null
                            ? null
                            : ParseElement(
                                new JsonObject
                                {
                                    ["anthropic"] = new JsonObject { ["caller"] = callerInfo },
                                }.ToJsonString()
                            )
                    );
                }
            }
        }
    }

    private IEnumerable<LanguageModelStreamPart> BlockStart(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        var block = node["content_block"] as JsonObject;
        var type = Text(block?["type"]);
        if (block == null)
        {
            yield break;
        }

        if (type == "fallback")
        {
            yield return new CustomStreamPart(
                "anthropic.fallback",
                ParseElement(
                    new JsonObject
                    {
                        ["anthropic"] = new JsonObject
                        {
                            ["type"] = "fallback",
                            ["from"] = block["from"]?.DeepClone(),
                            ["to"] = block["to"]?.DeepClone(),
                        },
                    }.ToJsonString()
                )
            );
            yield break;
        }

        _blockType = type;
        var id = index.ToString(CultureInfo.InvariantCulture);

        // Text before a tool call or result is preamble, not the answer.
        if (!TextOrThinkingBlockTypes.Contains(type ?? string.Empty))
        {
            _heldBack.Clear();
        }

        if (type == "text")
        {
            // When the JSON response tool is used, the tool call is returned as text, so the text
            // content is held back until we know whether the tool was called.
            if (_usesJsonTool)
            {
                var entry = new HeldBackText(id);
                _heldBack.Add(entry);
                _heldBackByIndex[index] = entry;
                yield break;
            }

            _blocks[index] = new Block("text", id);
            yield return new TextStartStreamPart(id);
        }
        else if (type == "compaction")
        {
            // A compaction block streams as text whose metadata marks it, so it can be replayed as a compaction block.
            var metadata = new JsonObject { ["type"] = "compaction" };
            var signature = Text(block["signature"]);
            if (signature != null)
            {
                metadata["signature"] = signature;
            }

            _blocks[index] = new Block("text", id);
            yield return new TextStartStreamPart(
                id,
                ParseElement(new JsonObject { ["anthropic"] = metadata }.ToJsonString())
            );

            // On-demand compaction blocks arrive complete in content_block_start, without compaction_delta events.
            var summary = Text(block["content"]);
            if (signature != null && !string.IsNullOrEmpty(summary))
            {
                yield return new TextDeltaStreamPart(id, summary!);
            }
        }
        else if (type == "thinking")
        {
            _blocks[index] = new Block("reasoning", id);
            yield return new ReasoningStartStreamPart(id);
        }
        else if (type == "redacted_thinking")
        {
            _blocks[index] = new Block("reasoning", id);
            yield return new ReasoningStartStreamPart(
                id,
                ParseElement(
                    new JsonObject
                    {
                        ["anthropic"] = new JsonObject { ["redactedData"] = Text(block["data"]) },
                    }.ToJsonString()
                )
            );
        }
        else if (type == "tool_use" && Text(block["toolset_name"]) is { } toolsetName)
        {
            var memberName = Text(block["name"]) ?? string.Empty;
            AnthropicToolset.Validate(toolsetName, memberName, PreparedTools);
            var customName = ComputerToolsetName ?? toolsetName;
            var toolInput = block["input"] as JsonObject;
            var hasInput = toolInput != null && toolInput.Count > 0;
            var metadata = new JsonObject { ["toolsetName"] = toolsetName };
            if (CallerInfo(block["caller"] as JsonObject) is { } caller)
            {
                metadata["caller"] = caller;
            }

            // Toolset member input is accumulated and emitted when the block completes.
            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = Text(block["id"]) ?? string.Empty,
                ToolName = customName,
                Input = new StringBuilder(hasInput ? Stringify(toolInput!) : string.Empty),
                HasInitialInput = hasInput,
                ToolsetMember = memberName,
                ToolsetMetadata = metadata,
            };
            yield return new ToolInputStartStreamPart(
                Text(block["id"]) ?? string.Empty,
                customName
            );
        }
        else if (type == "tool_use")
        {
            var input = block["input"];
            var hasNonEmptyInput = input is JsonObject { Count: > 0 };
            var initial = hasNonEmptyInput ? Stringify(input!) : string.Empty;

            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = Text(block["id"]) ?? string.Empty,
                ToolName = Text(block["name"]) ?? string.Empty,
                Input = new StringBuilder(initial),
                HasInitialInput = initial.Length > 0,
                FirstDelta = initial.Length == 0,
                Caller = CallerInfo(block["caller"] as JsonObject),
            };
            if (!(_usesJsonTool && Text(block["name"]) == "json"))
            {
                yield return new ToolInputStartStreamPart(
                    Text(block["id"]) ?? string.Empty,
                    CustomName(Text(block["name"]) ?? string.Empty)
                );
            }
        }
        else if (type == "server_tool_use")
        {
            foreach (var part in ServerToolUseStart(index, id, block))
            {
                yield return part;
            }
        }
        else if (type == "web_fetch_tool_result")
        {
            foreach (var part in WebFetchToolResult(block))
            {
                yield return part;
            }
        }
        else if (type == "web_search_tool_result")
        {
            foreach (var part in WebSearchToolResult(block))
            {
                yield return part;
            }
        }
        else if (type == "code_execution_tool_result")
        {
            yield return CodeExecutionToolResult(block);
        }
        else if (
            type == "bash_code_execution_tool_result"
            || type == "text_editor_code_execution_tool_result"
        )
        {
            yield return ToolResult(
                Text(block["tool_use_id"]),
                CustomName("code_execution"),
                block["content"]
            );
        }
        else if (type == "tool_search_tool_result")
        {
            yield return ToolSearchToolResult(block);
        }
        else if (type == "advisor_tool_result")
        {
            yield return AdvisorToolResult(block);
        }
        else if (type == "mcp_tool_use")
        {
            var mcpId = Text(block["id"]) ?? string.Empty;
            var mcpName = Text(block["name"]) ?? string.Empty;
            var mcpMetadata = ParseElement(
                new JsonObject
                {
                    ["anthropic"] = new JsonObject
                    {
                        ["type"] = "mcp-tool-use",
                        ["serverName"] = Text(block["server_name"]),
                    },
                }.ToJsonString()
            );
            _mcpToolCalls[mcpId] = (mcpName, mcpMetadata);
            yield return new ToolCallStreamPart(
                mcpId,
                mcpName,
                Stringify(block["input"]),
                mcpMetadata
            )
            {
                ProviderExecuted = true,
                Dynamic = true,
            };
        }
        else if (type == "mcp_tool_result")
        {
            var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
            _mcpToolCalls.TryGetValue(toolUseId, out var call);
            yield return new ToolResultStreamPart(
                toolUseId,
                call.ToolName ?? string.Empty,
                ParseElement(Stringify(block["content"]))
            )
            {
                IsError =
                    block["is_error"] is JsonValue isError
                    && isError.TryGetValue<bool>(out var flag)
                    && flag,
                ProviderMetadata = call.Metadata,
                Dynamic = true,
            };
        }
    }

    private IEnumerable<LanguageModelStreamPart> ServerToolUseStart(
        int index,
        string id,
        JsonObject block
    )
    {
        var name = Text(block["name"]) ?? string.Empty;
        var toolCallId = Text(block["id"]) ?? string.Empty;
        var caller = CallerInfo(block["caller"] as JsonObject);

        if (
            name
            is "web_fetch"
                or "web_search"
                or "code_execution"
                or "text_editor_code_execution"
                or "bash_code_execution"
        )
        {
            // Map tool names for the code execution 20250825 tool.
            var providerToolName = name is "text_editor_code_execution" or "bash_code_execution"
                ? "code_execution"
                : name;
            var providerToolInputType =
                name is "text_editor_code_execution" or "bash_code_execution" ? name
                : name == "code_execution" ? "programmatic-tool-call"
                : null;
            var customToolName = CustomName(providerToolName);

            // Dynamic web tools provide input data here. Other tools like code_execution_20260120
            // provide input data via deltas, so only a non-empty input is used.
            var finalInput = block["input"] is JsonObject { Count: > 0 } provided
                ? Stringify(provided)
                : string.Empty;
            var dynamicCall = MarkCodeExecutionDynamic && providerToolName == "code_execution";

            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = toolCallId,
                ToolName = customToolName,
                CustomToolName = customToolName,
                Input = new StringBuilder(finalInput),
                HasInitialInput = finalInput.Length > 0,
                ProviderExecuted = true,
                Dynamic = dynamicCall,
                FirstDelta = finalInput.Length == 0,
                ProviderToolName = providerToolName,
                ProviderToolInputType = providerToolInputType,
                Caller = caller,
            };
            yield return new ToolInputStartStreamPart(
                toolCallId,
                customToolName,
                providerExecuted: true,
                dynamic: dynamicCall ? true : null
            );
        }
        else if (name is "tool_search_tool_regex" or "tool_search_tool_bm25")
        {
            _serverToolCalls[toolCallId] = name;
            var customToolName = CustomName(name);
            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = toolCallId,
                ToolName = customToolName,
                CustomToolName = customToolName,
                ProviderExecuted = true,
                FirstDelta = true,
                ProviderToolName = name,
                Caller = caller,
            };
            yield return new ToolInputStartStreamPart(
                toolCallId,
                customToolName,
                providerExecuted: true
            );
        }
        else if (name == "advisor")
        {
            var customToolName = CustomName("advisor");
            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = toolCallId,
                ToolName = customToolName,
                CustomToolName = customToolName,
                Input = new StringBuilder("{}"),
                ProviderExecuted = true,
                FirstDelta = true,
                ProviderToolName = name,
                Caller = caller,
            };
            yield return new ToolInputStartStreamPart(
                toolCallId,
                customToolName,
                providerExecuted: true
            );
        }
    }

    private IEnumerable<LanguageModelStreamPart> WebFetchToolResult(JsonObject block)
    {
        var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
        var content = block["content"] as JsonObject;
        var contentType = Text(content?["type"]);
        if (contentType == "web_fetch_result")
        {
            var inner = content!["content"] as JsonObject;
            var source = inner?["source"] as JsonObject;
            (_citationDocuments ??= [.. CitationDocuments ?? []]).Add(
                new CitationDocument(
                    Text(inner?["title"]) ?? Text(content["url"]) ?? string.Empty,
                    null,
                    Text(source?["media_type"]) ?? string.Empty
                )
            );
            var innerResult = new JsonObject();
            Copy(innerResult, "type", inner, "type");
            Copy(innerResult, "title", inner, "title");
            Copy(innerResult, "citations", inner, "citations");
            var sourceResult = new JsonObject();
            Copy(sourceResult, "type", source, "type");
            Copy(sourceResult, "mediaType", source, "media_type");
            Copy(sourceResult, "data", source, "data");
            innerResult["source"] = sourceResult;
            var result = new JsonObject { ["type"] = "web_fetch_result" };
            Copy(result, "url", content, "url");
            Copy(result, "retrievedAt", content, "retrieved_at");
            result["content"] = innerResult;
            yield return ToolResult(
                toolUseId,
                CustomName("web_fetch"),
                result,
                callerMetadata: CallerMetadata(block)
            );
        }
        else if (contentType == "web_fetch_tool_result_error")
        {
            yield return ToolResult(
                toolUseId,
                CustomName("web_fetch"),
                new JsonObject
                {
                    ["type"] = "web_fetch_tool_result_error",
                    ["errorCode"] = content!["error_code"]?.DeepClone(),
                },
                isError: true,
                callerMetadata: CallerMetadata(block)
            );
        }
    }

    private IEnumerable<LanguageModelStreamPart> WebSearchToolResult(JsonObject block)
    {
        var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
        if (block["content"] is JsonArray results)
        {
            var mapped = new JsonArray();
            foreach (var item in results.OfType<JsonObject>())
            {
                var entry = new JsonObject { ["url"] = item["url"]?.DeepClone() };
                if (item["title"] is { } title)
                {
                    entry["title"] = title.DeepClone();
                }

                entry["pageAge"] = item["page_age"]?.DeepClone();
                entry["encryptedContent"] = item["encrypted_content"]?.DeepClone();
                entry["type"] = item["type"]?.DeepClone();
                mapped.Add(entry);
            }

            yield return ToolResult(
                toolUseId,
                CustomName("web_search"),
                mapped,
                callerMetadata: CallerMetadata(block)
            );

            foreach (var item in results.OfType<JsonObject>())
            {
                _hasWebSearchResults = true;
                yield return new SourceStreamPart(
                    NewId(),
                    Text(item["url"]) ?? string.Empty,
                    Text(item["title"]),
                    ParseElement(
                        new JsonObject
                        {
                            ["anthropic"] = new JsonObject
                            {
                                ["pageAge"] = item["page_age"]?.DeepClone(),
                            },
                        }.ToJsonString()
                    )
                );
            }
        }
        else
        {
            yield return ToolResult(
                toolUseId,
                CustomName("web_search"),
                new JsonObject
                {
                    ["type"] = "web_search_tool_result_error",
                    ["errorCode"] = (block["content"] as JsonObject)?["error_code"]?.DeepClone(),
                },
                isError: true,
                callerMetadata: CallerMetadata(block)
            );
        }
    }

    private ToolResultStreamPart CodeExecutionToolResult(JsonObject block)
    {
        var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
        var content = block["content"] as JsonObject;
        var contentType = Text(content?["type"]);
        var result = new JsonObject { ["type"] = contentType };
        var isError = false;
        if (contentType == "code_execution_result")
        {
            Copy(result, "stdout", content, "stdout");
            Copy(result, "stderr", content, "stderr");
            Copy(result, "return_code", content, "return_code");
            result["content"] = content!["content"]?.DeepClone() ?? new JsonArray();
        }
        else if (contentType == "encrypted_code_execution_result")
        {
            Copy(result, "encrypted_stdout", content, "encrypted_stdout");
            Copy(result, "stderr", content, "stderr");
            Copy(result, "return_code", content, "return_code");
            result["content"] = content!["content"]?.DeepClone() ?? new JsonArray();
        }
        else
        {
            isError = true;
            result["errorCode"] = content?["error_code"]?.DeepClone();
        }

        return ToolResult(toolUseId, CustomName("code_execution"), result, isError: isError);
    }

    private ToolResultStreamPart ToolSearchToolResult(JsonObject block)
    {
        var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
        if (!_serverToolCalls.TryGetValue(toolUseId, out var providerToolName))
        {
            var bm25CustomName = CustomName("tool_search_tool_bm25");
            var regexCustomName = CustomName("tool_search_tool_regex");
            providerToolName =
                bm25CustomName != "tool_search_tool_bm25" ? "tool_search_tool_bm25"
                : regexCustomName != "tool_search_tool_regex" ? "tool_search_tool_regex"
                : "tool_search_tool_regex";
        }

        var content = block["content"] as JsonObject;
        if (Text(content?["type"]) == "tool_search_tool_search_result")
        {
            var references = new JsonArray();
            foreach (
                var reference in (
                    content!["tool_references"] as JsonArray ?? []
                ).OfType<JsonObject>()
            )
            {
                references.Add(
                    new JsonObject
                    {
                        ["type"] = reference["type"]?.DeepClone(),
                        ["toolName"] = reference["tool_name"]?.DeepClone(),
                    }
                );
            }

            return ToolResult(toolUseId, CustomName(providerToolName), references);
        }

        return ToolResult(
            toolUseId,
            CustomName(providerToolName),
            new JsonObject
            {
                ["type"] = "tool_search_tool_result_error",
                ["errorCode"] = content?["error_code"]?.DeepClone(),
            },
            isError: true
        );
    }

    // Advisor results arrive fully formed in a single content_block_start (no deltas).
    private ToolResultStreamPart AdvisorToolResult(JsonObject block)
    {
        var toolUseId = Text(block["tool_use_id"]) ?? string.Empty;
        var advisorToolName = CustomName("advisor");
        var content = block["content"] as JsonObject;
        var contentType = Text(content?["type"]);
        if (contentType == "advisor_result")
        {
            var result = new JsonObject { ["type"] = "advisor_result" };
            Copy(result, "text", content, "text");
            if (content!["stop_reason"] is { } stopReason)
            {
                result["stopReason"] = stopReason.DeepClone();
            }

            return ToolResult(toolUseId, advisorToolName, result);
        }

        if (contentType == "advisor_redacted_result")
        {
            var result = new JsonObject { ["type"] = "advisor_redacted_result" };
            Copy(result, "encryptedContent", content, "encrypted_content");
            if (content!["stop_reason"] is { } stopReason)
            {
                result["stopReason"] = stopReason.DeepClone();
            }

            return ToolResult(toolUseId, advisorToolName, result);
        }

        return ToolResult(
            toolUseId,
            advisorToolName,
            new JsonObject
            {
                ["type"] = "advisor_tool_result_error",
                ["errorCode"] = content?["error_code"]?.DeepClone(),
            },
            isError: true
        );
    }

    private static ToolResultStreamPart ToolResult(
        string? toolCallId,
        string toolName,
        JsonNode? result,
        bool isError = false,
        JsonElement? callerMetadata = null
    )
    {
        return new ToolResultStreamPart(
            toolCallId ?? string.Empty,
            toolName,
            ParseElement(Stringify(result))
        )
        {
            IsError = isError,
            ProviderMetadata = callerMetadata,
        };
    }

    private IEnumerable<LanguageModelStreamPart> BlockDelta(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        var delta = node["delta"] as JsonObject;
        if (_heldBackByIndex.TryGetValue(index, out var heldBack))
        {
            if (Text(delta?["type"]) == "text_delta")
            {
                heldBack.Text.Append(Text(delta?["text"]) ?? string.Empty);
            }
            else if (Text(delta?["type"]) == "citations_delta" && delta?["citation"] != null)
            {
                heldBack.Citations.Add(ParseElement(delta["citation"]!.ToJsonString()));
                foreach (var part in Citation(delta["citation"]!))
                {
                    yield return part;
                }
            }

            yield break;
        }

        Block? block;
        if (delta == null || !_blocks.TryGetValue(index, out block) || block == null)
        {
            yield break;
        }

        var deltaType = Text(delta["type"]);
        if (deltaType == "text_delta" && block.Kind == "text")
        {
            var text = Text(delta["text"]) ?? string.Empty;
            yield return new TextDeltaStreamPart(block.Id, text);
        }
        else if (deltaType == "compaction_delta" && block.Kind == "text")
        {
            var summary = Text(delta["content"]);
            if (summary != null)
            {
                yield return new TextDeltaStreamPart(block.Id, summary);
            }
        }
        else if (deltaType == "thinking_delta" && block.Kind == "reasoning")
        {
            yield return new ReasoningDeltaStreamPart(
                block.Id,
                Text(delta["thinking"]) ?? string.Empty
            );
        }
        else if (deltaType == "signature_delta" && block.Kind == "reasoning")
        {
            // Signatures are only supported on thinking blocks.
            if (_blockType == "thinking")
            {
                yield return new ReasoningDeltaStreamPart(
                    block.Id,
                    string.Empty,
                    ParseElement(
                        new JsonObject
                        {
                            ["anthropic"] = new JsonObject
                            {
                                ["signature"] = Text(delta["signature"]),
                            },
                        }.ToJsonString()
                    )
                );
            }
        }
        else if (deltaType == "citations_delta" && delta["citation"] != null)
        {
            if (block.Kind == "text")
            {
                block.Citations.Add(ParseElement(delta["citation"]!.ToJsonString()));
            }

            foreach (var part in Citation(delta["citation"]!))
            {
                yield return part;
            }
        }
        else if (deltaType == "input_json_delta" && block.Kind == "tool")
        {
            block.SawDelta = true;
            var partial = Text(delta["partial_json"]) ?? string.Empty;

            // Skip empty deltas to enable replacing the first character in the code execution 20250825 tool.
            if (partial.Length == 0)
            {
                yield break;
            }

            // Toolset member input is emitted as a single delta once the block is complete.
            if (block.ToolsetMember != null)
            {
                block.Input.Append(partial);
            }
            else if (_usesJsonTool && block.ToolName == "json")
            {
                block.Input.Append(partial);
            }
            else
            {
                // For the code execution 20250825 tool, add the type to the delta.
                if (block.FirstDelta && block.ProviderToolInputType != null)
                {
                    partial =
                        "{\"type\": \""
                        + block.ProviderToolInputType
                        + "\","
                        + partial.Substring(1);
                }

                yield return new ToolInputDeltaStreamPart(block.ToolCallId, partial);
                block.Input.Append(partial);
                block.FirstDelta = false;
            }
        }
    }

    // Maps createCitationSource: web search locations are held until the message ends, document locations become sources.
    private IEnumerable<LanguageModelStreamPart> Citation(JsonNode citation)
    {
        var element = ParseElement(citation.ToJsonString());
        if (Text(citation["type"]) == "web_search_result_location")
        {
            _fallbackWebCitations.Add(element);
        }
        else if (CreateCitationSource(element) is { } source)
        {
            yield return source;
        }
    }

    private LanguageModelStreamPart? CreateCitationSource(JsonElement citation)
    {
        return AnthropicCitations.CreateSource(citation, Documents(), NewId);
    }

    private IReadOnlyList<CitationDocument> Documents()
    {
        return _citationDocuments ?? (IReadOnlyList<CitationDocument>?)CitationDocuments ?? [];
    }

    private string NewId()
    {
        return (GenerateId ?? (() => ProviderUtils.JsonValues.GenerateId(string.Empty)))();
    }

    private IEnumerable<LanguageModelStreamPart> BlockStop(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        Block? block;
        if (!_blocks.TryGetValue(index, out block) || block == null)
        {
            _blockType = null;
            yield break;
        }

        _blocks.Remove(index);
        _blockType = null;
        if (block.Kind == "text")
        {
            yield return CreateTextEnd(block.Id, block.Citations);
        }
        else if (block.Kind == "reasoning")
        {
            yield return new ReasoningEndStreamPart(block.Id);
        }
        else if (block.Kind == "tool" && !_invalid)
        {
            var arguments = block.Input.Length == 0 ? "{}" : block.Input.ToString();
            if (block.ToolsetMember != null)
            {
                arguments = AnthropicToolset.MemberInput(
                    block.ToolsetMember,
                    AnthropicToolset.ParseStreamedInput(block.Input.ToString())
                );
                yield return new ToolInputDeltaStreamPart(block.ToolCallId, arguments);
                yield return new ToolInputEndStreamPart(block.ToolCallId);
                yield return new ToolCallStreamPart(
                    block.ToolCallId,
                    block.ToolName,
                    arguments,
                    ParseElement(
                        new JsonObject
                        {
                            ["anthropic"] = block.ToolsetMetadata!.DeepClone(),
                        }.ToJsonString()
                    )
                );
            }
            else if (_usesJsonTool && block.ToolName == "json")
            {
                _jsonTool = true;
                yield return new TextDeltaStreamPart(block.Id, arguments);
            }
            else
            {
                yield return new ToolInputEndStreamPart(block.ToolCallId);

                // For code_execution, inject 'programmatic-tool-call' when the input has { code } format.
                if (block.ProviderToolName == "code_execution")
                {
                    try
                    {
                        if (
                            JsonNode.Parse(arguments) is JsonObject parsed
                            && parsed.ContainsKey("code")
                            && !parsed.ContainsKey("type")
                        )
                        {
                            var typed = new JsonObject { ["type"] = "programmatic-tool-call" };
                            foreach (var property in parsed)
                            {
                                typed[property.Key] = property.Value?.DeepClone();
                            }

                            arguments = Stringify(typed);
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore parse errors and use the original input.
                    }
                }

                JsonElement? metadata = null;
                if (block.Caller != null)
                {
                    metadata = ParseElement(
                        new JsonObject
                        {
                            ["anthropic"] = new JsonObject
                            {
                                ["caller"] = block.Caller.DeepClone(),
                            },
                        }.ToJsonString()
                    );
                }

                yield return new ToolCallStreamPart(
                    block.ToolCallId,
                    block.CustomToolName ?? CustomName(block.ToolName),
                    arguments,
                    metadata
                )
                {
                    ProviderExecuted = block.ProviderExecuted,
                    Dynamic = block.Dynamic,
                };
            }
        }
    }

    private LanguageModelStreamPart CreateTextEnd(string id, List<JsonElement> citations)
    {
        return citations.Count == 0
            ? new TextEndStreamPart(id)
            : new TextEndStreamPart(
                id,
                AnthropicCitations.MapAll(citations, Documents()),
                AnthropicCitations.TextMetadata(citations)
            );
    }

    private IEnumerable<LanguageModelStreamPart> FinishMessage()
    {
        if (!_hasWebSearchResults)
        {
            foreach (var citation in _fallbackWebCitations)
            {
                if (CreateCitationSource(citation) is { } source)
                {
                    yield return source;
                }
            }
        }

        _fallbackWebCitations.Clear();

        foreach (var part in FlushHeldBackText())
        {
            yield return part;
        }

        yield return Finish();
    }

    // A message that hands off to a tool or continuation is not the final answer,
    // so its held-back text is discarded.
    private IEnumerable<LanguageModelStreamPart> FlushHeldBackText()
    {
        var emit = !_jsonTool && !IsContinuationStopReason(_finish);
        var entries = _heldBack.ToArray();
        _heldBack.Clear();
        if (!emit)
        {
            yield break;
        }

        foreach (var entry in entries)
        {
            if (entry.Text.Length == 0)
            {
                continue;
            }

            yield return new TextStartStreamPart(entry.Id);
            yield return new TextDeltaStreamPart(entry.Id, entry.Text.ToString());
            yield return CreateTextEnd(entry.Id, entry.Citations);
        }
    }

    private static bool IsContinuationStopReason(string? stopReason)
    {
        return stopReason == "pause_turn" || stopReason == "tool_use";
    }

    private void MessageDelta(JsonObject node)
    {
        var delta = node["delta"] as JsonObject;
        var stop = Text(delta?["stop_reason"]);
        if (stop != null)
        {
            _finish = stop;
        }

        _stopSequence = Text(delta?["stop_sequence"]);
        _stopDetails = MapStopDetails(delta?["stop_details"] as JsonObject);
        _container = delta?["container"] is JsonObject container
            ? Container(container, true)
            : null;

        if (node["context_management"] is JsonObject contextManagement)
        {
            _contextManagement = MapContextManagement(contextManagement);
        }

        if (node["input_transformations"] is { } inputTransformations)
        {
            _inputTransformations = inputTransformations.DeepClone();
        }

        // Earlier deltas may carry null while the classifier is still running;
        // the last non-null value is the final verdict.
        if (delta?["safeguard_results"] is { } safeguardResults)
        {
            _safeguardResults = safeguardResults.DeepClone();
        }

        if (node["usage"] is JsonObject usage)
        {
            if (
                usage["input_tokens"] is { } inputTokens
                && !JsonNode.DeepEquals(_accumulatedUsage["input_tokens"], inputTokens)
            )
            {
                _accumulatedUsage["input_tokens"] = inputTokens.DeepClone();
            }

            _accumulatedUsage["output_tokens"] = usage["output_tokens"]?.DeepClone();
            if (usage["output_tokens_details"] is { } outputTokensDetails)
            {
                _accumulatedUsage["output_tokens_details"] = outputTokensDetails.DeepClone();
            }

            if (usage["cache_read_input_tokens"] is { } cacheRead)
            {
                _accumulatedUsage["cache_read_input_tokens"] = cacheRead.DeepClone();
            }

            if (usage["cache_creation_input_tokens"] is { } cacheCreation)
            {
                _accumulatedUsage["cache_creation_input_tokens"] = cacheCreation.DeepClone();
            }

            if (usage["iterations"] is { } iterations)
            {
                _accumulatedUsage["iterations"] = iterations.DeepClone();
            }

            _serviceTier = Text(usage["service_tier"]) ?? _serviceTier;
            // The delta only reports what changed; keep the counts from message_start.
            var merged = _usage is { ValueKind: JsonValueKind.Object } current
                ? (JsonNode.Parse(current.GetRawText()) as JsonObject ?? new JsonObject())
                : new JsonObject();
            foreach (var pair in usage)
            {
                merged[pair.Key] = pair.Value?.DeepClone();
            }

            _usage = ParseElement(merged.ToJsonString());
        }
    }

    private static JsonNode? MapStopDetails(JsonObject? stopDetails)
    {
        if (stopDetails == null)
        {
            return null;
        }

        var mapped = new JsonObject { ["type"] = stopDetails["type"]?.DeepClone() };
        if (stopDetails["category"] is { } category)
        {
            mapped["category"] = category.DeepClone();
        }

        if (stopDetails["explanation"] is { } explanation)
        {
            mapped["explanation"] = explanation.DeepClone();
        }

        if (stopDetails["recommended_model"] is { } recommendedModel)
        {
            mapped["recommendedModel"] = recommendedModel.DeepClone();
        }

        return mapped;
    }

    private static JsonNode MapContextManagement(JsonObject contextManagement)
    {
        var edits = new JsonArray();
        foreach (
            var edit in (contextManagement["applied_edits"] as JsonArray ?? []).OfType<JsonObject>()
        )
        {
            var editType = Text(edit["type"]);
            switch (editType)
            {
                case "clear_tool_uses_20250919":
                    edits.Add(
                        new JsonObject
                        {
                            ["type"] = editType,
                            ["clearedToolUses"] = edit["cleared_tool_uses"]?.DeepClone(),
                            ["clearedInputTokens"] = edit["cleared_input_tokens"]?.DeepClone(),
                        }
                    );
                    break;
                case "clear_thinking_20251015":
                    edits.Add(
                        new JsonObject
                        {
                            ["type"] = editType,
                            ["clearedThinkingTurns"] = edit["cleared_thinking_turns"]?.DeepClone(),
                            ["clearedInputTokens"] = edit["cleared_input_tokens"]?.DeepClone(),
                        }
                    );
                    break;
                case "compact_20260112":
                    edits.Add(new JsonObject { ["type"] = editType });
                    break;
            }
        }

        return new JsonObject { ["appliedEdits"] = edits };
    }

    private static JsonNode? MapIterations(JsonElement usage)
    {
        if (
            !usage.TryGetProperty("iterations", out var iterations)
            || iterations.ValueKind != JsonValueKind.Array
        )
        {
            return null;
        }

        var mapped = new JsonArray();
        foreach (var iteration in iterations.EnumerateArray())
        {
            var item = new JsonObject { ["type"] = AnthropicJson.String(iteration, "type") };
            if (AnthropicJson.String(iteration, "model") is { } iterationModel)
            {
                item["model"] = iterationModel;
            }

            item["inputTokens"] = iteration.TryGetProperty("input_tokens", out var input)
                ? AnthropicJson.Node(input)
                : null;
            item["outputTokens"] = iteration.TryGetProperty("output_tokens", out var output)
                ? AnthropicJson.Node(output)
                : null;
            if (
                iteration.TryGetProperty("cache_creation_input_tokens", out var cacheCreation)
                && cacheCreation.ValueKind == JsonValueKind.Number
                && cacheCreation.GetDouble() != 0
            )
            {
                item["cacheCreationInputTokens"] = AnthropicJson.Node(cacheCreation);
            }

            if (
                iteration.TryGetProperty("cache_read_input_tokens", out var cacheRead)
                && cacheRead.ValueKind == JsonValueKind.Number
                && cacheRead.GetDouble() != 0
            )
            {
                item["cacheReadInputTokens"] = AnthropicJson.Node(cacheRead);
            }

            mapped.Add(item);
        }

        return mapped;
    }

    /// <summary>
    /// Ends the stream. Returns an interrupted finish part when the events ended cleanly after output
    /// without a <c>message_stop</c> for the last message. A stream without output, or with an error,
    /// gets no finish part so the existing no-output and error paths apply.
    /// </summary>
    public IEnumerable<LanguageModelStreamPart> Complete()
    {
        if (_hasOutput && !_failed)
        {
            _hasOutput = false;
            // Keep the usage core already had before the cutoff.
            yield return new FinishStreamPart(
                FinishReason.Interrupted,
                _lastFinish?.Usage ?? LanguageModelUsage.Null,
                providerMetadata: _lastFinish?.ProviderMetadata
            );
        }
    }

    private static JsonObject Container(JsonObject container, bool withSkills)
    {
        var mapped = new JsonObject
        {
            ["expiresAt"] = Text(container["expires_at"]),
            ["id"] = Text(container["id"]),
            ["skills"] = null,
        };
        if (withSkills && container["skills"] is JsonArray skills)
        {
            mapped["skills"] = new JsonArray(
                skills
                    .OfType<JsonObject>()
                    .Select(skill =>
                        (JsonNode)
                            new JsonObject
                            {
                                ["type"] = skill["type"]?.DeepClone(),
                                ["skillId"] = skill["skill_id"]?.DeepClone(),
                                ["version"] = skill["version"]?.DeepClone(),
                            }
                    )
                    .ToArray()
            );
        }

        return mapped;
    }

    private JsonElement FinishMetadata()
    {
        var anthropic = new JsonObject
        {
            ["usage"] = _usage is { ValueKind: JsonValueKind.Object } usage
                ? JsonNode.Parse(usage.GetRawText())
                : null,
            ["stopSequence"] = _stopSequence,
        };
        if (_serviceTier != null)
        {
            anthropic["serviceTier"] = _serviceTier;
        }

        if (_stopDetails != null)
        {
            anthropic["stopDetails"] = _stopDetails.DeepClone();
        }

        if (_inputTransformations != null)
        {
            anthropic["inputTransformations"] = _inputTransformations.DeepClone();
        }

        if (_safeguardResults != null)
        {
            anthropic["safeguardResults"] = _safeguardResults.DeepClone();
        }

        anthropic["iterations"] = MapIterations(ParseElement(_accumulatedUsage.ToJsonString()));
        anthropic["container"] = _container?.DeepClone();
        anthropic["contextManagement"] = _contextManagement?.DeepClone();

        var metadata = new JsonObject { ["anthropic"] = anthropic };
        if (_usedCustomProviderKey && _providerOptionsName != "anthropic")
        {
            metadata[_providerOptionsName] = anthropic.DeepClone();
        }

        return ParseElement(metadata.ToJsonString());
    }

    private FinishStreamPart Finish()
    {
        var usage = AnthropicUsage.Convert(
            ParseElement(_accumulatedUsage.ToJsonString()),
            _usage is { ValueKind: JsonValueKind.Object } rawUsage ? rawUsage : null
        );
        return new FinishStreamPart(
            AnthropicStopReason.Map(_finish, _jsonTool),
            usage,
            _finish,
            FinishMetadata()
        );
    }

    private static JsonObject? CallerInfo(JsonObject? caller)
    {
        if (caller == null)
        {
            return null;
        }

        var info = new JsonObject { ["type"] = caller["type"]?.DeepClone() };
        if (caller["tool_id"] != null)
        {
            info["toolId"] = caller["tool_id"]!.DeepClone();
        }

        return info;
    }

    private static JsonElement? CallerMetadata(JsonObject part)
    {
        return CallerInfo(part["caller"] as JsonObject) is { } info
            ? ParseElement(
                new JsonObject
                {
                    ["anthropic"] = new JsonObject { ["caller"] = info },
                }.ToJsonString()
            )
            : null;
    }

    private static void Copy(JsonObject target, string key, JsonObject? source, string sourceKey)
    {
        if (source != null && source.TryGetPropertyValue(sourceKey, out var value))
        {
            target[key] = value?.DeepClone();
        }
    }

    private static readonly JsonSerializerOptions s_compact = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static string Stringify(JsonNode? node)
    {
        return node == null ? "null" : node.ToJsonString(s_compact);
    }

    private static JsonElement ParseElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string? Text(JsonNode? node)
    {
        string? text;
        if (node is JsonValue json && json.TryGetValue<string>(out text))
        {
            return text;
        }

        return null;
    }

    private static int? Int(JsonNode? node)
    {
        int number;
        if (node is JsonValue && ((JsonValue)node).TryGetValue<int>(out number))
        {
            return number;
        }

        return null;
    }

    private sealed class HeldBackText(string id)
    {
        public string Id { get; } = id;

        public StringBuilder Text { get; } = new();

        public List<JsonElement> Citations { get; } = [];
    }

    private sealed class Block
    {
        public Block(string kind, string id)
        {
            Kind = kind;
            Id = id;
            Input = new StringBuilder();
            ToolCallId = string.Empty;
            ToolName = string.Empty;
        }

        public string Kind { get; }

        public string Id { get; }

        public string ToolCallId { get; set; }

        public string ToolName { get; set; }

        /// <summary>The name the tool call is emitted with, when it is not the mapped <see cref="ToolName"/>.</summary>
        public string? CustomToolName { get; set; }

        public StringBuilder Input { get; set; }

        public bool HasInitialInput { get; set; }

        public bool SawDelta { get; set; }

        public bool FirstDelta { get; set; }

        public bool ProviderExecuted { get; set; }

        public bool Dynamic { get; set; }

        public string? ProviderToolName { get; set; }

        public string? ProviderToolInputType { get; set; }

        public JsonObject? Caller { get; set; }

        public string? ToolsetMember { get; set; }

        public JsonObject? ToolsetMetadata { get; set; }

        public List<JsonElement> Citations { get; } = new();
    }
}
