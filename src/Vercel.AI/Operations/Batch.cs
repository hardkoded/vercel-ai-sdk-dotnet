// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Operations;

/// <summary>A tool definition forwarded to a batch without being executed.</summary>
public sealed class BatchTool
{
    /// <summary>Creates a tool definition.</summary>
    public BatchTool(
        string name,
        JsonElement inputSchema,
        string? description = null,
        Func<JsonElement, CancellationToken, Task<object?>>? execute = null
    )
    {
        Name = name ?? string.Empty;
        InputSchema = inputSchema;
        Description = description;
        Execute = execute;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>JSON schema.</summary>
    public JsonElement InputSchema { get; }

    /// <summary>Description.</summary>
    public string? Description { get; }

    /// <summary>Local execute function. Batch submission does not call it.</summary>
    public Func<JsonElement, CancellationToken, Task<object?>>? Execute { get; }
}

/// <summary>One request in a batch, discriminated by <see cref="Type"/>. Maps to <c>BatchRequest</c>.</summary>
public class BatchRequest
{
    /// <summary>Creates a request.</summary>
    public BatchRequest(string id, string type, string model)
    {
        Id = id ?? string.Empty;
        Type = type ?? string.Empty;
        Model = model ?? string.Empty;
    }

    /// <summary>Caller request id.</summary>
    public string Id { get; }

    /// <summary><c>text</c>, <c>image</c>, or an unsupported type.</summary>
    public string Type { get; }

    /// <summary>Model id.</summary>
    public string Model { get; }

    /// <summary>Text prompt. Cannot be combined with <see cref="Messages"/> on a text request.</summary>
    public string? Prompt { get; set; }

    /// <summary>Prompt messages. File parts with URLs the batch API supports stay URLs, the rest are downloaded.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>Instructions of a text request. They win over <see cref="System"/>.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>Legacy system string of a text request.</summary>
    public string? System { get; set; }

    /// <summary>Image prompt.</summary>
    public ImagePrompt? ImagePrompt { get; set; }

    /// <summary>Image count.</summary>
    public int? N { get; set; }

    /// <summary>Image size.</summary>
    public string? Size { get; set; }

    /// <summary>Image aspect ratio.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Per-request provider options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-p.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Reasoning effort.</summary>
    public string? Reasoning { get; set; }

    /// <summary>Tools. They are forwarded and not executed.</summary>
    public IReadOnlyList<BatchTool>? Tools { get; set; }

    /// <summary>Controls the order in which tools are sent to the provider. See <see cref="GenerateTextOptions.ToolOrder"/>. Maps to <c>toolOrder</c>.</summary>
    public IReadOnlyList<string>? ToolOrder { get; set; }

    /// <summary>Tool choice such as <c>required</c>.</summary>
    public string? ToolChoice { get; set; }
}

/// <summary>One text generation request within a batch. Maps to <c>TextBatchRequest</c>.</summary>
public sealed class TextBatchRequest : BatchRequest
{
    /// <summary>Creates a text request.</summary>
    public TextBatchRequest(string id, string model)
        : base(id, "text", model) { }
}

/// <summary>One image generation request within a batch. Maps to <c>ImageBatchRequest</c>.</summary>
public sealed class ImageBatchRequest : BatchRequest
{
    /// <summary>Creates an image request.</summary>
    public ImageBatchRequest(string id, string model)
        : base(id, "image", model) { }
}

/// <summary>Normalized request sent to <c>doStartBatch</c>. Maps to <c>BatchV4Request</c>.</summary>
public sealed class NormalizedBatchRequest
{
    /// <summary>Creates a normalized request.</summary>
    public NormalizedBatchRequest(string id, string type, string modelId, JsonElement options)
    {
        Id = id;
        Type = type;
        ModelId = modelId;
        Options = options;
    }

    /// <summary>Request id.</summary>
    public string Id { get; }

    /// <summary>Request type.</summary>
    public string Type { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Normalized options as JSON. <see cref="ToTextCallOptions"/> and <see cref="ToImageCallOptions"/> give the typed view.</summary>
    public JsonElement Options { get; }

    /// <summary>
    /// Typed view of <see cref="Options"/> for a text request. Upstream types <c>options</c> as a <c>Pick</c> of
    /// <c>LanguageModelV4CallOptions</c>: prompt, maxOutputTokens, temperature, stopSequences, topP, topK,
    /// presencePenalty, frequencyPenalty, seed, reasoning, responseFormat, toolChoice, tools, and providerOptions.
    /// Never carries headers, an abort signal, or raw chunks.
    /// </summary>
    /// <param name="providerName">Provider named in the error for an unsupported tool or prompt part.</param>
    public LanguageModelCallOptions ToTextCallOptions(string providerName = "OpenAI")
    {
        if (Type != "text")
        {
            throw new InvalidOperationException(
                "Request \"" + Id + "\" is a " + Type + " request, not a text request."
            );
        }

        return OpenAI.OpenAIBatchApi.ToCallOptions(Options, providerName);
    }

    /// <summary>
    /// Typed view of <see cref="Options"/> for an image request. Upstream types <c>options</c> as a <c>Pick</c> of
    /// <c>ImageModelV4CallOptions</c>: prompt, n, size, aspectRatio, seed, files, mask, and providerOptions.
    /// </summary>
    public ImageCallOptions ToImageCallOptions()
    {
        if (Type != "image")
        {
            throw new InvalidOperationException(
                "Request \"" + Id + "\" is a " + Type + " request, not an image request."
            );
        }

        var result = new ImageCallOptions(StringOf(Options, "prompt") ?? string.Empty);
        if (Options.TryGetProperty("n", out var n) && n.ValueKind == JsonValueKind.Number)
        {
            result.Count = n.GetInt32();
        }

        result.Size = StringOf(Options, "size");
        result.AspectRatio = StringOf(Options, "aspectRatio");
        if (Options.TryGetProperty("seed", out var seed) && seed.ValueKind == JsonValueKind.Number)
        {
            result.Seed = seed.GetInt32();
        }

        if (
            Options.TryGetProperty("files", out var files)
            && files.ValueKind == JsonValueKind.Array
        )
        {
            result.Files = files.EnumerateArray().Select(ToImageFile).ToList();
        }

        if (Options.TryGetProperty("mask", out var mask) && mask.ValueKind == JsonValueKind.Object)
        {
            result.Mask = ToImageFile(mask);
        }

        if (
            Options.TryGetProperty("providerOptions", out var providerOptions)
            && providerOptions.ValueKind == JsonValueKind.Object
        )
        {
            result.ProviderOptions = providerOptions
                .EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value.Clone());
        }

        return result;
    }

