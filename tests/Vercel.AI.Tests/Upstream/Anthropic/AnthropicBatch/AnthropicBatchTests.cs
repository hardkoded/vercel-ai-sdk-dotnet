// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch;

public sealed class AnthropicBatchTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
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
        var stream = await batch.DoGetResultsAsync(new BatchOperationCall("msgbatch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        var results = stream.ToListAsync().AsTask();
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

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects message threads before making an API request", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Rejects_message_threads_before_making_an_API_request()
    {
        var calls = 0;
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key" }, new CountingHandler(() => calls++));
        var batch = provider.ExperimentalBatch()!;
        var options = System.Text.Json.JsonDocument.Parse("{\"prompt\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}],\"providerOptions\":{\"anthropic\":{\"thread\":{\"type\":\"create\"}}}}").RootElement.Clone();
        var call = new BatchOperationCall(null, null, new Dictionary<string, string>(), CancellationToken.None, requests: new[] { new NormalizedBatchRequest("thread-request", "text", "claude-3-haiku-20240307", options) });

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Contains("Message Batches do not support message threads", error.Message);
        Assert.Equal(0, calls);
    }

    private const string BatchesUrl = "https://api.anthropic.com/v1/messages/batches";
    private const string ResultsUrl = "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results";

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported request types before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_request_types_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(new NormalizedBatchRequest("image-1", "image", "claude-3-haiku-20240307", Json("{}")));

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("batch request type: image", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts a batch from prepared requests and combines batch and inferred betas", Coverage = UpstreamCoverage.Covered)]
    public async Task Starts_a_batch_from_prepared_requests_and_combines_batch_and_inferred_betas()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress"), ("request_counts", Counts(2, 0, 0, 0, 0)), ("results_url", null)));
        var batch = CreateBatch(handler, new Dictionary<string, string> { ["Provider-Header"] = "provider", ["Anthropic-Beta"] = "provider-header-beta" });
        var call = StartCall(
            [
                TextRequest("france", "What is the capital of France?", new JsonObject { ["maxOutputTokens"] = 100, ["frequencyPenalty"] = 0.5, ["providerOptions"] = new JsonObject { ["anthropic"] = new JsonObject { ["serviceTier"] = "auto" } } }),
                TextRequest("germany", "What is the capital of Germany?", new JsonObject
                {
                    ["maxOutputTokens"] = 200,
                    ["providerOptions"] = new JsonObject { ["anthropic"] = new JsonObject { ["fallbacks"] = new JsonArray(new JsonObject { ["model"] = "claude-sonnet-4-5", ["max_tokens"] = 150 }) } },
                }, "claude-sonnet-4-5"),
            ],
            new JsonObject { ["anthropic"] = new JsonObject { ["anthropicBeta"] = new JsonArray("batch-beta") } },
            new Dictionary<string, string> { ["Operation-Header"] = "operation", ["anthropic-beta"] = "operation-header-beta" },
            "https://example.com/batch-webhook");

        var result = await batch.DoStartAsync(call, CancellationToken.None);

        Assert.Equal("msgbatch_123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal("in_progress", result.RawStatus);
        Assert.Equal(new[] { 2, 2, 0, 0 }, new[] { result.RequestCounts!.Total, result.RequestCounts.Pending, result.RequestCounts.Completed, result.RequestCounts.Failed });
        Assert.Equal(2, result.Warnings.Count);
        Assert.Null(result.Warnings[0].RequestId);
        Assert.Equal("unsupported", result.Warnings[0].Warning.Type);
        Assert.Equal("webhookUrl", result.Warnings[0].Warning.Feature);
        Assert.Equal("The Anthropic Message Batches API does not support completion webhooks.", result.Warnings[0].Warning.Details);
        Assert.Equal("france", result.Warnings[1].RequestId);
        Assert.Equal("unsupported", result.Warnings[1].Warning.Type);
        Assert.Equal("frequencyPenalty", result.Warnings[1].Warning.Feature);

        var request = Assert.Single(handler.Requests);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(request.Body), new JsonObject
        {
            ["requests"] = new JsonArray(
                new JsonObject
                {
                    ["custom_id"] = "france",
                    ["params"] = new JsonObject
                    {
                        ["model"] = "claude-3-haiku-20240307",
                        ["max_tokens"] = 100,
                        ["service_tier"] = "auto",
                        ["messages"] = Messages("What is the capital of France?"),
                    },
                },
                new JsonObject
                {
                    ["custom_id"] = "germany",
                    ["params"] = new JsonObject
                    {
                        ["model"] = "claude-sonnet-4-5",
                        ["max_tokens"] = 200,
                        ["fallbacks"] = new JsonArray(new JsonObject { ["model"] = "claude-sonnet-4-5", ["max_tokens"] = 150 }),
                        ["messages"] = Messages("What is the capital of Germany?"),
                    },
                }),
        }));
        Assert.Equal("test-api-key", request.Headers["x-api-key"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        var betas = request.Headers["anthropic-beta"].Split(',').Select(beta => beta.Trim()).ToList();
        Assert.Contains("batch-beta", betas);
        Assert.Contains("server-side-fallback-2026-06-01", betas);
        Assert.Equal(2, betas.Count);
        Assert.Equal(betas.Count, betas.Distinct().Count());
    }

    [Theory]
    [InlineData("\"batch-beta\"")]
    [InlineData("[123]")]
    [InlineData("null")]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid batch-level betas %j before making an API request", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public async Task Rejects_invalid_batch_level_betas_before_making_an_API_request(string anthropicBeta)
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall([TextRequest("request-1", "Hello")], new JsonObject { ["anthropic"] = new JsonObject { ["anthropicBeta"] = JsonNode.Parse(anthropicBeta) } });

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects per-request betas before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_per_request_betas_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Hello", new JsonObject { ["providerOptions"] = new JsonObject { ["anthropic"] = new JsonObject { ["anthropicBeta"] = new JsonArray("request-beta") } } }));

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("per-request providerOptions.anthropic.anthropicBeta", error.Functionality);
        Assert.Equal("Anthropic Message Batches do not support per-request betas (request \"request-1\"). Set providerOptions.anthropic.anthropicBeta on startBatch instead.", error.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts a batch with thinking binding controls", Coverage = UpstreamCoverage.Covered)]
    public async Task Starts_a_batch_with_thinking_binding_controls()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress")));
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest(
            "preserved-thinking",
            "Continue the conversation.",
            new JsonObject
            {
                ["maxOutputTokens"] = 4096,
                ["providerOptions"] = new JsonObject
                {
                    ["anthropic"] = new JsonObject { ["thinking"] = new JsonObject { ["type"] = "adaptive", ["blockBinding"] = new JsonObject { ["prefixMismatchBehavior"] = "error" } } },
                },
            },
            "claude-fable-5-1"));

        await batch.DoStartAsync(call, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(request.Body), new JsonObject
        {
            ["requests"] = new JsonArray(new JsonObject
            {
                ["custom_id"] = "preserved-thinking",
                ["params"] = new JsonObject
                {
                    ["max_tokens"] = 4096,
                    ["messages"] = Messages("Continue the conversation."),
                    ["model"] = "claude-fable-5-1",
                    ["thinking"] = new JsonObject { ["block_binding"] = new JsonObject { ["prefix_mismatch_behavior"] = "error" }, ["type"] = "adaptive" },
                },
            }),
        }));
        Assert.Equal("thinking-binding-controls-2026-08-01", request.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects structured-output modes that require start-call context", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_structured_output_modes_that_require_start_call_context()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Return JSON", new JsonObject
        {
            ["responseFormat"] = new JsonObject
            {
                ["type"] = "json",
                ["schema"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject { ["answer"] = new JsonObject { ["type"] = "string" } } },
            },
        }));

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("batch responseFormat JSON-tool fallback", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects aliased provider tool names that cannot be restored later", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_aliased_provider_tool_names_that_cannot_be_restored_later()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Search", new JsonObject
        {
            ["tools"] = new JsonArray(new JsonObject { ["type"] = "provider", ["id"] = "anthropic.web_search_20250305", ["name"] = "custom_search", ["args"] = new JsonObject() }),
        }));

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("aliased provider tool names in batches", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::allows web tools that implicitly provision code execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_web_tools_that_implicitly_provision_code_execution()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress")));
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Search", new JsonObject
        {
            ["tools"] = new JsonArray(new JsonObject { ["type"] = "provider", ["id"] = "anthropic.web_search_20260209", ["name"] = "web_search", ["args"] = new JsonObject() }),
        }));

        await batch.DoStartAsync(call, CancellationToken.None);

        var tools = JsonNode.Parse(Assert.Single(handler.Requests).Body)!["requests"]![0]!["params"]!["tools"]!.AsArray();
        var tool = Assert.Single(tools);
        Assert.Equal("web_search_20260209", tool!["type"]!.GetValue<string>());
        Assert.Equal("web_search", tool["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::does not treat a custom code_execution function as the provider tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_treat_a_custom_code_execution_function_as_the_provider_tool()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress")));
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Search", new JsonObject
        {
            ["tools"] = new JsonArray(
                new JsonObject { ["type"] = "provider", ["id"] = "anthropic.web_search_20260209", ["name"] = "web_search", ["args"] = new JsonObject() },
                new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = "code_execution",
                    ["description"] = "A custom client-side function",
                    ["inputSchema"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject(), ["additionalProperties"] = false },
                }),
        }));

        await batch.DoStartAsync(call, CancellationToken.None);

        var tools = JsonNode.Parse(Assert.Single(handler.Requests).Body)!["requests"]![0]!["params"]!["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        Assert.Equal("web_search_20260209", tools[0]!["type"]!.GetValue<string>());
        Assert.Equal("web_search", tools[0]!["name"]!.GetValue<string>());
        Assert.Equal("code_execution", tools[1]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts an on-demand compaction batch request with the inferred beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Starts_an_on_demand_compaction_batch_request_with_the_inferred_beta_header()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress")));
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("compact-conversation", "Summarize this conversation.", new JsonObject
        {
            ["maxOutputTokens"] = 4096,
            ["providerOptions"] = new JsonObject
            {
                ["anthropic"] = new JsonObject { ["compaction"] = new JsonObject { ["type"] = "summarize", ["instructions"] = "Preserve decisions and open questions." } },
            },
        }));

        await batch.DoStartAsync(call, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        var entry = JsonNode.Parse(request.Body)!["requests"]![0]!;
        Assert.Equal("compact-conversation", entry["custom_id"]!.GetValue<string>());
        Assert.Equal("summarize", entry["params"]!["compaction"]!["type"]!.GetValue<string>());
        Assert.Equal("Preserve decisions and open questions.", entry["params"]!["compaction"]!["instructions"]!.GetValue<string>());
        Assert.Contains("compact-2026-09-04", request.Headers["anthropic-beta"].Split(',').Select(beta => beta.Trim()));
    }

    [Theory]
    [InlineData("providerOptions.anthropic.speed", "{\"speed\":\"fast\"}", "Anthropic Message Batches do not support speed (request \"request-1\").")]
    [InlineData("providerOptions.anthropic.fallbacks[].speed", "{\"fallbacks\":[{\"model\":\"claude-sonnet-4-5\",\"speed\":\"standard\"}]}", "Anthropic Message Batches do not support fallback speed (request \"request-1\").")]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported $feature", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_feature(string feature, string options, string message)
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(TextRequest("request-1", "Hello", new JsonObject { ["providerOptions"] = new JsonObject { ["anthropic"] = JsonNode.Parse(options) } }));

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal(feature, error.Functionality);
        Assert.Equal(message, error.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid request IDs before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_request_IDs_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);

        var invalid = await Assert.ThrowsAsync<InvalidArgumentException>(() => batch.DoStartAsync(StartCall(TextRequest("invalid id", "Hello")), CancellationToken.None));
        var tooLong = await Assert.ThrowsAsync<InvalidArgumentException>(() => batch.DoStartAsync(StartCall(TextRequest(new string('a', 65), "Hello")), CancellationToken.None));

        Assert.Equal("requests", invalid.Parameter);
        Assert.EndsWith("Anthropic batch request ID \"invalid id\" must match ^[A-Za-z0-9_-]{1,64}$.", invalid.Message);
        Assert.Equal("requests", tooLong.Parameter);
        Assert.Contains("must match ^[A-Za-z0-9_-]{1,64}$.", tooLong.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects duplicate request IDs before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_duplicate_request_IDs_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall([TextRequest("duplicate", "First"), TextRequest("duplicate", "Second")]);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => batch.DoStartAsync(call, CancellationToken.None));

        Assert.Equal("requests", error.Parameter);
        Assert.EndsWith("Anthropic batch request IDs must be unique; duplicate ID \"duplicate\".", error.Message);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("in_progress", "pending")]
    [InlineData("canceling", "pending")]
    [InlineData("ended", "completed")]
    [InlineData("future_status", "pending")]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::maps status %s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_status(string rawStatus, string status)
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", rawStatus)));
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal(status, result.Status);
        Assert.Equal(rawStatus, result.RawStatus);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::normalizes request counts", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_request_counts()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("request_counts", Counts(0, 2, 1, 1, 1))));
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        Assert.Equal("ended", result.RawStatus);
        Assert.Equal(new[] { 5, 0, 2, 3 }, new[] { result.RequestCounts!.Total, result.RequestCounts.Pending, result.RequestCounts.Completed, result.RequestCounts.Failed });
        Assert.Equal("2024-09-24T18:37:24.100Z", result.CreatedAt);
        Assert.Equal("2024-09-25T18:37:24.100Z", result.ExpiresAt);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText()), new JsonObject
        {
            ["anthropic"] = new JsonObject
            {
                ["archivedAt"] = null,
                ["cancelInitiatedAt"] = null,
                ["endedAt"] = "2024-09-24T18:38:24.100Z",
                ["requestCounts"] = Counts(0, 2, 1, 1, 1),
                ["resultsUrl"] = ResultsUrl,
            },
        }));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::cancels a batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_a_batch()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "canceling")));
        var batch = CreateBatch(handler, new Dictionary<string, string> { ["Provider-Header"] = "provider" });
        using var source = new CancellationTokenSource();
        handler.OnSend = () => source.Cancel();

        var result = await batch.DoCancelAsync(OperationCall(new Dictionary<string, string> { ["Operation-Header"] = "operation" }, source.Token), source.Token);

        Assert.Null(result.ProviderMetadata);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Equal(BatchesUrl + "/msgbatch_123/cancel", request.Url);
        Assert.Equal("{}", request.Body);
        Assert.Equal("test-api-key", request.Headers["x-api-key"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        Assert.True(request.TokenCancelledByCaller);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::lists and normalizes a page of batches", Coverage = UpstreamCoverage.Covered)]
    public async Task Lists_and_normalizes_a_page_of_batches()
    {
        var page = new JsonObject
        {
            ["data"] = new JsonArray(
                BatchResponse(("id", "msgbatch_123"), ("processing_status", "in_progress"), ("request_counts", Counts(2, 1, 0, 0, 0))),
                BatchResponse(("id", "msgbatch_122"))),
            ["first_id"] = "msgbatch_123",
            ["last_id"] = "msgbatch_122",
            ["has_more"] = true,
        };
        var handler = new RecordingHandler().Respond(page);
        var batch = CreateBatch(handler, new Dictionary<string, string> { ["Provider-Header"] = "provider" });
        using var source = new CancellationTokenSource();
        handler.OnSend = () => source.Cancel();

        var result = await batch.DoListAsync(new BatchOperationCall(null, null, new Dictionary<string, string> { ["Operation-Header"] = "operation" }, source.Token, limit: 2, cursor: "msgbatch_122"), source.Token);

        Assert.Equal(2, result.Batches.Count);
        Assert.Equal("msgbatch_123", result.Batches[0].Id);
        Assert.Equal("pending", result.Batches[0].Status);
        Assert.Equal("in_progress", result.Batches[0].RawStatus);
        Assert.Equal(new[] { 3, 2, 1, 0 }, new[] { result.Batches[0].RequestCounts!.Total, result.Batches[0].RequestCounts!.Pending, result.Batches[0].RequestCounts!.Completed, result.Batches[0].RequestCounts!.Failed });
        Assert.Equal("msgbatch_122", result.Batches[1].Id);
        Assert.Equal("completed", result.Batches[1].Status);
        Assert.Equal("ended", result.Batches[1].RawStatus);
        Assert.Equal("msgbatch_122", result.NextCursor);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("test-api-key", request.Headers["x-api-key"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        Assert.Equal(BatchesUrl + "?limit=2&after_id=msgbatch_122", request.Url);
        Assert.True(request.TokenCancelledByCaller);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::omits the next cursor when there are no more batches", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_the_next_cursor_when_there_are_no_more_batches()
    {
        var handler = new RecordingHandler().Respond(new JsonObject { ["data"] = new JsonArray(), ["first_id"] = null, ["last_id"] = null, ["has_more"] = false });
        var batch = CreateBatch(handler);

        var result = await batch.DoListAsync(OperationCall(), CancellationToken.None);

        Assert.Empty(result.Batches);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects result retrieval after Anthropic archives the result file", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_result_retrieval_after_Anthropic_archives_the_result_file()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("archived_at", "2024-10-24T18:37:24.100Z"), ("results_url", null)));
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => batch.DoGetResultsAsync(OperationCall(), CancellationToken.None));

        Assert.Equal("batchId", error.Parameter);
        Assert.EndsWith("Anthropic batch \"msgbatch_123\" results are no longer available.", error.Message);
    }

    private static IBatchApi CreateBatch(HttpMessageHandler handler, IReadOnlyDictionary<string, string>? headers = null)
    {
        var options = new AnthropicOptions { ApiKey = "test-api-key" };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return AnthropicProvider.Create(options, handler).ExperimentalBatch()!;
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static JsonArray Messages(string prompt) => new(new JsonObject { ["role"] = "user", ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = prompt }) });

    private static NormalizedBatchRequest TextRequest(string id, string prompt, JsonObject? extra = null, string modelId = "claude-3-haiku-20240307")
    {
        var options = new JsonObject { ["prompt"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = prompt }) }) };
        foreach (var pair in extra ?? new JsonObject())
        {
            options[pair.Key] = pair.Value?.DeepClone();
        }

        return new NormalizedBatchRequest(id, "text", modelId, Json(options.ToJsonString()));
    }

    private static BatchOperationCall StartCall(NormalizedBatchRequest request) => StartCall([request]);

    private static BatchOperationCall StartCall(IReadOnlyList<NormalizedBatchRequest> requests, JsonObject? providerOptions = null, IReadOnlyDictionary<string, string>? headers = null, string? webhookUrl = null) =>
        new(null, providerOptions == null ? null : Json(providerOptions.ToJsonString()), headers ?? new Dictionary<string, string>(), CancellationToken.None, webhookUrl: webhookUrl, requests: requests);

    private static BatchOperationCall OperationCall(IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default) =>
        new("msgbatch_123", null, headers ?? new Dictionary<string, string>(), cancellationToken);

    private static JsonObject Counts(int processing, int succeeded, int errored, int canceled, int expired) =>
        new() { ["processing"] = processing, ["succeeded"] = succeeded, ["errored"] = errored, ["canceled"] = canceled, ["expired"] = expired };

    private static JsonObject BatchResponse(params (string Key, object? Value)[] overrides)
    {
        var batch = new JsonObject
        {
            ["id"] = "msgbatch_123",
            ["type"] = "message_batch",
            ["processing_status"] = "ended",
            ["request_counts"] = Counts(0, 1, 0, 0, 0),
            ["created_at"] = "2024-09-24T18:37:24.100Z",
            ["expires_at"] = "2024-09-25T18:37:24.100Z",
            ["archived_at"] = null,
            ["cancel_initiated_at"] = null,
            ["ended_at"] = "2024-09-24T18:38:24.100Z",
            ["results_url"] = ResultsUrl,
        };
        foreach (var (key, value) in overrides)
        {
            batch[key] = value is JsonNode node ? node : JsonValue.Create(value as string);
        }

        return batch;
    }

    private sealed record RecordedRequest(string Method, string Url, string Body, Dictionary<string, string> Headers, bool TokenCancelledByCaller);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private string _response = "{}";

        public List<RecordedRequest> Requests { get; } = [];

        public Action? OnSend { get; set; }

        public RecordingHandler Respond(JsonNode response)
        {
            _response = response.ToJsonString();
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            var headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);
            OnSend?.Invoke();
            Requests.Add(new RecordedRequest(request.Method.Method, request.RequestUri!.AbsoluteUri, body, headers, cancellationToken.IsCancellationRequested));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_response, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class CountingHandler(Action onCall) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onCall();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
        }
    }
}
