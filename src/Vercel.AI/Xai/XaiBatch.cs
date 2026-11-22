// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>
/// xAI Batch API. Uploads the requests as a JSONL file, creates a file-backed batch, and pages through its results.
/// Maps to <c>XaiBatch</c>.
/// </summary>
public sealed class XaiBatch : BatchResultsApiBase
{
    private const string BatchEndpoint = "/v1/responses";
    private const string BatchName = "ai-sdk-text-batch";
    private const int ResultsPageSize = 1000;

    private readonly XaiProvider _provider;

    internal XaiBatch(XaiProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => "xai.batch";

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> SupportedUrls =>
        XaiResponsesLanguageModel.ResponsesSupportedUrls;

    /// <inheritdoc />
    public override bool CanCancel => true;

    /// <inheritdoc />
    public override bool CanList => true;

    /// <summary>
    /// Uploads the requests as <c>batch.jsonl</c>, then creates a batch from the uploaded file.
    /// <c>providerOptions.xai.inputFileExpiresAfter</c> sets the input file TTL in seconds (3600 to 2592000).
    /// </summary>
    public override async Task<BatchStartResult> DoStartAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        foreach (var request in requests)
        {
            if (request.Type != "text" && request.Type != "image")
            {
                throw new UnsupportedFunctionalityException(
                    "batch request type: " + request.Type,
                    "The xAI Batch API does not support batch requests with type \""
                        + request.Type
                        + "\"."
                );
            }
        }

        var warnings = new List<BatchRequestWarning>();
        if (call.WebhookUrl != null)
        {
            warnings.Add(
                new BatchRequestWarning(
                    null,
                    new OperationWarning(
                        "unsupported",
                        feature: "webhookUrl",
                        details: "The xAI Batch API does not support per-batch webhook URLs."
                    )
                )
            );
        }

        var expiresAfter = InputFileExpiresAfter(call.ProviderOptions);
        var file = new StringBuilder();
        foreach (var request in requests)
        {
            var prepared = PrepareRequest(request);
            file.Append(
                    new JsonObject
                    {
                        ["custom_id"] = request.Id,
                        ["method"] = "POST",
                        ["url"] = prepared.Endpoint,
                        ["body"] = prepared.Body,
                    }.ToJsonString()
                )
                .Append('\n');
            foreach (var warning in prepared.Warnings)
            {
                warnings.Add(new BatchRequestWarning(request.Id, warning));
            }
        }

        var uploaded = await _provider
            .Files()
            .UploadAsync(
                new XaiFileUpload
                {
                    Bytes = Encoding.UTF8.GetBytes(file.ToString()),
                    FileName = "batch.jsonl",
                    MediaType = "application/jsonl",
                    ExpiresAfter = expiresAfter,
                    Headers = call.Headers.ToDictionary(pair => pair.Key, pair => pair.Value),
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        var inputFileId = uploaded.ProviderReference.TryGetValue("xai", out var id)
            ? id
            : throw new ApiException("Invalid JSON response", 200, null);

        var batch = await SendBatchAsync(
                HttpMethod.Post,
                Url("/batches"),
                new JsonObject
                {
                    ["name"] = BatchName,
                    ["input_file_id"] = inputFileId,
                }.ToJsonString(),
                call,
                cancellationToken
            )
            .ConfigureAwait(false);

        var metadata = new JsonObject { ["inputFileId"] = inputFileId };
        if (uploaded.Metadata.ExpiresAt is { } expiresAt)
        {
            metadata["inputFileExpiresAt"] = DateTimeOffset
                .FromUnixTimeMilliseconds(expiresAt * 1000)
                .UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }

        var started = Fill(
            new BatchStartResult { Id = Str(batch, "batch_id")!, Warnings = warnings },
            batch
        );
        started.ProviderMetadata = OpenAIJson.ProviderMetadata(metadata, "xai");
        return started;
    }

    /// <inheritdoc />
    public override async Task<BatchStatus> DoGetStatusAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        return Fill(
            new BatchStatus(),
            await RetrieveBatchAsync(call, cancellationToken).ConfigureAwait(false)
        );
    }

    /// <inheritdoc />
    public override async Task<BatchCancelResult> DoCancelAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        await SendBatchAsync(
                HttpMethod.Post,
                Url("/batches/" + Uri.EscapeDataString(call.BatchId ?? string.Empty) + ":cancel"),
                "{}",
                call,
                cancellationToken
            )
            .ConfigureAwait(false);
        return new BatchCancelResult();
    }