    private static string? StringOf(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static ImageModelFile ToImageFile(JsonElement file)
    {
        if (StringOf(file, "type") == "url")
        {
            return ImageModelFile.FromUrl(StringOf(file, "url") ?? string.Empty);
        }

        return ImageModelFile.FromBase64(
            StringOf(file, "base64") ?? StringOf(file, "data") ?? string.Empty,
            StringOf(file, "mediaType") ?? "application/octet-stream",
            StringOf(file, "fileName")
        );
    }
}

/// <summary>Provider token usage before it is reshaped.</summary>
public sealed class BatchModelUsage
{
    /// <summary>Creates provider usage.</summary>
    public BatchModelUsage(
        int? inputTotal = null,
        int? noCache = null,
        int? cacheRead = null,
        int? cacheWrite = null,
        int? outputTotal = null,
        int? text = null,
        int? reasoning = null,
        JsonElement? raw = null
    )
    {
        InputTotal = inputTotal;
        NoCache = noCache;
        CacheRead = cacheRead;
        CacheWrite = cacheWrite;
        OutputTotal = outputTotal;
        Text = text;
        Reasoning = reasoning;
        Raw = raw;
    }

    /// <summary>Input total.</summary>
    public int? InputTotal { get; }

    /// <summary>Uncached input tokens.</summary>
    public int? NoCache { get; }

    /// <summary>Cache-read tokens.</summary>
    public int? CacheRead { get; }

    /// <summary>Cache-write tokens.</summary>
    public int? CacheWrite { get; }

    /// <summary>Output total.</summary>
    public int? OutputTotal { get; }

    /// <summary>Text tokens.</summary>
    public int? Text { get; }

    /// <summary>Reasoning tokens.</summary>
    public int? Reasoning { get; }

    /// <summary>Raw usage.</summary>
    public JsonElement? Raw { get; }
}

/// <summary>Usage returned to batch callers.</summary>
public sealed class BatchUsage
{
    /// <summary>Creates caller usage.</summary>
    public BatchUsage(
        int? inputTokens,
        int? noCacheTokens,
        int? cacheReadTokens,
        int? cacheWriteTokens,
        int? outputTokens,
        int? textTokens,
        int? reasoningTokens,
        int? totalTokens,
        JsonElement? raw
    )
    {
        InputTokens = inputTokens;
        NoCacheTokens = noCacheTokens;
        CacheReadTokens = cacheReadTokens;
        CacheWriteTokens = cacheWriteTokens;
        OutputTokens = outputTokens;
        TextTokens = textTokens;
        ReasoningTokens = reasoningTokens;
        TotalTokens = totalTokens;
        Raw = raw;
    }

    /// <summary>Input tokens.</summary>
    public int? InputTokens { get; }

    /// <summary>Uncached input tokens.</summary>
    public int? NoCacheTokens { get; }

    /// <summary>Cache-read tokens.</summary>
    public int? CacheReadTokens { get; }

    /// <summary>Cache-write tokens.</summary>
    public int? CacheWriteTokens { get; }

    /// <summary>Output tokens.</summary>
    public int? OutputTokens { get; }

    /// <summary>Text tokens.</summary>
    public int? TextTokens { get; }

    /// <summary>Reasoning tokens.</summary>
    public int? ReasoningTokens { get; }

    /// <summary>Input plus output when both totals are known.</summary>
    public int? TotalTokens { get; }

    /// <summary>Raw usage.</summary>
    public JsonElement? Raw { get; }

    /// <summary>Maps provider usage the way <c>asLanguageModelUsage</c> does.</summary>
    public static BatchUsage FromModel(BatchModelUsage usage)
    {
        int? total =
            usage.InputTotal == null && usage.OutputTotal == null
                ? null
                : (usage.InputTotal ?? 0) + (usage.OutputTotal ?? 0);
        return new BatchUsage(
            usage.InputTotal,
            usage.NoCache,
            usage.CacheRead,
            usage.CacheWrite,
            usage.OutputTotal,
            usage.Text,
            usage.Reasoning,
            total,
            usage.Raw
        );
    }
}

/// <summary>One content part inside a batch text result.</summary>
public sealed class BatchContentPart
{
    /// <summary>Creates a part.</summary>
    public BatchContentPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Text.</summary>
    public string? Text { get; set; }

    /// <summary>Tool call id.</summary>
    public string? ToolCallId { get; set; }

    /// <summary>Tool name.</summary>
    public string? ToolName { get; set; }

    /// <summary>Parsed tool input.</summary>
    public JsonElement? Input { get; set; }

    /// <summary>Tool output.</summary>
    public JsonElement? Output { get; set; }

    /// <summary>True when the provider executed the tool.</summary>
    public bool? ProviderExecuted { get; set; }

    /// <summary>True for a dynamic tool.</summary>
    public bool? Dynamic { get; set; }

    /// <summary>True when a tool result is an error.</summary>
    public bool? IsError { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>File media type.</summary>
    public string? MediaType { get; set; }

    /// <summary>Downloaded file bytes.</summary>
    public byte[]? FileBytes { get; set; }

    /// <summary>File URL before download.</summary>
    public string? FileUrl { get; set; }

    /// <summary>Citations of a text part.</summary>
    public IReadOnlyList<Citation>? Citations { get; set; }

    /// <summary>Source id of a <c>source</c> part.</summary>
    public string? SourceId { get; set; }

    /// <summary><c>url</c> or <c>document</c> for a <c>source</c> part.</summary>
    public string? SourceType { get; set; }

    /// <summary>URL of a <c>url</c> source.</summary>
    public string? Url { get; set; }

    /// <summary>Title of a <c>source</c> part.</summary>
    public string? Title { get; set; }

    /// <summary>Kind of a <c>custom</c> part, such as <c>anthropic.fallback</c>.</summary>
    public string? Kind { get; set; }
}

/// <summary>A provider text generation inside a batch item.</summary>
public sealed class BatchTextGeneration
{
    /// <summary>Content parts.</summary>
    public IReadOnlyList<BatchContentPart> Content { get; set; } = Array.Empty<BatchContentPart>();

    /// <summary>Unified finish reason.</summary>
    public string? FinishReason { get; set; }

    /// <summary>Raw finish reason.</summary>
    public string? RawFinishReason { get; set; }

    /// <summary>Provider usage.</summary>
    public BatchModelUsage? Usage { get; set; }

    /// <summary>Response id.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Response timestamp.</summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>Response model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>The text a JSON output parses instead of the joined text parts. Maps to <c>structuredOutputText</c>.</summary>
    public string? StructuredOutputText { get; set; }
}

/// <summary>An error of a batch item. Maps to <c>BatchError</c>.</summary>
public sealed class BatchError
{
    /// <summary>Error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error type, for example the name of the error.</summary>
    public string? Type { get; set; }
}

/// <summary>A provider image generation inside a batch item.</summary>
public sealed class BatchImageGeneration
{
    /// <summary>Image payloads.</summary>
    public IReadOnlyList<object?> Images { get; set; } = Array.Empty<object?>();

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; set; } = Array.Empty<OperationWarning>();

