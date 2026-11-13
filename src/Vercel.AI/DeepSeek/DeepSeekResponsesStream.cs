// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.DeepSeek.DeepSeekResponsesLanguageModel;

namespace Vercel.AI.DeepSeek;

/// <summary>Turns DeepSeek Responses stream events into language-model stream parts.</summary>
internal sealed class DeepSeekResponsesStream
{
    private readonly string _providerOptionsName;
    private readonly string _webSearchToolName;
    private readonly List<string> _openText = new();
    private readonly List<string> _openReasoning = new();
    private readonly Dictionary<string, (string CallId, string ToolName, string Arguments)> _functionCalls = new(StringComparer.Ordinal);
    private readonly List<JsonElement> _logprobs = new();
    private FinishReason _finish = FinishReason.Interrupted;
    private string? _rawFinish;
    private JsonElement? _usage;
    private string? _responseId;
    private bool _hasFunctionCall;

    public DeepSeekResponsesStream(string providerOptionsName, string webSearchToolName)
    {
        _providerOptionsName = providerOptionsName;
        _webSearchToolName = webSearchToolName;
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
            _finish = FinishReason.Error;
            parts.Add(new ErrorStreamPart("JSON parsing failed: Text: " + data + "."));
            return parts;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || OpenAIJson.String(root, "type") is not { } type)
            {
                _finish = FinishReason.Error;
                parts.Add(new ErrorStreamPart("Invalid DeepSeek Responses stream chunk."));
                return parts;
            }

