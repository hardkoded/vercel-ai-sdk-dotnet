// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Text;
using Vercel.AI.Anthropic;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class BatchAndJsonReadTests
{
    [Fact]
    public async Task ReadJsonAsync_decodes_the_body_with_the_response_charset()
    {
        var content = new ByteArrayContent(Encoding.GetEncoding("iso-8859-1").GetBytes("{\"text\":\"café\"}"));
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "iso-8859-1" };
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var body = await JsonStreams.ReadJsonAsync(response, null);
        Assert.Equal("café", body.Value.GetProperty("text").GetString());
    }

    [Fact]
    public async Task Gateway_batch_results_keep_a_non_positive_maxLineBytes_as_an_argument_error()
    {
        var handler = new RoutedHandler().Add("https://api.test.com/batch/results", "{}\n");
        var provider = GatewayProvider.Create(new GatewayOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com",
            BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = 0 },
        }, handler);
        var batch = provider.ExperimentalBatch()!;
        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            batch.DoGetResultsAsync(new BatchOperationCall("job_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None));
    }

    [Fact]
    public async Task Anthropic_batch_result_without_a_message_becomes_a_failed_item()
    {
        const string resultsUrl = "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results";
        var handler = new RoutedHandler()
            .Add("https://api.anthropic.com/v1/messages/batches/msgbatch_123", "{\"id\":\"msgbatch_123\",\"type\":\"message_batch\",\"processing_status\":\"ended\",\"request_counts\":{\"processing\":0,\"succeeded\":1,\"errored\":0,\"canceled\":0,\"expired\":0},\"created_at\":\"2026-01-01T00:00:00Z\",\"expires_at\":\"2026-01-02T00:00:00Z\",\"ended_at\":\"2026-01-01T01:00:00Z\",\"archived_at\":null,\"cancel_initiated_at\":null,\"results_url\":\"" + resultsUrl + "\"}")
            .Add(resultsUrl, "{\"custom_id\":\"france\",\"result\":{\"type\":\"succeeded\"}}\n");
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }, handler);
        var batch = provider.ExperimentalBatch()!;
        var item = Assert.Single(await batch.DoGetResultsAsync(new BatchOperationCall("msgbatch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None));
        Assert.Equal("failed", item.Status);
        Assert.Equal("invalid_response", item.ErrorCode);
    }
}