    /// <summary>Response.</summary>
    public ProviderResponse? Response { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Usage.</summary>
    public OperationUsage? Usage { get; set; }
}

/// <summary>One raw batch item.</summary>
public sealed class BatchItem
{
    /// <summary>Creates an item.</summary>
    public BatchItem(string type, string id, string status)
    {
        Type = type;
        Id = id;
        Status = status;
    }

    /// <summary><c>text</c> or <c>image</c>.</summary>
    public string Type { get; }

    /// <summary>Request id.</summary>
    public string Id { get; }

    /// <summary><c>succeeded</c>, <c>failed</c>, <c>cancelled</c>, or <c>expired</c>.</summary>
    public string Status { get; }

    /// <summary>Text generation.</summary>
    public BatchTextGeneration? Text { get; set; }

    /// <summary>Image generation.</summary>
    public BatchImageGeneration? Image { get; set; }

    /// <summary>Error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Error code.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Error type reported by the provider.</summary>
    public string? ErrorType { get; set; }

    /// <summary>HTTP status code of a failed provider response.</summary>
    public int? ErrorStatusCode { get; set; }

    /// <summary>Provider metadata for a failed item.</summary>
    public JsonElement? ProviderMetadata { get; set; }
}

/// <summary>Normalized batch item.</summary>
public class BatchItemResult
{
    /// <summary>Item type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Request id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Text content as the generate-text content parts. Maps to <c>ContentPart</c>.</summary>
    public IReadOnlyList<GeneratedContent>? Content { get; set; }

    /// <summary>Joined text.</summary>
    public string? Text { get; set; }

    /// <summary>Finish reason.</summary>
    public string? FinishReason { get; set; }

    /// <summary>Raw finish reason.</summary>
    public string? RawFinishReason { get; set; }

    /// <summary>Text usage.</summary>
    public BatchUsage? Usage { get; set; }

    /// <summary>Image usage.</summary>
    public OperationUsage? ImageUsage { get; set; }

    /// <summary>Response id.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Response timestamp as an ISO 8601 string, so the result serializes.</summary>
    public string? Timestamp { get; set; }

    /// <summary>Response model id.</summary>
    public string? ResponseModelId { get; set; }

    /// <summary>Images.</summary>
    public IReadOnlyList<GeneratedImage>? Images { get; set; }

    /// <summary>Image warnings.</summary>
    public IReadOnlyList<OperationWarning>? Warnings { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Error code.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Error type reported by the provider.</summary>
    public string? ErrorType { get; set; }

    /// <summary>HTTP status code of a failed provider response.</summary>
    public int? ErrorStatusCode { get; set; }

    /// <summary>
    /// The parsed output of a successful text item. Only set when <c>BatchRequestOptions.Output</c> is passed to
    /// <c>GetBatchResults</c> and the text parsed. Maps to <c>output</c>.
    /// </summary>
    public JsonElement? Output { get; set; }

    /// <summary>
    /// Why no <see cref="Output"/> is available. A parsing failure does not fail the item: <see cref="Text"/> and
    /// <see cref="Usage"/> remain available. Maps to <c>outputError</c>.
    /// </summary>
    public BatchError? OutputError { get; set; }
}

/// <summary>
/// Normalized batch item whose parsed output has the type of the <see cref="OutputSpec{TOutput}"/> passed to
/// <c>GetBatchResults</c>. Maps to <c>BatchItemResult&lt;OUTPUT&gt;</c>.
/// </summary>
/// <typeparam name="TOutput">The type of the parsed output.</typeparam>
public sealed class BatchItemResult<TOutput> : BatchItemResult
{
    /// <summary>
    /// The parsed output of a successful text item. Not set when <see cref="BatchItemResult.OutputError"/> is set.
    /// Maps to <c>output</c>.
    /// </summary>
    public new TOutput? Output { get; set; }

    internal static BatchItemResult<TOutput> From(BatchItemResult item, OutputSpec<TOutput> output)
    {
        var typed = new BatchItemResult<TOutput>
        {
            Type = item.Type,
            Id = item.Id,
            Status = item.Status,
            Content = item.Content,
            Text = item.Text,
            FinishReason = item.FinishReason,
            RawFinishReason = item.RawFinishReason,
            Usage = item.Usage,
            ImageUsage = item.ImageUsage,
            ResponseId = item.ResponseId,
            Timestamp = item.Timestamp,
            ResponseModelId = item.ResponseModelId,
            Images = item.Images,
            Warnings = item.Warnings,
            ProviderMetadata = item.ProviderMetadata,
            ErrorMessage = item.ErrorMessage,
            ErrorCode = item.ErrorCode,
            ErrorType = item.ErrorType,
            ErrorStatusCode = item.ErrorStatusCode,
            OutputError = item.OutputError,
        };
        ((BatchItemResult)typed).Output = item.Output;
        if (item.Output is { } json)
        {
            try
            {
                typed.Output = output.Deserialize(json);
            }
            catch (Exception error)
                when (error is JsonException or NotSupportedException or InvalidOperationException)
            {
                typed.OutputError = new BatchError
                {
                    Message =
                        "No object generated: response did not deserialize to the output type.",
                    Type = "AI_NoObjectGeneratedError",
                };
            }
        }

        return typed;
    }
}

/// <summary>A version-2 batch reference. Upstream <c>BatchReference</c>.</summary>
public class BatchReference
{
    /// <summary>Creates an empty reference. <see cref="Batch"/> fills it in.</summary>
    public BatchReference() { }

    /// <summary>Creates a reference.</summary>
    public BatchReference(int version, string id, string provider)
    {
        Version = version;
        Id = id ?? string.Empty;
        Provider = provider ?? string.Empty;
    }

    /// <summary>Reference version. Only 2 is accepted.</summary>
    public int Version { get; set; } = 2;

    /// <summary>Batch id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Provider id.</summary>
    public string Provider { get; set; } = string.Empty;
}

/// <summary>Request counts.</summary>
public sealed class BatchRequestCounts
{
    /// <summary>Creates counts.</summary>
    public BatchRequestCounts(int total, int pending, int completed, int failed)
    {
        Total = total;
        Pending = pending;
        Completed = completed;
        Failed = failed;
    }

    /// <summary>Total requests.</summary>
    public int Total { get; }

    /// <summary>Pending requests.</summary>
    public int Pending { get; }

    /// <summary>Completed requests.</summary>
    public int Completed { get; }

    /// <summary>Failed requests.</summary>
    public int Failed { get; }
}

/// <summary>A warning tied to a request.</summary>
public sealed class BatchRequestWarning
{
    /// <summary>Creates a warning.</summary>
    public BatchRequestWarning(string? requestId, OperationWarning warning)
    {
        RequestId = requestId;
        Warning = warning;
    }

