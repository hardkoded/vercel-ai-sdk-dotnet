// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>
/// Maps Interactions annotations and built-in tool results onto source parts. The API reports grounding sources
/// inline as <c>text_annotation</c> deltas and in the results of built-in tools.
/// </summary>
internal static class GoogleInteractionsSources
{
    private static readonly (string Extension, string MediaType)[] KnownDocumentExtensions =
    {
        ("pdf", "application/pdf"),
        ("txt", "text/plain"),
        ("md", "text/markdown"),
        ("markdown", "text/markdown"),
        ("doc", "application/msword"),
        ("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
    };

    /// <summary>
    /// Maps one <c>url_citation</c>, <c>file_citation</c> or <c>place_citation</c> annotation onto a source.
    /// Returns null when the annotation lacks what a source needs, such as a URL citation without a <c>url</c>.
    /// </summary>
    internal static LanguageModelStreamPart? AnnotationToSource(JsonObject annotation, Func<string> generateId)
    {
        switch (GoogleInteractionsStreaming.String(annotation, "type"))
        {
            case "url_citation":
                return Url(annotation, "url", "title", generateId);
            case "file_citation":
                {
                    var fileName = GoogleInteractionsStreaming.String(annotation, "file_name");
                    return FileSource(GoogleInteractionsStreaming.String(annotation, "url") ?? GoogleInteractionsStreaming.String(annotation, "document_uri") ?? fileName, fileName, fileName, null, generateId);
                }
            case "place_citation":
                return Url(annotation, "url", "name", generateId);
            default:
                return null;
        }
    }

    /// <summary>Maps the annotations of one text block onto sources. A URL or file that repeats is returned once.</summary>
    internal static IReadOnlyList<LanguageModelStreamPart> AnnotationsToSources(JsonArray? annotations, Func<string> generateId)
    {
        var sources = new List<LanguageModelStreamPart>();
        if (annotations == null)
        {
            return sources;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in annotations)
        {
            if (node is JsonObject annotation && AnnotationToSource(annotation, generateId) is { } source && seen.Add(Key(source)))
            {
                sources.Add(source);
            }
        }

        return sources;
    }

    /// <summary>
    /// Maps a built-in tool result block (<c>type</c>, <c>call_id</c>, <c>result</c>) onto sources. A result that carries
    /// no sources, such as <c>code_execution_result</c>, maps to none.
    /// </summary>
    internal static IReadOnlyList<LanguageModelStreamPart> BuiltinToolResultToSources(JsonObject block, Func<string> generateId)
    {
        var sources = new List<LanguageModelStreamPart>();
        var entries = block["result"] as JsonArray;
        if (entries == null)
        {
            return sources;
        }

        switch (GoogleInteractionsStreaming.String(block, "type"))
        {
            case "url_context_result":
                foreach (var entry in entries.OfType<JsonObject>())
                {
                    var url = GoogleInteractionsStreaming.String(entry, "url");
                    var status = GoogleInteractionsStreaming.String(entry, "status");
                    if (!string.IsNullOrEmpty(url) && (status == null || status == "success"))
                    {
                        sources.Add(new SourceStreamPart(generateId(), url!, null));
                    }
                }

                break;
            case "google_search_result":
                foreach (var entry in entries.OfType<JsonObject>())
                {
                    if (Url(entry, "url", "title", generateId) is { } source)
                    {
                        sources.Add(source);
                    }
                }

                break;
            case "google_maps_result":
                foreach (var entry in entries.OfType<JsonObject>())
                {
                    foreach (var place in (entry["places"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                    {
                        if (Url(place, "url", "name", generateId) is { } source)
                        {
                            sources.Add(source);
                        }
                    }
                }

                break;
            case "file_search_result":
                foreach (var entry in entries.OfType<JsonObject>())
                {
                    var fileName = GoogleInteractionsStreaming.String(entry, "file_name");
                    var title = GoogleInteractionsStreaming.String(entry, "title");
                    var uri = GoogleInteractionsStreaming.String(entry, "url") ?? GoogleInteractionsStreaming.String(entry, "document_uri") ?? fileName;
                    if (FileSource(uri, fileName, title, title, generateId) is { } source)
                    {
                        sources.Add(source);
                    }
                }

                break;
        }

        return sources;
    }

    /// <summary>The key that identifies a source across a stream: its URL, or the file name of a document.</summary>
    internal static string Key(LanguageModelStreamPart source)
    {
        return source switch
        {
            SourceStreamPart url => "url:" + url.Url,
            DocumentSourceStreamPart document => "doc:" + (document.Filename ?? document.Title),
            _ => throw new ArgumentException("Not a source part.", nameof(source)),
        };
    }

    private static LanguageModelStreamPart? Url(JsonObject node, string urlName, string titleName, Func<string> generateId)
    {
        var url = GoogleInteractionsStreaming.String(node, urlName);
        return string.IsNullOrEmpty(url) ? null : new SourceStreamPart(generateId(), url!, GoogleInteractionsStreaming.String(node, titleName));
    }

    // A file reference that is an http(s) URL is a URL source. Any other reference is a document source.
    private static LanguageModelStreamPart? FileSource(string? uri, string? fileName, string? urlTitle, string? documentTitle, Func<string> generateId)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return null;
        }

        if (uri!.StartsWith("http://", StringComparison.Ordinal) || uri.StartsWith("https://", StringComparison.Ordinal))
        {
            return new SourceStreamPart(generateId(), uri, urlTitle);
        }

        var filename = fileName ?? Basename(uri);
        return new DocumentSourceStreamPart(generateId(), MediaType(uri), documentTitle ?? fileName ?? filename ?? uri, filename);
    }

    private static string MediaType(string uriOrName)
    {
        foreach (var (extension, mediaType) in KnownDocumentExtensions)
        {
            if (uriOrName.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase))
            {
                return mediaType;
            }
        }

        return "application/octet-stream";
    }

    private static string? Basename(string uriOrName)
    {
        var last = uriOrName.Split('/').Last();
        return last.Length > 0 ? last : null;
    }
}
