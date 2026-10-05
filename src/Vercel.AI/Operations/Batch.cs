// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>A tool definition forwarded to a batch without being executed.</summary>
public sealed class BatchTool
{
    /// <summary>Creates a tool definition.</summary>
    public BatchTool(string name, JsonElement inputSchema, string? description = null, Func<JsonElement, CancellationToken, Task<object?>>? execute = null)
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

/// <summary>One request in a batch.</summary>
public sealed class BatchRequest
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

    /// <summary>Text prompt.</summary>
    public string? Prompt { get; set; }

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

    /// <summary>Tool choice such as <c>required</c>.</summary>
    public string? ToolChoice { get; set; }
}

/// <summary>Normalized request sent to <c>doStartBatch</c>.</summary>
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

    /// <summary>Normalized options.</summary>
    public JsonElement Options { get; }
}

/// <summary>Provider token usage before it is reshaped.</summary>
public sealed class BatchModelUsage
{
    /// <summary>Creates provider usage.</summary>
    public BatchModelUsage(int? inputTotal = null, int? noCache = null, int? cacheRead = null, int? cacheWrite = null, int? outputTotal = null, int? text = null, int? reasoning = null, JsonElement? raw = null)
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
    public BatchUsage(int? inputTokens, int? noCacheTokens, int? cacheReadTokens, int? cacheWriteTokens, int? outputTokens, int? textTokens, int? reasoningTokens, int? totalTokens, JsonElement? raw)
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
        int? total = usage.InputTotal == null && usage.OutputTotal == null ? null : (usage.InputTotal ?? 0) + (usage.OutputTotal ?? 0);
        return new BatchUsage(usage.InputTotal, usage.NoCache, usage.CacheRead, usage.CacheWrite, usage.OutputTotal, usage.Text, usage.Reasoning, total, usage.Raw);
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

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }

    /// <summary>File media type.</summary>
    public string? MediaType { get; set; }

    /// <summary>Downloaded file bytes.</summary>
    public byte[]? FileBytes { get; set; }

    /// <summary>File URL before download.</summary>
    public string? FileUrl { get; set; }
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

    /// <summary>Provider metadata for a failed item.</summary>
    public JsonElement? ProviderMetadata { get; set; }
}

/// <summary>Normalized batch item.</summary>
public sealed class BatchItemResult
{
    /// <summary>Item type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Request id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Text content.</summary>
    public IReadOnlyList<BatchContentPart>? Content { get; set; }

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

    /// <summary>Response timestamp.</summary>
    public DateTime? Timestamp { get; set; }

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
}

/// <summary>A version-2 batch reference.</summary>
public sealed class BatchReference
{
    /// <summary>Creates a reference.</summary>
    public BatchReference(int version, string id, string provider)
    {
        Version = version;
        Id = id ?? string.Empty;
        Provider = provider ?? string.Empty;
    }

    /// <summary>Reference version. Only 2 is accepted.</summary>
    public int Version { get; }

    /// <summary>Batch id.</summary>
    public string Id { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }
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

/// <summary>Status fields shared by start, list, and status calls.</summary>
public class BatchStatus
{
    /// <summary>Status name.</summary>
    public string? Status { get; set; }

    /// <summary>Provider status string.</summary>
    public string? RawStatus { get; set; }

    /// <summary>Request counts.</summary>
    public BatchRequestCounts? RequestCounts { get; set; }

    /// <summary>Creation time.</summary>
    public string? CreatedAt { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; set; }
}

/// <summary>Start acknowledgement.</summary>
public sealed class BatchStartResult : BatchStatus
{
    /// <summary>Reference version.</summary>
    public int Version { get; set; } = 2;

    /// <summary>Batch id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Provider id.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Warnings.</summary>
    public IReadOnlyList<BatchRequestWarning> Warnings { get; set; } = Array.Empty<BatchRequestWarning>();
}

/// <summary>One listed batch.</summary>
public sealed class ListedBatch : BatchStatus
{
    /// <summary>Reference version.</summary>
    public int Version { get; set; } = 2;

