// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>AI Gateway batch API. Downloads the JSONL results of a batch.</summary>
public sealed class GatewayBatchApi : BatchResultsApiBase
{
    private readonly GatewayProvider _provider;

    internal GatewayBatchApi(GatewayProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => "gateway.batch";

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> SupportedUrls { get; } =
        new Dictionary<string, string> { ["*/*"] = ".*" };

    /// <inheritdoc />
    public override bool CanCancel => true;

    /// <summary>
    /// Starts a durable batch of text-generation requests through the Gateway's async batch surface
    /// (<c>POST {baseURL}/batch/start</c>). Maps to <c>doStartBatch</c>.
    /// </summary>
    public override async Task<BatchStartResult> DoStartAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        AssertTextBatchRequests(requests);
        var modelId = ValidateSingleModel(requests);

        var idempotencyKey = GetGatewayBatchIdempotencyKey(call.ProviderOptions);
        var forwardedProviderOptions = OmitGatewayIdempotencyKey(call.ProviderOptions);
        var extra = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        extra["ai-model-id"] = modelId;
        if (idempotencyKey != null)
        {
            extra["idempotency-key"] = idempotencyKey;
        }

        var headers = await _provider
            .BatchHeadersAsync(extra, cancellationToken)
            .ConfigureAwait(false);
        var body = new JsonObject();
        if (call.WebhookUrl != null)
        {
            body["callbackUrl"] = call.WebhookUrl;
        }

        var items = new JsonArray();
        foreach (var request in requests)
        {
            items.Add(
                new JsonObject
                {
                    ["id"] = request.Id,
                    ["type"] = request.Type,
                    ["modelId"] = request.ModelId,
                    ["options"] = JsonNode.Parse(request.Options.GetRawText()),
                }
            );
        }

        body["requests"] = items;
        if (forwardedProviderOptions != null)
        {
            body["providerOptions"] = forwardedProviderOptions;
        }

        var json = body.ToJsonString();
        try
        {
            using var document = await _provider
                .Http.SendJsonAsync(
                    HttpMethod.Post,
                    _provider.Route("batch/start"),
                    json,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var root = document.RootElement;
            var result = new BatchStartResult();
            ConvertGatewayBatchStatus(root, result);
            result.Id = Str(root, "batchId") ?? throw InvalidResponse(root);
            var warnings = new List<BatchRequestWarning>();
            if (
                root.TryGetProperty("warnings", out var items2)
                && items2.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var item in items2.EnumerateArray())
                {
                    var warning = item.TryGetProperty("warning", out var value) ? value : default;
                    warnings.Add(
                        new BatchRequestWarning(
                            Str(item, "requestId"),
                            new OperationWarning(
                                Str(warning, "type") ?? "other",
                                Str(warning, "message"),
                                Str(warning, "feature"),
                                Str(warning, "details"),
                                Str(warning, "setting")
                            )
                        )
                    );
                }
            }

            result.Warnings = warnings;
            return result;
        }
        catch (Exception exception) when (!IsCancellation(exception))
        {
            throw GatewayProvider.MapFailure(exception, headers, json);
        }
    }

    /// <summary>Retrieves the lifecycle status of a Gateway batch job (<c>POST {baseURL}/batch/status</c>). Maps to <c>doGetBatchStatus</c>.</summary>
    public override async Task<BatchStatus> DoGetStatusAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        try
        {
            using var document = await PostBatchAsync(
                    "batch/status",
                    call,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var status = new BatchStatus();
            ConvertGatewayBatchStatus(document.RootElement, status);
            return status;
        }
        catch (Exception exception) when (!IsCancellation(exception))
        {
            throw GatewayProvider.MapFailure(exception, headers);
        }
    }

    /// <summary>Requests cancellation; status and partial results remain separate reads. Maps to <c>doCancelBatch</c>.</summary>
    public override async Task<BatchCancelResult> DoCancelAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        try
        {
            using var document = await PostBatchAsync(
                    "batch/cancel",
                    call,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var result = new BatchCancelResult();
            ConvertGatewayBatchStatus(document.RootElement, new BatchStatus());
            if (
                document.RootElement.TryGetProperty("providerMetadata", out var metadata)
                && metadata.ValueKind == JsonValueKind.Object
            )
            {
                result.ProviderMetadata = metadata.Clone();
            }

            return result;
        }
        catch (Exception exception) when (!IsCancellation(exception))
        {
            throw GatewayProvider.MapFailure(exception, headers);
        }
    }

