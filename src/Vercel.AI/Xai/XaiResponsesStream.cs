// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>Turns xAI Responses stream events into language-model stream parts.</summary>
internal sealed class XaiResponsesStream
{
    private readonly Func<string> _generateId;
    private readonly XaiToolNames _toolNames;
    private readonly HashSet<string> _seenToolCalls = new(StringComparer.Ordinal);
    private readonly List<string> _textBlocks = new();
    private readonly HashSet<string> _textBlockSet = new(StringComparer.Ordinal);
    private readonly HashSet<string> _activeReasoning = new(StringComparer.Ordinal);
    private readonly Dictionary<int, string> _ongoingToolCalls = new();
    private bool _responseMetadataSent;
    private bool _hasFunctionCall;
    private FinishReason _finish = FinishReason.Other;
    private string? _rawFinish;
    private LanguageModelUsage? _usage;
    private JsonNode? _costInUsdTicks;
    private string? _serviceTier;
    private string? _promptCacheKey;
    private string? _safetyIdentifier;

    public XaiResponsesStream(Func<string> generateId, XaiToolNames toolNames)
    {
        _generateId = generateId;
        _toolNames = toolNames;
    }

    /// <summary>Metadata of a reasoning item. Null fields are left out.</summary>
    public static JsonElement ReasoningMetadata(string? itemId, string? encryptedContent)
    {
        var xai = new JsonObject();
        if (!string.IsNullOrEmpty(encryptedContent))
        {
            xai["reasoningEncryptedContent"] = encryptedContent;
        }

        if (!string.IsNullOrEmpty(itemId))
        {
            xai["itemId"] = itemId;
        }

        return OpenAIJson.Element(new JsonObject { ["xai"] = xai });
    }

    /// <summary>Call-level provider metadata, or null when the response reports none.</summary>
    public static JsonElement? ProviderMetadata(JsonNode? costInUsdTicks, string? serviceTier, string? promptCacheKey, string? safetyIdentifier)
    {
        if (costInUsdTicks == null && serviceTier == null && promptCacheKey == null && safetyIdentifier == null)
        {
            return null;
        }

        var xai = new JsonObject();
        if (costInUsdTicks != null)
        {
            xai["costInUsdTicks"] = costInUsdTicks.DeepClone();
        }

        if (serviceTier != null)
        {
            xai["serviceTier"] = serviceTier;
        }

        if (promptCacheKey != null)
        {
            xai["promptCacheKey"] = promptCacheKey;
        }

        if (safetyIdentifier != null)
        {
            xai["safetyIdentifier"] = safetyIdentifier;
        }

        return OpenAIJson.Element(new JsonObject { ["xai"] = xai });
    }

    /// <summary>The URL source of a <c>url_citation</c> annotation, or null for other annotations.</summary>
    public static GeneratedSource? Citation(JsonElement annotation, Func<string> generateId)
    {
        if (OpenAIJson.String(annotation, "type") != "url_citation" || OpenAIJson.String(annotation, "url") is not { } url)
        {
            return null;
        }

        return new GeneratedSource(generateId(), url, OpenAIJson.String(annotation, "title") ?? url);
    }

    /// <summary>The <c>created_at</c> time of a response. Zero is a valid time, so it gives the Unix epoch.</summary>
    public static DateTimeOffset? CreatedAt(JsonElement response)
    {
        return OpenAIJson.Child(response, "created_at") is { ValueKind: JsonValueKind.Number } created && created.TryGetInt64(out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
    }

    public List<LanguageModelStreamPart> Accept(string data, bool includeRaw)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (includeRaw)
        {
            parts.Add(new RawStreamPart(data));
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(data);
        }
        catch (JsonException)
        {
            parts.Add(new ErrorStreamPart("JSON parsing failed: Text: " + data + "."));
            return parts;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || OpenAIJson.String(root, "type") is not { } type)
            {
                parts.Add(new ErrorStreamPart("Invalid xAI Responses stream chunk."));
                return parts;
            }

            try
            {
                Handle(type, root, parts);
            }
            catch (InvalidOperationException exception)
            {
                parts.Add(new ErrorStreamPart("Invalid xAI Responses stream chunk: " + exception.Message));
            }
        }

