// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

public sealed class OpenAIBatchBatchResultLifecycleTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service > batch result lifecycle::fails an invalid item and continues with later results", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_an_invalid_item_and_continues_with_later_results()
    {
        var handler = new RoutedHandler()
            .Add("https://api.openai.com/v1/batches/batch_123", "{\"id\":\"batch_123\",\"status\":\"completed\",\"output_file_id\":\"file-output\"}")
            .Add("https://api.openai.com/v1/files/file-output/content",
                "{\"custom_id\":\"invalid\",\"response\":{\"status_code\":200,\"body\":{\"output\":42}}}\n"
                + "{\"custom_id\":\"valid\",\"response\":{\"status_code\":200,\"body\":{\"id\":\"resp_123\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Paris\"}]}]}}}\n");
        var batch = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ExperimentalBatch()!;

        var results = await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("invalid", results[0].Id);
        Assert.Equal("failed", results[0].Status);
        Assert.Equal("OpenAI returned an invalid Responses batch result.", results[0].ErrorMessage);
        Assert.Equal("invalid_response", results[0].ErrorCode);
        Assert.Equal("valid", results[1].Id);
        Assert.Equal("succeeded", results[1].Status);
        Assert.Equal("Paris", results[1].Text!.Content[0].Text);
    }
}
