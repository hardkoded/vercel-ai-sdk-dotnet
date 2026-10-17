// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI Batch API. Starts, reads, cancels, and lists Responses batches, and downloads their results.</summary>
public sealed class OpenAIBatchApi : BatchResultsApiBase
{
    private const string Endpoint = "/v1/responses";
    private const int DefaultInputFileExpiresAfterSeconds = 48 * 60 * 60;

    // Provider tools whose output the batch results conversion handles.
    private static readonly HashSet<string> ConvertibleProviderToolIds = new()
    {
        "openai.code_interpreter",
        "openai.custom",
        "openai.file_search",
        "openai.web_search",
        "openai.web_search_preview",
    };

    private readonly OpenAIProvider _provider;

    internal OpenAIBatchApi(OpenAIProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.Name + ".batch";

    /// <inheritdoc />
    public override bool CanCancel => true;

    /// <inheritdoc />
    public override bool CanList => true;

    /// <summary>
    /// Uploads the requests as a JSONL input file, then creates a <c>/v1/responses</c> batch with a 24-hour completion window.
    /// Every request must be a text request for the same model. <c>providerOptions.openai.inputFileExpiresAfter</c> sets the
    /// input file TTL in seconds (3600 to 2592000, default 48 hours).
    /// </summary>
    public override async Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        foreach (var request in requests)
        {
            if (request.Type != "text")
            {
                throw new UnsupportedFunctionalityException("batch request type: " + request.Type, "The OpenAI Batch API does not support batch requests with type \"" + request.Type + "\".");
            }
        }

        var modelId = requests.Count > 0 ? requests[0].ModelId : null;
        foreach (var request in requests)
        {
            if (request.ModelId != modelId)
            {
                throw new InvalidArgumentException("requests", request.ModelId, "The OpenAI Batch API requires all requests in a batch to use the same model. Found \"" + modelId + "\" and \"" + request.ModelId + "\".");
            }
        }

        var warnings = new List<BatchRequestWarning>();
        if (call.WebhookUrl != null)
        {
            warnings.Add(new BatchRequestWarning(null, new OperationWarning("unsupported", feature: "webhookUrl", details: "The OpenAI Batch API does not support per-batch webhook URLs.")));
        }

        var expiresAfterSeconds = InputFileExpiresAfter(call.ProviderOptions) ?? DefaultInputFileExpiresAfterSeconds;
        var file = new StringBuilder();
        foreach (var request in requests)
        {
            var callOptions = ToCallOptions(request.Options);
            var prepared = OpenAIResponsesLanguageModel.Prepare(request.ModelId, callOptions, stream: false);
            file.Append(new JsonObject
            {
                ["custom_id"] = request.Id,
                ["method"] = "POST",
                ["url"] = Endpoint,
                ["body"] = prepared.Body,
            }.ToJsonString()).Append('\n');
            foreach (var warning in prepared.Warnings)
            {
                warnings.Add(new BatchRequestWarning(request.Id, new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details)));
            }

            foreach (var tool in callOptions.Tools ?? Array.Empty<LanguageModelTool>())
            {
                if (tool.IsProviderDefined && !ConvertibleProviderToolIds.Contains(tool.ProviderToolId!))
                {
                    warnings.Add(new BatchRequestWarning(
                        request.Id,
                        new OperationWarning(
                            "unsupported",
                            feature: "batch result conversion for tool \"" + tool.Name + "\"",
                            details: "OpenAI may return output for this tool that AI SDK text batches cannot currently convert.")));
                }
            }
        }

