// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class EventStreamTests
{
    [Fact]
    public void Parser_joins_data_fields_and_keeps_one_leading_space()
    {
        var events = Feed("event: ping\ndata:  hello\ndata: world\nid: abc\n\n");
        var message = Assert.Single(events);
        Assert.Equal(" hello\nworld", message.Data);
        Assert.Equal("ping", message.Event);
        Assert.Equal("abc", message.Id);
    }

    [Fact]
    public void Parser_handles_crlf_comments_retry_and_partial_chunks()
    {
        var parser = new EventSourceParser();
        var events = new List<EventSourceMessage>();
        var retries = new List<int>();
        var comments = new List<string>();
        parser.Feed("retry: 15\r", events.Add, retries.Add, comments.Add);
        parser.Feed("\n: note\r\ndata: hi\r\n\r\n", events.Add, retries.Add, comments.Add);
        Assert.Equal(new[] { 15 }, retries);
        Assert.Equal(new[] { "note" }, comments);
        Assert.Equal("hi", Assert.Single(events).Data);
    }

    [Fact]
    public void Parser_reports_invalid_retry_and_unknown_fields_and_strips_a_bom()
    {
        var errors = new List<string>();
        var events = Feed("\uFEFFdata: [DONE]\n\nfoo: bar\nretry: no\n", errors);
        Assert.Equal("[DONE]", Assert.Single(events).Data);
        Assert.Contains(errors, error => error.Contains("Unknown field"));
        Assert.Contains(errors, error => error.Contains("Invalid `retry` value"));
    }

    [Fact]
    public async Task Json_events_skip_done_and_parse_payloads()
    {
        var bytes = Encoding.UTF8.GetBytes("data: {\"ok\":true}\n\ndata: [DONE]\n\n");
        using var stream = new MemoryStream(bytes);
        var results = new List<JsonParseResult>();
        await foreach (var result in JsonStreams.ReadJsonEventsAsync(stream))
        {
            results.Add(result);
        }

        var parsed = Assert.Single(results);
        Assert.True(parsed.Success);
        Assert.True(parsed.Value!.Value.GetProperty("ok").GetBoolean());
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::parses JSON lines across byte boundaries",
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
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::errors when a line is invalid JSON",
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
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::cancels the response body when iteration stops early",
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
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::throws EmptyResponseBodyError when the response body is null",
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::rejects an invalid maxLineBytes: %s",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_invalid_maxLineBytes(int maxLineBytes)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}\n"));
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: maxLineBytes))
            {
            }
        });
        Assert.Contains("maxLineBytes must be a positive safe integer.", error.Message);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::applies a custom byte limit of %s to each UTF-8 row",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_a_custom_byte_limit_to_each_UTF8_row(int limit)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("\"é\"\n\"é\"\n"));
        var values = new List<string?>();
        var read = async () =>
        {
            await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: limit))
            {
                values.Add(value.GetString());
            }
        };
        if (limit == 3)
        {
            await Assert.ThrowsAsync<DownloadError>(read);
        }
        else
        {
            await read();
            Assert.Equal(new[] { "é", "é" }, values);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::allows raising the limit above the default",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_raising_the_limit_above_the_default()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: JsonStreams.DefaultMaxLineBytes + 1))
        {
            values.Add(value);
        }

        Assert.Single(values);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::rejects an oversized %s line across chunks and cancels the body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_oversized_line_across_chunks_and_cancels_the_body(bool utf8)
    {
        var chunks = Enumerable.Range(0, 6)
            .Select(_ => utf8
                ? Enumerable.Repeat(new byte[] { 0xC3, 0xB1 }, 8).SelectMany(b => b).ToArray()
                : Enumerable.Repeat((byte)0x41, 16).ToArray())
            .ToArray();
        foreach (var throwOnCancel in new[] { false, true })
        {
            var cancelled = false;
            using var stream = new ChunkedStream(chunks);
            var error = await Assert.ThrowsAsync<DownloadError>(async () =>
            {
                await foreach (var _ in JsonStreams.ReadJsonLinesAsync(
                    stream,
                    onCancel: () =>
                    {
                        cancelled = true;
                        if (throwOnCancel)
                        {
                            throw new InvalidOperationException("cancel failed");
                        }
                    },
                    maxLineBytes: 64,
                    url: "test-url"))
                {
                }
            });
            Assert.Contains("maximum line size of 64 bytes", error.Message);
            Assert.Equal("test-url", error.Url);
            Assert.True(cancelled);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::rejects an oversized line in one chunk (trailing newline: %s)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_oversized_line_in_one_chunk(bool trailingNewline)
    {
        const int limit = 1024;
        var bytes = new byte[limit + 1 + (trailingNewline ? 1 : 0)];
        Array.Fill(bytes, (byte)' ', 0, limit + 1);
        bytes[limit - 2] = (byte)'{';
        bytes[limit - 1] = (byte)'}';
        if (trailingNewline)
        {
            bytes[limit + 1] = (byte)'\n';
        }

        using var stream = new ChunkedStream(bytes);
        await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: limit))
            {
            }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::accepts a line at the byte limit (trailing newline: %s)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_line_at_the_byte_limit(bool trailingNewline)
    {
        var bytes = Encoding.UTF8.GetBytes(new string(' ', 62) + "{}" + (trailingNewline ? "\n" : string.Empty));
        using var stream = new MemoryStream(bytes);
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: 64))
        {
            values.Add(value);
        }

        Assert.Single(values);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::accepts a chunk larger than the limit when each line is below the limit",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_chunk_larger_than_the_limit_when_each_line_is_below_the_limit()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("{}\n", 9))));
        var count = 0;
        await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, maxLineBytes: 8))
        {
            count++;
        }

        Assert.Equal(9, count);
    }


    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler::should return both parsed value and rawValue",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_response_keeps_the_raw_value_and_projects_known_properties()
    {
        var schema = JsonSchemas.Object(
            new[]
            {
                Pair("name", JsonSchemas.String()),
                Pair("age", JsonSchemas.Number()),
            },
            new[] { "name", "age" },
            additionalPropertiesFlag: false);
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}", Encoding.UTF8, "application/json"),
        };
        var body = await JsonStreams.ReadJsonAsync(response, schema);
        JsonAssert.Equal(body.Value, "{\"name\":\"John\",\"age\":30}");
        JsonAssert.Equal(body.RawValue, "{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}");
    }

    private static List<EventSourceMessage> Feed(string text, List<string>? errors = null)
    {
        var events = new List<EventSourceMessage>();
        var parser = new EventSourceParser();
        parser.Feed(text, events.Add, onError: errors == null ? null : errors.Add);
        return events;
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }

    private sealed class ChunkedStream : Stream
    {
        private readonly byte[][] _chunks;
        private int _chunk;
        private int _offset;

        public ChunkedStream(params byte[][] chunks)
        {
            _chunks = chunks;
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
            if (_chunk >= _chunks.Length)
            {
                return 0;
            }

            var current = _chunks[_chunk];
            var take = Math.Min(count, current.Length - _offset);
            Array.Copy(current, _offset, buffer, offset, take);
            _offset += take;
            if (_offset == current.Length)
            {
                _chunk++;
                _offset = 0;
            }

            return take;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
