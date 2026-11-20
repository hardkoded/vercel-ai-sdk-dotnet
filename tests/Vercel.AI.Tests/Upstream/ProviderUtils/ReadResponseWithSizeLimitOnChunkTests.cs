// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

/// <summary>Port of <c>read-response-with-size-limit.test.ts</c> &gt; <c>readResponseWithSizeLimit &gt; onChunk</c>.</summary>
public sealed class ReadResponseWithSizeLimitOnChunkTests
{
    private const string Prefix =
        "packages/provider-utils/src/read-response-with-size-limit.test.ts::readResponseWithSizeLimit > onChunk::";

    private const string Commit = "89aff56";

    // Like createMockResponse: chunks of 4 bytes, and a record of whether the body was released.
    private sealed class MockContent : HttpContent
    {
        private readonly byte[] _body;

        public MockContent(byte[] body, long? contentLength = null)
        {
            _body = body;
            Headers.ContentLength = contentLength;
        }

        public bool Cancelled { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(_body, 0, _body.Length);

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(new ChunkedStream(_body, 4));

        protected override bool TryComputeLength(out long length)
        {
            length = _body.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Cancelled = true;
            base.Dispose(disposing);
        }
    }

    private static (HttpResponseMessage Response, MockContent Content) Create(
        byte[] body,
        long? contentLength = null
    )
    {
        var content = new MockContent(body, contentLength);
        return (new HttpResponseMessage { Content = content }, content);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the chunks to the callback instead of collecting them",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_the_chunks_to_the_callback_instead_of_collecting_them()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        var (response, _) = Create(data);
        var received = new List<byte>();

        var result = await ResponseBodies.ReadResponseWithSizeLimitAsync(
            response,
            "http://example.com/file",
            onChunk: (chunk, count) => received.AddRange(chunk.Take(count))
        );

        Assert.Equal(data, received);
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should enforce the limit before passing a chunk on",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_enforce_the_limit_before_passing_a_chunk_on()
    {
        var (response, content) = Create(new byte[15], null);
        var calls = 0;

        var error = await Assert.ThrowsAsync<DownloadError>(() =>
            ResponseBodies.ReadResponseWithSizeLimitAsync(
                response,
                "http://example.com/file",
                10,
                onChunk: (_, _) => calls++
            )
        );

        Assert.True(DownloadError.IsInstance(error));
        // Chunks of 4 bytes: the third one would bring the total to 12, and a
        // fourth one is still waiting to be read.
        Assert.Equal(2, calls);
        Assert.True(content.Cancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject on Content-Length above the limit before reading",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_on_Content_Length_above_the_limit_before_reading()
    {
        var (response, content) = Create(new byte[10], 1000);
        var calls = 0;

        var error = await Assert.ThrowsAsync<DownloadError>(() =>
            ResponseBodies.ReadResponseWithSizeLimitAsync(
                response,
                "http://example.com/file",
                100,
                onChunk: (_, _) => calls++
            )
        );

        Assert.Contains("Content-Length: 1000", error.Message);
        Assert.Equal(0, calls);
        Assert.True(content.Cancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel the body and reject with the error of the callback",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_cancel_the_body_and_reject_with_the_error_of_the_callback()
    {
        var (response, content) = Create(new byte[10]);
        var failure = new InvalidOperationException("callback failed");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ResponseBodies.ReadResponseWithSizeLimitAsync(
                response,
                "http://example.com/file",
                onChunk: (_, _) => throw failure
            )
        );

        Assert.Same(failure, error);
        Assert.True(content.Cancelled);
    }
}
