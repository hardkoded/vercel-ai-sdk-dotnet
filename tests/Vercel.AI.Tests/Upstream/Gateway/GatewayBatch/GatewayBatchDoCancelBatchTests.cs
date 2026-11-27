// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayBatch.GatewayBatchSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

/// <summary>Port of <c>gateway-batch.test.ts</c> &gt; <c>GatewayBatch &gt; doCancelBatch</c>.</summary>
public sealed class GatewayBatchDoCancelBatchTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doCancelBatch::";

    [Theory]
    [InlineData("pending", "cancel_requested")]
    [InlineData("completed", "completed")]
    [InlineData("failed", "cancelled")]
    [UpstreamTest(
        Prefix + "should acknowledge cancellation without treating %s as a cancellation result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_acknowledge_cancellation_without_treating_the_status_as_a_cancellation_result(
        string status,
        string jobStatus
    )
    {
        var providerMetadata =
            "{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"" + jobStatus + "\"}}}";
        var server = JsonResponse(
            "{\"batchId\":\"job_123\",\"status\":\""
                + status
                + "\",\"providerMetadata\":"
                + providerMetadata
                + "}"
        );
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        var result = await CreateTestBatch(
                GatewayLanguageModelSupport.Recording(server, tokens),
                new Dictionary<string, string> { ["ai-o11y-deployment-id"] = "dpl_123" }
            )
            .DoCancelAsync(
                Call(
                    "job_123",
                    headers: new Dictionary<string, string> { ["custom-header"] = "cancel-value" },
                    cancellationToken: source.Token
                ),
                source.Token
            );

        JsonAssert.Equal(result.ProviderMetadata!.Value, providerMetadata);
        Assert.Null(result.Status);
        Assert.Equal(1, server.Calls);
        JsonAssert.Equal(server.RequestJson, "{\"batchId\":\"job_123\"}");
        var headers = server.RequestHeaders;
        Assert.Equal("Bearer test-token", headers["authorization"]);
        Assert.Equal("api-key", headers["ai-gateway-auth-method"]);
        Assert.Equal("dpl_123", headers["ai-o11y-deployment-id"]);
        Assert.Equal("cancel-value", headers["custom-header"]);
        Assert.False(headers.ContainsKey("ai-model-id"));
        Assert.Equal(source.Token, tokens[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow an acknowledgement without provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_an_acknowledgement_without_provider_metadata()
    {
        var server = JsonResponse("{\"status\":\"pending\",\"providerMetadata\":null}");

        var result = await CreateTestBatch(server)
            .DoCancelAsync(Call("job_123"), CancellationToken.None);

        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Status);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map missing jobs to GatewayNotFoundError without retrying",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_missing_jobs_to_GatewayNotFoundError_without_retrying()
    {
        var server = GatewayLanguageModelSupport.ErrorResponse(
            404,
            "{\"error\":{\"message\":\"Async job not found.\",\"type\":\"not_found\"}}"
        );

        await Assert.ThrowsAsync<GatewayNotFoundError>(() =>
            CreateTestBatch(server).DoCancelAsync(Call("missing-job"), CancellationToken.None)
        );

        Assert.Equal(1, server.Calls);
    }

    [Theory]
    [InlineData("AbortError")]
    [InlineData("TimeoutError")]
    [UpstreamTest(
        Prefix + "should preserve %s without wrapping or retrying it",
        Coverage = UpstreamCoverage.Covered,
        Note = "A cancelled call surfaces as an ApiUserAbortException whose cause is the original AbortError, as a .NET cancel does. A TimeoutError is not a cancellation exception, so it is rethrown as is."
    )]
    public async Task Should_preserve_the_error_without_wrapping_or_retrying_it(string name)
    {
        Exception error =
            name == "AbortError"
                ? new OperationCanceledException("Request interrupted.")
                : new TimeoutError("Request interrupted.");
        using var source = new CancellationTokenSource();
        source.Cancel();
        var calls = 0;

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateTestBatch(
                    (_, _) =>
                    {
                        calls++;
                        return Task.FromException<HttpResponseMessage>(error);
                    }
                )
                .DoCancelAsync(Call("job_123", cancellationToken: source.Token), source.Token)
        );

        if (name == "AbortError")
        {
            Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(thrown);
            Assert.Same(error, thrown.InnerException);
        }
        else
        {
            Assert.Same(error, thrown);
        }

        Assert.Equal(1, calls);
    }
}
