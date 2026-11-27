// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayBatch.GatewayBatchSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

public sealed class GatewayBatchDoGetBatchResultsTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doGetBatchResults::";

    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest(
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doGetBatchResults::applies the factory maxLineBytes setting of %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        var handler = ResultsResponse(SucceededLine + "\n");
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.test.com",
                BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
            },
            handler
        );
        var batch = provider.ExperimentalBatch()!;
        var stream = await batch.DoGetResultsAsync(
            new BatchOperationCall(
                "job_123",
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
            Assert.Equal("https://api.test.com/batch/results", error.Url);
        }
        else
        {
            Assert.Single(await results);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should revive response.timestamp into a Date on succeeded items",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_revive_response_timestamp_into_a_Date_on_succeeded_items()
    {
        var server = ResultsResponse(SucceededLine + "\n");

        var stream = await CreateTestBatch(server)
            .DoGetResultsAsync(Call("job_123"), CancellationToken.None);
        var item = Assert.Single(await ReadAll(stream));

        Assert.Equal("succeeded", item.Status);
        var generation = item.Text!;
        Assert.Equal(new DateTime(2026, 8, 18, 0, 0, 0, DateTimeKind.Utc), generation.Timestamp);
        // Nested V4 usage is the shape core dereferences
        // (`usage.inputTokens.total` / `usage.outputTokens.total`).
        Assert.Equal(4, generation.Usage!.InputTotal);
        Assert.Equal(4, generation.Usage.NoCache);
        Assert.Equal(2, generation.Usage.OutputTotal);
        Assert.Equal(2, generation.Usage.Text);
        JsonAssert.Equal(
            generation.Usage.Raw!.Value,
            "{\"inputTokens\":{\"total\":4,\"noCache\":4},\"outputTokens\":{\"total\":2,\"text\":2}}"
        );
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
            CreateTestBatch(server).DoGetResultsAsync(Call("missing-job"), CancellationToken.None)
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
                .DoGetResultsAsync(Call("job_123", cancellationToken: source.Token), source.Token)
        );

        Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(thrown);
        Assert.Same(abortError, thrown.InnerException);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should post the batchId with the correct headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_post_the_batchId_with_the_correct_headers()
    {
        var server = ResultsResponse(SucceededLine + "\n");

        var stream = await CreateTestBatch(server)
            .DoGetResultsAsync(Call("job_123"), CancellationToken.None);
        await ReadAll(stream);

        JsonAssert.Equal(server.RequestJson, "{\"batchId\":\"job_123\"}");
        Assert.Equal("Bearer test-token", server.RequestHeaders["authorization"]);
        Assert.False(server.RequestHeaders.ContainsKey("ai-model-id"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should parse multi-line NDJSON results including lines split across chunks and a trailing line without a newline",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_multi_line_NDJSON_results_including_lines_split_across_chunks_and_a_trailing_line_without_a_newline()
    {
        var line1 = SucceededLine;
        var line2 = FailedLine;
        var line3 = CancelledLine;
        var server = ResultsResponse(
            // line 1 split across two chunks
            line1[..25],
            line1[25..] + "\n" + line2[..10],
            // line 2 completes; line 3 has no trailing newline
            line2[10..] + "\n" + line3
        );

        var stream = await CreateTestBatch(server)
            .DoGetResultsAsync(Call("job_123"), CancellationToken.None);
        var items = await ReadAll(stream);

        Assert.Collection(
            items,
            item =>
            {
                Assert.Equal("req-1", item.Id);
                Assert.Equal("succeeded", item.Status);
                Assert.Equal("pong 1", Assert.Single(item.Text!.Content).Text);
                Assert.Equal(
                    new DateTime(2026, 8, 18, 0, 0, 0, DateTimeKind.Utc),
                    item.Text.Timestamp
                );
            },
            item =>
            {
                Assert.Equal("req-2", item.Id);
                Assert.Equal("failed", item.Status);
                Assert.Equal("boom", item.ErrorMessage);
            },
            item =>
            {
                Assert.Equal("req-3", item.Id);
                Assert.Equal("cancelled", item.Status);
            }
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should skip empty lines", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_skip_empty_lines()
    {
        var server = ResultsResponse(SucceededLine + "\n\n" + FailedLine + "\n");

        var stream = await CreateTestBatch(server)
            .DoGetResultsAsync(Call("job_123"), CancellationToken.None);
        var items = await ReadAll(stream);

        Assert.Equal(["req-1", "req-2"], items.Select(item => item.Id));
        Assert.Equal(["succeeded", "failed"], items.Select(item => item.Status));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert a 400 for a non-terminal batch via the gateway error path",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_a_400_for_a_non_terminal_batch_via_the_gateway_error_path()
    {
        var server = GatewayLanguageModelSupport.ErrorResponse(
            400,
            "{\"error\":{\"message\":\"The batch is not yet terminal.\",\"type\":\"invalid_request_error\"}}"
        );

        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            CreateTestBatch(server).DoGetResultsAsync(Call("job_123"), CancellationToken.None)
        );

        Assert.Equal("The batch is not yet terminal.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abortSignal to fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abortSignal_to_fetch_when_provided()
    {
        var server = ResultsResponse(SucceededLine + "\n");
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        var stream = await CreateTestBatch(GatewayLanguageModelSupport.Recording(server, tokens))
            .DoGetResultsAsync(Call("job_123", cancellationToken: source.Token), source.Token);
        await ReadAll(stream);

        Assert.Equal(source.Token, tokens[0]);
    }
}
