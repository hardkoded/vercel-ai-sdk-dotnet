// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.FetchWithValidatedRedirects;

/// <summary>Port of <c>fetch-with-validated-redirects.test.ts</c> &gt; <c>fetchWithValidatedEndpoint</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FetchWithValidatedEndpointTests
{
    private const string Prefix = "packages/provider-utils/src/fetch-with-validated-redirects.test.ts::fetchWithValidatedEndpoint::";

    [Fact]
    [UpstreamTest(Prefix + "validates the URL before issuing the request", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_the_URL_before_issuing_the_request()
    {
        var calls = 0;
        DownloadFetch fetch = delegate
        {
            calls++;
            return Task.FromResult(OkResponse());
        };

        await Assert.ThrowsAsync<DownloadError>(delegate
        {
            return Download.FetchWithValidatedEndpointAsync("http://169.254.169.254/token", fetch: fetch);
        });
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("forwarded global")]
    [InlineData("custom")]
    [UpstreamTest(Prefix + "uses the selected runtime transport (%s)", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Uses_the_selected_runtime_transport(string mode)
    {
        var runtimeCalls = new List<(string Url, string Redirect)>();
        var customCalls = new List<(string Url, string Redirect)>();
        DownloadFetch runtimeFetch = (url, request) =>
        {
            runtimeCalls.Add((url, request.Redirect));
            return Task.FromResult(OkResponse());
        };
        DownloadFetch customFetch = (url, request) =>
        {
            customCalls.Add((url, request.Redirect));
            return Task.FromResult(OkResponse());
        };
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = runtimeFetch;
            await Download.FetchWithValidatedEndpointAsync(
                "https://files.example.com/file",
                fetch: mode == "default" ? null : mode == "custom" ? customFetch : runtimeFetch);
        }
        finally
        {
            Download.Fetch = previous;
        }

        var selected = mode == "custom" ? customCalls : runtimeCalls;
        var other = mode == "custom" ? runtimeCalls : customCalls;
        Assert.Equal(new[] { ("https://files.example.com/file", "error") }, selected);
        Assert.Empty(other);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("forwarded global")]
    [UpstreamTest(Prefix + "rejects the runtime fetch with strict DNS validation enabled (%s)", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Rejects_the_runtime_fetch_with_strict_DNS_validation_enabled(string mode)
    {
        var calls = 0;
        DownloadFetch runtimeFetch = (url, request) =>
        {
            calls++;
            return Task.FromResult(OkResponse());
        };
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = runtimeFetch;
            var error = await Assert.ThrowsAsync<DownloadError>(delegate
            {
                return Download.FetchWithValidatedEndpointAsync(
                    "https://example.com/metadata",
                    fetch: mode == "default" ? null : runtimeFetch,
                    requireDnsValidation: true);
            });
            Assert.Contains("This server runtime requires a custom fetch with connect-time DNS validation for untrusted URLs", error.Message);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses a custom fetch with strict DNS validation enabled", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Uses_a_custom_fetch_with_strict_DNS_validation_enabled()
    {
        var runtimeCalls = 0;
        var customCalls = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                runtimeCalls++;
                return Task.FromResult(OkResponse());
            };
            await Download.FetchWithValidatedEndpointAsync(
                "https://example.com/metadata",
                fetch: (url, request) =>
                {
                    customCalls++;
                    return Task.FromResult(OkResponse());
                },
                requireDnsValidation: true);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, customCalls);
        Assert.Equal(0, runtimeCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "allows the trusted origin through the runtime fetch with strict DNS validation enabled", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Allows_the_trusted_origin_through_the_runtime_fetch_with_strict_DNS_validation_enabled()
    {
        var calls = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                calls++;
                return Task.FromResult(OkResponse());
            };
            await Download.FetchWithValidatedEndpointAsync(
                "http://localhost:3000/metadata",
                trustedOrigin: "http://localhost:3000",
                requireDnsValidation: true);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves browser fetch with strict DNS validation enabled", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Preserves_browser_fetch_with_strict_DNS_validation_enabled()
    {
        var calls = 0;
        var previous = Download.Fetch;
        var previousBrowser = Download.IsBrowser;
        try
        {
            Download.IsBrowser = true;
            Download.Fetch = (url, request) =>
            {
                calls++;
                return Task.FromResult(OkResponse());
            };
            await Download.FetchWithValidatedEndpointAsync("https://example.com/metadata", requireDnsValidation: true);
        }
        finally
        {
            Download.Fetch = previous;
            Download.IsBrowser = previousBrowser;
        }

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses redirect: error for credential-bearing requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_redirect_error_for_credential_bearing_requests()
    {
        string? calledUrl = null;
        DownloadRequest? calledRequest = null;
        var calls = 0;
        DownloadFetch fetch = delegate (string url, DownloadRequest request)
        {
            calls++;
            calledUrl = url;
            calledRequest = request;
            return Task.FromResult(OkResponse());
        };
        var body = Encoding.UTF8.GetBytes("code=secret-code");
        var headers = new HeaderCollection();
        headers.Add("authorization", "Basic secret");

        await Download.FetchWithValidatedEndpointAsync(
            "https://auth.example.com/token",
            new DownloadRequest(headers, default, "manual") { Method = "POST", Body = body },
            fetch);

        Assert.Equal(1, calls);
        Assert.Equal("https://auth.example.com/token", calledUrl);
        Assert.NotNull(calledRequest);
        Assert.Equal("POST", calledRequest!.Method);
        Assert.Equal(
            new[] { new KeyValuePair<string, string>("authorization", "Basic secret") },
            calledRequest.Headers.Entries());
        Assert.Same(body, calledRequest.Body);
        Assert.Equal("error", calledRequest.Redirect);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the default native-fetch behavior without strict DNS validation", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Preserves_the_default_native_fetch_behavior_without_strict_DNS_validation()
    {
        var calls = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                calls++;
                return Task.FromResult(OkResponse());
            };
            await Download.FetchWithValidatedEndpointAsync("https://example.com/metadata");
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, calls);
    }

    private static DownloadResponse OkResponse()
    {
        return DownloadResponse.Bytes(null, "image/png");
    }
}
