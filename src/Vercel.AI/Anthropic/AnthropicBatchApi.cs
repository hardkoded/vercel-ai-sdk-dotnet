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
        var headers = await BatchHeadersAsync(call, "{}", uri, cancellationToken).ConfigureAwait(false);
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

        var headers = await BatchHeadersAsync(call, null, uri, cancellationToken).ConfigureAwait(false);
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

    // Like upstream's AWS fetch wrapper, only POST requests with a body are signed. GETs go out unsigned.
    private Task<Dictionary<string, string?>> BatchHeadersAsync(BatchOperationCall call, string? body, Uri signingUri, CancellationToken cancellationToken)
    {
        var callHeaders = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        return _provider.UsesAws
            ? _provider.CreateSignedHeadersAsync(Array.Empty<string>(), callHeaders, body, signingUri, cancellationToken)
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
        var headers = await BatchHeadersAsync(call, null, uri, cancellationToken).ConfigureAwait(false);
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

        var headers = await BatchHeadersAsync(call, null, new Uri(url), cancellationToken).ConfigureAwait(false);

        // The results URL comes from the response. Off the provider origin it must pass download validation and gets no credentials.
        if (!ProviderValues.IsSameOrigin(url, baseUrl))
        {
            DownloadUrls.ValidateDownloadUrl(url);
            headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["user-agent"] = headers.TryGetValue("user-agent", out var userAgent) ? userAgent : null };
        }

        var lines = await _provider.Http.OpenJsonLinesAsync(HttpMethod.Get, new Uri(url), null, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken).ConfigureAwait(false);
        var generateId = _provider.Options.GenerateId ?? (() => JsonValues.GenerateId(string.Empty));
        return ConvertRows(lines, line => Convert(line, generateId), cancellationToken);
    }

    private static readonly HashSet<string> s_knownContentTypes =
    [
        "advisor_tool_result", "bash_code_execution_tool_result", "code_execution_tool_result", "compaction", "container_upload",
        "fallback", "mcp_tool_result", "mcp_tool_use", "redacted_thinking", "server_tool_use", "text",
        "text_editor_code_execution_tool_result", "thinking", "tool_search_tool_result", "tool_use", "web_fetch_tool_result", "web_search_tool_result",
    ];

    private static BatchItem Convert(JsonElement line, Func<string> generateId)
    {
        var id = Str(line, "custom_id") ?? string.Empty;
        var result = line.TryGetProperty("result", out var value) ? value : default;
        switch (Str(result, "type"))
        {
            case "canceled":
                return new BatchItem("text", id, "cancelled");
            case "expired":
                return new BatchItem("text", id, "expired");
            case "errored":
                if (result.TryGetProperty("error", out var envelope) && envelope.ValueKind == JsonValueKind.Object
                    && Str(envelope, "type") == "error"
                    && envelope.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object
                    && Str(error, "type") is { } errorType && Str(error, "message") is { } errorMessage)
                {
                    var failed = new BatchItem("text", id, "failed") { ErrorMessage = errorMessage, ErrorType = errorType };
                    if (Str(envelope, "request_id") is { } requestId)
                    {
                        failed.ProviderMetadata = OperationJson.Serialize(new Dictionary<string, object> { ["anthropic"] = new Dictionary<string, string> { ["requestId"] = requestId } });
                    }

                    return failed;
                }

                return InvalidResult(id);
            case "succeeded":
                if (!result.TryGetProperty("message", out var message) || ParseResponse(message) is not { } response)
                {
                    return InvalidResult(id);
                }

                return new BatchItem("text", id, "succeeded") { Text = ConvertResponse(response, generateId) };
            default:
                return InvalidResult(id);
        }
    }

    private static BatchItem InvalidResult(string id) =>
        new("text", id, "failed") { ErrorCode = "invalid_response", ErrorMessage = "Anthropic returned an invalid Message batch result." };

    /// <summary>Validates a message like <c>anthropicResponseSchema</c>. Unknown content blocks are skipped. A malformed known block invalidates the message.</summary>
    private static JsonElement? ParseResponse(JsonElement message)
    {
        if (message.ValueKind != JsonValueKind.Object || Str(message, "type") != "message"
            || !message.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array
            || !message.TryGetProperty("usage", out var usage) || !IsValidUsage(usage))
        {
            return null;
        }

        foreach (var part in content.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object || Str(part, "type") is not { } type)
            {
                return null;
            }

            if (s_knownContentTypes.Contains(type) && !IsValidPart(type, part))
            {
                return null;
            }
        }

        return message;
    }

    private static bool IsValidUsage(JsonElement usage) =>
        usage.ValueKind == JsonValueKind.Object
        && usage.TryGetProperty("input_tokens", out var input) && input.ValueKind == JsonValueKind.Number
        && usage.TryGetProperty("output_tokens", out var output) && output.ValueKind == JsonValueKind.Number;

    private static bool IsValidPart(string type, JsonElement part)
    {
        switch (type)
        {
            case "text":
                return Str(part, "text") != null && (!part.TryGetProperty("citations", out var citations) || citations.ValueKind is JsonValueKind.Null or JsonValueKind.Array);
            case "thinking":
                return Str(part, "thinking") != null && Str(part, "signature") != null;
            case "redacted_thinking":
                return Str(part, "data") != null;
            case "compaction":
                return NullableString(part, "content") && NullableString(part, "signature");
            case "tool_use":
            case "server_tool_use":
                return Str(part, "id") != null && Str(part, "name") != null;
            case "mcp_tool_use":
                return Str(part, "id") != null && Str(part, "name") != null && Str(part, "server_name") != null;
            case "mcp_tool_result":
                return Str(part, "tool_use_id") != null && part.TryGetProperty("is_error", out var isError) && isError.ValueKind is JsonValueKind.True or JsonValueKind.False && part.TryGetProperty("content", out _);
            case "container_upload":
                return Str(part, "file_id") != null;
            case "fallback":
                return true;
            case "web_fetch_tool_result":
                return Str(part, "tool_use_id") != null && part.TryGetProperty("content", out var fetch) && fetch.ValueKind == JsonValueKind.Object
                    && (Str(fetch, "type") == "web_fetch_tool_result_error"
                        ? Str(fetch, "error_code") != null
                        : Str(fetch, "type") == "web_fetch_result" && Str(fetch, "url") != null && fetch.TryGetProperty("content", out var document) && document.ValueKind == JsonValueKind.Object
                            && document.TryGetProperty("source", out var source) && source.ValueKind == JsonValueKind.Object);
            case "web_search_tool_result":
                if (Str(part, "tool_use_id") == null || !part.TryGetProperty("content", out var search))
                {
                    return false;
                }

                if (search.ValueKind == JsonValueKind.Array)
                {
                    return search.EnumerateArray().All(result => Str(result, "url") != null && Str(result, "encrypted_content") != null && NullableString(result, "title"));
                }

                return search.ValueKind == JsonValueKind.Object && Str(search, "error_code") != null;
            case "advisor_tool_result":
                return Str(part, "tool_use_id") != null && part.TryGetProperty("content", out var advisor) && advisor.ValueKind == JsonValueKind.Object
                    && Str(advisor, "type") is "advisor_result" or "advisor_redacted_result" or "advisor_tool_result_error";
            default:
                return Str(part, "tool_use_id") != null && part.TryGetProperty("content", out var body) && body.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
    }

    private static bool NullableString(JsonElement element, string name) =>
        !element.TryGetProperty(name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.String;

    private static BatchTextGeneration ConvertResponse(JsonElement? validated, Func<string> generateId)
    {
        var response = validated!.Value;
        var parts = new List<BatchContentPart>();
        var mcpToolCalls = new Dictionary<string, BatchContentPart>();
        var serverToolCalls = new Dictionary<string, string>();
        var contentParts = response.GetProperty("content").EnumerateArray().Where(part => s_knownContentTypes.Contains(Str(part, "type")!)).ToList();
        var hasWebSearchResults = contentParts.Any(part => Str(part, "type") == "web_search_tool_result" && part.GetProperty("content").ValueKind == JsonValueKind.Array && part.GetProperty("content").GetArrayLength() > 0);

        foreach (var part in contentParts)
        {
            switch (Str(part, "type"))
            {
                case "text":
                    ConvertText(part, parts, hasWebSearchResults, generateId);
                    break;
                case "thinking":
                    parts.Add(new BatchContentPart("reasoning") { Text = Str(part, "thinking"), ProviderMetadata = Metadata(new JsonObject { ["signature"] = Str(part, "signature") }) });
                    break;
                case "redacted_thinking":
                    parts.Add(new BatchContentPart("reasoning") { Text = string.Empty, ProviderMetadata = Metadata(new JsonObject { ["redactedData"] = Str(part, "data") }) });
                    break;
                case "container_upload":
                    parts.Add(new BatchContentPart("custom") { Kind = "anthropic.container_upload", ProviderMetadata = Metadata(new JsonObject { ["fileId"] = Str(part, "file_id") }) });
                    break;
                case "compaction":
                    if (!string.IsNullOrEmpty(Str(part, "content")))
                    {
                        var compaction = new JsonObject { ["type"] = "compaction" };
                        if (Str(part, "signature") is { } signature)
                        {
                            compaction["signature"] = signature;
                        }

                        parts.Add(new BatchContentPart("text") { Text = Str(part, "content"), ProviderMetadata = Metadata(compaction) });
                    }

                    break;
                case "tool_use":
                    parts.Add(WithCaller(new BatchContentPart("tool-call") { ToolCallId = Str(part, "id"), ToolName = Str(part, "name"), Text = Raw(part, "input") }, part));
                    break;
                case "server_tool_use":
                    parts.Add(ConvertServerToolUse(part, serverToolCalls));
                    break;
                case "mcp_tool_use":
                    var toolCall = new BatchContentPart("tool-call")
                    {
                        ToolCallId = Str(part, "id"),
                        ToolName = Str(part, "name"),
                        Text = Raw(part, "input"),
                        ProviderExecuted = true,
                        Dynamic = true,
                        ProviderMetadata = Metadata(new JsonObject { ["serverName"] = Str(part, "server_name"), ["type"] = "mcp-tool-use" }),
                    };
                    mcpToolCalls[toolCall.ToolCallId!] = toolCall;
                    parts.Add(toolCall);
                    break;
                case "mcp_tool_result":
                    mcpToolCalls.TryGetValue(Str(part, "tool_use_id")!, out var mcpCall);
                    parts.Add(new BatchContentPart("tool-result")
                    {
                        ToolCallId = Str(part, "tool_use_id"),
                        ToolName = mcpCall?.ToolName ?? "mcp",
                        IsError = part.GetProperty("is_error").GetBoolean(),
                        Output = part.GetProperty("content").Clone(),
                        Dynamic = true,
                        ProviderMetadata = mcpCall?.ProviderMetadata,
                    });
                    break;
                case "web_fetch_tool_result":
                    parts.Add(WithCaller(ConvertWebFetch(part), part));
                    break;
                case "web_search_tool_result":
                    ConvertWebSearch(part, parts, generateId);
                    break;
                case "code_execution_tool_result":
                    parts.Add(Result(part, "code_execution", part.GetProperty("content"), Str(part.GetProperty("content"), "type") == "code_execution_tool_result_error"));
                    break;
                case "bash_code_execution_tool_result":
                case "text_editor_code_execution_tool_result":
                    parts.Add(new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = "code_execution", Output = part.GetProperty("content").Clone() });
                    break;
                case "tool_search_tool_result":
                    parts.Add(ConvertToolSearch(part, serverToolCalls));
                    break;
                case "advisor_tool_result":
                    parts.Add(ConvertAdvisor(part));
                    break;
                case "fallback":
                    var fallback = new JsonObject { ["type"] = "fallback" };
                    foreach (var name in new[] { "from", "to" })
                    {
                        if (part.TryGetProperty(name, out var endpoint))
                        {
                            fallback[name] = JsonNode.Parse(endpoint.GetRawText());
                        }
                    }

                    parts.Add(new BatchContentPart("custom") { Kind = "anthropic.fallback", ProviderMetadata = Metadata(fallback) });
                    break;
            }
        }

        var stopReason = Str(response, "stop_reason");
        var usage = AnthropicUsage.Convert(response.GetProperty("usage"));
        return new BatchTextGeneration
        {
            Content = parts,
            FinishReason = AnthropicStopReason.Map(stopReason) switch
            {
                FinishReason.Stop => "stop",
                FinishReason.Length => "length",
                FinishReason.ContentFilter => "content-filter",
                FinishReason.ToolCalls => "tool-calls",
                _ => "other",
            },
            RawFinishReason = stopReason,
            Usage = new BatchModelUsage(usage.InputTokens, usage.NoCacheInputTokens, usage.CacheReadTokens, usage.CacheWriteTokens, usage.OutputTokens, usage.TextTokens, usage.ReasoningTokens, usage.Raw),
            ResponseId = Str(response, "id"),
            ModelId = Str(response, "model"),
            ProviderMetadata = MessageMetadata(response),
        };
    }

    private static void ConvertText(JsonElement part, List<BatchContentPart> parts, bool hasWebSearchResults, Func<string> generateId)
    {
        var citations = part.TryGetProperty("citations", out var citationArray) && citationArray.ValueKind == JsonValueKind.Array ? citationArray.EnumerateArray().ToList() : [];
        var text = new BatchContentPart("text") { Text = Str(part, "text") };
        if (citations.Count > 0)
        {
            text.Citations = AnthropicCitations.MapAll(citations);
            text.ProviderMetadata = AnthropicCitations.TextMetadata(citations);
        }

        parts.Add(text);
        foreach (var citation in citations)
        {
            if (Str(citation, "type") != "web_search_result_location" || hasWebSearchResults)
            {
                continue;
            }

            var metadata = new JsonObject { ["citedText"] = Str(citation, "cited_text"), ["encryptedIndex"] = Str(citation, "encrypted_index") };
            parts.Add(UrlSource(generateId(), Str(citation, "url") ?? string.Empty, Str(citation, "title"), Metadata(metadata)));
        }
    }

    private static BatchContentPart UrlSource(string id, string url, string? title, JsonElement? metadata) =>
        new("source") { SourceType = "url", SourceId = id, Url = url, Title = title, ProviderMetadata = metadata };

    private static BatchContentPart ConvertServerToolUse(JsonElement part, Dictionary<string, string> serverToolCalls)
    {
        var name = Str(part, "name")!;
        var id = Str(part, "id")!;
        var isCodeExecutionAlias = name is "bash_code_execution" or "text_editor_code_execution";
        var isCodeExecution = isCodeExecutionAlias || name == "code_execution";
        if (name is "tool_search_tool_bm25" or "tool_search_tool_regex")
        {
            serverToolCalls[id] = name;
        }

        JsonNode? input = part.TryGetProperty("input", out var inputElement) && inputElement.ValueKind != JsonValueKind.Null ? AnthropicJson.Node(inputElement) : null;
        if (isCodeExecutionAlias)
        {
            input = WithType(name, input);
        }
        else if (name == "code_execution" && input is JsonObject code && code["code"] != null && code["type"] == null)
        {
            input = WithType("programmatic-tool-call", code);
        }

        return WithCaller(
            new BatchContentPart("tool-call")
            {
                ToolCallId = id,
                ToolName = isCodeExecutionAlias ? "code_execution" : name,
                Text = Stringify(input),
                ProviderExecuted = true,

                // Batch results are retrieved without the original request tools, so implicitly provisioned code execution must remain self-describing.
                Dynamic = isCodeExecution ? true : null,
            },
            part);
    }

    private static JsonObject WithType(string type, JsonNode? input)
    {
        var wrapped = new JsonObject { ["type"] = type };
        if (input is JsonObject source)
        {
            foreach (var property in source)
            {
                wrapped[property.Key] = property.Value?.DeepClone();
            }
        }

        return wrapped;
    }

    private static BatchContentPart ConvertWebFetch(JsonElement part)
    {
        var body = part.GetProperty("content");
        if (Str(body, "type") == "web_fetch_tool_result_error")
        {
            return Result(part, "web_fetch", body, true);
        }

        var document = body.GetProperty("content");
        var source = document.GetProperty("source");
        var content = new JsonObject();
        if (document.TryGetProperty("citations", out var documentCitations))
        {
            content["citations"] = AnthropicJson.Node(documentCitations);
        }

        content["source"] = new JsonObject { ["data"] = Str(source, "data"), ["mediaType"] = Str(source, "media_type"), ["type"] = Str(source, "type") };
        content["title"] = Str(document, "title");
        content["type"] = Str(document, "type");
        var result = new JsonObject { ["content"] = content, ["retrievedAt"] = Str(body, "retrieved_at"), ["type"] = Str(body, "type"), ["url"] = Str(body, "url") };
        return new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = "web_fetch", Output = Element(result) };
    }

    private static void ConvertWebSearch(JsonElement part, List<BatchContentPart> parts, Func<string> generateId)
    {
        var body = part.GetProperty("content");
        if (body.ValueKind != JsonValueKind.Array)
        {
            parts.Add(WithCaller(Result(part, "web_search", body, true), part));
            return;
        }

        var results = new JsonArray();
        foreach (var result in body.EnumerateArray())
        {
            var item = new JsonObject
            {
                ["encryptedContent"] = Str(result, "encrypted_content"),
                ["pageAge"] = Str(result, "page_age"),
            };
            if (Str(result, "title") is { } title)
            {
                item["title"] = title;
            }

            item["type"] = Str(result, "type");
            item["url"] = Str(result, "url");
            results.Add(item);
        }

        parts.Add(WithCaller(new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = "web_search", Output = Element(results) }, part));
        foreach (var result in body.EnumerateArray())
        {
            parts.Add(UrlSource(generateId(), Str(result, "url") ?? string.Empty, Str(result, "title"), Metadata(new JsonObject { ["pageAge"] = Str(result, "page_age") })));
        }
    }

    private static BatchContentPart ConvertToolSearch(JsonElement part, Dictionary<string, string> serverToolCalls)
    {
        var toolName = serverToolCalls.TryGetValue(Str(part, "tool_use_id")!, out var name) ? name : "tool_search_tool_regex";
        var body = part.GetProperty("content");
        if (Str(body, "type") == "tool_search_tool_result_error")
        {
            return Result(part, toolName, body, true);
        }

        var references = new JsonArray();
        foreach (var reference in body.GetProperty("tool_references").EnumerateArray())
        {
            references.Add(new JsonObject { ["toolName"] = Str(reference, "tool_name"), ["type"] = Str(reference, "type") });
        }

        return new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = toolName, Output = Element(references) };
    }

    private static BatchContentPart ConvertAdvisor(JsonElement part)
    {
        var body = part.GetProperty("content");
        var type = Str(body, "type");
        if (type == "advisor_tool_result_error")
        {
            return Result(part, "advisor", body, true);
        }

        var result = new JsonObject { ["type"] = type };
        if (type == "advisor_result")
        {
            result["text"] = Str(body, "text");
        }
        else
        {
            result["encryptedContent"] = Str(body, "encrypted_content");
        }

        if (Str(body, "stop_reason") is { } stopReason)
        {
            result["stopReason"] = stopReason;
        }

        return new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = "advisor", Output = Element(result) };
    }

    /// <summary>A tool result. An error body becomes <c>{ errorCode, type }</c>. Any other body is kept as received.</summary>
    private static BatchContentPart Result(JsonElement part, string toolName, JsonElement body, bool isError)
    {
        var result = new BatchContentPart("tool-result") { ToolCallId = Str(part, "tool_use_id"), ToolName = toolName };
        if (isError)
        {
            result.IsError = true;
            result.Output = Element(new JsonObject { ["errorCode"] = Str(body, "error_code"), ["type"] = Str(body, "type") });
        }
        else
        {
            result.Output = body.Clone();
        }

        return result;
    }

    private static BatchContentPart WithCaller(BatchContentPart target, JsonElement part)
    {
        if (part.TryGetProperty("caller", out var caller) && caller.ValueKind == JsonValueKind.Object)
        {
            var info = new JsonObject { ["type"] = Str(caller, "type") };
            if (Str(caller, "tool_id") is { } toolId)
            {
                info["toolId"] = toolId;
            }

            target.ProviderMetadata = Metadata(new JsonObject { ["caller"] = info });
        }

        return target;
    }

    private static JsonElement MessageMetadata(JsonElement response)
    {
        var usage = response.GetProperty("usage");
        var metadata = new JsonObject
        {
            ["usage"] = AnthropicJson.Node(usage),
            ["stopSequence"] = Str(response, "stop_sequence"),
        };
        if ((Str(usage, "service_tier") ?? Str(response, "service_tier")) is { } serviceTier)
        {
            metadata["serviceTier"] = serviceTier;
        }

        if (response.TryGetProperty("stop_details", out var stopDetails) && stopDetails.ValueKind == JsonValueKind.Object)
        {
            var details = new JsonObject { ["type"] = Str(stopDetails, "type") };
            CopyString(stopDetails, "category", details, "category");
            CopyString(stopDetails, "explanation", details, "explanation");
            CopyString(stopDetails, "recommended_model", details, "recommendedModel");
            metadata["stopDetails"] = details;
        }

        if (response.TryGetProperty("input_transformations", out var transformations) && transformations.ValueKind != JsonValueKind.Null)
        {
            metadata["inputTransformations"] = AnthropicJson.Node(transformations);
        }

        if (response.TryGetProperty("safeguard_results", out var safeguardResults) && safeguardResults.ValueKind != JsonValueKind.Null)
        {
            metadata["safeguardResults"] = AnthropicJson.Node(safeguardResults);
        }

        metadata["iterations"] = usage.TryGetProperty("iterations", out var iterations) && iterations.ValueKind == JsonValueKind.Array ? MapIterations(iterations) : null;
        metadata["container"] = response.TryGetProperty("container", out var container) && container.ValueKind == JsonValueKind.Object ? MapContainer(container) : null;
        metadata["contextManagement"] = response.TryGetProperty("context_management", out var contextManagement) && contextManagement.ValueKind == JsonValueKind.Object ? MapContextManagement(contextManagement) : null;
        return Metadata(metadata)!.Value;
    }

    private static JsonArray MapIterations(JsonElement iterations)
    {
        var mapped = new JsonArray();
        foreach (var iteration in iterations.EnumerateArray())
        {
            var item = new JsonObject { ["type"] = Str(iteration, "type") };
            CopyString(iteration, "model", item, "model");
            item["inputTokens"] = AnthropicJson.Node(iteration.GetProperty("input_tokens"));
            item["outputTokens"] = AnthropicJson.Node(iteration.GetProperty("output_tokens"));
            CopyTruthyNumber(iteration, "cache_creation_input_tokens", item, "cacheCreationInputTokens");
            CopyTruthyNumber(iteration, "cache_read_input_tokens", item, "cacheReadInputTokens");
            mapped.Add(item);
        }

        return mapped;
    }

    private static JsonObject MapContainer(JsonElement container)
    {
        JsonNode? skills = null;
        if (container.TryGetProperty("skills", out var skillArray) && skillArray.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var skill in skillArray.EnumerateArray())
            {
                mapped.Add(new JsonObject { ["type"] = Str(skill, "type"), ["skillId"] = Str(skill, "skill_id"), ["version"] = Str(skill, "version") });
            }

            skills = mapped;
        }

        return new JsonObject { ["expiresAt"] = Str(container, "expires_at"), ["id"] = Str(container, "id"), ["skills"] = skills };
    }

    private static JsonObject MapContextManagement(JsonElement contextManagement)
    {
        var edits = new JsonArray();
        foreach (var edit in contextManagement.GetProperty("applied_edits").EnumerateArray())
        {
            var type = Str(edit, "type");
            var mapped = new JsonObject { ["type"] = type };
            switch (type)
            {
                case "clear_tool_uses_20250919":
                    mapped["clearedToolUses"] = AnthropicJson.Node(edit.GetProperty("cleared_tool_uses"));
                    mapped["clearedInputTokens"] = AnthropicJson.Node(edit.GetProperty("cleared_input_tokens"));
                    break;
                case "clear_thinking_20251015":
                    mapped["clearedThinkingTurns"] = AnthropicJson.Node(edit.GetProperty("cleared_thinking_turns"));
                    mapped["clearedInputTokens"] = AnthropicJson.Node(edit.GetProperty("cleared_input_tokens"));
                    break;
            }

            edits.Add(mapped);
        }

        return new JsonObject { ["appliedEdits"] = edits };
    }

    private static void CopyString(JsonElement source, string name, JsonObject target, string targetName)
    {
        if (Str(source, name) is { } value)
        {
            target[targetName] = value;
        }
    }

    private static void CopyTruthyNumber(JsonElement source, string name, JsonObject target, string targetName)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.GetDouble() != 0)
        {
            target[targetName] = AnthropicJson.Node(value);
        }
    }

    private static JsonElement? Metadata(JsonObject anthropic) =>
        Element(new JsonObject { ["anthropic"] = anthropic });

    private static JsonElement Element(JsonNode node)
    {
        using var document = JsonDocument.Parse(Stringify(node));
        return document.RootElement.Clone();
    }

    private static string Raw(JsonElement part, string name) =>
        part.TryGetProperty(name, out var value) ? Stringify(AnthropicJson.Node(value)) : "null";

    private static string Stringify(JsonNode? node) =>
        node == null ? "null" : node.ToJsonString(s_compact);

    private static readonly JsonSerializerOptions s_compact = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}