    /// <inheritdoc />
    public override async Task<BatchListResult> DoListAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var query = new List<string>();
        if (call.Limit is { } limit)
        {
            query.Add("limit=" + limit.ToString(CultureInfo.InvariantCulture));
        }

        if (call.Cursor != null)
        {
            query.Add("pagination_token=" + Uri.EscapeDataString(call.Cursor));
        }

        var url = Url("/batches");
        if (query.Count > 0)
        {
            url = new Uri(url.AbsoluteUri + "?" + string.Join("&", query));
        }

        var page = await GetJsonAsync(url, call, cancellationToken).ConfigureAwait(false);
        if (!page.TryGetProperty("batches", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            throw new ApiException("Invalid JSON response", 200, page.GetRawText());
        }

        var batches = new List<ListedBatch>();
        foreach (var batch in data.EnumerateArray())
        {
            batches.Add(
                Fill(new ListedBatch { Id = Str(ValidateBatch(batch), "batch_id")! }, batch)
            );
        }

        return new BatchListResult(batches, Str(page, "pagination_token"));
    }

    /// <inheritdoc />
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var batch = await RetrieveBatchAsync(call, cancellationToken).ConfigureAwait(false);
        if (Fill(new BatchStatus(), batch).Status == "pending")
        {
            throw new InvalidArgumentException(
                "batchId",
                call.BatchId,
                "xAI batch \"" + call.BatchId + "\" is not complete."
            );
        }

