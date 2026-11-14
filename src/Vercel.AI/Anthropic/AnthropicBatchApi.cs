// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic Message Batches API. Starts, inspects, cancels, and lists batches, and downloads the JSONL results of a finished batch.</summary>
public sealed class AnthropicBatchApi : BatchResultsApiBase
{
    private readonly AnthropicProvider _provider;

    internal AnthropicBatchApi(AnthropicProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.ModelProvider.Replace(".messages", string.Empty) + ".batch";

    /// <inheritdoc />
    public override bool CanCancel => true;

    /// <inheritdoc />
    public override bool CanList => true;

    /// <inheritdoc />
    public override async Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        foreach (var request in requests)
        {
            if (request.Type != "text")
            {
                throw new UnsupportedFunctionalityException("batch request type: " + request.Type, "The Anthropic Message Batches API does not support batch requests with type \"" + request.Type + "\".");
            }
        }

        AnthropicBatch.ValidateRequestIds(requests.Select(request => request.Id));

        var explicitBetas = GetAnthropicBatchProviderBetas(call.ProviderOptions);
        var batchBetas = new List<string>(explicitBetas);
        var warnings = new List<BatchRequestWarning>();
        if (call.WebhookUrl != null)
        {
            warnings.Add(new BatchRequestWarning(null, new OperationWarning("unsupported", feature: "webhookUrl", details: "The Anthropic Message Batches API does not support completion webhooks.")));
        }

        var prepared = new JsonArray();
        foreach (var request in requests)
        {
            var options = request.Options;
            var requestOptions = options.ValueKind == JsonValueKind.Object && options.TryGetProperty("providerOptions", out var po) ? po : (JsonElement?)null;
            if (GetAnthropicBatchProviderBetas(requestOptions).Count > 0)
            {
                throw new UnsupportedFunctionalityException(
                    "per-request providerOptions.anthropic.anthropicBeta",
                    "Anthropic Message Batches do not support per-request betas (request \"" + request.Id + "\"). Set providerOptions.anthropic.anthropicBeta on startBatch instead.");
            }

            var result = AnthropicMessagesRequest.Prepare(
                request.ModelId,
                OpenAIBatchApi.ToCallOptions(options, "Anthropic"),
                false,
                _provider.ModelProvider,
                _provider.SupportsNativeStructuredOutput,
                _provider.SupportsStrictTools,
                AnthropicProvider.ThreadSettings);
            if (result.Body["thread"] != null)
            {
                throw new UnsupportedFunctionalityException(
                    "message threads in batches",
                    "Anthropic Message Batches do not support message threads (request \"" + request.Id + "\").");
            }

            if (result.UsesJsonResponseTool)
            {
                throw new UnsupportedFunctionalityException(
                    "batch responseFormat JSON-tool fallback",
                    "Anthropic Message Batches cannot decode the JSON-tool structured-output fallback (request \"" + request.Id + "\") because batch results are retrieved independently of the start call. Use a model that supports native output_format structured outputs.");
            }

            var aliasedProviderTool = AliasedProviderToolName(result.ToolNames);
            if (aliasedProviderTool != null)
            {
                throw new UnsupportedFunctionalityException(
                    "aliased provider tool names in batches",
                    "Anthropic Message Batches cannot restore the custom provider-tool name \"" + aliasedProviderTool + "\" when results are retrieved independently of the start call (request \"" + request.Id + "\"). Use the provider's canonical tool name.");
            }

            var body = _provider.TransformRequestBody(result.Body, result.Betas);
            AnthropicBatch.ValidateAnthropicBatchBody(body, request.Id);
            prepared.Add(new JsonObject { ["custom_id"] = request.Id, ["params"] = body.DeepClone() });
            foreach (var beta in result.Betas)
            {
                if (!batchBetas.Contains(beta))
                {
                    batchBetas.Add(beta);
                }
            }

            foreach (var warning in result.Warnings)
            {
                warnings.Add(new BatchRequestWarning(request.Id, new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details)));
            }
        }

