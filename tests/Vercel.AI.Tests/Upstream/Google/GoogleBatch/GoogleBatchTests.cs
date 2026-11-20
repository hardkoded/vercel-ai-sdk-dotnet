// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Google.GoogleBatch;

public sealed class GoogleBatchTests
{
    private const string Create =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:batchGenerateContent";
    private const string UploadStart =
        "https://generativelanguage.googleapis.com/upload/v1beta/files";
    private const string UploadSession =
        "https://generativelanguage.googleapis.com/upload/v1beta/files/session-123";
    private const string Batches = "https://generativelanguage.googleapis.com/v1beta/batches";
    private const string Batch =
        "https://generativelanguage.googleapis.com/v1beta/batches/batch-123";
    private const string CancelUrl =
        "https://generativelanguage.googleapis.com/v1beta/batches/batch-123:cancel";

    private static IBatchApi CreateBatch(
        GoogleBatchRecorder handler,
        Action<GoogleOptions>? configure = null
    )
    {
        var options = new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" };
        configure?.Invoke(options);
        return GoogleProvider.Create(options, handler).ExperimentalBatch()!;
    }

    private static NormalizedBatchRequest Request(
        string id,
        string prompt,
        string extraOptions = "",
        string modelId = "gemini-2.5-flash"
    )
    {
        var options = JsonDocument
            .Parse(
                "{\"prompt\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":"
                    + JsonSerializer.Serialize(prompt)
                    + "}]}]"
                    + extraOptions
                    + "}"
            )
            .RootElement.Clone();
        return new NormalizedBatchRequest(id, "text", modelId, options);
    }

    private static BatchOperationCall StartCall(
        IReadOnlyList<NormalizedBatchRequest> requests,
        string? webhookUrl = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    ) =>
        new(
            null,
            null,
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            webhookUrl: webhookUrl,
            requests: requests
        );

    private static BatchOperationCall OperationCall(
        string batchId = "batches/batch-123",
        Dictionary<string, string>? headers = null,
        int? limit = null,
        string? cursor = null,
        CancellationToken cancellationToken = default
    ) =>
        new(
            batchId,
            null,
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            limit,
            cursor
        );

    private static JsonObject Operation(
        JsonObject? metadataOverrides = null,
        JsonObject? operationOverrides = null
    )
    {
        var metadata = new JsonObject
        {
            ["name"] = "batches/batch-123",
            ["model"] = "models/gemini-2.5-flash",
            ["displayName"] = "ai-sdk-batch-test-id",
            ["state"] = "BATCH_STATE_SUCCEEDED",
            ["createTime"] = "2026-08-04T12:34:56.123Z",
            ["batchStats"] = new JsonObject
            {
                ["requestCount"] = "2",
                ["successfulRequestCount"] = "2",
                ["failedRequestCount"] = "0",
            },
        };
        foreach (var pair in metadataOverrides ?? [])
        {
            metadata[pair.Key] = pair.Value?.DeepClone();
        }

        var operation = new JsonObject
        {
            ["name"] = "batches/batch-123",
            ["done"] = true,
            ["metadata"] = metadata,
        };
        foreach (var pair in operationOverrides ?? [])
        {
            operation[pair.Key] = pair.Value?.DeepClone();
        }

        return operation;
    }

