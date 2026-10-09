// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

public sealed class OpenAIBatchServiceTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service::applies the factory maxLineBytes setting of %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        const string outputUrl = "https://api.openai.com/v1/files/file-output/content";
        var handler = new RoutedHandler()
            .Add("https://api.openai.com/v1/batches/batch_123", "{\"id\":\"batch_123\",\"object\":\"batch\",\"status\":\"completed\",\"output_file_id\":\"file-output\",\"error_file_id\":null,\"request_counts\":{\"total\":1,\"completed\":1,\"failed\":0}}")
            .Add(outputUrl, "{\"custom_id\":\"france\",\"response\":{\"status_code\":200,\"request_id\":\"openai-france\",\"body\":{\"id\":\"resp_123\",\"created_at\":1700000000,\"model\":\"gpt-5.6\",\"output\":[{\"type\":\"message\",\"role\":\"assistant\",\"id\":\"msg_123\",\"content\":[{\"type\":\"output_text\",\"text\":\"Paris\",\"annotations\":[]}]}]}},\"error\":null}\n");
        var provider = OpenAIProvider.Create(new OpenAIOptions
        {
            ApiKey = "test-api-key",
            BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
        }, handler);
        var batch = provider.ExperimentalBatch()!;
        var results = batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        if (maxLineBytes == 16)
        {
            var error = await Assert.ThrowsAsync<DownloadError>(() => results);
            Assert.Equal("https://api.openai.com/v1/files/file-output/content", error.Url);
        }
        else
        {
            Assert.Single(await results);
        }
    }
}
