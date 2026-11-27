// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>A document of the prompt that citations point to. Maps to <c>CitationDocument</c>.</summary>
public sealed record CitationDocument(string Title, string? Filename, string MediaType);

/// <summary>Maps Anthropic text citations onto the shared <see cref="Citation"/> type.</summary>
internal static class AnthropicCitations
{
    /// <summary>
    /// Maps one Anthropic citation. Web and search-result locations cite a URL. Page, character, and content-block
    /// locations cite a document. The complete Anthropic citation stays on the source's provider metadata.
    /// </summary>
    public static Citation Map(
        JsonElement citation,
        IReadOnlyList<CitationDocument>? documents = null
    )
    {
        var type = AnthropicJson.String(citation, "type");
        var metadata = Metadata(citation);
        var citedText = AnthropicJson.String(citation, "cited_text");
        if (type == "web_search_result_location" || type == "search_result_location")
        {
            var url = (
                type == "web_search_result_location"
                    ? AnthropicJson.String(citation, "url")
                    : AnthropicJson.String(citation, "source")
            ) ?? string.Empty;
            return new Citation(
                new GeneratedSource(url, url, AnthropicJson.String(citation, "title"), metadata),
                citedText: citedText
            );
        }

        var index =
            citation.TryGetProperty("document_index", out var documentIndex)
            && documentIndex.ValueKind == JsonValueKind.Number
                ? documentIndex.GetInt32()
                : 0;
        var document =
            documents != null && index >= 0 && index < documents.Count ? documents[index] : null;
        var indexText = index.ToString(CultureInfo.InvariantCulture);
        return new Citation(
            new GeneratedDocumentSource(
                AnthropicJson.String(citation, "file_id") ?? indexText,
                document?.MediaType ?? "text/plain",
                AnthropicJson.String(citation, "document_title")
                    ?? document?.Title
                    ?? "Document " + indexText,
                document?.Filename,
                metadata
            ),
            citedText: citedText
        );
    }

    /// <summary>Maps every citation in <paramref name="citations"/>, or returns null when there are none.</summary>
    public static IReadOnlyList<Citation>? MapAll(
        IEnumerable<JsonElement> citations,
        IReadOnlyList<CitationDocument>? documents = null
    )
    {
        var mapped = new List<Citation>();
        foreach (var citation in citations)
        {
            mapped.Add(Map(citation, documents));
        }

        return mapped.Count == 0 ? null : mapped;
    }

    /// <summary>
    /// Creates the source part of a streamed citation. Web search locations cite a URL, page and character locations
    /// cite a document of the prompt, and other citations have no source part. Maps to <c>createCitationSource</c>.
    /// </summary>
    public static LanguageModelStreamPart? CreateSource(
        JsonElement citation,
        IReadOnlyList<CitationDocument> documents,
        Func<string> generateId
    )
    {
        var type = AnthropicJson.String(citation, "type");
        if (type == "web_search_result_location")
        {
            return new SourceStreamPart(
                generateId(),
                AnthropicJson.String(citation, "url") ?? string.Empty,
                AnthropicJson.String(citation, "title"),
                Wrap(
                    new JsonObject
                    {
                        ["citedText"] = AnthropicJson.String(citation, "cited_text"),
                        ["encryptedIndex"] = AnthropicJson.String(citation, "encrypted_index"),
                    }
                )
            );
        }

        if (type != "page_location" && type != "char_location")
        {
            return null;
        }

        var index =
            citation.TryGetProperty("document_index", out var documentIndex)
            && documentIndex.ValueKind == JsonValueKind.Number
                ? documentIndex.GetInt32()
                : -1;
        if (index < 0 || index >= documents.Count)
        {
            return null;
        }

        var document = documents[index];
        var metadata = new JsonObject
        {
            ["citedText"] = AnthropicJson.String(citation, "cited_text"),
        };
        if (type == "page_location")
        {
            metadata["startPageNumber"] = Number(citation, "start_page_number");
            metadata["endPageNumber"] = Number(citation, "end_page_number");
        }
        else
        {
            metadata["startCharIndex"] = Number(citation, "start_char_index");
            metadata["endCharIndex"] = Number(citation, "end_char_index");
        }

        return new DocumentSourceStreamPart(
            generateId(),
            document.MediaType,
            AnthropicJson.String(citation, "document_title") ?? document.Title,
            document.Filename,
            Wrap(metadata)
        );
    }

    private static JsonNode? Number(JsonElement citation, string name)
    {
        return citation.TryGetProperty(name, out var value) ? AnthropicJson.Node(value) : null;
    }

    private static JsonElement Wrap(JsonObject metadata)
    {
        using var document = JsonDocument.Parse(
            new JsonObject { ["anthropic"] = metadata }.ToJsonString()
        );
        return document.RootElement.Clone();
    }

    /// <summary>Provider metadata with the raw citations under <c>anthropic.citations</c>.</summary>
    public static JsonElement TextMetadata(IEnumerable<JsonElement> citations)
    {
        var array = new JsonArray();
        foreach (var citation in citations)
        {
            array.Add(AnthropicJson.Node(citation));
        }

        using var document = JsonDocument.Parse(
            new JsonObject
            {
                ["anthropic"] = new JsonObject { ["citations"] = array },
            }.ToJsonString()
        );
        return document.RootElement.Clone();
    }

    private static JsonElement Metadata(JsonElement citation)
    {
        using var document = JsonDocument.Parse(
            new JsonObject { ["anthropic"] = AnthropicJson.Node(citation) }.ToJsonString()
        );
        return document.RootElement.Clone();
    }
}
