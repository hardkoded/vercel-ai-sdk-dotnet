// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Generated text that carries the Responses output item id and its annotations.</summary>
public sealed class OpenAIText : GeneratedText
{
    /// <summary>Creates text.</summary>
    public OpenAIText(string text, IReadOnlyList<Citation>? citations, JsonElement providerMetadata)
        : base(text, citations)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata: <c>openai.itemId</c> and, when the text is annotated, <c>openai.annotations</c>.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>
/// Maps Responses annotations, web-search sources, and file-search results. Retrieved references are sources.
/// Annotations are the citations that hang off the text they support.
/// </summary>
internal static class OpenAIResponsesOutput
{
    /// <summary>The source an annotation points at: a URL source for <c>url_citation</c>, else a document source.</summary>
    public static GeneratedContent AnnotationSource(JsonElement annotation, string id)
    {
        var type = OpenAIJson.String(annotation, "type");
        if (type == "url_citation")
        {
            return new GeneratedSource(id, OpenAIJson.String(annotation, "url") ?? string.Empty, OpenAIJson.String(annotation, "title"));
        }

        var fileId = OpenAIJson.String(annotation, "file_id") ?? string.Empty;
        var filename = type == "file_path" ? fileId : OpenAIJson.String(annotation, "filename") ?? string.Empty;
        var metadata = new JsonObject { ["type"] = type, ["fileId"] = fileId };
        if (type == "container_file_citation")
        {
            metadata["containerId"] = OpenAIJson.String(annotation, "container_id");
        }
        else if (annotation.TryGetProperty("index", out var index) && index.ValueKind == JsonValueKind.Number)
        {
            metadata["index"] = JsonNode.Parse(index.GetRawText());
        }

        return new GeneratedDocumentSource(
            id,
            type == "file_path" ? "application/octet-stream" : "text/plain",
            filename,
            filename,
            OpenAIJson.ProviderMetadata(metadata));
    }

    /// <summary>The citations for a text block. A <c>file_path</c> annotation is a generated file, not a citation.</summary>
    public static IReadOnlyList<Citation>? Citations(IEnumerable<JsonElement> annotations)
    {
        var citations = new List<Citation>();
        foreach (var annotation in annotations)
        {
            var type = OpenAIJson.String(annotation, "type");
            if (type == "file_path")
            {
                continue;
            }

            var id = type == "url_citation" ? OpenAIJson.String(annotation, "url") : OpenAIJson.String(annotation, "file_id");
            var source = AnnotationSource(annotation, id ?? string.Empty);
            var start = Index(annotation, "start_index");
            var end = start == null ? null : Index(annotation, "end_index");
            citations.Add(source switch
            {
                GeneratedSource url => new Citation(url, start, end),
                _ => new Citation((GeneratedDocumentSource)source, start, end),
            });
        }

        return citations.Count == 0 ? null : citations;
    }

    /// <summary>Provider metadata for a text block.</summary>
    public static JsonElement TextMetadata(string? itemId, IReadOnlyList<JsonElement> annotations)
    {
        var metadata = new JsonObject { ["itemId"] = itemId };
        if (annotations.Count > 0)
        {
            var array = new JsonArray();
            foreach (var annotation in annotations)
            {
                array.Add(JsonNode.Parse(annotation.GetRawText()));
            }

            metadata["annotations"] = array;
        }

        return OpenAIJson.ProviderMetadata(metadata);
    }

    /// <summary>The URL sources of a <c>web_search_call</c> search action.</summary>
    public static List<string> WebSearchUrls(JsonElement item)
    {
        var urls = new List<string>();
        if (item.TryGetProperty("action", out var action)
            && action.ValueKind == JsonValueKind.Object
            && OpenAIJson.String(action, "type") == "search"
            && action.TryGetProperty("sources", out var sources)
            && sources.ValueKind == JsonValueKind.Array)
        {
            foreach (var source in sources.EnumerateArray())
            {
                if (OpenAIJson.String(source, "type") == "url" && OpenAIJson.String(source, "url") is { } url)
                {
                    urls.Add(url);
                }
            }
        }

        return urls;
    }

    /// <summary>The result a <c>web_search_call</c> reports.</summary>
    public static string WebSearchResult(JsonElement item)
    {
        if (!item.TryGetProperty("action", out var action) || action.ValueKind != JsonValueKind.Object)
        {
            return "{}";
        }

        var result = new JsonObject();
        switch (OpenAIJson.String(action, "type"))
        {
            case "search":
                var search = new JsonObject { ["type"] = "search" };
                if (OpenAIJson.String(action, "query") is { } query)
                {
                    search["query"] = query;
                }

                if (action.TryGetProperty("queries", out var queries) && queries.ValueKind == JsonValueKind.Array)
                {
                    search["queries"] = JsonNode.Parse(queries.GetRawText());
                }

                result["action"] = search;
                if (action.TryGetProperty("sources", out var sources) && sources.ValueKind == JsonValueKind.Array)
                {
                    result["sources"] = JsonNode.Parse(sources.GetRawText());
                }

                break;
            case "open_page":
                result["action"] = new JsonObject { ["type"] = "openPage", ["url"] = OpenAIJson.String(action, "url") };
                break;
            case "find_in_page":
                result["action"] = new JsonObject
                {
                    ["type"] = "findInPage",
                    ["url"] = OpenAIJson.String(action, "url"),
                    ["pattern"] = OpenAIJson.String(action, "pattern"),
                };
                break;
        }

        return result.ToJsonString();
    }

    /// <summary>The result a <c>file_search_call</c> reports.</summary>
    public static string FileSearchResult(JsonElement item)
    {
        var queries = item.TryGetProperty("queries", out var queryArray) && queryArray.ValueKind == JsonValueKind.Array
            ? JsonNode.Parse(queryArray.GetRawText())
            : new JsonArray();
        JsonNode? results = null;
        if (item.TryGetProperty("results", out var resultArray) && resultArray.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var result in resultArray.EnumerateArray())
            {
                mapped.Add(new JsonObject
                {
                    ["attributes"] = result.TryGetProperty("attributes", out var attributes) ? JsonNode.Parse(attributes.GetRawText()) : new JsonObject(),
                    ["fileId"] = OpenAIJson.String(result, "file_id"),
                    ["filename"] = OpenAIJson.String(result, "filename"),
                    ["score"] = result.TryGetProperty("score", out var score) ? JsonNode.Parse(score.GetRawText()) : null,
                    ["text"] = OpenAIJson.String(result, "text"),
                });
            }

            results = mapped;
        }

        return new JsonObject { ["queries"] = queries, ["results"] = results }.ToJsonString();
    }

    /// <summary>The retrieved files of a <c>file_search_call</c>, one per file id.</summary>
    public static IEnumerable<(string FileId, string Filename)> RetrievedFiles(JsonElement item)
    {
        if (item.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var result in results.EnumerateArray())
            {
                yield return (OpenAIJson.String(result, "file_id") ?? string.Empty, OpenAIJson.String(result, "filename") ?? string.Empty);
            }
        }
    }

    /// <summary>Provider metadata of a retrieved file source.</summary>
    public static JsonElement FileSearchMetadata(string fileId) =>
        OpenAIJson.ProviderMetadata(new JsonObject { ["type"] = "file_search", ["fileId"] = fileId });

    public static bool HasResults(JsonElement item) =>
        item.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0;

    private static int? Index(JsonElement annotation, string name) =>
        annotation.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;
}
