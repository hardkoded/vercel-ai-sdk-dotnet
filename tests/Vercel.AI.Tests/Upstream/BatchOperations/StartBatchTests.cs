// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests;
using BatchApi = Vercel.AI.Operations.Batch;

namespace Vercel.AI.Tests.Upstream.BatchOperations;

/// <summary>Upstream parity for <c>startBatch</c> in <c>batch.test.ts</c>.</summary>
public sealed class StartBatchTests
{
    private const string Prefix = "packages/ai/src/batch/batch.test.ts::";

    private const string OutputSchema =
        "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}";

    [Fact]
    [UpstreamTest(
        Prefix
            + "startBatch::sends the response format of the output specification with every text request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Sends_the_response_format_of_the_output_specification_with_every_text_request()
    {
        var api = new BatchTests.BatchFake();
        await BatchApi.StartBatchAsync(
            new BatchRequestOptions
            {
                Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(api),
                Output = OutputSpec.Object(OutputSchema),
                Requests = new[]
                {
                    new BatchRequest("request-1", "text", "mock-model-id") { Prompt = "translate" },
                    new BatchRequest("request-2", "text", "mock-model-id") { Prompt = "translate" },
                    new BatchRequest("request-3", "image", "mock-image-model-id")
                    {
                        Prompt = "a cat",
                    },
                },
            }
        );

        var requests = api.Last!.Requests!;
        foreach (var request in requests.Take(2))
        {
            var format = request.Options.GetProperty("responseFormat");
            Assert.Equal("json", format.GetProperty("type").GetString());
            Assert.Equal("object", format.GetProperty("schema").GetProperty("type").GetString());
            Assert.Equal(
                "string",
                format
                    .GetProperty("schema")
                    .GetProperty("properties")
                    .GetProperty("text")
                    .GetProperty("type")
                    .GetString()
            );
        }

        Assert.False(requests[2].Options.TryGetProperty("responseFormat", out _));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "startBatch::does not send a response format without an output specification",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Does_not_send_a_response_format_without_an_output_specification()
    {
        var api = new BatchTests.BatchFake();
        await BatchApi.StartBatchAsync(
            new BatchRequestOptions
            {
                Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(api),
                Requests = new[]
                {
                    new BatchRequest("request-1", "text", "mock-model-id") { Prompt = "translate" },
                },
            }
        );

        Assert.False(api.Last!.Requests![0].Options.TryGetProperty("responseFormat", out _));
    }
}