    private static GoogleBatchRecorder PrepareUpload(GoogleBatchRecorder handler)
    {
        return handler
            .Json(
                UploadStart,
                new JsonObject(),
                headers: new Dictionary<string, string> { ["x-goog-upload-url"] = UploadSession }
            )
            .Json(
                UploadSession,
                new JsonObject
                {
                    ["file"] = new JsonObject
                    {
                        ["name"] = "files/batch-input",
                        ["displayName"] = "batch.jsonl",
                        ["mimeType"] = "application/jsonl",
                        ["sizeBytes"] = "256",
                        ["uri"] =
                            "https://generativelanguage.googleapis.com/v1beta/files/batch-input",
                        ["state"] = "ACTIVE",
                        ["expirationTime"] = "2026-08-27T12:00:00Z",
                    },
                }
            );
    }

    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::applies the factory maxLineBytes setting of %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        const string outputUrl =
            "https://generativelanguage.googleapis.com/download/v1beta/files/batch-output:download?alt=media";
        var handler = new RoutedHandler()
            .Add(
                "https://generativelanguage.googleapis.com/v1beta/batches/batch-123",
                "{\"name\":\"batches/batch-123\",\"done\":true,\"metadata\":{\"state\":\"BATCH_STATE_SUCCEEDED\",\"output\":{\"responsesFile\":\"files/batch-output\"}}}"
            )
            .Add(
                outputUrl,
                "{\"key\":\"france\",\"response\":{\"responseId\":\"response-france\",\"modelVersion\":\"gemini-2.5-flash\",\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Paris\"}]},\"finishReason\":\"STOP\"}]}}\n"
            );
        var provider = GoogleProvider.Create(
            new GoogleOptions
            {
                ApiKey = "test-api-key",
                BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
            },
            handler
        );
        var batch = provider.ExperimentalBatch()!;
        var stream = await batch.DoGetResultsAsync(
            new BatchOperationCall(
                "batches/batch-123",
                null,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
        var results = stream.ToListAsync().AsTask();
        if (maxLineBytes == 16)
        {
            var error = await Assert.ThrowsAsync<DownloadError>(() => results);
            Assert.Equal(
                "https://generativelanguage.googleapis.com/download/v1beta/files/batch-output:download?alt=media",
                error.Url
            );
        }
        else
        {
            Assert.Single(await results);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::rejects unsupported request types before creating a batch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_unsupported_request_types_before_creating_a_batch()
    {
        var handler = new GoogleBatchRecorder();
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(
                StartCall([
                    new NormalizedBatchRequest(
                        "audio-1",
                        "audio",
                        "gemini-2.5-flash",
                        JsonDocument.Parse("{}").RootElement
                    ),
                ]),
                CancellationToken.None
            )
        );

        Assert.Equal("batch request type: audio", error.Functionality);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::rejects response schemas because the batch endpoint does not reliably enforce them",
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_response_schemas_because_the_batch_endpoint_does_not_reliably_enforce_them()
    {
        var handler = new GoogleBatchRecorder();
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(
                StartCall([
                    Request(
                        "schema",
                        "Hello",
                        ",\"responseFormat\":{\"type\":\"json\",\"schema\":{\"type\":\"object\",\"properties\":{}}}"
                    ),
                ]),
                CancellationToken.None
            )
        );

        Assert.Equal("structured output schemas in Google batches", error.Functionality);
        Assert.Contains("providerOptions.google.structuredOutputs", error.Message);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::allows response schemas when structuredOutputs is disabled and does not send the schema",
        Commit = "fe1b382b8e02f3e0bfd9939fb46b74a433a29a87",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Allows_response_schemas_when_structuredOutputs_is_disabled_and_does_not_send_the_schema()
    {
        var handler = new GoogleBatchRecorder().Json(Create, Operation());
        var batch = CreateBatch(handler);

        await batch.DoStartAsync(
            StartCall([
                Request(
                    "schema",
                    "Hello",
                    ",\"responseFormat\":{\"type\":\"json\",\"schema\":{\"type\":\"object\",\"properties\":{}}},\"providerOptions\":{\"google\":{\"structuredOutputs\":false}}"
                ),
            ]),
            CancellationToken.None
        );

        var generationConfig = handler.Calls[0].BodyJson["batch"]!["inputConfig"]!["requests"]![
            "requests"
        ]![0]!["request"]!["generationConfig"]!.AsObject();
        Assert.Equal("application/json", (string?)generationConfig["responseMimeType"]);
        Assert.False(generationConfig.ContainsKey("responseJsonSchema"));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::allows JSON mode without a schema",
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Allows_JSON_mode_without_a_schema()
    {
        var handler = new GoogleBatchRecorder().Json(Create, Operation());
        var batch = CreateBatch(handler);

        await batch.DoStartAsync(
            StartCall([Request("json", "Hello", ",\"responseFormat\":{\"type\":\"json\"}")]),
            CancellationToken.None
        );

        var generationConfig = handler.Calls[0].BodyJson["batch"]!["inputConfig"]!["requests"]![
            "requests"
        ]![0]!["request"]!["generationConfig"]!;
        Assert.Equal("application/json", (string?)generationConfig["responseMimeType"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::rejects mixed models before creating a batch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_mixed_models_before_creating_a_batch()
    {
        var handler = new GoogleBatchRecorder();
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoStartAsync(
                StartCall([
                    Request("flash", "Hello"),
                    Request("pro", "Hello", modelId: "gemini-2.5-pro"),
                ]),
                CancellationToken.None
            )
        );

        Assert.Equal("requests", error.Parameter);
        Assert.Contains(
            "Google batches require every request to use the same model because the model is part of the batch endpoint.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::starts a batch with inline requests when the creation body is under 20 MB",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Starts_a_batch_with_inline_requests_when_the_creation_body_is_under_20_MB()
    {
        var handler = new GoogleBatchRecorder().Json(
            Create,
            Operation(
                new JsonObject
                {
                    ["state"] = "BATCH_STATE_PENDING",
                    ["batchStats"] = new JsonObject
                    {
                        ["requestCount"] = "1",
                        ["successfulRequestCount"] = "0",
                        ["failedRequestCount"] = "0",
                        ["pendingRequestCount"] = "1",
                    },
                },
                new JsonObject { ["done"] = false }
            )
        );
        var batch = CreateBatch(handler, options =>
            options.Headers["Provider-Header"] = "provider"
        );
        var request = new NormalizedBatchRequest(
            "france",
            "text",
            "gemini-2.5-flash",
            JsonDocument
                .Parse(
                    """
                    {"prompt":[{"role":"system","content":"Answer with only the city name."},{"role":"user","content":[{"type":"text","text":"What is the capital of France?"}]}],"maxOutputTokens":20,"temperature":0.2,"topP":0.9,"topK":10,"frequencyPenalty":0.1,"presencePenalty":0.2,"stopSequences":["END"],"seed":42,"providerOptions":{"google":{"sharedRequestType":"flex"}}}
                    """
                )
                .RootElement.Clone()
        );

        using var abort = new CancellationTokenSource();
        handler.ObserveAbort(abort, 1);
        var result = await batch.DoStartAsync(
            StartCall(
                [request],
                "https://example.com/google-batch-webhook",
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                abort.Token
            ),
            abort.Token
        );

        Assert.Null(result.ProviderMetadata);
        Assert.Equal("batches/batch-123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal("BATCH_STATE_PENDING", result.RawStatus);
        Assert.Equal(1, result.RequestCounts!.Total);
        Assert.Equal(1, result.RequestCounts.Pending);
        Assert.Equal(0, result.RequestCounts.Completed);
        Assert.Equal(0, result.RequestCounts.Failed);
        Assert.Equal("2026-08-04T12:34:56.123Z", result.CreatedAt);
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("france", warning.RequestId);
                Assert.Equal("other", warning.Warning.Type);
                Assert.Contains(
                    "'sharedRequestType' and 'requestType' are Vertex AI options",
                    warning.Warning.Message
                );
            },
            warning =>
            {
                Assert.Equal("france", warning.RequestId);
                Assert.Equal("unsupported", warning.Warning.Type);
                Assert.Equal("frequencyPenalty", warning.Warning.Feature);
            },
            warning =>
            {
                Assert.Equal("france", warning.RequestId);
                Assert.Equal("unsupported", warning.Warning.Type);
                Assert.Equal("presencePenalty", warning.Warning.Feature);
            }
        );

        var call = Assert.Single(handler.Calls);
        Assert.True(handler.Calls[^1].SawCancellation);
        Assert.Equal(Create, call.Url);
        Assert.Equal("POST", call.Method);
        Assert.Equal("provider", call.Headers["provider-header"]);
        Assert.Equal("operation", call.Headers["operation-header"]);
        Assert.Equal("test-api-key", call.Headers["x-goog-api-key"]);
        JsonAssert.Equal(
            call.BodyJson,
            """
            {"batch":{"displayName":"ai-sdk-batch-test-id","webhookConfig":{"uris":["https://example.com/google-batch-webhook"]},"inputConfig":{"requests":{"requests":[{"request":{"generationConfig":{"maxOutputTokens":20,"temperature":0.2,"topK":10,"topP":0.9,"stopSequences":["END"],"seed":42},"contents":[{"role":"user","parts":[{"text":"What is the capital of France?"}]}],"systemInstruction":{"parts":[{"text":"Answer with only the city name."}]}},"metadata":{"key":"france"}}]}}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::starts an inline image generation batch"
    )]
    public async Task Starts_an_inline_image_generation_batch()
    {
        var handler = new GoogleBatchRecorder().Json(
            Create,
            Operation(new JsonObject(), new JsonObject { ["done"] = false })
        );
        var batch = CreateBatch(handler);
        var request = new NormalizedBatchRequest(
            "image-1",
            "image",
            "gemini-2.5-flash",
            JsonDocument
                .Parse(
                    """
                    {"prompt":"A red panda","n":1,"aspectRatio":"16:9","seed":42,"providerOptions":{}}
                    """
                )
                .RootElement.Clone()
        );

        await batch.DoStartAsync(StartCall([request]), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        JsonAssert.Equal(
            call.BodyJson,
            """
            {"batch":{"displayName":"ai-sdk-batch-test-id","inputConfig":{"requests":{"requests":[{"metadata":{"key":"image-1"},"request":{"contents":[{"role":"user","parts":[{"text":"A red panda"}]}],"generationConfig":{"seed":42,"responseModalities":["IMAGE"],"imageConfig":{"aspectRatio":"16:9"}}}}]}}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::cancels a batch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Cancels_a_batch()
    {
        var handler = new GoogleBatchRecorder().Json(CancelUrl, new JsonObject());
        var batch = CreateBatch(handler, options =>
            options.Headers["Provider-Header"] = "provider"
        );

        using var abort = new CancellationTokenSource();
        handler.ObserveAbort(abort, 1);
        var result = await batch.DoCancelAsync(
            OperationCall(
                headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                cancellationToken: abort.Token
            ),
            abort.Token
        );

        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Status);
        var call = handler.Calls[0];
        Assert.Equal("POST", call.Method);
        JsonAssert.Equal(call.BodyJson, "{}");
        Assert.Equal("test-api-key", call.Headers["x-goog-api-key"]);
        Assert.Equal("provider", call.Headers["provider-header"]);
        Assert.Equal("operation", call.Headers["operation-header"]);
        Assert.True(handler.Calls[^1].SawCancellation);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::lists and normalizes a page of batches",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Lists_and_normalizes_a_page_of_batches()
    {
        var handler = new GoogleBatchRecorder().Json(
            Batches,
            new JsonObject
            {
                ["operations"] = new JsonArray(
                    Operation(
                        new JsonObject
                        {
                            ["state"] = "BATCH_STATE_RUNNING",
                            ["batchStats"] = new JsonObject
                            {
                                ["requestCount"] = "3",
                                ["successfulRequestCount"] = "1",
                                ["failedRequestCount"] = "0",
                                ["pendingRequestCount"] = "2",
                            },
                        }
                    ),
                    Operation(
                        new JsonObject
                        {
                            ["state"] = "BATCH_STATE_SUCCEEDED",
                            ["batchStats"] = new JsonObject
                            {
                                ["requestCount"] = "2",
                                ["successfulRequestCount"] = "2",
                                ["failedRequestCount"] = "0",
                            },
                        },
                        new JsonObject { ["name"] = "batches/batch-122" }
                    )
                ),
                ["nextPageToken"] = "page-token-2",
            }
        );
        var batch = CreateBatch(handler, options =>
            options.Headers["Provider-Header"] = "provider"
        );

        using var abort = new CancellationTokenSource();
        handler.ObserveAbort(abort, 1);
        var page = await batch.DoListAsync(
            OperationCall(
                "",
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                2,
                "page-token-1",
                abort.Token
            ),
            abort.Token
        );

        Assert.Equal("page-token-2", page.NextCursor);
        Assert.Collection(
            page.Batches,
            first =>
            {
                Assert.Equal("batches/batch-123", first.Id);
                Assert.Equal("pending", first.Status);
                Assert.Equal("BATCH_STATE_RUNNING", first.RawStatus);
                Assert.Equal(3, first.RequestCounts!.Total);
                Assert.Equal(2, first.RequestCounts.Pending);
                Assert.Equal(1, first.RequestCounts.Completed);
                Assert.Equal(0, first.RequestCounts.Failed);
                Assert.Equal("2026-08-04T12:34:56.123Z", first.CreatedAt);
            },
            second =>
            {
                Assert.Equal("batches/batch-122", second.Id);
                Assert.Equal("completed", second.Status);
                Assert.Equal("BATCH_STATE_SUCCEEDED", second.RawStatus);
                Assert.Equal(2, second.RequestCounts!.Total);
                Assert.Equal(0, second.RequestCounts.Pending);
                Assert.Equal(2, second.RequestCounts.Completed);
                Assert.Equal(0, second.RequestCounts.Failed);
            }
        );
        var call = handler.Calls[0];
        Assert.Equal("test-api-key", call.Headers["x-goog-api-key"]);
        Assert.Equal("provider", call.Headers["provider-header"]);
        Assert.Equal("operation", call.Headers["operation-header"]);
        Assert.Equal("?pageSize=2&pageToken=page-token-1", new Uri(call.FullUrl).Query);
        Assert.True(handler.Calls[^1].SawCancellation);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"operations\":[],\"nextPageToken\":null}")]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::returns an empty terminal page for %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_an_empty_terminal_page_for(string response)
    {
        var handler = new GoogleBatchRecorder().Json(Batches, JsonNode.Parse(response)!);
        var batch = CreateBatch(handler);

        var page = await batch.DoListAsync(OperationCall(""), CancellationToken.None);

        Assert.Empty(page.Batches);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::uses a resumable file upload when the creation body reaches 20 MB",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the abort signal reached the final HTTP call only; HttpClient disposes the token of each earlier call before the test can cancel it."
    )]
    public async Task Uses_a_resumable_file_upload_when_the_creation_body_reaches_20_MB()
    {
        var handler = PrepareUpload(new GoogleBatchRecorder()).Json(Create, Operation());
        const string webhook = "https://example.com/google-batch-webhook";
        string InlineBody(string largePrompt) =>
            new JsonObject
            {
                ["batch"] = new JsonObject
                {
                    ["displayName"] = "ai-sdk-batch-test-id",
                    ["webhookConfig"] = new JsonObject { ["uris"] = new JsonArray(webhook) },
                    ["inputConfig"] = new JsonObject
                    {
                        ["requests"] = new JsonObject
                        {
                            ["requests"] = new JsonArray(
                                InlinedRequest("small-request", "small"),
                                InlinedRequest("large-request", largePrompt)
                            ),
                        },
                    },
                },
            }.ToJsonString();
        var emptyBodyBytes = Encoding.UTF8.GetByteCount(InlineBody(""));
        var prompt = new string('a', 20_000_000 - emptyBodyBytes);
        Assert.Equal(20_000_000, Encoding.UTF8.GetByteCount(InlineBody(prompt)));
        var batch = CreateBatch(handler, options =>
            options.Headers["Provider-Header"] = "provider"
        );

        using var abort = new CancellationTokenSource();
        handler.ObserveAbort(abort, 3);
        var result = await batch.DoStartAsync(
            StartCall(
                [Request("small-request", "small"), Request("large-request", prompt)],
                webhook,
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                abort.Token
            ),
            abort.Token
        );

        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            """{"google":{"inputFileId":"files/batch-input","inputFileExpiresAt":"2026-08-27T12:00:00Z"}}"""
        );
        Assert.Equal(
            [UploadStart, UploadSession, Create],
            handler.Calls.Select(call => call.Url).ToArray()
        );
        Assert.Equal("provider", handler.Calls[0].Headers["provider-header"]);
        Assert.Equal("operation", handler.Calls[0].Headers["operation-header"]);
        Assert.Equal("test-api-key", handler.Calls[0].Headers["x-goog-api-key"]);
        Assert.Equal("resumable", handler.Calls[0].Headers["x-goog-upload-protocol"]);
        Assert.Equal("start", handler.Calls[0].Headers["x-goog-upload-command"]);
        Assert.Equal(
            "application/jsonl",
            handler.Calls[0].Headers["x-goog-upload-header-content-type"]
        );
        Assert.Equal("upload, finalize", handler.Calls[1].Headers["x-goog-upload-command"]);
        Assert.Equal("0", handler.Calls[1].Headers["x-goog-upload-offset"]);
        Assert.DoesNotContain("x-goog-api-key", handler.Calls[1].Headers.Keys);
        Assert.DoesNotContain("provider-header", handler.Calls[1].Headers.Keys);
        Assert.DoesNotContain("operation-header", handler.Calls[1].Headers.Keys);
        JsonAssert.Equal(
            handler.Calls[2].BodyJson,
            """{"batch":{"displayName":"ai-sdk-batch-test-id","webhookConfig":{"uris":["https://example.com/google-batch-webhook"]},"inputConfig":{"fileName":"files/batch-input"}}}"""
        );
        Assert.Equal("provider", handler.Calls[2].Headers["provider-header"]);
        Assert.Equal("operation", handler.Calls[2].Headers["operation-header"]);
        Assert.Equal("test-api-key", handler.Calls[2].Headers["x-goog-api-key"]);
        var uploadText = handler.Calls[1].Body;
        Assert.StartsWith("{\"key\":\"small-request\",\"request\":", uploadText);
        Assert.Contains("\n{\"key\":\"large-request\",\"request\":", uploadText);
        Assert.EndsWith("\n", uploadText);
        Assert.Equal(3, handler.Calls.Count);
        Assert.True(handler.Calls[^1].SawCancellation);

        static JsonObject InlinedRequest(string id, string text) =>
            new()
            {
                ["request"] = new JsonObject
                {
                    ["generationConfig"] = new JsonObject(),
                    ["contents"] = new JsonArray(
                        new JsonObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JsonArray(new JsonObject { ["text"] = text }),
                        }
                    ),
                },
                ["metadata"] = new JsonObject { ["key"] = id },
            };
    }

    [Theory]
    [InlineData("inline")]
    [InlineData("file")]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::omits webhook configuration for %s input when no webhook URL is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_webhook_configuration_for_input_when_no_webhook_URL_is_provided(
        string inputType
    )
    {
        var handler = PrepareUpload(new GoogleBatchRecorder()).Json(Create, Operation());
        var batch = CreateBatch(handler);

        await batch.DoStartAsync(
            StartCall([
                Request("request-1", inputType == "file" ? new string('a', 20_000_000) : "Hello"),
            ]),
            CancellationToken.None
        );

        Assert.Equal(
            inputType == "file" ? [UploadStart, UploadSession, Create] : new[] { Create },
            handler.Calls.Select(call => call.Url).ToArray()
        );
        var body = handler.Calls[inputType == "file" ? 2 : 0].BodyJson;
        Assert.False(body["batch"]!.AsObject().ContainsKey("webhookConfig"));
    }

    [Theory]
    [InlineData("JOB_STATE_PENDING", "pending")]
    [InlineData("JOB_STATE_RUNNING", "pending")]
    [InlineData("JOB_STATE_SUCCEEDED", "completed")]
    [InlineData("JOB_STATE_FAILED", "failed")]
    [InlineData("JOB_STATE_CANCELLED", "failed")]
    [InlineData("JOB_STATE_EXPIRED", "failed")]
    [InlineData("BATCH_STATE_SUCCEEDED", "completed")]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::maps status %s to %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_status_to(string rawStatus, string status)
    {
        var handler = new GoogleBatchRecorder().Json(
            Batch,
            Operation(new JsonObject { ["state"] = rawStatus })
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal(status, result.Status);
        Assert.Equal(rawStatus, result.RawStatus);
    }

    [Theory]
    [InlineData(false, false, "pending")]
    [InlineData(true, false, "completed")]
    [InlineData(true, true, "failed")]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::uses Operation.done and error when metadata has no state ($status)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_Operation_done_and_error_when_metadata_has_no_state(
        bool done,
        bool hasError,
        string status
    )
    {
        var overrides = new JsonObject { ["done"] = done };
        if (hasError)
        {
            overrides["error"] = new JsonObject
            {
                ["code"] = 13,
                ["message"] = "The operation failed.",
            };
        }

        var metadata = Operation()["metadata"]!.DeepClone().AsObject();
        metadata.Remove("state");
        overrides["metadata"] = metadata;
        var handler = new GoogleBatchRecorder().Json(Batch, Operation(null, overrides));
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal(status, result.Status);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::normalizes int64 counts, timestamps, and a top-level RPC error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Normalizes_int64_counts_timestamps_and_a_top_level_RPC_error()
    {
        var handler = new GoogleBatchRecorder().Json(
            Batch,
            Operation(
                new JsonObject
                {
                    ["state"] = "BATCH_STATE_FAILED",
                    ["batchStats"] = new JsonObject
                    {
                        ["requestCount"] = "7",
                        ["successfulRequestCount"] = "2",
                        ["failedRequestCount"] = "3",
                        ["pendingRequestCount"] = "2",
                    },
                },
                new JsonObject
                {
                    ["error"] = new JsonObject
                    {
                        ["code"] = 3,
                        ["message"] = "The batch input was invalid.",
                        ["details"] = new JsonArray(
                            new JsonObject { ["reason"] = "INVALID_ARGUMENT" }
                        ),
                    },
                }
            )
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal("failed", result.Status);
        Assert.Equal("BATCH_STATE_FAILED", result.RawStatus);
        Assert.Equal(7, result.RequestCounts!.Total);
        Assert.Equal(2, result.RequestCounts.Pending);
        Assert.Equal(2, result.RequestCounts.Completed);
        Assert.Equal(3, result.RequestCounts.Failed);
        Assert.Equal("The batch input was invalid.", result.ErrorMessage);
        Assert.Equal("3", result.ErrorCode);
        Assert.Equal("2026-08-04T12:34:56.123Z", result.CreatedAt);
    }

    [Theory]
    [InlineData("all-failed batch with omitted zero counters", "1", null, "1", null, 1, 0, 0, 1)]
    [InlineData("running batch with omitted zero counters", "2", null, null, "2", 2, 2, 0, 0)]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::normalizes $name",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Normalizes_name(
        string name,
        string requestCount,
        string? successful,
        string? failed,
        string? pending,
        int total,
        int pendingCount,
        int completed,
        int failedCount
    )
    {
        Assert.NotEmpty(name);
        var stats = new JsonObject { ["requestCount"] = requestCount };
        if (successful != null)
        {
            stats["successfulRequestCount"] = successful;
        }

        if (failed != null)
        {
            stats["failedRequestCount"] = failed;
        }

        if (pending != null)
        {
            stats["pendingRequestCount"] = pending;
        }

        var handler = new GoogleBatchRecorder().Json(
            Batch,
            Operation(new JsonObject { ["batchStats"] = stats })
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal(total, result.RequestCounts!.Total);
        Assert.Equal(pendingCount, result.RequestCounts.Pending);
        Assert.Equal(completed, result.RequestCounts.Completed);
        Assert.Equal(failedCount, result.RequestCounts.Failed);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::surfaces Google HTTP errors from status retrieval",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Surfaces_Google_HTTP_errors_from_status_retrieval()
    {
        var handler = new GoogleBatchRecorder().Json(
            Batch,
            JsonNode.Parse(
                """{"error":{"code":404,"message":"Batch not found.","status":"NOT_FOUND"}}"""
            )!,
            HttpStatusCode.NotFound
        );
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            batch.DoGetStatusAsync(OperationCall(), CancellationToken.None)
        );

        Assert.Equal("Batch not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal(Batch, error.Url);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-batch.test.ts::GoogleBatch::surfaces Google HTTP errors from starting a batch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Surfaces_Google_HTTP_errors_from_starting_a_batch()
    {
        var handler = new GoogleBatchRecorder().Json(
            Create,
            JsonNode.Parse(
                """{"error":{"code":400,"message":"The batch input was invalid.","status":"INVALID_ARGUMENT"}}"""
            )!,
            HttpStatusCode.BadRequest
        );
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            batch.DoStartAsync(
                StartCall([Request("france", "What is the capital of France?")]),
                CancellationToken.None
            )
        );

        Assert.Equal("The batch input was invalid.", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(Create, error.Url);
    }
}
