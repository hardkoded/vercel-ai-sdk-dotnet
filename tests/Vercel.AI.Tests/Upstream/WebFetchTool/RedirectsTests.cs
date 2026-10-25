// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using Vercel.AI.WebFetch;
using static Vercel.AI.Tests.Upstream.WebFetchTool.WebFetchToolFixture;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>redirects</c>.</summary>
public sealed class RedirectsTests
{
    private const string Prefix = FilePath + "::createWebFetchTool > redirects::";

    [Fact]
    [UpstreamTest(Prefix + "follows redirects and reports the final URL", Coverage = UpstreamCoverage.Covered)]
    public async Task FollowsRedirectsAndReportsTheFinalUrl()
    {
        var fetch = new RecordingFetch((url, _) => url == "https://example.com/old" ? Redirect(301, "/new") : DownloadResponse.Text("moved content"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/old");

        Assert.True(IsDocument(output));
        Assert.Equal("https://example.com/old", output.GetProperty("url").GetString());
        Assert.Equal("https://example.com/new", output.GetProperty("finalUrl").GetString());
        Assert.Equal("moved content", Content(output));
        Assert.Equal("manual", fetch.Calls[0].Request.Redirect);
    }

    [Fact]
    [UpstreamTest(Prefix + "applies the domain filter to redirects", Coverage = UpstreamCoverage.Covered)]
    public async Task AppliesTheDomainFilterToRedirects()
    {
        var fetch = new RecordingFetch((_, _) => Redirect(302, "https://evil.org/"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch, AllowedDomains = new[] { "example.com" } });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("url_not_allowed", ErrorCode(output));
        Assert.Equal("The URL redirected to a domain that is not allowed.", output.GetProperty("message").GetString());
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "blocks redirects to private addresses", Coverage = UpstreamCoverage.Covered)]
    public async Task BlocksRedirectsToPrivateAddresses()
    {
        var fetch = new RecordingFetch((_, _) => Redirect(307, "http://169.254.169.254/"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("url_not_allowed", ErrorCode(output));
        Assert.Single(fetch.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "limits the number of redirects", Coverage = UpstreamCoverage.Covered)]
    public async Task LimitsTheNumberOfRedirects()
    {
        var count = 0;
        var fetch = new RecordingFetch((_, _) => Redirect(302, "/redirect-" + ++count));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });

        var output = await ExecuteAsync(webFetch, "https://example.com/");

        Assert.Equal("too_many_redirects", ErrorCode(output));
    }
}
