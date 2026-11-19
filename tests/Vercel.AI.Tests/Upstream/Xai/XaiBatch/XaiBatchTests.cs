// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Xai.XaiBatch.XaiBatchTestSupport;
using XaiProviderType = Vercel.AI.Xai.XaiProvider;

namespace Vercel.AI.Tests.Upstream.Xai.XaiBatch;

/// <summary>Port of <c>xai-batch.test.ts</c> &gt; <c>xAI batch</c>.</summary>
public sealed class XaiBatchTests
{
    private const string Prefix = "packages/xai/src/xai-batch.test.ts::xAI batch::";

    [Fact]
    [UpstreamTest(
        Prefix + "rejects unsupported request types before uploading a batch input file",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_unsupported_request_types_before_uploading_a_batch_input_file()
    {
        var server = new XaiBatchServer();
        var batch = CreateBatch(server);

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(
                Call(requests: [new NormalizedBatchRequest("audio-1", "audio", "grok", default)]),
                CancellationToken.None
            )
        );

        Assert.Equal("batch request type: audio", error.Functionality);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uploads image generation requests to the image batch endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uploads_image_generation_requests_to_the_image_batch_endpoint()
    {
        var server = new XaiBatchServer()
            .Respond(Files, new { id = "file_123", filename = "batch.jsonl" })
            .Respond(Batches, BatchResponse().ToJsonString());
        var batch = CreateBatch(server);

        await batch.DoStartAsync(
            Call(
                requests:
                [
                    new NormalizedBatchRequest(
                        "image-1",
                        "image",
                        "grok-imagine-image",
                        Json(
                            "{\"prompt\":\"A red panda\",\"n\":2,\"size\":null,\"aspectRatio\":\"16:9\",\"seed\":null,\"files\":null,\"mask\":null,\"providerOptions\":{\"xai\":{\"quality\":\"high\"}}}"
                        )
                    ),
                ]
            ),
            CancellationToken.None
        );

        var file = server.Calls[0].Multipart().Single(field => field.Name == "file");
        JsonAssert.Equal(
            JsonNode.Parse(file.Value.Trim()),
            "{\"custom_id\":\"image-1\",\"method\":\"POST\",\"url\":\"/v1/images/generations\",\"body\":{\"model\":\"grok-imagine-image\",\"prompt\":\"A red panda\",\"n\":2,\"response_format\":\"b64_json\",\"aspect_ratio\":\"16:9\",\"quality\":\"high\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uploads prepared JSONL requests and creates a file-backed batch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uploads_prepared_JSONL_requests_and_creates_a_file_backed_batch()
    {
        var server = new XaiBatchServer()
            .Respond(
                Files,
                new
                {
                    id = "file_123",
                    filename = "batch.jsonl",
                    expires_at = 1_700_172_800,
                }
            )
            .Respond(Batches, BatchResponse(state: State(2, 2, 0, 0, 0)).ToJsonString());
        var batch = CreateBatch(
            server,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );

        var result = await batch.DoStartAsync(
            Call(
                headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                webhookUrl: "https://example.com/batch-webhook",
                requests:
                [
                    Request("france", "What is the capital of France?"),
                    Request(
                        "germany",
                        "What is the capital of Germany?",
                        "grok-4.20-non-reasoning",
                        topK: 10
                    ),
                ]
            ),
            CancellationToken.None
        );

        Assert.Equal("batch_123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal((2, 2, 0, 0), Counts(result.RequestCounts!));
        Assert.Equal("2026-08-25T12:00:00Z", result.CreatedAt);
        Assert.Equal("2099-08-26T12:00:00Z", result.ExpiresAt);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"xai\":{\"inputFileId\":\"file_123\",\"inputFileExpiresAt\":\"2023-11-16T22:13:20.000Z\"}}"
        );
        var warning = Assert.Single(result.Warnings);
        Assert.Null(warning.RequestId);
        Assert.Equal("unsupported", warning.Warning.Type);
        Assert.Equal("webhookUrl", warning.Warning.Feature);
        Assert.Equal(
            "The xAI Batch API does not support per-batch webhook URLs.",
            warning.Warning.Details
        );

        var fields = server.Calls[0].Multipart();
        var file = fields.Single(field => field.Name == "file");
        Assert.Equal("batch.jsonl", file.FileName);
        Assert.Equal("application/jsonl", file.ContentType);
        Assert.DoesNotContain(fields, field => field.Name == "expires_after");
        var lines = file.Value.Trim().Split('\n').Select(line => JsonNode.Parse(line)).ToList();
        Assert.Equal(2, lines.Count);
        JsonAssert.Equal(
            lines[0],
            "{\"custom_id\":\"france\",\"method\":\"POST\",\"url\":\"/v1/responses\",\"body\":{\"model\":\"grok-4.3\",\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"What is the capital of France?\"}]}]}}"
        );
        JsonAssert.Equal(
            lines[1],
            "{\"custom_id\":\"germany\",\"method\":\"POST\",\"url\":\"/v1/responses\",\"body\":{\"model\":\"grok-4.20-non-reasoning\",\"top_k\":10,\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"What is the capital of Germany?\"}]}]}}"
        );
        JsonAssert.Equal(
            server.Calls[1].Json,
            "{\"name\":\"ai-sdk-text-batch\",\"input_file_id\":\"file_123\"}"
        );
        foreach (var call in server.Calls)
        {
            Assert.Equal("Bearer test-api-key", call.Headers["authorization"]);
            Assert.Equal("provider", call.Headers["provider-header"]);
            Assert.Equal("operation", call.Headers["operation-header"]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "appends inputFileExpiresAfter as expires_after before the file part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Appends_inputFileExpiresAfter_as_expires_after_before_the_file_part()
    {
        var server = new XaiBatchServer()
            .Respond(Files, new { id = "file_123", filename = "batch.jsonl" })
            .Respond(Batches, BatchResponse().ToJsonString());
        var batch = CreateBatch(server);

        var result = await batch.DoStartAsync(
            Call(
                requests: [Request("france", "What is the capital of France?")],
                providerOptions: Json("{\"xai\":{\"inputFileExpiresAfter\":172800}}")
            ),
            CancellationToken.None
        );

        var fields = server.Calls[0].Multipart();
        Assert.Equal("172800", fields.Single(field => field.Name == "expires_after").Value);
        // xAI rejects uploads where expires_after arrives after the file part
        Assert.Equal(new[] { "expires_after", "file" }, fields.Select(field => field.Name));
        JsonAssert.Equal(
            server.Calls[1].Json,
            "{\"name\":\"ai-sdk-text-batch\",\"input_file_id\":\"file_123\"}"
        );
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"xai\":{\"inputFileId\":\"file_123\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps xAI state counters and cancellation metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_xAI_state_counters_and_cancellation_metadata()
    {
        var server = new XaiBatchServer().Respond(
            BatchUrl,
            BatchResponse(
                    cancelTime: "2026-08-25T12:30:00Z",
                    cancelByXaiMessage: "Cancelled by user.",
                    state: State(4, 0, 2, 1, 1)
                )
                .ToJsonString()
        );
        var batch = CreateBatch(server);

        var status = await batch.DoGetStatusAsync(Call("batch_123"), CancellationToken.None);

        Assert.Equal("failed", status.Status);
        Assert.Equal((4, 0, 2, 2), Counts(status.RequestCounts!));
        Assert.Equal("Cancelled by user.", status.ErrorMessage);
        Assert.Equal("batch_cancelled", status.ErrorCode);
        Assert.Equal("2026-08-25T12:00:00Z", status.CreatedAt);
        Assert.Equal("2099-08-26T12:00:00Z", status.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(Prefix + "cancels a batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_a_batch()
    {
        var server = new XaiBatchServer().Respond(
            Cancel,
            BatchResponse(cancelTime: "2026-08-25T12:30:00Z").ToJsonString()
        );
        var batch = CreateBatch(
            server,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );
        using var abort = new CancellationTokenSource();

        var result = await batch.DoCancelAsync(
            Call(
                "batch_123",
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                abort.Token
            ),
            abort.Token
        );

        Assert.Null(result.ProviderMetadata);
        var call = server.Calls[0];
        Assert.Equal(HttpMethod.Post, call.Method);
        JsonAssert.Equal(call.Json, "{}");
        Assert.Equal("Bearer test-api-key", call.Headers["authorization"]);
        Assert.Equal("provider", call.Headers["provider-header"]);
        Assert.Equal("operation", call.Headers["operation-header"]);
        // The caller's abort signal reaches the request.
        Assert.True(call.TokenCanBeCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "lists and normalizes a page of batches",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Lists_and_normalizes_a_page_of_batches()
    {
        var server = new XaiBatchServer().Respond(
            Batches,
            new JsonObject
            {
                ["batches"] = new JsonArray(
                    BatchResponse("batch_123", State(3, 2, 1, 0, 0)),
                    BatchResponse("batch_122")
                ),
                ["pagination_token"] = "next/page",
            }.ToJsonString()
        );
        var batch = CreateBatch(
            server,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );
        using var abort = new CancellationTokenSource();

        var page = await batch.DoListAsync(
            Call(
                headers: new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                cancellationToken: abort.Token,
                limit: 2,
                cursor: "previous/page"
            ),
            abort.Token
        );

        Assert.Equal(2, page.Batches.Count);
        Assert.Equal("batch_123", page.Batches[0].Id);
        Assert.Equal("pending", page.Batches[0].Status);
        Assert.Equal((3, 2, 1, 0), Counts(page.Batches[0].RequestCounts!));
        Assert.Equal("2026-08-25T12:00:00Z", page.Batches[0].CreatedAt);
        Assert.Equal("2099-08-26T12:00:00Z", page.Batches[0].ExpiresAt);
        Assert.Equal("batch_122", page.Batches[1].Id);
        Assert.Equal("completed", page.Batches[1].Status);
        Assert.Equal((2, 0, 2, 0), Counts(page.Batches[1].RequestCounts!));
        Assert.Equal("next/page", page.NextCursor);

        var call = server.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Headers["authorization"]);
        Assert.Equal("provider", call.Headers["provider-header"]);
        Assert.Equal("operation", call.Headers["operation-header"]);
        Assert.Equal("?limit=2&pagination_token=previous%2Fpage", new Uri(call.Url).Query);
        Assert.True(call.TokenCanBeCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits the next cursor when there are no more batches",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_the_next_cursor_when_there_are_no_more_batches()
    {
        var server = new XaiBatchServer().Respond(
            Batches,
            "{\"batches\":[],\"pagination_token\":null}"
        );
        var batch = CreateBatch(server);

        var page = await batch.DoListAsync(Call(), CancellationToken.None);

        Assert.Empty(page.Batches);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits inconsistent request counts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_inconsistent_request_counts()
    {
        var server = new XaiBatchServer().Respond(
            BatchUrl,
            BatchResponse(state: State(2, 1, 2, 0, 0)).ToJsonString()
        );
        var batch = CreateBatch(server);

        var status = await batch.DoGetStatusAsync(Call("batch_123"), CancellationToken.None);

        Assert.Equal("pending", status.Status);
        Assert.Null(status.RequestCounts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects result retrieval while the batch is pending",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_result_retrieval_while_the_batch_is_pending()
    {
        var server = new XaiBatchServer().Respond(
            BatchUrl,
            BatchResponse(state: State(2, 1, 1, 0, 0)).ToJsonString()
        );
        var batch = CreateBatch(server);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        Assert.Equal("batchId", error.Parameter);
        Assert.EndsWith("xAI batch \"batch_123\" is not complete.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "paginates and converts successful and failed results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Paginates_and_converts_successful_and_failed_results()
    {
        var server = new XaiBatchServer()
            .Respond(BatchUrl, BatchResponse().ToJsonString())
            .Respond(Results, callNumber =>
                callNumber == 1
                    ? new JsonObject
                    {
                        ["results"] = new JsonArray(SuccessfulResult("france", "Paris")),
                        ["pagination_token"] = "next/page",
                    }.ToJsonString()
                    : "{\"results\":[{\"batch_request_id\":\"failed\",\"batch_result\":{\"error\":{\"code\":3,\"message\":\"Invalid request.\"}},\"error_message\":\"Invalid request.\"},{\"batch_request_id\":\"cancelled\",\"batch_result\":{\"error\":{\"code\":1,\"message\":\"Cancelled.\"}}}],\"pagination_token\":null}"
            );
        var batch = CreateBatch(server);

        var results = await ReadAsync(
            await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        Assert.Equal(3, results.Count);
        var france = results[0];
        Assert.Equal(("text", "france", "succeeded"), (france.Type, france.Id, france.Status));
        var text = france.Text!;
        Assert.Collection(
            text.Content,
            part =>
            {
                Assert.Equal("text", part.Type);
                Assert.Equal("Paris", part.Text);
            },
            part =>
            {
                Assert.Equal("reasoning", part.Type);
                Assert.Equal("Reasoning", part.Text);
            },
            part =>
            {
                Assert.Equal("source", part.Type);
                Assert.Equal("url", part.SourceType);
                Assert.Equal("https://example.com/source", part.Url);
            }
        );
        Assert.Equal("stop", text.FinishReason);
        Assert.Equal("stop", text.RawFinishReason);
        Assert.Equal(10, text.Usage!.InputTotal);
        Assert.Equal(8, text.Usage.NoCache);
        Assert.Equal(2, text.Usage.CacheRead);
        Assert.Equal(4, text.Usage.OutputTotal);
        Assert.Equal(3, text.Usage.Text);
        Assert.Equal(1, text.Usage.Reasoning);
        JsonAssert.Equal(
            text.ProviderMetadata!.Value,
            "{\"xai\":{\"costInUsdTicks\":123,\"serviceTier\":\"default\"}}"
        );

        Assert.Equal(
            ("text", "failed", "failed"),
            (results[1].Type, results[1].Id, results[1].Status)
        );
        Assert.Equal("Invalid request.", results[1].ErrorMessage);
        Assert.Equal("3", results[1].ErrorCode);
        Assert.Equal(
            ("text", "cancelled", "cancelled"),
            (results[2].Type, results[2].Id, results[2].Status)
        );
        Assert.Equal("Cancelled.", results[2].ErrorMessage);
        Assert.Equal("1", results[2].ErrorCode);
        Assert.Equal(
            new[] { BatchUrl, ResultsPage1, ResultsPage2 },
            server.Calls.Select(call => call.Url)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "converts image generation results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Converts_image_generation_results()
    {
        var server = new XaiBatchServer()
            .Respond(BatchUrl, BatchResponse().ToJsonString())
            .Respond(
                Results,
                "{\"results\":[{\"batch_request_id\":\"image-1\",\"batch_result\":{\"response\":{\"image_generation\":{\"data\":[{\"b64_json\":\"aGVsbG8=\",\"revised_prompt\":\"A vivid red panda\"}],\"usage\":{\"cost_in_usd_ticks\":42}}},\"error\":{\"code\":0,\"message\":\"\"}}}],\"pagination_token\":null}"
            );
        var batch = CreateBatch(server);

        var results = await ReadAsync(
            await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        var item = Assert.Single(results);
        Assert.Equal(("image", "image-1", "succeeded"), (item.Type, item.Id, item.Status));
        Assert.Equal(new object?[] { "aGVsbG8=" }, item.Image!.Images);
        JsonAssert.Equal(
            item.Image.ProviderMetadata!.Value,
            "{\"xai\":{\"images\":[{\"revisedPrompt\":\"A vivid red panda\"}],\"costInUsdTicks\":42}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns moderated image generation results as failed items",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_moderated_image_generation_results_as_failed_items()
    {
        var server = new XaiBatchServer()
            .Respond(BatchUrl, BatchResponse().ToJsonString())
            .Respond(
                Results,
                "{\"results\":[{\"batch_request_id\":\"image-1\",\"batch_result\":{\"response\":{\"image_generation\":{\"data\":[{\"url\":null,\"b64_json\":null,\"respect_moderation\":false}]}},\"error\":{\"code\":0,\"message\":\"\"}}}],\"pagination_token\":null}"
            );
        var batch = CreateBatch(server);

        var results = await ReadAsync(
            await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        var item = Assert.Single(results);
        Assert.Equal(("image", "image-1", "failed"), (item.Type, item.Id, item.Status));
        Assert.Equal(
            "Image generation was blocked due to a content policy violation.",
            item.ErrorMessage
        );
        Assert.Null(item.ErrorCode);
        Assert.Null(item.Image);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves tool calls and fails invalid items without stopping later results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_tool_calls_and_fails_invalid_items_without_stopping_later_results()
    {
        var toolCall = ChatResultBody("");
        toolCall["choices"] = JsonNode.Parse(
            "[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"reasoning_content\":null,\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]"
        );
        var server = new XaiBatchServer()
            .Respond(BatchUrl, BatchResponse().ToJsonString())
            .Respond(
                Results,
                new JsonObject
                {
                    ["results"] = new JsonArray(
                        new JsonObject
                        {
                            ["batch_request_id"] = "invalid",
                            ["batch_result"] = new JsonObject
                            {
                                ["response"] = new JsonObject
                                {
                                    ["chat_get_completion"] = new JsonObject { ["choices"] = 42 },
                                },
                            },
                        },
                        new JsonObject
                        {
                            ["batch_request_id"] = "tool-call",
                            ["batch_result"] = new JsonObject
                            {
                                ["response"] = new JsonObject
                                {
                                    ["chat_get_completion"] = toolCall,
                                },
                            },
                        },
                        SuccessfulResult("valid", "Berlin")
                    ),
                    ["pagination_token"] = null,
                }.ToJsonString()
            );
        var batch = CreateBatch(server);

        var results = await ReadAsync(
            await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        Assert.Equal(3, results.Count);
        Assert.Equal(
            ("text", "invalid", "failed"),
            (results[0].Type, results[0].Id, results[0].Status)
        );
        Assert.Equal("invalid_response", results[0].ErrorCode);

        Assert.Equal(
            ("text", "tool-call", "succeeded"),
            (results[1].Type, results[1].Id, results[1].Status)
        );
        Assert.Collection(
            results[1].Text!.Content,
            part =>
            {
                Assert.Equal("tool-call", part.Type);
                Assert.Equal("call_1", part.ToolCallId);
                Assert.Equal("weather", part.ToolName);
                Assert.Equal("{}", part.Text);
            },
            part =>
            {
                Assert.Equal("source", part.Type);
                Assert.Equal("https://example.com/source", part.Url);
            }
        );

        Assert.Equal(
            ("text", "valid", "succeeded"),
            (results[2].Type, results[2].Id, results[2].Status)
        );
        Assert.Collection(
            results[2].Text!.Content,
            part => Assert.Equal("Berlin", part.Text),
            part => Assert.Equal("Reasoning", part.Text),
            part => Assert.Equal("source", part.Type)
        );
        JsonAssert.Equal(
            results[2].Text!.ProviderMetadata!.Value,
            "{\"xai\":{\"costInUsdTicks\":123,\"serviceTier\":\"default\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves provider-executed tool calls and final text from batch transcripts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_provider_executed_tool_calls_and_final_text_from_batch_transcripts()
    {
        var transcript = ChatResultBody("");
        transcript["choices"] = JsonNode.Parse(
            "[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"web-search-1\",\"type\":\"function\",\"function\":{\"name\":\"web_search\",\"arguments\":\"{\\\"query\\\":\\\"Vercel\\\"}\"}}]},\"finish_reason\":\"\"},"
                + "{\"index\":1,\"message\":{\"role\":\"tool\",\"content\":\"Search results\",\"tool_calls\":[{\"id\":\"web-search-1\",\"type\":\"function\",\"function\":{\"name\":\"web_search\",\"arguments\":\"{\\\"query\\\":\\\"Vercel\\\"}\"}}]},\"finish_reason\":\"\"},"
                + "{\"index\":2,\"message\":{\"role\":\"assistant\",\"content\":\"Final answer\",\"tool_calls\":null},\"finish_reason\":\"stop\"},"
                + "{\"index\":3,\"message\":{\"role\":\"tool\",\"content\":null,\"tool_calls\":[]},\"finish_reason\":\"\"}]"
        );
        var clientTool = ChatResultBody("");
        clientTool["choices"] = JsonNode.Parse(
            "[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"client-web-search-1\",\"type\":\"function\",\"function\":{\"name\":\"web_search\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]"
        );
        var server = new XaiBatchServer()
            .Respond(BatchUrl, BatchResponse().ToJsonString())
            .Respond(
                Results,
                new JsonObject
                {
                    ["results"] = new JsonArray(
                        new JsonObject
                        {
                            ["batch_request_id"] = "provider-tool",
                            ["batch_result"] = new JsonObject
                            {
                                ["response"] = new JsonObject
                                {
                                    ["chat_get_completion"] = transcript,
                                },
                            },
                        },
                        new JsonObject
                        {
                            ["batch_request_id"] = "client-tool-with-provider-name",
                            ["batch_result"] = new JsonObject
                            {
                                ["response"] = new JsonObject
                                {
                                    ["chat_get_completion"] = clientTool,
                                },
                            },
                        }
                    ),
                    ["pagination_token"] = null,
                }.ToJsonString()
            );
        var batch = CreateBatch(server);

        var results = await ReadAsync(
            await batch.DoGetResultsAsync(Call("batch_123"), CancellationToken.None)
        );

        Assert.Equal(2, results.Count);
        var providerTool = results[0];
        Assert.Equal(
            ("text", "provider-tool", "succeeded"),
            (providerTool.Type, providerTool.Id, providerTool.Status)
        );
        Assert.Collection(
            providerTool.Text!.Content,
            part =>
            {
                Assert.Equal("tool-call", part.Type);
                Assert.Equal("web-search-1", part.ToolCallId);
                Assert.Equal("web_search", part.ToolName);
                Assert.Equal("{\"query\":\"Vercel\"}", part.Text);
                Assert.True(part.ProviderExecuted);
                Assert.True(part.Dynamic);
            },
            part =>
            {
                Assert.Equal("tool-result", part.Type);
                Assert.Equal("web-search-1", part.ToolCallId);
                Assert.Equal("web_search", part.ToolName);
                JsonAssert.Equal(part.Output!.Value, "\"Search results\"");
                Assert.True(part.Dynamic);
            },
            part =>
            {
                Assert.Equal("text", part.Type);
                Assert.Equal("Final answer", part.Text);
            },
            part =>
            {
                Assert.Equal("source", part.Type);
                Assert.Equal("https://example.com/source", part.Url);
            }
        );
        Assert.Equal("stop", providerTool.Text.FinishReason);
        Assert.Equal("stop", providerTool.Text.RawFinishReason);

        var client = results[1];
        Assert.Equal(
            ("text", "client-tool-with-provider-name", "succeeded"),
            (client.Type, client.Id, client.Status)
        );
        Assert.Collection(
            client.Text!.Content,
            part =>
            {
                Assert.Equal("tool-call", part.Type);
                Assert.Equal("client-web-search-1", part.ToolCallId);
                Assert.Equal("web_search", part.ToolName);
                Assert.Equal("{}", part.Text);
                Assert.Null(part.ProviderExecuted);
                Assert.Null(part.Dynamic);
            },
            part => Assert.Equal("source", part.Type)
        );
        Assert.Equal("tool-calls", client.Text.FinishReason);
        Assert.Equal("tool_calls", client.Text.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes batch support on the provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Exposes_batch_support_on_the_provider()
    {
        var provider = XaiProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" }
        );

        var batch = provider.ExperimentalBatch();

        Assert.NotNull(batch);
        Assert.IsAssignableFrom<IBatchApi>(batch);
        Assert.True(batch!.CanCancel);
        Assert.True(batch.CanList);
        foreach (
            var model in new object[]
            {
                provider.LanguageModel("grok-4.3"),
                provider.ResponsesModel("grok-4.3"),
                provider.ResponsesModel("grok-4.3"),
            }
        )
        {
            Assert.IsNotAssignableFrom<IBatchApi>(model);
        }
    }

    private static (int Total, int Pending, int Completed, int Failed) Counts(
        BatchRequestCounts counts
    ) => (counts.Total, counts.Pending, counts.Completed, counts.Failed);
}