    /// <summary>Batch id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Provider id.</summary>
    public string Provider { get; set; } = string.Empty;
}

/// <summary>A page of batches.</summary>
public sealed class BatchListResult
{
    /// <summary>Creates a page.</summary>
    public BatchListResult(IReadOnlyList<ListedBatch> batches, string? nextCursor = null, JsonElement? providerMetadata = null)
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
    public BatchOperationCall(string? batchId, JsonElement? providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, int? limit = null, string? cursor = null, string? webhookUrl = null, IReadOnlyList<NormalizedBatchRequest>? requests = null)
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

    /// <summary>True when cancellation is implemented.</summary>
    bool CanCancel { get; }

    /// <summary>True when listing is implemented.</summary>
    bool CanList { get; }

    /// <summary>Starts a batch.</summary>
    Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <summary>Reads status.</summary>
    Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <summary>Opens the result stream.</summary>
    Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <summary>Cancels a batch.</summary>
    Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <summary>Lists batches. The implementation is invoked on this instance.</summary>
    Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken);
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
    public object? Provider { get; set; }

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

    /// <summary>Total timeout in milliseconds.</summary>
    public int? TimeoutMs { get; set; }

    /// <summary>Downloads file URLs inside results.</summary>
    public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
}

/// <summary>Batch operations. Maps to <c>startBatch</c>, <c>getBatchStatus</c>, <c>getBatchResults</c>, <c>listBatches</c>, and <c>cancelBatch</c>.</summary>
public static class Batch
{
    /// <summary>Requests cancellation.</summary>
    public static async Task<BatchCancelResult> CancelBatchAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        if (!api.CanCancel)
        {
            throw new UnsupportedFunctionalityException("batch cancellation", "The provider does not support batch cancellation.");
        }

