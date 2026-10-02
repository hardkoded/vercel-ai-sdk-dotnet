// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAIChatLanguageModelStreamTests
{
    private const string File = "packages/openai/src/chat/openai-chat-language-model.test.ts";

    private const string TextId = "chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP";

    private const string ToolId = "call_O17Uplv4lJvD6DVdIvFFeRMw";

    [Fact]
    [UpstreamTest(File + "::doStream::should stream text after Azure content filter chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_after_azure_content_filter_chunks()
    {
        var parts = await StreamAsync(OpenAIParity.Sse(
            """{"choices":[],"created":0,"id":"","model":"","object":"","prompt_filter_results":[{"prompt_index":0,"content_filter_results":{}}]}""",
            """{"id":"chatcmpl-test","object":"chat.completion.chunk","created":1,"model":"gpt-4o","choices":[{"index":0,"delta":{"content":"","role":"assistant"},"finish_reason":null}]}""",
            """{"id":"chatcmpl-test","object":"chat.completion.chunk","created":1,"model":"gpt-4o","choices":[{"index":0,"delta":{"content":"Hello"},"finish_reason":null}]}""",
            """{"id":"chatcmpl-test","object":"chat.completion.chunk","created":1,"model":"gpt-4o","choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}""",
            """{"choices":[{"content_filter_offsets":{},"content_filter_results":{},"finish_reason":null,"index":0}],"created":0,"id":"","model":"","object":""}"""));
        var deltas = parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray();
        Assert.Equal(new[] { string.Empty, "Hello" }, deltas);
        Assert.Equal("chatcmpl-test", Assert.Single(parts.OfType<ResponseMetadataStreamPart>()).Id);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_deltas()
    {
        const string logprobs = """[{"token":"Hello","logprob":-0.0009994634,"top_logprobs":[{"token":"Hello","logprob":-0.0009994634}]}]""";
        var parts = await StreamAsync(TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":1,"delta":{"content":"Hello"},"finish_reason":null}""",
            """{"index":1,"delta":{"content":", "},"finish_reason":null}""",
            """{"index":1,"delta":{"content":"World!"},"finish_reason":null}""",
            "{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\",\"logprobs\":{\"content\":" + logprobs + "}}",
            usageChoice: """{"prompt_tokens":17,"total_tokens":244,"completion_tokens":227}"""));
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal(TextId, metadata.Id);
        Assert.Equal("gpt-3.5-turbo-0613", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1702657020), metadata.Timestamp);
        Assert.Equal(new[] { string.Empty, "Hello", ", ", "World!" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal("0", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        OpenAIParity.AssertUsage(finish.Usage, 17, 227, 0, 0, null, 244);
        OpenAIParity.AssertJson(finish.ProviderMetadata!.Value.GetProperty("openai").GetProperty("logprobs").GetRawText(), logprobs);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream annotations/citations", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_annotations()
    {
        var parts = await StreamAsync(TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":1,"delta":{"content":"Based on search results"},"finish_reason":null}""",
            """{"index":1,"delta":{"annotations":[{"type":"url_citation","url_citation":{"start_index":24,"end_index":29,"url":"https://example.com/doc1.pdf","title":"Document 1"}}]},"finish_reason":null}""",
            """{"index":0,"delta":{},"finish_reason":"stop"}""",
            usageChoice: """{"prompt_tokens":17,"completion_tokens":227,"total_tokens":244}"""));
        var source = Assert.Single(parts.OfType<SourceStreamPart>());
        Assert.Equal("https://example.com/doc1.pdf", source.Url);
        Assert.Equal("Document 1", source.Title);
        Assert.Contains(parts, part => part is TextDeltaStreamPart delta && delta.Delta == "Based on search results");
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_tool_deltas()
    {
        var parts = await StreamTools(new[]
        {
            ToolStart(string.Empty),
            ToolArgs("{\\\""),
            ToolArgs("value"),
            ToolArgs("\\\":\\\""),
            ToolArgs("Spark"),
            ToolArgs("le"),
            ToolArgs(" Day"),
            ToolArgs("\\\"}"),
        });
        Assert.Equal(
            new[] { "{\"", "value", "\":\"", "Spark", "le", " Day", "\"}" },
            parts.OfType<OpenAIToolInputDeltaStreamPart>().Select(part => part.Delta).ToArray());
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(ToolId, call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream tool call deltas when tool call arguments are passed in the first chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_tool_arguments_from_the_first_chunk()
    {
        var parts = await StreamTools(new[]
        {
            ToolStart("{\\\""),
            ToolArgs("va"),
            ToolArgs("lue"),
            ToolArgs("\\\":\\\""),
            ToolArgs("Spark"),
            ToolArgs("le"),
            ToolArgs(" Day"),
            ToolArgs("\\\"}"),
        });
        Assert.Equal("{\"", parts.OfType<OpenAIToolInputDeltaStreamPart>().First().Delta);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", Assert.Single(parts.OfType<ToolCallStreamPart>()).ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should not duplicate tool calls when there is an additional empty chunk after the tool call has been completed", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_duplicate_a_finished_tool_call()
    {
        var id = "chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa";
        var parts = await StreamTools(
            new[]
            {
                """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
                "{\"index\":0,\"delta\":{\"tool_calls\":[{\"id\":\"" + id + "\",\"type\":\"function\",\"index\":0,\"function\":{\"name\":\"searchGoogle\"}}]},\"finish_reason\":null}",
                ToolArgs("{\\\"query\\\": \\\""),
                ToolArgs("latest"),
                ToolArgs(" news"),
                ToolArgs(" on"),
                ToolArgs(" ai\\\"}"),
                ToolArgs(string.Empty, finish: "tool_calls"),
            },
            "searchGoogle",
            """{"prompt_tokens":226,"total_tokens":246,"completion_tokens":20}""");
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(id, call.ToolCallId);
        Assert.Equal("searchGoogle", call.ToolName);
        Assert.Equal("{\"query\": \"latest news on ai\"}", call.ArgumentsJson);
        Assert.Contains(parts, part => part is OpenAIToolInputDeltaStreamPart delta && delta.Delta.Length == 0);
        OpenAIParity.AssertUsage(Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).Usage, 226, 20, 0, 0, null, 246);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should not finalize tool call early when partial JSON is coincidentally parsable", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_finalize_partial_json()
    {
        var parts = await StreamTools(
            new[]
            {
                ToolStart(string.Empty, "call_early123", "search"),
                ToolArgs("{\\\"query\\\": \\\"test\\\"}"),
                ToolArgs(string.Empty),
                ToolArgs(", \\\"limit\\\": 10}"),
                """{"index":0,"delta":{},"finish_reason":"tool_calls"}""",
            },
            "search",
            usage: null);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_early123", call.ToolCallId);
        Assert.Equal("search", call.ToolName);
        Assert.Equal("{\"query\": \"test\"}, \"limit\": 10}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream tool call with missing type field (Azure AI Foundry / Mistral)", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_tool_call_without_a_type()
    {
        var parts = await StreamTools(
            new[]
            {
                """{"index":0,"delta":{"role":"assistant","content":null,"tool_calls":[{"index":0,"id":"call_abc123","function":{"name":"test-tool","arguments":""}}]},"finish_reason":null}""",
                ToolArgs("{\\\"value\\\""),
                ToolArgs(":\\\"hello\\\"}", finish: "tool_calls"),
            },
            usage: """{"prompt_tokens":10,"completion_tokens":5,"total_tokens":15}""");
        var start = Assert.Single(parts.OfType<OpenAIToolInputStartStreamPart>());
        Assert.Equal("call_abc123", start.Id);
        Assert.Equal("test-tool", start.ToolName);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_abc123", call.ToolCallId);
        Assert.Equal("{\"value\":\"hello\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should stream tool call that is sent in one chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_tool_call_in_one_chunk()
    {
        var parts = await StreamTools(new[] { ToolStart("{\\\"value\\\":\\\"Sparkle Day\\\"}") });
        Assert.Equal("{\"value\":\"Sparkle Day\"}", Assert.Single(parts.OfType<OpenAIToolInputDeltaStreamPart>()).Delta);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", Assert.Single(parts.OfType<ToolCallStreamPart>()).ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should throw an api error when the first stream chunk is an error", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_the_first_chunk_is_an_error()
    {
        var error = await Assert.ThrowsAsync<OpenAIApiCallException>(() => ReadAsync(OpenAIParity.Sse(
            """{"error":{"message":"The server had an error processing your request. Sorry about that! You can retry your request, or contact us through our help center at help.openai.com if you keep seeing this error.","type":"server_error","param":null,"code":null}}""")));
        Assert.Equal("The server had an error processing your request. Sorry about that! You can retry your request, or contact us through our help center at help.openai.com if you keep seeing this error.", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.True(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should preserve numeric status codes from early stream errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_numeric_status_codes()
    {
        var error = await Assert.ThrowsAsync<OpenAIApiCallException>(() => ReadAsync(OpenAIParity.Sse(
            """{"error":{"message":"bad request","type":"provider_error","param":null,"code":400}}""")));
        Assert.Equal("bad request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should forward error stream parts after output has started", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_an_error_after_output()
    {
        var parts = await StreamAsync(OpenAIParity.Sse(
            Chunk("[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null}]", id: "chatcmpl-error-after-output"),
            """{"error":{"message":"stream failed after output","type":"server_error","param":null,"code":null}}"""));
        Assert.Equal("Hello", Assert.Single(parts.OfType<TextDeltaStreamPart>()).Delta);
        var error = Assert.Single(parts.OfType<OpenAIStreamErrorPart>());
        Assert.Equal("stream failed after output", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.True(error.IsRetryable);
        Assert.Equal("server_error", error.ErrorType);
        Assert.Single(parts.OfType<TextEndStreamPart>());
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_unparsable_stream_parts()
    {
        var parts = await StreamAsync(OpenAIParity.Sse("{unparsable}"));
        var error = Assert.Single(parts.OfType<ErrorStreamPart>());
        Assert.Contains("JSON parsing failed", error.Message, StringComparison.Ordinal);
        Assert.Contains("{unparsable}", error.Message, StringComparison.Ordinal);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_stream_request_body()
    {
        var handler = await SendAsync(OpenAIParity.Hello());
        OpenAIParity.AssertBody(handler, StreamBody("gpt-3.5-turbo"));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_raw_stream_response_headers()
    {
        var handler = new RecordingHandler
        {
            ResponseBody = OpenAIParity.Sse(Chunk("""{"index":0,"delta":{"content":"Hi"},"finish_reason":"stop"}""")),
            MediaType = "text/event-stream",
        };
        handler.ResponseHeaders["test-header"] = "test-value";
        handler.ResponseHeaders["cache-control"] = "no-cache";
        handler.ResponseHeaders["connection"] = "keep-alive";
        var model = OpenAIParity.Model(handler);
        await ReadModelAsync(model, OpenAIParity.Hello());
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
        Assert.Equal("no-cache", model.LastResponseHeaders["cache-control"]);
        Assert.Equal("keep-alive", model.LastResponseHeaders["connection"]);
        Assert.StartsWith("text/event-stream", model.LastResponseHeaders["Content-Type"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should pass the messages and the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_messages_and_the_model()
    {
        var handler = await SendAsync(OpenAIParity.Hello());
        OpenAIParity.AssertBody(handler, StreamBody("gpt-3.5-turbo"));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_stream_headers()
    {
        var options = new OpenAIOptions { ApiKey = "test-api-key", Organization = "test-organization", Project = "test-project" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var call = OpenAIParity.Hello();
        call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        var handler = await SendAsync(call, provider: options);
        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.StartsWith("application/json", handler.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.Equal("test-organization", handler.Headers["OpenAI-Organization"]);
        Assert.Equal("test-project", handler.Headers["OpenAI-Project"]);
        Assert.Contains("ai-sdk/openai/0.0.0-test", handler.Headers["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should return cached tokens in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_cached_tokens()
    {
        var handler = new RecordingHandler();
        var parts = await StreamOnAsync(handler, TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":0,"delta":{},"finish_reason":"stop","logprobs":null}""",
            usageChoice: """{"prompt_tokens":2000,"completion_tokens":20,"total_tokens":2020,"prompt_tokens_details":{"cached_tokens":1152,"cache_write_tokens":256}}"""));
        OpenAIParity.AssertBody(handler, StreamBody("gpt-3.5-turbo"));
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        OpenAIParity.AssertUsage(finish.Usage, 2000, 20, 0, 1152, 256, 2020);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should return accepted_prediction_tokens and rejected_prediction_tokens in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_prediction_tokens()
    {
        var parts = await StreamAsync(TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":0,"delta":{},"finish_reason":"stop","logprobs":null}""",
            usageChoice: """{"prompt_tokens":15,"completion_tokens":20,"total_tokens":35,"completion_tokens_details":{"accepted_prediction_tokens":123,"rejected_prediction_tokens":456}}"""));
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        OpenAIParity.AssertJson(finish.ProviderMetadata!.Value.GetProperty("openai").GetRawText(), """{"acceptedPredictionTokens":123,"rejectedPredictionTokens":456}""");
        OpenAIParity.AssertUsage(finish.Usage, 15, 20, 0, 0, null, 35);
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should send store extension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_store()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"store":true}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, StreamBody("gpt-3.5-turbo", """{"store":true}"""));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should send metadata extension values", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_metadata()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"metadata":{"custom":"value"}}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, StreamBody("gpt-3.5-turbo", """{"metadata":{"custom":"value"}}"""));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should send serviceTier flex processing setting in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_flex_while_streaming()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"serviceTier":"flex"}""");
        var handler = await SendAsync(options, "o4-mini");
        OpenAIParity.AssertBody(handler, StreamBody("o4-mini", """{"service_tier":"flex"}"""));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should send serviceTier priority processing setting in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_priority_while_streaming()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"serviceTier":"priority"}""");
        var handler = await SendAsync(options, "gpt-4o-mini");
        OpenAIParity.AssertBody(handler, StreamBody("gpt-4o-mini", """{"service_tier":"priority"}"""));
    }

    [Fact]
    [UpstreamTest(File + "::doStream::should set .modelId for model-router request", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_routed_model_id()
    {
        var parts = await StreamAsync(OpenAIParity.Sse(
            """{"choices":[],"created":0,"id":"","model":"","object":""}""",
            """{"choices":[{"delta":{"content":"","role":"assistant"},"finish_reason":null,"index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[{"delta":{"content":"Capital"},"finish_reason":null,"index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[{"delta":{"content":" of"},"finish_reason":null,"index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[{"delta":{"content":" Denmark"},"finish_reason":null,"index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[{"delta":{"content":"."},"finish_reason":null,"index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[{"delta":{},"finish_reason":"stop","index":0}],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk"}""",
            """{"choices":[],"created":1762317021,"id":"chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt","model":"gpt-5-nano-2025-08-07","object":"chat.completion.chunk","usage":{"completion_tokens":78,"completion_tokens_details":{"accepted_prediction_tokens":0,"reasoning_tokens":64,"rejected_prediction_tokens":0},"prompt_tokens":15,"prompt_tokens_details":{"cached_tokens":0},"total_tokens":93}}"""),
            "test-azure-model-router");
        var metadata = Assert.Single(parts.OfType<ResponseMetadataStreamPart>());
        Assert.Equal("gpt-5-nano-2025-08-07", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1762317021), metadata.Timestamp);
        Assert.Equal("Capital of Denmark.", string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        OpenAIParity.AssertUsage(finish.Usage, 15, 78, 64, 0, null, 93);
        OpenAIParity.AssertJson(finish.ProviderMetadata!.Value.GetProperty("openai").GetRawText(), """{"acceptedPredictionTokens":0,"rejectedPredictionTokens":0}""");
    }

    [Fact]
    [UpstreamTest(File + "::doStream > reasoning models::should stream text delta", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning_model_text()
    {
        var parts = await StreamAsync(TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":1,"delta":{"content":"Hello, World!"},"finish_reason":null}""",
            """{"index":0,"delta":{},"finish_reason":"stop","logprobs":null}""",
            usageChoice: """{"prompt_tokens":17,"total_tokens":244,"completion_tokens":227}""",
            model: "o4-mini"),
            "o4-mini");
        Assert.Equal(new[] { string.Empty, "Hello, World!" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal("o4-mini", Assert.Single(parts.OfType<ResponseMetadataStreamPart>()).ModelId);
        OpenAIParity.AssertUsage(Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).Usage, 17, 227, 0, 0, null, 244);
    }

    [Fact]
    [UpstreamTest(File + "::doStream > reasoning models::should send reasoning tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_reasoning_tokens_on_usage()
    {
        var parts = await StreamAsync(TextChunks(
            """{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}""",
            """{"index":1,"delta":{"content":"Hello, World!"},"finish_reason":null}""",
            """{"index":0,"delta":{},"finish_reason":"stop","logprobs":null}""",
            usageChoice: """{"prompt_tokens":15,"completion_tokens":20,"total_tokens":35,"completion_tokens_details":{"reasoning_tokens":10}}""",
            model: "o4-mini"),
            "o4-mini");
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        OpenAIParity.AssertUsage(finish.Usage, 15, 20, 10, 0, null, 35);
        Assert.Equal("{}", finish.ProviderMetadata!.Value.GetProperty("openai").GetRawText());
    }

    [Fact]
    [UpstreamTest(File + "::doStream > raw chunks::should include raw chunks when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_raw_chunks()
    {
        var events = RawEvents();
        var options = OpenAIParity.Hello();
        options.IncludeRawChunks = true;
        var parts = await StreamAsync(OpenAIParity.Sse(events), options: options);
        var raw = parts.OfType<RawStreamPart>().Select(part => part.RawJson).ToArray();
        Assert.Equal(5, raw.Length);
        for (var index = 0; index < events.Length; index++)
        {
            OpenAIParity.AssertJson(raw[index], events[index]);
        }

        Assert.Contains("null", raw[0], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::doStream > raw chunks::should not include raw chunks when includeRawChunks is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_raw_chunks()
    {
        var options = OpenAIParity.Hello();
        options.IncludeRawChunks = false;
        var parts = await StreamAsync(OpenAIParity.Sse(RawEvents()), options: options);
        Assert.Empty(parts.OfType<RawStreamPart>());
    }

    private static string[] RawEvents()
    {
        return new[]
        {
            Chunk("""{"index":0,"delta":{"role":"assistant","content":""},"finish_reason":null}"""),
            Chunk("""{"index":1,"delta":{"content":"Hello"},"finish_reason":null}"""),
            Chunk("""{"index":1,"delta":{"content":" World!"},"finish_reason":null}"""),
            Chunk("""{"index":0,"delta":{},"finish_reason":"stop","logprobs":null}"""),
            Chunk("[]", """{"prompt_tokens":17,"total_tokens":244,"completion_tokens":227}""", "fp_3bc1b5746c"),
        };
    }

    private static string StreamBody(string modelId, string? extraObject = null)
    {
        var extra = string.Empty;
        if (extraObject != null)
        {
            var trimmed = extraObject.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal) && trimmed.Length >= 2)
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }

            extra = "," + trimmed;
        }

        return "{\"model\":\"" + modelId + "\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]" + extra + ",\"stream\":true,\"stream_options\":{\"include_usage\":true}}";
    }

    private static string TextChunks(string first, string? second = null, string? third = null, string? fourth = null, string? fifth = null, string? usageChoice = null, string model = "gpt-3.5-turbo-0613")
    {
        var events = new List<string>();
        foreach (var choice in new[] { first, second, third, fourth, fifth })
        {
            if (choice != null)
            {
                events.Add(Chunk("[" + choice + "]", model: model));
            }
        }

        if (usageChoice != null)
        {
            events.Add(Chunk("[]", usageChoice, "fp_3bc1b5746c", model));
        }

        return OpenAIParity.Sse(events.ToArray());
    }

    private static string Chunk(string choice, string? usage = null, string? fingerprint = null, string model = "gpt-3.5-turbo-0613", string id = TextId)
    {
        var fingerprintJson = fingerprint == null ? "null" : "\"" + fingerprint + "\"";
        var usageJson = usage == null ? string.Empty : ",\"usage\":" + usage;
        return "{\"id\":\"" + id + "\",\"object\":\"chat.completion.chunk\",\"created\":1702657020,\"model\":\"" + model + "\",\"system_fingerprint\":" + fingerprintJson + ",\"choices\":" + choice + usageJson + "}";
    }

    private static string ToolStart(string arguments, string id = ToolId, string name = "test-tool")
    {
        return "{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"index\":0,\"id\":\"" + id + "\",\"type\":\"function\",\"function\":{\"name\":\"" + name + "\",\"arguments\":\"" + arguments + "\"}}]},\"finish_reason\":null}";
    }

    private static string ToolArgs(string arguments, string? finish = null)
    {
        var finishJson = finish == null ? "null" : "\"" + finish + "\"";
        return "{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"" + arguments + "\"}}]},\"finish_reason\":" + finishJson + "}";
    }

    private static async Task<List<LanguageModelStreamPart>> StreamTools(string[] choices, string toolName = "test-tool", string? usage = """{"prompt_tokens":53,"completion_tokens":17,"total_tokens":70}""")
    {
        var events = choices.Select(choice => Chunk("[" + choice + "]", model: "gpt-3.5-turbo-0125")).ToList();
        events.Add("""{"id":"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP","object":"chat.completion.chunk","created":1711357598,"model":"gpt-3.5-turbo-0125","choices":[{"index":0,"delta":{},"finish_reason":"tool_calls"}]}""");
        if (usage != null)
        {
            events.Add(Chunk("[]", usage, model: "gpt-3.5-turbo-0125"));
        }

        var options = OpenAIParity.Hello();
        options.Tools = new[] { OpenAIParity.ValueTool(toolName) };
        return await StreamAsync(OpenAIParity.Sse(events.ToArray()), options: options);
    }

    private static async Task<RecordingHandler> SendAsync(LanguageModelCallOptions options, string modelId = "gpt-3.5-turbo", OpenAIOptions? provider = null)
    {
        var handler = new RecordingHandler
        {
            ResponseBody = OpenAIParity.Sse(Chunk("""{"index":0,"delta":{"content":"ok"},"finish_reason":"stop"}""")),
            MediaType = "text/event-stream",
        };
        var model = OpenAIParity.Model(handler, modelId, provider);
        await ReadModelAsync(model, options);
        return handler;
    }

    private static Task<List<LanguageModelStreamPart>> StreamAsync(string sse, string modelId = "gpt-3.5-turbo", LanguageModelCallOptions? options = null)
    {
        var handler = new RecordingHandler { ResponseBody = sse, MediaType = "text/event-stream" };
        return StreamOnAsync(handler, sse, modelId, options);
    }

    private static Task<List<LanguageModelStreamPart>> StreamOnAsync(RecordingHandler handler, string sse, string modelId = "gpt-3.5-turbo", LanguageModelCallOptions? options = null)
    {
        handler.ResponseBody = sse;
        handler.MediaType = "text/event-stream";
        return ReadModelAsync(OpenAIParity.Model(handler, modelId), options ?? OpenAIParity.Hello());
    }

    private static async Task<List<LanguageModelStreamPart>> ReadAsync(string sse)
    {
        return await StreamAsync(sse);
    }

    private static async Task<List<LanguageModelStreamPart>> ReadModelAsync(OpenAIChatLanguageModel model, LanguageModelCallOptions options)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in model.DoStreamAsync(options, CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }
}
