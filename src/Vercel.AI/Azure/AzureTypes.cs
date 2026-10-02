// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>
/// Image or PDF bytes addressed by a string. An <c>assistant-</c> prefix is sent as a file id.
/// Other image strings are placed in a data URI without being encoded again.
/// </summary>
public sealed class AzureInputFile : UserContentPart
{
    /// <summary>Creates a file part whose data is already a string.</summary>
    public AzureInputFile(string mediaType, string data)
        : base("file")
    {
        MediaType = mediaType ?? string.Empty;
        Data = data ?? string.Empty;
    }

    /// <summary>IANA media type, such as <c>image/jpeg</c> or <c>application/pdf</c>.</summary>
    public string MediaType { get; }

    /// <summary>File id or raw payload. Strings are not base64-encoded again.</summary>
    public string Data { get; }
}

/// <summary>A provider-defined tool such as file search or code interpreter.</summary>
public sealed class AzureProviderTool
{
    /// <summary>Creates a provider tool.</summary>
    public AzureProviderTool(string id, string name, JsonElement arguments)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Arguments = arguments;
    }

    /// <summary>Provider tool id, such as <c>openai.file_search</c>.</summary>
    public string Id { get; }

    /// <summary>Tool name sent to the API.</summary>
    public string Name { get; }

    /// <summary>Tool arguments. Unknown properties are forwarded when the mapper knows the tool.</summary>
    public JsonElement Arguments { get; }
}

/// <summary>Reasoning text plus Azure item metadata.</summary>
public sealed class AzureGeneratedReasoning : GeneratedContent
{
    /// <summary>Creates reasoning content.</summary>
    public AzureGeneratedReasoning(string text, JsonElement? providerMetadata)
        : base("reasoning")
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Summary text. Empty when the provider sent no summary.</summary>
    public string Text { get; }

    /// <summary>Azure item id and encrypted reasoning content.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Assistant text that carries the Responses item id.</summary>
public sealed class AzureGeneratedText : GeneratedContent
{
    /// <summary>Creates text content.</summary>
    public AzureGeneratedText(string text, JsonElement? providerMetadata)
        : base("text")
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Generated text.</summary>
    public string Text { get; }

    /// <summary>Azure item id and annotations.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A provider-executed tool result from the Responses API.</summary>
public sealed class AzureToolResult : GeneratedContent
{
    /// <summary>Creates a tool result.</summary>
    public AzureToolResult(string toolCallId, string toolName, string resultJson)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ResultJson = resultJson ?? "{}";
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result object.</summary>
    public string ResultJson { get; }

    /// <summary>Provider-executed tools report <c>true</c>.</summary>
    public bool ProviderExecuted { get; set; } = true;
}

/// <summary>A document citation emitted while streaming Responses output.</summary>
public sealed class AzureDocumentSourceStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a document source.</summary>
    public AzureDocumentSourceStreamPart(string id, string filename, string title, JsonElement? providerMetadata)
        : base("source")
    {
        Id = id ?? string.Empty;
        Filename = filename ?? string.Empty;
        Title = title ?? string.Empty;
        MediaType = "text/plain";
        SourceType = "document";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Generated source id.</summary>
    public string Id { get; }

    /// <summary>Cited file name.</summary>
    public string Filename { get; }

    /// <summary>Document title.</summary>
    public string Title { get; }

    /// <summary>Always <c>text/plain</c> for file citations.</summary>
    public string MediaType { get; }

    /// <summary>Always <c>document</c>.</summary>
    public string SourceType { get; }

    /// <summary>File id, index, and citation type.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>End of a text block, including annotation metadata.</summary>
public sealed class AzureAnnotatedTextEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a text-end part.</summary>
    public AzureAnnotatedTextEndStreamPart(string id, JsonElement? providerMetadata)
        : base("text-end")
    {
        Id = id ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Text block id.</summary>
    public string Id { get; }

    /// <summary>Item id and annotations.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Image generation settings that include style.</summary>
public sealed class AzureImageCallOptions
{
    /// <summary>Creates image options.</summary>
    public AzureImageCallOptions(string prompt)
    {
        Prompt = prompt ?? string.Empty;
    }

    /// <summary>Image prompt.</summary>
    public string Prompt { get; }

    /// <summary>How many images to generate.</summary>
    public int Count { get; set; } = 1;

    /// <summary>Size string such as <c>1024x1024</c>.</summary>
    public string? Size { get; set; }

    /// <summary>Style such as <c>natural</c>. Provider option <c>openai.style</c> is used when this is empty.</summary>
    public string? Style { get; set; }

    /// <summary>Provider options. <c>openai.style</c> maps to the image <c>style</c> field.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>Images as the provider returned them, before a failed base64 decode is replaced.</summary>
public sealed class AzureImageResult
{
    /// <summary>Creates an image result.</summary>
    public AzureImageResult(IReadOnlyList<string> base64Images)
    {
        Base64Images = base64Images ?? Array.Empty<string>();
    }

    /// <summary><c>b64_json</c> values in response order.</summary>
    public IReadOnlyList<string> Base64Images { get; }
}

/// <summary>A Responses or chat stream that exposes HTTP headers before the first part.</summary>
public sealed class AzureStreamResponse
{
    /// <summary>Creates a stream response.</summary>
    public AzureStreamResponse(IReadOnlyDictionary<string, string> headers, IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        Headers = headers ?? new Dictionary<string, string>();
        Parts = parts ?? Empty();
    }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Stream parts.</summary>
    public IAsyncEnumerable<LanguageModelStreamPart> Parts { get; }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Empty()
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }
}