        var token = Token(request, cancellationToken);
        return await api.DoCancelAsync(Call(api, request, token, request.Batch!.Id), token).ConfigureAwait(false);
    }

    /// <summary>Lists one page. <c>doListBatches</c> is invoked on the batch API instance.</summary>
    public static async Task<BatchListResult> ListBatchesAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
    {
        request ??= new BatchRequestOptions();
        var api = Resolve(request);
        if (!api.CanList)
        {
            throw new UnsupportedFunctionalityException("batch listing", "The provider does not support listing batches.");
        }

        var token = Token(request, cancellationToken);
        var call = Call(api, request, token, null);
        call.Receiver = api;
        var page = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => api.DoListAsync(call, ct), null).ConfigureAwait(false);
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
    public static async Task<BatchStartResult> StartBatchAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requests = request.Requests ?? Array.Empty<BatchRequest>();
        ValidateRequests(requests);
        var api = Resolve(request);
        var token = Token(request, cancellationToken);
        var normalized = new List<NormalizedBatchRequest>();
        var tools = new Dictionary<string, string>();
        foreach (var item in requests)
        {
            if (item.Type == "text")
            {
                var prepared = PrepareTools(item.Tools);
                foreach (var tool in prepared)
                {
                    var signature = tool.GetRawText();
                    if (tools.TryGetValue(NameOf(tool), out var previous) && previous != signature)
                    {
                        throw new InvalidArgumentException("requests", item.Id, "tool \"" + NameOf(tool) + "\" must have the same definition in every batch request");
                    }

                    tools[NameOf(tool)] = signature;
                }

                normalized.Add(new NormalizedBatchRequest(item.Id, item.Type, item.Model, TextOptions(item, prepared)));
            }
            else if (item.Type == "image")
            {
                var parts = GenerateImage.NormalizePrompt(item.ImagePrompt, item.Prompt);
                normalized.Add(new NormalizedBatchRequest(item.Id, item.Type, item.Model, ImageOptions(item, parts)));
            }
            else
            {
                throw new InvalidArgumentException("requests", item.Type, "Unsupported batch request type \"" + item.Type + "\".");
            }

            OperationRetry.ThrowIfAborted(token, request.AbortReason);
        }

        var started = await api.DoStartAsync(Call(api, request, token, null, normalized), token).ConfigureAwait(false);
        var models = new Dictionary<string, string>();
        foreach (var item in normalized)
        {
            models[item.Id] = item.ModelId;
        }

        foreach (var warning in started.Warnings)
        {
            models.TryGetValue(warning.RequestId ?? string.Empty, out var modelId);
            WarningLog.Write(new[] { warning.Warning }, api.Provider, warning.RequestId == null ? null : modelId);
        }

        started.Version = 2;
        started.Provider = api.Provider;
        return started;
    }

    /// <summary>Reads the latest status.</summary>
    public static async Task<BatchStatus> GetBatchStatusAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        var token = Token(request, cancellationToken);
        return await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => api.DoGetStatusAsync(Call(api, request, token, request.Batch!.Id), ct), null).ConfigureAwait(false);
    }

    /// <summary>Opens results immediately and normalizes them as they are read.</summary>
    public static IAsyncEnumerable<BatchItemResult> GetBatchResults(BatchRequestOptions request, CancellationToken cancellationToken = default)
    {
        var api = Resolve(request);
        ValidateReference(api, request.Batch);
        var token = Token(request, cancellationToken);
        var queue = new PartQueue<BatchItemResult>();
        _ = OpenResultsAsync(api, request, token, queue);
        return ReadResults(queue, token);
    }

    private static async Task OpenResultsAsync(IBatchApi api, BatchRequestOptions request, CancellationToken cancellationToken, PartQueue<BatchItemResult> queue)
    {
        try
        {
            var items = await OperationRetry.ExecuteAsync(request.MaxRetries, cancellationToken, request.AbortReason, ct => api.DoGetResultsAsync(Call(api, request, ct, request.Batch!.Id), ct), null).ConfigureAwait(false);
            foreach (var item in items)
            {
                queue.Enqueue(await ConvertAsync(item, request, cancellationToken).ConfigureAwait(false));
            }

            queue.Complete(null);
        }
        catch (Exception error)
        {
            queue.Complete(error);
        }
    }

    private static async IAsyncEnumerable<BatchItemResult> ReadResults(PartQueue<BatchItemResult> queue, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await queue.WaitAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return queue.Dequeue();
        }
    }

    private static async Task<BatchItemResult> ConvertAsync(BatchItem item, BatchRequestOptions request, CancellationToken cancellationToken)
    {
        var result = new BatchItemResult { Type = item.Type, Id = item.Id, Status = item.Status, ErrorMessage = item.ErrorMessage, ErrorCode = item.ErrorCode, ProviderMetadata = item.ProviderMetadata };
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
                images.Add(new GeneratedImage(bytes, MediaTypeDetector.Detect(bytes, "image") ?? "image/png", ImageMetadata(item.Image.ProviderMetadata, index)));
            }

            result.Images = images;
            result.Warnings = item.Image.Warnings;
            result.Timestamp = item.Image.Response?.Timestamp;
            result.ResponseModelId = item.Image.Response?.ModelId;
            result.ProviderMetadata = item.Image.ProviderMetadata;
            result.ImageUsage = item.Image.Usage;
            return result;
        }

        if (item.Text != null)
        {
            var content = new List<BatchContentPart>();
            var text = new List<string>();
            JsonElement? pendingInput = null;
            string? pendingCall = null;
            foreach (var part in item.Text.Content)
            {
                if (part.Type == "text")
                {
                    text.Add(part.Text ?? string.Empty);
                    content.Add(part);
                }
                else if (part.Type == "tool-call")
                {
                    var input = ParseInput(part.Text);
                    part.Input = input;
                    pendingInput = input;
                    pendingCall = part.ToolCallId;
                    content.Add(part);
                }
                else if (part.Type == "tool-result")
                {
                    if (part.ToolCallId == pendingCall)
                    {
                        part.Input = pendingInput;
                    }

                    content.Add(part);
                }
                else if (part.Type == "file" || part.Type == "reasoning-file")
                {
                    if (part.FileUrl != null)
                    {
                        if (request.Download == null)
                        {
                            throw new InvalidArgumentException("download", null, "download is required for file results");
                        }

                        var downloaded = await request.Download(part.FileUrl, cancellationToken).ConfigureAwait(false);
                        part.FileBytes = downloaded.Data;
                    }

                    content.Add(part);
                }
                else
                {
                    content.Add(part);
                }
            }

            result.Content = content;
            result.Text = string.Concat(text);
            result.FinishReason = item.Text.FinishReason;
            result.RawFinishReason = item.Text.RawFinishReason;
            result.Usage = item.Text.Usage == null ? null : BatchUsage.FromModel(item.Text.Usage);
            result.ResponseId = item.Text.ResponseId;
            result.Timestamp = item.Text.Timestamp;
            result.ResponseModelId = item.Text.ModelId;
            result.ProviderMetadata = item.Text.ProviderMetadata;
        }

        return result;
    }

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
            if (provider.Value.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array && index < images.GetArrayLength() && images[index].ValueKind == JsonValueKind.Object)
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
                throw new InvalidArgumentException("requests", requests, "request IDs must not be empty");
            }

            if (!ids.Add(request.Id))
            {
                throw new InvalidArgumentException("requests", requests, "request IDs must be unique; duplicate ID \"" + request.Id + "\"");
            }
        }
    }

    private static void ValidateReference(IBatchApi api, BatchReference? batch)
    {
        if (batch == null || batch.Version != 2)
        {
            throw new InvalidArgumentException("batch", batch, "batch must be a supported batch reference");
        }

        if (batch.Provider != api.Provider)
        {
            throw new InvalidArgumentException("provider", api, "provider " + api.Provider + " is not compatible with batch provider " + batch.Provider);
        }
    }

    private static IBatchApi Resolve(BatchRequestOptions request)
    {
        var provider = request.Provider;
        if (provider == null)
        {
            provider = BatchModels.Default?.Invoke() ?? throw new UnsupportedFunctionalityException("batch processing", "The provider does not support batch processing. Make sure it exposes an experimental_batch() method.");
        }

        if (provider is IBatchApi api)
        {
            return api;
        }

        if (provider is IBatchProvider factory)
        {
            var created = factory.ExperimentalBatch();
            if (created != null)
            {
                return created;
            }
        }

        throw new UnsupportedFunctionalityException("batch processing", "The provider does not support batch processing. Make sure it exposes an experimental_batch() method.");
    }

    private static CancellationToken Token(BatchRequestOptions request, CancellationToken cancellationToken)
    {
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        if (request.TimeoutMs is int timeout)
        {
            var source = new CancellationTokenSource(timeout);
            return CancellationTokenSource.CreateLinkedTokenSource(token, source.Token).Token;
        }

        return token;
    }

    private static BatchOperationCall Call(IBatchApi api, BatchRequestOptions request, CancellationToken cancellationToken, string? batchId, IReadOnlyList<NormalizedBatchRequest>? requests = null)
    {
        return new BatchOperationCall(batchId, request.ProviderOptions, OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent), cancellationToken, request.Limit, request.Cursor, request.WebhookUrl, requests) { Receiver = api };
    }

    private static List<JsonElement> PrepareTools(IReadOnlyList<BatchTool>? tools)
    {
        var prepared = new List<JsonElement>();
        if (tools == null)
        {
            return prepared;
        }

        foreach (var tool in tools)
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
        return tool.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty;
    }

    private static JsonElement TextOptions(BatchRequest request, List<JsonElement> tools)
    {
        var prompt = OperationJson.Parse("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":" + JsonSerializer.Serialize(request.Prompt ?? string.Empty) + "}]}]");
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
        if (tools.Count > 0)
        {
            options["tools"] = tools;
            options["toolChoice"] = request.ToolChoice == null ? null : OperationJson.Parse("{\"type\":" + JsonSerializer.Serialize(request.ToolChoice) + "}");
        }

        return OperationJson.Serialize(options);
    }

    private static JsonElement ImageOptions(BatchRequest request, ImagePromptParts parts)
    {
        return OperationJson.Serialize(new Dictionary<string, object?>
        {
            ["prompt"] = parts.Prompt,
            ["n"] = request.N ?? 1,
            ["size"] = request.Size,
            ["aspectRatio"] = request.AspectRatio,
            ["seed"] = request.Seed,
            ["files"] = parts.Files,
            ["mask"] = parts.Mask,
            ["providerOptions"] = request.ProviderOptions ?? OperationJson.Parse("{}"),
        });
    }
}
