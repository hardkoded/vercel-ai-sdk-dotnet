// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch.AnthropicBatchTestSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch;

public sealed class AnthropicBatchTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(4096)]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::applies the factory maxLineBytes setting of %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Applies_the_factory_maxLineBytes_setting(int maxLineBytes)
    {
        const string resultsUrl =
            "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results";
        var handler = new RoutedHandler()
            .Add(
                "https://api.anthropic.com/v1/messages/batches/msgbatch_123",
                "{\"id\":\"msgbatch_123\",\"type\":\"message_batch\",\"processing_status\":\"ended\",\"request_counts\":{\"processing\":0,\"succeeded\":1,\"errored\":0,\"canceled\":0,\"expired\":0},\"created_at\":\"2026-01-01T00:00:00Z\",\"expires_at\":\"2026-01-02T00:00:00Z\",\"ended_at\":\"2026-01-01T01:00:00Z\",\"archived_at\":null,\"cancel_initiated_at\":null,\"results_url\":\""
                    + resultsUrl
                    + "\"}"
            )
            .Add(
                resultsUrl,
                "{\"custom_id\":\"france\",\"result\":{\"type\":\"succeeded\",\"message\":{\"id\":\"msg_123\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-test\",\"content\":[{\"type\":\"text\",\"text\":\"Paris\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":3}}}}\n"
            );
        var provider = AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = maxLineBytes },
            },
            handler
        );
        var batch = provider.ExperimentalBatch()!;
        var stream = await batch.DoGetResultsAsync(
            new BatchOperationCall(
                "msgbatch_123",
                null,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
        var results = stream.ToListAsync().AsTask();
        if (maxLineBytes == 16)
        {
            var error = await Assert.ThrowsAsync<DownloadError>(() => results);
            Assert.Equal(
                "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results",
                error.Url
            );
        }
        else
        {
            Assert.Single(await results);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects message threads before making an API request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4"
    )]
    public async Task Rejects_message_threads_before_making_an_API_request()
    {
        var calls = 0;
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-key" },
            new CountingHandler(() => calls++)
        );
        var batch = provider.ExperimentalBatch()!;
        var options = System
            .Text.Json.JsonDocument.Parse(
                "{\"prompt\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}],\"providerOptions\":{\"anthropic\":{\"thread\":{\"type\":\"create\"}}}}"
            )
            .RootElement.Clone();
        var call = new BatchOperationCall(
            null,
            null,
            new Dictionary<string, string>(),
            CancellationToken.None,
            requests: new[]
            {
                new NormalizedBatchRequest(
                    "thread-request",
                    "text",
                    "claude-3-haiku-20240307",
                    options
                ),
            }
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Contains("Message Batches do not support message threads", error.Message);
        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::exposes the applied service tier from batch result usage",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public async Task Exposes_the_applied_service_tier_from_batch_result_usage()
    {
        var message = MessageResultBody("OK");
        message["usage"] = new JsonObject
        {
            ["input_tokens"] = 1,
            ["output_tokens"] = 1,
            ["service_tier"] = "batch",
        };

        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("tier", message) + "\n"
        );

        var result = Succeeded(Assert.Single(results));
        Assert.Equal(
            "batch",
            result
                .ProviderMetadata!.Value.GetProperty("anthropic")
                .GetProperty("serviceTier")
                .GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported request types before making an API request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_unsupported_request_types_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            new NormalizedBatchRequest("image-1", "image", "claude-3-haiku-20240307", Json("{}"))
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("batch request type: image", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts a batch from prepared requests and combines batch and inferred betas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Starts_a_batch_from_prepared_requests_and_combines_batch_and_inferred_betas()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(
                ("processing_status", "in_progress"),
                ("request_counts", Counts(2, 0, 0, 0, 0)),
                ("results_url", null)
            )
        );
        var batch = CreateBatch(
            handler,
            new Dictionary<string, string>
            {
                ["Provider-Header"] = "provider",
                ["Anthropic-Beta"] = "provider-header-beta",
            }
        );
        var call = StartCall(
            [
                TextRequest(
                    "france",
                    "What is the capital of France?",
                    new JsonObject
                    {
                        ["maxOutputTokens"] = 100,
                        ["frequencyPenalty"] = 0.5,
                        ["providerOptions"] = new JsonObject
                        {
                            ["anthropic"] = new JsonObject { ["serviceTier"] = "auto" },
                        },
                    }
                ),
                TextRequest(
                    "germany",
                    "What is the capital of Germany?",
                    new JsonObject
                    {
                        ["maxOutputTokens"] = 200,
                        ["providerOptions"] = new JsonObject
                        {
                            ["anthropic"] = new JsonObject
                            {
                                ["fallbacks"] = new JsonArray(
                                    new JsonObject
                                    {
                                        ["model"] = "claude-sonnet-4-5",
                                        ["max_tokens"] = 150,
                                    }
                                ),
                            },
                        },
                    },
                    "claude-sonnet-4-5"
                ),
            ],
            new JsonObject
            {
                ["anthropic"] = new JsonObject { ["anthropicBeta"] = new JsonArray("batch-beta") },
            },
            new Dictionary<string, string>
            {
                ["Operation-Header"] = "operation",
                ["anthropic-beta"] = "operation-header-beta",
            },
            "https://example.com/batch-webhook"
        );

        var result = await batch.DoStartAsync(call, CancellationToken.None);

        Assert.Equal("msgbatch_123", result.Id);
        Assert.Equal("pending", result.Status);
        Assert.Equal("in_progress", result.RawStatus);
        Assert.Equal(
            new[] { 2, 2, 0, 0 },
            new[]
            {
                result.RequestCounts!.Total,
                result.RequestCounts.Pending,
                result.RequestCounts.Completed,
                result.RequestCounts.Failed,
            }
        );
        Assert.Equal(2, result.Warnings.Count);
        Assert.Null(result.Warnings[0].RequestId);
        Assert.Equal("unsupported", result.Warnings[0].Warning.Type);
        Assert.Equal("webhookUrl", result.Warnings[0].Warning.Feature);
        Assert.Equal(
            "The Anthropic Message Batches API does not support completion webhooks.",
            result.Warnings[0].Warning.Details
        );
        Assert.Equal("france", result.Warnings[1].RequestId);
        Assert.Equal("unsupported", result.Warnings[1].Warning.Type);
        Assert.Equal("frequencyPenalty", result.Warnings[1].Warning.Feature);

        var request = Assert.Single(handler.Requests);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(request.Body),
                new JsonObject
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
                                ["fallbacks"] = new JsonArray(
                                    new JsonObject
                                    {
                                        ["model"] = "claude-sonnet-4-5",
                                        ["max_tokens"] = 150,
                                    }
                                ),
                                ["messages"] = Messages("What is the capital of Germany?"),
                            },
                        }
                    ),
                }
            )
        );
        Assert.Equal("test-api-key", request.Headers["x-api-key"]);
        Assert.Equal("provider", request.Headers["provider-header"]);
        Assert.Equal("operation", request.Headers["operation-header"]);
        var betas = request
            .Headers["anthropic-beta"]
            .Split(',')
            .Select(beta => beta.Trim())
            .ToList();
        Assert.Contains("batch-beta", betas);
        Assert.Contains("server-side-fallback-2026-06-01", betas);
        Assert.Equal(2, betas.Count);
        Assert.Equal(betas.Count, betas.Distinct().Count());
    }

    [Theory]
    [InlineData("\"batch-beta\"")]
    [InlineData("[123]")]
    [InlineData("null")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid batch-level betas %j before making an API request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023"
    )]
    public async Task Rejects_invalid_batch_level_betas_before_making_an_API_request(
        string anthropicBeta
    )
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            [TextRequest("request-1", "Hello")],
            new JsonObject
            {
                ["anthropic"] = new JsonObject
                {
                    ["anthropicBeta"] = JsonNode.Parse(anthropicBeta),
                },
            }
        );

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects per-request betas before making an API request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_per_request_betas_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Hello",
                new JsonObject
                {
                    ["providerOptions"] = new JsonObject
                    {
                        ["anthropic"] = new JsonObject
                        {
                            ["anthropicBeta"] = new JsonArray("request-beta"),
                        },
                    },
                }
            )
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("per-request providerOptions.anthropic.anthropicBeta", error.Functionality);
        Assert.Equal(
            "Anthropic Message Batches do not support per-request betas (request \"request-1\"). Set providerOptions.anthropic.anthropicBeta on startBatch instead.",
            error.Message
        );
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts a batch with thinking binding controls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Starts_a_batch_with_thinking_binding_controls()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", "in_progress"))
        );
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "preserved-thinking",
                "Continue the conversation.",
                new JsonObject
                {
                    ["maxOutputTokens"] = 4096,
                    ["providerOptions"] = new JsonObject
                    {
                        ["anthropic"] = new JsonObject
                        {
                            ["thinking"] = new JsonObject
                            {
                                ["type"] = "adaptive",
                                ["blockBinding"] = new JsonObject
                                {
                                    ["prefixMismatchBehavior"] = "error",
                                },
                            },
                        },
                    },
                },
                "claude-fable-5-1"
            )
        );

        await batch.DoStartAsync(call, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(request.Body),
                new JsonObject
                {
                    ["requests"] = new JsonArray(
                        new JsonObject
                        {
                            ["custom_id"] = "preserved-thinking",
                            ["params"] = new JsonObject
                            {
                                ["max_tokens"] = 4096,
                                ["messages"] = Messages("Continue the conversation."),
                                ["model"] = "claude-fable-5-1",
                                ["thinking"] = new JsonObject
                                {
                                    ["block_binding"] = new JsonObject
                                    {
                                        ["prefix_mismatch_behavior"] = "error",
                                    },
                                    ["type"] = "adaptive",
                                },
                            },
                        }
                    ),
                }
            )
        );
        Assert.Equal("thinking-binding-controls-2026-08-01", request.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects structured-output modes that require start-call context",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_structured_output_modes_that_require_start_call_context()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Return JSON",
                new JsonObject
                {
                    ["responseFormat"] = new JsonObject
                    {
                        ["type"] = "json",
                        ["schema"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["answer"] = new JsonObject { ["type"] = "string" },
                            },
                        },
                    },
                }
            )
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("batch responseFormat JSON-tool fallback", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects aliased provider tool names that cannot be restored later",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_aliased_provider_tool_names_that_cannot_be_restored_later()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Search",
                new JsonObject
                {
                    ["tools"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "provider",
                            ["id"] = "anthropic.web_search_20250305",
                            ["name"] = "custom_search",
                            ["args"] = new JsonObject(),
                        }
                    ),
                }
            )
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("aliased provider tool names in batches", error.Functionality);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::allows web tools that implicitly provision code execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Allows_web_tools_that_implicitly_provision_code_execution()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", "in_progress"))
        );
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Search",
                new JsonObject
                {
                    ["tools"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "provider",
                            ["id"] = "anthropic.web_search_20260209",
                            ["name"] = "web_search",
                            ["args"] = new JsonObject(),
                        }
                    ),
                }
            )
        );

        await batch.DoStartAsync(call, CancellationToken.None);

        var tools = JsonNode.Parse(Assert.Single(handler.Requests).Body)!["requests"]![0]![
            "params"
        ]!["tools"]!.AsArray();
        var tool = Assert.Single(tools);
        Assert.Equal("web_search_20260209", tool!["type"]!.GetValue<string>());
        Assert.Equal("web_search", tool["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::does not treat a custom code_execution function as the provider tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_treat_a_custom_code_execution_function_as_the_provider_tool()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", "in_progress"))
        );
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Search",
                new JsonObject
                {
                    ["tools"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "provider",
                            ["id"] = "anthropic.web_search_20260209",
                            ["name"] = "web_search",
                            ["args"] = new JsonObject(),
                        },
                        new JsonObject
                        {
                            ["type"] = "function",
                            ["name"] = "code_execution",
                            ["description"] = "A custom client-side function",
                            ["inputSchema"] = new JsonObject
                            {
                                ["type"] = "object",
                                ["properties"] = new JsonObject(),
                                ["additionalProperties"] = false,
                            },
                        }
                    ),
                }
            )
        );

        await batch.DoStartAsync(call, CancellationToken.None);

        var tools = JsonNode.Parse(Assert.Single(handler.Requests).Body)!["requests"]![0]![
            "params"
        ]!["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        Assert.Equal("web_search_20260209", tools[0]!["type"]!.GetValue<string>());
        Assert.Equal("web_search", tools[0]!["name"]!.GetValue<string>());
        Assert.Equal("code_execution", tools[1]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::starts an on-demand compaction batch request with the inferred beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Starts_an_on_demand_compaction_batch_request_with_the_inferred_beta_header()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", "in_progress"))
        );
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "compact-conversation",
                "Summarize this conversation.",
                new JsonObject
                {
                    ["maxOutputTokens"] = 4096,
                    ["providerOptions"] = new JsonObject
                    {
                        ["anthropic"] = new JsonObject
                        {
                            ["compaction"] = new JsonObject
                            {
                                ["type"] = "summarize",
                                ["instructions"] = "Preserve decisions and open questions.",
                            },
                        },
                    },
                }
            )
        );

        await batch.DoStartAsync(call, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        var entry = JsonNode.Parse(request.Body)!["requests"]![0]!;
        Assert.Equal("compact-conversation", entry["custom_id"]!.GetValue<string>());
        Assert.Equal("summarize", entry["params"]!["compaction"]!["type"]!.GetValue<string>());
        Assert.Equal(
            "Preserve decisions and open questions.",
            entry["params"]!["compaction"]!["instructions"]!.GetValue<string>()
        );
        Assert.Contains(
            "compact-2026-09-04",
            request.Headers["anthropic-beta"].Split(',').Select(beta => beta.Trim())
        );
    }

    [Theory]
    [InlineData(
        "providerOptions.anthropic.speed",
        "{\"speed\":\"fast\"}",
        "Anthropic Message Batches do not support speed (request \"request-1\")."
    )]
    [InlineData(
        "providerOptions.anthropic.fallbacks[].speed",
        "{\"fallbacks\":[{\"model\":\"claude-sonnet-4-5\",\"speed\":\"standard\"}]}",
        "Anthropic Message Batches do not support fallback speed (request \"request-1\")."
    )]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported $feature",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_unsupported_feature(string feature, string options, string message)
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall(
            TextRequest(
                "request-1",
                "Hello",
                new JsonObject
                {
                    ["providerOptions"] = new JsonObject
                    {
                        ["anthropic"] = JsonNode.Parse(options),
                    },
                }
            )
        );

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal(feature, error.Functionality);
        Assert.Equal(message, error.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid request IDs before making an API request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_invalid_request_IDs_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);

        var invalid = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoStartAsync(
                StartCall(TextRequest("invalid id", "Hello")),
                CancellationToken.None
            )
        );
        var tooLong = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoStartAsync(
                StartCall(TextRequest(new string('a', 65), "Hello")),
                CancellationToken.None
            )
        );

        Assert.Equal("requests", invalid.Parameter);
        Assert.EndsWith(
            "Anthropic batch request ID \"invalid id\" must match ^[A-Za-z0-9_-]{1,64}$.",
            invalid.Message
        );
        Assert.Equal("requests", tooLong.Parameter);
        Assert.Contains("must match ^[A-Za-z0-9_-]{1,64}$.", tooLong.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects duplicate request IDs before making an API request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_duplicate_request_IDs_before_making_an_API_request()
    {
        var handler = new RecordingHandler();
        var batch = CreateBatch(handler);
        var call = StartCall([
            TextRequest("duplicate", "First"),
            TextRequest("duplicate", "Second"),
        ]);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoStartAsync(call, CancellationToken.None)
        );

        Assert.Equal("requests", error.Parameter);
        Assert.EndsWith(
            "Anthropic batch request IDs must be unique; duplicate ID \"duplicate\".",
            error.Message
        );
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("in_progress", "pending")]
    [InlineData("canceling", "pending")]
    [InlineData("ended", "completed")]
    [InlineData("future_status", "pending")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::maps status %s to %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_status(string rawStatus, string status)
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", rawStatus))
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal(status, result.Status);
        Assert.Equal(rawStatus, result.RawStatus);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::normalizes request counts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Normalizes_request_counts()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("request_counts", Counts(0, 2, 1, 1, 1)))
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoGetStatusAsync(OperationCall(), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        Assert.Equal("ended", result.RawStatus);
        Assert.Equal(
            new[] { 5, 0, 2, 3 },
            new[]
            {
                result.RequestCounts!.Total,
                result.RequestCounts.Pending,
                result.RequestCounts.Completed,
                result.RequestCounts.Failed,
            }
        );
        Assert.Equal("2024-09-24T18:37:24.100Z", result.CreatedAt);
        Assert.Equal("2024-09-25T18:37:24.100Z", result.ExpiresAt);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText()),
                new JsonObject
                {
                    ["anthropic"] = new JsonObject
                    {
                        ["archivedAt"] = null,
                        ["cancelInitiatedAt"] = null,
                        ["endedAt"] = "2024-09-24T18:38:24.100Z",
                        ["requestCounts"] = Counts(0, 2, 1, 1, 1),
                        ["resultsUrl"] = ResultsUrl,
                    },
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::cancels a batch",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks that the caller token reaches the HTTP call, not the identity of the JS abort signal."
    )]
    public async Task Cancels_a_batch()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("processing_status", "canceling"))
        );
        var batch = CreateBatch(
            handler,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );
        using var source = new CancellationTokenSource();
        handler.OnSend = () => source.Cancel();

        var result = await batch.DoCancelAsync(
            OperationCall(
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                source.Token
            ),
            source.Token
        );

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
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::lists and normalizes a page of batches",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks that the caller token reaches the HTTP call, not the identity of the JS abort signal."
    )]
    public async Task Lists_and_normalizes_a_page_of_batches()
    {
        var page = new JsonObject
        {
            ["data"] = new JsonArray(
                BatchResponse(
                    ("id", "msgbatch_123"),
                    ("processing_status", "in_progress"),
                    ("request_counts", Counts(2, 1, 0, 0, 0))
                ),
                BatchResponse(("id", "msgbatch_122"))
            ),
            ["first_id"] = "msgbatch_123",
            ["last_id"] = "msgbatch_122",
            ["has_more"] = true,
        };
        var handler = new RecordingHandler().Respond(page);
        var batch = CreateBatch(
            handler,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );
        using var source = new CancellationTokenSource();
        handler.OnSend = () => source.Cancel();

        var result = await batch.DoListAsync(
            new BatchOperationCall(
                null,
                null,
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                source.Token,
                limit: 2,
                cursor: "msgbatch_122"
            ),
            source.Token
        );

        Assert.Equal(2, result.Batches.Count);
        Assert.Equal("msgbatch_123", result.Batches[0].Id);
        Assert.Equal("pending", result.Batches[0].Status);
        Assert.Equal("in_progress", result.Batches[0].RawStatus);
        Assert.Equal(
            new[] { 3, 2, 1, 0 },
            new[]
            {
                result.Batches[0].RequestCounts!.Total,
                result.Batches[0].RequestCounts!.Pending,
                result.Batches[0].RequestCounts!.Completed,
                result.Batches[0].RequestCounts!.Failed,
            }
        );
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
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::omits the next cursor when there are no more batches",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_the_next_cursor_when_there_are_no_more_batches()
    {
        var handler = new RecordingHandler().Respond(
            new JsonObject
            {
                ["data"] = new JsonArray(),
                ["first_id"] = null,
                ["last_id"] = null,
                ["has_more"] = false,
            }
        );
        var batch = CreateBatch(handler);

        var result = await batch.DoListAsync(OperationCall(), CancellationToken.None);

        Assert.Empty(result.Batches);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects result retrieval after Anthropic archives the result file",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_result_retrieval_after_Anthropic_archives_the_result_file()
    {
        var handler = new RecordingHandler().Respond(
            BatchResponse(("archived_at", "2024-10-24T18:37:24.100Z"), ("results_url", null))
        );
        var batch = CreateBatch(handler);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            batch.DoGetResultsAsync(OperationCall(), CancellationToken.None)
        );

        Assert.Equal("batchId", error.Parameter);
        Assert.EndsWith(
            "Anthropic batch \"msgbatch_123\" results are no longer available.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::incrementally maps all Anthropic JSONL result variants",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Incrementally_maps_all_Anthropic_JSONL_result_variants()
    {
        var succeeded = SucceededLine("france", MessageResultBody("Paris"));
        var errored =
            """{"custom_id":"invalid","result":{"type":"errored","error":{"type":"error","error":{"type":"invalid_request_error","message":"Invalid request."},"request_id":"req_123"}}}""";
        var canceled = """{"custom_id":"canceled","result":{"type":"canceled"}}""";
        var expired = """{"custom_id":"expired","result":{"type":"expired"}}""";
        var handler = new RecordingHandler();
        handler
            .Respond(BatchResponse())
            .RespondChunks(
                ResultsUrl,
                succeeded[..17],
                succeeded[17..] + "\r",
                "\n\n" + errored[..23],
                errored[23..] + "\n" + canceled + "\r\n" + expired[..11],
                expired[11..]
            );

        var results = await (
            await CreateBatch(handler).DoGetResultsAsync(OperationCall(), CancellationToken.None)
        )
            .ToListAsync()
            .AsTask();

        Assert.Equal(4, results.Count);
        Assert.Equal("text", results[0].Type);
        Assert.Equal("france", results[0].Id);
        var result = Succeeded(results[0]);
        var text = Assert.Single(result.Content);
        Assert.Equal("text", text.Type);
        Assert.Equal("Paris", text.Text);
        Assert.Equal("stop", result.FinishReason);
        Assert.Equal("end_turn", result.RawFinishReason);
        Assert.Equal(13, result.Usage!.InputTotal);
        Assert.Equal(10, result.Usage.NoCache);
        Assert.Equal(2, result.Usage.CacheRead);
        Assert.Equal(1, result.Usage.CacheWrite);
        Assert.Equal(3, result.Usage.OutputTotal);
        Assert.Null(result.Usage.Text);
        Assert.Null(result.Usage.Reasoning);
        AssertJson(
            """{"input_tokens":10,"output_tokens":3,"cache_creation_input_tokens":1,"cache_read_input_tokens":2}""",
            result.Usage.Raw
        );
        Assert.Equal("msg_123", result.ResponseId);
        Assert.Equal("claude-3-haiku-20240307", result.ModelId);
        AssertJson(
            """{"anthropic":{"usage":{"input_tokens":10,"output_tokens":3,"cache_creation_input_tokens":1,"cache_read_input_tokens":2},"stopSequence":null,"iterations":null,"container":null,"contextManagement":null}}""",
            result.ProviderMetadata
        );

        Assert.Equal("invalid", results[1].Id);
        Assert.Equal("failed", results[1].Status);
        Assert.Equal("Invalid request.", results[1].ErrorMessage);
        Assert.Equal("invalid_request_error", results[1].ErrorType);
        AssertJson("""{"anthropic":{"requestId":"req_123"}}""", results[1].ProviderMetadata);
        Assert.Equal(("canceled", "cancelled"), (results[2].Id, results[2].Status));
        Assert.Equal(("expired", "expired"), (results[3].Id, results[3].Status));
        Assert.Equal(
            [BatchesUrl + "/msgbatch_123", ResultsUrl],
            handler.Requests.Select(request => request.Url)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::exposes safeguard results on batch result metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Exposes_safeguard_results_on_batch_result_metadata()
    {
        var safeguardResults = JsonNode.Parse(
            """[{"type":"dangerous_tool_use","status":{"type":"available","tool_uses":{"toolu_123":{"type":"evaluated","outcome":"not_flagged"}}}}]"""
        )!;
        var message = MessageWith(
            string.Empty,
            JsonNode
                .Parse(
                    """[{"type":"tool_use","id":"toolu_123","name":"bash","input":{"command":"echo hello"}}]"""
                )!
                .AsArray(),
            ("stop_reason", "tool_use"),
            ("safeguard_results", safeguardResults.DeepClone())
        );

        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("safeguarded", message)
        );

        var result = Succeeded(Assert.Single(results));
        Assert.True(
            JsonNode.DeepEquals(
                safeguardResults,
                JsonNode.Parse(
                    result
                        .ProviderMetadata!.Value.GetProperty("anthropic")
                        .GetProperty("safeguardResults")
                        .GetRawText()
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::preserves signed compaction blocks in batch results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_signed_compaction_blocks_in_batch_results()
    {
        var message = MessageWith(
            string.Empty,
            JsonNode
                .Parse(
                    """[{"type":"compaction","content":"Summary of the conversation.","signature":"compaction-signature"}]"""
                )!
                .AsArray(),
            ("stop_reason", "compaction")
        );

        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("compaction", message)
        );

        var item = Assert.Single(results);
        Assert.Equal("compaction", item.Id);
        var result = Succeeded(item);
        var part = Assert.Single(result.Content);
        Assert.Equal("text", part.Type);
        Assert.Equal("Summary of the conversation.", part.Text);
        AssertJson(
            """{"anthropic":{"type":"compaction","signature":"compaction-signature"}}""",
            part.ProviderMetadata
        );
        Assert.Equal("other", result.FinishReason);
        Assert.Equal("compaction", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::preserves client and provider-executed tool content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_client_and_provider_executed_tool_content()
    {
        var message = MessageWith(
            string.Empty,
            JsonNode
                .Parse(
                    """
                    [
                      {"type":"tool_use","id":"toolu_123","name":"get_weather","input":{"city":"Paris"}},
                      {"type":"server_tool_use","id":"srvtoolu_123","name":"web_search","input":{"query":"weather Paris"}},
                      {"type":"server_tool_use","id":"code_123","name":"code_execution","input":{"code":"print(\"Paris\")"}},
                      {"type":"web_search_tool_result","tool_use_id":"srvtoolu_123","content":[{"type":"web_search_result","url":"https://example.com/weather","title":"Paris weather","encrypted_content":"encrypted"}]},
                      {"type":"mcp_tool_use","id":"mcp_123","name":"lookup","server_name":"weather","input":{"city":"Paris"}},
                      {"type":"mcp_tool_result","tool_use_id":"mcp_123","is_error":false,"content":[{"type":"text","text":"sunny","citations":[{"type":"web_search_result_location","cited_text":"sunny","url":"https://example.com/weather","title":"Paris weather","encrypted_index":"encrypted-index"}]}]}
                    ]
                    """
                )!
                .AsArray(),
            ("stop_reason", "tool_use")
        );
        var handler = new RecordingHandler();
        handler
            .Respond(BatchResponse())
            .RespondChunks(ResultsUrl, SucceededLine("tool-call", message));

        var results = await (
            await CreateBatch(handler, generateId: () => "source-1")
                .DoGetResultsAsync(OperationCall(), CancellationToken.None)
        )
            .ToListAsync()
            .AsTask();

        var item = Assert.Single(results);
        Assert.Equal("tool-call", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(7, content.Count);

        Assert.Equal(
            ("tool-call", "toolu_123", "get_weather", """{"city":"Paris"}"""),
            (content[0].Type, content[0].ToolCallId, content[0].ToolName, content[0].Text)
        );
        Assert.Null(content[0].ProviderExecuted);

        Assert.Equal(
            ("tool-call", "srvtoolu_123", "web_search", """{"query":"weather Paris"}"""),
            (content[1].Type, content[1].ToolCallId, content[1].ToolName, content[1].Text)
        );
        Assert.True(content[1].ProviderExecuted);

        Assert.Equal(
            (
                "tool-call",
                "code_123",
                "code_execution",
                """{"type":"programmatic-tool-call","code":"print(\"Paris\")"}"""
            ),
            (content[2].Type, content[2].ToolCallId, content[2].ToolName, content[2].Text)
        );
        Assert.True(content[2].ProviderExecuted);
        Assert.True(content[2].Dynamic);

        Assert.Equal(
            ("tool-result", "srvtoolu_123", "web_search"),
            (content[3].Type, content[3].ToolCallId, content[3].ToolName)
        );
        AssertJson(
            """[{"encryptedContent":"encrypted","pageAge":null,"title":"Paris weather","type":"web_search_result","url":"https://example.com/weather"}]""",
            content[3].Output
        );

        Assert.Equal(
            ("source", "url", "source-1", "https://example.com/weather", "Paris weather"),
            (
                content[4].Type,
                content[4].SourceType,
                content[4].SourceId,
                content[4].Url,
                content[4].Title
            )
        );
        AssertJson("""{"anthropic":{"pageAge":null}}""", content[4].ProviderMetadata);

        Assert.Equal(
            ("tool-call", "mcp_123", "lookup", """{"city":"Paris"}"""),
            (content[5].Type, content[5].ToolCallId, content[5].ToolName, content[5].Text)
        );
        Assert.True(content[5].ProviderExecuted);
        Assert.True(content[5].Dynamic);
        AssertJson(
            """{"anthropic":{"serverName":"weather","type":"mcp-tool-use"}}""",
            content[5].ProviderMetadata
        );

        Assert.Equal(
            ("tool-result", "mcp_123", "lookup"),
            (content[6].Type, content[6].ToolCallId, content[6].ToolName)
        );
        Assert.False(content[6].IsError);
        Assert.True(content[6].Dynamic);
        AssertJson(
            """[{"type":"text","text":"sunny","citations":[{"type":"web_search_result_location","cited_text":"sunny","url":"https://example.com/weather","title":"Paris weather","encrypted_index":"encrypted-index"}]}]""",
            content[6].Output
        );
        AssertJson(
            """{"anthropic":{"serverName":"weather","type":"mcp-tool-use"}}""",
            content[6].ProviderMetadata
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::preserves raw batch citations without misattributing document indices",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_raw_batch_citations_without_misattributing_document_indices()
    {
        const string RawCitations = """
            [
              {"type":"page_location","cited_text":"Paris is sunny.","document_index":0,"document_title":"Weather report","start_page_number":1,"end_page_number":1,"file_id":"file_page"},
              {"type":"char_location","cited_text":"Paris is sunny.","document_index":0,"document_title":"Weather report","start_char_index":0,"end_char_index":15,"file_id":"file_char"},
              {"type":"web_search_result_location","cited_text":"Paris is sunny.","url":"https://example.com/weather","title":"Paris weather","encrypted_index":"encrypted-index"}
            ]
            """;
        var message = MessageWith(
            "Paris is sunny.",
            new JsonArray(
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = "Paris is sunny.",
                    ["citations"] = JsonNode.Parse(RawCitations),
                }
            )
        );
        var handler = new RecordingHandler();
        handler
            .Respond(BatchResponse())
            .RespondChunks(ResultsUrl, SucceededLine("citation", message));

        var results = await (
            await CreateBatch(handler, generateId: () => "citation-source")
                .DoGetResultsAsync(OperationCall(), CancellationToken.None)
        )
            .ToListAsync()
            .AsTask();

        var item = Assert.Single(results);
        Assert.Equal("citation", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(2, content.Count);
        var text = content[0];
        Assert.Equal("Paris is sunny.", text.Text);
        Assert.Equal(3, text.Citations!.Count);
        var page = Assert.IsType<GeneratedDocumentSource>(text.Citations[0].Source);
        Assert.Equal(("file_page", "Weather report"), (page.Id, page.Title));
        var chars = Assert.IsType<GeneratedDocumentSource>(text.Citations[1].Source);
        Assert.Equal(("file_char", "Weather report"), (chars.Id, chars.Title));
        var web = Assert.IsType<GeneratedSource>(text.Citations[2].Source);
        Assert.Equal("https://example.com/weather", web.Url);
        Assert.All(text.Citations, citation => Assert.Equal("Paris is sunny.", citation.CitedText));
        AssertJson("{\"anthropic\":{\"citations\":" + RawCitations + "}}", text.ProviderMetadata);

        var source = content[1];
        Assert.Equal(
            ("source", "url", "citation-source", "https://example.com/weather", "Paris weather"),
            (source.Type, source.SourceType, source.SourceId, source.Url, source.Title)
        );
        AssertJson(
            """{"anthropic":{"citedText":"Paris is sunny.","encryptedIndex":"encrypted-index"}}""",
            source.ProviderMetadata
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::accepts nullable web result fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Accepts_nullable_web_result_fields()
    {
        var message = MessageWith(
            string.Empty,
            JsonNode
                .Parse(
                    """
                    [
                      {"type":"web_fetch_tool_result","tool_use_id":"web-fetch-1","content":{"type":"web_fetch_result","url":"https://example.com/document","retrieved_at":null,"content":{"type":"document","title":null,"source":{"type":"text","media_type":"text/plain","data":"document text"}}}},
                      {"type":"web_search_tool_result","tool_use_id":"web-search-1","content":[{"type":"web_search_result","url":"https://example.com/search-result","title":null,"encrypted_content":"encrypted"}]},
                      {"type":"text","text":"Document excerpt","citations":[{"type":"char_location","cited_text":"document text","document_index":0,"document_title":null,"start_char_index":0,"end_char_index":13}]}
                    ]
                    """
                )!
                .AsArray()
        );
        var handler = new RecordingHandler();
        handler
            .Respond(BatchResponse())
            .RespondChunks(ResultsUrl, SucceededLine("nullable-web-fields", message));

        var results = await (
            await CreateBatch(handler, generateId: () => "nullable-source")
                .DoGetResultsAsync(OperationCall(), CancellationToken.None)
        )
            .ToListAsync()
            .AsTask();

        var item = Assert.Single(results);
        Assert.Equal("nullable-web-fields", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(4, content.Count);
        Assert.Equal(
            ("tool-result", "web-fetch-1", "web_fetch"),
            (content[0].Type, content[0].ToolCallId, content[0].ToolName)
        );
        AssertJson(
            """{"type":"web_fetch_result","url":"https://example.com/document","retrievedAt":null,"content":{"type":"document","title":null,"source":{"type":"text","mediaType":"text/plain","data":"document text"}}}""",
            content[0].Output
        );
        Assert.Equal(
            ("tool-result", "web-search-1", "web_search"),
            (content[1].Type, content[1].ToolCallId, content[1].ToolName)
        );
        AssertJson(
            """[{"type":"web_search_result","url":"https://example.com/search-result","pageAge":null,"encryptedContent":"encrypted"}]""",
            content[1].Output
        );
        Assert.Equal(
            ("source", "url", "nullable-source", "https://example.com/search-result"),
            (content[2].Type, content[2].SourceType, content[2].SourceId, content[2].Url)
        );
        Assert.Equal(("text", "Document excerpt"), (content[3].Type, content[3].Text));
        AssertJson(
            """{"anthropic":{"citations":[{"type":"char_location","cited_text":"document text","document_index":0,"document_title":null,"start_char_index":0,"end_char_index":13}]}}""",
            content[3].ProviderMetadata
        );
        Assert.False(content[1].Output!.Value[0].TryGetProperty("title", out _));
        Assert.Null(content[2].Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::normalizes advisor tool results to the provider output shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Normalizes_advisor_tool_results_to_the_provider_output_shape()
    {
        var message = MessageWith(
            string.Empty,
            JsonNode
                .Parse(
                    """
                    [
                      {"type":"advisor_tool_result","tool_use_id":"advisor-plain","content":{"type":"advisor_result","text":"Use a queue.","stop_reason":"end_turn"}},
                      {"type":"advisor_tool_result","tool_use_id":"advisor-redacted","content":{"type":"advisor_redacted_result","encrypted_content":"opaque-advice","stop_reason":"max_tokens"}},
                      {"type":"advisor_tool_result","tool_use_id":"advisor-error","content":{"type":"advisor_tool_result_error","error_code":"max_uses_exceeded"}}
                    ]
                    """
                )!
                .AsArray()
        );

        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("advisor-results", message)
        );

        var item = Assert.Single(results);
        Assert.Equal("advisor-results", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(3, content.Count);
        Assert.Equal(
            ("tool-result", "advisor-plain", "advisor"),
            (content[0].Type, content[0].ToolCallId, content[0].ToolName)
        );
        AssertJson(
            """{"type":"advisor_result","text":"Use a queue.","stopReason":"end_turn"}""",
            content[0].Output
        );
        Assert.Equal(
            ("tool-result", "advisor-redacted", "advisor"),
            (content[1].Type, content[1].ToolCallId, content[1].ToolName)
        );
        AssertJson(
            """{"type":"advisor_redacted_result","encryptedContent":"opaque-advice","stopReason":"max_tokens"}""",
            content[1].Output
        );
        Assert.Equal(
            ("tool-result", "advisor-error", "advisor"),
            (content[2].Type, content[2].ToolCallId, content[2].ToolName)
        );
        Assert.True(content[2].IsError);
        AssertJson(
            """{"type":"advisor_tool_result_error","errorCode":"max_uses_exceeded"}""",
            content[2].Output
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::fails an invalid succeeded item without aborting later results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Fails_an_invalid_succeeded_item_without_aborting_later_results()
    {
        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("invalid", new JsonObject { ["type"] = "message" }) + "\n",
            SucceededLine("valid", MessageResultBody("Paris"))
        );

        Assert.Equal(2, results.Count);
        AssertInvalid(results[0], "invalid");
        Assert.Equal("valid", results[1].Id);
        var text = Assert.Single(Succeeded(results[1]).Content);
        Assert.Equal(("text", "Paris"), (text.Type, text.Text));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::fails an unknown result type without aborting later results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Fails_an_unknown_result_type_without_aborting_later_results()
    {
        var results = await GetResultsAsync(
            new RecordingHandler(),
            """{"custom_id":"unknown","result":{"type":"future_result","data":"opaque"}}""" + "\n",
            SucceededLine("valid", MessageResultBody("Paris"))
        );

        Assert.Equal(2, results.Count);
        AssertInvalid(results[0], "unknown");
        Assert.Equal("valid", results[1].Id);
        var text = Assert.Single(Succeeded(results[1]).Content);
        Assert.Equal(("text", "Paris"), (text.Type, text.Text));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::skips unknown content blocks in a succeeded result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Skips_unknown_content_blocks_in_a_succeeded_result()
    {
        var future = MessageWith(
            "Paris",
            JsonNode
                .Parse(
                    """[{"type":"future_content","data":"opaque"},{"type":"text","text":"Paris"}]"""
                )!
                .AsArray()
        );
        var malformed = MessageWith(
            "Paris",
            JsonNode.Parse("""[{"type":"text"},{"type":"text","text":"Paris"}]""")!.AsArray()
        );

        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("future-content", future) + "\n",
            SucceededLine("malformed-known-content", malformed)
        );

        Assert.Equal(2, results.Count);
        Assert.Equal("future-content", results[0].Id);
        var result = Succeeded(results[0]);
        var text = Assert.Single(result.Content);
        Assert.Equal(("text", "Paris"), (text.Type, text.Text));
        Assert.Equal(13, result.Usage!.InputTotal);
        Assert.Equal(3, result.Usage.OutputTotal);
        AssertInvalid(results[1], "malformed-known-content");
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::forwards operation headers and the abort signal while retrieving results",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks that the caller token reaches both HTTP calls, not the identity of the JS abort signal."
    )]
    public async Task Forwards_operation_headers_and_the_abort_signal_while_retrieving_results()
    {
        var handler = new RecordingHandler()
            .Respond(BatchResponse())
            .Respond(ResultsUrl, "{\"custom_id\":\"canceled\",\"result\":{\"type\":\"canceled\"}}");
        var batch = CreateBatch(
            handler,
            new Dictionary<string, string> { ["Provider-Header"] = "provider" }
        );
        using var source = new CancellationTokenSource();

        var stream = await batch.DoGetResultsAsync(
            OperationCall(
                new Dictionary<string, string> { ["Operation-Header"] = "operation" },
                source.Token
            ),
            source.Token
        );
        await stream.ToListAsync().AsTask();

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("provider", request.Headers["provider-header"]);
            Assert.Equal("operation", request.Headers["operation-header"]);
        });
        Assert.All(handler.Requests, request => Assert.True(request.TokenCanBeCanceled));
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::exposes batch support on the provider",
        Coverage = UpstreamCoverage.Partial,
        Note = "Language models have no batch members by construction in C#, so only the batch API surface is checked."
    )]
    public void Exposes_batch_support_on_the_provider()
    {
        var batch = AnthropicProvider
            .Create(new AnthropicOptions { ApiKey = "test-api-key" })
            .ExperimentalBatch();

        Assert.NotNull(batch);
        Assert.True(batch!.CanCancel);
        Assert.True(batch.CanList);
        Assert.Equal("anthropic.batch", batch.Provider);
    }
}
