// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenResponses;

/// <summary>Usage and provider metadata shared by generate and stream.</summary>
internal static class OpenResponsesResults
{
    /// <summary>Converts a Responses <c>usage</c> object. Uncached input and text output are derived from the details.</summary>
    public static LanguageModelUsage Usage(JsonElement usage)
    {
        var inputTokens = OpenAIJson.Int(usage, "input_tokens");
        var cachedInputTokens = OpenAIJson.NestedInt(usage, "input_tokens_details", "cached_tokens");
        var outputTokens = OpenAIJson.Int(usage, "output_tokens");
        var reasoningTokens = OpenAIJson.NestedInt(usage, "output_tokens_details", "reasoning_tokens");
        return new LanguageModelUsage(
            inputTokens,
            outputTokens,
            null,
            cacheReadTokens: cachedInputTokens,
            reasoningTokens: reasoningTokens,
            raw: usage.Clone(),
            noCacheInputTokens: (inputTokens ?? 0) - (cachedInputTokens ?? 0),
            textTokens: (outputTokens ?? 0) - (reasoningTokens ?? 0));
    }

    /// <summary>
    /// Metadata of a reasoning item: its id, summary, content (null when the item has none), and encrypted content.
    /// <paramref name="reasoningContent"/> replaces the item content when the caller splits the item into parts.
    /// </summary>
    public static JsonElement ReasoningMetadata(string providerOptionsName, JsonElement item, IEnumerable<JsonElement>? reasoningContent = null)
    {
        var summary = new JsonArray();
        if (OpenAIJson.Child(item, "summary") is { ValueKind: JsonValueKind.Array } summaryParts)
        {
            foreach (var part in summaryParts.EnumerateArray())
            {
                summary.Add(new JsonObject { ["type"] = "summary_text", ["text"] = OpenAIJson.String(part, "text") });
            }
        }

        reasoningContent ??= OpenAIJson.Child(item, "content") is { ValueKind: JsonValueKind.Array } contentParts
            ? contentParts.EnumerateArray()
            : null;
        JsonArray? content = null;
        if (reasoningContent != null)
        {
            content = new JsonArray();
            foreach (var part in reasoningContent)
            {
                content.Add(new JsonObject { ["type"] = "reasoning_text", ["text"] = OpenAIJson.String(part, "text") });
            }
        }

        var data = new JsonObject
        {
            ["itemId"] = OpenAIJson.String(item, "id"),
            ["reasoningSummary"] = summary,
            ["reasoningContent"] = content,
        };
        if (OpenAIJson.String(item, "encrypted_content") is { } encrypted)
        {
            data["reasoningEncryptedContent"] = encrypted;
        }

        return OpenAIJson.Element(new JsonObject { [providerOptionsName] = data });
    }

    /// <summary>Metadata of a message item, with its <c>url_citation</c> annotations when every content part has valid ones.</summary>
    public static JsonElement MessageMetadata(string providerOptionsName, string? itemId, JsonArray annotations)
    {
        var data = new JsonObject { ["itemId"] = itemId };
        if (annotations.Count > 0)
        {
            data["annotations"] = annotations;
        }

        return OpenAIJson.Element(new JsonObject { [providerOptionsName] = data });
    }

    /// <summary>The <c>url_citation</c> annotations of a content part, or none when any annotation is not a valid citation.</summary>
    public static JsonArray Annotations(JsonElement contentPart)
    {
        var result = new JsonArray();
        if (OpenAIJson.Child(contentPart, "annotations") is not { ValueKind: JsonValueKind.Array } annotations)
        {
            return result;
        }

        foreach (var annotation in annotations.EnumerateArray())
        {
            if (OpenAIJson.String(annotation, "type") != "url_citation"
                || OpenAIJson.Child(annotation, "start_index") is not { ValueKind: JsonValueKind.Number } start
                || OpenAIJson.Child(annotation, "end_index") is not { ValueKind: JsonValueKind.Number } end
                || OpenAIJson.String(annotation, "url") is not { } url
                || OpenAIJson.String(annotation, "title") is not { } title)
            {
                return new JsonArray();
            }

            result.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = OpenAIJson.Node(start),
                ["end_index"] = OpenAIJson.Node(end),
                ["url"] = url,
                ["title"] = title,
            });
        }

        return result;
    }

    public static JsonElement ItemMetadata(string providerOptionsName, string? itemId)
    {
        return OpenAIJson.Element(new JsonObject { [providerOptionsName] = new JsonObject { ["itemId"] = itemId } });
    }
}