    /// <summary>Request id.</summary>
    public string? RequestId { get; }

    /// <summary>Warning.</summary>
    public OperationWarning Warning { get; }
}

/// <summary>
/// The latest status of a batch. It is also upstream's <c>Batch</c> (<c>BatchReference &amp; BatchStatus</c>), so the
/// start, list, and status results are all <see cref="BatchReference"/> values.
/// </summary>
public class BatchStatus : BatchReference
{
    /// <summary>Status name.</summary>
    public string? Status { get; set; }

    /// <summary>Provider status string.</summary>
    public string? RawStatus { get; set; }

    /// <summary>Request counts.</summary>
    public BatchRequestCounts? RequestCounts { get; set; }

    /// <summary>Creation time.</summary>
    public string? CreatedAt { get; set; }

    /// <summary>Expiry time.</summary>
    public string? ExpiresAt { get; set; }

    /// <summary>Message of the batch-level error.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Code of the batch-level error.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Type of the batch-level error.</summary>
    public string? ErrorType { get; set; }

    /// <summary>HTTP status code of the batch-level error.</summary>
    public int? ErrorStatusCode { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }
}

/// <summary>Start acknowledgement.</summary>
public sealed class BatchStartResult : BatchStatus
{
    /// <summary>Warnings.</summary>
    public IReadOnlyList<BatchRequestWarning> Warnings { get; set; } =
        Array.Empty<BatchRequestWarning>();
}

/// <summary>One listed batch.</summary>
public sealed class ListedBatch : BatchStatus { }

/// <summary>A page of batches.</summary>
public sealed class BatchListResult
{
    /// <summary>Creates a page.</summary>
    public BatchListResult(
        IReadOnlyList<ListedBatch> batches,
        string? nextCursor = null,
        JsonElement? providerMetadata = null
    )
    {
        Batches = batches ?? Array.Empty<ListedBatch>();
        NextCursor = nextCursor;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Batches.</summary>
    public IReadOnlyList<ListedBatch> Batches { get; }

    /// <summary>Next page cursor.</summary>
    public string? NextCursor { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Cancellation result.</summary>
public sealed class BatchCancelResult
{
    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>Status after cancellation.</summary>
    public string? Status { get; set; }
}

/// <summary>Arguments for a batch operation.</summary>
public sealed class BatchOperationCall
{
    /// <summary>Creates a call.</summary>
    public BatchOperationCall(
        string? batchId,
        JsonElement? providerOptions,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken,
        int? limit = null,
        string? cursor = null,
        string? webhookUrl = null,
        IReadOnlyList<NormalizedBatchRequest>? requests = null
    )
    {
        BatchId = batchId;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
        Limit = limit;
        Cursor = cursor;
        WebhookUrl = webhookUrl;
        Requests = requests;
    }

    /// <summary>Batch id.</summary>
    public string? BatchId { get; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Page size.</summary>
    public int? Limit { get; }

    /// <summary>Page cursor.</summary>
    public string? Cursor { get; }

    /// <summary>Webhook URL.</summary>
    public string? WebhookUrl { get; }

    /// <summary>Normalized requests.</summary>
    public IReadOnlyList<NormalizedBatchRequest>? Requests { get; }

    /// <summary>True when this call was made on the batch API instance.</summary>
    public object? Receiver { get; set; }
}

/// <summary>Batch API.</summary>
public interface IBatchApi
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Supported URL patterns (regular expression text) by media type for requests in this batch API.</summary>
    IReadOnlyDictionary<string, string> SupportedUrls { get; }

    /// <summary>
    /// Model IDs this batch API accepts for each modality. Maps to the <c>ModelIds</c> type argument of <c>BatchV4</c>.
    /// Null leaves every modality open.
    /// </summary>
    BatchModelIds? ModelIds { get; }

    /// <summary>True when cancellation is implemented.</summary>
    bool CanCancel { get; }

    /// <summary>True when listing is implemented.</summary>
    bool CanList { get; }

    /// <summary>Starts a batch.</summary>
    Task<BatchStartResult> DoStartAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    );

    /// <summary>Reads status.</summary>
    Task<BatchStatus> DoGetStatusAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Opens the result stream. Errors found while opening, such as a batch that is not complete, throw from the task.
    /// Errors found while reading, such as a malformed row, throw from the stream.
    /// </summary>
    Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    );

    /// <summary>Cancels a batch.</summary>
    Task<BatchCancelResult> DoCancelAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    );

    /// <summary>Lists batches. The implementation is invoked on this instance.</summary>
    Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken);
}

/// <summary>
/// Batch API whose accepted model IDs have a declared type. Maps to <c>BatchV4&lt;TModelIds&gt;</c>. Derive a type from
/// <see cref="BatchModelIds"/> to expose the closed model-id sets of a provider, and implement this interface to hand
/// callers that type. The runtime check of <see cref="Batch.StartBatchAsync"/> still applies to every request.
/// </summary>
/// <typeparam name="TModelIds">Model IDs accepted for each modality.</typeparam>
public interface IBatchApi<out TModelIds> : IBatchApi
    where TModelIds : BatchModelIds
{
    /// <summary>Typed model IDs this batch API accepts. The same instance as the untyped <see cref="IBatchApi.ModelIds"/>.</summary>
    new TModelIds? ModelIds { get; }
}

/// <summary>
/// Model IDs accepted by each batch modality. Maps to <c>BatchV4ModelIds</c>. C# has no string-literal union types, so a
/// batch API declares the closed set at runtime and <see cref="Batch.StartBatchAsync"/> checks every request against it.
/// A null list leaves that modality open.
/// </summary>
public class BatchModelIds
{
    /// <summary>Model IDs accepted by text requests. Null accepts any id.</summary>
    public IReadOnlyList<string>? Text { get; set; }

    /// <summary>Model IDs accepted by image requests. Null accepts any id.</summary>
    public IReadOnlyList<string>? Image { get; set; }
}

/// <summary>Provider that exposes <c>experimental_batch()</c>.</summary>
public interface IBatchProvider
{
    /// <summary>Returns the batch API, or null when batch is unsupported.</summary>
    IBatchApi? ExperimentalBatch();
}

/// <summary>Default provider used when a batch call omits one.</summary>
public static class BatchModels
{
    /// <summary>Resolves the global default provider.</summary>
    public static Func<IBatchProvider?>? Default { get; set; }
}

/// <summary>Options for batch operations.</summary>
public sealed class BatchRequestOptions : OperationRequest
{
    /// <summary>Batch API or a provider that exposes one. Null uses <see cref="BatchModels.Default"/>.</summary>
    public ApiOrProvider<IBatchApi, IBatchProvider>? Provider { get; set; }

