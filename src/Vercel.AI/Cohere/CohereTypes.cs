// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Cohere;

/// <summary>A warning produced while mapping a Cohere request.</summary>
public sealed class CohereWarning
{
    /// <summary>Creates a warning.</summary>
    public CohereWarning(string type, string? feature, string? details)
    {
        Type = type ?? string.Empty;
        Feature = feature;
        Details = details;
    }

    /// <summary>Warning category, such as <c>unsupported</c> or <c>compatibility</c>.</summary>
    public string Type { get; }

    /// <summary>Feature name.</summary>
    public string? Feature { get; }

    /// <summary>Extra detail.</summary>
    public string? Details { get; }
}

/// <summary>A user text part in a Cohere prompt.</summary>
public sealed class CohereTextPart : UserContentPart
{
    /// <summary>Creates a text part.</summary>
    public CohereTextPart(string text)
        : base("text")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>A file or image part in a Cohere prompt.</summary>
public sealed class CohereFilePart : UserContentPart
{
    /// <summary>Creates a file part.</summary>
    public CohereFilePart(string mediaType)
        : base("file")
    {
        MediaType = mediaType ?? string.Empty;
    }

    /// <summary>IANA media type. A bare <c>image</c> value is detected from the bytes.</summary>
    public string MediaType { get; }

    /// <summary>Inline bytes. Documents are decoded as UTF-8. Images are base64-encoded.</summary>
    public byte[]? Bytes { get; set; }

    /// <summary>Text payload. Documents use this string as-is.</summary>
    public string? Text { get; set; }

    /// <summary>String payload for the data form. Image strings are not encoded again.</summary>
    public string? StringData { get; set; }

    /// <summary>Remote URL for an image.</summary>
    public string? Url { get; set; }

    /// <summary>File name sent as the document title.</summary>
    public string? FileName { get; set; }

    /// <summary>Image detail forwarded as <c>image_url.detail</c>.</summary>
    public string? Detail { get; set; }

    /// <summary>When true, conversion throws because provider file references are unsupported.</summary>
    public bool ProviderReference { get; set; }
}

/// <summary>One tool result inside a Cohere tool message.</summary>
public sealed class CohereToolResultPart
{
    /// <summary>Creates a tool result.</summary>
    public CohereToolResultPart(string toolCallId, string kind)
    {
        ToolCallId = toolCallId ?? string.Empty;
        Kind = kind ?? "json";
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary><c>json</c>, <c>text</c>, <c>error-text</c>, or <c>execution-denied</c>.</summary>
    public string Kind { get; }

    /// <summary>Text or error text.</summary>
    public string? Text { get; set; }

    /// <summary>JSON value for <c>json</c> results.</summary>
    public JsonElement? Json { get; set; }

    /// <summary>Denial reason.</summary>
    public string? Reason { get; set; }
}

/// <summary>A tool message that can contain more than one result.</summary>
public sealed class CohereToolMessage : ModelMessage
{
    /// <summary>Creates a tool message.</summary>
    public CohereToolMessage(IReadOnlyList<CohereToolResultPart> results)
        : base("tool")
    {
        Results = results ?? Array.Empty<CohereToolResultPart>();
    }

    /// <summary>Tool results. Each one becomes its own Cohere tool message.</summary>
    public IReadOnlyList<CohereToolResultPart> Results { get; }
}

/// <summary>A function or provider-defined tool passed to <see cref="CohereChatMapping.PrepareTools"/>.</summary>
public sealed class CohereToolDefinition
{
    /// <summary>Creates a function tool.</summary>
    public static CohereToolDefinition Function(string name, string? description, JsonElement parameters)
    {
        return new CohereToolDefinition("function", name, description, parameters, null);
    }

    /// <summary>Creates a provider-defined tool.</summary>
    public static CohereToolDefinition Provider(string id, string name)
    {
        return new CohereToolDefinition("provider", name, null, default, id);
    }

    private CohereToolDefinition(string kind, string? name, string? description, JsonElement parameters, string? providerId)
    {
        Kind = kind;
        Name = name;
        Description = description;
        Parameters = parameters;
        ProviderId = providerId;
    }