/// <summary>Turns Open Responses stream events into language-model stream parts.</summary>
internal sealed class OpenResponsesStream
{
    private readonly string _providerOptionsName;
    private readonly OpenResponsesExtensionRegistry _registry;
    private readonly Dictionary<string, object?> _extensionState = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ToolCallState> _toolCallsByItemId = new(StringComparer.Ordinal);
    private string? _activeReasoningId;
    private bool _hasToolCalls;
    private FinishReason _finishReason = FinishReason.Other;
    private string? _rawFinishReason;
    private LanguageModelUsage _usage = LanguageModelUsage.Null;

    public OpenResponsesStream(string providerOptionsName, OpenResponsesExtensionRegistry registry)
    {
        _providerOptionsName = providerOptionsName;
        _registry = registry;
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
            Accept(document.RootElement, parts);
        }

        return parts;
    }

    public List<LanguageModelStreamPart> Flush()
    {
        var parts = new List<LanguageModelStreamPart>();
        if (_activeReasoningId != null)
        {
            parts.Add(new ReasoningEndStreamPart(_activeReasoningId));
        }

        parts.Add(new FinishStreamPart(_finishReason, _usage, _rawFinishReason));
        return parts;
    }

    private void Accept(JsonElement chunk, List<LanguageModelStreamPart> parts)
    {
        var type = OpenAIJson.String(chunk, "type");
        var item = OpenAIJson.Child(chunk, "item");
        var itemType = OpenAIJson.String(item, "type");
        var itemId = OpenAIJson.String(item, "id") ?? string.Empty;
        if (type != null && type.Contains(':'))
        {
            AcceptExtensionEvent(type, chunk, parts);
            return;
        }

        switch (type)
        {
            case "response.output_item.added" when itemType == "function_call":
                _toolCallsByItemId[itemId] = new ToolCallState
                {
                    ToolName = OpenAIJson.String(item, "name"),
                    ToolCallId = OpenAIJson.String(item, "call_id"),
                    Arguments = OpenAIJson.String(item, "arguments"),
                };
                break;
            case "response.function_call_arguments.delta":
                AppendArguments(OpenAIJson.String(chunk, "item_id") ?? string.Empty, OpenAIJson.String(chunk, "delta") ?? string.Empty, replace: false);
                break;
            case "response.function_call_arguments.done":
                AppendArguments(OpenAIJson.String(chunk, "item_id") ?? string.Empty, OpenAIJson.String(chunk, "arguments") ?? string.Empty, replace: true);
                break;
            case "response.output_item.done" when itemType == "function_call":
                _toolCallsByItemId.TryGetValue(itemId, out var toolCall);
                parts.Add(new ToolCallStreamPart(
                    toolCall?.ToolCallId ?? OpenAIJson.String(item, "call_id") ?? string.Empty,
                    toolCall?.ToolName ?? OpenAIJson.String(item, "name") ?? string.Empty,
                    toolCall?.Arguments ?? OpenAIJson.String(item, "arguments") ?? string.Empty,
                    OpenResponsesResults.ItemMetadata(_providerOptionsName, itemId)));
                _hasToolCalls = true;
                _toolCallsByItemId.Remove(itemId);
                break;
            case "response.output_item.done" when itemType != null && itemType.Contains(':'):
                AcceptExtensionItem(item!.Value, parts);
                break;
            case "response.output_item.added" when itemType == "reasoning":
                parts.Add(new ReasoningStartStreamPart(itemId));
                _activeReasoningId = itemId;
                break;
            case "response.reasoning_summary_text.delta":
            case "response.reasoning_text.delta":
                // response.reasoning_text.delta is an LM Studio extension, not part of the Open Responses specification.
                parts.Add(new ReasoningDeltaStreamPart(OpenAIJson.String(chunk, "item_id") ?? string.Empty, OpenAIJson.String(chunk, "delta") ?? string.Empty));
                break;
            case "response.output_item.done" when itemType == "reasoning":
                parts.Add(new ReasoningEndStreamPart(itemId, OpenResponsesResults.ReasoningMetadata(_providerOptionsName, item!.Value)));
                if (_activeReasoningId == itemId)
                {
                    _activeReasoningId = null;
                }

                break;
            case "response.output_item.added" when itemType == "message":
                parts.Add(new TextStartStreamPart(itemId));
                break;
            case "response.output_text.delta":
                parts.Add(new TextDeltaStreamPart(OpenAIJson.String(chunk, "item_id") ?? string.Empty, OpenAIJson.String(chunk, "delta") ?? string.Empty));
                break;
            case "response.output_item.done" when itemType == "message":
                var annotations = new JsonArray();
                if (OpenAIJson.Child(item, "content") is { ValueKind: JsonValueKind.Array } content)
                {
                    foreach (var part in content.EnumerateArray())
                    {
                        foreach (var annotation in OpenResponsesResults.Annotations(part))
                        {
                            annotations.Add(annotation!.DeepClone());
                        }
                    }
                }

                parts.Add(new TextEndStreamPart(itemId, providerMetadata: OpenResponsesResults.MessageMetadata(_providerOptionsName, itemId, annotations)));
                break;
            case "response.completed":
            case "response.incomplete":
                var response = OpenAIJson.Child(chunk, "response");
                _rawFinishReason = OpenAIJson.String(OpenAIJson.Child(response, "incomplete_details"), "reason");
                _finishReason = OpenResponsesFinishReason.Map(_rawFinishReason, _hasToolCalls);
                UpdateUsage(response);
                break;
            case "response.failed":
                var failed = OpenAIJson.Child(chunk, "response");
                var failedError = OpenAIJson.Child(failed, "error");
                _finishReason = FinishReason.Error;
                _rawFinishReason = OpenAIJson.String(failedError, "code") ?? OpenAIJson.String(failed, "status");
                UpdateUsage(failed);
                if (failedError is { ValueKind: JsonValueKind.Object })
                {
                    parts.Add(new ErrorStreamPart(OpenAIJson.String(failedError, "message") ?? string.Empty));
                }

                break;
            case "error":
                var error = OpenAIJson.Child(chunk, "error");
                _finishReason = FinishReason.Error;
                _rawFinishReason = OpenAIJson.String(error, "code");
                parts.Add(new ErrorStreamPart(OpenAIJson.String(error, "message") ?? string.Empty));
                break;
        }
    }

    private void AcceptExtensionEvent(string type, JsonElement chunk, List<LanguageModelStreamPart> parts)
    {
        if (OpenAIJson.Node(chunk) is not JsonObject @event
            || !OpenResponsesJson.IsExtensionEvent(@event)
            || !_registry.ByEventType.TryGetValue(type, out var extension)
            || extension.DecodeEvent == null)
        {
            return;
        }

        IReadOnlyList<LanguageModelStreamPart>? decoded;
        try
        {
            decoded = extension.DecodeEvent(new OpenResponsesDecodeEventContext(@event, _extensionState));
        }
        catch (Exception exception)
        {
            parts.Add(new ErrorStreamPart(exception.Message));
            return;
        }

        foreach (var part in decoded ?? Array.Empty<LanguageModelStreamPart>())
        {
            parts.Add(part);
            _hasToolCalls |= part is ToolCallStreamPart || part is ToolInputStartStreamPart;
        }
    }

    private void AcceptExtensionItem(JsonElement item, List<LanguageModelStreamPart> parts)
    {
        if (OpenAIJson.Node(item) is not JsonObject extensionItem || !OpenResponsesJson.IsExtensionItem(extensionItem))
        {
            return;
        }

        List<GeneratedContent>? decoded;
        try
        {
            decoded = OpenResponsesExtensionItems.Decode(_registry, _providerOptionsName, extensionItem, OpenResponsesItemMode.Stream);
        }
        catch (Exception exception)
        {
            parts.Add(new ErrorStreamPart(exception.Message));
            return;
        }

        foreach (var part in decoded ?? new List<GeneratedContent>())
        {
            parts.Add(OpenResponsesExtensionItems.ToStreamPart(part));
            _hasToolCalls |= part is GeneratedToolCall;
        }
    }

    private void AppendArguments(string itemId, string arguments, bool replace)
    {
        if (!_toolCallsByItemId.TryGetValue(itemId, out var toolCall))
        {
            _toolCallsByItemId[itemId] = new ToolCallState { Arguments = arguments };
            return;
        }

        toolCall.Arguments = replace ? arguments : (toolCall.Arguments ?? string.Empty) + arguments;
    }

    private void UpdateUsage(JsonElement? response)
    {
        if (OpenAIJson.Child(response, "usage") is { ValueKind: JsonValueKind.Object } usage)
        {
            _usage = OpenResponsesResults.Usage(usage);
        }
    }

    private sealed class ToolCallState
    {
        public string? ToolName { get; set; }

        public string? ToolCallId { get; set; }

        public string? Arguments { get; set; }
    }
}
