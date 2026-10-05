// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Util;
using UtilTextPart = Vercel.AI.Util.TextStreamPart;

namespace Vercel.AI.Tests;

public sealed class TextAndServerResponseTests
{
    [UpstreamTest("packages/ai/src/text-stream/to-text-stream.test.ts::toTextStream::keeps only text deltas", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Keeps_only_text_deltas()
    {
        var stream = TextStreams.ToTextStream(ReadableStream<UtilTextPart>.FromArray(new[]
        {
            new UtilTextPart("start"),
            new UtilTextPart("text-start", "t1"),
            new UtilTextPart("text-delta", "t1", "Hello"),
            new UtilTextPart("text-delta", "t1", ", world!"),
            new UtilTextPart("text-end", "t1"),
        }));
        Assert.Equal(new[] { "Hello", ", world!" }, await stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/text-stream/create-text-stream-response.test.ts::createTextStreamResponse::should create a Response with correct headers and encoded stream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Creates_a_text_response()
    {
        var headers = new HeaderCollection();
        headers.Add("Custom-Header", "test");
        var response = TextStreams.CreateTextStreamResponse(ReadableStream<string>.FromArray(new[] { "test-data" }), 200, "OK", headers);
        Assert.Equal(200, response.Status);
        Assert.Equal("OK", response.StatusText!);
        Assert.Equal("text/plain; charset=utf-8", response.Headers.Get("Content-Type")!);
        Assert.Equal("test", response.Headers.Get("Custom-Header")!);
        Assert.Equal(new[] { "test-data" }, await Decode(response.Body));
    }

    [UpstreamTest("packages/ai/src/text-stream/create-text-stream-response.test.ts::createTextStreamResponse::can respond with a stream created by toTextStream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Responds_with_a_text_stream()
    {
        var text = TextStreams.ToTextStream(ReadableStream<UtilTextPart>.FromArray(new[]
        {
            new UtilTextPart("start"),
            new UtilTextPart("text-delta", "t1", "Hello"),
            new UtilTextPart("text-delta", "t1", ", world!"),
            new UtilTextPart("text-end", "t1"),
        }));
        var response = TextStreams.CreateTextStreamResponse(text);
        Assert.Equal(new[] { "Hello", ", world!" }, await Decode(response.Body));
    }

    [UpstreamTest("packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should write to ServerResponse with correct headers and encoded stream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Pipes_text_with_headers()
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("Custom-Header", "test");
        await TextStreams.PipeTextStreamToResponseAsync(response, ReadableStream<string>.FromArray(new[] { "test-data" }), 200, "OK", headers);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusMessage);
        Assert.Equal("text/plain; charset=utf-8", response.Headers["content-type"]);
        Assert.Equal("test", response.Headers["custom-header"]);
        Assert.Equal(new[] { "test-data" }, response.GetDecodedChunks());
    }

    [UpstreamTest("packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should preserve multiple Set-Cookie headers with $name", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Preserves_multiple_set_cookie_headers()
    {
        var cookies = new[]
        {
            "theme=light; Expires=Wed, 21 Oct 2030 07:28:00 GMT; Path=/",
            "locale=en; Path=/",
        };
        await AssertCookies(cookies);
        await AssertCookies(cookies);
    }

    [UpstreamTest("packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::can pipe a stream created by toTextStream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Pipes_a_text_stream()
    {
        var response = new ServerResponse();
        var text = TextStreams.ToTextStream(ReadableStream<UtilTextPart>.FromArray(new[]
        {
            new UtilTextPart("start"),
            new UtilTextPart("text-delta", "t1", "Hello"),
            new UtilTextPart("text-delta", "t1", ", world!"),
            new UtilTextPart("text-end", "t1"),
        }));
        await TextStreams.PipeTextStreamToResponseAsync(response, text);
        Assert.Equal(new[] { "Hello", ", world!" }, response.GetDecodedChunks());
    }

    [UpstreamTest("packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should reject when reading the stream fails", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Pipe_rejects_when_the_stream_fails()
    {
        var response = new ServerResponse();
        var error = new InvalidOperationException("stream read failed");
        var stream = new ReadableStream<string>(pull: delegate { return Task.FromException(error); });
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(delegate
        {
            return TextStreams.PipeTextStreamToResponseAsync(response, stream);
        });
        Assert.Same(error, thrown);
        Assert.True(response.Ended);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse::should write data to ServerResponse", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Writes_chunks_to_the_response()
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("Content-Type", "text/plain");
        var stream = ReadableStream<byte[]>.FromArray(new[] { Encoding.UTF8.GetBytes("chunk1"), Encoding.UTF8.GetBytes("chunk2") });
        await ServerResponseWriter.WriteAsync(response, stream, 200, "OK", headers);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusMessage);
        Assert.Equal(2, response.WrittenChunks.Count);
        Assert.True(response.Ended);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse::should reject when reading the stream fails", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Write_rejects_when_the_stream_fails()
    {
        var response = new ServerResponse();
        var error = new InvalidOperationException("stream read failed");
        var stream = new ReadableStream<byte[]>(pull: delegate { return Task.FromException(error); });
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(delegate
        {
            return ServerResponseWriter.WriteAsync(response, stream);
        });
        Assert.Same(error, thrown);
        Assert.True(response.Ended);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse > backpressure handling::should respect backpressure and wait for drain event", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Waits_for_drain_before_the_next_write()
    {
        var response = new ServerResponse { EnableBackpressure = true };
        var flushCounts = new List<int>();
        response.Flush = delegate (ServerResponse current)
        {
            Assert.Same(response, current);
            flushCounts.Add(current.WrittenChunks.Count);
        };
        Action<byte[]?>? enqueue = null;
        var stream = new ReadableStream<byte[]>(delegate (ReadableStreamController<byte[]> controller)
        {
            controller.Enqueue(Encoding.UTF8.GetBytes("chunk1"));
            enqueue = delegate (byte[]? value)
            {
                if (value == null)
                {
                    controller.Close();
                }
                else
                {
                    controller.Enqueue(value);
                }
            };
        });
        var write = ServerResponseWriter.WriteAsync(response, stream, 200);
        await WaitUntil(delegate { return response.WriteCallCount == 1; });
        Assert.Equal(new[] { 1 }, flushCounts);
        Assert.NotNull(enqueue);
        Action<byte[]?> push = enqueue!;
        push(Encoding.UTF8.GetBytes("chunk2"));
        await WaitUntil(delegate { return response.WriteCallCount == 2; });
        Assert.Equal(2, response.WrittenChunks.Count);
        Assert.Equal(new[] { 1, 2 }, flushCounts);
        push(Encoding.UTF8.GetBytes("chunk3"));
        await Task.Yield();
        Assert.Equal(2, response.WriteCallCount);
        response.SimulateDrain();
        await WaitUntil(delegate { return response.WriteCallCount == 3; });
        Assert.Equal(new[] { 1, 2, 3 }, flushCounts);
        push(null);
        await write;
        Assert.True(response.Ended);
        Assert.True(response.DrainCount >= 1);
        Assert.Equal(3, response.WrittenChunks.Count);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse::should set headers correctly when statusText is undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Sets_headers_without_status_text()
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("x-example-header", "example-value");
        headers.Add("x-example-chat-title", "My Conversation");
        await ServerResponseWriter.WriteAsync(response, Bytes("test data"), 200, null, headers);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(string.Empty, response.StatusMessage);
        Assert.Equal("example-value", response.Headers["x-example-header"]);
        Assert.Equal("My Conversation", response.Headers["x-example-chat-title"]);
        Assert.True(response.Ended);
        Assert.Single(response.WrittenChunks);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse::should set headers correctly when statusText is provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Sets_headers_with_status_text()
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("x-example-header", "example-value");
        headers.Add("x-example-chat-title", "New Chat Session");
        await ServerResponseWriter.WriteAsync(response, Bytes("test data"), 201, "Created", headers);
        Assert.Equal(201, response.StatusCode);
        Assert.Equal("Created", response.StatusMessage);
        Assert.Equal("example-value", response.Headers["x-example-header"]);
        Assert.Equal("New Chat Session", response.Headers["x-example-chat-title"]);
        Assert.True(response.Ended);
        Assert.Single(response.WrittenChunks);
    }

    [UpstreamTest("packages/ai/src/util/write-to-server-response.test.ts::writeToServerResponse::should set headers correctly when statusText is not set and status is not set", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Defaults_the_status_when_writing()
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("x-example-header", "example-value");
        headers.Add("x-example-message", "Hello World");
        await ServerResponseWriter.WriteAsync(response, Bytes("test data"), headers: headers);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("example-value", response.Headers["x-example-header"]);
        Assert.Equal("Hello World", response.Headers["x-example-message"]);
        Assert.True(response.Ended);
        Assert.Single(response.WrittenChunks);
    }

    private static async Task AssertCookies(string[] cookies)
    {
        var response = new ServerResponse();
        var headers = new HeaderCollection();
        headers.Add("set-cookie", cookies[0]);
        headers.Add("set-cookie", cookies[1]);
        await TextStreams.PipeTextStreamToResponseAsync(response, ReadableStream<string>.FromArray(new[] { "test-data" }), headers: headers);
        Assert.Equal(cookies, (List<string>)response.Headers["set-cookie"]);
    }

    private static async Task<List<string>> Decode(ReadableStream<byte[]> body)
    {
        var chunks = await body.ToArrayAsync();
        var text = new List<string>();
        for (var i = 0; i < chunks.Count; i++)
        {
            text.Add(Encoding.UTF8.GetString(chunks[i]));
        }

        return text;
    }

    private static ReadableStream<byte[]> Bytes(string text)
    {
        return ReadableStream<byte[]>.FromArray(new[] { Encoding.UTF8.GetBytes(text) });
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 100; i++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}
