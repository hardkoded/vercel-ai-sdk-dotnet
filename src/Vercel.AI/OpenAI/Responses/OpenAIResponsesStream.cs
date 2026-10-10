// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Turns Responses stream events into stream parts. Retrieved web and file sources are sources. Annotations become
/// the citations on the <c>text-end</c> of the message that carries them. Citation-derived sources are held back
/// until the stream ends, because the retrieval event can arrive after the annotation.
/// </summary>
internal sealed class OpenAIResponsesStream
{
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

    public OpenAIResponsesStream(Func<string>? generateId = null)
    {
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
                if (node["item"] is JsonObject added && added["type"]?.GetValue<string>() == "message")
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
            case "response.output_item.done":
                if (node["item"] is JsonObject done)
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