        return IterateBatchResults(call, cancellationToken);
    }

    private static T Fill<T>(T target, JsonElement batch)
        where T : BatchStatus
    {
        var state = batch.TryGetProperty("state", out var value) ? value : default;
        var numError = Number(state, "num_error");
        var numCancelled = Number(state, "num_cancelled");
        var counts = ProviderValues.NormalizeBatchRequestCounts(
            Number(state, "num_requests"),
            Number(state, "num_pending"),
            Number(state, "num_success"),
            numError != null && numCancelled != null ? numError + numCancelled : null
        );
        var requestCounts =
            counts == null || counts.Total > int.MaxValue
                ? null
                : new BatchRequestCounts(
                    (int)counts.Total,
                    (int)counts.Pending,
                    (int)counts.Completed,
                    (int)counts.Failed
                );
        var isCancelled =
            Str(batch, "cancel_time") != null || Str(batch, "cancel_by_xai_message") != null;
        var isExpired = IsPastDate(Str(batch, "expire_time"));
        var batchId = Str(batch, "batch_id");
        target.Status =
            isCancelled || isExpired ? "failed"
            : requestCounts != null && requestCounts.Total > 0 && requestCounts.Pending == 0
                ? "completed"
            : "pending";
        target.RequestCounts = requestCounts;
        if (isCancelled)
        {
            target.ErrorMessage = Str(batch, "cancel_by_xai_message")
                ?? "xAI batch \"" + batchId + "\" was cancelled.";
            target.ErrorCode = "batch_cancelled";
        }
        else if (isExpired)
        {
            target.ErrorMessage = "xAI batch \"" + batchId + "\" expired.";
            target.ErrorCode = "batch_expired";
        }

        target.CreatedAt = Str(batch, "create_time");
        target.ExpiresAt = Str(batch, "expire_time");
        return target;
    }

    private static bool IsPastDate(string? value)
    {
        return value != null
            && DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var timestamp
            )
            && timestamp <= DateTimeOffset.UtcNow;
    }

    private static double? Number(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;
    }

    private static int? InputFileExpiresAfter(JsonElement? providerOptions)
    {
        var xai = OpenAIJson.Child(providerOptions, "xai");
        if (
            OpenAIJson.Child(xai, "inputFileExpiresAfter") is not { } value
            || value.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        var seconds = value.ValueKind == JsonValueKind.Number ? value.GetDouble() : double.NaN;
        if (!(seconds >= 3600 && seconds <= 2_592_000) || seconds != Math.Floor(seconds))
        {
            throw new InvalidArgumentException(
                "providerOptions",
                xai,
                "invalid xai provider options: inputFileExpiresAfter must be an integer between 3600 and 2592000"
            );
        }

        return (int)seconds;
    }

    // Upstream's batch response schema requires a string batch_id.
    private static JsonElement ValidateBatch(JsonElement batch)
    {
        if (Str(batch, "batch_id") == null)
        {
            throw new ApiException("Invalid JSON response", 200, batch.GetRawText());
        }

        return batch;
    }

    private Uri Url(string path) => ApiKeys.Combine(_provider.Options.BaseUrl, path);

    private Task<Dictionary<string, string?>> HeadersAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    ) =>
        _provider.CreateHeadersAsync(
            call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            cancellationToken
        );

    private async Task<JsonElement> SendBatchAsync(
        HttpMethod method,
        Uri url,
        string? body,
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                method,
                url,
                body == null ? null : ProviderExchange.Json(body),
                await HeadersAsync(call, cancellationToken).ConfigureAwait(false),
                cancellationToken,
                XaiError.FailedResponseHandler
            )
            .ConfigureAwait(false);
        return Validate(response.Body);
    }

    private async Task<JsonElement> GetJsonAsync(
        Uri url,
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Get,
                url,
                null,
                await HeadersAsync(call, cancellationToken).ConfigureAwait(false),
                cancellationToken,
                XaiError.FailedResponseHandler
            )
            .ConfigureAwait(false);
        return ParseJson(response.Body);
    }

    private async Task<JsonElement> RetrieveBatchAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        return ValidateBatch(
            await GetJsonAsync(
                    Url("/batches/" + Uri.EscapeDataString(call.BatchId ?? string.Empty)),
                    call,
                    cancellationToken
                )
                .ConfigureAwait(false)
        );
    }

    private static JsonElement Validate(string body)
    {
        return ValidateBatch(ParseJson(body));
    }

    private static JsonElement ParseJson(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, body);
        }
    }

    private async IAsyncEnumerable<BatchItem> IterateBatchResults(
        BatchOperationCall call,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        string? paginationToken = null;
        do
        {
            var query = "limit=" + ResultsPageSize.ToString(CultureInfo.InvariantCulture);
            if (paginationToken != null)
            {
                query += "&pagination_token=" + Uri.EscapeDataString(paginationToken);
            }

            var page = await GetJsonAsync(
                    Url(
                        "/batches/"
                            + Uri.EscapeDataString(call.BatchId ?? string.Empty)
                            + "/results?"
                            + query
                    ),
                    call,
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (
                !page.TryGetProperty("results", out var results)
                || results.ValueKind != JsonValueKind.Array
                || results.EnumerateArray().Any(result => !IsValidResult(result))
            )
            {
                throw new ApiException("Invalid JSON response", 200, page.GetRawText());
            }

            foreach (var result in results.EnumerateArray())
            {
                yield return await ConvertBatchResultAsync(result, call, cancellationToken)
                    .ConfigureAwait(false);
            }

            paginationToken = Str(page, "pagination_token");
        } while (paginationToken != null);
    }

    private async Task<BatchItem> ConvertBatchResultAsync(
        JsonElement result,
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var id = Str(result, "batch_request_id")!;
        var batchResult = result.TryGetProperty("batch_result", out var value) ? value : default;
        var error =
            batchResult.ValueKind == JsonValueKind.Object
            && batchResult.TryGetProperty("error", out var errorValue)
            && errorValue.ValueKind == JsonValueKind.Object
                ? errorValue
                : default;
        var hasCode = HasErrorCode(error);
        if (
            !string.IsNullOrEmpty(Str(result, "error_message"))
            || hasCode
            || (!HasCodeProperty(error) && !string.IsNullOrEmpty(Str(error, "message")))
        )
        {
            var item = new BatchItem(
                "text",
                id,
                IsCancellationError(error) ? "cancelled" : "failed"
            )
            {
                ErrorMessage = NullIfEmpty(Str(result, "error_message"))
                    ?? NullIfEmpty(Str(error, "message"))
                    ?? "xAI batch request failed.",
            };
            if (hasCode)
            {
                item.ErrorCode =
                    error.GetProperty("code").ValueKind == JsonValueKind.String
                        ? error.GetProperty("code").GetString()
                        : error.GetProperty("code").GetRawText();
            }

            return item;
        }

        var response =
            batchResult.ValueKind == JsonValueKind.Object
            && batchResult.TryGetProperty("response", out var responseValue)
            && responseValue.ValueKind == JsonValueKind.Object
                ? responseValue
                : default;
        if (
            response.ValueKind == JsonValueKind.Object
            && response.TryGetProperty("chat_get_completion", out var completion)
            && completion.ValueKind != JsonValueKind.Null
        )
        {
            if (!IsValidTextResponse(completion))
            {
                return InvalidResult(id, "text", "xAI returned an invalid Responses batch result.");
            }

            return ConvertTextResponse(id, completion);
        }

        if (
            response.ValueKind == JsonValueKind.Object
            && response.TryGetProperty("image_generation", out var generation)
            && generation.ValueKind != JsonValueKind.Null
        )
        {
            if (!IsValidImageResponse(generation))
            {
                return InvalidResult(id, "image", "xAI returned an invalid image batch result.");
            }

            if (
                generation
                    .GetProperty("data")
                    .EnumerateArray()
                    .Any(image =>
                        image.TryGetProperty("respect_moderation", out var moderation)
                        && moderation.ValueKind == JsonValueKind.False
                    )
            )
            {
                return new BatchItem("image", id, "failed")
                {
                    ErrorMessage =
                        "Image generation was blocked due to a content policy violation.",
                };
            }

            return new BatchItem("image", id, "succeeded")
            {
                Image = await ConvertImageBatchResponseAsync(generation, call, cancellationToken)
                    .ConfigureAwait(false),
            };
        }

        return InvalidResult(id, "text", "xAI returned an invalid Responses batch result.");
    }

    private static bool HasCodeProperty(JsonElement error)
    {
        return error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("code", out var code)
            && code.ValueKind != JsonValueKind.Null;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    // A code other than 0 or "0" is an error. A string code stays as is, a number keeps its text.
    private static bool HasErrorCode(JsonElement error)
    {
        if (error.ValueKind != JsonValueKind.Object || !error.TryGetProperty("code", out var code))
        {
            return false;
        }

        return code.ValueKind switch
        {
            JsonValueKind.String => code.GetString() != "0",
            JsonValueKind.Number => code.GetDouble() != 0,
            _ => false,
        };
    }

    private static bool IsCancellationError(JsonElement error)
    {
        if (error.ValueKind != JsonValueKind.Object || !error.TryGetProperty("code", out var code))
        {
            return false;
        }

        if (code.ValueKind == JsonValueKind.Number)
        {
            return code.GetDouble() == 1;
        }

        if (code.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var normalized = code.GetString()!.ToLowerInvariant();
        return normalized is "1" or "cancelled" or "batch_cancelled";
    }

    private static BatchItem InvalidResult(string id, string type, string message)
    {
        return new BatchItem(type, id, "failed")
        {
            ErrorCode = "invalid_response",
            ErrorMessage = message,
        };
    }

    private static BatchItem ConvertTextResponse(string id, JsonElement response)
    {
        if (Str(response, "error") is { } responseError)
        {
            return new BatchItem("text", id, "failed")
            {
                ErrorMessage = responseError,
                ErrorCode = Str(response, "code"),
            };
        }

        if (
            !response.TryGetProperty("choices", out var choiceList)
            || choiceList.ValueKind != JsonValueKind.Array
            || choiceList.GetArrayLength() == 0
        )
        {
            return new BatchItem("text", id, "failed")
            {
                ErrorCode = "invalid_response",
                ErrorMessage = "xAI returned a batch response without any choices.",
            };
        }

        var choices = choiceList.EnumerateArray().ToList();
        var content = new List<BatchContentPart>();
        var providerExecutedToolCallIds = new HashSet<string>(
            choices
                .Where(choice => Str(choice.GetProperty("message"), "role") == "tool")
                .SelectMany(choice => ToolCalls(choice.GetProperty("message")))
                .Select(toolCall => Str(toolCall, "id")!)
        );
        JsonElement? lastAssistantChoice = null;

        foreach (var choice in choices)
        {
            var message = choice.GetProperty("message");
            if (Str(message, "role") == "tool")
            {
                if (Str(message, "content") is { } toolContent)
                {
                    foreach (var toolCall in ToolCalls(message))
                    {
                        content.Add(
                            new BatchContentPart("tool-result")
                            {
                                ToolCallId = Str(toolCall, "id"),
                                ToolName = Str(toolCall.GetProperty("function"), "name"),
                                Output = OpenAIJson.Element(JsonValue.Create(toolContent)!),
                                Dynamic = true,
                            }
                        );
                    }
                }

                continue;
            }

            lastAssistantChoice = choice;

            if (!string.IsNullOrEmpty(Str(message, "content")))
            {
                content.Add(new BatchContentPart("text") { Text = Str(message, "content") });
            }

            if (!string.IsNullOrEmpty(Str(message, "reasoning_content")))
            {
                content.Add(
                    new BatchContentPart("reasoning") { Text = Str(message, "reasoning_content") }
                );
            }

            foreach (var toolCall in ToolCalls(message))
            {
                var function = toolCall.GetProperty("function");
                var part = new BatchContentPart("tool-call")
                {
                    ToolCallId = Str(toolCall, "id"),
                    ToolName = Str(function, "name"),
                    Text = Str(function, "arguments"),
                };
                if (providerExecutedToolCallIds.Contains(Str(toolCall, "id")!))
                {
                    part.ProviderExecuted = true;
                    part.Dynamic = true;
                }

                content.Add(part);
            }
        }

        if (
            response.TryGetProperty("citations", out var citations)
            && citations.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var citation in citations.EnumerateArray())
            {
                var url = citation.GetString()!;
                content.Add(
                    new BatchContentPart("source")
                    {
                        SourceType = "url",
                        SourceId = url,
                        Url = url,
                    }
                );
            }
        }

        var rawFinishReason = lastAssistantChoice is { } last ? Str(last, "finish_reason") : null;
        var usage =
            response.TryGetProperty("usage", out var usageValue)
            && usageValue.ValueKind == JsonValueKind.Object
                ? ConvertTextUsage(usageValue)
                : null;
        var costInUsdTicks = response.TryGetProperty("usage", out var usageForCost)
            ? Number(usageForCost, "cost_in_usd_ticks")
            : null;
        var serviceTier = Str(response, "service_tier");
        var generation = new BatchTextGeneration
        {
            Content = content,
            FinishReason = MapXaiFinishReason.Map(rawFinishReason),
            RawFinishReason = rawFinishReason,
            Usage = usage ?? new BatchModelUsage(),
            ResponseId = Str(response, "id"),
            // Like an invalid JavaScript Date, a value outside DateTimeOffset's range gives no timestamp.
            Timestamp =
                Number(response, "created") is { } created
                && created * 1000 >= -62135596800000
                && created * 1000 <= 253402300799999
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long)(created * 1000)).UtcDateTime
                    : null,
            ModelId = Str(response, "model"),
        };
        if (costInUsdTicks != null || serviceTier != null)
        {
            var xai = new JsonObject();
            if (costInUsdTicks != null)
            {
                xai["costInUsdTicks"] = costInUsdTicks;
            }

            if (serviceTier != null)
            {
                xai["serviceTier"] = serviceTier;
            }

            generation.ProviderMetadata = OpenAIJson.ProviderMetadata(xai, "xai");
        }

        return new BatchItem("text", id, "succeeded") { Text = generation };
    }

    private static IEnumerable<JsonElement> ToolCalls(JsonElement message)
    {
        return
            message.TryGetProperty("tool_calls", out var toolCalls)
            && toolCalls.ValueKind == JsonValueKind.Array
            ? toolCalls.EnumerateArray()
            : Enumerable.Empty<JsonElement>();
    }

    // Maps to convertXaiBatchTextUsage.
    private static BatchModelUsage ConvertTextUsage(JsonElement usage)
    {
        var promptTokens = (int)Number(usage, "prompt_tokens")!.Value;
        var completionTokens = (int)Number(usage, "completion_tokens")!.Value;
        var cacheReadTokens = usage.TryGetProperty("prompt_tokens_details", out var promptDetails)
            ? (int)(Number(promptDetails, "cached_tokens") ?? 0)
            : 0;
        var reasoningTokens = usage.TryGetProperty(
            "completion_tokens_details",
            out var completionDetails
        )
            ? (int)(Number(completionDetails, "reasoning_tokens") ?? 0)
            : 0;
        var promptTokensIncludesCached = cacheReadTokens <= promptTokens;
        return new BatchModelUsage(
            promptTokensIncludesCached ? promptTokens : promptTokens + cacheReadTokens,
            promptTokensIncludesCached ? promptTokens - cacheReadTokens : promptTokens,
            cacheReadTokens,
            null,
            completionTokens + reasoningTokens,
            completionTokens,
            reasoningTokens,
            usage.Clone()
        );
    }

    private async Task<BatchImageGeneration> ConvertImageBatchResponseAsync(
        JsonElement response,
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var data = response.GetProperty("data").EnumerateArray().ToList();
        var images = new List<object?>();
        var hasAllBase64 = data.All(image => Str(image, "b64_json") != null);
        foreach (var image in data)
        {
            if (hasAllBase64)
            {
                images.Add(Str(image, "b64_json"));
            }
            else if (Str(image, "b64_json") is { } base64)
            {
                images.Add(System.Convert.FromBase64String(base64));
            }
            else if (Str(image, "url") is { } url)
            {
                var baseUrl = _provider.Options.BaseUrl;
                var download = await ProviderExchange
                    .GetResponseUrlAsync(
                        _provider.SharedHttpClient,
                        url,
                        baseUrl,
                        Vercel.AI.GenerateText.DownloadUrls.ValidateDownloadUrl,
                        null,
                        cancellationToken,
                        ProviderExchange.DownloadClient(_provider.Options),
                        null,
                        _provider.Options.DownloadHttpClient == null
                            ? Vercel.AI.Util.Download.Fetch
                            : null,
                        XaiError.FailedResponseHandler
                    )
                    .ConfigureAwait(false);
                images.Add(download.Bytes);
            }
            else
            {
                throw new InvalidArgumentException(
                    "batchResult",
                    image,
                    "xAI returned an image without data or a URL."
                );
            }
        }

        var xai = new JsonObject
        {
            ["images"] = new JsonArray(
                data.Select(image =>
                        (JsonNode)(
                            Str(image, "revised_prompt") is { } revisedPrompt
                                ? new JsonObject { ["revisedPrompt"] = revisedPrompt }
                                : new JsonObject()
                        )
                    )
                    .ToArray()
            ),
        };
        if (
            response.TryGetProperty("usage", out var usage)
            && Number(usage, "cost_in_usd_ticks") is { } cost
        )
        {
            xai["costInUsdTicks"] = cost;
        }

        return new BatchImageGeneration
        {
            Images = images,
            Warnings = Array.Empty<OperationWarning>(),
            Response = new ProviderResponse(timestamp: DateTime.UtcNow, modelId: string.Empty),
            ProviderMetadata = OpenAIJson.ProviderMetadata(xai, "xai"),
        };
    }

    private PreparedBatchRequest PrepareRequest(NormalizedBatchRequest request)
    {
        if (request.Type == "text")
        {
            var prepared = XaiResponsesLanguageModel.PrepareRequest(
                request.ModelId,
                OpenAIBatchApi.ToCallOptions(request.Options, "xAI"),
                stream: false
            );
            return new PreparedBatchRequest(
                BatchEndpoint,
                prepared.Body,
                prepared
                    .Warnings.Select(warning => new OperationWarning(
                        warning.Type,
                        warning.Feature == null ? warning.Message : null,
                        warning.Feature,
                        warning.Feature != null && warning.Message != warning.Feature
                            ? warning.Message
                            : null,
                        warning.Setting
                    ))
                    .ToList()
            );
        }

        var options = request.Options;
        var prompt = Str(options, "prompt");
        var n =
            options.TryGetProperty("n", out var nValue) && nValue.ValueKind == JsonValueKind.Number
                ? nValue.GetInt32()
                : 1;
        var size = Str(options, "size");
        var aspectRatio = Str(options, "aspectRatio");
        var warnings = new List<OperationWarning>();
        if (size != null)
        {
            warnings.Add(
                new OperationWarning(
                    "unsupported",
                    feature: "size",
                    details: "This model does not support the `size` option. Use `aspectRatio` instead."
                )
            );
        }

        if (IsPresent(options, "seed"))
        {
            warnings.Add(new OperationWarning("unsupported", feature: "seed"));
        }

        if (IsPresent(options, "mask"))
        {
            warnings.Add(new OperationWarning("unsupported", feature: "mask"));
        }

        var xaiOptions = XaiImageModelOptions.Parse(OpenAIJson.Child(options, "providerOptions"));
        var imageUrls = new List<string>();
        if (
            options.TryGetProperty("files", out var files)
            && files.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var file in files.EnumerateArray())
            {
                imageUrls.Add(
                    FileDataConversions.ConvertImageModelFileToDataUri(ToImageModelFile(file))
                );
            }
        }

        var body = new JsonObject
        {
            ["model"] = request.ModelId,
            ["prompt"] = prompt,
            ["n"] = n,
            ["response_format"] = "b64_json",
        };
        if (aspectRatio != null)
        {
            body["aspect_ratio"] = aspectRatio;
        }

        if (xaiOptions.OutputFormat != null)
        {
            body["output_format"] = xaiOptions.OutputFormat;
        }

        if (xaiOptions.SyncMode != null)
        {
            body["sync_mode"] = xaiOptions.SyncMode;
        }

        if (xaiOptions.AspectRatio != null && aspectRatio == null)
        {
            body["aspect_ratio"] = xaiOptions.AspectRatio;
        }

        if (xaiOptions.Resolution != null)
        {
            body["resolution"] = xaiOptions.Resolution;
        }

        if (xaiOptions.Quality != null)
        {
            body["quality"] = xaiOptions.Quality;
        }

        if (xaiOptions.User != null)
        {
            body["user"] = xaiOptions.User;
        }

        if (imageUrls.Count == 1)
        {
            body["image"] = new JsonObject { ["url"] = imageUrls[0], ["type"] = "image_url" };
        }
        else if (imageUrls.Count > 1)
        {
            body["images"] = new JsonArray(
                imageUrls
                    .Select(url =>
                        (JsonNode)new JsonObject { ["url"] = url, ["type"] = "image_url" }
                    )
                    .ToArray()
            );
        }

        return new PreparedBatchRequest(
            imageUrls.Count > 0 ? "/v1/images/edits" : "/v1/images/generations",
            body,
            warnings
        );
    }

    private static bool IsPresent(JsonElement options, string name)
    {
        return options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            && value.ValueKind != JsonValueKind.Undefined;
    }

    // Reads an ImageModelFile that Batch.StartBatchAsync serialized into the normalized options.
    private static ImageModelFile ToImageModelFile(JsonElement file)
    {
        if (Str(file, "Url") is { } url)
        {
            return ImageModelFile.FromUrl(url);
        }

        var mediaType = Str(file, "MediaType") ?? "application/octet-stream";
        return ImageModelFile.FromBase64(
            Str(file, "Base64") ?? Str(file, "Data") ?? string.Empty,
            mediaType
        );
    }

    private static bool IsValidResult(JsonElement result)
    {
        return result.ValueKind == JsonValueKind.Object
            && Str(result, "batch_request_id") != null
            && IsNullishString(result, "error_message")
            && (
                !result.TryGetProperty("batch_result", out var batchResult)
                || batchResult.ValueKind == JsonValueKind.Null
                || (
                    batchResult.ValueKind == JsonValueKind.Object
                    && IsNullishObject(batchResult, "response")
                    && (
                        !batchResult.TryGetProperty("error", out var error)
                        || error.ValueKind == JsonValueKind.Null
                        || (
                            error.ValueKind == JsonValueKind.Object
                            && IsNullishString(error, "message")
                            && (
                                !error.TryGetProperty("code", out var code)
                                || code.ValueKind
                                    is JsonValueKind.Null
                                        or JsonValueKind.String
                                        or JsonValueKind.Number
                            )
                        )
                    )
                )
            );
    }

    private static bool IsNullishString(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.String;
    }

    private static bool IsNullishNumber(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Number;
    }

    private static bool IsNullishObject(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Object;
    }

    private static bool IsNullishBoolean(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.True or JsonValueKind.False;
    }

    // Validates the Chat Completions shape of a text batch result. Maps to xaiBatchTextResponseSchema.
    private static bool IsValidTextResponse(JsonElement response)
    {
        if (
            response.ValueKind != JsonValueKind.Object
            || !IsNullishString(response, "id")
            || !IsNullishNumber(response, "created")
            || !IsNullishString(response, "model")
            || !IsNullishString(response, "service_tier")
            || !IsNullishString(response, "code")
            || !IsNullishString(response, "error")
        )
        {
            return false;
        }

        if (
            response.TryGetProperty("choices", out var choices)
            && choices.ValueKind != JsonValueKind.Null
        )
        {
            if (choices.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var choice in choices.EnumerateArray())
            {
                if (
                    choice.ValueKind != JsonValueKind.Object
                    || !choice.TryGetProperty("index", out var index)
                    || index.ValueKind != JsonValueKind.Number
                    || !IsNullishString(choice, "finish_reason")
                    || !choice.TryGetProperty("message", out var message)
                    || message.ValueKind != JsonValueKind.Object
                    || Str(message, "role") is not ("assistant" or "tool")
                    || !IsNullishString(message, "content")
                    || !IsNullishString(message, "reasoning_content")
                    || !IsValidToolCalls(message)
                )
                {
                    return false;
                }
            }
        }

        if (
            response.TryGetProperty("usage", out var usage)
            && usage.ValueKind != JsonValueKind.Null
        )
        {
            if (
                usage.ValueKind != JsonValueKind.Object
                || Number(usage, "prompt_tokens") == null
                || Number(usage, "completion_tokens") == null
                || Number(usage, "total_tokens") == null
                || !IsNullishNumber(usage, "cost_in_usd_ticks")
                || !IsNullishObject(usage, "prompt_tokens_details")
                || !IsNullishObject(usage, "completion_tokens_details")
                || (
                    usage.TryGetProperty("prompt_tokens_details", out var promptDetails)
                    && promptDetails.ValueKind == JsonValueKind.Object
                    && !IsNullishNumber(promptDetails, "cached_tokens")
                )
                || (
                    usage.TryGetProperty("completion_tokens_details", out var completionDetails)
                    && completionDetails.ValueKind == JsonValueKind.Object
                    && !IsNullishNumber(completionDetails, "reasoning_tokens")
                )
            )
            {
                return false;
            }
        }

        if (
            response.TryGetProperty("citations", out var citations)
            && citations.ValueKind != JsonValueKind.Null
        )
        {
            if (
                citations.ValueKind != JsonValueKind.Array
                || citations
                    .EnumerateArray()
                    .Any(citation =>
                        citation.ValueKind != JsonValueKind.String
                        || !Uri.TryCreate(citation.GetString(), UriKind.Absolute, out _)
                    )
            )
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidToolCalls(JsonElement message)
    {
        if (
            !message.TryGetProperty("tool_calls", out var toolCalls)
            || toolCalls.ValueKind == JsonValueKind.Null
        )
        {
            return true;
        }

        return toolCalls.ValueKind == JsonValueKind.Array
            && toolCalls
                .EnumerateArray()
                .All(toolCall =>
                    toolCall.ValueKind == JsonValueKind.Object
                    && Str(toolCall, "id") != null
                    && Str(toolCall, "type") == "function"
                    && toolCall.TryGetProperty("function", out var function)
                    && function.ValueKind == JsonValueKind.Object
                    && Str(function, "name") != null
                    && Str(function, "arguments") != null
                );
    }

    // Maps to xaiBatchImageResponseSchema.
    private static bool IsValidImageResponse(JsonElement response)
    {
        return response.ValueKind == JsonValueKind.Object
            && response.TryGetProperty("data", out var data)
            && data.ValueKind == JsonValueKind.Array
            && data.EnumerateArray()
                .All(image =>
                    image.ValueKind == JsonValueKind.Object
                    && IsNullishString(image, "url")
                    && IsNullishString(image, "b64_json")
                    && IsNullishString(image, "revised_prompt")
                    && IsNullishBoolean(image, "respect_moderation")
                )
            && IsNullishObject(response, "usage")
            && (
                !response.TryGetProperty("usage", out var usage)
                || usage.ValueKind != JsonValueKind.Object
                || IsNullishNumber(usage, "cost_in_usd_ticks")
            );
    }

    private sealed class PreparedBatchRequest
    {
        public PreparedBatchRequest(
            string endpoint,
            JsonNode body,
            IReadOnlyList<OperationWarning> warnings
        )
        {
            Endpoint = endpoint;
            Body = body;
            Warnings = warnings;
        }

        public string Endpoint { get; }

        public JsonNode Body { get; }

        public IReadOnlyList<OperationWarning> Warnings { get; }
    }
}
