// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini Batch API. Starts, reads, cancels, and lists batches and downloads their results.</summary>
public sealed class GoogleBatchApi : BatchResultsApiBase
{
    private const long BatchInputFileMaxBytes = 2L * 1024 * 1024 * 1024;
    private const int BatchInlineCreationMaxBytes = 20_000_000;

    private readonly GoogleProvider _provider;

    internal GoogleBatchApi(GoogleProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.ModelProvider + ".batch";

    /// <inheritdoc />
    /// <remarks>Batch prompt conversion happens before the model is known, so only URL support shared by every batch model is advertised.</remarks>
    public override IReadOnlyDictionary<string, string> SupportedUrls =>
        _provider.GetSupportedUrls(null);

    /// <inheritdoc />
    public override bool CanCancel => true;

    /// <inheritdoc />
    public override bool CanList => true;

    /// <inheritdoc />
    public override async Task<BatchStartResult> DoStartAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        AssertSupportedBatchRequests(requests);
        var modelId = GetGoogleBatchModelId(requests);
        var warnings = new List<BatchRequestWarning>();
        var displayName = "ai-sdk-batch-" + (_provider.Options.GenerateId ?? DefaultId)();
        var inlinedRequests = new List<JsonObject>();
        JsonObject BatchBody(JsonObject inputConfig)
        {
            var batch = new JsonObject { ["displayName"] = displayName };
            if (call.WebhookUrl != null)
            {
                batch["webhookConfig"] = new JsonObject
                {
                    ["uris"] = new JsonArray(JsonValue.Create(call.WebhookUrl)),
                };
            }

            batch["inputConfig"] = inputConfig;
            return new JsonObject { ["batch"] = batch };
        }

        var inlineInputBytes = Utf8Length(
            BatchBody(
                new JsonObject { ["requests"] = new JsonObject { ["requests"] = new JsonArray() } }
            )
        );
        StringBuilder? fileParts = null;

        foreach (var request in requests)
        {
            var prepared =
                request.Type == "text" ? PrepareTextRequest(request) : PrepareImageRequest(request);
            var inlinedRequest = new JsonObject
            {
                ["request"] = prepared.Body.DeepClone(),
                ["metadata"] = new JsonObject { ["key"] = request.Id },
            };

            if (fileParts == null)
            {
                var requestBytes = Utf8Length(inlinedRequest);
                var next = inlineInputBytes + requestBytes + (inlinedRequests.Count > 0 ? 1 : 0);
                if (next < BatchInlineCreationMaxBytes)
                {
                    inlinedRequests.Add(inlinedRequest);
                    inlineInputBytes = next;
                }
                else
                {
                    fileParts = new StringBuilder();
                    foreach (var previous in inlinedRequests)
                    {
                        AppendFileLine(
                            fileParts,
                            previous["metadata"]!["key"]!.GetValue<string>(),
                            previous["request"]!
                        );
                    }

                    inlinedRequests.Clear();
                    AppendFileLine(fileParts, request.Id, prepared.Body);
                }
            }
            else
            {
                AppendFileLine(fileParts, request.Id, prepared.Body);
            }

            foreach (var warning in prepared.Warnings)
            {
                warnings.Add(
                    new BatchRequestWarning(
                        request.Id,
                        new OperationWarning(
                            warning.Type,
                            warning.Message,
                            warning.Feature,
                            warning.Details
                        )
                    )
                );
            }
        }

        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');
        var createUri = new Uri(
            baseUrl + "/" + GoogleModelPath.Get(modelId) + ":batchGenerateContent"
        );

        if (fileParts == null)
        {
            var body = BatchBody(
                new JsonObject
                {
                    ["requests"] = new JsonObject
                    {
                        ["requests"] = new JsonArray(inlinedRequests.Cast<JsonNode?>().ToArray()),
                    },
                }
            );
            var operation = await SendOperationAsync(
                    HttpMethod.Post,
                    createUri,
                    body.ToJsonString(),
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            return Fill(operation, warnings);
        }

        var inputBytes = Encoding.UTF8.GetBytes(fileParts.ToString());
        if (inputBytes.LongLength > BatchInputFileMaxBytes)
        {
            throw new InvalidArgumentException(
                "requests",
                null,
                "Google batch input files must not exceed 2 GB."
            );
        }

        var uploadHeaders = new Dictionary<string, string?>(headers)
        {
            ["X-Goog-Upload-Protocol"] = "resumable",
            ["X-Goog-Upload-Command"] = "start",
            ["X-Goog-Upload-Header-Content-Length"] = inputBytes.Length.ToString(
                CultureInfo.InvariantCulture
            ),
            ["X-Goog-Upload-Header-Content-Type"] = "application/jsonl",
        };
        var origin = GetBaseOrigin();
        var started = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                new Uri(origin + "/upload/v1beta/files"),
                new JsonObject
                {
                    ["file"] = new JsonObject { ["display_name"] = displayName + "-input" },
                }.ToJsonString(),
                uploadHeaders,
                cancellationToken
            )
            .ConfigureAwait(false);
        var uploadUrl = started
            .Headers.FirstOrDefault(pair =>
                string.Equals(pair.Key, "x-goog-upload-url", StringComparison.OrdinalIgnoreCase)
            )
            .Value;
        if (uploadUrl == null)
        {
            throw new InvalidResponseDataException(
                started.Headers,
                "Google did not return a resumable upload URL."
            );
        }

        using var content = new ByteArrayContent(inputBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/jsonl");
        var uploaded = await _provider
            .Http.SendBinaryAsync(
                HttpMethod.Post,
                new Uri(uploadUrl),
                content,
                new Dictionary<string, string?>
                {
                    ["X-Goog-Upload-Offset"] = "0",
                    ["X-Goog-Upload-Command"] = "upload, finalize",
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        JsonElement file;
        using (var document = JsonDocument.Parse(uploaded.Body))
        {
            file = document.RootElement.Clone();
        }

        if (
            !file.TryGetProperty("file", out var fileInfo)
            || Str(fileInfo, "name") is not { } fileName
        )
        {
            throw new ApiException(
                "Invalid JSON response",
                200,
                Encoding.UTF8.GetString(uploaded.Body)
            );
        }

        var fileOperation = await SendOperationAsync(
                HttpMethod.Post,
                createUri,
                BatchBody(new JsonObject { ["fileName"] = fileName }).ToJsonString(),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var result = Fill(fileOperation, warnings);
        var google = new JsonObject { ["inputFileId"] = fileName };
        if (Str(fileInfo, "expirationTime") is { } expiration)
        {
            google["inputFileExpiresAt"] = expiration;
        }

        result.ProviderMetadata = JsonSerializer.SerializeToElement(
            new JsonObject { ["google"] = google }
        );
        return result;
    }

    /// <inheritdoc />
    public override async Task<BatchStatus> DoGetStatusAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var operation = await RetrieveBatchAsync(call, cancellationToken).ConfigureAwait(false);
        return ConvertStatus(operation, new BatchStatus());
    }

    /// <inheritdoc />
    public override async Task<BatchCancelResult> DoCancelAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        using var document = await _provider
            .Http.SendJsonAsync(
                HttpMethod.Post,
                new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/" + call.BatchId + ":cancel"),
                "{}",
                headers,
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
            query.Add("pageSize=" + limit.ToString(CultureInfo.InvariantCulture));
        }

        if (call.Cursor != null)
        {
            query.Add("pageToken=" + Uri.EscapeDataString(call.Cursor));
        }

        var url = _provider.Options.BaseUrl.TrimEnd('/') + "/batches";
        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        JsonElement page;
        using (
            var document = await _provider
                .Http.SendJsonAsync(HttpMethod.Get, new Uri(url), null, headers, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            page = document.RootElement.Clone();
        }

        var batches = new List<ListedBatch>();
        if (
            page.TryGetProperty("operations", out var operations)
            && operations.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var operation in operations.EnumerateArray())
            {
                var listed = new ListedBatch { Id = ValidateOperation(operation) };
                batches.Add(ConvertStatus(operation, listed));
            }
        }

        return new BatchListResult(batches, Str(page, "nextPageToken"));
    }

    /// <inheritdoc />
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var batchId = call.BatchId ?? string.Empty;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');
        JsonElement operation;
        using (
            var document = await _provider
                .Http.SendJsonAsync(
                    HttpMethod.Get,
                    new Uri(
                        baseUrl
                            + "/"
                            + string.Join("/", batchId.Split('/').Select(Uri.EscapeDataString))
                    ),
                    null,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
        {
            operation = document.RootElement.Clone();
        }

        var done =
            operation.TryGetProperty("done", out var doneValue)
            && doneValue.ValueKind == JsonValueKind.True;
        var file = Find(operation, "metadata", "output") ?? Find(operation, "response");
        var responsesFile = file == null ? null : Str(file.Value, "responsesFile");
        if (responsesFile == null)
        {
            if (!done)
            {
                throw new InvalidArgumentException(
                    "batchId",
                    batchId,
                    "Google batch \"" + batchId + "\" is not complete."
                );
            }

            throw new AiSdkException(
                "Google batch \"" + batchId + "\" completed without batch output."
            );
        }

        var origin = new Uri(baseUrl).GetLeftPart(UriPartial.Authority);
        var encoded = string.Join("/", responsesFile.Split('/').Select(Uri.EscapeDataString));
        var uri = new Uri(origin + "/download/v1beta/" + encoded + ":download?alt=media");
        var lines = await _provider
            .Http.OpenJsonLinesAsync(
                HttpMethod.Get,
                uri,
                null,
                headers,
                _provider.Options.BatchResultDownloads?.MaxLineBytes,
                cancellationToken
            )
            .ConfigureAwait(false);
        return ConvertRows(lines, Convert, cancellationToken);
    }

    // Upstream's assertSupportedBatchRequests accepts only text and image requests.
    private static void AssertSupportedBatchRequests(IReadOnlyList<NormalizedBatchRequest> requests)
    {
        foreach (var request in requests)
        {
            if (request.Type != "text" && request.Type != "image")
            {
                throw new UnsupportedFunctionalityException(
                    "batch request type: " + request.Type,
                    "The Google Batch API does not support batch requests with type \""
                        + request.Type
                        + "\"."
                );
            }
        }
    }

    private static string GetGoogleBatchModelId(IReadOnlyList<NormalizedBatchRequest> requests)
    {
        if (requests.Count == 0)
        {
            throw new InvalidArgumentException(
                "requests",
                null,
                "Google batches require at least one request."
            );
        }

        var modelId = requests[0].ModelId;
        foreach (var request in requests)
        {
            if (request.ModelId != modelId)
            {
                throw new InvalidArgumentException(
                    "requests",
                    null,
                    "Google batches require every request to use the same model because the model is part of the batch endpoint."
                );
            }
        }

        return modelId;
    }

    private static string DefaultId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    private static int Utf8Length(JsonNode node)
    {
        return Encoding.UTF8.GetByteCount(node.ToJsonString());
    }

    private static void AppendFileLine(StringBuilder file, string key, JsonNode request)
    {
        file.Append(
                new JsonObject { ["key"] = key, ["request"] = request.DeepClone() }.ToJsonString()
            )
            .Append('\n');
    }

    private GooglePreparedRequest PrepareTextRequest(NormalizedBatchRequest request)
    {
        var prepared = GoogleRequest.Prepare(
            request.ModelId,
            _provider.ModelProvider,
            OpenAIBatchApi.ToCallOptions(request.Options, "Google"),
            false,
            _provider.ToolResultDownloadSettings?.SupportsGoogleCloudStorageUrls ?? false
        );
        if (
            prepared.Body["generationConfig"] is JsonObject generationConfig
            && generationConfig["responseJsonSchema"] != null
        )
        {
            throw new UnsupportedFunctionalityException(
                "structured output schemas in Google batches",
                "The Google Batch API does not reliably enforce JSON schemas (request \""
                    + request.Id
                    + "\"). Remove the output schema, use `Output.Json()` without a schema, or set "
                    + "`providerOptions.google.structuredOutputs` to `false` to not send the schema "
                    + "and validate the results with `GetBatchResults` and an output specification instead."
            );
        }

        return prepared;
    }

    // Upstream's prepareImageRequest: the image options become a one-message language model call with the IMAGE modality.
    private GooglePreparedRequest PrepareImageRequest(NormalizedBatchRequest request)
    {
        var options = request.Options;
        var warnings = new List<GoogleWarning>();
        if (IsSet(options, "mask"))
        {
            throw new UnsupportedFunctionalityException(
                "mask-based image editing in Google batches",
                "'mask-based image editing in Google batches' functionality not supported."
            );
        }

        if (
            options.TryGetProperty("n", out var n)
            && n.ValueKind == JsonValueKind.Number
            && n.GetDouble() > 1
        )
        {
            throw new UnsupportedFunctionalityException(
                "multiple images per Google batch request",
                "'multiple images per Google batch request' functionality not supported."
            );
        }

        if (IsSet(options, "size"))
        {
            warnings.Add(
                new GoogleWarning(
                    "unsupported",
                    string.Empty,
                    "size",
                    "This model does not support the `size` option. Use `aspectRatio` instead."
                )
            );
        }

        var content = new List<UserContentPart>();
        if (Str(options, "prompt") is { } prompt)
        {
            content.Add(new TextContentPart(prompt));
        }

        if (
            options.TryGetProperty("files", out var files)
            && files.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var file in files.EnumerateArray())
            {
                content.Add(
                    Str(file, "type") == "url"
                        ? new FileContentPart(
                            "image/*",
                            new Uri(Str(file, "url")!).ToString(),
                            null,
                            null
                        )
                        : new FileContentPart(
                            Str(file, "mediaType") ?? "application/octet-stream",
                            null,
                            System.Convert.FromBase64String(Str(file, "data") ?? string.Empty),
                            null
                        )
                );
            }
        }

        var languageModelOptions = new JsonObject();
        JsonElement? googleSearch = null;
        JsonNode? userImageConfig = null;
        if (
            options.TryGetProperty("providerOptions", out var providerOptions)
            && providerOptions.ValueKind == JsonValueKind.Object
            && providerOptions.TryGetProperty("google", out var google)
            && google.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var property in google.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "googleSearch":
                        googleSearch = property.Value.Clone();
                        break;
                    case "responseModalities":
                        break;
                    case "imageConfig":
                        userImageConfig = JsonNode.Parse(property.Value.GetRawText());
                        break;
                    default:
                        languageModelOptions[property.Name] = JsonNode.Parse(
                            property.Value.GetRawText()
                        );
                        break;
                }
            }
        }

        languageModelOptions["responseModalities"] = new JsonArray(JsonValue.Create("IMAGE"));
        var aspectRatio = Str(options, "aspectRatio");
        if (aspectRatio != null || userImageConfig != null)
        {
            var imageConfig = userImageConfig as JsonObject ?? new JsonObject();
            if (aspectRatio != null)
            {
                imageConfig["aspectRatio"] = aspectRatio;
            }

            languageModelOptions["imageConfig"] = imageConfig;
        }

        if (googleSearch is { } search)
        {
            if (search.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidArgumentException(
                    "providerOptions",
                    null,
                    "Invalid Google image provider option googleSearch."
                );
            }

            languageModelOptions["providerTools"] = new JsonArray(
                new JsonObject
                {
                    ["id"] = "google.google_search",
                    ["name"] = "google_search",
                    ["args"] = JsonNode.Parse(search.GetRawText()),
                }
            );
        }

        var callOptions = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(content) },
            Seed = OpenAIJson.Int(options, "seed"),
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["google"] = JsonSerializer.SerializeToElement(languageModelOptions),
            },
        };
        var prepared = GoogleRequest.Prepare(
            request.ModelId,
            _provider.ModelProvider,
            callOptions,
            false,
            _provider.ToolResultDownloadSettings?.SupportsGoogleCloudStorageUrls ?? false
        );
        return new GooglePreparedRequest(
            prepared.Body,
            warnings.Concat(prepared.Warnings).ToList(),
            prepared.ExtraHeaders,
            prepared.Tools,
            prepared.JsonResponseToolName
        );
    }

    private static bool IsSet(JsonElement options, string name)
    {
        return options.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            && value.ValueKind != JsonValueKind.Undefined;
    }

    private async Task<Dictionary<string, string?>> BatchHeadersAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    private string GetBaseOrigin()
    {
        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');
        return baseUrl.EndsWith("/v1beta", StringComparison.Ordinal)
            ? baseUrl.Substring(0, baseUrl.Length - "/v1beta".Length)
            : baseUrl;
    }

    private async Task<JsonElement> RetrieveBatchAsync(
        BatchOperationCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await BatchHeadersAsync(call, cancellationToken).ConfigureAwait(false);
        return await SendOperationAsync(
                HttpMethod.Get,
                new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/" + call.BatchId),
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task<JsonElement> SendOperationAsync(
        HttpMethod method,
        Uri uri,
        string? body,
        Dictionary<string, string?> headers,
        CancellationToken cancellationToken
    )
    {
        using var document = await _provider
            .Http.SendJsonAsync(method, uri, body, headers, cancellationToken)
            .ConfigureAwait(false);
        var operation = document.RootElement.Clone();
        ValidateOperation(operation);
        return operation;
    }

    // Upstream's googleBatchOperationSchema requires a string name.
    private static string ValidateOperation(JsonElement operation)
    {
        return Str(operation, "name")
            ?? throw new ApiException("Invalid JSON response", 200, operation.GetRawText());
    }

    private static BatchStartResult Fill(JsonElement operation, List<BatchRequestWarning> warnings)
    {
        var result = new BatchStartResult { Id = Str(operation, "name")!, Warnings = warnings };
        return ConvertStatus(operation, result);
    }

    private static T ConvertStatus<T>(JsonElement operation, T target)
        where T : BatchStatus
    {
        var metadata = operation.TryGetProperty("metadata", out var m) ? m : default;
        var rawStatus = Str(metadata, "state");
        var stats =
            metadata.ValueKind == JsonValueKind.Object
            && metadata.TryGetProperty("batchStats", out var s)
                ? s
                : default;
        var counts = ProviderValues.NormalizeBatchRequestCounts(
            ParseCount(stats, "requestCount", false),
            ParseCount(stats, "pendingRequestCount", true),
            ParseCount(stats, "successfulRequestCount", true),
            ParseCount(stats, "failedRequestCount", true)
        );
        var error =
            operation.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.Object
                ? e
                : (JsonElement?)null;
        var done = operation.TryGetProperty("done", out var d) && d.ValueKind == JsonValueKind.True;
        target.Status = MapStatus(rawStatus, done, error != null);
        target.RawStatus = rawStatus;
        target.RequestCounts =
            counts == null || counts.Total > int.MaxValue
                ? null
                : new BatchRequestCounts(
                    (int)counts.Total,
                    (int)counts.Pending,
                    (int)counts.Completed,
                    (int)counts.Failed
                );
        target.CreatedAt = Str(metadata, "createTime");
        if (error != null)
        {
            target.ErrorMessage = Str(error.Value, "message") ?? "Google batch failed.";
            target.ErrorCode = RpcCode(error.Value);
        }

        return target;
    }

    private static string? RpcCode(JsonElement error)
    {
        if (!error.TryGetProperty("code", out var code))
        {
            return null;
        }

        return code.ValueKind switch
        {
            JsonValueKind.String => code.GetString(),
            JsonValueKind.Number => code.GetRawText(),
            _ => null,
        };
    }

    private static string MapStatus(string? rawStatus, bool done, bool hasError)
    {
        if (hasError)
        {
            return "failed";
        }

        if (rawStatus == null)
        {
            return done ? "completed" : "pending";
        }

        var normalized =
            rawStatus.StartsWith("BATCH_STATE_", StringComparison.Ordinal)
                ? rawStatus.Substring("BATCH_STATE_".Length)
            : rawStatus.StartsWith("JOB_STATE_", StringComparison.Ordinal)
                ? rawStatus.Substring("JOB_STATE_".Length)
            : rawStatus;
        return normalized switch
        {
            "SUCCEEDED" => "completed",
            "FAILED" or "CANCELLED" or "EXPIRED" => "failed",
            _ => "pending",
        };
    }

    // Upstream's parseCount: a decimal string or a safe non-negative integer. Missing optional counts are 0.
    private static double? ParseCount(JsonElement stats, string name, bool defaultsToZero)
    {
        if (
            stats.ValueKind != JsonValueKind.Object
            || !stats.TryGetProperty(name, out var value)
            || value.ValueKind == JsonValueKind.Null
        )
        {
            return defaultsToZero ? 0 : null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString()!;
            return text.Length > 0 && text.All(c => c >= '0' && c <= '9')
                ? double.Parse(text, CultureInfo.InvariantCulture)
                : null;
        }

        return value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)
            ? number
            : null;
    }

    private static JsonElement? Find(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var name in path)
        {
            if (
                current.ValueKind != JsonValueKind.Object
                || !current.TryGetProperty(name, out current)
            )
            {
                return null;
            }
        }

        return current;
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "key") ?? string.Empty;
        if (
            line.TryGetProperty("response", out var response)
            && response.ValueKind == JsonValueKind.Object
        )
        {
            var texts = new List<string>();
            string? finish = null;
            if (
                response.TryGetProperty("candidates", out var candidates)
                && candidates.ValueKind == JsonValueKind.Array
                && candidates.GetArrayLength() > 0
            )
            {
                var candidate = candidates[0];
                finish = Str(candidate, "finishReason");
                var parts = Find(candidate, "content", "parts");
                if (parts != null && parts.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in parts.Value.EnumerateArray())
                    {
                        var text = Str(part, "text");
                        if (text != null)
                        {
                            texts.Add(text);
                        }
                    }
                }
            }

            return TextItem(
                id,
                texts,
                finish,
                Str(response, "responseId"),
                Str(response, "modelVersion")
            );
        }

        var error = line.TryGetProperty("error", out var value) ? value : default;
        return new BatchItem("text", id, "failed") { ErrorMessage = Str(error, "message") };
    }
}
