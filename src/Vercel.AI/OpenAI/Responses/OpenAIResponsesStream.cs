// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Turns Responses stream events into stream parts. Retrieved web and file sources are sources. Annotations become
/// the citations on the <c>text-end</c> of the message that carries them. Citation-derived sources are held back
/// until the stream ends, because the retrieval event can arrive after the annotation.
/// </summary>
internal sealed class OpenAIResponsesStream
{
    // JSON.stringify leaves characters such as '>' and non-ASCII text unescaped.
    private static readonly JsonSerializerOptions TextInputOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly Func<string> _generateId;
    private readonly List<JsonElement> _annotations = new();
    private readonly List<JsonElement> _fallbackAnnotations = new();
    private readonly HashSet<string> _retrievedFileIds = new(StringComparer.Ordinal);
    private string? _openTextId;
    private bool _hasWebSearchActionSources;
    private bool _hasFileSearchResults;
    private string? _incompleteReason;
    private bool _hasFunctionCall;
    private LanguageModelUsage _usage = LanguageModelUsage.Empty;
    private string? _responseId;
    private readonly IReadOnlyCollection<string> _functionToolNames;
    private readonly Dictionary<int, OngoingFunctionCall> _ongoingFunctionCalls = new();
    private readonly Dictionary<int, OngoingCustomToolCall> _ongoingCustomToolCalls = new();

    private readonly ToolNameMapping? _toolNameMapping;
    private readonly bool _store;
    private readonly Dictionary<string, ActiveReasoning> _activeReasoning = new(StringComparer.Ordinal);

    // Compatible providers can rotate item ids between events. The output index stays the same for the life of an item.
    private readonly Dictionary<int, string> _activeOutputItemIds = new();

    public OpenAIResponsesStream(Func<string>? generateId = null, IReadOnlyCollection<string>? functionToolNames = null, ToolNameMapping? toolNameMapping = null, bool store = false)
    {
        _store = store;
        _toolNameMapping = toolNameMapping;
        _functionToolNames = functionToolNames ?? Array.Empty<string>();
        _generateId = generateId ?? (() => Vercel.AI.ProviderUtils.JsonValues.GenerateId("source_"));
    }

