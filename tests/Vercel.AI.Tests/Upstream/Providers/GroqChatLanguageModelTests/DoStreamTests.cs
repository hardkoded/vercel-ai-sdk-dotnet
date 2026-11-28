// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest(
        "packages/groq/src/groq-chat-language-model.test.ts::doStream::handles a stream ending with %s as %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_a_stream_ending_with_ending_as_unified(
        string ending,
        FinishReason unified
    )
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"id\":\"response-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Partial answer\"},\"finish_reason\":null}]}\n\n",
            }.Concat(
                ending switch
                {
                    "missing" => Array.Empty<string>(),
                    "invalid-json" => new[] { "data: {invalid-json}\n\n" },
                    _ => new[]
                    {
                        "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\""
                            + ending
                            + "\"}]}\n\n",
                    },
                }
            )
        );
        var model = (OpenAICompatibleLanguageModel)
            GroqProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler)
                .LanguageModel("gemma2-9b-it");

        var parts = await UpstreamChat.Read(
            await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None)
        );

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part =>
            part.Delta == "Partial answer"
        );
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }

    private const string Prefix = TestFile + "::doStream::";

    private const string ToolSchemaTool = ValueSchema;

    private static string Chunk(string json) => "data: " + json + "\n\n";

    private static LanguageModelCallOptions WithTool(
        string name = "test-tool",
        string schema = ValueSchema
    )
    {
        var options = TestPrompt();
        options.Tools = [new LanguageModelTool(name, null, Schema(schema))];
        return options;
    }

    private static void AssertToolCallStream(
        List<LanguageModelStreamPart> parts,
        string id,
        string name,
        string[] deltas,
        string input
    )
    {
        Assert.Collection(
            parts.Where(part => part is not FinishStreamPart),
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part => Assert.IsType<ResponseMetadataStreamPart>(part),
            part =>
            {
                var start = Assert.IsType<ToolInputStartStreamPart>(part);
                Assert.Equal(id, start.Id);
                Assert.Equal(name, start.ToolName);
            },
            part => Assert.Equal(deltas[0], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[1], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[2], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[3], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[4], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[5], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[6], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(deltas[7], Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal(id, Assert.IsType<ToolInputEndStreamPart>(part).Id),
            part =>
            {
                var call = Assert.IsType<ToolCallStreamPart>(part);
                Assert.Equal(id, call.ToolCallId);
                Assert.Equal(name, call.ToolName);
                Assert.Equal(input, call.ArgumentsJson);
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should stream tool call deltas when tool call arguments are passed in the first chunk",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_call_deltas_when_tool_call_arguments_are_passed_in_the_first_chunk()
    {
        const string head =
            """{"id":"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798","object":"chat.completion.chunk","created":1711357598,"model":"gemma2-9b-it","system_fingerprint":"fp_3bc1b5746c","choices":[{"index":0,"delta":""";
        string Delta(string arguments) =>
            Chunk(
                head
                    + """{"tool_calls":[{"index":0,"function":{"arguments":"""
                    + JsonValue.Create(arguments)!.ToJsonString()
                    + "}}]},\"finish_reason\":null}]}"
            );
        var handler = new SseStreamHandler([
            Chunk(
                head
                    + """{"role":"assistant","content":null,"tool_calls":[{"index":0,"id":"call_O17Uplv4lJvD6DVdIvFFeRMw","type":"function","function":{"name":"test-tool","arguments":"{\""}}]},"finish_reason":null}]}"""
            ),
            Delta("va"),
            Delta("lue"),
            Delta("\":\""),
            Delta("Spark"),
            Delta("le"),
            Delta(" Day"),
            Delta("\"}"),
            Chunk(
                """{"id":"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798","object":"chat.completion.chunk","created":1729171479,"model":"gemma2-9b-it","system_fingerprint":"fp_10c08bf97d","choices":[{"index":0,"delta":{},"logprobs":null,"finish_reason":"tool_calls"}],"x_groq":{"id":"req_01jadadp0femyae9kav1gpkhe8","usage":{"queue_time":0.061348671,"prompt_tokens":18,"prompt_time":0.000211569,"completion_tokens":439,"completion_time":0.798181818,"total_tokens":457,"total_time":0.798393387}}}"""
            ),
            "data: [DONE]\n\n",
        ]);

        var parts = await Stream(Model(handler), WithTool());

        AssertToolCallStream(
            parts,
            "call_O17Uplv4lJvD6DVdIvFFeRMw",
            "test-tool",
            ["{\"", "va", "lue", "\":\"", "Spark", "le", " Day", "\"}"],
            "{\"value\":\"Sparkle Day\"}"
        );
        var response = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798", response.Id);
        Assert.Equal("gemma2-9b-it", response.ModelId);
        Assert.Equal(new DateTimeOffset(2024, 3, 25, 9, 6, 38, TimeSpan.Zero), response.Timestamp);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("tool_calls", finish.RawFinishReason);
        AssertUsage(
            finish.Usage,
            18,
            18,
            null,
            439,
            439,
            null,
            """{"prompt_tokens":18,"completion_tokens":439,"total_tokens":457}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should not duplicate tool calls when there is an additional empty chunk after the tool call has been completed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_duplicate_tool_calls_when_there_is_an_additional_empty_chunk_after_the_tool_call_has_been_completed()
    {
        const string head =
            """{"id":"chat-2267f7e2910a4254bac0650ba74cfc1c","object":"chat.completion.chunk","created":1733162241,"model":"meta/llama-3.1-8b-instruct:fp8","choices":""";
        string Args(string arguments, int completion, string finish = "null", string extra = "") =>
            Chunk(
                head
                    + """[{"index":0,"delta":{"tool_calls":[{"index":0,"function":{"arguments":"""
                    + JsonValue.Create(arguments)!.ToJsonString()
                    + "}}]},\"logprobs\":null,\"finish_reason\":"
                    + finish
                    + extra
                    + "}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":"
                    + (226 + completion)
                    + ",\"completion_tokens\":"
                    + completion
                    + "}}"
            );
        var handler = new SseStreamHandler([
            Chunk(
                head
                    + """[{"index":0,"delta":{"role":"assistant","content":""},"logprobs":null,"finish_reason":null}],"usage":{"prompt_tokens":226,"total_tokens":226,"completion_tokens":0}}"""
            ),
            Chunk(
                head
                    + """[{"index":0,"delta":{"tool_calls":[{"id":"chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa","type":"function","index":0,"function":{"name":"searchGoogle"}}]},"logprobs":null,"finish_reason":null}],"usage":{"prompt_tokens":226,"total_tokens":233,"completion_tokens":7}}"""
            ),
            Args("{\"query\": \"", 15),
            Args("latest", 16),
            Args(" news", 17),
            Args(" on", 18),
            Args(" ai\"}", 19),
            Args("", 20, "\"tool_calls\"", ",\"stop_reason\":128008"),
            Chunk(
                head
                    + """[],"usage":{"prompt_tokens":226,"total_tokens":246,"completion_tokens":20}}"""
            ),
            "data: [DONE]\n\n",
        ]);
        var options = WithTool(
            "searchGoogle",
            """{"type":"object","properties":{"query":{"type":"string"}},"required":["query"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
        );

        var parts = await Stream(Model(handler), options);

        Assert.Single(parts.OfType<ToolCallStreamPart>());
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa", call.ToolCallId);
        Assert.Equal("searchGoogle", call.ToolName);
        Assert.Equal("{\"query\": \"latest news on ai\"}", call.ArgumentsJson);
        Assert.Equal(
            ["{\"query\": \"", "latest", " news", " on", " ai\"}", ""],
            parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta)
        );
        var response = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chat-2267f7e2910a4254bac0650ba74cfc1c", response.Id);
        Assert.Equal("meta/llama-3.1-8b-instruct:fp8", response.ModelId);
        Assert.Equal(
            new DateTimeOffset(2024, 12, 2, 17, 57, 21, TimeSpan.Zero),
            response.Timestamp
        );
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("tool_calls", finish.RawFinishReason);
        // Only `x_groq.usage` carries usage in Groq chunks. A top-level `usage` is ignored.
        AssertUsage(finish.Usage, null, null, null, null, null, null, null);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream tool call that is sent in one chunk",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_call_that_is_sent_in_one_chunk()
    {
        var handler = new SseStreamHandler([
            Chunk(
                """{"id":"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798","object":"chat.completion.chunk","created":1711357598,"model":"gemma2-9b-it","system_fingerprint":"fp_3bc1b5746c","choices":[{"index":0,"delta":{"role":"assistant","content":null,"tool_calls":[{"index":0,"id":"call_O17Uplv4lJvD6DVdIvFFeRMw","type":"function","function":{"name":"test-tool","arguments":"{\"value\":\"Sparkle Day\"}"}}]},"finish_reason":null}]}"""
            ),
            Chunk(
                """{"id":"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798","object":"chat.completion.chunk","created":1729171479,"model":"gemma2-9b-it","system_fingerprint":"fp_10c08bf97d","choices":[{"index":0,"delta":{},"logprobs":null,"finish_reason":"tool_calls"}],"x_groq":{"id":"req_01jadadp0femyae9kav1gpkhe8","usage":{"queue_time":0.061348671,"prompt_tokens":18,"prompt_time":0.000211569,"completion_tokens":439,"completion_time":0.798181818,"total_tokens":457,"total_time":0.798393387}}}"""
            ),
            "data: [DONE]\n\n",
        ]);

        var parts = await Stream(Model(handler), WithTool());

        Assert.Collection(
            parts,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part => Assert.IsType<ResponseMetadataStreamPart>(part),
            part =>
            {
                var start = Assert.IsType<ToolInputStartStreamPart>(part);
                Assert.Equal("call_O17Uplv4lJvD6DVdIvFFeRMw", start.Id);
                Assert.Equal("test-tool", start.ToolName);
            },
            part =>
                Assert.Equal(
                    "{\"value\":\"Sparkle Day\"}",
                    Assert.IsType<ToolInputDeltaStreamPart>(part).Delta
                ),
            part => Assert.IsType<ToolInputEndStreamPart>(part),
            part =>
            {
                var call = Assert.IsType<ToolCallStreamPart>(part);
                Assert.Equal("call_O17Uplv4lJvD6DVdIvFFeRMw", call.ToolCallId);
                Assert.Equal("test-tool", call.ToolName);
                Assert.Equal("{\"value\":\"Sparkle Day\"}", call.ArgumentsJson);
            },
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
                Assert.Equal("tool_calls", finish.RawFinishReason);
                AssertUsage(
                    finish.Usage,
                    18,
                    18,
                    null,
                    439,
                    439,
                    null,
                    """{"prompt_tokens":18,"completion_tokens":439,"total_tokens":457}"""
                );
            }
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle error stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_error_stream_parts()
    {
        var handler = new SseStreamHandler([
            Chunk("""{"error":{"message":"Rate limit reached","type":"rate_limit_error"}}"""),
            "data: [DONE]\n\n",
        ]);

        var parts = await Stream(Model(handler));

        Assert.Collection(
            parts,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part =>
            {
                var error = Assert.IsType<ErrorStreamPart>(part).Error!;
                Assert.Equal("Rate limit reached", error.Message);
                Assert.Equal("rate_limit_error", error.Type);
                Assert.Null(error.Code);
                Assert.Equal(429, error.StatusCode);
                Assert.True(error.IsRetryable);
                Assert.NotNull(error.Data);
            },
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(FinishReason.Error, finish.FinishReason);
                Assert.Null(finish.RawFinishReason);
                AssertUsage(finish.Usage, null, null, null, null, null, null, null);
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle unparsable stream parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_unparsable_stream_parts()
    {
        var handler = new SseStreamHandler([Chunk("{unparsable}"), "data: [DONE]\n\n"]);

        var parts = await Stream(Model(handler));

        Assert.Collection(
            parts,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part => Assert.IsType<ErrorStreamPart>(part),
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(FinishReason.Error, finish.FinishReason);
                Assert.Null(finish.RawFinishReason);
                AssertUsage(finish.Usage, null, null, null, null, null, null, null);
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var handler = ChunksFixture(
            "groq-text",
            new Dictionary<string, string> { ["test-header"] = "test-value" }
        );

        var result = await Model(handler).DoStreamAsync(TestPrompt(), CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        Assert.Equal("test-value", result.Response!.Headers["test-header"]);
        Assert.Equal("text/event-stream", result.Response.Headers["content-type"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct streaming request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_streaming_request_body()
    {
        var handler = ChunksFixture("groq-text");

        await Stream(Model(handler));

        AssertJson(
            """{"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it","stream":true}""",
            JsonNode.Parse(handler.Body)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = string.Concat(SseStreamHandler.FixtureChunks("groq-text", done: true)),
        };
        var provider = GroqChatTestSupport.Provider(capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var options = TestPrompt();
        options.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        var result = await provider
            .LanguageModel("gemma2-9b-it")
            .DoStreamAsync(options, CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        var headers = capture.Requests[0].Headers;
        Assert.Equal("Bearer test-api-key", headers["Authorization"]);
        Assert.StartsWith("application/json", headers["Content-Type"]);
        Assert.Equal("provider-header-value", headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", headers["Custom-Request-Header"]);
    }
}
