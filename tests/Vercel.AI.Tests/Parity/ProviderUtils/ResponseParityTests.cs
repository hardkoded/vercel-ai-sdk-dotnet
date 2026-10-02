// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class ResponseParityTests
{
    private const string Handler = "packages/provider-utils/src/response-handler.test.ts::";

    private const string Cancel = "packages/provider-utils/src/cancel-response-body.test.ts::cancelResponseBody::";

    private const string Fetch = "packages/provider-utils/src/handle-fetch-error.test.ts::handleFetchError > ";

    private const string Size = "packages/provider-utils/src/read-response-with-size-limit.test.ts::readResponseWithSizeLimit::";

    private const string TestUrl = "https://api.example.com/v1/chat";

    [Fact]
    [UpstreamTest(Handler + "createJsonResponseHandler::should return both parsed value and rawValue", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_handler_returns_the_validated_value_and_the_raw_value()
    {
        var response = Body("{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}");
        var handler = ResponseHandlers.CreateJsonResponseHandler(PersonSchema());
        var result = await handler(Context(response));
        Assert.Equal("John", result.Value!["name"]!.GetValue<string>());
        Assert.Equal(30, result.Value["age"]!.GetValue<int>());
        Assert.Null(result.Value["extraField"]);
        Assert.True(result.HasRawValue);
        Assert.Equal("John", result.RawValue!["name"]!.GetValue<string>());
        Assert.Equal(30, result.RawValue["age"]!.GetValue<int>());
        Assert.Equal("ignored", result.RawValue["extraField"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonResponseHandler::should reject oversized responses before reading the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_handler_rejects_an_oversized_body()
    {
        var (response, cancelled) = Oversized();
        var handler = ResponseHandlers.CreateJsonResponseHandler(IdentitySchema());
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await handler(Context(response));
        });
        Assert.Contains("exceeded maximum size", error.Message);
        Assert.True(cancelled());
    }

    [Fact]
    [UpstreamTest(Handler + "createEventSourceResponseHandler::should preserve context and mark response body socket errors as retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task Event_stream_socket_errors_are_retryable_and_keep_context()
    {
        var socketError = new JsCauseError("other side closed", code: "UND_ERR_SOCKET");
        var terminated = new JsCauseError("terminated", name: "TypeError", cause: socketError);
        var requestBody = new Dictionary<string, object?> { ["prompt"] = "test" };
        var response = new FetchResponse
        {
            Status = 200,
            Body = new ChunkStream(
                new[] { Encoding.UTF8.GetBytes("data: {\"value\":\"partial\"}\n\n") },
                terminated),
        };
        response.Headers["x-request-id"] = "request-id";
        var handler = ResponseHandlers.CreateEventSourceResponseHandler(IdentitySchema());
        var result = await handler(new ResponseHandlerContext("test-url", requestBody, response));
        await using var reader = result.Value.GetAsyncEnumerator();
        Assert.True(await reader.MoveNextAsync());
        Assert.True(reader.Current.Success);
        Assert.Equal("partial", reader.Current.Value!["value"]!.GetValue<string>());

        var error = await Assert.ThrowsAsync<APICallError>(async () =>
        {
            await reader.MoveNextAsync();
        });
        Assert.Equal("AI_APICallError", error.Name);
        Assert.Equal("Failed to process successful response", error.Message);
        Assert.True(error.IsRetryable);
        Assert.Equal(200, error.StatusCode);
        Assert.Equal("request-id", error.ResponseHeaders!["x-request-id"]);
        Assert.Same(terminated, error.Cause);
        Assert.Equal("test-url", error.Url);
        Assert.Same(requestBody, error.RequestBodyValues);
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonLinesResponseHandler::parses JSON lines across byte boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_survive_a_split_utf8_character()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"id\":\"first\",\"text\":\"café\"}\r\n\n{\"id\":\"second\",\"text\":\"done\"}");
        var response = new FetchResponse
        {
            Body = new ChunkStream(new[]
            {
                Slice(bytes, 0, 24),
                Slice(bytes, 24, 27),
                Slice(bytes, 27, bytes.Length),
            }),
        };
        response.Headers["x-test"] = "value";
        var handler = ResponseHandlers.CreateJsonLinesResponseHandler(IdentitySchema());
        var result = await handler(Context(response));
        var values = new List<JsonNode?>();
        await foreach (var value in result.Value)
        {
            values.Add(value);
        }

        Assert.Equal(2, values.Count);
        Assert.Equal("first", values[0]!["id"]!.GetValue<string>());
        Assert.Equal("café", values[0]!["text"]!.GetValue<string>());
        Assert.Equal("second", values[1]!["id"]!.GetValue<string>());
        Assert.Equal("done", values[1]!["text"]!.GetValue<string>());
        Assert.Equal("value", result.ResponseHeaders["x-test"]);
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonLinesResponseHandler::errors when a line is invalid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Invalid_json_line_throws()
    {
        var handler = ResponseHandlers.CreateJsonLinesResponseHandler(IdentitySchema());
        var result = await handler(Context(Body("{\"id\":\"first\"}\n{invalid}\n")));
        await using var reader = result.Value.GetAsyncEnumerator();
        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("first", reader.Current!["id"]!.GetValue<string>());
        await Assert.ThrowsAsync<JSONParseError>(async () =>
        {
            await reader.MoveNextAsync();
        });
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonLinesResponseHandler::cancels the response body when iteration stops early", Coverage = UpstreamCoverage.Covered)]
    public async Task Stopping_json_lines_early_cancels_the_body()
    {
        var cancelled = false;
        var response = new FetchResponse
        {
            Body = new ChunkStream(new[] { Encoding.UTF8.GetBytes("{\"id\":\"first\"}\n") }),
            BodyCancel = () =>
            {
                cancelled = true;
                return Task.CompletedTask;
            },
        };
        var handler = ResponseHandlers.CreateJsonLinesResponseHandler(IdentitySchema());
        var result = await handler(Context(response));
        await using (var reader = result.Value.GetAsyncEnumerator())
        {
            Assert.True(await reader.MoveNextAsync());
        }

        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonLinesResponseHandler::throws EmptyResponseBodyError when the response body is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_reject_a_null_body()
    {
        var handler = ResponseHandlers.CreateJsonLinesResponseHandler(IdentitySchema());
        var error = await Assert.ThrowsAsync<EmptyResponseBodyError>(async () =>
        {
            await handler(Context(new FetchResponse()));
        });
        Assert.Equal("Empty response body", error.Message);
    }

    [Fact]
    [UpstreamTest(Handler + "createJsonErrorResponseHandler::should reject oversized responses before reading the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_error_handler_rejects_an_oversized_body()
    {
        var (response, cancelled) = Oversized("{\"error\":\"too large\"}", 500, "Internal Server Error");
        var handler = ResponseHandlers.CreateJsonErrorResponseHandler(IdentitySchema(), _ => "unused");
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await handler(Context(response));
        });
        Assert.Contains("exceeded maximum size", error.Message);
        Assert.True(cancelled());
    }

    [Fact]
    [UpstreamTest(Handler + "createBinaryResponseHandler::should handle binary response successfully", Coverage = UpstreamCoverage.Covered)]
    public async Task Binary_handler_returns_the_body_bytes()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        var handler = ResponseHandlers.CreateBinaryResponseHandler();
        var result = await handler(Context(Body(data)));
        Assert.Equal(data, result.Value);
    }

    [Fact]
    [UpstreamTest(Handler + "createBinaryResponseHandler::should throw APICallError when response body is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Binary_handler_rejects_a_null_body()
    {
        var handler = ResponseHandlers.CreateBinaryResponseHandler();
        var error = await Assert.ThrowsAsync<APICallError>(async () =>
        {
            await handler(Context(new FetchResponse()));
        });
        Assert.Equal("Response body is empty", error.Message);
    }

    [Fact]
    [UpstreamTest(Handler + "createBinaryStreamResponseHandler::should pass the response body through as a stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Binary_stream_handler_returns_the_body_stream()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        var body = new MemoryStream(data);
        var handler = ResponseHandlers.CreateBinaryStreamResponseHandler();
        var result = await handler(Context(new FetchResponse { Body = body }));
        Assert.Same(body, result.Value);
        var collected = new byte[4];
        Assert.Equal(4, await result.Value.ReadAsync(collected, 0, collected.Length));
        Assert.Equal(data, collected);
    }

    [Fact]
    [UpstreamTest(Handler + "createBinaryStreamResponseHandler::should throw EmptyResponseBodyError when response body is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Binary_stream_handler_rejects_a_null_body()
    {
        var handler = ResponseHandlers.CreateBinaryStreamResponseHandler();
        var error = await Assert.ThrowsAsync<EmptyResponseBodyError>(async () =>
        {
            await handler(Context(new FetchResponse()));
        });
        Assert.Equal("Empty response body", error.Message);
    }

    [Fact]
    [UpstreamTest(Handler + "createStatusCodeErrorResponseHandler::should create error with status text and response body", Coverage = UpstreamCoverage.Covered)]
    public async Task Status_handler_uses_the_status_text_and_body()
    {
        var requestBody = new Dictionary<string, object?> { ["some"] = "data" };
        var response = Body("Error message");
        response.Status = 404;
        response.StatusText = "Not Found";
        var handler = ResponseHandlers.CreateStatusCodeErrorResponseHandler();
        var result = await handler(new ResponseHandlerContext("test-url", requestBody, response));
        Assert.Equal("Not Found", result.Value.Message);
        Assert.Equal(404, result.Value.StatusCode);
        Assert.Equal("Error message", result.Value.ResponseBody);
        Assert.Equal("test-url", result.Value.Url);
        Assert.Same(requestBody, result.Value.RequestBodyValues);
    }

    [Fact]
    [UpstreamTest(Handler + "createStatusCodeErrorResponseHandler::should reject oversized responses before reading the body", Coverage = UpstreamCoverage.Covered)]
    public async Task Status_handler_rejects_an_oversized_body()
    {
        var (response, cancelled) = Oversized("too large", 500, "Internal Server Error");
        var handler = ResponseHandlers.CreateStatusCodeErrorResponseHandler();
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await handler(Context(response));
        });
        Assert.Contains("exceeded maximum size", error.Message);
        Assert.True(cancelled());
    }

    [Fact]
    [UpstreamTest(Cancel + "should cancel the body to release the connection", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancel_invokes_the_body_cancel_callback()
    {
        var calls = 0;
        var response = new FetchResponse
        {
            Body = new MemoryStream(new byte[] { 1, 2, 3 }),
            BodyCancel = () =>
            {
                calls++;
                return Task.CompletedTask;
            },
        };
        await ResponseHandlers.CancelResponseBody(response);
        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(Cancel + "should be a no-op when the body is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancel_does_nothing_when_the_body_is_null()
    {
        await ResponseHandlers.CancelResponseBody(new FetchResponse());
    }

    [Fact]
    [UpstreamTest(Cancel + "should swallow cancel errors so the original rejection is preserved", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancel_swallows_body_cancel_errors()
    {
        var response = new FetchResponse
        {
            Body = new MemoryStream(Array.Empty<byte>()),
            BodyCancel = () => Task.FromException(new Exception("cannot cancel")),
        };
        await ResponseHandlers.CancelResponseBody(response);
    }

    [Fact]
    [UpstreamTest(Fetch + "abort errors::should return abort error as-is", Coverage = UpstreamCoverage.Covered)]
    public void Abort_errors_are_returned_unchanged()
    {
        var abortError = new DomException("Aborted", "AbortError");
        var requestBody = new Dictionary<string, object?> { ["prompt"] = "test" };
        Assert.Same(abortError, FetchErrors.HandleFetchError(abortError, TestUrl, requestBody));
    }

    [Fact]
    [UpstreamTest(Fetch + "node.js fetch errors::should handle TypeError with \"fetch failed\" message", Coverage = UpstreamCoverage.Covered)]
    public void Node_fetch_failed_becomes_a_retryable_api_call_error()
    {
        var cause = new Exception("ECONNREFUSED");
        var fetchError = new JsCauseError("fetch failed", name: "TypeError", cause: cause);
        var result = Assert.IsType<APICallError>(FetchErrors.HandleFetchError(fetchError, TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: ECONNREFUSED", result.Message);
    }

    [Fact]
    [UpstreamTest(Fetch + "node.js fetch errors::should mark nested Undici socket errors as retryable", Coverage = UpstreamCoverage.Covered)]
    public void Nested_undici_socket_errors_are_retryable()
    {
        var socketError = new JsCauseError("other side closed", code: "UND_ERR_SOCKET");
        var terminated = new JsCauseError("terminated", name: "TypeError", cause: socketError);
        var requestBody = Prompt();
        var headers = new Dictionary<string, string> { ["x-request-id"] = "request-id" };
        var data = new Dictionary<string, object?> { ["partial"] = true };
        var apiCallError = new APICallError(
            "Failed to process successful response",
            TestUrl,
            requestBody,
            200,
            headers,
            "partial response",
            terminated,
            false,
            data);
        var result = Assert.IsType<APICallError>(FetchErrors.HandleFetchError(apiCallError, TestUrl, requestBody));
        Assert.NotSame(apiCallError, result);
        Assert.Equal("Failed to process successful response", result.Message);
        Assert.Same(terminated, result.Cause);
        Assert.Equal(TestUrl, result.Url);
        Assert.Same(requestBody, result.RequestBodyValues);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("request-id", result.ResponseHeaders!["x-request-id"]);
        Assert.Equal("partial response", result.ResponseBody);
        Assert.Same(data, result.Data);
        Assert.True(result.IsRetryable);
    }

    [Fact]
    [UpstreamTest(Fetch + "node.js fetch errors::should stop traversing cyclic error causes", Coverage = UpstreamCoverage.Covered)]
    public void Cyclic_causes_stop_the_walk()
    {
        var error = new JsCauseError("cyclic");
        error.Cause = error;
        Assert.Same(error, FetchErrors.HandleFetchError(error, TestUrl, Prompt()));
    }

    [Fact]
    [UpstreamTest(Fetch + "browser fetch errors::should handle TypeError with \"Failed to fetch\" message", Coverage = UpstreamCoverage.Covered)]
    public void Browser_failed_to_fetch_is_retryable()
    {
        var fetchError = new JsCauseError("Failed to fetch", name: "TypeError", cause: new Exception("Network error"));
        var result = Assert.IsType<APICallError>(FetchErrors.HandleFetchError(fetchError, TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: Network error", result.Message);
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ConnectionRefused error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_refused_is_retryable()
    {
        AssertRetryableCode(
            "Unable to connect. Is the computer able to access the url?",
            "ConnectionRefused");
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ConnectionClosed error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_closed_is_retryable()
    {
        AssertRetryableCode("The socket connection was closed unexpectedly", "ConnectionClosed");
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle FailedToOpenSocket error", Coverage = UpstreamCoverage.Covered)]
    public void Failed_to_open_socket_is_retryable()
    {
        AssertRetryableCode("Was there a typo in the url or port?", "FailedToOpenSocket");
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ECONNRESET error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_reset_is_retryable()
    {
        AssertRetryableCode(
            "Client network socket disconnected before secure TLS connection was established",
            "ECONNRESET");
    }

    [Fact]
    [UpstreamTest(Fetch + "unknown errors::should return unknown errors as-is", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_errors_are_returned_unchanged()
    {
        var unknown = new Exception("Something unexpected");
        Assert.Same(unknown, FetchErrors.HandleFetchError(unknown, TestUrl, Prompt()));
    }

    [Fact]
    [UpstreamTest(Size + "should read response within limit successfully", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_a_body_inside_the_limit()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var response = Body(data);
        response.Headers["content-length"] = "8";
        var result = await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/file", 100);
        Assert.Equal(data, result);
    }

    [Fact]
    [UpstreamTest(Size + "should reject when Content-Length exceeds limit (early check)", Coverage = UpstreamCoverage.Covered)]
    public async Task Content_length_above_the_limit_is_rejected()
    {
        var response = Body(new byte[10]);
        response.Headers["content-length"] = "1000";
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/large", 100);
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("Content-Length: 1000", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should cancel the body when Content-Length exceeds limit (prevents socket leak)", Coverage = UpstreamCoverage.Covered)]
    public async Task Content_length_rejection_cancels_the_body()
    {
        var cancelled = false;
        var response = Body(new byte[10]);
        response.Headers["content-length"] = "1000";
        response.BodyCancel = () =>
        {
            cancelled = true;
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/large", 100);
        });
        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(Size + "should abort when streamed bytes exceed limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Streamed_bytes_above_the_limit_are_rejected()
    {
        var large = new byte[200];
        for (var i = 0; i < large.Length; i++)
        {
            large[i] = 42;
        }

        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(Body(large), "http://example.com/streaming", 50);
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("exceeded maximum size of 50 bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should preserve streamed size-limit errors when cancellation fails", Coverage = UpstreamCoverage.Covered)]
    public async Task A_failed_cancel_does_not_replace_the_size_limit_error()
    {
        var cancelError = new Exception("cancel failed");
        var cancelled = false;
        var response = new FetchResponse
        {
            Body = new MemoryStream(new byte[] { 1, 2 }),
            BodyCancel = () =>
            {
                cancelled = true;
                return Task.FromException(cancelError);
            },
        };
        response.Locked = false;
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/streaming", 1);
        });
        Assert.NotSame(cancelError, error);
        Assert.True(DownloadError.IsInstance(error));
        Assert.Equal(
            "Download of http://example.com/streaming exceeded maximum size of 1 bytes.",
            error.Message);
        Assert.True(cancelled);
        Assert.False(response.Locked);
    }

    [Fact]
    [UpstreamTest(Size + "should handle lying Content-Length (says small, sends large)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_small_content_length_does_not_hide_a_large_body()
    {
        var large = new byte[200];
        for (var i = 0; i < large.Length; i++)
        {
            large[i] = 42;
        }

        var response = Body(large);
        response.Headers["content-length"] = "10";
        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/liar", 50);
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("exceeded maximum size of 50 bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should handle empty body (null)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_null_body_reads_as_empty()
    {
        var result = await ResponseHandlers.ReadResponseWithSizeLimit(new FetchResponse(), "http://example.com/empty", 100);
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(Size + "should handle empty body (zero-length)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_zero_length_body_reads_as_empty()
    {
        var result = await ResponseHandlers.ReadResponseWithSizeLimit(Body(Array.Empty<byte>()), "http://example.com/empty", 100);
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(Size + "should respect custom maxBytes", Coverage = UpstreamCoverage.Covered)]
    public async Task A_body_equal_to_the_limit_is_accepted()
    {
        var data = new byte[10];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = 1;
        }

        var response = Body(data);
        response.Headers["content-length"] = "10";
        var result = await ResponseHandlers.ReadResponseWithSizeLimit(response, "http://example.com/custom", 10);
        Assert.Equal(data, result);
    }

    [Fact]
    [UpstreamTest(Size + "should reject at exact boundary (maxBytes + 1)", Coverage = UpstreamCoverage.Covered)]
    public async Task One_byte_over_the_limit_is_rejected()
    {
        var data = new byte[11];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = 1;
        }

        var error = await Assert.ThrowsAsync<DownloadError>(async () =>
        {
            await ResponseHandlers.ReadResponseWithSizeLimit(Body(data), "http://example.com/boundary", 10);
        });
        Assert.True(DownloadError.IsInstance(error));
    }

    private static void AssertRetryableCode(string message, string code)
    {
        var error = new JsCauseError(message, code: code);
        var result = Assert.IsType<APICallError>(FetchErrors.HandleFetchError(error, TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: " + message, result.Message);
    }

    private static Dictionary<string, object?> Prompt()
    {
        return new Dictionary<string, object?> { ["prompt"] = "test" };
    }

    private static ResponseHandlerContext Context(FetchResponse response)
    {
        return new ResponseHandlerContext("test-url", new Dictionary<string, object?>(), response);
    }

    private static FetchResponse Body(string text)
    {
        return Body(Encoding.UTF8.GetBytes(text));
    }

    private static FetchResponse Body(byte[] data)
    {
        return new FetchResponse { Body = new MemoryStream(data) };
    }

    private static (FetchResponse Response, Func<bool> Cancelled) Oversized(string body = "{}", int status = 200, string statusText = "")
    {
        var cancelled = false;
        var response = Body(body);
        response.Status = status;
        response.StatusText = statusText;
        response.Headers["content-length"] = (ResponseHandlers.DefaultMaxDownloadSize + 1).ToString(CultureInfo.InvariantCulture);
        response.BodyCancel = () =>
        {
            cancelled = true;
            return Task.CompletedTask;
        };
        return (response, () => cancelled);
    }

    private static FlexibleSchema PersonSchema()
    {
        return new FlexibleSchema(node =>
        {
            var source = node!.AsObject();
            return SchemaValidationResult.Ok(new JsonObject
            {
                ["name"] = source["name"]!.DeepClone(),
                ["age"] = source["age"]!.DeepClone(),
            });
        });
    }

    private static FlexibleSchema IdentitySchema()
    {
        return new FlexibleSchema(node => SchemaValidationResult.Ok(node));
    }

    private static byte[] Slice(byte[] bytes, int start, int end)
    {
        var slice = new byte[end - start];
        Buffer.BlockCopy(bytes, start, slice, 0, slice.Length);
        return slice;
    }

    private sealed class ChunkStream : Stream
    {
        private readonly Queue<byte[]> _chunks;
        private readonly Exception? _errorWhenEmpty;
        private byte[]? _current;
        private int _offset;

        public ChunkStream(IEnumerable<byte[]> chunks, Exception? errorWhenEmpty = null)
        {
            _chunks = new Queue<byte[]>(chunks);
            _errorWhenEmpty = errorWhenEmpty;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_current == null || _offset >= _current.Length)
            {
                if (_chunks.Count == 0)
                {
                    if (_errorWhenEmpty != null)
                    {
                        throw _errorWhenEmpty;
                    }

                    return Task.FromResult(0);
                }

                _current = _chunks.Dequeue();
                _offset = 0;
            }

            var length = Math.Min(count, _current.Length - _offset);
            Buffer.BlockCopy(_current, _offset, buffer, offset, length);
            _offset += length;
            return Task.FromResult(length);
        }
    }
}
