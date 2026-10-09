// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleBatch;

public sealed class GoogleBatchTests
{
    [Theory]
    [InlineData(16L)]
    [InlineData(4096L)]
    [UpstreamTest("packages/google/src/google-batch.test.ts::GoogleBatch::applies the factory maxLineBytes setting of %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_the_factory_maxLineBytes_setting(long maxLineBytes)
    {
        const string outputUrl = "https://generativelanguage.googleapis.com/download/v1beta/files/batch-output:download?alt=media";
        var handler = new RoutedHandler()
            .Add("https://generativelanguage.googleapis.com/v1beta/batches/batch-123", "{\"name\":\"batches/batch-123\",\"done\":true,\"metadata\":{\"state\":\"BATCH_STATE_SUCCEEDED\",\"output\":{\"responsesFile\":\"files/batch-output\"}}}")
            .Add(outputUrl, "{\"key\":\"france\",\"response\":{\"responseId\":\"response-france\",\"modelVersion\":\"gemini-2.5-flash\",\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Paris\"}]},\"finishReason\":\"STOP\"}]}}\n");
        var provider = GoogleProvider.Create(new GoogleOptions
        {
            ApiKey = "test-api-key",
            BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
        }, handler);
        var batch = provider.ExperimentalBatch()!;
        var results = batch.DoGetResultsAsync(new BatchOperationCall("batches/batch-123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        if (maxLineBytes == 16)
        {
            var error = await Assert.ThrowsAsync<DownloadError>(() => results);
            Assert.Equal("https://generativelanguage.googleapis.com/download/v1beta/files/batch-output:download?alt=media", error.Url);
        }
        else
        {
            Assert.Single(await results);
        }
    }
}
