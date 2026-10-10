// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Maps Anthropic text citations onto the shared <see cref="Citation"/> type.</summary>
internal static class AnthropicCitations
{
    /// <summary>
    /// Maps one Anthropic citation. Web and search-result locations cite a URL. Page, character, and content-block
    /// locations cite a document. The complete Anthropic citation stays on the source's provider metadata.
    /// </summary>
    public static Citation Map(JsonElement citation)
    {
        var type = AnthropicJson.String(citation, "type");
        var metadata = Metadata(citation);
        var citedText = AnthropicJson.String(citation, "cited_text");
        if (type == "web_search_result_location" || type == "search_result_location")
        {
            var url = (type == "web_search_result_location" ? AnthropicJson.String(citation, "url") : AnthropicJson.String(citation, "source")) ?? string.Empty;
            return new Citation(
                new GeneratedSource(url, url, AnthropicJson.String(citation, "title"), metadata),
                citedText: citedText);
        }

        var index = citation.TryGetProperty("document_index", out var documentIndex) && documentIndex.ValueKind == JsonValueKind.Number
            ? documentIndex.GetInt32().ToString(CultureInfo.InvariantCulture)
            : "0";
        return new Citation(
            new GeneratedDocumentSource(
                AnthropicJson.String(citation, "file_id") ?? index,
                "text/plain",
                AnthropicJson.String(citation, "document_title") ?? "Document " + index,
                null,
                metadata),
            citedText: citedText);
    }

    /// <summary>Maps every citation in <paramref name="citations"/>, or returns null when there are none.</summary>
    public static IReadOnlyList<Citation>? MapAll(IEnumerable<JsonElement> citations)
    {
        var mapped = new List<Citation>();
        foreach (var citation in citations)
        {
            mapped.Add(Map(citation));
        }

        return mapped.Count == 0 ? null : mapped;
    }

    /// <summary>Provider metadata with the raw citations under <c>anthropic.citations</c>.</summary>
    public static JsonElement TextMetadata(IEnumerable<JsonElement> citations)
    {
        var array = new JsonArray();
        foreach (var citation in citations)
        {
            array.Add(AnthropicJson.Node(citation));
        }

        using var document = JsonDocument.Parse(new JsonObject { ["anthropic"] = new JsonObject { ["citations"] = array } }.ToJsonString());
        return document.RootElement.Clone();
    }

    private static JsonElement Metadata(JsonElement citation)
    {
        using var document = JsonDocument.Parse(new JsonObject { ["anthropic"] = AnthropicJson.Node(citation) }.ToJsonString());
        return document.RootElement.Clone();
    }
}
