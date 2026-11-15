// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>
/// Citation payload Cohere returns for text content, in non-streaming responses and in
/// <c>citation-start</c> stream events. Maps <c>cohereCitationSchema</c>.
/// </summary>
/// <remarks>
/// Every field is optional in the API specification, and a source can reference either a document or tool
/// output, so partial citations are accepted instead of discarding the whole response.
/// </remarks>
internal sealed class CohereCitation
{
    private CohereCitation(double? start, double? end, string? text, IReadOnlyList<CohereCitationSource>? sources, string? type)
    {
        Start = start;
        End = end;
        Text = text;
        Sources = sources;
        Type = type;
    }

    public double? Start { get; }

    public double? End { get; }

    public string? Text { get; }

    public IReadOnlyList<CohereCitationSource>? Sources { get; }

    public string? Type { get; }

    /// <summary>Validates a citation object. Throws <see cref="TypeValidationException"/> when a field has the wrong type.</summary>
    public static CohereCitation Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("Cohere citation must be an object.", value);
        }

        List<CohereCitationSource>? sources = null;
        if (value.TryGetProperty("sources", out var sourcesElement) && sourcesElement.ValueKind != JsonValueKind.Null)
        {
            if (sourcesElement.ValueKind != JsonValueKind.Array)
            {
                throw Invalid("Cohere citation sources must be an array.", value);
            }

            sources = new List<CohereCitationSource>();
            foreach (var source in sourcesElement.EnumerateArray())
            {
                sources.Add(CohereCitationSource.Parse(source));
            }
        }

        return new CohereCitation(
            Number(value, "start"),
            Number(value, "end"),
            StringField(value, "text"),
            sources,
            StringField(value, "type"));
    }

    /// <summary>Validates the citations of a <c>citation-start</c> event: a single citation or an array of them.</summary>
    public static IReadOnlyList<CohereCitation> ParseMany(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            var citations = new List<CohereCitation>();
            foreach (var citation in value.EnumerateArray())
            {
                citations.Add(Parse(citation));
            }

            return citations;
        }

        return new[] { Parse(value) };
    }

    /// <summary>
    /// Maps a Cohere citation to a document source. The first source with a document provides the title, and
    /// all sources are kept in the provider metadata. Citations that only reference tool output have no
    /// document to point at and return null, because their data stays in the response body.
    /// </summary>
    public GeneratedDocumentSource? ToSource(Func<string> generateId)
    {
        var document = Sources?.FirstOrDefault(source => source.Document != null)?.Document;
        if (document == null)
        {
            return null;
        }

        var cohere = new JsonObject();
        if (Start is { } start)
        {
            cohere["start"] = start;
        }

        if (End is { } end)
        {
            cohere["end"] = end;
        }

        if (Text != null)
        {
            cohere["text"] = Text;
        }

        if (Sources != null)
        {
            var sources = new JsonArray();
            foreach (var source in Sources)
            {
                sources.Add(source.ToJson());
            }

            cohere["sources"] = sources;
        }

        if (Type != null)
        {
            cohere["citationType"] = Type;
        }

        var metadata = new JsonObject { [CohereProvider.ProviderName] = cohere };
        var title = string.IsNullOrEmpty(document.Value.Title) ? "Document" : document.Value.Title!;
        return new GeneratedDocumentSource(generateId(), "text/plain", title, providerMetadata: JsonSerializer.SerializeToElement(metadata));
    }

    internal static string? StringField(JsonElement owner, string name)
    {
        if (!owner.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : throw Invalid("Cohere citation " + name + " must be a string.", owner);
    }

    internal static TypeValidationException Invalid(string message, JsonElement value)
    {
        return new TypeValidationException(message, value.Clone());
    }

    private static double? Number(JsonElement owner, string name)
    {
        if (!owner.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : throw Invalid("Cohere citation " + name + " must be a number.", owner);
    }
}

/// <summary>A source of a Cohere citation: a document or tool output. Unknown source fields are dropped.</summary>
internal sealed class CohereCitationSource
{
    private readonly JsonElement _json;

    private CohereCitationSource(string? type, string? id, CohereCitationDocument? document, JsonElement json)
    {
        Type = type;
        Id = id;
        Document = document;
        _json = json;
    }

    public string? Type { get; }

    public string? Id { get; }

    public CohereCitationDocument? Document { get; }

    public static CohereCitationSource Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw CohereCitation.Invalid("Cohere citation source must be an object.", value);
        }

        CohereCitationDocument? document = null;
        if (value.TryGetProperty("document", out var documentElement) && documentElement.ValueKind != JsonValueKind.Null)
        {
            document = CohereCitationDocument.Parse(documentElement);
        }

        if (value.TryGetProperty("tool_output", out var toolOutput) && toolOutput.ValueKind is not (JsonValueKind.Null or JsonValueKind.Object))
        {
            throw CohereCitation.Invalid("Cohere citation tool_output must be an object.", value);
        }

        return new CohereCitationSource(CohereCitation.StringField(value, "type"), CohereCitation.StringField(value, "id"), document, value);
    }

    /// <summary>The typed fields as they were sent, without unknown keys. Absent keys stay absent.</summary>
    public JsonNode ToJson()
    {
        var result = new JsonObject();
        foreach (var name in new[] { "type", "id", "document", "tool_output" })
        {
            if (_json.TryGetProperty(name, out var value))
            {
                result[name] = JsonNode.Parse(value.GetRawText());
            }
        }

        return result;
    }
}

/// <summary>A cited document. Only the fields the provider maps are typed, the rest passes through as provider metadata.</summary>
internal readonly struct CohereCitationDocument
{
    private CohereCitationDocument(string? title)
    {
        Title = title;
    }

    public string? Title { get; }

    public static CohereCitationDocument Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw CohereCitation.Invalid("Cohere citation document must be an object.", value);
        }

        _ = CohereCitation.StringField(value, "id");
        _ = CohereCitation.StringField(value, "text");
        return new CohereCitationDocument(CohereCitation.StringField(value, "title"));
    }
}