    /// <summary>Batch reference.</summary>
    public BatchReference? Batch { get; set; }

    /// <summary>Requests to start.</summary>
    public IReadOnlyList<BatchRequest>? Requests { get; set; }

    /// <summary>Webhook URL.</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Page size.</summary>
    public int? Limit { get; set; }

    /// <summary>Page cursor.</summary>
    public string? Cursor { get; set; }

    /// <summary>Tools used to parse client tool calls. They are not executed.</summary>
    public IReadOnlyList<BatchTool>? Tools { get; set; }

    /// <summary>
    /// Specification for structured data in the text responses. Its response format is sent with every text request
    /// in the batch; image requests ignore it. Pass the same value to <c>GetBatchResults</c> to parse and
    /// validate each result.
    /// </summary>
    public OutputSpec? Output { get; set; }

    /// <summary>Total timeout in milliseconds.</summary>
    public int? TimeoutMs { get; set; }

    /// <summary>Downloads file URLs inside results.</summary>
    public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
}

/// <summary>
/// Options for <see cref="Batch.StartBatchAsync"/>. Upstream <c>StartBatchOptions</c>. Starting a batch never retries,
/// so there is no <c>MaxRetries</c>.
/// </summary>
public sealed class StartBatchOptions
{
    /// <summary>Batch API or a provider that exposes one. Null uses <see cref="BatchModels.Default"/>.</summary>
    public ApiOrProvider<IBatchApi, IBatchProvider>? Provider { get; set; }

    /// <summary>Requests to start.</summary>
    public IReadOnlyList<BatchRequest>? Requests { get; set; }

    /// <summary>Provider-specific options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>
    /// Specification for structured data in the text responses. Its response format is sent with every text request
    /// in the batch; image requests ignore it. Pass the same value to <c>GetBatchResults</c> to parse and
    /// validate each result.
    /// </summary>
    public OutputSpec? Output { get; set; }

    /// <summary>URL the provider notifies when the batch reaches a terminal state.</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Total timeout in milliseconds.</summary>
    public int? TimeoutMs { get; set; }

    /// <summary>Cancels the call.</summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Exception thrown when <see cref="CancellationToken"/> is already cancelled.</summary>
    public Exception? AbortReason { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    internal BatchRequestOptions ToRequestOptions()
    {
        return new BatchRequestOptions
        {
            Provider = Provider,
            Requests = Requests,
            ProviderOptions = ProviderOptions,
            Output = Output,
            WebhookUrl = WebhookUrl,
            TimeoutMs = TimeoutMs,
            CancellationToken = CancellationToken,
            AbortReason = AbortReason,
            Headers = Headers,
            MaxRetries = 0,
        };
    }
}

/// <summary>Batch operations. Maps to <c>startBatch</c>, <c>getBatchStatus</c>, <c>getBatchResults</c>, <c>listBatches</c>, and <c>cancelBatch</c>.</summary>
public static class Batch
{
    /// <summary>Requests cancellation.</summary>
    public static async Task<BatchCancelResult> CancelBatchAsync(
        BatchRequestOptions request,
        CancellationToken cancellationToken = default
    )
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        if (!api.CanCancel)
        {
            throw new UnsupportedFunctionalityException(
                "batch cancellation",
                "The provider does not support batch cancellation."
            );
        }

        using var scope = Token(request, cancellationToken);
        var token = scope.Token;
        return await api.DoCancelAsync(Call(api, request, token, request.Batch!.Id), token)
            .ConfigureAwait(false);
    }

    /// <summary>Lists one page. <c>doListBatches</c> is invoked on the batch API instance.</summary>
    public static async Task<BatchListResult> ListBatchesAsync(
        BatchRequestOptions request,
        CancellationToken cancellationToken = default
    )
    {
        request ??= new BatchRequestOptions();
        var api = Resolve(request);
        if (!api.CanList)
        {
            throw new UnsupportedFunctionalityException(
                "batch listing",
                "The provider does not support listing batches."
            );
        }

        using var scope = Token(request, cancellationToken);
        var token = scope.Token;
        var call = Call(api, request, token, null);
        call.Receiver = api;
        var page = await OperationRetry
            .ExecuteAsync(
                request.MaxRetries,
                token,
                request.AbortReason,
                ct => api.DoListAsync(call, ct),
                null
            )
            .ConfigureAwait(false);
        var batches = new List<ListedBatch>();
        foreach (var batch in page.Batches)
        {
            batch.Version = 2;
            batch.Provider = api.Provider;
            batches.Add(batch);
        }

        return new BatchListResult(batches, page.NextCursor, page.ProviderMetadata);
    }

    /// <summary>Validates and starts a batch.</summary>
    public static async Task<BatchStartResult> StartBatchAsync(
        StartBatchOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var request = options.ToRequestOptions();

        var requests = request.Requests ?? Array.Empty<BatchRequest>();
        ValidateRequests(requests);
        var api = Resolve(request);
        ValidateModelIds(api, requests);
        using var scope = Token(request, cancellationToken);
        var token = scope.Token;
        var normalized = new List<NormalizedBatchRequest>();
        var tools = new Dictionary<string, string>();
        foreach (var item in requests)
        {
            if (item.Type == "text")
            {
                var prepared = PrepareTools(item.Tools, item.ToolOrder);
                foreach (var tool in prepared)
                {
                    var signature = tool.GetRawText();
                    if (tools.TryGetValue(NameOf(tool), out var previous) && previous != signature)
                    {
                        throw new InvalidArgumentException(
                            "requests",
                            item.Id,
                            "tool \""
                                + NameOf(tool)
                                + "\" must have the same definition in every batch request"
                        );
                    }

                    tools[NameOf(tool)] = signature;
                }

                normalized.Add(
                    new NormalizedBatchRequest(
                        item.Id,
                        item.Type,
                        item.Model,
                        await TextOptionsAsync(item, prepared, request.Output, api, token)
                            .ConfigureAwait(false)
                    )
                );
            }
            else if (item.Type == "image")
            {
                var parts = GenerateImage.NormalizePrompt(item.ImagePrompt, item.Prompt);
                normalized.Add(
                    new NormalizedBatchRequest(
                        item.Id,
                        item.Type,
                        item.Model,
                        ImageOptions(item, parts)
                    )
                );
            }
            else
            {
                throw new InvalidArgumentException(
                    "requests",
                    item.Type,
                    "Unsupported batch request type \"" + item.Type + "\"."
                );
            }

            OperationRetry.ThrowIfAborted(token, request.AbortReason);
        }

        var started = await api.DoStartAsync(Call(api, request, token, null, normalized), token)
            .ConfigureAwait(false);
        var models = new Dictionary<string, string>();
        foreach (var item in normalized)
        {
            models[item.Id] = item.ModelId;
        }

        foreach (var warning in started.Warnings)
        {
            models.TryGetValue(warning.RequestId ?? string.Empty, out var modelId);
            WarningLog.Write(
                new[] { warning.Warning },
                api.Provider,
                warning.RequestId == null ? null : modelId
            );
        }

        started.Version = 2;
        started.Provider = api.Provider;
        return started;
    }