            try
            {
                Handle(type, root, parts);
            }
            catch (InvalidOperationException exception)
            {
                _finish = FinishReason.Error;
                parts.Add(new ErrorStreamPart("Invalid DeepSeek Responses stream chunk: " + exception.Message));
            }
        }

        return parts;
    }

    public List<LanguageModelStreamPart> Flush()
    {
        var parts = new List<LanguageModelStreamPart>();
        foreach (var id in _openReasoning)
        {
            parts.Add(new ReasoningEndStreamPart(id));
        }

        foreach (var id in _openText)
        {
            parts.Add(new TextEndStreamPart(id));
        }

        parts.Add(new FinishStreamPart(
            _finish,
            DeepSeekResponsesResults.ConvertUsage(_usage),
            _rawFinish,
            DeepSeekResponsesResults.ProviderMetadata(_providerOptionsName, _responseId, _usage, _logprobs)));
        return parts;
    }

    private void Handle(string type, JsonElement root, List<LanguageModelStreamPart> parts)
    {
        switch (type)
        {
            case "response.created":
                var created = Response(root);
                _responseId = OpenAIJson.String(created, "id");
                parts.Add(new ResponseMetadataStreamPart(_responseId, OpenAIJson.String(created, "model"), CreatedAt(created)));
                break;
            case "response.output_item.added":
                ItemAdded(Item(root), parts);
                break;
            case "response.output_text.delta":
                var textId = Required(root, "item_id");
                if (OpenAIJson.Child(root, "logprobs") is { ValueKind: JsonValueKind.Array } logprobs)
                {
                    _logprobs.AddRange(logprobs.EnumerateArray().Select(entry => entry.Clone()));
                }

                if (!_openText.Contains(textId))
                {
                    _openText.Add(textId);
                    parts.Add(new TextStartStreamPart(textId));
                }

                parts.Add(new TextDeltaStreamPart(textId, Required(root, "delta")));
                break;
            case "response.reasoning_text.delta":
                var reasoningId = Required(root, "item_id");
                if (!_openReasoning.Contains(reasoningId))
                {
                    _openReasoning.Add(reasoningId);
                    parts.Add(new ReasoningStartStreamPart(reasoningId, ItemIdMetadata(_providerOptionsName, reasoningId)));
                }

                parts.Add(new ReasoningDeltaStreamPart(reasoningId, Required(root, "delta")));
                break;
            case "response.function_call_arguments.delta":
                var delta = Required(root, "delta");
                if (_functionCalls.TryGetValue(Required(root, "item_id"), out var call))
                {
                    _functionCalls[Required(root, "item_id")] = (call.CallId, call.ToolName, call.Arguments + delta);
                    parts.Add(new ToolInputDeltaStreamPart(call.CallId, delta));
                }

                break;
            case "response.output_item.done":
                ItemDone(Item(root), parts);
                break;
            case "response.completed":
            case "response.incomplete":
            case "response.failed":
                Finish(type, root, parts);
                break;
            case "error":
                _finish = FinishReason.Error;
                var message = OpenAIJson.String(root, "message") ?? "DeepSeek stream error";
                parts.Add(new ErrorStreamPart(new ProviderStreamError(message, root.Clone(), "error", OpenAIJson.String(root, "code"))));
                break;
        }
    }

    private void ItemAdded(JsonElement item, List<LanguageModelStreamPart> parts)
    {
        var itemId = OpenAIJson.String(item, "id");
        switch (OpenAIJson.String(item, "type"))
        {
            case "message" when itemId != null:
                if (!_openText.Contains(itemId))
                {
                    _openText.Add(itemId);
                }

                parts.Add(new TextStartStreamPart(itemId));
                break;
            case "reasoning" when itemId != null:
                if (!_openReasoning.Contains(itemId))
                {
                    _openReasoning.Add(itemId);
                }

                parts.Add(new ReasoningStartStreamPart(itemId, ItemIdMetadata(_providerOptionsName, itemId)));
                break;
            case "function_call":
                var callId = Required(item, "call_id");
                var name = Required(item, "name");
                _functionCalls[itemId ?? callId] = (callId, name, OpenAIJson.String(item, "arguments") ?? string.Empty);
                parts.Add(new ToolInputStartStreamPart(callId, name));
                break;
        }
    }

    private void ItemDone(JsonElement item, List<LanguageModelStreamPart> parts)
    {
        var itemId = OpenAIJson.String(item, "id");
        switch (OpenAIJson.String(item, "type"))
        {
            case "message" when itemId != null:
                if (_openText.Remove(itemId))
                {
                    parts.Add(new TextEndStreamPart(itemId));
                }

                break;
            case "reasoning" when itemId != null:
                if (_openReasoning.Remove(itemId))
                {
                    parts.Add(new ReasoningEndStreamPart(itemId, ItemIdMetadata(_providerOptionsName, itemId)));
                }

                break;
            case "function_call":
                var callId = Required(item, "call_id");
                var name = Required(item, "name");
                var key = itemId ?? callId;
                var tracked = _functionCalls.TryGetValue(key, out var existing);
                _functionCalls.Remove(key);
                _hasFunctionCall = true;
                if (!tracked)
                {
                    parts.Add(new ToolInputStartStreamPart(callId, name));
                }

                parts.Add(new ToolInputEndStreamPart(callId));
                parts.Add(new ToolCallStreamPart(callId, name, OpenAIJson.String(item, "arguments") ?? (tracked ? existing.Arguments : string.Empty)));
                break;
            case "web_search_call":
                var id = Required(item, "id");
                parts.Add(new ToolCallStreamPart(id, _webSearchToolName, "{}", WebSearchCallMetadata(_providerOptionsName, item)) { ProviderExecuted = true });
                parts.Add(new ToolResultStreamPart(id, _webSearchToolName, OpenAIJson.Element(WebSearchResult(item))));
                break;
        }
    }

    private void Finish(string type, JsonElement root, List<LanguageModelStreamPart> parts)
    {
        var response = Response(root);
        _usage = OpenAIJson.Child(response, "usage") is { ValueKind: JsonValueKind.Object } usage ? usage.Clone() : null;
        _responseId = OpenAIJson.String(response, "id") ?? _responseId;
        var status = OpenAIJson.String(response, "status");
        var incompleteReason = OpenAIJson.String(OpenAIJson.Child(response, "incomplete_details"), "reason");
        _finish = DeepSeekResponsesResults.MapFinishReason(status, incompleteReason, _hasFunctionCall);
        _rawFinish = incompleteReason ?? status;
        if (type != "response.failed")
        {
            return;
        }

        var error = OpenAIJson.Child(response, "error");
        var code = OpenAIJson.String(error, "code");
        var (statusCode, isRetryable) = FailedResponseStatus(code);
        parts.Add(new ErrorStreamPart(new ProviderStreamError(
            OpenAIJson.String(error, "message") ?? "DeepSeek response failed",
            root.Clone(),
            "response.failed",
            code,
            statusCode,
            isRetryable)));
    }

    private static JsonElement Response(JsonElement root)
    {
        return OpenAIJson.Child(root, "response") ?? throw new InvalidOperationException("missing response");
    }

    private static JsonElement Item(JsonElement root)
    {
        return OpenAIJson.Child(root, "item") ?? throw new InvalidOperationException("missing item");
    }
}