    /// <summary>
    /// Streams the per-request results of a terminal Gateway batch job (<c>POST {baseURL}/batch/results</c>, <c>application/x-ndjson</c>).
    /// The route responds 400 while the batch is non-terminal. Maps to <c>doGetBatchResults</c>.
    /// </summary>
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        var body = new JsonObject { ["batchId"] = call.BatchId }.ToJsonString();
        try
        {
            var lines = await _provider
                .Http.OpenJsonLinesAsync(
                    HttpMethod.Post,
                    _provider.Route("batch/results"),
                    body,
                    headers,
                    _provider.Options.BatchResultDownloads?.MaxLineBytes,
                    cancellationToken
                )
                .ConfigureAwait(false);
            return ConvertRows(lines, Convert, cancellationToken);
        }
        catch (Exception exception)
            when (!IsCancellation(exception)
                && !Util.DownloadError.IsInstance(exception)
                && !(exception is Util.InvalidArgumentError)
                && !(exception is Operations.InvalidArgumentException)
            )
        {
            throw GatewayProvider.MapFailure(exception, headers);
        }
    }

    // The Gateway already sanitizes the complete result objects server-side, so the items pass through.
    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "id") ?? throw InvalidResponse(line);
        var status = Str(line, "status") ?? throw InvalidResponse(line);
        if (status == "succeeded" && line.TryGetProperty("result", out var result))
        {
            var parts = new List<BatchContentPart>();
            if (
                result.TryGetProperty("content", out var content)
                && content.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var part in content.EnumerateArray())
                {
                    if (Str(part, "type") == "text")
                    {
                        parts.Add(
                            new BatchContentPart("text")
                            {
                                Text = Str(part, "text") ?? string.Empty,
                            }
                        );
                    }
                }
            }

            var finish = result.TryGetProperty("finishReason", out var reason) ? reason : default;
            var response = result.TryGetProperty("response", out var value) ? value : default;
            var generation = new BatchTextGeneration
            {
                Content = parts,
                FinishReason = Str(finish, "unified"),
                RawFinishReason = Str(finish, "raw"),
                ResponseId = Str(response, "id"),
                ModelId = Str(response, "modelId"),
                // JSON carries response.timestamp as an ISO string; core expects a date.
                Timestamp =
                    Str(response, "timestamp") is { } timestamp
                    && DateTimeOffset.TryParse(
                        timestamp,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out var parsed
                    )
                        ? parsed.UtcDateTime
                        : null,
            };
            if (
                result.TryGetProperty("usage", out var usage)
                && usage.ValueKind == JsonValueKind.Object
            )
            {
                var input = usage.TryGetProperty("inputTokens", out var inputTokens)
                    ? inputTokens
                    : default;
                var output = usage.TryGetProperty("outputTokens", out var outputTokens)
                    ? outputTokens
                    : default;
                generation.Usage = new BatchModelUsage(
                    Int(input, "total"),
                    Int(input, "noCache"),
                    Int(input, "cacheRead"),
                    Int(input, "cacheWrite"),
                    Int(output, "total"),
                    Int(output, "text"),
                    Int(output, "reasoning"),
                    usage.Clone()
                );
            }

            if (
                result.TryGetProperty("providerMetadata", out var metadata)
                && metadata.ValueKind == JsonValueKind.Object
            )
            {
                generation.ProviderMetadata = metadata.Clone();
            }

            return new BatchItem("text", id, status) { Text = generation };
        }

        var item = new BatchItem("text", id, status);
        if (line.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            item.ErrorMessage = Str(error, "message");
            item.ErrorCode = Str(error, "code");
            item.ErrorType = Str(error, "type");
            item.ErrorStatusCode = Int(error, "statusCode");
        }

        return item;
    }

    private Task<Dictionary<string, string?>> BatchHeadersAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    ) =>
        _provider.BatchHeadersAsync(
            call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            cancellationToken
        );

    private Task<JsonDocument> PostBatchAsync(
        string path,
        BatchOperationCall call,
        IReadOnlyDictionary<string, string?> headers,
        CancellationToken cancellationToken
    ) =>
        _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.Route(path),
            new JsonObject { ["batchId"] = call.BatchId }.ToJsonString(),
            headers,
            cancellationToken
        );

    // A cancelled batch start may still have been accepted server-side, so a cancel must not surface as a retryable 500.
    private static bool IsCancellation(Exception exception) =>
        exception is OperationCanceledException or Util.TimeoutError;

    private static Exception InvalidResponse(JsonElement data) =>
        new InvalidResponseDataException(data.Clone(), "Invalid Gateway batch response.");

    private static int? Int(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var number)
            ? number
            : null;

    private static void ConvertGatewayBatchStatus(JsonElement body, BatchStatus target)
    {
        var status = Str(body, "status");
        if (status is not ("completed" or "failed" or "pending"))
        {
            throw InvalidResponse(body);
        }

        target.Status = status;
        target.RawStatus = Str(body, "rawStatus");
        var counts = body.TryGetProperty("requestCounts", out var requestCounts)
            ? requestCounts
            : default;
        var normalized = ProviderValues.NormalizeBatchRequestCounts(
            Int(counts, "total"),
            Int(counts, "pending"),
            Int(counts, "completed"),
            Int(counts, "failed")
        );
        target.RequestCounts =
            normalized == null
                ? null
                : new BatchRequestCounts(
                    (int)normalized.Total,
                    (int)normalized.Pending,
                    (int)normalized.Completed,
                    (int)normalized.Failed
                );
        if (body.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            target.ErrorMessage = Str(error, "message") ?? throw InvalidResponse(body);
            target.ErrorType = Str(error, "type");
            target.ErrorCode = Str(error, "code");
            target.ErrorStatusCode = Int(error, "statusCode");
        }

        target.CreatedAt = Str(body, "createdAt");
        target.ExpiresAt = Str(body, "expiresAt");
        if (
            body.TryGetProperty("providerMetadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
        )
        {
            target.ProviderMetadata = metadata.Clone();
        }
    }

    private static void AssertTextBatchRequests(IReadOnlyList<NormalizedBatchRequest> requests)
    {
        foreach (var request in requests)
        {
            if (request.Type != "text")
            {
                throw new UnsupportedFunctionalityException(
                    "batch request type: " + request.Type,
                    "The AI Gateway Batch API does not support batch requests with type \""
                        + request.Type
                        + "\"."
                );
            }
        }
    }

    private static string ValidateSingleModel(IReadOnlyList<NormalizedBatchRequest> requests)
    {
        if (requests.Count == 0)
        {
            throw new InvalidArgumentException(
                "requests",
                requests,
                "The AI Gateway Batch API requires at least one request."
            );
        }

        var modelId = requests[0].ModelId;
        foreach (var request in requests)
        {
            if (request.ModelId != modelId)
            {
                throw new InvalidArgumentException(
                    "requests",
                    requests,
                    "The AI Gateway Batch API requires all requests in a batch to use the same model. Found \""
                        + modelId
                        + "\" and \""
                        + request.ModelId
                        + "\"."
                );
            }
        }

        return modelId;
    }

    // The key rides the idempotency-key header, the Gateway's replay contract for batch starts, and stays out of the body.
    private static string? GetGatewayBatchIdempotencyKey(JsonElement? providerOptions) =>
        providerOptions is { ValueKind: JsonValueKind.Object } options
        && options.TryGetProperty("gateway", out var gateway)
        && gateway.ValueKind == JsonValueKind.Object
        && gateway.TryGetProperty("idempotencyKey", out var key)
        && key.ValueKind == JsonValueKind.String
        && key.GetString() is { Length: > 0 } text
            ? text
            : null;

    private static JsonNode? OmitGatewayIdempotencyKey(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } options)
        {
            return null;
        }

        var node = JsonNode.Parse(options.GetRawText())!.AsObject();
        if (node["gateway"] is JsonObject gateway && gateway.ContainsKey("idempotencyKey"))
        {
            gateway.Remove("idempotencyKey");
            if (gateway.Count == 0)
            {
                node.Remove("gateway");
            }
        }

        return node.Count == 0 ? null : node;
    }
}
