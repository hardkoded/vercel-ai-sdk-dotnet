// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Provider;

/// <summary>Writes citations in the JSON shape of the V4 specification and the UI message stream.</summary>
internal static class CitationJson
{
    public static JsonArray ToJson(IEnumerable<Citation> citations)
    {
        var array = new JsonArray();
        foreach (var citation in citations)
        {
            array.Add(ToJson(citation));
        }

        return array;
    }

    public static JsonObject ToJson(Citation citation)
    {
        var json = new JsonObject { ["source"] = SourceToJson(citation.Source) };
        if (citation.StartIndex is { } start)
        {
            json["startIndex"] = start;
        }

        if (citation.EndIndex is { } end)
        {
            json["endIndex"] = end;
        }

        if (citation.CitedText != null)
        {
            json["citedText"] = citation.CitedText;
        }

        return json;
    }

    public static List<Citation> FromJson(JsonArray array)
    {
        var citations = new List<Citation>();
        foreach (var node in array)
        {
            if (node is JsonObject json && FromJson(json) is { } citation)
            {
                citations.Add(citation);
            }
        }

        return citations;
    }

    public static Citation? FromJson(JsonObject json)
    {
        if (json["source"] is not JsonObject source)
        {
            return null;
        }

        var start = json["startIndex"] is JsonValue s && s.TryGetValue<int>(out var startIndex) ? startIndex : (int?)null;
        var end = json["endIndex"] is JsonValue e && e.TryGetValue<int>(out var endIndex) ? endIndex : (int?)null;
        var citedText = (string?)json["citedText"];
        JsonElement? metadata = source["providerMetadata"] is { } node
            ? JsonSerializer.SerializeToElement(node)
            : null;
        var id = (string?)source["id"] ?? string.Empty;

        if ((string?)source["sourceType"] == "document")
        {
            return new Citation(
                new GeneratedDocumentSource(
                    id,
                    (string?)source["mediaType"] ?? string.Empty,
                    (string?)source["title"] ?? string.Empty,
                    (string?)source["filename"],
                    metadata),
                start,
                end,
                citedText);
        }

        return new Citation(
            new GeneratedSource(id, (string?)source["url"] ?? string.Empty, (string?)source["title"], metadata),
            start,
            end,
            citedText);
    }

    private static JsonObject SourceToJson(GeneratedContent source)
    {
        JsonObject json;
        JsonElement? metadata;
        switch (source)
        {
            case GeneratedSource url:
                json = new JsonObject
                {
                    ["type"] = "source",
                    ["sourceType"] = "url",
                    ["id"] = url.Id,
                    ["url"] = url.Url,
                };
                if (url.Title != null)
                {
                    json["title"] = url.Title;
                }

                metadata = url.ProviderMetadata;
                break;
            case GeneratedDocumentSource document:
                json = new JsonObject
                {
                    ["type"] = "source",
                    ["sourceType"] = "document",
                    ["id"] = document.Id,
                    ["mediaType"] = document.MediaType,
                    ["title"] = document.Title,
                };
                if (document.Filename != null)
                {
                    json["filename"] = document.Filename;
                }

                metadata = document.ProviderMetadata;
                break;
            default:
                throw new ArgumentException("A citation source is a URL source or a document source.", nameof(source));
        }

        if (metadata is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } element)
        {
            json["providerMetadata"] = JsonNode.Parse(element.GetRawText());
        }

        return json;
    }
}