    private static void ValidateModelIds(IBatchApi api, IReadOnlyList<BatchRequest> requests)
    {
        var ids = api.ModelIds;
        if (ids == null)
        {
            return;
        }

        foreach (var item in requests)
        {
            var allowed = item.Type switch
            {
                "text" => ids.Text,
                "image" => ids.Image,
                _ => null,
            };
            if (allowed != null && !allowed.Contains(item.Model, StringComparer.Ordinal))
            {
                throw new InvalidArgumentException(
                    "requests",
                    item.Id,
                    "model \""
                        + item.Model
                        + "\" is not a "
                        + item.Type
                        + " model of the batch API. Expected one of: "
                        + string.Join(", ", allowed)
                );
            }
        }
    }

    /// <summary>Reads the latest status.</summary>
    public static async Task<BatchStatus> GetBatchStatusAsync(
        BatchRequestOptions request,
        CancellationToken cancellationToken = default
    )
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        using var scope = Token(request, cancellationToken);
        var token = scope.Token;
        var status = await OperationRetry
            .ExecuteAsync(
                request.MaxRetries,
                token,
                request.AbortReason,
                ct => api.DoGetStatusAsync(Call(api, request, token, request.Batch!.Id), ct),
                null
            )
            .ConfigureAwait(false);
        status.Version = 2;
        status.Id = request.Batch!.Id;
        status.Provider = api.Provider;
        return status;
    }

    /// <summary>
    /// Opens results immediately and normalizes them as they are read. Each item is read from the provider only when
    /// the caller asks for it. Use <see cref="GetAllBatchResultsAsync"/> to read every result into a list.
    /// </summary>
    public static IAsyncEnumerable<BatchItemResult> GetBatchResults(
        BatchRequestOptions request,
        CancellationToken cancellationToken = default
    )
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        var scope = Token(request, cancellationToken);
        var token = scope.Token;
        var opening = OperationRetry.ExecuteAsync(
            request.MaxRetries,
            token,
            request.AbortReason,
            ct => api.DoGetResultsAsync(Call(api, request, ct, request.Batch!.Id), ct),
            null
        );
        return ReadResults(opening, request, request.Output, scope, token);
    }

    /// <summary>
    /// Like <see cref="GetBatchResults(BatchRequestOptions, CancellationToken)"/>, but parses each text result with
    /// <paramref name="output"/> and exposes it as <typeparamref name="TOutput"/>.
    /// </summary>
    public static async IAsyncEnumerable<BatchItemResult<TOutput>> GetBatchResults<TOutput>(
        BatchRequestOptions request,
        OutputSpec<TOutput> output,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (request.Output != null && !ReferenceEquals(request.Output, output))
        {
            throw new ArgumentException(
                "request.Output must be the same specification as output.",
                nameof(output)
            );
        }

        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        var scope = Token(request, cancellationToken);
        var token = scope.Token;
        var opening = OperationRetry.ExecuteAsync(
            request.MaxRetries,
            token,
            request.AbortReason,
            ct => api.DoGetResultsAsync(Call(api, request, ct, request.Batch!.Id), ct),
            null
        );
        await foreach (
            var item in ReadResults(opening, request, output, scope, token)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false)
        )
        {
            yield return BatchItemResult<TOutput>.From(item, output);
        }
    }

    /// <summary>Reads every result of <see cref="GetBatchResults"/> into a list.</summary>
    public static async Task<IReadOnlyList<BatchItemResult>> GetAllBatchResultsAsync(
        BatchRequestOptions request,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<BatchItemResult>();
        await foreach (
            var result in GetBatchResults(request, cancellationToken).ConfigureAwait(false)
        )
        {
            results.Add(result);
        }

        return results;
    }

