// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

public sealed class GatewayBatchDoGetBatchResultsTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest("packages/gateway/src/gateway-batch.test.ts::GatewayBatch > doGetBatchResults::applies the factory maxLineBytes setting of %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        var handler = new RoutedHandler()
            .Add("https://api.test.com/batch/results", "{\"type\":\"text\",\"id\":\"req-1\",\"status\":\"succeeded\",\"result\":{\"content\":[{\"type\":\"text\",\"text\":\"pong 1\"}],\"finishReason\":{\"unified\":\"stop\"},\"response\":{\"modelId\":\"openai/gpt-5.6-luna\",\"timestamp\":\"2026-08-18T00:00:00.000Z\"},\"usage\":{\"inputTokens\":{\"total\":4,\"noCache\":4},\"outputTokens\":{\"total\":2,\"text\":2}},\"warnings\":[]}}\n");
        var provider = GatewayProvider.Create(new GatewayOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com",
            BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
        }, handler);
        var batch = provider.ExperimentalBatch()!;
        var results = batch.DoGetResultsAsync(new BatchOperationCall("job_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
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
}
