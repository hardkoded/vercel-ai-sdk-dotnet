// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class ResponseBodyTests
{
    private const string Cancel = "packages/provider-utils/src/cancel-response-body.test.ts::cancelResponseBody::";

    private const string Fetch = "packages/provider-utils/src/handle-fetch-error.test.ts::handleFetchError > ";

    private const string Size = "packages/provider-utils/src/read-response-with-size-limit.test.ts::readResponseWithSizeLimit::";

    private const string TestUrl = "https://api.example.com/v1/chat";

    [Fact]
    [UpstreamTest(Cancel + "should cancel the body to release the connection", Coverage = UpstreamCoverage.Covered)]
    public void Cancel_releases_the_body()
    {
        var content = new TrackingContent(new byte[] { 1, 2, 3 });
        ResponseBodies.CancelResponseBody(new HttpResponseMessage { Content = content });
        Assert.True(content.Disposed);
    }

    [Fact]
    [UpstreamTest(Cancel + "should be a no-op when the body is null", Coverage = UpstreamCoverage.Covered)]
    public void Cancel_does_nothing_without_a_body()
    {
        ResponseBodies.CancelResponseBody(new HttpResponseMessage { Content = null });
    }

    [Fact]
    [UpstreamTest(Cancel + "should swallow cancel errors so the original rejection is preserved", Coverage = UpstreamCoverage.Covered)]
    public void Cancel_swallows_dispose_errors()
    {
        var content = new TrackingContent(Array.Empty<byte>()) { ThrowOnDispose = true };
        ResponseBodies.CancelResponseBody(new HttpResponseMessage { Content = content });
        Assert.True(content.Disposed);
    }

    [Fact]
    [UpstreamTest(Fetch + "abort errors::should return abort error as-is", Coverage = UpstreamCoverage.Covered)]
    public void Abort_errors_are_returned_unchanged()
    {
        var abort = new TaskCanceledException("Aborted");
        Assert.Same(abort, FetchErrors.HandleFetchError(abort, TestUrl, Prompt()));
    }

    [Fact]
    [UpstreamTest(Fetch + "node.js fetch errors::should handle TypeError with \"fetch failed\" message", Coverage = UpstreamCoverage.Covered)]
    public void Failed_request_becomes_a_retryable_api_call_error()
    {
        var cause = new Exception("ECONNREFUSED");
        var result = Assert.IsType<ApiCallError>(FetchErrors.HandleFetchError(new HttpRequestException("fetch failed", cause), TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: ECONNREFUSED", result.Message);
        Assert.Same(cause, result.Cause);
    }

    [Fact]
    [UpstreamTest(Fetch + "node.js fetch errors::should mark nested Undici socket errors as retryable", Coverage = UpstreamCoverage.Covered)]
    public void Nested_socket_errors_make_an_api_call_error_retryable()
    {
        var terminated = new IOException("terminated", new SocketException((int)SocketError.ConnectionReset));
        var requestBody = Prompt();
        var headers = new Dictionary<string, string> { ["x-request-id"] = "request-id" };
        var data = new Dictionary<string, object?> { ["partial"] = true };
        var original = new ApiCallError("Failed to process successful response", TestUrl, requestBody, 200, headers, "partial response", terminated, false, data);
        var result = Assert.IsType<ApiCallError>(FetchErrors.HandleFetchError(original, TestUrl, requestBody));
        Assert.NotSame(original, result);
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
    [UpstreamTest(Fetch + "browser fetch errors::should handle TypeError with \"Failed to fetch\" message", Coverage = UpstreamCoverage.Covered)]
    public void Browser_failed_fetch_is_retryable()
    {
        var result = Assert.IsType<ApiCallError>(FetchErrors.HandleFetchError(new HttpRequestException("TypeError: Failed to fetch", new Exception("Network error")), TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: Network error", result.Message);
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ConnectionRefused error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_refused_is_retryable()
    {
        AssertRetryable(SocketError.ConnectionRefused);
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ConnectionClosed error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_closed_is_retryable()
    {
        AssertRetryable(SocketError.ConnectionAborted);
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle FailedToOpenSocket error", Coverage = UpstreamCoverage.Covered)]
    public void Failed_to_open_socket_is_retryable()
    {
        AssertRetryable(SocketError.HostUnreachable);
    }

    [Fact]
    [UpstreamTest(Fetch + "bun fetch errors::should handle ECONNRESET error", Coverage = UpstreamCoverage.Covered)]
    public void Connection_reset_is_retryable()
    {
        AssertRetryable(SocketError.ConnectionReset);
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
        var result = await ResponseBodies.ReadResponseWithSizeLimitAsync(Response(data, declaredLength: 8), "http://example.com/file", 100);
        Assert.Equal(data, result);
    }

    [Fact]
    [UpstreamTest(Size + "should reject when Content-Length exceeds limit (early check)", Coverage = UpstreamCoverage.Covered)]
    public async Task Content_length_above_the_limit_is_rejected()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(Response(new byte[10], declaredLength: 1000), "http://example.com/large", 100));
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("Content-Length: 1000", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should cancel the body when Content-Length exceeds limit (prevents socket leak)", Coverage = UpstreamCoverage.Covered)]
    public async Task Content_length_rejection_releases_the_body()
    {
        var content = new TrackingContent(new byte[10], declaredLength: 1000);
        await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(new HttpResponseMessage { Content = content }, "http://example.com/large", 100));
        Assert.True(content.Disposed);
    }

    [Fact]
    [UpstreamTest(Size + "should abort when streamed bytes exceed limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Streamed_bytes_above_the_limit_are_rejected()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(Response(Filled(200)), "http://example.com/streaming", 50));
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("exceeded maximum size of 50 bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should preserve streamed size-limit errors when cancellation fails", Coverage = UpstreamCoverage.Covered)]
    public async Task A_failed_release_does_not_replace_the_size_limit_error()
    {
        var content = new TrackingContent(new byte[] { 1, 2 }) { ThrowOnDispose = true };
        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(new HttpResponseMessage { Content = content }, "http://example.com/streaming", 1));
        Assert.Equal("Download of http://example.com/streaming exceeded maximum size of 1 bytes.", error.Message);
        Assert.True(content.Disposed);
    }

    [Fact]
    [UpstreamTest(Size + "should handle lying Content-Length (says small, sends large)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_small_content_length_does_not_hide_a_large_body()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(Response(Filled(200), declaredLength: 10), "http://example.com/liar", 50));
        Assert.Contains("exceeded maximum size of 50 bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(Size + "should handle empty body (null)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_missing_body_reads_as_empty()
    {
        Assert.Empty(await ResponseBodies.ReadResponseWithSizeLimitAsync(new HttpResponseMessage { Content = null }, "http://example.com/empty", 100));
    }

    [Fact]
    [UpstreamTest(Size + "should handle empty body (zero-length)", Coverage = UpstreamCoverage.Covered)]
    public async Task A_zero_length_body_reads_as_empty()
    {
        Assert.Empty(await ResponseBodies.ReadResponseWithSizeLimitAsync(Response(Array.Empty<byte>()), "http://example.com/empty", 100));
    }

    [Fact]
    [UpstreamTest(Size + "should respect custom maxBytes", Coverage = UpstreamCoverage.Covered)]
    public async Task A_body_equal_to_the_limit_is_accepted()
    {
        var data = Filled(10);
        Assert.Equal(data, await ResponseBodies.ReadResponseWithSizeLimitAsync(Response(data, declaredLength: 10), "http://example.com/custom", 10));
    }

    [Fact]
    [UpstreamTest(Size + "should reject at exact boundary (maxBytes + 1)", Coverage = UpstreamCoverage.Covered)]
    public async Task One_byte_over_the_limit_is_rejected()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseBodies.ReadResponseWithSizeLimitAsync(Response(Filled(11)), "http://example.com/boundary", 10));
        Assert.True(DownloadError.IsInstance(error));
    }

    private static void AssertRetryable(SocketError code)
    {
        var error = new SocketException((int)code);
        var result = Assert.IsType<ApiCallError>(FetchErrors.HandleFetchError(error, TestUrl, Prompt()));
        Assert.True(result.IsRetryable);
        Assert.Equal("Cannot connect to API: " + error.Message, result.Message);
        Assert.Same(error, result.Cause);
    }

    private static Dictionary<string, object?> Prompt()
    {
        return new Dictionary<string, object?> { ["prompt"] = "test" };
    }

    private static HttpResponseMessage Response(byte[] data, long? declaredLength = null)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new TrackingContent(data, declaredLength) };
    }

    private static byte[] Filled(int length)
    {
        var data = new byte[length];
        for (var i = 0; i < length; i++)
        {
            data[i] = 42;
        }

        return data;
    }

    private sealed class TrackingContent : HttpContent
    {
        private readonly byte[] _data;

        public TrackingContent(byte[] data, long? declaredLength = null)
        {
            _data = data;
            Headers.ContentLength = declaredLength;
        }

        public bool Disposed { get; private set; }

        public bool ThrowOnDispose { get; set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return stream.WriteAsync(_data, 0, _data.Length);
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(_data));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
            if (ThrowOnDispose)
            {
                throw new InvalidOperationException("cannot cancel");
            }
        }
    }
}
