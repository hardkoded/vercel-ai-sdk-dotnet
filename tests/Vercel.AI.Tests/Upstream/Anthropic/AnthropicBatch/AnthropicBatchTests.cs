// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch;

public sealed class AnthropicBatchTests
{
    [Theory]
    [InlineData(16L)]
    [InlineData(4096L)]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::applies the factory maxLineBytes setting of %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        const string resultsUrl = "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results";
        var handler = new RoutedHandler()
            .Add("https://api.anthropic.com/v1/messages/batches/msgbatch_123", "{\"id\":\"msgbatch_123\",\"type\":\"message_batch\",\"processing_status\":\"ended\",\"request_counts\":{\"processing\":0,\"succeeded\":1,\"errored\":0,\"canceled\":0,\"expired\":0},\"created_at\":\"2026-01-01T00:00:00Z\",\"expires_at\":\"2026-01-02T00:00:00Z\",\"ended_at\":\"2026-01-01T01:00:00Z\",\"archived_at\":null,\"cancel_initiated_at\":null,\"results_url\":\"" + resultsUrl + "\"}")
            .Add(resultsUrl, "{\"custom_id\":\"france\",\"result\":{\"type\":\"succeeded\",\"message\":{\"id\":\"msg_123\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-test\",\"content\":[{\"type\":\"text\",\"text\":\"Paris\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":3}}}}\n");
        var provider = AnthropicProvider.Create(new AnthropicOptions
        {
            ApiKey = "test-api-key",
            BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
        }, handler);
        var batch = provider.ExperimentalBatch()!;
        var results = batch.DoGetResultsAsync(new BatchOperationCall("msgbatch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        if (maxLineBytes == 16)
        {
            var error = await Assert.ThrowsAsync<DownloadError>(() => results);
            Assert.Equal("https://api.anthropic.com/v1/messages/batches/msgbatch_123/results", error.Url);
        }
        else
        {
            Assert.Single(await results);
        }
    }
}