    /// <summary><c>function</c> or <c>provider</c>.</summary>
    public string Kind { get; }

    /// <summary>Tool name.</summary>
    public string? Name { get; }

    /// <summary>Function description.</summary>
    public string? Description { get; }

    /// <summary>JSON schema.</summary>
    public JsonElement Parameters { get; }

    /// <summary>Provider tool id.</summary>
    public string? ProviderId { get; }
}

/// <summary>Messages and documents produced for the Cohere chat API.</summary>
public sealed class CoherePromptConversion
{
    /// <summary>Creates a conversion result.</summary>
    public CoherePromptConversion(string messagesJson, string documentsJson, IReadOnlyList<CohereWarning> warnings)
    {
        MessagesJson = messagesJson ?? "[]";
        DocumentsJson = documentsJson ?? "[]";
        Warnings = warnings ?? Array.Empty<CohereWarning>();
    }

    /// <summary>JSON array of chat messages.</summary>
    public string MessagesJson { get; }

    /// <summary>JSON array of documents.</summary>
    public string DocumentsJson { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<CohereWarning> Warnings { get; }
}

/// <summary>Tools and tool choice prepared for Cohere.</summary>
public sealed class CoherePreparedTools
{
    /// <summary>Creates a prepared tool set.</summary>
    public CoherePreparedTools(string? toolsJson, string? toolChoice, IReadOnlyList<CohereWarning> warnings)
    {
        ToolsJson = toolsJson;
        ToolChoice = toolChoice;
        Warnings = warnings ?? Array.Empty<CohereWarning>();
    }

    /// <summary>JSON array of tools. Null when no tools were provided.</summary>
    public string? ToolsJson { get; }

    /// <summary><c>NONE</c>, <c>REQUIRED</c>, or null for automatic.</summary>
    public string? ToolChoice { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<CohereWarning> Warnings { get; }
}

/// <summary>A citation source returned by Cohere.</summary>
public sealed class CohereCitation : GeneratedContent
{
    /// <summary>Creates a citation.</summary>
    public CohereCitation(string id, string title, JsonElement? providerMetadata)
        : base("source")
    {
        Id = id ?? string.Empty;
        Title = title ?? "Document";
        MediaType = "text/plain";
        SourceType = "document";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Citation id.</summary>
    public string Id { get; }

    /// <summary>Document title.</summary>
    public string Title { get; }

    /// <summary>Always <c>text/plain</c>.</summary>
    public string MediaType { get; }

    /// <summary>Always <c>document</c>.</summary>
    public string SourceType { get; }

    /// <summary>Start, end, text, sources, and citation type.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Start of a streamed tool input.</summary>
public sealed class CohereToolInputStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-start part.</summary>
    public CohereToolInputStartStreamPart(string id, string toolName)
        : base("tool-input-start")
    {
        Id = id ?? string.Empty;
        ToolName = toolName ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }
}

/// <summary>A streamed tool argument fragment.</summary>
public sealed class CohereToolInputDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-delta part.</summary>
    public CohereToolInputDeltaStreamPart(string id, string delta)
        : base("tool-input-delta")
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Argument fragment.</summary>
    public string Delta { get; }
}

/// <summary>End of a streamed tool input.</summary>
public sealed class CohereToolInputEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input-end part.</summary>
    public CohereToolInputEndStreamPart(string id)
        : base("tool-input-end")
    {
        Id = id ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }
}

/// <summary>Embedding call settings.</summary>
public sealed class CohereEmbedRequest
{
    /// <summary>Creates an embedding request.</summary>
    public CohereEmbedRequest(IReadOnlyList<string> values)
    {
        Values = values ?? Array.Empty<string>();
    }

    /// <summary>Texts to embed.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Cohere input type. Null uses <c>search_query</c>.</summary>
    public string? InputType { get; set; }

    /// <summary>Output dimension.</summary>
    public int? OutputDimension { get; set; }

