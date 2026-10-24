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
    private readonly string _providerOptionsName;
    private readonly bool _usedCustomProviderKey;
    private readonly Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
    private bool _invalid;
    private bool _open;
    private string? _activeId;
    private string? _finish;
    private JsonElement? _usage;
    private string? _stopSequence;
    private JsonNode? _container;
    private bool _jsonTool;
    private bool _finished;
    private bool _failed;

    /// <summary>Creates a stream reader.</summary>
    public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool)
        : this(includeRawChunks, usesJsonResponseTool, "anthropic", false)
    {
    }

    /// <summary>Creates a stream reader that mirrors the finish metadata under a custom provider key.</summary>
    public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey)
    {
        _includeRaw = includeRawChunks;
        _usesJsonTool = usesJsonResponseTool;
        _providerOptionsName = providerOptionsName;
        _usedCustomProviderKey = usedCustomProviderKey;
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
            _failed = true;
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
            _failed = true;
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
            _finished = true;
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
            _failed = true;
            yield return new ErrorStreamPart("Received a new message_start event while a message was still open.");
            yield break;
        }

        if (_open && id == _activeId)
        {
            yield break;
        }

        _open = true;
        _finished = false;
        _finish = null;
        _stopSequence = null;
        _container = null;
        _activeId = id;
        var usage = message?["usage"];
        if (usage != null)
        {
            _usage = ParseElement(usage.ToJsonString());
        }

        if (message?["container"] is JsonObject startContainer)
        {
            _container = Container(startContainer, false);
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
        yield return new CustomStreamPart("anthropic.message_start", ParseElement(metadata.ToJsonString()));
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
            yield return new TextStartStreamPart(id, ParseElement(new JsonObject { ["anthropic"] = metadata }.ToJsonString()));
            var summary = Text(block["content"]);
            if (!string.IsNullOrEmpty(summary))
            {
                yield return new TextDeltaStreamPart(id, summary!);
            }
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
        else if (deltaType == "compaction_delta" && block.Kind == "text")
        {
            var summary = Text(delta["content"]);
            if (!string.IsNullOrEmpty(summary))
            {
                yield return new TextDeltaStreamPart(block.Id, summary!);
            }
        }
        else if ((deltaType == "thinking_delta" || deltaType == "signature_delta") && block.Kind == "reasoning")
        {
            if (deltaType == "thinking_delta")
            {
                yield return new ReasoningDeltaStreamPart(block.Id, Text(delta["thinking"]) ?? string.Empty);
            }
        }
        else if (deltaType == "citations_delta" && block.Kind == "text" && delta["citation"] != null)
        {
            block.Citations.Add(ParseElement(delta["citation"]!.ToJsonString()));
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
            yield return block.Citations.Count == 0
                ? new TextEndStreamPart(block.Id)
                : new TextEndStreamPart(block.Id, AnthropicCitations.MapAll(block.Citations), AnthropicCitations.TextMetadata(block.Citations));
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

        _stopSequence = Text(delta?["stop_sequence"]);
        _container = delta?["container"] is JsonObject container ? Container(container, true) : null;

        if (node["usage"] is JsonObject usage)
        {
            // The delta only reports what changed; keep the counts from message_start.
            var merged = _usage is { ValueKind: JsonValueKind.Object } current
                ? (JsonNode.Parse(current.GetRawText()) as JsonObject ?? new JsonObject())
                : new JsonObject();
            foreach (var pair in usage)
            {
                if (pair.Value != null)
                {
                    merged[pair.Key] = pair.Value.DeepClone();
                }
            }

            _usage = ParseElement(merged.ToJsonString());
        }
    }

    /// <summary>
    /// Ends the stream. Returns an interrupted finish part when the events ended cleanly without a
    /// <c>message_stop</c> for the last message. Errors keep their own error path and get no finish part.
    /// </summary>
    public IEnumerable<LanguageModelStreamPart> Complete()
    {
        if (!_finished && !_failed)
        {
            _finished = true;
            yield return Finish(true);
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
            mapped["skills"] = new JsonArray(skills.OfType<JsonObject>().Select(skill => (JsonNode)new JsonObject
            {
                ["type"] = skill["type"]?.DeepClone(),
                ["skillId"] = skill["skill_id"]?.DeepClone(),
                ["version"] = skill["version"]?.DeepClone(),
            }).ToArray());
        }

        return mapped;
    }

    private JsonElement FinishMetadata()
    {
        var anthropic = new JsonObject
        {
            ["usage"] = _usage is { ValueKind: JsonValueKind.Object } usage ? JsonNode.Parse(usage.GetRawText()) : null,
            ["stopSequence"] = _stopSequence,
            ["iterations"] = null,
            ["container"] = _container?.DeepClone(),
            ["contextManagement"] = null,
        };
        var metadata = new JsonObject { ["anthropic"] = anthropic };
        if (_usedCustomProviderKey && _providerOptionsName != "anthropic")
        {
            metadata[_providerOptionsName] = anthropic.DeepClone();
        }

        return ParseElement(metadata.ToJsonString());
    }

    private FinishStreamPart Finish(bool interrupted = false)
    {
        var usage = _usage is { } element && element.ValueKind == JsonValueKind.Object
            ? AnthropicUsage.Convert(element)
            : LanguageModelUsage.Empty;
        return interrupted
            ? new FinishStreamPart(FinishReason.Interrupted, usage)
            : new FinishStreamPart(AnthropicStopReason.Map(_finish, _jsonTool), usage, _finish, FinishMetadata());
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

        public List<JsonElement> Citations { get; } = new();
    }
}