    private static async IAsyncEnumerable<BatchItemResult> ReadResults(
        Task<IAsyncEnumerable<BatchItem>> opening,
        BatchRequestOptions request,
        OutputSpec? output,
        BatchTokenScope scope,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        try
        {
            var items = await opening.ConfigureAwait(false);
            await foreach (
                var item in items.WithCancellation(cancellationToken).ConfigureAwait(false)
            )
            {
                yield return await ConvertAsync(item, request, output, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            scope.Dispose();
        }
    }

    private static async Task<BatchItemResult> ConvertAsync(
        BatchItem item,
        BatchRequestOptions request,
        OutputSpec? output,
        CancellationToken cancellationToken
    )
    {
        var result = new BatchItemResult
        {
            Type = item.Type,
            Id = item.Id,
            Status = item.Status,
            ErrorMessage = item.ErrorMessage,
            ErrorCode = item.ErrorCode,
            ErrorType = item.ErrorType,
            ErrorStatusCode = item.ErrorStatusCode,
            ProviderMetadata = item.ProviderMetadata,
        };
        if (item.Status != "succeeded")
        {
            return result;
        }

        if (item.Type == "image" && item.Image != null)
        {
            var images = new List<GeneratedImage>();
            for (var index = 0; index < item.Image.Images.Count; index++)
            {
                var bytes = MediaTypeDetector.ToBytes(item.Image.Images[index]);
                images.Add(
                    new GeneratedImage(
                        bytes,
                        MediaTypeDetector.Detect(bytes, "image") ?? "image/png",
                        ImageMetadata(item.Image.ProviderMetadata, index)
                    )
                );
            }

            result.Images = images;
            result.Warnings = item.Image.Warnings;
            result.Timestamp = ToIsoTimestamp(item.Image.Response?.Timestamp);
            result.ResponseModelId = item.Image.Response?.ModelId;
            result.ProviderMetadata = item.Image.ProviderMetadata;
            result.ImageUsage = item.Image.Usage;
            return result;
        }

        if (item.Text != null)
        {
            var content = new List<GeneratedContent>();
            var text = new List<string>();
            JsonElement? pendingInput = null;
            string? pendingCall = null;
            foreach (var part in item.Text.Content)
            {
                if (part.Type == "text")
                {
                    text.Add(part.Text ?? string.Empty);
                    content.Add(ToContentPart(part, null));
                }
                else if (part.Type == "tool-call")
                {
                    var input = ParseInput(part.Text);
                    part.Input = input;
                    pendingInput = input;
                    pendingCall = part.ToolCallId;
                    content.Add(ToContentPart(part, null));
                }
                else if (part.Type == "tool-result")
                {
                    if (part.ToolCallId == pendingCall)
                    {
                        part.Input = pendingInput;
                    }

                    content.Add(ToContentPart(part, part.Input));
                }
                else if (part.Type == "file" || part.Type == "reasoning-file")
                {
                    if (part.FileUrl != null)
                    {
                        if (request.Download == null)
                        {
                            throw new InvalidArgumentException(
                                "download",
                                null,
                                "download is required for file results"
                            );
                        }

                        var downloaded = await request
                            .Download(part.FileUrl, cancellationToken)
                            .ConfigureAwait(false);
                        part.FileBytes = downloaded.Data;
                    }

                    content.Add(ToContentPart(part, null));
                }
                else
                {
                    content.Add(ToContentPart(part, null));
                }
            }

            result.Content = content;
            result.Text = string.Concat(text);
            result.FinishReason = item.Text.FinishReason;
            result.RawFinishReason = item.Text.RawFinishReason;
            result.Usage = item.Text.Usage == null ? null : BatchUsage.FromModel(item.Text.Usage);
            result.ResponseId = item.Text.ResponseId;
            result.Timestamp = ToIsoTimestamp(item.Text.Timestamp);
            result.ResponseModelId = item.Text.ModelId;
            result.ProviderMetadata = item.Text.ProviderMetadata;
            if (output != null)
            {
                ParseBatchOutput(result, item.Text, output);
            }
        }

        return result;
    }

    // Maps a provider content part to the generate-text content part. Upstream does this with asContent.
    private static GeneratedContent ToContentPart(BatchContentPart part, JsonElement? input)
    {
        switch (part.Type)
        {
            case "text":
                return new GeneratedText(
                    part.Text ?? string.Empty,
                    part.Citations,
                    part.ProviderMetadata
                );
            case "reasoning":
                return new GeneratedReasoning(part.Text ?? string.Empty, part.ProviderMetadata);
            case "tool-call":
                return new GeneratedToolCall(
                    part.ToolCallId ?? string.Empty,
                    part.ToolName ?? string.Empty,
                    part.Input?.GetRawText() ?? "{}",
                    part.ProviderMetadata
                )
                {
                    ProviderExecuted = part.ProviderExecuted ?? false,
                    Dynamic = part.Dynamic ?? false,
                };
            case "tool-result":
                return new GeneratedToolResult(
                    part.ToolCallId ?? string.Empty,
                    part.ToolName ?? string.Empty,
                    part.Output?.GetRawText() ?? "null",
                    part.IsError ?? false
                )
                {
                    ProviderMetadata = part.ProviderMetadata,
                    Dynamic = part.Dynamic ?? false,
                    ProviderExecuted = part.ProviderExecuted ?? true,
                    InputJson = input?.GetRawText(),
                };
            case "file":
                return new GeneratedFile(part.FileBytes ?? [], part.MediaType ?? string.Empty);
            case "reasoning-file":
                return new GeneratedReasoningFile(
                    part.FileBytes ?? [],
                    part.MediaType ?? string.Empty,
                    part.ProviderMetadata
                );
            case "source":
                return part.SourceType == "document"
                    ? new GeneratedDocumentSource(
                        part.SourceId ?? string.Empty,
                        part.MediaType ?? string.Empty,
                        part.Title ?? string.Empty,
                        null,
                        part.ProviderMetadata
                    )
                    : new GeneratedSource(
                        part.SourceId ?? string.Empty,
                        part.Url ?? string.Empty,
                        part.Title,
                        part.ProviderMetadata
                    );
            default:
                return new GeneratedCustom(part.Kind ?? part.Type, part.ProviderMetadata);
        }
    }

    private static string? ToIsoTimestamp(DateTime? timestamp) =>
        timestamp
            ?.ToUniversalTime()
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

    private static JsonElement ParseInput(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return OperationJson.Parse("{}");
        }

        return OperationJson.Parse(text!);
    }

    private static JsonElement? ImageMetadata(JsonElement? providerMetadata, int index)
    {
        if (providerMetadata is not JsonElement root || root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var collected = new Dictionary<string, JsonElement>();
        foreach (var provider in root.EnumerateObject())
        {
            if (
                provider.Value.TryGetProperty("images", out var images)
                && images.ValueKind == JsonValueKind.Array
                && index < images.GetArrayLength()
                && images[index].ValueKind == JsonValueKind.Object
            )
            {
                collected[provider.Name] = images[index];
            }
        }

        return collected.Count == 0 ? null : OperationJson.Serialize(collected);
    }

    private static void ValidateRequests(IReadOnlyList<BatchRequest> requests)
    {
        if (requests.Count == 0)
        {
            throw new InvalidArgumentException("requests", requests, "requests must not be empty");
        }

        var ids = new HashSet<string>();
        foreach (var request in requests)
        {
            if (request.Id.Trim().Length == 0)
            {
                throw new InvalidArgumentException(
                    "requests",
                    requests,
                    "request IDs must not be empty"
                );
            }

            if (!ids.Add(request.Id))
            {
                throw new InvalidArgumentException(
                    "requests",
                    requests,
                    "request IDs must be unique; duplicate ID \"" + request.Id + "\""
                );
            }
        }
    }

    private static void ValidateReference(IBatchApi api, BatchReference? batch)
    {
        if (batch == null || batch.Version != 2)
        {
            throw new InvalidArgumentException(
                "batch",
                batch,
                "batch must be a supported batch reference"
            );
        }

        if (batch.Provider != api.Provider)
        {
            throw new InvalidArgumentException(
                "provider",
                api,
                "provider "
                    + api.Provider
                    + " is not compatible with batch provider "
                    + batch.Provider
            );
        }
    }

    private static IBatchApi Resolve(BatchRequestOptions request)
    {
        var selected = request.Provider
            ?? (
                BatchModels.Default?.Invoke() is IBatchProvider fallback
                    ? ApiOrProvider<IBatchApi, IBatchProvider>.FromProvider(fallback)
                    : null
            );
        if (selected == null)
        {
            throw new UnsupportedFunctionalityException(
                "batch processing",
                "The provider does not support batch processing. Make sure it exposes an experimental_batch() method."
            );
        }

        if (selected.Api != null)
        {
            return selected.Api;
        }

        var created = selected.Provider?.ExperimentalBatch();
        if (created != null)
        {
            return created;
        }

        throw new UnsupportedFunctionalityException(
            "batch processing",
            "The provider does not support batch processing. Make sure it exposes an experimental_batch() method."
        );
    }

    private static BatchTokenScope Token(
        BatchRequestOptions request,
        CancellationToken cancellationToken
    )
    {
        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        return request.TimeoutMs is int timeout
            ? new BatchTokenScope(token, timeout)
            : new BatchTokenScope(token);
    }

    // Owns the timeout and linked sources of one call, so the timer and the registration on the caller's token are released.
    private sealed class BatchTokenScope : IDisposable
    {
        private readonly CancellationTokenSource? _timeout;
        private readonly CancellationTokenSource? _linked;

        public BatchTokenScope(CancellationToken token)
        {
            Token = token;
        }

        public BatchTokenScope(CancellationToken token, int timeoutMs)
        {
            _timeout = new CancellationTokenSource(timeoutMs);
            _linked = CancellationTokenSource.CreateLinkedTokenSource(token, _timeout.Token);
            Token = _linked.Token;
        }

        public CancellationToken Token { get; }

        public void Dispose()
        {
            _linked?.Dispose();
            _timeout?.Dispose();
        }
    }

    private static BatchOperationCall Call(
        IBatchApi api,
        BatchRequestOptions request,
        CancellationToken cancellationToken,
        string? batchId,
        IReadOnlyList<NormalizedBatchRequest>? requests = null
    )
    {
        return new BatchOperationCall(
            batchId,
            request.ProviderOptions,
            OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent),
            cancellationToken,
            request.Limit,
            request.Cursor,
            request.WebhookUrl,
            requests
        )
        {
            Receiver = api,
        };
    }

    private static List<JsonElement> PrepareTools(
        IReadOnlyList<BatchTool>? tools,
        IReadOnlyList<string>? toolOrder
    )
    {
        var prepared = new List<JsonElement>();
        if (tools == null)
        {
            return prepared;
        }

        // Listed tools come first, in order. The rest follow, sorted by name. Maps to orderToolEntries.
        var ordered =
            toolOrder == null
                ? tools
                : tools
                    .Where(tool => toolOrder.Contains(tool.Name))
                    .OrderBy(tool => ((List<string>)[.. toolOrder]).IndexOf(tool.Name))
                    .Concat(
                        tools
                            .Where(tool => !toolOrder.Contains(tool.Name))
                            .OrderBy(tool => tool.Name, StringComparer.Ordinal)
                    )
                    .ToList();
        foreach (var tool in ordered)
        {
            var payload = new Dictionary<string, object?>
            {
                ["type"] = "function",
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["inputSchema"] = tool.InputSchema,
            };
            prepared.Add(OperationJson.Serialize(payload));
        }

        return prepared;
    }

    private static string NameOf(JsonElement tool)
    {
        return tool.TryGetProperty("name", out var name)
            ? name.GetString() ?? string.Empty
            : string.Empty;
    }

    private static async Task<JsonElement> TextOptionsAsync(
        BatchRequest request,
        List<JsonElement> tools,
        OutputSpec? output,
        IBatchApi api,
        CancellationToken cancellationToken
    )
    {
        var standardized = Prompts.StandardizePrompt(
            new StandardizePromptInput
            {
                System = request.System,
                Instructions = request.Instructions,
                Prompt =
                    request.Messages == null ? (PromptInput)(request.Prompt ?? string.Empty) : null,
                Messages = request.Messages,
            }
        );
        var messages = new List<ModelMessage>(
            Generation.InstructionMessages(standardized.Instructions)
        );
        messages.AddRange(standardized.Messages);

        // Maps to the supportedUrls argument of convertToLanguageModelPrompt: no custom download function.
        var converted = await LanguageModelPrompts
            .ConvertToLanguageModelPromptAsync(
                messages,
                api.SupportedUrls,
                null,
                cancellationToken,
                api.Provider.Split('.')[0]
            )
            .ConfigureAwait(false);
        var prompt = OperationJson
            .Parse(V4Json.CallOptions(new LanguageModelCallOptions { Prompt = converted }))
            .GetProperty("prompt")
            .Clone();
        var options = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["maxOutputTokens"] = request.MaxOutputTokens,
            ["temperature"] = request.Temperature,
            ["topP"] = request.TopP,
            ["topK"] = request.TopK,
            ["presencePenalty"] = request.PresencePenalty,
            ["frequencyPenalty"] = request.FrequencyPenalty,
            ["stopSequences"] = request.StopSequences,
            ["seed"] = request.Seed,
            ["reasoning"] = request.Reasoning,
            ["providerOptions"] = request.ProviderOptions,
        };
        if (output != null)
        {
            options["responseFormat"] = ResponseFormat(output);
        }

        if (tools.Count > 0)
        {
            options["tools"] = tools;
            options["toolChoice"] =
                request.ToolChoice == null
                    ? null
                    : OperationJson.Parse(
                        "{\"type\":" + JsonSerializer.Serialize(request.ToolChoice) + "}"
                    );
        }

        return OperationJson.Serialize(options);
    }

