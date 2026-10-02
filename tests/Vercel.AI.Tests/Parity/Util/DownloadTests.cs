// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class DownloadTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF protection::should reject private IPv4 addresses", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_private_ipv4_addresses()
    {
        await AssertRejectedWithoutRequest(new Uri("http://127.0.0.1/file"));
        await AssertRejectedWithoutRequest(new Uri("http://10.0.0.1/file"));
        await AssertRejectedWithoutRequest(new Uri("http://169.254.169.254/latest/meta-data/"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF protection::should reject localhost", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_localhost()
    {
        await AssertRejectedWithoutRequest(new Uri("http://localhost/file"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should reject a redirect to a private IP without requesting it",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_redirect_to_a_private_ip()
    {
        var body = new TrackingContent(Encoding.UTF8.GetBytes("redirecting"));
        var handler = new ScriptedHandler((_, _, _) => Redirect(HttpStatusCode.Redirect, "http://169.254.169.254/latest/meta-data/", body));
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("https://evil.com/redirect"), handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Single(handler.Requests);
        Assert.True(body.Disposed);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should reject a redirect to localhost without requesting it",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_redirect_to_localhost()
    {
        var body = new TrackingContent(Encoding.UTF8.GetBytes("redirecting"));
        var handler = new ScriptedHandler((_, _, _) => Redirect(HttpStatusCode.TemporaryRedirect, "http://localhost:8080/admin", body));
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("https://evil.com/redirect"), handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Single(handler.Requests);
        Assert.True(body.Disposed);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should follow redirects to safe URLs",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Follows_a_redirect_to_a_safe_url()
    {
        var content = new byte[] { 1, 2, 3 };
        var handler = new ScriptedHandler((_, _, index) =>
        {
            if (index == 0)
            {
                return Redirect(HttpStatusCode.Redirect, "https://cdn.example.com/image.png", new TrackingContent(Array.Empty<byte>()));
            }

            return Ok(content, "image/png");
        });
        var result = await FileDownload.DownloadAsync(new Uri("https://example.com/image.png"), handler);
        Assert.Equal(content, result.Data);
        Assert.Equal("image/png", result.MediaType);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://cdn.example.com/image.png", handler.Requests[1].RequestUri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should download data successfully and match expected bytes", Coverage = UpstreamCoverage.Covered)]
    public async Task Downloads_the_response_bytes()
    {
        var expected = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var handler = new ScriptedHandler((request, _, _) =>
        {
            Assert.True(request.Headers.TryGetValues("User-Agent", out var agents));
            Assert.Contains("ai-sdk/dotnet", agents!);
            return Ok(expected, "application/octet-stream");
        });
        var result = await FileDownload.DownloadAsync(new Uri("http://example.com/file"), handler);
        Assert.Equal(expected, result.Data);
        Assert.Equal("application/octet-stream", result.MediaType);
        Assert.Equal("http://example.com/file", handler.Requests[0].RequestUri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should allow inline data URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Decodes_an_inline_data_url()
    {
        var result = await FileDownload.DownloadAsync(new Uri("data:text/plain;base64,aGVsbG8="));
        Assert.Equal(Encoding.UTF8.GetBytes("hello"), result.Data);
        Assert.Equal("text/plain", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should throw DownloadError when response is not ok", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_the_response_is_not_ok()
    {
        var handler = new ScriptedHandler((_, _, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Not Found",
            };
            return response;
        });
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/file"), handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Not Found", error.StatusText);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should throw DownloadError when fetch throws an error", Coverage = UpstreamCoverage.Covered)]
    public async Task Wraps_a_handler_exception()
    {
        var handler = new ScriptedHandler((_, _, _) => throw new InvalidOperationException("Network error"));
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/file"), handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Contains("Network error", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download.test.ts::download::should cancel the body on non-ok response (prevents socket leak)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Disposes_the_body_when_the_response_is_not_ok()
    {
        var body = new TrackingContent(new byte[10]);
        var handler = new ScriptedHandler((_, _, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Not Found",
                Content = body,
            };
        });
        await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/not-found"), handler));
        Assert.True(body.Disposed);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download.test.ts::download::should cancel the body when Content-Length exceeds limit (prevents socket leak)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Disposes_the_body_when_content_length_exceeds_the_limit()
    {
        var body = Oversized();
        var handler = new ScriptedHandler((_, _, _) => Ok(body, "application/octet-stream"));
        await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/large"), handler));
        Assert.True(body.Disposed);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should abort when response exceeds default size limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_body_over_the_default_size_limit()
    {
        var handler = new ScriptedHandler((_, _, _) => Ok(Oversized(), "application/octet-stream"));
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/large"), handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Contains("exceeded maximum size", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should pass abortSignal to fetch", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_an_already_canceled_token_to_the_handler()
    {
        var called = false;
        var canceled = false;
        var handler = new ScriptedHandler((_, token, _) =>
        {
            called = true;
            canceled = token.IsCancellationRequested;
            throw new OperationCanceledException(token);
        });
        using (var source = new CancellationTokenSource())
        {
            source.Cancel();
            var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(new Uri("http://example.com/file"), handler, cancellationToken: source.Token));
            Assert.True(DownloadException.IsInstance(error));
        }

        Assert.True(called);
        Assert.True(canceled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/download/download-function.test.ts::createDefaultDownloadFunction::should pass the abort signal to downloads",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_abort_token_to_the_download_function()
    {
        using (var source = new CancellationTokenSource())
        {
            Uri? seenUrl = null;
            var seenSupported = true;
            CancellationToken seenToken = default;
            var download = new Func<Uri, bool, CancellationToken, Task<DownloadResult>>((url, supported, token) =>
            {
                seenUrl = url;
                seenSupported = supported;
                seenToken = token;
                return Task.FromResult(new DownloadResult(new byte[] { 1, 2, 3 }, "text/plain"));
            });
            var function = FileDownload.CreateDefaultDownloadFunction(download, source.Token);
            await function(new[] { new DownloadRequest(new Uri("https://example.com/file.txt"), false) });
            Assert.Equal(new Uri("https://example.com/file.txt"), seenUrl);
            Assert.False(seenSupported);
            Assert.Equal(source.Token, seenToken);
        }
    }

    private static async Task AssertRejectedWithoutRequest(Uri url)
    {
        var handler = new ScriptedHandler((_, _, _) => throw new InvalidOperationException("request was sent"));
        var error = await Assert.ThrowsAsync<DownloadException>(() => FileDownload.DownloadAsync(url, handler));
        Assert.True(DownloadException.IsInstance(error));
        Assert.Empty(handler.Requests);
    }

    private static HttpResponseMessage Redirect(HttpStatusCode status, string location, HttpContent body)
    {
        var response = new HttpResponseMessage(status) { Content = body };
        response.Headers.Location = new Uri(location);
        return response;
    }

    private static HttpResponseMessage Ok(byte[] data, string mediaType)
    {
        var content = new ByteArrayContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static HttpResponseMessage Ok(TrackingContent content, string mediaType)
    {
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static TrackingContent Oversized()
    {
        var content = new TrackingContent(new byte[10]);
        content.Headers.ContentLength = 3L * 1024L * 1024L * 1024L;
        return content;
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, int, HttpResponseMessage> _respond;
        private int _count;

        public ScriptedHandler(Func<HttpRequestMessage, CancellationToken, int, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_respond(request, cancellationToken, _count++));
        }
    }

    private sealed class TrackingContent : HttpContent
    {
        private readonly byte[] _data;

        public TrackingContent(byte[] data)
        {
            _data = data;
        }

        public bool Disposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return stream.WriteAsync(_data, 0, _data.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _data.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