        var headers = Headers(call);
        JsonElement upload;
        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(file.ToString()));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/jsonl");
            content.Add(fileContent, "file", "batch.jsonl");
            content.Add(new StringContent("batch"), "purpose");
            content.Add(new StringContent("created_at"), "expires_after[anchor]");
            content.Add(new StringContent(expiresAfterSeconds.ToString(CultureInfo.InvariantCulture)), "expires_after[seconds]");
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, Url("files"), content, headers, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(bytes);
            upload = document.RootElement.Clone();
        }

        var inputFileId = Str(upload, "id") ?? throw new ApiException("Invalid JSON response", 200, upload.GetRawText());
        var body = new JsonObject { ["input_file_id"] = inputFileId, ["endpoint"] = Endpoint, ["completion_window"] = "24h" }.ToJsonString();
        var batch = await SendBatchAsync(HttpMethod.Post, Url("batches"), body, headers, cancellationToken).ConfigureAwait(false);

        var metadata = new JsonObject { ["inputFileId"] = inputFileId };
        if (Timestamp(upload, "expires_at") is { } inputFileExpiresAt)
        {
            metadata["inputFileExpiresAt"] = inputFileExpiresAt;
        }

        var started = Fill(new BatchStartResult { Id = Str(batch, "id")!, Warnings = warnings }, batch);
        started.ProviderMetadata = OpenAIJson.ProviderMetadata(metadata);
        return started;
    }

    /// <inheritdoc />
    public override async Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        return Fill(new BatchStatus(), await RetrieveAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public override async Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        await SendBatchAsync(HttpMethod.Post, Url("batches/" + Uri.EscapeDataString(call.BatchId ?? string.Empty) + "/cancel"), "{}", Headers(call), cancellationToken).ConfigureAwait(false);
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
            query.Add("after=" + Uri.EscapeDataString(call.Cursor));
        }

        var url = Url("batches");
        if (query.Count > 0)
        {
            url = new Uri(url.AbsoluteUri + "?" + string.Join("&", query));
        }

        JsonElement page;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, url, null, Headers(call), cancellationToken).ConfigureAwait(false))
        {
            page = document.RootElement.Clone();
        }

        if (!page.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array
            || !page.TryGetProperty("has_more", out var hasMore) || (hasMore.ValueKind != JsonValueKind.True && hasMore.ValueKind != JsonValueKind.False))
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

    /// <inheritdoc />
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var batch = await RetrieveAsync(call, cancellationToken).ConfigureAwait(false);
        var status = MapStatus(Str(batch, "status"));
        if (status == "pending")
        {
            throw new InvalidArgumentException("batchId", batchId, "OpenAI batch \"" + batchId + "\" is not complete.");
        }

        var fileIds = new List<string>();
        foreach (var name in new[] { "output_file_id", "error_file_id" })
        {
            var fileId = Str(batch, name);
            if (fileId != null)
            {
                fileIds.Add(fileId);
            }
        }

        if (status == "completed" && fileIds.Count == 0)
        {
            throw new InvalidResponseDataException(batch, "OpenAI batch \"" + batchId + "\" completed without batch output.");
        }

        return ReadResultFiles(fileIds, Headers(call), cancellationToken);
    }

    // Each file downloads only when the caller reaches it, so a malformed row fails the stream after the rows before it.
    private async IAsyncEnumerable<BatchItem> ReadResultFiles(List<string> fileIds, Dictionary<string, string?> headers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var fileId in fileIds)
        {
            var uri = Url("files/" + Uri.EscapeDataString(fileId) + "/content");
            await foreach (var line in _provider.Http.SendJsonLinesAsync(HttpMethod.Get, uri, null, headers, _provider.MaxBatchLineBytes, cancellationToken).ConfigureAwait(false))
            {
                yield return Convert(line);
            }
        }
    }

    private Uri Url(string path) => ApiKeys.Combine(_provider.Options.BaseUrl, path);

    private Dictionary<string, string?> Headers(BatchOperationCall call) =>
        _provider.CreateOpenAIHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));

    private Task<JsonElement> RetrieveAsync(BatchOperationCall call, CancellationToken cancellationToken) =>
        SendBatchAsync(HttpMethod.Get, Url("batches/" + Uri.EscapeDataString(call.BatchId ?? string.Empty)), null, Headers(call), cancellationToken);

    private async Task<JsonElement> SendBatchAsync(HttpMethod method, Uri url, string? body, Dictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(method, url, body, headers, cancellationToken).ConfigureAwait(false);
        return Validate(document.RootElement.Clone());
    }

    // Upstream's batch response schema requires a string id and status.
    private static JsonElement Validate(JsonElement batch)
    {
        if (Str(batch, "id") == null || Str(batch, "status") == null)
        {
            throw new ApiException("Invalid JSON response", 200, batch.GetRawText());
        }

        return batch;
    }

    private static T Fill<T>(T target, JsonElement batch)
        where T : BatchStatus
    {
        var rawStatus = Str(batch, "status");
        target.Status = MapStatus(rawStatus);
        target.RawStatus = rawStatus;
        var counts = batch.TryGetProperty("request_counts", out var requestCounts) ? requestCounts : default;
        var total = Number(counts, "total");
        var completed = Number(counts, "completed");
        var failed = Number(counts, "failed");
        // Every count is at most the total, so a total that fits in int makes the casts safe.
        var normalized = ProviderValues.NormalizeBatchRequestCounts(total, total - completed - failed, completed, failed);
        target.RequestCounts = normalized == null || normalized.Total > int.MaxValue
            ? null
            : new BatchRequestCounts((int)normalized.Total, (int)normalized.Pending, (int)normalized.Completed, (int)normalized.Failed);
        if (batch.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object
            && errors.TryGetProperty("data", out var errorData) && errorData.ValueKind == JsonValueKind.Array && errorData.GetArrayLength() > 0)
        {
            var first = errorData[0];
            target.ErrorMessage = Str(first, "message") ?? "OpenAI batch failed.";
            target.ErrorCode = Str(first, "code");
        }

        target.CreatedAt = Timestamp(batch, "created_at");
        target.ExpiresAt = Timestamp(batch, "expires_at");
        return target;
    }

    // Unknown states are treated as pending so callers do not read incomplete results.
    private static string MapStatus(string? rawStatus) => rawStatus switch
    {
        "completed" => "completed",
        "failed" or "expired" or "cancelled" => "failed",
        _ => "pending",
    };

    private static double? Number(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;

    private static string? Timestamp(JsonElement element, string name)
    {
        // Outside DateTimeOffset's range, like an invalid JavaScript Date, there is no timestamp.
        if (Number(element, name) is not { } seconds || seconds * 1000 < -62135596800000 || seconds * 1000 > 253402300799999)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds((long)Math.Truncate(seconds * 1000)).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    private static int? InputFileExpiresAfter(JsonElement? providerOptions)
    {
        var openai = OpenAIJson.Child(providerOptions, "openai");
        if (OpenAIJson.Child(openai, "inputFileExpiresAfter") is not { } value || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var seconds = value.ValueKind == JsonValueKind.Number ? value.GetDouble() : double.NaN;
        if (!(seconds >= 3600 && seconds <= 2_592_000) || seconds != Math.Floor(seconds))
        {
            throw new InvalidArgumentException("providerOptions", openai, "invalid openai provider options: inputFileExpiresAfter must be an integer between 3600 and 2592000");
        }

        return (int)seconds;
    }

    // Reads the normalized options that Batch.StartBatchAsync builds for a text request.
    private static LanguageModelCallOptions ToCallOptions(JsonElement options)
    {
        var prompt = new List<ModelMessage>();
        if (OpenAIJson.Child(options, "prompt") is { ValueKind: JsonValueKind.Array } messages)
        {
            foreach (var message in messages.EnumerateArray())
            {
                prompt.Add(ToMessage(message));
            }
        }

        var result = new LanguageModelCallOptions
        {
            Prompt = prompt,
            MaxOutputTokens = OpenAIJson.Int(options, "maxOutputTokens"),
            Temperature = OpenAIJson.Double(options, "temperature"),
            TopP = OpenAIJson.Double(options, "topP"),
            TopK = OpenAIJson.Int(options, "topK"),
            PresencePenalty = OpenAIJson.Double(options, "presencePenalty"),
            FrequencyPenalty = OpenAIJson.Double(options, "frequencyPenalty"),
            Seed = OpenAIJson.Int(options, "seed"),
            Reasoning = OpenAIJson.String(options, "reasoning"),
        };
        if (OpenAIJson.Child(options, "stopSequences") is { ValueKind: JsonValueKind.Array } stops)
        {
            result.StopSequences = stops.EnumerateArray().Select(stop => stop.GetString() ?? string.Empty).ToList();
        }

        if (OpenAIJson.Child(options, "providerOptions") is { ValueKind: JsonValueKind.Object } providerOptions)
        {
            result.ProviderOptions = providerOptions.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
        }

        if (OpenAIJson.Child(options, "tools") is { ValueKind: JsonValueKind.Array } tools)
        {
            var converted = new List<LanguageModelTool>();
            foreach (var tool in tools.EnumerateArray())
            {
                if (Str(tool, "type") == "provider")
                {
                    converted.Add(LanguageModelTool.ProviderDefined(
                        Str(tool, "id") ?? string.Empty,
                        Str(tool, "name") ?? string.Empty,
                        tool.TryGetProperty("args", out var args) ? args.Clone() : OpenAIJson.Parse("{}")));
                    continue;
                }

                if (Str(tool, "type") != "function")
                {
                    throw new UnsupportedFunctionalityException("batch tool type: " + Str(tool, "type"), "OpenAI batches support only function and provider tools.");
                }

                converted.Add(new LanguageModelTool(Str(tool, "name") ?? string.Empty, Str(tool, "description"), tool.TryGetProperty("inputSchema", out var schema) ? schema.Clone() : OpenAIJson.Parse("{}")));
            }

            result.Tools = converted;
        }

        if (OpenAIJson.Child(options, "toolChoice") is { ValueKind: JsonValueKind.Object } choice)
        {
            result.ToolChoice = Str(choice, "type") switch
            {
                "auto" => ToolChoice.Auto,
                "none" => ToolChoice.None,
                "required" => ToolChoice.Required,
                "tool" => ToolChoice.Tool(Str(choice, "toolName") ?? string.Empty),
                var type => throw new InvalidArgumentException("toolChoice", type, "Unsupported tool choice type \"" + type + "\"."),
            };
        }

        return result;
    }

    private static ModelMessage ToMessage(JsonElement message)
    {
        var role = Str(message, "role");
        var content = OpenAIJson.Child(message, "content");
        if (role == "system" && content is { ValueKind: JsonValueKind.String } system)
        {
            return new SystemModelMessage(system.GetString()!);
        }

        if (role == "user" && content is { ValueKind: JsonValueKind.Array } parts)
        {
            var converted = new List<UserContentPart>();
            foreach (var part in parts.EnumerateArray())
            {
                if (Str(part, "type") != "text")
                {
                    throw new UnsupportedFunctionalityException("batch prompt part: " + Str(part, "type"), "OpenAI batches support only text prompt parts.");
                }

                converted.Add(new TextContentPart(Str(part, "text") ?? string.Empty));
            }

            return new UserModelMessage(converted);
        }

        throw new UnsupportedFunctionalityException("batch prompt message: " + role, "OpenAI batches support only system and user text messages.");
    }

    private static BatchItem Convert(JsonElement line)
    {
        if (!OpenAIResponsesSchema.IsValidResultLine(line))
        {
            throw new TypeValidationException("Type validation failed: Value: " + line.GetRawText() + ".", line);
        }

        var id = Str(line, "custom_id") ?? string.Empty;
        if (line.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var code = Str(error, "code");
            var status = code == "batch_cancelled" ? "cancelled" : code == "batch_expired" ? "expired" : "failed";
            return new BatchItem("text", id, status) { ErrorCode = code, ErrorMessage = Str(error, "message") };
        }

        if (!line.TryGetProperty("response", out var response) || response.ValueKind != JsonValueKind.Object)
        {
            return new BatchItem("text", id, "failed") { ErrorCode = "invalid_batch_result", ErrorMessage = "OpenAI returned a batch result without a response or error." };
        }

        response.TryGetProperty("body", out var body);
        if (!response.TryGetProperty("status_code", out var codeElement) || !codeElement.TryGetDouble(out var statusCode))
        {
            return new BatchItem("text", id, "failed") { ErrorCode = "invalid_batch_result", ErrorMessage = "OpenAI returned a batch result without a response or error." };
        }

        if (statusCode < 200 || statusCode >= 300)
        {
            return ConvertErrorResponse(id, body, (int)statusCode);
        }

        // Upstream converts only the Responses API shape for OpenAI batches.
        if (!OpenAIResponsesSchema.IsValidResponse(body))
        {
            return InvalidResponse(id, "OpenAI returned an invalid Responses batch result.");
        }

        if (body.TryGetProperty("error", out var bodyError) && bodyError.ValueKind == JsonValueKind.Object)
        {
            return new BatchItem("text", id, "failed") { ErrorCode = Str(bodyError, "code"), ErrorType = Str(bodyError, "type"), ErrorMessage = Str(bodyError, "message") };
        }

        body.TryGetProperty("output", out var output);
        var incompleteReason = body.TryGetProperty("incomplete_details", out var details) ? Str(details, "reason") : null;
        if (output.ValueKind == JsonValueKind.Undefined)
        {
            return InvalidResponse(id, incompleteReason != null ? "OpenAI Responses returned no output (" + incompleteReason + ")." : "OpenAI Responses returned no output.");
        }

        var content = new List<BatchContentPart>();
        var logprobs = new JsonArray();
        var hasFunctionCall = false;
        foreach (var entry in output.EnumerateArray())
        {
            var type = Str(entry, "type");
            switch (type)
            {
                case "reasoning":
                    var summaries = entry.GetProperty("summary").EnumerateArray().Select(summary => Str(summary, "text") ?? string.Empty).ToList();
                    if (summaries.Count == 0)
                    {
                        summaries.Add(string.Empty);
                    }

                    foreach (var summary in summaries)
                    {
                        content.Add(new BatchContentPart("reasoning")
                        {
                            Text = summary,
                            ProviderMetadata = OpenAIJson.ProviderMetadata(new JsonObject
                            {
                                ["itemId"] = Str(entry, "id"),
                                ["reasoningEncryptedContent"] = Str(entry, "encrypted_content"),
                            }),
                        });
                    }

                    break;
                case "message":
                    foreach (var part in entry.GetProperty("content").EnumerateArray())
                    {
                        content.Add(new BatchContentPart("text") { Text = Str(part, "text") ?? string.Empty });
                        if (part.TryGetProperty("logprobs", out var partLogprobs) && partLogprobs.ValueKind != JsonValueKind.Null)
                        {
                            logprobs.Add(Node(partLogprobs));
                        }
                    }

                    break;
                case "function_call":
                    hasFunctionCall = true;
                    var metadata = new JsonObject { ["itemId"] = Str(entry, "id") };
                    if (Str(entry, "namespace") is { } ns)
                    {
                        metadata["namespace"] = ns;
                    }

                    if (entry.TryGetProperty("caller", out var caller) && caller.ValueKind == JsonValueKind.Object)
                    {
                        metadata["caller"] = Str(caller, "type") == "program"
                            ? new JsonObject { ["type"] = "program", ["callerId"] = Str(caller, "caller_id") }
                            : Node(caller);
                    }

                    content.Add(new BatchContentPart("tool-call") { ToolCallId = Str(entry, "call_id"), ToolName = Str(entry, "name"), Text = Str(entry, "arguments"), ProviderMetadata = OpenAIJson.ProviderMetadata(metadata) });
                    break;
                case "custom_tool_call":
                    hasFunctionCall = true;
                    content.Add(new BatchContentPart("tool-call")
                    {
                        ToolCallId = Str(entry, "call_id"),
                        ToolName = Str(entry, "name"),
                        Text = Str(entry, "input") is { } customInput ? JsonSerializer.Serialize(customInput) : "null",
                        ProviderMetadata = OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = Str(entry, "id") }),
                    });
                    break;
                case "web_search_call":
                    var searchId = Str(entry, "id");
                    AddProviderToolCall(content, searchId, "web_search", "{}");
                    var searchStatus = Str(entry, "status");
                    var searchResult = new BatchContentPart("tool-result") { ToolCallId = searchId, ToolName = "web_search", Dynamic = true };
                    if (searchStatus is "failed" or "incomplete")
                    {
                        searchResult.IsError = true;
                        searchResult.Output = OpenAIJson.Element(new JsonObject { ["status"] = searchStatus });
                    }
                    else
                    {
                        searchResult.Output = OpenAIJson.Element(MapWebSearchOutput(entry.TryGetProperty("action", out var action) ? action : default));
                    }

                    content.Add(searchResult);
                    break;
                case "file_search_call":
                    var fileSearchId = Str(entry, "id");
                    AddProviderToolCall(content, fileSearchId, "file_search", "{}");
                    var files = entry.TryGetProperty("results", out var fileResults) && fileResults.ValueKind == JsonValueKind.Array ? new JsonArray() : null;
                    if (files != null)
                    {
                        foreach (var result in fileResults.EnumerateArray())
                        {
                            files.Add(new JsonObject
                            {
                                ["attributes"] = result.TryGetProperty("attributes", out var attributes) ? Node(attributes) : null,
                                ["fileId"] = Str(result, "file_id"),
                                ["filename"] = Str(result, "filename"),
                                ["score"] = result.TryGetProperty("score", out var score) ? Node(score) : null,
                                ["text"] = Str(result, "text"),
                            });
                        }
                    }

                    content.Add(new BatchContentPart("tool-result")
                    {
                        ToolCallId = fileSearchId,
                        ToolName = "file_search",
                        Dynamic = true,
                        Output = OpenAIJson.Element(new JsonObject { ["queries"] = entry.TryGetProperty("queries", out var queries) ? Node(queries) : null, ["results"] = files }),
                    });
                    break;
                case "code_interpreter_call":
                    var interpreterId = Str(entry, "id");
                    AddProviderToolCall(content, interpreterId, "code_interpreter", OpenAIJson.Element(new JsonObject { ["code"] = Str(entry, "code"), ["containerId"] = Str(entry, "container_id") }).GetRawText());
                    content.Add(new BatchContentPart("tool-result")
                    {
                        ToolCallId = interpreterId,
                        ToolName = "code_interpreter",
                        Dynamic = true,
                        Output = OpenAIJson.Element(new JsonObject { ["outputs"] = entry.TryGetProperty("outputs", out var outputs) ? Node(outputs) : null }),
                    });
                    break;
                default:
                    return new BatchItem("text", id, "failed") { ErrorCode = "unsupported_content", ErrorMessage = "OpenAI returned an unsupported \"" + type + "\" output item in an AI SDK text batch." };
            }
        }

        var openai = new JsonObject { ["responseId"] = Str(body, "id") };
        if (logprobs.Count > 0)
        {
            openai["logprobs"] = logprobs;
        }

        if (Str(body, "service_tier") is { } serviceTier)
        {
            openai["serviceTier"] = serviceTier;
        }

        if (body.TryGetProperty("reasoning", out var reasoning) && reasoning.ValueKind == JsonValueKind.Object
            && reasoning.TryGetProperty("context", out var reasoningContext) && reasoningContext.ValueKind != JsonValueKind.Null)
        {
            openai["reasoningContext"] = Node(reasoningContext);
        }

        var item = new BatchItem("text", id, "succeeded");
        item.Text = new BatchTextGeneration
        {
            Content = content,
            FinishReason = incompleteReason switch
            {
                null => hasFunctionCall ? "tool-calls" : "stop",
                "max_output_tokens" => "length",
                "content_filter" => "content-filter",
                _ => hasFunctionCall ? "tool-calls" : "other",
            },
            RawFinishReason = incompleteReason,
            Usage = ConvertUsage(body),
            ResponseId = Str(body, "id"),
            Timestamp = body.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.Number && created.TryGetInt64(out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : null,
            ModelId = Str(body, "model"),
            ProviderMetadata = OpenAIJson.ProviderMetadata(openai),
        };
        return item;
    }

    private static JsonNode? Node(JsonElement element) => JsonNode.Parse(element.GetRawText());

    private static void AddProviderToolCall(List<BatchContentPart> content, string? id, string name, string input)
    {
        content.Add(new BatchContentPart("tool-call") { ToolCallId = id, ToolName = name, Text = input, ProviderExecuted = true, Dynamic = true });
    }

    private static JsonObject MapWebSearchOutput(JsonElement action)
    {
        if (action.ValueKind != JsonValueKind.Object)
        {
            return new JsonObject();
        }

        switch (Str(action, "type"))
        {
            case "search":
                var search = new JsonObject { ["type"] = "search" };
                if (Str(action, "query") is { } query)
                {
                    search["query"] = query;
                }

                if (action.TryGetProperty("queries", out var queries) && queries.ValueKind != JsonValueKind.Null)
                {
                    search["queries"] = Node(queries);
                }

                var result = new JsonObject { ["action"] = search };
                if (action.TryGetProperty("sources", out var sources) && sources.ValueKind != JsonValueKind.Null)
                {
                    result["sources"] = Node(sources);
                }

                return result;
            case "open_page":
                return new JsonObject { ["action"] = new JsonObject { ["type"] = "openPage", ["url"] = Str(action, "url") } };
            case "find_in_page":
                return new JsonObject { ["action"] = new JsonObject { ["type"] = "findInPage", ["url"] = Str(action, "url"), ["pattern"] = Str(action, "pattern") } };
            default:
                return new JsonObject();
        }
    }

    private static BatchModelUsage? ConvertUsage(JsonElement body)
    {
        if (!body.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var input = Int(usage, "input_tokens");
        var output = Int(usage, "output_tokens");
        var cached = usage.TryGetProperty("input_tokens_details", out var inputDetails) ? Int(inputDetails, "cached_tokens") : null;
        var reasoning = usage.TryGetProperty("output_tokens_details", out var outputDetails) ? Int(outputDetails, "reasoning_tokens") : null;
        return new BatchModelUsage(
            input,
            input == null ? (int?)null : input - (cached ?? 0),
            cached ?? (input == null ? (int?)null : 0),
            null,
            output,
            output == null ? (int?)null : output - (reasoning ?? 0),
            reasoning ?? (output == null ? (int?)null : 0),
            usage.Clone());
    }

    private static int? Int(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    }

    private static BatchItem ConvertErrorResponse(string id, JsonElement body, int statusCode)
    {
        var item = new BatchItem("text", id, "failed") { ErrorStatusCode = statusCode };
        if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object && Str(error, "message") is { } message)
        {
            item.ErrorMessage = message;
            item.ErrorType = Str(error, "type");
            item.ErrorCode = error.TryGetProperty("code", out var code) && code.ValueKind is JsonValueKind.String or JsonValueKind.Number ? (code.ValueKind == JsonValueKind.String ? code.GetString() : code.GetRawText()) : null;
        }
        else
        {
            item.ErrorMessage = "OpenAI batch request failed with status code " + statusCode + ".";
        }

        return item;
    }

    private static BatchItem InvalidResponse(string id, string message)
    {
        return new BatchItem("text", id, "failed") { ErrorCode = "invalid_response", ErrorMessage = message };
    }
}
