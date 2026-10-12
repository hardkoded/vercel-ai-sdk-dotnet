// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoStreamTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream::";
    private const string NovaModelId = "us.amazon.nova-2-lite-v1:0";
    private const string StopOnly = """{"messageStop":{"stopReason":"end_turn"}}""";
    private const string ToolSchema = """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";

    [Theory]
    [InlineData("amazonBedrock")]
    [InlineData("bedrock")]
    [UpstreamTest(Suite + "should ignore portable max without warnings in streams with explicit %s reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_ignore_portable_max_without_warnings_in_streams_with_explicit_reasoning_effort(string providerKey)
    {
        var handler = Chunks(StopOnly);
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = "max",
            MaxOutputTokens = 1024,
            ProviderOptions = Options("{\"" + providerKey + "\":{\"reasoningConfig\":{\"type\":\"enabled\",\"maxReasoningEffort\":\"low\"}}}"),
        }, NovaModelId);

        var body = handler.BodyJson;
        Assert.True(JsonEquals("""{"type":"enabled","maxReasoningEffort":"low"}""", body.GetProperty("additionalModelRequestFields").GetProperty("reasoningConfig")));
        Assert.Equal(1024, body.GetProperty("inferenceConfig").GetProperty("maxTokens").GetInt32());
        Assert.Equal("stream-start warnings=0", Render(parts[0]));
    }

    [Theory]
    [InlineData("high")]
    [InlineData("max")]
    [UpstreamTest(
        Suite + "should derive portable %s effort with type-only Nova 2 reasoningConfig in streams",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_derive_portable_effort_with_type_only_Nova_2_reasoningConfig_in_streams(string reasoning)
    {
        var handler = Chunks(StopOnly);
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = reasoning,
            ProviderOptions = Options("""{"amazonBedrock":{"reasoningConfig":{"type":"enabled"}}}"""),
        }, NovaModelId);

        var body = handler.BodyJson;
        Assert.True(JsonEquals("""{"type":"enabled","maxReasoningEffort":"high"}""", body.GetProperty("additionalModelRequestFields").GetProperty("reasoningConfig")));
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        if (reasoning == "max")
        {
            var warning = Assert.Single(start.Warnings);
            Assert.Equal("compatibility", warning.Type);
            Assert.Equal("reasoning \"max\" is not directly supported by this model. mapped to effort \"high\".", warning.Message);
        }
        else
        {
            Assert.Empty(start.Warnings);
        }
    }

    [Theory]
    [InlineData("high", "amazonBedrock")]
    [InlineData("high", "bedrock")]
    [InlineData("max", "amazonBedrock")]
    [InlineData("max", "bedrock")]
    [UpstreamTest(Suite + "should preserve a budget-only configuration with portable %s reasoning in %s streams", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_a_budget_only_configuration_with_portable_reasoning_in_streams(string reasoning, string providerKey)
    {
        var handler = Chunks(StopOnly);
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = reasoning,
            MaxOutputTokens = 1024,
            ProviderOptions = Options("{\"" + providerKey + "\":{\"reasoningConfig\":{\"budgetTokens\":5000}}}"),
        });

        var body = handler.BodyJson;
        Assert.True(JsonEquals("""{"type":"enabled","budget_tokens":5000}""", body.GetProperty("additionalModelRequestFields").GetProperty("thinking")));
        Assert.Equal(6024, body.GetProperty("inferenceConfig").GetProperty("maxTokens").GetInt32());
        Assert.Equal("stream-start warnings=0", Render(parts[0]));
    }

    [Fact]
    [UpstreamTest(Suite + "should accept citation deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_citation_deltas()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"citation":{"location":{"documentPage":{"documentIndex":0,"start":1,"end":2}},"sourceContent":[{"text":"Source content"}],"title":"document"}}}}""");
        var parts = await Stream(handler);

        Assert.Empty(parts.OfType<ErrorStreamPart>());
    }

    [Fact]
    [UpstreamTest(Suite + "should stream text deltas with metadata and usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas_with_metadata_and_usage()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"text":", "}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":2,"delta":{"text":"World!"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34,"totalTokens":38},"metrics":{"latencyMs":10}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler);

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Hello\"",
                "text-start 1",
                "text-delta 1 \", \"",
                "text-start 2",
                "text-delta 2 \"World!\"",
                "finish stop raw=stop_sequence usage=in(4,4,0,0) out(34,34,undefined) raw={\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}",
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_deltas()
    {
        var handler = Chunks(
            """{"contentBlockStart":{"contentBlockIndex":0,"start":{"toolUse":{"toolUseId":"tool-use-id","name":"test-tool"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"toolUse":{"input":"{\"value\":"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"toolUse":{"input":"\"Sparkle Day\"}"}}}}""",
            """{"contentBlockStop":{"contentBlockIndex":0}}""",
            """{"messageStop":{"stopReason":"tool_use"}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Tools = new[] { new LanguageModelTool("test-tool", null, Schema(ToolSchema)) },
            ToolChoice = ToolChoice.Tool("test-tool"),
        });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "tool-input-start tool-use-id test-tool",
                "tool-input-delta tool-use-id \"{\\\"value\\\":\"",
                "tool-input-delta tool-use-id \"\\\"Sparkle Day\\\"}\"",
                "tool-input-end tool-use-id",
                "tool-call tool-use-id test-tool \"{\\\"value\\\":\\\"Sparkle Day\\\"}\"",
                "finish tool-calls raw=tool_use usage=" + NoUsage,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream parallel tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_parallel_tool_calls()
    {
        var handler = Chunks(
            """{"contentBlockStart":{"contentBlockIndex":0,"start":{"toolUse":{"toolUseId":"tool-use-id-1","name":"test-tool-1"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"toolUse":{"input":"{\"value1\":"}}}}""",
            """{"contentBlockStart":{"contentBlockIndex":1,"start":{"toolUse":{"toolUseId":"tool-use-id-2","name":"test-tool-2"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"toolUse":{"input":"{\"value2\":"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"toolUse":{"input":"\"Sparkle Day\"}"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"toolUse":{"input":"\"Sparkle Day\"}"}}}}""",
            """{"contentBlockStop":{"contentBlockIndex":0}}""",
            """{"contentBlockStop":{"contentBlockIndex":1}}""",
            """{"messageStop":{"stopReason":"tool_use"}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Tools = new[]
            {
                new LanguageModelTool("test-tool-1", null, Schema("""{"type":"object","properties":{"value1":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""")),
                new LanguageModelTool("test-tool-2", null, Schema("""{"type":"object","properties":{"value2":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""")),
            },
            ToolChoice = ToolChoice.Tool("test-tool"),
        });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "tool-input-start tool-use-id-1 test-tool-1",
                "tool-input-delta tool-use-id-1 \"{\\\"value1\\\":\"",
                "tool-input-start tool-use-id-2 test-tool-2",
                "tool-input-delta tool-use-id-2 \"{\\\"value2\\\":\"",
                "tool-input-delta tool-use-id-2 \"\\\"Sparkle Day\\\"}\"",
                "tool-input-delta tool-use-id-1 \"\\\"Sparkle Day\\\"}\"",
                "tool-input-end tool-use-id-1",
                "tool-call tool-use-id-1 test-tool-1 \"{\\\"value1\\\":\\\"Sparkle Day\\\"}\"",
                "tool-input-end tool-use-id-2",
                "tool-call tool-use-id-2 test-tool-2 \"{\\\"value2\\\":\\\"Sparkle Day\\\"}\"",
                "finish tool-calls raw=tool_use usage=" + NoUsage,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should handle error stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_error_stream_parts()
    {
        await AssertStreamError("internalServerException", "InternalServerException", "Internal Server Error", 500, true);
    }

    [Fact]
    [UpstreamTest(Suite + "should handle modelStreamErrorException error", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_modelStreamErrorException_error()
    {
        await AssertStreamError("modelStreamErrorException", "ModelStreamErrorException", "Model Stream Error", 424, true);
    }

    [Fact]
    [UpstreamTest(Suite + "should handle throttlingException error", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_throttlingException_error()
    {
        await AssertStreamError("throttlingException", "ThrottlingException", "Throttling Error", 429, true);
    }

    [Fact]
    [UpstreamTest(Suite + "should handle validationException error", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_validationException_error()
    {
        await AssertStreamError("validationException", "ValidationException", "Validation Error", 400, false);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should handle failed chunk parsing",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream mocks the response handler to emit { message: 'Chunk Parsing Failed' }; here a frame with an invalid JSON payload fails parsing, so the error message differs.")]
    public async Task Should_handle_failed_chunk_parsing()
    {
        var handler = new UpstreamEventStreamHandler(new[] { EventStreamFrames.Event("contentBlockDelta", "not json") });
        var parts = await Stream(handler);

        Assert.Equal(4, parts.Count);
        Assert.Equal(Start, Render(parts[0]));
        Assert.Equal(Meta, Render(parts[1]));
        var error = Assert.IsType<ErrorStreamPart>(parts[2]);
        Assert.Contains("JSON parsing failed", error.Message);
        Assert.Equal("finish error raw=undefined usage=" + NoUsage, Render(parts[3]));
    }

    [Fact]
    [UpstreamTest(Suite + "should pass the messages and the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_messages_and_the_model()
    {
        var handler = Chunks();
        await Stream(handler);

        Assert.True(JsonEquals(
            """{"additionalModelResponseFieldPaths":["/delta/stop_sequence"],"messages":[{"content":[{"text":"Hello"}],"role":"user"}],"system":[{"text":"System Prompt"}]}""",
            handler.BodyJson));
    }

    [Fact]
    [UpstreamTest(Suite + "should support guardrails", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_guardrails()
    {
        var handler = Chunks();
        await Stream(handler, Call("""{"bedrock":{"guardrailConfig":{"guardrailIdentifier":"-1","guardrailVersion":"1","trace":"enabled","streamProcessingMode":"async"}}}"""));

        Assert.True(JsonMatches(
            """{"messages":[{"role":"user","content":[{"text":"Hello"}]}],"system":[{"text":"System Prompt"}],"guardrailConfig":{"guardrailIdentifier":"-1","guardrailVersion":"1","trace":"enabled","streamProcessingMode":"async"}}""",
            handler.BodyJson));
    }

    [Fact]
    [UpstreamTest(Suite + "should accept the new `amazonBedrock` providerOptions key", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_the_new_amazonBedrock_providerOptions_key()
    {
        var handler = Chunks();
        await Stream(handler, Call("""{"amazonBedrock":{"guardrailConfig":{"guardrailIdentifier":"-1","guardrailVersion":"1","trace":"enabled","streamProcessingMode":"async"}}}"""));

        Assert.True(JsonMatches(
            """{"guardrailConfig":{"guardrailIdentifier":"-1","guardrailVersion":"1","trace":"enabled","streamProcessingMode":"async"}}""",
            handler.BodyJson));
    }

    [Fact]
    [UpstreamTest(Suite + "should prefer `amazonBedrock` providerOptions over the legacy `bedrock` key", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_amazonBedrock_providerOptions_over_the_legacy_bedrock_key()
    {
        var handler = Chunks();
        await Stream(handler, Call("""{"bedrock":{"guardrailConfig":{"guardrailIdentifier":"legacy","guardrailVersion":"1"}},"amazonBedrock":{"guardrailConfig":{"guardrailIdentifier":"preferred","guardrailVersion":"2"}}}"""));

        Assert.True(JsonMatches(
            """{"guardrailConfig":{"guardrailIdentifier":"preferred","guardrailVersion":"2"}}""",
            handler.BodyJson));
    }

    [Fact]
    [UpstreamTest(
        Suite + "should map guardrail_intervened messageStop to a content-filter finish reason in streaming",
        Coverage = UpstreamCoverage.Partial,
        Note = "The prompt text part carries no guardContent provider option: user content parts have no provider options in this port.")]
    public async Task Should_map_guardrail_intervened_messageStop_to_a_content_filter_finish_reason_in_streaming()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Sorry, the model cannot answer this question."}}}""",
            """{"metadata":{"usage":{"inputTokens":18,"outputTokens":12,"totalTokens":30},"metrics":{"latencyMs":412},"trace":{"guardrail":{"inputAssessment":{"1abcd2ef34gh":{"topicPolicy":{"topics":[{"action":"BLOCKED","name":"Restricted topic","type":"DENY"}]}}}}}}}""",
            """{"messageStop":{"stopReason":"guardrail_intervened"}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Tell me something restricted.") },
        });

        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal(FinishReason.ContentFilter, finish.FinishReason);
        Assert.Equal("guardrail_intervened", finish.RawFinishReason);
        Assert.True(JsonEquals(
            """{"guardrail":{"inputAssessment":{"1abcd2ef34gh":{"topicPolicy":{"topics":[{"action":"BLOCKED","name":"Restricted topic","type":"DENY"}]}}}}}""",
            finish.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("trace")));
    }

    [Fact]
    [UpstreamTest(Suite + "should include trace information in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_trace_information_in_providerMetadata()
    {
        const string Trace = """{"guardrail":{"inputAssessment":{"1abcd2ef34gh":{"contentPolicy":{"filters":[{"action":"BLOCKED","confidence":"LOW","type":"INSULTS"}]},"wordPolicy":{"managedWordLists":[{"action":"BLOCKED","match":"<rude word>","type":"PROFANITY"}]}}}}}""";
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34,"totalTokens":38},"metrics":{"latencyMs":10},"trace":""" + Trace + "}}",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler);

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Hello\"",
            },
            Render(parts.Take(4)));
        var finish = Assert.IsType<FinishStreamPart>(parts[4]);
        Assert.Equal("finish stop raw=stop_sequence usage=in(4,4,0,0) out(34,34,undefined) raw={\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}", Render(finish).Split(" meta=")[0]);
        Assert.True(JsonEquals("{\"amazonBedrock\":{\"trace\":" + Trace + "},\"bedrock\":{\"trace\":" + Trace + "}}", finish.ProviderMetadata!.Value));
    }

    [Fact]
    [UpstreamTest(Suite + "should include stop_sequence in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_stop_sequence_in_provider_metadata()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34,"totalTokens":38}}}""",
            """{"messageStop":{"stopReason":"stop_sequence","additionalModelResponseFields":{"delta":{"stop_sequence":"STOP"}}}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions { Prompt = TestPrompt, StopSequences = new[] { "STOP" } });

        Assert.Equal(
            "finish stop raw=stop_sequence usage=in(4,4,0,0) out(34,34,undefined) raw={\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38} meta={\"amazonBedrock\":{\"stopSequence\":\"STOP\"},\"bedrock\":{\"stopSequence\":\"STOP\"}}",
            Render(parts.OfType<FinishStreamPart>().Single()));
    }

    [Fact]
    [UpstreamTest(Suite + "should correctly parse delta.stop_sequence structure in streaming with additional fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_correctly_parse_delta_stop_sequence_structure_in_streaming_with_additional_fields()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Response"}}}""",
            """{"metadata":{"usage":{"inputTokens":5,"outputTokens":10,"totalTokens":15}}}""",
            """{"messageStop":{"stopReason":"stop_sequence","additionalModelResponseFields":{"delta":{"stop_sequence":"CUSTOM_END"},"otherField":"value"}}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions { Prompt = TestPrompt, StopSequences = new[] { "CUSTOM_END" } });

        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal("CUSTOM_END", finish.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("stopSequence").GetString());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop_sequence", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should include response headers in rawResponse",
        Coverage = UpstreamCoverage.Partial,
        Note = "The stream result has no response object; the headers are checked on the response handler that feeds the stream.")]
    public async Task Should_include_response_headers_in_rawResponse()
    {
        var handler = new UpstreamEventStreamHandler(
            EventStreamFrames.FromChunkLines(new[]
            {
                """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
                """{"messageStop":{"stopReason":"stop_sequence"}}""",
            }),
            new Dictionary<string, string> { ["x-amzn-requestid"] = "test-request-id", ["x-amzn-trace-id"] = "test-trace-id" });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var response = await client.PostAsync("https://bedrock-runtime.us-east-1.amazonaws.com/model/x/converse-stream", new StringContent("{}"));
        var (headers, _) = await Vercel.AI.AmazonBedrock.AmazonBedrockEventStreamResponseHandler.HandleAsync(response, _ => null);

        Assert.Equal("no-cache", headers["cache-control"]);
        Assert.Equal("keep-alive", headers["connection"]);
        Assert.Equal("application/vnd.amazon.eventstream", headers["content-type"]);
        Assert.Equal("test-request-id", headers["x-amzn-requestid"]);
        Assert.Equal("test-trace-id", headers["x-amzn-trace-id"]);
    }

    [Fact]
    [UpstreamTest(Suite + "should extract response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_response_metadata()
    {
        var handler = new UpstreamEventStreamHandler(
            EventStreamFrames.FromChunkLines(new[]
            {
                """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
                """{"messageStop":{"stopReason":"end_turn"}}""",
            }),
            new Dictionary<string, string> { ["x-amzn-requestid"] = "test-request-id", ["date"] = "Wed, 01 Jan 2025 00:00:00 GMT" });
        var parts = await Stream(handler);

        Assert.Equal(
            "response-metadata id=test-request-id modelId=anthropic.claude-3-haiku-20240307-v1:0 timestamp=2025-01-01T00:00:00.000Z",
            Render(parts.OfType<ResponseMetadataStreamPart>().Single()));
    }

    [Fact]
    [UpstreamTest(Suite + "should properly combine headers from all sources", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_properly_combine_headers_from_all_sources()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = TestPrompt,
                Headers = new Dictionary<string, string?> { ["options-header"] = "options-value", ["shared-header"] = "options-shared" },
            },
            providerHeaders: new Dictionary<string, string?> { ["model-header"] = "model-value", ["shared-header"] = "model-shared" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.Equal("options-shared", handler.Headers["shared-header"]);
        Assert.StartsWith("AWS4-HMAC-SHA256", handler.Headers["Authorization"]);
        Assert.Contains("options-header", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest(Suite + "should work with partial headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_with_partial_headers()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = TestPrompt,
                Headers = new Dictionary<string, string?> { ["options-header"] = "options-value" },
            },
            providerHeaders: new Dictionary<string, string?> { ["model-header"] = "model-value" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
    }

    [Fact]
    [UpstreamTest(Suite + "should include providerOptions in the request for streaming calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_providerOptions_in_the_request_for_streaming_calls()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Dummy"}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, Call("""{"bedrock":{"foo":"bar"}}"""));

        Assert.True(JsonMatches("""{"foo":"bar"}""", handler.BodyJson));
    }

    [Fact]
    [UpstreamTest(Suite + "should include cache token usage in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_cache_token_usage_in_providerMetadata()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34,"totalTokens":38,"cacheReadInputTokens":2,"cacheWriteInputTokens":3}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler);

        Assert.Equal(
            "finish stop raw=stop_sequence usage=in(9,4,2,3) out(34,34,undefined) raw={\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38,\"cacheReadInputTokens\":2,\"cacheWriteInputTokens\":3}"
            + " meta={\"amazonBedrock\":{\"usage\":{\"cacheWriteInputTokens\":3}},\"bedrock\":{\"usage\":{\"cacheWriteInputTokens\":3}}}",
            Render(parts.OfType<FinishStreamPart>().Single()));
    }

    [Fact]
    [UpstreamTest(Suite + "should include performanceConfig in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_performanceConfig_in_providerMetadata()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34},"performanceConfig":{"latency":"optimized"}}}""",
            """{"messageStop":{"stopReason":"end_turn"}}""");
        var parts = await Stream(handler);

        Assert.True(JsonEquals(
            """{"amazonBedrock":{"performanceConfig":{"latency":"optimized"}},"bedrock":{"performanceConfig":{"latency":"optimized"}}}""",
            parts.OfType<FinishStreamPart>().Single().ProviderMetadata!.Value));
    }

    [Fact]
    [UpstreamTest(Suite + "should include serviceTier in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_serviceTier_in_providerMetadata()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34},"serviceTier":{"type":"on-demand"}}}""",
            """{"messageStop":{"stopReason":"end_turn"}}""");
        var parts = await Stream(handler);

        Assert.True(JsonEquals(
            """{"amazonBedrock":{"serviceTier":{"type":"on-demand"}},"bedrock":{"serviceTier":{"type":"on-demand"}}}""",
            parts.OfType<FinishStreamPart>().Single().ProviderMetadata!.Value));
    }

    [Fact]
    [UpstreamTest(Suite + "should include cacheDetails in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_cacheDetails_in_providerMetadata()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"metadata":{"usage":{"inputTokens":4,"outputTokens":34,"cacheDetails":[{"inputTokens":100,"ttl":"T5M"}]}}}""",
            """{"messageStop":{"stopReason":"end_turn"}}""");
        var parts = await Stream(handler);

        Assert.True(JsonEquals(
            """{"amazonBedrock":{"usage":{"cacheDetails":[{"inputTokens":100,"ttl":"T5M"}]}},"bedrock":{"usage":{"cacheDetails":[{"inputTokens":100,"ttl":"T5M"}]}}}""",
            parts.OfType<FinishStreamPart>().Single().ProviderMetadata!.Value));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream reasoning text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_text_deltas()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"reasoningContent":{"text":"I am thinking"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"reasoningContent":{"text":" about this problem..."}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"reasoningContent":{"signature":"abc123signature"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"text":"Based on my reasoning, the answer is 42."}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler);

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "reasoning-start 0",
                "reasoning-delta 0 \"I am thinking\"",
                "reasoning-delta 0 \" about this problem...\"",
                "reasoning-delta 0 \"\" meta={\"amazonBedrock\":{\"signature\":\"abc123signature\"},\"bedrock\":{\"signature\":\"abc123signature\"}}",
                "text-start 1",
                "text-delta 1 \"Based on my reasoning, the answer is 42.\"",
                "finish stop raw=stop_sequence usage=" + NoUsage,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream redacted reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_redacted_reasoning()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"reasoningContent":{"data":"redacted-reasoning-data"}}}}""",
            """{"contentBlockDelta":{"contentBlockIndex":1,"delta":{"text":"Here is my answer."}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler);

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "reasoning-start 0",
                "reasoning-delta 0 \"\" meta={\"amazonBedrock\":{\"redactedData\":\"redacted-reasoning-data\"},\"bedrock\":{\"redactedData\":\"redacted-reasoning-data\"}}",
                "text-start 1",
                "text-delta 1 \"Here is my answer.\"",
                "finish stop raw=stop_sequence usage=" + NoUsage,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should include raw chunks when includeRawChunks is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_raw_chunks_when_includeRawChunks_is_true()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        var parts = await Stream(handler, new LanguageModelCallOptions { Prompt = TestPrompt, IncludeRawChunks = true });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "raw {\"contentBlockDelta\":{\"contentBlockIndex\":0,\"delta\":{\"text\":\"Hello\"}}}",
                "text-start 0",
                "text-delta 0 \"Hello\"",
                "raw {\"messageStop\":{\"stopReason\":\"stop_sequence\"}}",
                "finish stop raw=stop_sequence usage=" + NoUsage,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should transform reasoningConfig to thinking in stream requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_reasoningConfig_to_thinking_in_stream_requests()
    {
        var handler = Chunks(
            """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello"}}}""",
            """{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            MaxOutputTokens = 100,
            ProviderOptions = Options("""{"bedrock":{"reasoningConfig":{"type":"enabled","budgetTokens":2000}}}"""),
        });

        var requestBody = handler.BodyJson;
        Assert.True(JsonMatches("""{"additionalModelRequestFields":{"thinking":{"type":"enabled","budget_tokens":2000}},"inferenceConfig":{"maxTokens":2100}}""", requestBody));
        Assert.False(requestBody.TryGetProperty("reasoningConfig", out _));
    }

    [Fact]
    [UpstreamTest(Suite + "merges user additionalModelRequestFields with derived thinking (stream)", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_user_additionalModelRequestFields_with_derived_thinking_stream()
    {
        var handler = Chunks("""{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, Call("""{"bedrock":{"reasoningConfig":{"type":"enabled","budgetTokens":500},"additionalModelRequestFields":{"foo":"bar","custom":42}}}"""));

        var body = handler.BodyJson;
        Assert.False(body.TryGetProperty("reasoningConfig", out _));
        Assert.True(JsonMatches("""{"foo":"bar","custom":42,"thinking":{"type":"enabled","budget_tokens":500}}""", body.GetProperty("additionalModelRequestFields")));
    }

    [Fact]
    [UpstreamTest(Suite + "should pass serviceTier provider option in stream requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_serviceTier_provider_option_in_stream_requests()
    {
        var handler = Chunks("""{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, Call("""{"bedrock":{"serviceTier":"priority"}}"""));

        var requestBody = handler.BodyJson;
        Assert.True(JsonMatches("""{"serviceTier":{"type":"priority"}}""", requestBody));
        Assert.False(requestBody.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty("serviceTier", out _));
    }

    [Fact]
    [UpstreamTest(Suite + "should pass requestMetadata in stream requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_requestMetadata_in_stream_requests()
    {
        var handler = Chunks("""{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, Call("""{"amazonBedrock":{"requestMetadata":{"team":"search","environment":"prod"}}}"""));

        var requestBody = handler.BodyJson;
        Assert.True(JsonMatches("""{"requestMetadata":{"team":"search","environment":"prod"}}""", requestBody));
        Assert.False(requestBody.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty("requestMetadata", out _));
    }

    [Fact]
    [UpstreamTest(Suite + "should omit requestMetadata from stream requests when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_requestMetadata_from_stream_requests_when_not_provided()
    {
        var handler = Chunks("""{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler);

        Assert.False(handler.BodyJson.TryGetProperty("requestMetadata", out _));
        Assert.DoesNotContain("requestMetadata", handler.Body);
    }

    [Fact]
    [UpstreamTest(Suite + "should handle JSON response format in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_JSON_response_format_in_streaming()
    {
        var parts = await Stream(Fixture("json-tool.1"), JsonCall("Generate JSON", """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""));

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"{\\\"value\\\":\\\"test\\\"}\"",
                "text-end 0",
                "finish stop raw=tool_use usage=" + NoUsage + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should include text content before JSON tool call in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_text_content_before_JSON_tool_call_in_streaming()
    {
        var parts = await Stream(Fixture("json-tool.2"), JsonCall("Generate JSON", """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""));

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Let me generate\"",
                "text-delta 0 \" that JSON for you.\"",
                "text-start 1",
                "text-delta 1 \"{\\\"value\\\":\\\"test\\\"}\"",
                "text-end 1",
                "finish stop raw=tool_use usage=" + NoUsage + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream text introduction before JSON-only response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_introduction_before_JSON_only_response()
    {
        var parts = await Stream(Fixture("json-only-text-first.1"), JsonCall("Return name data", """{"type":"object","properties":{"name":{"type":"string"}},"required":["name"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""));

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Here is your data:\"",
                "text-end 0",
                "text-start 1",
                "text-delta 1 \"{\\\"name\\\":\\\"John\\\"}\"",
                "text-end 1",
                "finish stop raw=tool_use usage=in(100,100,0,0) out(20,20,undefined) raw={\"inputTokens\":100,\"outputTokens\":20,\"totalTokens\":120}" + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should include multiple text blocks before JSON tool call in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_multiple_text_blocks_before_JSON_tool_call_in_streaming()
    {
        var parts = await Stream(Fixture("json-tool.3"), JsonCall("Generate data", """{"type":"object","properties":{"result":{"type":"number"}},"required":["result"]}"""));

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"First text block.\"",
                "text-start 1",
                "text-delta 1 \"Second text block.\"",
                "text-start 2",
                "text-delta 2 \"Third block.\"",
                "text-start 3",
                "text-delta 3 \"{\\\"result\\\":42}\"",
                "text-end 3",
                "finish stop raw=tool_use usage=" + NoUsage + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should handle regular tool call before JSON tool call in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_regular_tool_call_before_JSON_tool_call_in_streaming()
    {
        var parts = await Stream(Fixture("json-with-tool.1"), new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Test") },
            Tools = new[] { new LanguageModelTool("get-weather", null, Schema("""{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}""")) },
            JsonSchema = Schema("""{"type":"object","properties":{"result":{"type":"string"}},"required":["result"]}"""),
        });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Calling a tool first.\"",
                "tool-input-start tool-1 get-weather",
                "tool-input-delta tool-1 \"{\\\"location\\\":\\\"SF\\\"}\"",
                "tool-input-end tool-1",
                "tool-call tool-1 get-weather \"{\\\"location\\\":\\\"SF\\\"}\"",
                "text-start 2",
                "text-delta 2 \"Now JSON.\"",
                "text-start 3",
                "text-delta 3 \"{\\\"result\\\":\\\"data\\\"}\"",
                "text-end 3",
                "finish stop raw=tool_use usage=" + NoUsage + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should handle multiple regular tool calls before JSON tool call in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_multiple_regular_tool_calls_before_JSON_tool_call_in_streaming()
    {
        var parts = await Stream(Fixture("json-with-tools.1"), new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Test") },
            Tools = new[]
            {
                new LanguageModelTool("tool-a", null, Schema("""{"type":"object","properties":{"a":{"type":"string"}}}""")),
                new LanguageModelTool("tool-b", null, Schema("""{"type":"object","properties":{"b":{"type":"string"}}}""")),
            },
            JsonSchema = Schema("""{"type":"object","properties":{"final":{"type":"string"}},"required":["final"]}"""),
        });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Multiple tools.\"",
                "tool-input-start tool-1 tool-a",
                "tool-input-delta tool-1 \"{\\\"a\\\":\\\"1\\\"}\"",
                "tool-input-end tool-1",
                "tool-call tool-1 tool-a \"{\\\"a\\\":\\\"1\\\"}\"",
                "tool-input-start tool-2 tool-b",
                "tool-input-delta tool-2 \"{\\\"b\\\":\\\"2\\\"}\"",
                "tool-input-end tool-2",
                "tool-call tool-2 tool-b \"{\\\"b\\\":\\\"2\\\"}\"",
                "text-start 3",
                "text-delta 3 \"Final JSON.\"",
                "text-start 4",
                "text-delta 4 \"{\\\"final\\\":\\\"result\\\"}\"",
                "text-end 4",
                "finish stop raw=tool_use usage=" + NoUsage + JsonToolMeta,
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should stream text, then regular tool calls, with JSON response format available", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_then_regular_tool_calls_with_JSON_response_format_available()
    {
        var parts = await Stream(Fixture("json-tool-text-then-weather-then-json.1"), new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("What's 2+2? Also check the weather in San Francisco and London.") },
            Tools = new[] { new LanguageModelTool("weather", null, Schema("""{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}""")) },
            JsonSchema = Schema("""{"type":"object","properties":{"locations":{"type":"array","items":{"type":"object","properties":{"location":{"type":"string"},"temperature":{"type":"number"}}}}}}"""),
        });

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"2 + 2 equals 4. Now let me check\"",
                "text-delta 0 \" the weather for you.\"",
                "text-end 0",
                "tool-input-start weather-tool-1 weather",
                "tool-input-delta weather-tool-1 \"{\\\"location\\\":\"",
                "tool-input-delta weather-tool-1 \"\\\"San Francisco\\\"}\"",
                "tool-input-end weather-tool-1",
                "tool-call weather-tool-1 weather \"{\\\"location\\\":\\\"San Francisco\\\"}\"",
                "tool-input-start weather-tool-2 weather",
                "tool-input-delta weather-tool-2 \"{\\\"location\\\":\\\"London\\\"}\"",
                "tool-input-end weather-tool-2",
                "tool-call weather-tool-2 weather \"{\\\"location\\\":\\\"London\\\"}\"",
                "finish tool-calls raw=tool_use usage=in(500,500,0,0) out(100,100,undefined) raw={\"inputTokens\":500,\"outputTokens\":100,\"totalTokens\":600}",
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should warn when Anthropic model receives maxReasoningEffort in stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_when_Anthropic_model_receives_maxReasoningEffort_in_stream()
    {
        var handler = Chunks("""{"messageStop":{"stopReason":"stop_sequence"}}""");
        await Stream(handler, Call("""{"bedrock":{"reasoningConfig":{"type":"enabled","maxReasoningEffort":"medium"}}}"""));

        var body = handler.BodyJson;
        Assert.False(body.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty("reasoningConfig", out _));
    }

    [Fact]
    [UpstreamTest(Suite + "should support tool calls with empty input (no arguments)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_tool_calls_with_empty_input_no_arguments()
    {
        var parts = await Stream(Fixture("tool-no-args"), new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Tools = new[]
            {
                new LanguageModelTool("updateIssueList", null, Schema("""{"type":"object","properties":{},"required":[],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""")),
            },
        });

        var toolCall = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("{}", toolCall.ArgumentsJson);
    }

    private static async Task AssertStreamError(string type, string name, string message, int statusCode, bool isRetryable)
    {
        var data = "{\"message\":\"" + message + "\",\"name\":\"" + name + "\",\"$fault\":\"server\",\"$metadata\":{}}";
        var handler = Chunks("{\"" + type + "\":" + data + "}");
        var parts = await Stream(handler);

        Assert.Equal(4, parts.Count);
        Assert.Equal(Start, Render(parts[0]));
        Assert.Equal(Meta, Render(parts[1]));
        var error = Assert.IsType<ErrorStreamPart>(parts[2]);
        var providerError = Assert.IsType<Vercel.AI.Prompt.ProviderStreamError>(error.Error);
        Assert.Null(providerError.Code);
        Assert.Equal(isRetryable, providerError.IsRetryable);
        Assert.Equal(message, providerError.Message);
        Assert.Equal(statusCode, providerError.StatusCode);
        Assert.Equal(type, providerError.Type);
        Assert.True(JsonEquals("{\"" + type + "\":" + data + "}", Assert.IsType<JsonElement>(providerError.Data)));
        Assert.Equal("finish error raw=undefined usage=" + NoUsage, Render(parts[3]));
    }

    private static LanguageModelCallOptions JsonCall(string prompt, string schema)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(prompt) },
            JsonSchema = Schema(schema),
        };
    }

    private static JsonElement Schema(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