    // Maps to the responseFormat of an output specification.
    private static Dictionary<string, object?> ResponseFormat(OutputSpec output)
    {
        if (!output.IsJson)
        {
            return new Dictionary<string, object?> { ["type"] = "text" };
        }

        return new Dictionary<string, object?>
        {
            ["type"] = "json",
            ["schema"] = output.Schema,
            ["name"] = output.SchemaName,
            ["description"] = output.SchemaDescription,
        };
    }

    // Maps to parseBatchOutput. Failures are reported as OutputError so that one malformed item does not affect the
    // rest of the batch or discard the item's text and usage.
    private static void ParseBatchOutput(
        BatchItemResult result,
        BatchTextGeneration text,
        OutputSpec output
    )
    {
        var finishReason = text.FinishReason;

        // Same conditions as generateText: stop responses and non-empty responses that are not tool calls.
        if (
            !(
                finishReason == "stop"
                || (finishReason != "tool-calls" && (result.Text ?? string.Empty).Length > 0)
            )
        )
        {
            var error = new NoOutputGeneratedException();
            result.OutputError = new BatchError { Message = error.Message, Type = error.Name };
            return;
        }

        var textForOutput = output.IsJson
            ? text.StructuredOutputText ?? result.Text ?? string.Empty
            : result.Text ?? string.Empty;
        try
        {
            result.Output = output.ParseCompleteOutput(textForOutput);
        }
        catch (Exception error)
            when (error is ProviderUtils.JsonParseException or ProviderUtils.TypeValidationException
            )
        {
            result.OutputError = new BatchError
            {
                Message =
                    error is ProviderUtils.TypeValidationException
                        ? "No object generated: response did not match schema."
                        : "No object generated: could not parse the response.",
                Type = "AI_NoObjectGeneratedError",
            };
        }
    }

    private static JsonElement ImageOptions(BatchRequest request, ImagePromptParts parts)
    {
        return OperationJson.Serialize(
            new Dictionary<string, object?>
            {
                ["prompt"] = parts.Prompt,
                ["n"] = request.N ?? 1,
                ["size"] = request.Size,
                ["aspectRatio"] = request.AspectRatio,
                ["seed"] = request.Seed,
                ["files"] = parts.Files,
                ["mask"] = parts.Mask,
                ["providerOptions"] = request.ProviderOptions ?? OperationJson.Parse("{}"),
            }
        );
    }
}
