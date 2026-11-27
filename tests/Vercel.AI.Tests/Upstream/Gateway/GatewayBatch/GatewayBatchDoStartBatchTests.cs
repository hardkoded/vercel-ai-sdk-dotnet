// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayBatch.GatewayBatchSupport;
using BatchApi = Vercel.AI.Operations.Batch;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

/// <summary>Port of <c>gateway-batch.test.ts</c> &gt; <c>GatewayBatch &gt; doStartBatch</c>.</summary>
public sealed class GatewayBatchDoStartBatchTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doStartBatch::";

    private const string StartResponse =
        "{\"batchId\":\"job_123\",\"status\":\"pending\",\"warnings\":[]}";

    private static GatewayLanguageModelServer PrepareBatchStartResponse(
        string body = StartResponse
    ) => JsonResponse(body);

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported request types before sending a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_unsupported_request_types_before_sending_a_request()
    {
        var server = PrepareBatchStartResponse();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            CreateTestBatch(server)
                .DoStartAsync(
                    Call(requests: [new("image-1", "image", "test-model-1", Parse("{}"))]),
                    CancellationToken.None
                )
        );

        Assert.Equal("batch request type: image", error.Functionality);
        Assert.Equal(0, server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fail before sending a request when models are mixed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fail_before_sending_a_request_when_models_are_mixed()
    {
        var server = PrepareBatchStartResponse();
        var requests = new NormalizedBatchRequest[]
        {
            BatchPromptRequests[0],
            new(
                BatchPromptRequests[1].Id,
                BatchPromptRequests[1].Type,
                "test-model-2",
                BatchPromptRequests[1].Options
            ),
        };

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateTestBatch(server).DoStartAsync(Call(requests: requests), CancellationToken.None)
        );

        Assert.Equal("requests", error.Parameter);
        Assert.Equal(
            "Invalid argument for parameter requests: The AI Gateway Batch API requires all requests in a batch to use the same model. Found \"test-model-1\" and \"test-model-2\".",
            error.Message
        );
        Assert.Equal(0, server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the shared request model in the model header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_shared_request_model_in_the_model_header()
    {
        var server = PrepareBatchStartResponse();

        await CreateTestBatch(
                server,
                new Dictionary<string, string> { ["ai-o11y-deployment-id"] = "dpl_123" }
            )
            .DoStartAsync(
                Call(
                    headers: new Dictionary<string, string> { ["Custom-Header"] = "batch-value" },
                    requests: BatchPromptRequests
                ),
                CancellationToken.None
            );

        var headers = server.RequestHeaders;
        Assert.Equal("Bearer test-token", headers["authorization"]);
        Assert.Equal("api-key", headers["ai-gateway-auth-method"]);
        Assert.Equal("test-model-1", headers["ai-model-id"]);
        Assert.Equal("dpl_123", headers["ai-o11y-deployment-id"]);
        Assert.Equal("batch-value", headers["custom-header"]);
        Assert.False(headers.ContainsKey("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the modality, per-request model ids, and provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_modality_per_request_model_ids_and_provider_options()
    {
        var server = PrepareBatchStartResponse();

        await CreateTestBatch(server)
            .DoStartAsync(
                Call(
                    providerOptions: "{\"gateway\":{\"order\":[\"openai\"]}}",
                    requests: BatchPromptRequests
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            server.RequestJson,
            "{\"requests\":"
                + BatchPromptRequestsJson
                + ",\"providerOptions\":{\"gateway\":{\"order\":[\"openai\"]}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map the start response to a BatchV4StartResult",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_the_start_response_to_a_BatchV4StartResult()
    {
        var server = PrepareBatchStartResponse(
            "{\"batchId\":\"job_123\",\"status\":\"pending\",\"rawStatus\":\"validating\",\"requestCounts\":{\"total\":2,\"pending\":2,\"completed\":0,\"failed\":0},\"warnings\":[{\"requestId\":\"req-1\",\"warning\":{\"type\":\"other\",\"message\":\"a warning\"}}],\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"queued\"}}}}"
        );

        var result = await CreateTestBatch(server)
            .DoStartAsync(Call(requests: BatchPromptRequests), CancellationToken.None);

        Assert.Equal("job_123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal("validating", result.RawStatus);
        Assert.Equal(2, result.RequestCounts!.Total);
        Assert.Equal(2, result.RequestCounts.Pending);
        Assert.Equal(0, result.RequestCounts.Completed);
        Assert.Equal(0, result.RequestCounts.Failed);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("req-1", warning.RequestId);
        Assert.Equal("other", warning.Warning.Type);
        Assert.Equal("a warning", warning.Warning.Message);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"queued\"}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should send the idempotency-key header from providerOptions.gateway.idempotencyKey without forwarding it in the body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_idempotency_key_header_without_forwarding_it_in_the_body()
    {
        var server = PrepareBatchStartResponse();

        await CreateTestBatch(server)
            .DoStartAsync(
                Call(
                    providerOptions: "{\"gateway\":{\"idempotencyKey\":\"idem-abc\"}}",
                    requests: BatchPromptRequests
                ),
                CancellationToken.None
            );

        Assert.Equal("idem-abc", server.RequestHeaders["idempotency-key"]);
        // Transport metadata stays out of the payload the Gateway digests for
        // replay identity; an empty providerOptions is omitted entirely.
        JsonAssert.Equal(server.RequestJson, "{\"requests\":" + BatchPromptRequestsJson + "}");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should keep other gateway provider options in the body while stripping the idempotency key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_other_gateway_provider_options_in_the_body_while_stripping_the_idempotency_key()
    {
        var server = PrepareBatchStartResponse();

        await CreateTestBatch(server)
            .DoStartAsync(
                Call(
                    providerOptions: "{\"gateway\":{\"idempotencyKey\":\"idem-abc\",\"order\":[\"openai\"]}}",
                    requests: BatchPromptRequests
                ),
                CancellationToken.None
            );

        Assert.Equal("idem-abc", server.RequestHeaders["idempotency-key"]);
        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"order\":[\"openai\"]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send webhookUrl as the top-level callbackUrl body field",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_webhookUrl_as_the_top_level_callbackUrl_body_field()
    {
        var server = PrepareBatchStartResponse(
            "{\"batchId\":\"batch_abc123\",\"status\":\"pending\",\"warnings\":[],\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"batch_abc123\",\"status\":\"queued\",\"webhookSigningSecret\":\"whsec_test\"}}}}"
        );

        var result = await CreateTestBatch(server)
            .DoStartAsync(
                Call(
                    webhookUrl: "https://example.com/batch-webhook",
                    requests: BatchPromptRequests
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            server.RequestJson,
            "{\"callbackUrl\":\"https://example.com/batch-webhook\",\"requests\":"
                + BatchPromptRequestsJson
                + "}"
        );
        // The webhook signing secret rides back on providerMetadata.
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"gateway\":{\"asyncJob\":{\"jobId\":\"batch_abc123\",\"status\":\"queued\",\"webhookSigningSecret\":\"whsec_test\"}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not send a callbackUrl body field when no webhookUrl is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_send_a_callbackUrl_body_field_when_no_webhookUrl_is_provided()
    {
        var server = PrepareBatchStartResponse();

        await CreateTestBatch(server)
            .DoStartAsync(Call(requests: BatchPromptRequests), CancellationToken.None);

        Assert.Null(server.RequestJson["callbackUrl"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should base64-encode Uint8Array file data in batch request prompts",
        Coverage = UpstreamCoverage.Covered,
        Note = "The core encodes file bytes when it normalizes a request, so the test starts the batch through StartBatchAsync."
    )]
    public async Task Should_base64_encode_Uint8Array_file_data_in_batch_request_prompts()
    {
        var server = PrepareBatchStartResponse();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var expectedBase64 = Convert.ToBase64String(bytes);
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" },
            server
        );

        await BatchApi.StartBatchAsync(
            new StartBatchOptions
            {
                Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromProvider(provider),
                Requests =
                [
                    new BatchRequest("req-1", "text", "test-model-1")
                    {
                        Messages =
                        [
                            new UserModelMessage(
                                new UserContentPart[]
                                {
                                    new FileContentPart("image/png", null, bytes, null),
                                }
                            ),
                        ],
                    },
                ],
            }
        );

        JsonAssert.Equal(
            server.RequestJson["requests"]![0]!["options"]!["prompt"]![0]!["content"]![0]!["data"],
            "{\"type\":\"data\",\"data\":\"" + expectedBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abortSignal to fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abortSignal_to_fetch_when_provided()
    {
        var server = PrepareBatchStartResponse();
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        await CreateTestBatch(GatewayLanguageModelSupport.Recording(server, tokens))
            .DoStartAsync(
                Call(requests: BatchPromptRequests, cancellationToken: source.Token),
                source.Token
            );

        Assert.Equal(source.Token, tokens[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should preserve an AbortError instead of converting it to a retryable gateway error",
        Coverage = UpstreamCoverage.Covered,
        Note = "A cancelled call surfaces as an ApiUserAbortException whose cause is the original AbortError, as a .NET cancel does."
    )]
    public async Task Should_preserve_an_AbortError_instead_of_converting_it_to_a_retryable_gateway_error()
    {
        var abortError = new OperationCanceledException("The operation was aborted.");
        using var source = new CancellationTokenSource();
        source.Cancel();

        // An aborted batch start may still have been accepted server-side;
        // surfacing a retryable 500 would invite a duplicate submission.
        var thrown = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateTestBatch((_, _) => Task.FromException<HttpResponseMessage>(abortError))
                .DoStartAsync(
                    Call(requests: BatchPromptRequests, cancellationToken: source.Token),
                    source.Token
                )
        );

        Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(thrown);
        Assert.Same(abortError, thrown.InnerException);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert HTTP errors via the gateway error path",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_HTTP_errors_via_the_gateway_error_path()
    {
        var server = GatewayLanguageModelSupport.ErrorResponse(
            404,
            "{\"error\":{\"message\":\"Model xyz not found\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"xyz\"}}}"
        );

        var error = await Assert.ThrowsAsync<GatewayModelNotFoundError>(() =>
            CreateTestBatch(server)
                .DoStartAsync(Call(requests: BatchPromptRequests), CancellationToken.None)
        );

        Assert.Equal("Model xyz not found", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