    public IEnumerable<LanguageModelStreamPart> Push(JsonObject node)
    {
        var parts = new List<LanguageModelStreamPart>();
        var type = node["type"]?.GetValue<string>();
        switch (type)
        {
            case "response.created":
                if (node["response"] is JsonObject created)
                {
                    _responseId = created["id"]?.GetValue<string>();
                    parts.Add(new ResponseMetadataStreamPart(
                        _responseId,
                        created["model"]?.GetValue<string>(),
                        created["created_at"] is JsonValue createdAt && createdAt.TryGetValue<long>(out var seconds)
                            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
                            : null));
                }

                break;
            case "response.output_item.added":
                if (node["item"] is JsonObject functionCall
                    && functionCall["type"]?.GetValue<string>() == "function_call"
                    && node["output_index"] is JsonValue addedIndex
                    && addedIndex.TryGetValue<int>(out var functionCallIndex))
                {
                    var functionCallName = functionCall["name"]?.GetValue<string>() ?? string.Empty;
                    var functionCallId = functionCall["call_id"]?.GetValue<string>() ?? "call";
                    var suppress = OpenAIParallelToolCall.IsUndeclared(functionCallName, _functionToolNames);
                    _ongoingFunctionCalls[functionCallIndex] = new OngoingFunctionCall(
                        functionCallName,
                        functionCallId,
                        suppress,
                        functionCall["async"] is JsonValue functionAsync && functionAsync.TryGetValue<bool>(out var functionAsyncMode) ? functionAsyncMode : null);
                    if (!suppress)
                    {
                        parts.Add(new ToolInputStartStreamPart(functionCallId, functionCallName));
                    }
                }
                else if (node["item"] is JsonObject customCall
                    && customCall["type"]?.GetValue<string>() == "custom_tool_call"
                    && node["output_index"] is JsonValue customIndexValue
                    && customIndexValue.TryGetValue<int>(out var customIndex))
                {
                    var customCallId = customCall["call_id"]?.GetValue<string>() ?? "call";
                    var customName = customCall["name"]?.GetValue<string>() ?? string.Empty;
                    _ongoingCustomToolCalls[customIndex] = new OngoingCustomToolCall(
                        customCallId,
                        customCall["async"] is JsonValue asyncValue && asyncValue.TryGetValue<bool>(out var asyncMode) ? asyncMode : null);
                    parts.Add(new ToolInputStartStreamPart(customCallId, customName, inputEncoding: ToolInputEncoding.Text));
                }
                else if (node["item"] is JsonObject reasoningAdded
                    && reasoningAdded["type"]?.GetValue<string>() == "reasoning"
                    && reasoningAdded["id"]?.GetValue<string>() is { } reasoningId)
                {
                    if (OutputIndex(node) is { } reasoningIndex)
                    {
                        _activeOutputItemIds[reasoningIndex] = reasoningId;
                    }

                    var encrypted = reasoningAdded["encrypted_content"]?.GetValue<string>();
                    _activeReasoning[reasoningId] = new ActiveReasoning(encrypted);
                    parts.Add(new ReasoningStartStreamPart(ReasoningPartId(reasoningId, 0), ReasoningMetadata(reasoningId, encrypted, includeEncrypted: true)));
                }
                else if (node["item"] is JsonObject added && added["type"]?.GetValue<string>() == "message")
                {
                    _annotations.Clear();
                    OpenText(parts, added["id"]?.GetValue<string>() ?? "text");
                }

                break;
            case "response.output_text.delta":
                var itemId = node["item_id"]?.GetValue<string>() ?? _openTextId ?? "text";
                OpenText(parts, itemId);
                var delta = node["delta"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(delta))
                {
                    parts.Add(new TextDeltaStreamPart(itemId, delta!));
                }

                break;
            case "response.reasoning_summary_part.added":
                if (node["item_id"]?.GetValue<string>() is { } partItem)
                {
                    var partItemId = ResolveOutputItemId(partItem, node);
                    var summaryIndex = SummaryIndex(node);
                    // The first reasoning start comes from output_item.added.
                    if (summaryIndex > 0 && _activeReasoning.TryGetValue(partItemId, out var activeReasoning))
                    {
                        activeReasoning.SummaryParts[summaryIndex] = ReasoningPartState.Active;

                        // A new summary part lets every part that could conclude conclude.
                        foreach (var concludable in activeReasoning.SummaryParts.Where(entry => entry.Value == ReasoningPartState.CanConclude).Select(entry => entry.Key).ToList())
                        {
                            parts.Add(new ReasoningEndStreamPart(ReasoningPartId(partItemId, concludable), ReasoningMetadata(partItemId, null)));
                            activeReasoning.SummaryParts[concludable] = ReasoningPartState.Concluded;
                        }

                        parts.Add(new ReasoningStartStreamPart(
                            ReasoningPartId(partItemId, summaryIndex),
                            ReasoningMetadata(partItemId, activeReasoning.EncryptedContent, includeEncrypted: true)));
                    }
                }

                break;
            case "response.reasoning_summary_text.delta":
                if (node["item_id"]?.GetValue<string>() is { } deltaItem)
                {
                    var reasoningDeltaItemId = ResolveOutputItemId(deltaItem, node);
                    parts.Add(new ReasoningDeltaStreamPart(
                        ReasoningPartId(reasoningDeltaItemId, SummaryIndex(node)),
                        node["delta"]?.GetValue<string>() ?? string.Empty,
                        ReasoningMetadata(reasoningDeltaItemId, null)));
                }

                break;
            case "response.reasoning_summary_part.done":
                if (node["item_id"]?.GetValue<string>() is { } doneItem)
                {
                    var reasoningDoneItemId = ResolveOutputItemId(doneItem, node);
                    if (_activeReasoning.TryGetValue(reasoningDoneItemId, out var doneReasoning))
                    {
                        var doneSummaryIndex = SummaryIndex(node);
                        if (_store)
                        {
                            // Stored reasoning needs no encrypted content, so the part can end now.
                            parts.Add(new ReasoningEndStreamPart(ReasoningPartId(reasoningDoneItemId, doneSummaryIndex), ReasoningMetadata(reasoningDoneItemId, null)));
                            doneReasoning.SummaryParts[doneSummaryIndex] = ReasoningPartState.Concluded;
                        }
                        else
                        {
                            // Wait for the final item, which carries the encrypted content.
                            doneReasoning.SummaryParts[doneSummaryIndex] = ReasoningPartState.CanConclude;
                        }
                    }
                }

                break;
            case "response.output_text.annotation.added":
                if (node["annotation"] is JsonObject annotation)
                {
                    var element = JsonSerializer.Deserialize<JsonElement>(annotation.ToJsonString());
                    _annotations.Add(element);
                    var annotationType = annotation["type"]?.GetValue<string>();
                    if (annotationType == "url_citation" || annotationType == "file_citation")
                    {
                        // Retrieval events may arrive after the annotation. Decide the fallback once the stream ends.
                        _fallbackAnnotations.Add(element);
                    }
                    else
                    {
                        parts.Add(ToSourcePart(OpenAIResponsesOutput.AnnotationSource(element, _generateId())));
                    }
                }

                break;
            case "response.function_call_arguments.delta":
                if (node["output_index"] is JsonValue deltaIndex
                    && deltaIndex.TryGetValue<int>(out var deltaFunctionCallIndex)
                    && _ongoingFunctionCalls.TryGetValue(deltaFunctionCallIndex, out var ongoing)
                    && node["delta"]?.GetValue<string>() is { } argumentsDelta)
                {
                    if (ongoing.SuppressInputStreaming)
                    {
                        ongoing.BufferedInputDeltas.Add(argumentsDelta);
                    }
                    else
                    {
                        parts.Add(new ToolInputDeltaStreamPart(ongoing.ToolCallId, argumentsDelta));
                    }
                }

                break;
            case "response.custom_tool_call_input.delta":
                if (node["output_index"] is JsonValue customDeltaIndex
                    && customDeltaIndex.TryGetValue<int>(out var customDeltaKey)
                    && _ongoingCustomToolCalls.TryGetValue(customDeltaKey, out var ongoingCustom)
                    && ReadDelta(node["delta"]) is { } customDelta)
                {
                    parts.Add(new ToolInputDeltaStreamPart(ongoingCustom.ToolCallId, customDelta));
                }

                break;
            case "response.output_item.done":
                if (node["item"] is JsonObject functionCallDone
                    && functionCallDone["type"]?.GetValue<string>() == "function_call")
                {
                    var doneIndex = node["output_index"] is JsonValue indexValue && indexValue.TryGetValue<int>(out var parsedIndex)
                        ? parsedIndex
                        : -1;
                    _ongoingFunctionCalls.TryGetValue(doneIndex, out var finished);
                    _ongoingFunctionCalls.Remove(doneIndex);
                    FunctionCallDone(parts, JsonSerializer.Deserialize<JsonElement>(functionCallDone.ToJsonString()), finished);
                }
                else if (node["item"] is JsonObject customCallDone
                    && customCallDone["type"]?.GetValue<string>() == "custom_tool_call")
                {
                    var customDoneIndex = node["output_index"] is JsonValue customDoneValue && customDoneValue.TryGetValue<int>(out var parsedCustomIndex)
                        ? parsedCustomIndex
                        : -1;
                    _ongoingCustomToolCalls.TryGetValue(customDoneIndex, out var finishedCustom);
                    _ongoingCustomToolCalls.Remove(customDoneIndex);
                    CustomToolCallDone(parts, JsonSerializer.Deserialize<JsonElement>(customCallDone.ToJsonString()), finishedCustom);
                }
                else if (node["item"] is JsonObject reasoningDone
                    && reasoningDone["type"]?.GetValue<string>() == "reasoning"
                    && reasoningDone["id"]?.GetValue<string>() is { } reasoningDoneId)
                {
                    var itemIdForDone = ResolveOutputItemId(reasoningDoneId, node);
                    if (_activeReasoning.Remove(itemIdForDone, out var finishedReasoning))
                    {
                        var finalEncrypted = reasoningDone["encrypted_content"]?.GetValue<string>();
                        foreach (var summaryIndex in finishedReasoning.SummaryParts
                            .Where(entry => entry.Value is ReasoningPartState.Active or ReasoningPartState.CanConclude)
                            .Select(entry => entry.Key))
                        {
                            parts.Add(new ReasoningEndStreamPart(
                                ReasoningPartId(itemIdForDone, summaryIndex),
                                ReasoningMetadata(itemIdForDone, finalEncrypted, includeEncrypted: true)));
                        }
                    }

                    if (OutputIndex(node) is { } finishedIndex)
                    {
                        _activeOutputItemIds.Remove(finishedIndex);
                    }
                }
                else if (node["item"] is JsonObject done)
                {
                    ItemDone(parts, JsonSerializer.Deserialize<JsonElement>(done.ToJsonString()));
                }

                break;
            case "response.completed":
            case "response.incomplete":
                if (node["response"] is JsonObject response)
                {
                    _responseId = response["id"]?.GetValue<string>() ?? _responseId;
                    _incompleteReason = response["incomplete_details"]?["reason"]?.GetValue<string>();
                    if (response["output"] is JsonArray output)
                    {
                        foreach (var item in output)
                        {
                            _hasFunctionCall |= item?["type"]?.GetValue<string>() == "function_call";
                        }
                    }

                    if (response["usage"] != null)
                    {
                        _usage = OpenAIJson.ResponsesUsage(JsonSerializer.Deserialize<JsonElement>(response["usage"]!.ToJsonString()));
                    }
                }

                break;
            case "error":
                parts.Add(new ErrorStreamPart(node["message"]?.GetValue<string>() ?? node.ToJsonString()));
                break;
        }

        return parts;
    }

