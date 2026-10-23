// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using Vercel.AI.WebFetch;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c> &gt; <c>options</c>.</summary>
public sealed class OptionsTests
{
    private const string Prefix = WebFetchToolFixture.FilePath + "::createWebFetchTool > options::";

    [Fact]
    [UpstreamTest(Prefix + "rejects combining allowed and blocked domains", Coverage = UpstreamCoverage.Covered)]
    public void RejectsCombiningAllowedAndBlockedDomains()
    {
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions
        {
            AllowedDomains = new[] { "example.com" },
            BlockedDomains = new[] { "example.org" },
        }));
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("example.com/path")]
    [InlineData("*.example.com")]
    [InlineData("example.com:8080")]
    [InlineData("")]
    [UpstreamTest(Prefix + "rejects invalid domain %j", Coverage = UpstreamCoverage.Covered)]
    public void RejectsInvalidDomain(string domain)
    {
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions { AllowedDomains = new[] { domain } }));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects invalid option %j", Coverage = UpstreamCoverage.Partial, Note = "MaxLength is an int, so the fractional case { maxLength: 1.5 } cannot be written.")]
    public void RejectsInvalidOption()
    {
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions { MaxLength = 0 }));
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions { MaxBytes = -1 }));
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions { Timeout = TimeSpan.Zero }));
        Assert.Throws<InvalidArgumentError>(() => WebFetchTools.Create(new WebFetchToolOptions { Timeout = TimeSpan.FromMilliseconds(2_147_483_648) }));
    }

    [Fact]
    [UpstreamTest(Prefix + "describes the configured behavior", Coverage = UpstreamCoverage.Covered)]
    public void DescribesTheConfiguredBehavior()
    {
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions
        {
            AllowedDomains = new[] { "Example.COM" },
            MaxLength = 1000,
            RequireUrlInUserMessage = true,
        });

        Assert.Contains("Only these domains and their subdomains can be fetched: example.com.", webFetch.Description);
        Assert.Contains("Only URLs that appear in user messages can be fetched.", webFetch.Description);
    }
}
