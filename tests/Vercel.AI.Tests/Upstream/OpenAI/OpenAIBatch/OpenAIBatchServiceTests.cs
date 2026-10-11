// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch.OpenAIBatchTestSupport;

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

    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service::incrementally parses Responses results across chunk boundaries", Coverage = UpstreamCoverage.Partial, Note = "The test handler returns one body, so chunk boundaries and the request URL order are not asserted.")]
    public async Task Incrementally_parses_Responses_results_across_chunk_boundaries()
    {
        var first = ResultLine("france", ResponsesResultBody("Paris"));
        var second = ResultLine("germany", ResponsesResultBody("Berlin"));
        var handler = new RoutedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl, first + "\r\n" + second);

        var results = await GetResultsAsync(handler);

        Assert.Equal(2, results.Count);
        Assert.Equal("france", results[0].Id);
        Assert.Equal("succeeded", results[0].Status);
        var text = results[0].Text!;
        Assert.Equal("Paris", Assert.Single(text.Content).Text);
        Assert.Equal("stop", text.FinishReason);
        Assert.Null(text.RawFinishReason);
        Assert.Equal(10, text.Usage!.InputTotal);
        Assert.Equal(8, text.Usage.NoCache);
        Assert.Equal(2, text.Usage.CacheRead);
        Assert.Equal(3, text.Usage.OutputTotal);
        Assert.Equal(2, text.Usage.Text);
        Assert.Equal(1, text.Usage.Reasoning);
        Assert.Equal("resp_123", text.ResponseId);
        Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc), text.Timestamp);
        Assert.Equal("gpt-5.6", text.ModelId);
        var openai = text.ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal("resp_123", openai.GetProperty("responseId").GetString());
        Assert.Equal("default", openai.GetProperty("serviceTier").GetString());
        Assert.Equal("germany", results[1].Id);
        Assert.Equal("succeeded", results[1].Status);
        Assert.Equal("Berlin", Assert.Single(results[1].Text!.Content).Text);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service::preserves reasoning output and its provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_reasoning_output_and_its_provider_metadata()
    {
        var reasoning = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "reasoning",
            ["id"] = "reasoning-123",
            ["encrypted_content"] = "encrypted-reasoning",
            ["summary"] = new System.Text.Json.Nodes.JsonArray(new System.Text.Json.Nodes.JsonObject { ["type"] = "summary_text", ["text"] = "I should answer directly." }),
        };
        var handler = new RoutedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl, ResultLine("reasoning", WithOutput(ResponsesResultBody("Paris"), reasoning, MessageOutput("Paris"))));

        var item = Assert.Single(await GetResultsAsync(handler));

        Assert.Equal("reasoning", item.Id);
        Assert.Equal("succeeded", item.Status);
        var content = item.Text!.Content;
        Assert.Equal(2, content.Count);
        Assert.Equal("reasoning", content[0].Type);
        Assert.Equal("I should answer directly.", content[0].Text);
        var openai = content[0].ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal("reasoning-123", openai.GetProperty("itemId").GetString());
        Assert.Equal("encrypted-reasoning", openai.GetProperty("reasoningEncryptedContent").GetString());
        Assert.Equal("text", content[1].Type);
        Assert.Equal("Paris", content[1].Text);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service::streams output and error files after retrieving fresh batch metadata", Coverage = UpstreamCoverage.Partial, Note = "Request URL order is not asserted.")]
    public async Task Streams_output_and_error_files_after_retrieving_fresh_batch_metadata()
    {
        var handler = new RoutedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output", errorFileId: "file-errors"))
            .Add(OutputUrl, "{\"custom_id\":\"http-error\",\"response\":{\"status_code\":400,\"request_id\":\"request-error\",\"body\":{\"error\":{\"message\":\"Invalid request.\",\"type\":\"invalid_request_error\",\"param\":null,\"code\":\"invalid_request\"}}},\"error\":null}")
            .Add(ErrorsUrl,
                "{\"custom_id\":\"cancelled\",\"response\":null,\"error\":{\"code\":\"batch_cancelled\",\"message\":\"Batch cancelled.\"}}\n"
                + "{\"custom_id\":\"expired\",\"response\":null,\"error\":{\"code\":\"batch_expired\",\"message\":\"Batch expired.\"}}\n"
                + "{\"custom_id\":\"failed\",\"response\":null,\"error\":{\"code\":\"request_timeout\",\"message\":\"Request timed out.\"}}");

        var results = await GetResultsAsync(handler);

        Assert.Equal(4, results.Count);
        Assert.Equal(("http-error", "failed", "Invalid request.", "invalid_request", "invalid_request_error", (int?)400), (results[0].Id, results[0].Status, results[0].ErrorMessage, results[0].ErrorCode, results[0].ErrorType, results[0].ErrorStatusCode));
        Assert.Equal(("cancelled", "cancelled", "Batch cancelled.", "batch_cancelled", (string?)null, (int?)null), (results[1].Id, results[1].Status, results[1].ErrorMessage, results[1].ErrorCode, results[1].ErrorType, results[1].ErrorStatusCode));
        Assert.Equal(("expired", "expired", "Batch expired.", "batch_expired", (string?)null, (int?)null), (results[2].Id, results[2].Status, results[2].ErrorMessage, results[2].ErrorCode, results[2].ErrorType, results[2].ErrorStatusCode));
        Assert.Equal(("failed", "failed", "Request timed out.", "request_timeout", (string?)null, (int?)null), (results[3].Id, results[3].Status, results[3].ErrorMessage, results[3].ErrorCode, results[3].ErrorType, results[3].ErrorStatusCode));
    }
}