    /// <summary>Ends the stream: sources that only citations supplied, then the finish part.</summary>
    // A custom tool input delta can end in half of a surrogate pair. System.Text.Json refuses to decode a lone
    // surrogate escape, so decode the raw text by hand in that case.
    private static string? ReadDelta(JsonNode? node)
    {
        try
        {
            return node?.GetValue<string>();
        }
        catch (InvalidOperationException) when (node is JsonValue value && value.TryGetValue<JsonElement>(out var element))
        {
            var raw = element.GetRawText();
            var text = new System.Text.StringBuilder(raw.Length);
            for (var i = 1; i < raw.Length - 1; i++)
            {
                var c = raw[i];
                if (c != '\\')
                {
                    text.Append(c);
                    continue;
                }

                c = raw[++i];
                if (c == 'u')
                {
                    text.Append((char)int.Parse(raw.Substring(i + 1, 4), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture));
                    i += 4;
                }
                else
                {
                    text.Append(c switch { 'b' => '\b', 'f' => '\f', 'n' => '\n', 'r' => '\r', 't' => '\t', _ => c });
                }
            }

            return text.ToString();
        }
    }

    public IEnumerable<LanguageModelStreamPart> Complete()
    {
        var parts = new List<LanguageModelStreamPart>();
        if (_openTextId != null)
        {
            parts.Add(new TextEndStreamPart(_openTextId));
            _openTextId = null;
        }

        foreach (var annotation in _fallbackAnnotations)
        {
            var annotationType = OpenAIJson.String(annotation, "type");
            if ((annotationType == "url_citation" && _hasWebSearchActionSources)
                || (annotationType == "file_citation" && _hasFileSearchResults))
            {
                continue;
            }

            parts.Add(ToSourcePart(OpenAIResponsesOutput.AnnotationSource(annotation, _generateId())));
        }

        _fallbackAnnotations.Clear();
        parts.Add(new FinishStreamPart(
            OpenAIResponsesLanguageModel.MapFinishReason(_incompleteReason, _hasFunctionCall),
            _usage,
            _incompleteReason,
            OpenAIJson.ProviderMetadata(new JsonObject { ["responseId"] = _responseId })));
        return parts;
    }

