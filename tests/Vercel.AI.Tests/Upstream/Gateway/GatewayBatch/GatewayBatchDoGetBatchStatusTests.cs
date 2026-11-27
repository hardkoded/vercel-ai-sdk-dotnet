// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayBatch.GatewayBatchSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

/// <summary>Port of <c>gateway-batch.test.ts</c> &gt; <c>GatewayBatch &gt; doGetBatchStatus</c>.</summary>
public sealed class GatewayBatchDoGetBatchStatusTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doGetBatchStatus::";

    [Fact]
    [UpstreamTest(
        Prefix + "should post the batchId and map the status response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_post_the_batchId_and_map_the_status_response()
    {
        var server = JsonResponse(
            "{\"batchId\":\"job_123\",\"status\":\"completed\",\"rawStatus\":\"ended\",\"requestCounts\":{\"total\":2,\"pending\":0,\"completed\":2,\"failed\":0},\"createdAt\":\"2026-08-18T00:00:00.000Z\"}"
        );

        var status = await CreateTestBatch(server)
            .DoGetStatusAsync(Call("job_123"), CancellationToken.None);

        JsonAssert.Equal(server.RequestJson, "{\"batchId\":\"job_123\"}");
        Assert.Equal("Bearer test-token", server.RequestHeaders["authorization"]);
        Assert.False(server.RequestHeaders.ContainsKey("ai-model-id"));
        Assert.Equal("completed", status.Status);
        Assert.Equal("ended", status.RawStatus);
        Assert.Equal(2, status.RequestCounts!.Total);
        Assert.Equal(0, status.RequestCounts.Pending);
        Assert.Equal(2, status.RequestCounts.Completed);
        Assert.Equal(0, status.RequestCounts.Failed);
        Assert.Equal("2026-08-18T00:00:00.000Z", status.CreatedAt);
        Assert.Null(status.ErrorMessage);
        Assert.Null(status.ExpiresAt);
        Assert.Null(status.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map a failed status with error details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_failed_status_with_error_details()
    {
        var server = JsonResponse(
            "{\"batchId\":\"job_123\",\"status\":\"failed\",\"error\":{\"message\":\"Batch failed.\",\"type\":\"batch_failed\",\"statusCode\":502}}"
        );

        var status = await CreateTestBatch(server)
            .DoGetStatusAsync(Call("job_123"), CancellationToken.None);

        Assert.Equal("failed", status.Status);
        Assert.Equal("Batch failed.", status.ErrorMessage);
        Assert.Equal("batch_failed", status.ErrorType);
        Assert.Null(status.ErrorCode);
        Assert.Equal(502, status.ErrorStatusCode);
        Assert.Null(status.RawStatus);
        Assert.Null(status.RequestCounts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit partial request counts instead of fabricating zeros",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_partial_request_counts_instead_of_fabricating_zeros()
    {
        var server = JsonResponse(
            "{\"batchId\":\"job_123\",\"status\":\"pending\",\"requestCounts\":{\"total\":2}}"
        );

        var status = await CreateTestBatch(server)
            .DoGetStatusAsync(Call("job_123"), CancellationToken.None);

        Assert.Equal("pending", status.Status);
        Assert.Null(status.RequestCounts);
        Assert.Null(status.RawStatus);
        Assert.Null(status.CreatedAt);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map a 404 not_found response to GatewayNotFoundError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_404_not_found_response_to_GatewayNotFoundError()
    {
        var server = GatewayLanguageModelSupport.ErrorResponse(
            404,
            "{\"error\":{\"message\":\"Async job not found.\",\"type\":\"not_found\"}}"
        );

        var error = await Assert.ThrowsAsync<GatewayNotFoundError>(() =>
            CreateTestBatch(server).DoGetStatusAsync(Call("missing-job"), CancellationToken.None)
        );

        Assert.Equal("Async job not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
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

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateTestBatch((_, _) => Task.FromException<HttpResponseMessage>(abortError))
                .DoGetStatusAsync(Call("job_123", cancellationToken: source.Token), source.Token)
        );

        Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(thrown);
        Assert.Same(abortError, thrown.InnerException);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abortSignal to fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abortSignal_to_fetch_when_provided()
    {
        var server = JsonResponse("{\"batchId\":\"job_123\",\"status\":\"pending\"}");
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        await CreateTestBatch(GatewayLanguageModelSupport.Recording(server, tokens))
            .DoGetStatusAsync(Call("job_123", cancellationToken: source.Token), source.Token);

        Assert.Equal(source.Token, tokens[0]);
    }
}