        var baseUrl = _provider.NormalizedBase().TrimEnd('/');
        var batchUri = new Uri(baseUrl + "/messages/batches");
        var payload = new JsonObject { ["requests"] = prepared }.ToJsonString();
        var callHeaders = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        var headers = _provider.UsesAws
            ? await _provider.CreateSignedHeadersAsync(batchBetas, callHeaders, payload, batchUri, cancellationToken).ConfigureAwait(false)
            : await _provider.CreateHeadersAsync(batchBetas, callHeaders, payload, cancellationToken).ConfigureAwait(false);
        var batch = await SendBatchAsync(HttpMethod.Post, batchUri, payload, headers, cancellationToken).ConfigureAwait(false);
        var started = Fill(new BatchStartResult { Id = Str(batch, "id")! }, batch);
        started.Warnings = warnings;
        return started;
    }

    /// <inheritdoc />
    public override async Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        return Fill(new BatchStatus(), await RetrieveBatchAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public override async Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var uri = GetBatchUrl("/" + Uri.EscapeDataString(call.BatchId ?? string.Empty) + "/cancel");
        var headers = await BatchHeadersAsync(call, "POST", "{}", uri, cancellationToken).ConfigureAwait(false);
        await SendBatchAsync(HttpMethod.Post, uri, "{}", headers, cancellationToken).ConfigureAwait(false);
        return new BatchCancelResult();
    }

    /// <inheritdoc />
    public override async Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (call.Limit is { } limit)
        {
            query.Add("limit=" + limit.ToString(CultureInfo.InvariantCulture));
        }

        if (call.Cursor != null)
        {
            query.Add("after_id=" + Uri.EscapeDataString(call.Cursor));
        }

        var uri = GetBatchUrl(string.Empty);
        if (query.Count > 0)
        {
            uri = new Uri(uri.AbsoluteUri + "?" + string.Join("&", query));
        }

        var headers = await BatchHeadersAsync(call, "GET", null, uri, cancellationToken).ConfigureAwait(false);
        JsonElement page;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, uri, null, headers, cancellationToken).ConfigureAwait(false))
        {
            page = document.RootElement.Clone();
        }

        if (!page.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array
            || !page.TryGetProperty("has_more", out var hasMore) || (hasMore.ValueKind != JsonValueKind.True && hasMore.ValueKind != JsonValueKind.False)
            || (page.TryGetProperty("last_id", out var lastIdValue) && lastIdValue.ValueKind != JsonValueKind.Null && lastIdValue.ValueKind != JsonValueKind.String))
        {
            throw new ApiException("Invalid JSON response", 200, page.GetRawText());
        }

        var batches = new List<ListedBatch>();
        foreach (var batch in data.EnumerateArray())
        {
            batches.Add(Fill(new ListedBatch { Id = Str(Validate(batch), "id")! }, batch));
        }

        var lastId = Str(page, "last_id");
        return new BatchListResult(batches, hasMore.GetBoolean() && lastId != null ? lastId : null);
    }

    // Upstream's batch response schema.
    private static JsonElement Validate(JsonElement batch)
    {
        var valid = Str(batch, "id") != null
            && Str(batch, "type") == "message_batch"
            && Str(batch, "processing_status") != null
            && Str(batch, "created_at") != null
            && Str(batch, "expires_at") != null
            && batch.TryGetProperty("request_counts", out var counts) && counts.ValueKind == JsonValueKind.Object
            && new[] { "processing", "succeeded", "errored", "canceled", "expired" }.All(name => counts.TryGetProperty(name, out var count) && count.ValueKind == JsonValueKind.Number)
            && new[] { "archived_at", "cancel_initiated_at", "ended_at", "results_url" }.All(name => !batch.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.String);
        if (!valid)
        {
            throw new ApiException("Invalid JSON response", 200, batch.GetRawText());
        }

        return batch;
    }

    private static T Fill<T>(T target, JsonElement batch)
        where T : BatchStatus
    {
        var rawStatus = Str(batch, "processing_status");
        var counts = AnthropicBatch.Count(batch);
        target.Status = AnthropicBatch.MapStatus(rawStatus);
        target.RawStatus = rawStatus;
        target.RequestCounts = new BatchRequestCounts(counts.Total, counts.Pending, counts.Completed, counts.Failed);
        target.CreatedAt = Str(batch, "created_at");
        target.ExpiresAt = Str(batch, "expires_at");
        var metadata = new JsonObject
        {
            ["anthropic"] = new JsonObject
            {
                ["archivedAt"] = Str(batch, "archived_at"),
                ["cancelInitiatedAt"] = Str(batch, "cancel_initiated_at"),
                ["endedAt"] = Str(batch, "ended_at"),
                ["requestCounts"] = JsonNode.Parse(batch.GetProperty("request_counts").GetRawText()),
                ["resultsUrl"] = Str(batch, "results_url"),
            },
        };
        using var document = JsonDocument.Parse(metadata.ToJsonString());
        target.ProviderMetadata = document.RootElement.Clone();
        return target;
    }

    // ToolNames maps the API name of every provider tool the request declares (tools or providerOptions.anthropic.providerTools) to the caller's name, so a pair that differs is an alias.
    private static string? AliasedProviderToolName(IReadOnlyDictionary<string, string> toolNames)
    {
        foreach (var pair in toolNames)
        {
            if (pair.Key != pair.Value)
            {
                return pair.Value;
            }
        }

        return null;
    }

    private Uri GetBatchUrl(string path) => new Uri(_provider.NormalizedBase().TrimEnd('/') + "/messages/batches" + path);

    private Task<Dictionary<string, string?>> BatchHeadersAsync(BatchOperationCall call, string method, string? body, Uri signingUri, CancellationToken cancellationToken)
    {
        var callHeaders = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        return _provider.UsesAws
            ? _provider.CreateSignedHeadersAsync(Array.Empty<string>(), callHeaders, body, signingUri, cancellationToken, method)
            : _provider.CreateHeadersAsync(Array.Empty<string>(), callHeaders, body, cancellationToken);
    }

    private async Task<JsonElement> SendBatchAsync(HttpMethod method, Uri uri, string? body, Dictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(method, uri, body, headers, cancellationToken).ConfigureAwait(false);
        return Validate(document.RootElement.Clone());
    }

    private async Task<JsonElement> RetrieveBatchAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var uri = GetBatchUrl("/" + Uri.EscapeDataString(call.BatchId ?? string.Empty));
        var headers = await BatchHeadersAsync(call, "GET", null, uri, cancellationToken).ConfigureAwait(false);
        return await SendBatchAsync(HttpMethod.Get, uri, null, headers, cancellationToken).ConfigureAwait(false);
    }

    private IReadOnlyList<string> GetAnthropicBatchProviderBetas(JsonElement? providerOptions)
    {
        var betas = new List<string>();
        if (providerOptions is not { ValueKind: JsonValueKind.Object } options)
        {
            return betas;
        }

        var name = _provider.ModelProvider.Split('.')[0];
        JsonElement? effective = null;
        foreach (var key in name == "anthropic" ? new[] { "anthropic" } : new[] { "anthropic", name })
        {
            // Custom provider options overlay the canonical ones.
            if (options.TryGetProperty(key, out var entry) && entry.ValueKind == JsonValueKind.Object
                && entry.TryGetProperty("anthropicBeta", out var list))
            {
                if (list.ValueKind != JsonValueKind.Array || list.EnumerateArray().Any(beta => beta.ValueKind != JsonValueKind.String))
                {
                    throw new InvalidArgumentException("providerOptions", entry, "invalid " + key + " provider options: anthropicBeta must be an array of strings");
                }

                effective = list;
            }
        }

        if (effective is { } array)
        {
            foreach (var beta in array.EnumerateArray())
            {
                if (beta.ValueKind == JsonValueKind.String && !betas.Contains(beta.GetString()!))
                {
                    betas.Add(beta.GetString()!);
                }
            }
        }

        return betas;
    }

    /// <inheritdoc />
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var baseUrl = _provider.NormalizedBase().TrimEnd('/');
        var batch = await RetrieveBatchAsync(call, cancellationToken).ConfigureAwait(false);

        if (AnthropicBatch.ResultsUnavailable(batch))
        {
            throw new InvalidArgumentException("batchId", batchId, "Anthropic batch \"" + batchId + "\" is not complete.");
        }

        if (batch.TryGetProperty("archived_at", out var archived) && archived.ValueKind != JsonValueKind.Null)
        {
            throw new InvalidArgumentException("batchId", batchId, "Anthropic batch \"" + batchId + "\" results are no longer available.");
        }

        var url = Str(batch, "results_url");
        if (url == null)
        {
            throw new AiSdkException("Anthropic batch \"" + batchId + "\" completed without batch output.");
        }

        var headers = await BatchHeadersAsync(call, "GET", null, new Uri(url), cancellationToken).ConfigureAwait(false);

        // The results URL comes from the response. Off the provider origin it must pass download validation and gets no credentials.
        if (!ProviderValues.IsSameOrigin(url, baseUrl))
        {
            DownloadUrls.ValidateDownloadUrl(url);
            headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["user-agent"] = headers.TryGetValue("user-agent", out var userAgent) ? userAgent : null };
        }

        var lines = await _provider.Http.OpenJsonLinesAsync(HttpMethod.Get, new Uri(url), null, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken).ConfigureAwait(false);
        return ConvertRows(lines, Convert, cancellationToken);
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "custom_id") ?? string.Empty;
        var result = line.TryGetProperty("result", out var value) ? value : default;
        switch (Str(result, "type"))
        {
            case "succeeded":
                if (!result.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.Object)
                {
                    return new BatchItem("text", id, "failed") { ErrorCode = "invalid_response", ErrorMessage = "Anthropic returned an invalid Message batch result." };
                }

                var parts = new List<BatchContentPart>();
                if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in content.EnumerateArray())
                    {
                        switch (Str(part, "type"))
                        {
                            case "text":
                                parts.Add(new BatchContentPart("text") { Text = Str(part, "text") ?? string.Empty });
                                break;
                            case "tool_use":
                                parts.Add(new BatchContentPart("tool-call")
                                {
                                    ToolCallId = Str(part, "id"),
                                    ToolName = Str(part, "name"),
                                    Text = part.TryGetProperty("input", out var input) ? input.GetRawText() : "null",
                                });
                                break;
                        }
                    }
                }

                var item = new BatchItem("text", id, "succeeded");
                item.Text = new BatchTextGeneration { Content = parts, RawFinishReason = Str(message, "stop_reason"), ResponseId = Str(message, "id"), ModelId = Str(message, "model") };
                return item;
            case "canceled":
                return new BatchItem("text", id, "cancelled");
            case "expired":
                return new BatchItem("text", id, "expired");
            default:
                return new BatchItem("text", id, "failed")
                {
                    ErrorMessage = result.ValueKind == JsonValueKind.Object && result.TryGetProperty("error", out var error) ? Str(error, "message") ?? Str(error.TryGetProperty("error", out var inner) ? inner : default, "message") : null,
                };
        }
    }
}