    /// <summary>Truncate mode.</summary>
    public string? Truncate { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>Embedding result including the raw response.</summary>
public sealed class CohereEmbeddingResult
{
    /// <summary>Creates an embedding result.</summary>
    public CohereEmbeddingResult(IReadOnlyList<float[]> embeddings, int? tokens, string rawBody, IReadOnlyDictionary<string, string> headers)
    {
        Embeddings = embeddings ?? Array.Empty<float[]>();
        Tokens = tokens;
        RawBody = rawBody ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Warnings = Array.Empty<CohereWarning>();
    }

    /// <summary>Float vectors.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Billed input tokens.</summary>
    public int? Tokens { get; }

    /// <summary>Raw response body.</summary>
    public string RawBody { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Warnings. Embedding calls do not emit any.</summary>
    public IReadOnlyList<CohereWarning> Warnings { get; }
}

/// <summary>One document in a rerank request. Set <see cref="Json"/> for an object document.</summary>
public sealed class CohereRerankDocument
{
    /// <summary>Creates a text document.</summary>
    public static CohereRerankDocument Text(string text)
    {
        return new CohereRerankDocument(text, null);
    }

    /// <summary>Creates an object document. It is sent as JSON text.</summary>
    public static CohereRerankDocument Object(JsonElement json)
    {
        return new CohereRerankDocument(null, json);
    }

    private CohereRerankDocument(string? text, JsonElement? json)
    {
        PlainText = text;
        Json = json;
    }

    /// <summary>Text document.</summary>
    public string? PlainText { get; }

    /// <summary>Object document.</summary>
    public JsonElement? Json { get; }

    /// <summary>True when this document was an object.</summary>
    public bool IsObject => Json != null;
}

/// <summary>Rerank call settings.</summary>
public sealed class CohereRerankRequest
{
    /// <summary>Creates a rerank request.</summary>
    public CohereRerankRequest(string query, IReadOnlyList<CohereRerankDocument> documents)
    {
        Query = query ?? string.Empty;
        Documents = documents ?? Array.Empty<CohereRerankDocument>();
    }

    /// <summary>Query.</summary>
    public string Query { get; }

    /// <summary>Documents.</summary>
    public IReadOnlyList<CohereRerankDocument> Documents { get; }

    /// <summary>Maximum results.</summary>
    public int? TopN { get; set; }

    /// <summary>Maximum tokens per document.</summary>
    public int? MaxTokensPerDoc { get; set; }

    /// <summary>Priority.</summary>
    public int? Priority { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>Rerank result with warnings and the raw response.</summary>
public sealed class CohereRerankResult
{
    /// <summary>Creates a rerank result.</summary>
    public CohereRerankResult(
        IReadOnlyList<RerankItem> ranking,
        IReadOnlyList<CohereWarning> warnings,
        string? id,
        string rawBody,
        IReadOnlyDictionary<string, string> headers)
    {
        Ranking = ranking ?? Array.Empty<RerankItem>();
        Warnings = warnings ?? Array.Empty<CohereWarning>();
        Id = id;
        RawBody = rawBody ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Ranked documents.</summary>
    public IReadOnlyList<RerankItem> Ranking { get; }

    /// <summary>Warnings. Object documents produce one compatibility warning.</summary>
    public IReadOnlyList<CohereWarning> Warnings { get; }

    /// <summary>Response id. There is no separate provider metadata object.</summary>
    public string? Id { get; }

    /// <summary>Always null. The raw body carries provider data.</summary>
    public JsonElement? ProviderMetadata => null;

    /// <summary>Raw response body.</summary>
    public string RawBody { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>A chat stream that exposes HTTP headers before the first part.</summary>
public sealed class CohereStreamResponse
{
    /// <summary>Creates a stream response.</summary>
    public CohereStreamResponse(IReadOnlyDictionary<string, string> headers, IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        Headers = headers ?? new Dictionary<string, string>();
        Parts = parts;
    }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Stream parts.</summary>
    public IAsyncEnumerable<LanguageModelStreamPart> Parts { get; }
}
