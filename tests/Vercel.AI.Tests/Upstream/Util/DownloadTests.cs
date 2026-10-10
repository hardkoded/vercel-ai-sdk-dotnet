// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

[Collection("DownloadFetch")]
public sealed class DownloadTests
{
    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF protection::should reject private IPv4 addresses", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Rejects_private_ipv4_addresses()
    {
        await AssertBlocked("http://127.0.0.1/file");
        await AssertBlocked("http://10.0.0.1/file");
        await AssertBlocked("http://169.254.169.254/latest/meta-data/");
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF protection::should reject localhost", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Rejects_localhost()
    {
        await AssertBlocked("http://localhost/file");
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::uses the current download mock after mocks are reset", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Uses_the_fetch_installed_for_each_call()
    {
        var previous = Download.Fetch;
        try
        {
            var firstCalls = 0;
            Download.Fetch = delegate (string url, DownloadRequest request)
            {
                firstCalls++;
                Assert.Null(typeof(DownloadRequest).GetProperty("Dispatcher"));
                return Task.FromResult(DownloadResponse.Text("first"));
            };
            var first = await Download.GetAsync(new Uri("https://download.invalid/file"));
            Assert.Equal(EncodingBytes("first"), first.Data);
            Assert.Equal(1, firstCalls);

            var secondCalls = 0;
            Download.Fetch = delegate (string url, DownloadRequest request)
            {
                secondCalls++;
                return Task.FromResult(DownloadResponse.Text("second"));
            };
            var second = await Download.GetAsync(new Uri("https://download.invalid/file"));
            Assert.Equal(EncodingBytes("second"), second.Data);
            Assert.Equal(1, secondCalls);
            Assert.Equal(1, firstCalls);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should reject a redirect to a private IP without requesting it", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Does_not_request_a_private_redirect()
    {
        var calls = 0;
        var cancelled = false;
        var headers = new HeaderCollection();
        headers.Add("location", "http://169.254.169.254/latest/meta-data/");
        var response = new DownloadResponse(302, "Found", headers, EncodingBytes("redirecting"))
        {
            OnCancel = delegate { cancelled = true; },
        };
        var error = await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("https://evil.com/redirect"), fetch: delegate
            {
                calls++;
                return Task.FromResult(response);
            });
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Equal(1, calls);
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should reject a redirect to localhost without requesting it", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Does_not_request_a_localhost_redirect()
    {
        var calls = 0;
        var cancelled = false;
        var headers = new HeaderCollection();
        headers.Add("location", "http://localhost:8080/admin");
        var response = new DownloadResponse(307, "Temporary Redirect", headers, EncodingBytes("redirecting"))
        {
            OnCancel = delegate { cancelled = true; },
        };
        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("https://evil.com/redirect"), fetch: delegate
            {
                calls++;
                return Task.FromResult(response);
            });
        });
        Assert.Equal(1, calls);
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should let the browser follow redirects natively on an opaque redirect", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Follows_an_opaque_redirect_in_the_browser()
    {
        var previous = Download.IsBrowser;
        Download.IsBrowser = true;
        try
        {
            var redirects = new List<string>();
            var content = new byte[] { 1, 2, 3 };
            var calls = 0;
            var result = await Download.GetAsync(new Uri("https://example.com/image.png"), fetch: delegate (string url, DownloadRequest request)
            {
                calls++;
                redirects.Add(request.Redirect);
                if (calls == 1)
                {
                    return Task.FromResult(new DownloadResponse(0, string.Empty, new HeaderCollection(), Array.Empty<byte>(), "opaqueredirect"));
                }

                var headers = new HeaderCollection();
                headers.Add("content-type", "image/png");
                return Task.FromResult(new DownloadResponse(200, "OK", headers, content));
            });
            Assert.Equal(content, result.Data);
            Assert.Equal(2, calls);
            Assert.Equal(new[] { "manual", "follow" }, redirects);
        }
        finally
        {
            Download.IsBrowser = previous;
        }
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should fail closed on an opaque redirect outside the browser", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Blocks_an_opaque_redirect_outside_the_browser()
    {
        var previous = Download.IsBrowser;
        Download.IsBrowser = false;
        try
        {
            var calls = 0;
            var error = await Assert.ThrowsAsync<DownloadError>(delegate
            {
                return Download.GetAsync(new Uri("https://example.com/redirect"), fetch: delegate
                {
                    calls++;
                    return Task.FromResult(new DownloadResponse(0, string.Empty, new HeaderCollection(), Array.Empty<byte>(), "opaqueredirect"));
                });
            });
            Assert.Contains("could not be validated", error.Message);
            Assert.Equal(1, calls);
        }
        finally
        {
            Download.IsBrowser = previous;
        }
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download SSRF redirect protection::should follow redirects to safe URLs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Follows_a_safe_redirect()
    {
        var calls = new List<string>();
        var content = new byte[] { 1, 2, 3 };
        var result = await Download.GetAsync(new Uri("https://example.com/image.png"), fetch: delegate (string url, DownloadRequest request)
        {
            calls.Add(url);
            Assert.Equal("manual", request.Redirect);
            if (calls.Count == 1)
            {
                var headers = new HeaderCollection();
                headers.Add("location", "https://cdn.example.com/image.png");
                return Task.FromResult(new DownloadResponse(302, "Found", headers, Array.Empty<byte>()));
            }

            var ok = new HeaderCollection();
            ok.Add("content-type", "image/png");
            return Task.FromResult(new DownloadResponse(200, "OK", ok, content));
        });
        Assert.Equal(content, result.Data);
        Assert.Equal("image/png", result.MediaType!);
        Assert.Equal(new[] { "https://example.com/image.png", "https://cdn.example.com/image.png" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should download data successfully and match expected bytes", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Downloads_the_response_bytes()
    {
        var expected = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        string? seen = null;
        var result = await Download.GetAsync(new Uri("http://example.com/file"), fetch: delegate (string url, DownloadRequest request)
        {
            seen = url;
            Assert.Equal("ai-sdk/dotnet", request.Headers.Get("user-agent")!);
            var headers = new HeaderCollection();
            headers.Add("content-type", "application/octet-stream");
            return Task.FromResult(new DownloadResponse(200, "OK", headers, expected));
        });
        Assert.Equal(expected, result.Data);
        Assert.Equal("application/octet-stream", result.MediaType!);
        Assert.Equal("http://example.com/file", seen);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should allow inline data URLs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Allows_inline_data_urls()
    {
        var result = await Download.GetAsync(new Uri("data:text/plain;base64,aGVsbG8="));
        Assert.Equal(EncodingBytes("hello"), result.Data);
        Assert.Equal("text/plain", result.MediaType!);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should throw DownloadError when response is not ok", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Throws_when_the_response_is_not_ok()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("http://example.com/file"), fetch: delegate
            {
                return Task.FromResult(new DownloadResponse(404, "Not Found", new HeaderCollection(), Array.Empty<byte>()));
            });
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Not Found", error.StatusText!);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should throw DownloadError when fetch throws an error", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Wraps_a_fetch_exception()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("http://example.com/file"), fetch: delegate
            {
                throw new InvalidOperationException("Network error");
            });
        });
        Assert.True(DownloadError.IsInstance(error));
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should cancel the body on non-ok response (prevents socket leak)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancels_a_failed_response()
    {
        var cancelled = false;
        var response = new DownloadResponse(404, "Not Found", new HeaderCollection(), new byte[10])
        {
            OnCancel = delegate { cancelled = true; },
        };
        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("http://example.com/not-found"), fetch: delegate { return Task.FromResult(response); });
        });
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should cancel the body when Content-Length exceeds limit (prevents socket leak)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancels_when_content_length_is_too_large()
    {
        var cancelled = false;
        var headers = new HeaderCollection();
        headers.Add("content-type", "application/octet-stream");
        headers.Add("content-length", (3L * 1024L * 1024L * 1024L).ToString());
        var response = new DownloadResponse(200, "OK", headers, new byte[10])
        {
            OnCancel = delegate { cancelled = true; },
        };
        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("http://example.com/large"), fetch: delegate { return Task.FromResult(response); });
        });
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should abort when response exceeds default size limit", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Rejects_a_body_over_the_default_limit()
    {
        var headers = new HeaderCollection();
        headers.Add("content-type", "application/octet-stream");
        headers.Add("content-length", (3L * 1024L * 1024L * 1024L).ToString());
        var error = await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri("http://example.com/large"), fetch: delegate
            {
                return Task.FromResult(new DownloadResponse(200, "OK", headers, new byte[10]));
            });
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Contains("exceeded maximum size", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/download/download.test.ts::download::should pass abortSignal to fetch", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Passes_the_abort_signal_to_fetch()
    {
        using (var source = new CancellationTokenSource())
        {
            source.Cancel();
            CancellationToken seen = default(CancellationToken);
            var error = await Assert.ThrowsAsync<DownloadError>(delegate
            {
                return Download.GetAsync(new Uri("http://example.com/file"), abortSignal: source.Token, fetch: delegate (string url, DownloadRequest request)
                {
                    seen = request.AbortSignal;
                    throw new OperationCanceledException();
                });
            });
            Assert.True(DownloadError.IsInstance(error));
            Assert.Equal(source.Token, seen);
        }
    }

    [UpstreamTest("packages/ai/src/util/download/download-function.test.ts::createDefaultDownloadFunction::should pass the abort signal to downloads", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Default_download_forwards_the_abort_signal()
    {
        using (var source = new CancellationTokenSource())
        {
            AssetDownloadCall? seen = null;
            var download = DefaultDownloadFunction.Create(delegate (AssetDownloadCall call)
            {
                seen = call;
                return Task.FromResult(new DownloadResult(new byte[] { 1, 2, 3 }, "text/plain"));
            }, source.Token);
            await download(new[] { new DownloadRequestItem(new Uri("https://example.com/file.txt"), false) });
            Assert.NotNull(seen);
            Assert.Equal("https://example.com/file.txt", seen!.Url.AbsoluteUri);
            Assert.False(seen.IsUrlSupportedByModel);
            Assert.Equal(source.Token, seen.AbortSignal);
        }
    }

    private static async Task AssertBlocked(string url)
    {
        var calls = 0;
        var error = await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.GetAsync(new Uri(url), fetch: delegate
            {
                calls++;
                return Task.FromResult(DownloadResponse.Bytes(new byte[] { 1 }, "text/plain"));
            });
        });
        Assert.True(DownloadError.IsInstance(error));
        Assert.Equal(0, calls);
    }

    private static byte[] EncodingBytes(string text)
    {
        return System.Text.Encoding.UTF8.GetBytes(text);
    }
}
