// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class OpenAIBatchNoOutputTests
{
    [Theory]
    [InlineData("{\"id\":\"resp_1\"}", "OpenAI Responses returned no output.")]
    [InlineData("{\"id\":\"resp_1\",\"output\":null,\"incomplete_details\":{\"reason\":\"max_output_tokens\"}}", "OpenAI Responses returned no output (max_output_tokens).")]
    public async Task Fails_a_succeeded_row_that_has_no_output(string body, string message)
    {
        var handler = new RoutedHandler()
            .Add("https://api.openai.com/v1/batches/batch_123", "{\"id\":\"batch_123\",\"status\":\"completed\",\"output_file_id\":\"file-output\"}")
            .Add("https://api.openai.com/v1/files/file-output/content", "{\"custom_id\":\"row\",\"response\":{\"status_code\":200,\"body\":" + body + "}}\n");
        var batch = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ExperimentalBatch()!;

        var item = Assert.Single(await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None));

        Assert.Equal("failed", item.Status);
        Assert.Equal("invalid_response", item.ErrorCode);
        Assert.Equal(message, item.ErrorMessage);
    }
}