    private void OpenText(List<LanguageModelStreamPart> parts, string id)
    {
        if (_openTextId == id)
        {
            return;
        }

        if (_openTextId != null)
        {
            parts.Add(new TextEndStreamPart(_openTextId));
        }

        _openTextId = id;
        parts.Add(new TextStartStreamPart(id, OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = id })));
    }

    private static int? OutputIndex(JsonObject node) =>
        node["output_index"] is JsonValue value && value.TryGetValue<int>(out var index) ? index : null;

    private static int SummaryIndex(JsonObject node) =>
        node["summary_index"] is JsonValue value && value.TryGetValue<int>(out var index) ? index : 0;

    private string ResolveOutputItemId(string itemId, JsonObject node) =>
        OutputIndex(node) is { } index && _activeOutputItemIds.TryGetValue(index, out var active) ? active : itemId;

    private static string ReasoningPartId(string itemId, int summaryIndex) => itemId + ":" + summaryIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);

    // The start and end parts carry the encrypted content, null when there is none. Deltas carry only the item id.
    private static JsonElement ReasoningMetadata(string itemId, string? encryptedContent, bool includeEncrypted = false)
    {
        var values = new JsonObject { ["itemId"] = itemId };
        if (includeEncrypted)
        {
            values["reasoningEncryptedContent"] = encryptedContent;
        }

        return OpenAIJson.ProviderMetadata(values);
    }

    private enum ReasoningPartState
    {
        Active,
        CanConclude,
        Concluded,
    }

    private sealed class ActiveReasoning(string? encryptedContent)
    {
        public string? EncryptedContent { get; } = encryptedContent;

        public Dictionary<int, ReasoningPartState> SummaryParts { get; } = new() { [0] = ReasoningPartState.Active };
    }

    private sealed class OngoingFunctionCall(string toolName, string toolCallId, bool suppressInputStreaming, bool? async)
    {
        public bool? Async { get; } = async;

        public string ToolName { get; } = toolName;

        public string ToolCallId { get; } = toolCallId;

        public bool SuppressInputStreaming { get; } = suppressInputStreaming;

        public List<string> BufferedInputDeltas { get; } = new();
    }

    private sealed class OngoingCustomToolCall(string toolCallId, bool? async)
    {
        public string ToolCallId { get; } = toolCallId;

        public bool? Async { get; } = async;
    }

    // The input deltas are raw text. The final tool call input is the JSON encoding of that text.
    private void CustomToolCallDone(List<LanguageModelStreamPart> parts, JsonElement item, OngoingCustomToolCall? ongoing)
    {
        _hasFunctionCall = true;
        var callId = OpenAIJson.String(item, "call_id") ?? ongoing?.ToolCallId ?? "call";
        var metadata = new JsonObject();
        if (OpenAIJson.String(item, "id") is { Length: > 0 } itemId)
        {
            metadata["itemId"] = itemId;
        }

        if (item.TryGetProperty("async", out var asyncMode) && asyncMode.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            metadata["async"] = asyncMode.GetBoolean();
        }
        else if (ongoing?.Async is { } ongoingAsync)
        {
            metadata["async"] = ongoingAsync;
        }

        parts.Add(new ToolInputEndStreamPart(callId));
        parts.Add(new ToolCallStreamPart(
            callId,
            OpenAIJson.String(item, "name") ?? string.Empty,
            JsonSerializer.Serialize(OpenAIJson.String(item, "input"), TextInputOptions),
            OpenAIJson.ProviderMetadata(metadata)));
    }

    private void FunctionCallDone(List<LanguageModelStreamPart> parts, JsonElement item, OngoingFunctionCall? ongoing)
    {
        _hasFunctionCall = true;
        var callId = OpenAIJson.String(item, "call_id") ?? ongoing?.ToolCallId ?? "call";
        var name = OpenAIJson.String(item, "name") ?? ongoing?.ToolName ?? string.Empty;
        var input = OpenAIJson.String(item, "arguments") ?? "{}";
        var itemId = OpenAIJson.String(item, "id") ?? string.Empty;
        var suppress = ongoing?.SuppressInputStreaming ?? OpenAIParallelToolCall.IsUndeclared(name, _functionToolNames);

        if (suppress
            && OpenAIParallelToolCall.Expand(callId, name, input, _functionToolNames, itemId) is { } expanded)
        {
            foreach (var call in expanded)
            {
                parts.Add(new ToolInputStartStreamPart(call.ToolCallId, call.ToolName));
                parts.Add(new ToolInputDeltaStreamPart(call.ToolCallId, call.Input));
                parts.Add(new ToolInputEndStreamPart(call.ToolCallId));
                parts.Add(new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.Input, call.ProviderMetadata));
            }

            return;
        }

        if (suppress)
        {
            parts.Add(new ToolInputStartStreamPart(callId, name));
            if (ongoing is { BufferedInputDeltas.Count: > 0 })
            {
                foreach (var delta in ongoing.BufferedInputDeltas)
                {
                    parts.Add(new ToolInputDeltaStreamPart(callId, delta));
                }
            }
            else if (input.Length > 0)
            {
                parts.Add(new ToolInputDeltaStreamPart(callId, input));
            }
        }

        JsonElement? endMetadata = OpenAIJson.String(item, "namespace") is { } ns
            ? OpenAIJson.ProviderMetadata(new JsonObject { ["namespace"] = ns })
            : null;
        parts.Add(new ToolInputEndStreamPart(callId, endMetadata));
        parts.Add(new ToolCallStreamPart(callId, name, input, OpenAIParallelToolCall.CallMetadata(item, ongoing?.Async)));
    }

    private void ItemDone(List<LanguageModelStreamPart> parts, JsonElement item)
    {
        var itemType = OpenAIJson.String(item, "type");
        if (itemType == "message")
        {
            var id = OpenAIJson.String(item, "id") ?? _openTextId ?? "text";
            _openTextId = null;
            parts.Add(new TextEndStreamPart(
                id,
                OpenAIResponsesOutput.Citations(_annotations),
                OpenAIResponsesOutput.TextMetadata(id, _annotations)));
            _annotations.Clear();
        }
        else if (itemType == "program")
        {
            parts.Add(new ToolCallStreamPart(
                OpenAIJson.String(item, "call_id") ?? "call",
                OpenAIResponsesLanguageModel.ProgrammaticToolName(_toolNameMapping),
                OpenAIResponsesOutput.ProgramInput(item),
                OpenAIResponsesOutput.ItemMetadata(item))
            {
                ProviderExecuted = true,
            });
        }
        else if (itemType == "program_output")
        {
            parts.Add(new ToolResultStreamPart(
                OpenAIJson.String(item, "call_id") ?? "call",
                OpenAIResponsesLanguageModel.ProgrammaticToolName(_toolNameMapping),
                JsonSerializer.Deserialize<JsonElement>(OpenAIResponsesOutput.ProgramOutputResult(item)))
            {
                ProviderMetadata = OpenAIResponsesOutput.ItemMetadata(item),
            });
        }
        else if (itemType == "web_search_call")
        {
            parts.Add(new ToolResultStreamPart(
                OpenAIJson.String(item, "id") ?? string.Empty,
                "web_search",
                JsonSerializer.Deserialize<JsonElement>(OpenAIResponsesOutput.WebSearchResult(item))));
            foreach (var url in OpenAIResponsesOutput.WebSearchUrls(item))
            {
                _hasWebSearchActionSources = true;
                parts.Add(new SourceStreamPart(_generateId(), url, null));
            }
        }
        else if (itemType == "file_search_call")
        {
            parts.Add(new ToolResultStreamPart(
                OpenAIJson.String(item, "id") ?? string.Empty,
                "file_search",
                JsonSerializer.Deserialize<JsonElement>(OpenAIResponsesOutput.FileSearchResult(item))));
            foreach (var (fileId, filename) in OpenAIResponsesOutput.RetrievedFiles(item))
            {
                _hasFileSearchResults = true;
                if (_retrievedFileIds.Add(fileId))
                {
                    parts.Add(new DocumentSourceStreamPart(_generateId(), "text/plain", filename, filename, OpenAIResponsesOutput.FileSearchMetadata(fileId)));
                }
            }
        }
    }

    private static LanguageModelStreamPart ToSourcePart(GeneratedContent source) => source switch
    {
        GeneratedSource url => new SourceStreamPart(url.Id, url.Url, url.Title, url.ProviderMetadata),
        GeneratedDocumentSource document => new DocumentSourceStreamPart(document.Id, document.MediaType, document.Title, document.Filename, document.ProviderMetadata),
        _ => throw new InvalidOperationException("A source is a URL source or a document source."),
    };
}
