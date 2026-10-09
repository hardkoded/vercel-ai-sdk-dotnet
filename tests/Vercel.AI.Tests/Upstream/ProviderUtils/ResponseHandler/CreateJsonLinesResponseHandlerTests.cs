// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateJsonLinesResponseHandlerTests
{
    private const string Prefix = "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::";
    private const long DefaultMaxLineBytes = 64L * 1024 * 1024;

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [UpstreamTest(
        Prefix + "rejects an invalid maxLineBytes: %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "A C# long has no 1.5, NaN, Infinity, or beyond-safe-integer values, so only 0 and -1 run.")]
    public async Task Rejects_an_invalid_maxLineBytes(long maxLineBytes)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        var error = await Assert.ThrowsAsync<Vercel.AI.Operations.InvalidArgumentException>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: maxLineBytes))
            {
            }
        });
        Assert.Contains("maxLineBytes must be a positive safe integer.", error.Message);
    }

    [Theory]
    [InlineData(3L)]
    [InlineData(4L)]
    [UpstreamTest(Prefix + "applies a custom byte limit of %s to each UTF-8 row", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_a_custom_byte_limit_to_each_UTF8_row(long maxLineBytes)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("\"é\"\n\"é\"\n"));
        var lines = new List<string?>();
        var read = JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: maxLineBytes);
        if (maxLineBytes == 3)
        {
            await Assert.ThrowsAsync<DownloadError>(async () =>
            {
                await foreach (var line in read)
                {
                    lines.Add(line.GetString());
                }
            });
        }
        else
        {
            await foreach (var line in read)
            {
                lines.Add(line.GetString());
            }

            Assert.Equal(new[] { "é", "é" }, lines);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "allows raising the limit above the default", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_raising_the_limit_above_the_default()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: DefaultMaxLineBytes + 1))
        {
            values.Add(value);
        }

        Assert.Single(values);
        Assert.Equal(JsonValueKind.Object, values[0].ValueKind);
    }

    [Theory]
    [InlineData("ASCII")]
    [InlineData("UTF-8")]
    [UpstreamTest(Prefix + "rejects an oversized %s line across chunks and cancels the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_oversized_line_across_chunks_and_cancels_the_body(string encoding)
    {
        const long maxLineBytes = 64;
        var chunk = new byte[16];
        if (encoding == "ASCII")
        {
            for (var i = 0; i < chunk.Length; i++)
            {
                chunk[i] = 65;
            }
        }
        else
        {
            // Each character uses two UTF-8 bytes, so the character count stays below the limit.
            for (var i = 0; i < chunk.Length; i += 2)
            {
                chunk[i] = 0xc3;
                chunk[i + 1] = 0xb1;
            }
        }

        var cancelled = false;
        using var stream = new ChunkedStream(chunk, 6);
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(
                stream,
                onCancel: () =>
                {
                    cancelled = true;
                    throw new InvalidOperationException("Cancellation failed");
                },
                maxLineBytes: maxLineBytes,
                url: "test-url"))
            {
            }
        });

        Assert.Equal("JSON Lines response exceeded maximum line size of " + maxLineBytes + " bytes.", error.Message);
        Assert.Equal("test-url", error.Url);
        Assert.True(cancelled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Prefix + "rejects an oversized line in one chunk (trailing newline: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_oversized_line_in_one_chunk(bool trailingNewline)
    {
        var bytes = new byte[DefaultMaxLineBytes + 1 + (trailingNewline ? 1 : 0)];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = 32;
        }

        bytes[DefaultMaxLineBytes - 1] = (byte)'{';
        bytes[DefaultMaxLineBytes] = (byte)'}';
        if (trailingNewline)
        {
            bytes[bytes.Length - 1] = 10;
        }

        using var stream = new MemoryStream(bytes);
        await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, url: "test-url"))
            {
            }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Prefix + "accepts a line at the byte limit (trailing newline: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_line_at_the_byte_limit(bool trailingNewline)
    {
        const long maxLineBytes = 64;
        var bytes = new byte[maxLineBytes + (trailingNewline ? 1 : 0)];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = 32;
        }

        bytes[maxLineBytes - 2] = (byte)'{';
        bytes[maxLineBytes - 1] = (byte)'}';
        if (trailingNewline)
        {
            bytes[bytes.Length - 1] = 10;
        }

        using var stream = new MemoryStream(bytes);
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: maxLineBytes))
        {
            values.Add(value);
        }

        Assert.Single(values);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts a chunk larger than the limit when each line is below the limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_chunk_larger_than_the_limit_when_each_line_is_below_the_limit()
    {
        const long maxLineBytes = 64;
        const int lineBytes = 8;
        var bytes = new byte[9 * lineBytes];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = 32;
        }

        for (var i = 1; i <= 9; i++)
        {
            bytes[i * lineBytes - 3] = (byte)'{';
            bytes[i * lineBytes - 2] = (byte)'}';
            bytes[i * lineBytes - 1] = 10;
        }

        using var stream = new MemoryStream(bytes);
        var count = 0;
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: maxLineBytes))
        {
            Assert.Equal(JsonValueKind.Object, value.ValueKind);
            count++;
        }

        Assert.Equal(9, count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "parses JSON lines across byte boundaries",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_cross_byte_boundaries_and_keep_headers()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"id\":\"first\",\"text\":\"café\"}\r\n\n{\"id\":\"second\",\"text\":\"done\"}");
        using var stream = new MemoryStream();
        stream.Write(bytes, 0, 24);
        stream.Write(bytes, 24, 3);
        stream.Write(bytes, 27, bytes.Length - 27);
        stream.Position = 0;
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream))
        {
            values.Add(value);
        }

        Assert.Equal(2, values.Count);
        Assert.Equal("first", values[0].GetProperty("id").GetString());
        Assert.Equal("café", values[0].GetProperty("text").GetString());
        Assert.Equal("done", values[1].GetProperty("text").GetString());

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        response.Headers.TryAddWithoutValidation("x-test", "value");
        Assert.Equal("value", JsonStreams.ExtractResponseHeaders(response)["x-test"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors when a line is invalid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_reject_invalid_json()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"id\":\"first\"}\n{invalid}\n"));
        var enumerator = JsonStreams.ReadJsonLinesAsync(stream).GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("first", enumerator.Current.GetProperty("id").GetString());
        await Assert.ThrowsAnyAsync<Exception>(async () => await enumerator.MoveNextAsync());
        await enumerator.DisposeAsync();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "cancels the response body when iteration stops early",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_cancel_when_iteration_stops()
    {
        var cancelled = false;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"id\":\"first\"}\n{\"id\":\"second\"}\n"));
        await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, onCancel: () => cancelled = true))
        {
            break;
        }

        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws EmptyResponseBodyError when the response body is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_reject_a_missing_body()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(null))
            {
            }
        });
        Assert.Contains("Empty response body", error.Message);
    }

    private sealed class ChunkedStream : Stream
    {
        private readonly byte[] _chunk;
        private int _remaining;

        public ChunkedStream(byte[] chunk, int count)
        {
            _chunk = chunk;
            _remaining = count;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining == 0)
            {
                return 0;
            }

            _remaining--;
            Array.Copy(_chunk, 0, buffer, offset, _chunk.Length);
            return _chunk.Length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
