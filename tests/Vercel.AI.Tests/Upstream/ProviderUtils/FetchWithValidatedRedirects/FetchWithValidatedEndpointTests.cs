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
