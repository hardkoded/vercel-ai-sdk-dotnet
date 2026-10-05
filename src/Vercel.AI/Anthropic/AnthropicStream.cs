// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Turns Anthropic SSE JSON events into SDK stream parts.</summary>
public sealed class AnthropicStream
{
    private readonly bool _includeRaw;
    private readonly bool _usesJsonTool;
    private readonly Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
    private bool _invalid;
    private bool _open;
    private string? _activeId;
    private string? _finish;
    private JsonElement? _usage;
    private bool _jsonTool;

    /// <summary>Creates a stream reader.</summary>
    public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool)
    {
        _includeRaw = includeRawChunks;
        _usesJsonTool = usesJsonResponseTool;
    }

    /// <summary>Reads one SSE data payload.</summary>
    public IEnumerable<LanguageModelStreamPart> Push(string json)
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
            yield break;
        }

        if (type == "error")
        {
            var error = node["error"] as JsonObject;
            var errorType = Text(error?["type"]);
            var message = Text(error?["message"]) ?? "Unknown provider error.";
            var status = AnthropicStreamError.StatusCode(errorType);
            yield return new ErrorStreamPart(status == null ? message : message + " (" + status.Value.ToString(CultureInfo.InvariantCulture) + ")");
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
            yield return Finish();
        }
    }

    private IEnumerable<LanguageModelStreamPart> MessageStart(JsonObject node)
    {
        var message = node["message"] as JsonObject;
        var id = Text(message?["id"]);
        if (_open && id != _activeId)
        {
            _invalid = true;
            yield return new ErrorStreamPart("Received a new message_start event while a message was still open.");
            yield break;
        }

        if (_open && id == _activeId)
        {
            yield break;
        }

        _open = true;
        _activeId = id;
        var usage = message?["usage"];
        if (usage != null)
        {
            _usage = ParseElement(usage.ToJsonString());
        }

        DateTimeOffset? timestamp = null;
        yield return new ResponseMetadataStreamPart(id, Text(message?["model"]), timestamp);
    }

    private IEnumerable<LanguageModelStreamPart> BlockStart(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        var block = node["content_block"] as JsonObject;
        var type = Text(block?["type"]);
        if (type == "fallback" || block == null)
        {
            yield break;
        }

        var id = index.ToString(CultureInfo.InvariantCulture);
        if (type == "text")
        {
            _blocks[index] = new Block("text", id);
            yield return new TextStartStreamPart(id);
        }
        else if (type == "thinking" || type == "redacted_thinking")
        {
            _blocks[index] = new Block("reasoning", id);
            yield return new ReasoningStartStreamPart(id);
        }
        else if (type == "tool_use" || type == "server_tool_use")
        {
            var input = block["input"];
            var initial = input == null ? string.Empty : input.ToJsonString();
            if (initial == "{}")
            {
                initial = string.Empty;
            }

            _blocks[index] = new Block("tool", id)
            {
                ToolCallId = Text(block["id"]) ?? string.Empty,
                ToolName = Text(block["name"]) ?? string.Empty,
                Input = new StringBuilder(initial),
                HasInitialInput = initial.Length > 0,
            };
        }
    }

    private IEnumerable<LanguageModelStreamPart> BlockDelta(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        var delta = node["delta"] as JsonObject;
        Block? block;
        if (delta == null || !_blocks.TryGetValue(index, out block) || block == null)
        {
            yield break;
        }

        var deltaType = Text(delta["type"]);
        if (deltaType == "text_delta" && block.Kind == "text")
        {
            yield return new TextDeltaStreamPart(block.Id, Text(delta["text"]) ?? string.Empty);
        }
        else if ((deltaType == "thinking_delta" || deltaType == "signature_delta") && block.Kind == "reasoning")
        {
            if (deltaType == "thinking_delta")
            {
                yield return new ReasoningDeltaStreamPart(block.Id, Text(delta["thinking"]) ?? string.Empty);
            }
        }
        else if (deltaType == "input_json_delta" && block.Kind == "tool")
        {
            block.SawDelta = true;
            block.Input.Append(Text(delta["partial_json"]) ?? string.Empty);
        }
    }

    private IEnumerable<LanguageModelStreamPart> BlockStop(JsonObject node)
    {
        var index = Int(node["index"]) ?? 0;
        Block? block;
        if (!_blocks.TryGetValue(index, out block) || block == null)
        {
            yield break;
        }

        _blocks.Remove(index);
        if (block.Kind == "text")
        {
            yield return new TextEndStreamPart(block.Id);
        }
        else if (block.Kind == "reasoning")
        {
            yield return new ReasoningEndStreamPart(block.Id);
        }
        else if (block.Kind == "tool" && !_invalid)
        {
            var arguments = block.Input.Length == 0 ? "{}" : block.Input.ToString();
            if (_usesJsonTool && block.ToolName == "json")
            {
                _jsonTool = true;
                yield return new TextDeltaStreamPart(block.Id, arguments);
            }
            else
            {
                yield return new ToolCallStreamPart(block.ToolCallId, block.ToolName, arguments);
            }
        }
    }

    private void MessageDelta(JsonObject node)
    {
        var delta = node["delta"] as JsonObject;
        var stop = Text(delta?["stop_reason"]);
        if (stop != null)
        {
            _finish = stop;
        }

        var usage = node["usage"];
        if (usage != null)
        {
            _usage = ParseElement(usage.ToJsonString());
        }
    }

    private FinishStreamPart Finish()
    {
        var usage = _usage is { } element && element.ValueKind == JsonValueKind.Object
            ? AnthropicUsage.Convert(element)
            : LanguageModelUsage.Empty;
        return new FinishStreamPart(AnthropicStopReason.Map(_finish, _jsonTool), usage, _finish);
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

        public StringBuilder Input { get; set; }

        public bool HasInitialInput { get; set; }

        public bool SawDelta { get; set; }
    }
}
