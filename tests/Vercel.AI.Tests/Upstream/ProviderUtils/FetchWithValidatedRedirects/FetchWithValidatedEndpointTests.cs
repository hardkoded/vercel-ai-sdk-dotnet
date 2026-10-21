// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.FetchWithValidatedRedirects;

/// <summary>Port of <c>fetch-with-validated-redirects.test.ts</c> &gt; <c>fetchWithValidatedEndpoint</c>.</summary>
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

    private static DownloadResponse OkResponse()
    {
        return DownloadResponse.Bytes(null, "image/png");
    }
}
