// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class UiMessageResponseTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/create-ui-message-stream-response.test.ts::createUIMessageStreamResponse::should create a Response with correct headers and encoded stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_response_with_protocol_headers_and_an_encoded_stream()
    {
        var context = await Execute(
            new[] { Chunk("type", "text-delta", "id", "1", "delta", "test-data") },
            new KeyValuePair<string, string>("Custom-Header", "test"));

        Assert.Equal(200, context.Response.StatusCode);
        AssertProtocolHeaders(context);
        Assert.Equal("test", context.Response.Headers["Custom-Header"].ToString());
        Assert.Equal(
            "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"test-data\"}\n\ndata: [DONE]\n\n",
            Body(context));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/create-ui-message-stream-response.test.ts::createUIMessageStreamResponse::should handle errors in the stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_an_error_chunk_and_done()
    {
        var context = await Execute(new[] { Chunk("type", "error", "errorText", "Custom error message") });

        Assert.Equal(
            "data: {\"type\":\"error\",\"errorText\":\"Custom error message\"}\n\ndata: [DONE]\n\n",
            Body(context));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/pipe-ui-message-stream-to-response.test.ts::pipeUIMessageStreamToResponse::should write to ServerResponse with correct headers and encoded stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipes_chunks_with_protocol_headers()
    {
        var context = await Execute(
            new[]
            {
                Chunk("type", "text-start", "id", "1"),
                Chunk("type", "text-delta", "id", "1", "delta", "test-data"),
                Chunk("type", "text-end", "id", "1"),
            },
            new KeyValuePair<string, string>("Custom-Header", "test"));

        Assert.Equal(200, context.Response.StatusCode);
        AssertProtocolHeaders(context);
        Assert.Equal("test", context.Response.Headers["Custom-Header"].ToString());
        Assert.Equal(
            "data: {\"type\":\"text-start\",\"id\":\"1\"}\n\n"
            + "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"test-data\"}\n\n"
            + "data: {\"type\":\"text-end\",\"id\":\"1\"}\n\n"
            + "data: [DONE]\n\n",
            Body(context));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/pipe-ui-message-stream-to-response.test.ts::pipeUIMessageStreamToResponse::should preserve multiple Set-Cookie headers with $name",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_multiple_set_cookie_headers()
    {
        var cookies = new[]
        {
            "theme=light; Expires=Wed, 21 Oct 2030 07:28:00 GMT; Path=/",
            "locale=en; Path=/",
        };
        var context = await Execute(
            new[]
            {
                Chunk("type", "start", "messageId", "message-id"),
                Chunk("type", "finish"),
            },
            new KeyValuePair<string, string>("Set-Cookie", cookies[0]),
            new KeyValuePair<string, string>("Set-Cookie", cookies[1]));

        var setCookie = context.Response.Headers["Set-Cookie"];
        Assert.Equal(2, setCookie.Count);
        Assert.Equal(cookies[0], setCookie[0]);
        Assert.Equal(cookies[1], setCookie[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/pipe-ui-message-stream-to-response.test.ts::pipeUIMessageStreamToResponse::should handle errors in the stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipes_an_error_chunk_and_done()
    {
        var context = await Execute(new[] { Chunk("type", "error", "errorText", "Custom error message") });

        Assert.Equal(
            "data: {\"type\":\"error\",\"errorText\":\"Custom error message\"}\n\ndata: [DONE]\n\n",
            Body(context));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps error parts through onError",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Routes_error_parts_through_onError()
    {
        string? seen = null;
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[] { new ErrorStreamPart("boom") },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "hi" });
        using var buffer = new MemoryStream();

        var error = await Assert.ThrowsAsync<AiSdkException>(() => UIMessageStreamResult.WriteAsync(
            stream,
            buffer,
            onError: message =>
            {
                seen = message;
                return "handled error";
            }));

        Assert.Equal("boom", error.Message);
        Assert.Equal("boom", seen);
        Assert.Contains("\"type\":\"error\"", Body(buffer), StringComparison.Ordinal);
        Assert.Contains("\"errorText\":\"handled error\"", Body(buffer), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps tool input streaming parts",
        Coverage = UpstreamCoverage.Partial,
        Note = "No dynamic tools, tool metadata, or tool titles in .NET. The dynamic flag comes from the provider part.")]
    public async Task Maps_tool_input_streaming_parts()
    {
        using var metadata = JsonDocument.Parse("{\"testProvider\":{\"signature\":\"sig-1\"}}");
        var frames = Frames(await Write(new LanguageModelStreamPart[]
        {
            new ToolInputStartStreamPart("call-1", "dynamicTool", metadata.RootElement.Clone(), providerExecuted: true, dynamic: true),
            new ToolInputStartStreamPart("call-2", "providerTool", dynamic: true),
            new ToolInputDeltaStreamPart("call-1", "{\"value\""),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
        }));

        AssertFrame(
            "{\"type\":\"tool-input-start\",\"toolCallId\":\"call-1\",\"toolName\":\"dynamicTool\",\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"dynamic\":true}",
            frames[2]);
        AssertFrame("{\"type\":\"tool-input-start\",\"toolCallId\":\"call-2\",\"toolName\":\"providerTool\",\"dynamic\":true}", frames[3]);
        AssertFrame("{\"type\":\"tool-input-delta\",\"toolCallId\":\"call-1\",\"inputTextDelta\":\"{\\\"value\\\"\"}", frames[4]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::returns undefined for parts that do not produce UI message chunks",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Writes_no_chunk_for_tool_input_end_or_raw_parts()
    {
        var frames = Frames(await Write(new LanguageModelStreamPart[]
        {
            new ToolInputEndStreamPart("call-1"),
            new RawStreamPart("{\"provider\":\"raw\"}"),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
        }));

        Assert.Equal(new[] { "start", "start-step", "finish-step", "finish", null }, frames.Select(frame => (string?)frame?["type"]));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.toUIMessageStream::should send tool call, tool call stream start, tool call deltas, and tool result stream parts",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_tool_input_stream_and_tool_result_chunks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ToolInputStartStreamPart("call-1", "tool1"),
                new ToolInputDeltaStreamPart("call-1", "{ \"value\":"),
                new ToolInputDeltaStreamPart("call-1", " \"value\" }"),
                new ToolInputEndStreamPart("call-1"),
                new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[]
            {
                Tool.Function(
                    "tool1",
                    null,
                    "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}",
                    (input, _) => Task.FromResult("\"" + input.GetProperty("value").GetString() + "-result\"")),
            },
        });
        using var buffer = new MemoryStream();
        await UIMessageStreamResult.WriteAsync(stream, buffer);
        var frames = Frames(Body(buffer));

        Assert.Equal(10, frames.Count);
        Assert.Equal("start", (string?)frames[0]?["type"]);
        AssertFrame("{\"type\":\"start-step\"}", frames[1]);
        AssertFrame("{\"type\":\"tool-input-start\",\"toolCallId\":\"call-1\",\"toolName\":\"tool1\"}", frames[2]);
        AssertFrame("{\"type\":\"tool-input-delta\",\"toolCallId\":\"call-1\",\"inputTextDelta\":\"{ \\\"value\\\":\"}", frames[3]);
        AssertFrame("{\"type\":\"tool-input-delta\",\"toolCallId\":\"call-1\",\"inputTextDelta\":\" \\\"value\\\" }\"}", frames[4]);
        AssertFrame("{\"type\":\"tool-input-available\",\"toolCallId\":\"call-1\",\"toolName\":\"tool1\",\"input\":{\"value\":\"value\"}}", frames[5]);
        AssertFrame("{\"type\":\"tool-output-available\",\"toolCallId\":\"call-1\",\"output\":\"value-result\"}", frames[6]);
        AssertFrame("{\"type\":\"finish-step\"}", frames[7]);
        AssertFrame("{\"type\":\"finish\",\"finishReason\":\"stop\"}", frames[8]);
        Assert.Null(frames[9]);
    }

    [Fact]
    public async Task Writes_protocol_finish_reasons_metadata_and_null_tool_output()
    {
        using var metadataDocument = JsonDocument.Parse("{\"testProvider\":{\"signature\":\"sig-1\"}}");
        var metadata = metadataDocument.RootElement.Clone();
        var toolCalls = await Write(new LanguageModelStreamPart[]
        {
            new ToolCallStreamPart("call_1", "lookup", "{\"q\":1}", metadata),
            new SourceStreamPart("src_1", "https://example.test/doc", null, metadata),
            new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(1, 1, 2), "tool-calls"),
        }, "null");
        Assert.Contains("\"finishReason\":\"tool-calls\"", toolCalls, StringComparison.Ordinal);
        Assert.DoesNotContain("\"finishReason\":\"toolcalls\"", toolCalls, StringComparison.Ordinal);
        Assert.Contains("\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}", toolCalls, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"source-url\"", toolCalls, StringComparison.Ordinal);
        Assert.DoesNotContain("\"title\"", toolCalls, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"tool-output-available\"", toolCalls, StringComparison.Ordinal);
        Assert.Contains("\"output\":null", toolCalls, StringComparison.Ordinal);

        var filtered = await Write(new LanguageModelStreamPart[]
        {
            new FinishStreamPart(FinishReason.ContentFilter, new LanguageModelUsage(1, 1, 2), "content-filter"),
        });
        Assert.Contains("\"type\":\"start-step\"", filtered, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"finish-step\"", filtered, StringComparison.Ordinal);
        Assert.Contains("\"finishReason\":\"content-filter\"", filtered, StringComparison.Ordinal);
        Assert.DoesNotContain("\"finishReason\":\"contentfilter\"", filtered, StringComparison.Ordinal);

        var reasoning = await Write(new LanguageModelStreamPart[]
        {
            new ReasoningDeltaStreamPart("r1", "thinking"),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
        });
        Assert.Contains("\"type\":\"reasoning-start\"", reasoning, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"reasoning-delta\"", reasoning, StringComparison.Ordinal);
        Assert.Contains("\"delta\":\"thinking\"", reasoning, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"reasoning-end\"", reasoning, StringComparison.Ordinal);

        var failed = await Write(
            new LanguageModelStreamPart[]
            {
                new ToolCallStreamPart("call_2", "lookup", "{}"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(1, 1, 2), "tool-calls"),
            },
            output: null,
            throwOnExecute: true);
        Assert.Contains("\"type\":\"tool-output-error\"", failed, StringComparison.Ordinal);
        Assert.Contains("\"toolCallId\":\"call_2\"", failed, StringComparison.Ordinal);
        Assert.Contains("tool failed", failed, StringComparison.Ordinal);
    }

    private static async Task<string> Write(
        LanguageModelStreamPart[] parts,
        string? output = "{\"n\":1}",
        bool throwOnExecute = false)
    {
        var model = new TestLanguageModel { StreamParts = parts };
        var stream = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "hi",
            Tools = new[]
            {
                Tool.Function(
                    "lookup",
                    "find",
                    "{\"type\":\"object\"}",
                    (_, _) => throwOnExecute
                        ? Task.FromException<string>(new InvalidOperationException("tool failed"))
                        : Task.FromResult(output ?? "null")),
            },
        });
        using var buffer = new MemoryStream();
        await UIMessageStreamResult.WriteAsync(stream, buffer);
        return Body(buffer);
    }

    private static async Task<HttpContext> Execute(JsonObject[] chunks, params KeyValuePair<string, string>[] headers)
    {
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;
        await new UIMessageChunkStreamResult(chunks, headers: headers).ExecuteAsync(context);
        return context;
    }

    private static JsonObject Chunk(params string[] pairs)
    {
        var chunk = new JsonObject();
        for (var i = 0; i < pairs.Length; i += 2)
        {
            chunk[pairs[i]] = pairs[i + 1];
        }

        return chunk;
    }

    private static void AssertProtocolHeaders(HttpContext context)
    {
        Assert.Equal("text/event-stream", context.Response.ContentType);
        Assert.Equal("no-cache", context.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("keep-alive", context.Response.Headers["Connection"].ToString());
        Assert.Equal("no", context.Response.Headers["x-accel-buffering"].ToString());
        Assert.Equal("v1", context.Response.Headers["x-vercel-ai-ui-message-stream"].ToString());
    }

    private static string Body(HttpContext context)
    {
        return Body((MemoryStream)context.Response.Body);
    }

    private static string Body(MemoryStream buffer)
    {
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static List<JsonNode?> Frames(string body)
    {
        return body
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(frame => frame == "data: [DONE]" ? null : JsonNode.Parse(frame.Substring("data: ".Length)))
            .ToList();
    }

    private static void AssertFrame(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
