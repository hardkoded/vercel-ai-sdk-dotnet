// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class TextStreamTests
{
    private static readonly string[] Cookies =
    {
        "theme=light; Expires=Wed, 21 Oct 2030 07:28:00 GMT; Path=/",
        "locale=en; Path=/",
    };

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/to-text-stream.test.ts::toTextStream::keeps only text deltas",
        Coverage = UpstreamCoverage.Covered)]
    public async Task To_text_stream_keeps_only_text_deltas()
    {
        var parts = new LanguageModelStreamPart[]
        {
            new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty),
            new TextDeltaStreamPart("t1", "Hello"),
            new ReasoningDeltaStreamPart("r1", "thinking"),
            new TextDeltaStreamPart("t1", ", world!"),
            new ErrorStreamPart("ignored"),
        };

        var text = new List<string>();
        await foreach (var chunk in TextStreams.ToTextStream(AsAsync(parts)))
        {
            text.Add(chunk);
        }

        Assert.Equal(new[] { "Hello", ", world!" }, text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/create-text-stream-response.test.ts::createTextStreamResponse::should create a Response with correct headers and encoded stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Create_text_stream_response_sets_headers_and_encodes_chunks()
    {
        var response = TextStreams.CreateTextStreamResponse(
            AsAsync(new[] { "test-data" }),
            status: 200,
            statusText: "OK",
            headers: new[] { new KeyValuePair<string, string>("Custom-Header", "test") });

        Assert.Equal(200, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.Equal("text/plain; charset=utf-8", Assert.Single(response.GetHeaderValues("content-type")));
        Assert.Equal("test", Assert.Single(response.GetHeaderValues("custom-header")));
        Assert.Equal(new[] { "test-data" }, await ReadTextAsync(response.Body));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/create-text-stream-response.test.ts::createTextStreamResponse::can respond with a stream created by toTextStream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Create_text_stream_response_accepts_to_text_stream()
    {
        var parts = new LanguageModelStreamPart[]
        {
            new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty),
            new TextDeltaStreamPart("t1", "Hello"),
            new TextDeltaStreamPart("t1", ", world!"),
        };
        var response = TextStreams.CreateTextStreamResponse(TextStreams.ToTextStream(AsAsync(parts)));

        Assert.Equal(new[] { "Hello", ", world!" }, await ReadTextAsync(response.Body));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should write to ServerResponse with correct headers and encoded stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipe_writes_status_headers_and_chunks()
    {
        var response = new RecordingTextResponse();
        await TextStreams.PipeTextStreamToResponseAsync(
            response,
            AsAsync(new[] { "test-data" }),
            status: 200,
            statusText: "OK",
            headers: new[] { new KeyValuePair<string, string>("Custom-Header", "test") });

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusMessage);
        Assert.Equal("text/plain; charset=utf-8", Assert.Single(response.GetHeaderValues("content-type")));
        Assert.Equal("test", Assert.Single(response.GetHeaderValues("custom-header")));
        Assert.True(response.Ended);
        Assert.Equal(new[] { "test-data" }, response.DecodedChunks());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should preserve multiple Set-Cookie headers with $name",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipe_preserves_repeated_set_cookie_headers()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, string>("set-cookie", Cookies[0]),
            new KeyValuePair<string, string>("set-cookie", Cookies[1]),
        };
        var response = new RecordingTextResponse();
        await TextStreams.PipeTextStreamToResponseAsync(response, AsAsync(new[] { "test-data" }), headers: pairs);

        Assert.Equal(Cookies, response.GetHeaderValues("set-cookie"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::can pipe a stream created by toTextStream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipe_accepts_to_text_stream()
    {
        var parts = new LanguageModelStreamPart[]
        {
            new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty),
            new TextDeltaStreamPart("t1", "Hello"),
            new TextDeltaStreamPart("t1", ", world!"),
        };
        var response = new RecordingTextResponse();
        await TextStreams.PipeTextStreamToResponseAsync(response, TextStreams.ToTextStream(AsAsync(parts)));

        Assert.Equal(new[] { "Hello", ", world!" }, response.DecodedChunks());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/text-stream/pipe-text-stream-to-response.test.ts::pipeTextStreamToResponse::should reject when reading the stream fails",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Pipe_rejects_when_the_stream_fails()
    {
        var response = new RecordingTextResponse();
        var error = new InvalidOperationException("stream read failed");

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TextStreams.PipeTextStreamToResponseAsync(response, new ThrowingTextStream(error)));

        Assert.Same(error, thrown);
        Assert.True(response.Ended);
    }

    private static async IAsyncEnumerable<T> AsAsync<T>(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            yield return value;
            await Task.Yield();
        }
    }

    private sealed class ThrowingTextStream : IAsyncEnumerable<string>
    {
        private readonly Exception _error;

        public ThrowingTextStream(Exception error)
        {
            _error = error;
        }

        public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(_error);
        }

        private sealed class Enumerator : IAsyncEnumerator<string>
        {
            private readonly Exception _error;

            public Enumerator(Exception error)
            {
                _error = error;
            }

            public string Current => string.Empty;

            public ValueTask<bool> MoveNextAsync()
            {
                throw _error;
            }

            public ValueTask DisposeAsync()
            {
                return default;
            }
        }
    }

    private static async Task<List<string>> ReadTextAsync(IAsyncEnumerable<byte[]> body)
    {
        var text = new List<string>();
        await foreach (var chunk in body)
        {
            text.Add(Encoding.UTF8.GetString(chunk));
        }

        return text;
    }

    private sealed class RecordingTextResponse : ITextStreamServerResponse
    {
        private readonly List<KeyValuePair<string, string>> _headers = new();
        private readonly List<byte[]> _chunks = new();

        public int StatusCode { get; set; }

        public string? StatusMessage { get; set; }

        public bool Ended { get; private set; }

        public void SetHeaders(IReadOnlyList<KeyValuePair<string, string>> headers)
        {
            _headers.Clear();
            _headers.AddRange(headers);
        }

        public bool Write(ReadOnlyMemory<byte> chunk)
        {
            _chunks.Add(chunk.ToArray());
            return true;
        }

        public Task WaitForDrainAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void End()
        {
            Ended = true;
        }

        public IReadOnlyList<string> GetHeaderValues(string name)
        {
            return _headers.Where(header => string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
                .Select(header => header.Value)
                .ToList();
        }

        public IReadOnlyList<string> DecodedChunks()
        {
            return _chunks.Select(chunk => Encoding.UTF8.GetString(chunk)).ToList();
        }
    }
}