        return parts;
    }

    public List<LanguageModelStreamPart> Flush()
    {
        var parts = new List<LanguageModelStreamPart>();
        foreach (var id in _textBlocks)
        {
            parts.Add(new TextEndStreamPart(id));
        }

        parts.Add(new FinishStreamPart(
            _finish,
            _usage ?? XaiResponsesResults.MissingUsage(),
            _rawFinish,
            ProviderMetadata(_costInUsdTicks, _serviceTier, _promptCacheKey, _safetyIdentifier)));
        return parts;
    }

    private void Handle(string type, JsonElement root, List<LanguageModelStreamPart> parts)
    {
        switch (type)
        {
            case "response.created":
            case "response.in_progress":
                if (!_responseMetadataSent && OpenAIJson.Child(root, "response") is { } response)
                {
                    _responseMetadataSent = true;
                    parts.Add(new ResponseMetadataStreamPart(
                        OpenAIJson.String(response, "id"),
                        OpenAIJson.String(response, "model"),
                        CreatedAt(response)));
                }

                break;
            case "response.reasoning_summary_part.added":
                StartReasoning(Required(root, "item_id"), parts);
                break;
            case "response.reasoning_summary_text.delta":
                var summaryItem = Required(root, "item_id");
                parts.Add(new ReasoningDeltaStreamPart(ReasoningId(summaryItem), Required(root, "delta"), ReasoningMetadata(summaryItem, null)));
                break;
            case "response.reasoning_text.delta":
                var reasoningItem = Required(root, "item_id");
                StartReasoning(reasoningItem, parts);
                parts.Add(new ReasoningDeltaStreamPart(ReasoningId(reasoningItem), Required(root, "delta"), ReasoningMetadata(reasoningItem, null)));
                break;
            case "response.output_text.delta":
                var blockId = TextId(Required(root, "item_id"));
                StartText(blockId, parts);
                parts.Add(new TextDeltaStreamPart(blockId, Required(root, "delta")));
                break;
            case "response.output_text.done":
                AddSources(OpenAIJson.Child(root, "annotations"), parts);
                break;
            case "response.output_text.annotation.added":
                if (OpenAIJson.Child(root, "annotation") is { } annotation && Citation(annotation, _generateId) is { } source)
                {
                    parts.Add(ToPart(source));
                }

                break;
            case "response.done":
            case "response.completed":
            case "response.incomplete":
                Finish(type, OpenAIJson.Child(root, "response") ?? throw new InvalidOperationException("missing response"));
                break;
            case "response.failed":
                Fail(root, parts);
                break;
            case "error":
                parts.Add(StreamError(root, Required(root, "message")));
                break;
            case "response.function_call_arguments.delta":
                if (_ongoingToolCalls.TryGetValue(OutputIndex(root), out var toolCallId))
                {
                    parts.Add(new ToolInputDeltaStreamPart(toolCallId, Required(root, "delta")));
                }

                break;
            case "response.image_generation_call.in_progress":
            case "response.image_generation_call.generating":
            case "response.image_generation_call.completed":
                var imageItem = Required(root, "item_id");
                StartProviderToolCall(imageItem, _toolNames.Resolve("image_generation_call", null), "{}", parts);
                break;
            case "response.output_item.added":
            case "response.output_item.done":
                Item(type == "response.output_item.done", root, parts);
                break;
        }
    }

    private void Item(bool done, JsonElement root, List<LanguageModelStreamPart> parts)
    {
        var item = OpenAIJson.Child(root, "item") ?? throw new InvalidOperationException("missing item");
        var itemType = OpenAIJson.String(item, "type");
        if (XaiResponsesOutput.IsProviderToolItem(itemType))
        {
            ProviderToolItem(done, itemType!, item, parts);
            return;
        }

        switch (itemType)
        {
            case "reasoning":
                if (done)
                {
                    var id = Required(item, "id");
                    StartReasoning(id, parts);
                    parts.Add(new ReasoningEndStreamPart(ReasoningId(id), ReasoningMetadata(id, OpenAIJson.String(item, "encrypted_content"))));
                    _activeReasoning.Remove(id);
                }

                break;
            case "message":
                Message(item, parts);
                break;
            case "function_call":
                var callId = Required(item, "call_id");
                if (!done)
                {
                    // Argument deltas that follow carry only the output index.
                    _ongoingToolCalls[OutputIndex(root)] = callId;
                    parts.Add(new ToolInputStartStreamPart(callId, Required(item, "name")));
                }
                else
                {
                    _hasFunctionCall = true;
                    _ongoingToolCalls.Remove(OutputIndex(root));
                    parts.Add(new ToolInputEndStreamPart(callId));
                    parts.Add(new ToolCallStreamPart(callId, Required(item, "name"), Required(item, "arguments")));
                }

                break;
        }
    }

    private void ProviderToolItem(bool done, string itemType, JsonElement item, List<LanguageModelStreamPart> parts)
    {
        var id = Required(item, "id");
        var toolName = _toolNames.Resolve(itemType, OpenAIJson.String(item, "name"));

        // A custom tool call has its final input only when the item is done.
        if (itemType != "custom_tool_call" || done)
        {
            StartProviderToolCall(id, toolName, XaiResponsesOutput.ToolInput(itemType, item), parts);
        }

        if (!done)
        {
            return;
        }

        switch (itemType)
        {
            case "file_search_call":
                parts.Add(new ToolResultStreamPart(id, toolName, OpenAIJson.Element(XaiResponsesOutput.FileSearchResult(item))));
                break;
            case "image_generation_call":
                var (result, isError) = XaiResponsesOutput.ImageGenerationResult(item);
                parts.Add(new ToolResultStreamPart(id, toolName, OpenAIJson.Element(result)) { IsError = isError });
                break;
            case "web_search_call":
                parts.Add(new ToolResultStreamPart(id, toolName, OpenAIJson.Element(XaiResponsesOutput.WebSearchResult(OpenAIJson.Child(item, "action")))));
                break;
            default:
                parts.Add(new ToolResultStreamPart(id, toolName, OpenAIJson.Element(new JsonObject())));
                break;
        }
    }

    /// <summary>Sends the input and the call of a provider-executed tool once per item id.</summary>
    private void StartProviderToolCall(string id, string toolName, string input, List<LanguageModelStreamPart> parts)
    {
        if (!_seenToolCalls.Add(id))
        {
            return;
        }

        parts.Add(new ToolInputStartStreamPart(id, toolName));
        parts.Add(new ToolInputDeltaStreamPart(id, input));
        parts.Add(new ToolInputEndStreamPart(id));
        parts.Add(new ToolCallStreamPart(id, toolName, input) { ProviderExecuted = true });
    }

    private void Message(JsonElement item, List<LanguageModelStreamPart> parts)
    {
        if (OpenAIJson.Child(item, "content") is not { ValueKind: JsonValueKind.Array } contentParts)
        {
            return;
        }

        foreach (var part in contentParts.EnumerateArray())
        {
            if (OpenAIJson.String(part, "text") is { Length: > 0 } text)
            {
                var blockId = TextId(Required(item, "id"));

                // Text that already streamed through output_text.delta events is not sent a second time.
                if (!_textBlockSet.Contains(blockId))
                {
                    StartText(blockId, parts);
                    parts.Add(new TextDeltaStreamPart(blockId, text));
                }
            }

            AddSources(OpenAIJson.Child(part, "annotations"), parts);
        }
    }

    private void Finish(string type, JsonElement response)
    {
        if (OpenAIJson.Child(response, "usage") is { } usage)
        {
            _usage = XaiResponsesResults.ConvertUsage(usage);
            _costInUsdTicks = OpenAIJson.Node(OpenAIJson.Child(usage, "cost_in_usd_ticks"));
        }

        _serviceTier = OpenAIJson.String(response, "service_tier");
        _promptCacheKey = OpenAIJson.String(response, "prompt_cache_key");
        _safetyIdentifier = OpenAIJson.String(response, "safety_identifier");

        if (type == "response.incomplete")
        {
            var reason = IncompleteReason(response);
            _finish = reason != null ? XaiResponsesResults.MapFinishReason(reason) : FinishReason.Other;
            _rawFinish = reason ?? "incomplete";
        }
        else if (OpenAIJson.String(response, "status") is { Length: > 0 } status)
        {
            _finish = _hasFunctionCall ? FinishReason.ToolCalls : XaiResponsesResults.MapFinishReason(status);
            _rawFinish = status;
        }
    }

    private void Fail(JsonElement root, List<LanguageModelStreamPart> parts)
    {
        var response = OpenAIJson.Child(root, "response") ?? throw new InvalidOperationException("missing response");
        var reason = IncompleteReason(response);
        _finish = reason != null ? XaiResponsesResults.MapFinishReason(reason) : FinishReason.Error;
        _rawFinish = reason ?? "error";
        if (OpenAIJson.Child(response, "usage") is { } usage)
        {
            _usage = XaiResponsesResults.ConvertUsage(usage);
        }

        if (OpenAIJson.Child(response, "error") is { } error)
        {
            parts.Add(StreamError(root, Required(error, "message")));
        }
    }

    private static string? IncompleteReason(JsonElement response)
    {
        return OpenAIJson.String(OpenAIJson.Child(response, "incomplete_details"), "reason");
    }

    private void StartReasoning(string itemId, List<LanguageModelStreamPart> parts)
    {
        if (_activeReasoning.Add(itemId))
        {
            parts.Add(new ReasoningStartStreamPart(ReasoningId(itemId), ReasoningMetadata(itemId, null)));
        }
    }

    private void StartText(string blockId, List<LanguageModelStreamPart> parts)
    {
        if (_textBlockSet.Add(blockId))
        {
            _textBlocks.Add(blockId);
            parts.Add(new TextStartStreamPart(blockId));
        }
    }

    private void AddSources(JsonElement? annotations, List<LanguageModelStreamPart> parts)
    {
        if (annotations is not { ValueKind: JsonValueKind.Array } list)
        {
            return;
        }

        foreach (var annotation in list.EnumerateArray())
        {
            if (Citation(annotation, _generateId) is { } source)
            {
                parts.Add(ToPart(source));
            }
        }
    }

    private static SourceStreamPart ToPart(GeneratedSource source)
    {
        return new SourceStreamPart(source.Id, source.Url, source.Title);
    }

    private static string ReasoningId(string itemId)
    {
        return "reasoning-" + itemId;
    }

    private static string TextId(string itemId)
    {
        return "text-" + itemId;
    }

    private static int OutputIndex(JsonElement element)
    {
        return OpenAIJson.Int(element, "output_index") ?? throw new InvalidOperationException("missing output_index");
    }

    private static ErrorStreamPart StreamError(JsonElement root, string message)
    {
        return OpenAIStreamError.CreateProviderStreamError(root) is { } streamError
            ? new ErrorStreamPart(streamError)
            : new ErrorStreamPart(message);
    }

    private static string Required(JsonElement element, string name)
    {
        return OpenAIJson.String(element, name) ?? throw new InvalidOperationException("missing " + name);
    }
}
