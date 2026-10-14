// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch.OpenAIBatchTestSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

public sealed class OpenAIBatchServiceTests
{
    private const string Prefix = "packages/openai/src/openai-batch.test.ts::OpenAI batch service::";

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported request types before uploading a batch input file", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_request_types_before_uploading_a_batch_input_file()
    {
        var handler = new RecordingHandler();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Batch(handler).DoStartAsync(Call(requests: new[] { new NormalizedBatchRequest("image-1", "image", string.Empty, JsonDocument.Parse("{}").RootElement) }), CancellationToken.None));

        Assert.Equal("batch request type: image", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "creates a Responses batch from prepared JSONL requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_Responses_batch_from_prepared_JSONL_requests()
    {
        var handler = new RecordingHandler()
            .Add(FilesUrl, UploadResponse())
            .Add(BatchesUrl, BatchObject(status: "validating", requestCounts: Counts(2, 0, 0)).ToJsonString());
        var batch = Batch(handler, options => options.Headers["Provider-Header"] = "provider");

        var result = await batch.DoStartAsync(
            Call(
                requests: new[]
                {
                    TextRequest("france", "What is the capital of France?"),
                    TextRequest("germany", "What is the capital of Germany?", options => options["topK"] = 10),
                },
                headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                webhookUrl: "https://example.com/batch-webhook"),
            CancellationToken.None);

        Assert.Equal("batch_123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal("validating", result.RawStatus);
        Assert.Equal((2, 2, 0, 0), (result.RequestCounts!.Total, result.RequestCounts.Pending, result.RequestCounts.Completed, result.RequestCounts.Failed));
        Assert.Equal("2023-11-14T22:13:20.000Z", result.CreatedAt);
        Assert.Equal("2023-11-15T22:13:20.000Z", result.ExpiresAt);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorCode);
        Assert.Equal("{\"openai\":{\"inputFileId\":\"file-input\",\"inputFileExpiresAt\":\"2023-11-16T22:13:20.000Z\"}}", result.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(2, result.Warnings.Count);
        Assert.Null(result.Warnings[0].RequestId);
        Assert.Equal(("unsupported", "webhookUrl", "The OpenAI Batch API does not support per-batch webhook URLs."), (result.Warnings[0].Warning.Type, result.Warnings[0].Warning.Feature, result.Warnings[0].Warning.Details));
        Assert.Equal("germany", result.Warnings[1].RequestId);
        Assert.Equal(("unsupported", "topK", (string?)null), (result.Warnings[1].Warning.Type, result.Warnings[1].Warning.Feature, result.Warnings[1].Warning.Details));

        var upload = handler.Requests[0];
        Assert.Equal("batch", upload.Parts["purpose"].Text);
        Assert.Equal("created_at", upload.Parts["expires_after[anchor]"].Text);
        Assert.Equal("172800", upload.Parts["expires_after[seconds]"].Text);
        var file = upload.Parts["file"];
        Assert.Equal("batch.jsonl", file.FileName);
        Assert.Equal("application/jsonl", file.MediaType);

        var lines = file.Text.Trim().Split('\n').Select(line => JsonNode.Parse(line)!).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Equal("france", lines[0]["custom_id"]!.GetValue<string>());
        Assert.Equal("POST", lines[0]["method"]!.GetValue<string>());
        Assert.Equal("/v1/responses", lines[0]["url"]!.GetValue<string>());
        Assert.Equal("gpt-5.6", lines[0]["body"]!["model"]!.GetValue<string>());
        Assert.Equal("[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"What is the capital of France?\"}]}]", lines[0]["body"]!["input"]!.ToJsonString());
        Assert.Equal("germany", lines[1]["custom_id"]!.GetValue<string>());
        Assert.Equal("gpt-5.6", lines[1]["body"]!["model"]!.GetValue<string>());

        Assert.Equal("{\"input_file_id\":\"file-input\",\"endpoint\":\"/v1/responses\",\"completion_window\":\"24h\"}", handler.Requests[1].Body);
        foreach (var request in handler.Requests)
        {
            Assert.Equal("Bearer test-api-key", request.Headers["authorization"]);
            Assert.Equal("provider", request.Headers["provider-header"]);
            Assert.Equal("operation", request.Headers["operation-header"]);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects mixed models before uploading the batch input file", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_mixed_models_before_uploading_the_batch_input_file()
    {
        var handler = new RecordingHandler();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch(handler).DoStartAsync(
            Call(requests: new[] { TextRequest("first", "First request"), TextRequest("second", "Second request", modelId: "gpt-5-mini") }),
            CancellationToken.None));

        Assert.Equal("requests", error.Parameter);
        Assert.EndsWith("The OpenAI Batch API requires all requests in a batch to use the same model. Found \"gpt-5.6\" and \"gpt-5-mini\".", error.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "applies the inputFileExpiresAfter provider option to the input file upload", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_the_inputFileExpiresAfter_provider_option_to_the_input_file_upload()
    {
        var handler = new RecordingHandler().Add(FilesUrl, UploadResponse()).Add(BatchesUrl, BatchResponse());

        await Batch(handler).DoStartAsync(
            Call(requests: new[] { TextRequest("france", "What is the capital of France?") }, providerOptions: "{\"openai\":{\"inputFileExpiresAfter\":3600}}"),
            CancellationToken.None);

        Assert.Equal("created_at", handler.Requests[0].Parts["expires_after[anchor]"].Text);
        Assert.Equal("3600", handler.Requests[0].Parts["expires_after[seconds]"].Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits inputFileExpiresAt when the upload response carries no expiry", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_inputFileExpiresAt_when_the_upload_response_carries_no_expiry()
    {
        var handler = new RecordingHandler().Add(FilesUrl, "{\"id\":\"file-input\",\"object\":\"file\"}").Add(BatchesUrl, BatchResponse());

        var result = await Batch(handler).DoStartAsync(Call(requests: new[] { TextRequest("france", "What is the capital of France?") }), CancellationToken.None);

        Assert.Equal("{\"openai\":{\"inputFileId\":\"file-input\"}}", result.ProviderMetadata!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "appends an explicit compaction trigger to batch request input", Coverage = UpstreamCoverage.Covered)]
    public async Task Appends_an_explicit_compaction_trigger_to_batch_request_input()
    {
        var handler = new RecordingHandler().Add(FilesUrl, UploadResponse()).Add(BatchesUrl, BatchResponse());

        await Batch(handler).DoStartAsync(
            Call(requests: new[] { TextRequest("compact", "Compact this context.", options => options["providerOptions"] = new JsonObject { ["openai"] = new JsonObject { ["compactionTrigger"] = true } }) }),
            CancellationToken.None);

        var line = JsonNode.Parse(handler.Requests[0].Parts["file"].Text.Trim())!;
        Assert.Equal("[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Compact this context.\"}]},{\"type\":\"compaction_trigger\"}]", line["body"]!["input"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards the abort signal to file upload and batch creation", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_abort_signal_to_file_upload_and_batch_creation()
    {
        await AssertForwardsCancellation(
            2,
            () => new RecordingHandler().Add(FilesUrl, UploadResponse()).Add(BatchesUrl, BatchResponse()),
            (batch, token) => batch.DoStartAsync(Call(requests: new[] { TextRequest("france", "What is the capital of France?") }), token));
    }

    [Fact]
    [UpstreamTest(Prefix + "cancels a batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_a_batch()
    {
        var handler = new RecordingHandler().Add(CancelUrl, BatchObject(status: "cancelling").ToJsonString());
        var batch = Batch(handler, options => options.Headers["Provider-Header"] = "provider");

        var result = await batch.DoCancelAsync(Call("batch_123", headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" }), CancellationToken.None);

        Assert.Null(result.Status);
        Assert.Null(result.ProviderMetadata);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Equal("{}", request.Body);
        Assert.Equal("Bearer test-api-key", request.Headers["authorization"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        await AssertForwardsCancellation(1, () => new RecordingHandler(), (api, token) => api.DoCancelAsync(Call("batch_123"), token));
    }

    [Fact]
    [UpstreamTest(Prefix + "lists and normalizes a page of batches", Coverage = UpstreamCoverage.Covered)]
    public async Task Lists_and_normalizes_a_page_of_batches()
    {
        var page = new JsonObject
        {
            ["object"] = "list",
            ["data"] = new JsonArray(
                BatchObject(id: "batch_123", status: "in_progress", requestCounts: Counts(3, 1, 0)),
                BatchObject(id: "batch_122", status: "completed", requestCounts: Counts(2, 2, 0))),
            ["first_id"] = "batch_123",
            ["last_id"] = "batch_122",
            ["has_more"] = true,
        }.ToJsonString();
        var handler = new RecordingHandler().Add(BatchesUrl, page);
        var batch = Batch(handler, options => options.Headers["Provider-Header"] = "provider");

        var result = await batch.DoListAsync(Call(headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" }, limit: 2, cursor: "batch_122"), CancellationToken.None);

        Assert.Equal(2, result.Batches.Count);
        Assert.Equal(("batch_123", "pending", "in_progress"), (result.Batches[0].Id, result.Batches[0].Status, result.Batches[0].RawStatus));
        Assert.Equal((3, 2, 1, 0), (result.Batches[0].RequestCounts!.Total, result.Batches[0].RequestCounts!.Pending, result.Batches[0].RequestCounts!.Completed, result.Batches[0].RequestCounts!.Failed));
        Assert.Equal(("batch_122", "completed", "completed"), (result.Batches[1].Id, result.Batches[1].Status, result.Batches[1].RawStatus));
        Assert.Equal((2, 0, 2, 0), (result.Batches[1].RequestCounts!.Total, result.Batches[1].RequestCounts!.Pending, result.Batches[1].RequestCounts!.Completed, result.Batches[1].RequestCounts!.Failed));
        foreach (var listed in result.Batches)
        {
            Assert.Equal("2023-11-14T22:13:20.000Z", listed.CreatedAt);
            Assert.Equal("2023-11-15T22:13:20.000Z", listed.ExpiresAt);
            Assert.Null(listed.ErrorMessage);
            Assert.Null(listed.ProviderMetadata);
        }

        Assert.Equal("batch_122", result.NextCursor);
        Assert.Null(result.ProviderMetadata);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bearer test-api-key", request.Headers["authorization"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        Assert.Equal("?limit=2&after=batch_122", new Uri(request.Url).Query);
        await AssertForwardsCancellation(1, () => new RecordingHandler(), (api, token) => api.DoListAsync(Call(limit: 2, cursor: "batch_122"), token));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits the next cursor when there are no more batches", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_the_next_cursor_when_there_are_no_more_batches()
    {
        var handler = new RecordingHandler().Add(BatchesUrl, "{\"object\":\"list\",\"data\":[],\"first_id\":null,\"last_id\":null,\"has_more\":false}");

        var result = await Batch(handler).DoListAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Batches);
        Assert.Null(result.NextCursor);
        Assert.Null(result.ProviderMetadata);
    }

    [Theory]
    [InlineData("validating", "pending")]
    [InlineData("in_progress", "pending")]
    [InlineData("finalizing", "pending")]
    [InlineData("cancelling", "pending")]
    [InlineData("completed", "completed")]
    [InlineData("failed", "failed")]
    [InlineData("expired", "failed")]
    [InlineData("cancelled", "failed")]
    [InlineData("future_status", "pending")]
    [UpstreamTest(Prefix + "maps status %s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_status(string rawStatus, string status)
    {
        var handler = new RecordingHandler().Add(BatchUrl, BatchObject(status: rawStatus).ToJsonString());

        var result = await Batch(handler).DoGetStatusAsync(Call("batch_123"), CancellationToken.None);

        Assert.Equal(status, result.Status);
        Assert.Equal(rawStatus, result.RawStatus);
    }

    [Fact]
    [UpstreamTest(Prefix + "normalizes status counts, timestamps, and batch errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_status_counts_timestamps_and_batch_errors()
    {
        var errors = new JsonObject { ["data"] = new JsonArray(new JsonObject { ["code"] = "invalid_request", ["message"] = "Invalid input file." }) };
        var handler = new RecordingHandler().Add(BatchUrl, BatchObject(status: "failed", requestCounts: Counts(5, 2, 1), errors: errors).ToJsonString());

        var result = await Batch(handler).DoGetStatusAsync(Call("batch_123"), CancellationToken.None);

        Assert.Equal("failed", result.Status);
        Assert.Equal("failed", result.RawStatus);
        Assert.Equal((5, 2, 2, 1), (result.RequestCounts!.Total, result.RequestCounts.Pending, result.RequestCounts.Completed, result.RequestCounts.Failed));
        Assert.Equal("invalid_request", result.ErrorCode);
        Assert.Equal("Invalid input file.", result.ErrorMessage);
        Assert.Equal("2023-11-14T22:13:20.000Z", result.CreatedAt);
        Assert.Equal("2023-11-15T22:13:20.000Z", result.ExpiresAt);
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts incomplete batch error details", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_incomplete_batch_error_details()
    {
        var errors = new JsonObject { ["data"] = new JsonArray(new JsonObject { ["code"] = "invalid_request" }) };
        var handler = new RecordingHandler().Add(BatchUrl, BatchObject(status: "failed", errors: errors).ToJsonString());

        var result = await Batch(handler).DoGetStatusAsync(Call("batch_123"), CancellationToken.None);

        Assert.Equal("failed", result.Status);
        Assert.Equal("invalid_request", result.ErrorCode);
        Assert.Equal("OpenAI batch failed.", result.ErrorMessage);
    }
    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest(Prefix + "applies the factory maxLineBytes setting of %s", Coverage = UpstreamCoverage.Covered)]
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
        var stream = await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        var results = stream.ToListAsync().AsTask();
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
    [UpstreamTest(Prefix + "incrementally parses Responses results across chunk boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Incrementally_parses_Responses_results_across_chunk_boundaries()
    {
        var first = ResultLine("france", ResponsesResultBody("Paris"));
        var second = ResultLine("germany", ResponsesResultBody("Berlin"));
        var handler = new ChunkedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl, first[..19], first[19..] + "\r", "\n" + second[..31], second[31..]);

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
        Assert.Equal(new[] { BatchUrl, OutputUrl }, handler.RequestUrls);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves reasoning output and its provider metadata", Coverage = UpstreamCoverage.Covered)]
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
    [UpstreamTest(Prefix + "forwards the abort signal when retrieving results", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_abort_signal_when_retrieving_results()
    {
        await AssertForwardsCancellation(
            2,
            () => new RecordingHandler().Add(BatchUrl, BatchResponse(outputFileId: "file-output")).Add(OutputUrl, ResultLine("france", ResponsesResultBody("Paris"))),
            async (batch, token) =>
            {
                var stream = await batch.DoGetResultsAsync(Call("batch_123"), token);
                await stream.ToListAsync();
            });
    }

    [Fact]
    [UpstreamTest(Prefix + "errors the result stream when a JSONL line is malformed", Coverage = UpstreamCoverage.Covered)]
    public async Task Errors_the_result_stream_when_a_JSONL_line_is_malformed()
    {
        var handler = new ChunkedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl, ResultLine("france", ResponsesResultBody("Paris")) + "\n", "{not json}\n");
        var batch = Batch(handler);

        var stream = await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None);
        await using var reader = stream.GetAsyncEnumerator();

        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("france", reader.Current.Id);
        Assert.Equal("succeeded", reader.Current.Status);
        await Assert.ThrowsAsync<JsonParseException>(async () => await reader.MoveNextAsync());
    }

    [Fact]
    [UpstreamTest(Prefix + "streams output and error files after retrieving fresh batch metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_output_and_error_files_after_retrieving_fresh_batch_metadata()
    {
        var handler = new ChunkedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output", errorFileId: "file-errors"))
            .Add(OutputUrl, "{\"custom_id\":\"http-error\",\"response\":{\"status_code\":400,\"request_id\":\"request-error\",\"body\":{\"error\":{\"message\":\"Invalid request.\",\"type\":\"invalid_request_error\",\"param\":null,\"code\":\"invalid_request\"}}},\"error\":null}")
            .Add(ErrorsUrl,
                "{\"custom_id\":\"cancelled\",\"response\":null,\"error\":{\"code\":\"batch_cancelled\",\"message\":\"Batch cancelled.\"}}\n",
                "{\"custom_id\":\"expired\",\"response\":null,\"error\":{\"code\":\"batch_expired\",\"message\":\"Batch expired.\"}}\n",
                "{\"custom_id\":\"failed\",\"response\":null,\"error\":{\"code\":\"request_timeout\",\"message\":\"Request timed out.\"}}");

        var batch = Batch(handler);

        await batch.DoGetStatusAsync(Call("batch_123"), CancellationToken.None);
        var stream = await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None);
        var results = await stream.ToListAsync();

        Assert.Equal(new[] { BatchUrl, BatchUrl, OutputUrl, ErrorsUrl }, handler.RequestUrls);
        Assert.Equal(4, results.Count);
        Assert.Equal(("http-error", "failed", "Invalid request.", "invalid_request", "invalid_request_error", (int?)400), (results[0].Id, results[0].Status, results[0].ErrorMessage, results[0].ErrorCode, results[0].ErrorType, results[0].ErrorStatusCode));
        Assert.Equal(("cancelled", "cancelled", "Batch cancelled.", "batch_cancelled", (string?)null, (int?)null), (results[1].Id, results[1].Status, results[1].ErrorMessage, results[1].ErrorCode, results[1].ErrorType, results[1].ErrorStatusCode));
        Assert.Equal(("expired", "expired", "Batch expired.", "batch_expired", (string?)null, (int?)null), (results[2].Id, results[2].Status, results[2].ErrorMessage, results[2].ErrorCode, results[2].ErrorType, results[2].ErrorStatusCode));
        Assert.Equal(("failed", "failed", "Request timed out.", "request_timeout", (string?)null, (int?)null), (results[3].Id, results[3].Status, results[3].ErrorMessage, results[3].ErrorCode, results[3].ErrorType, results[3].ErrorStatusCode));
    }

    [Fact]
    [UpstreamTest(Prefix + "exposes batch support on the provider instead of its models", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_batch_support_on_the_provider_instead_of_its_models()
    {
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" });

        var batch = provider.ExperimentalBatch()!;
        Assert.True(batch.CanCancel);
        Assert.True(batch.CanList);

        Assert.IsNotAssignableFrom<IBatchApi>(provider.LanguageModel("gpt-5.6"));
        Assert.IsNotAssignableFrom<IBatchApi>(provider.ResponsesModel("gpt-5.6"));
        Assert.IsNotAssignableFrom<IBatchApi>(provider.ChatModel("gpt-5.6"));
        Assert.IsNotAssignableFrom<IBatchApi>(provider.CompletionModel("gpt-3.5-turbo-instruct"));
    }
}
