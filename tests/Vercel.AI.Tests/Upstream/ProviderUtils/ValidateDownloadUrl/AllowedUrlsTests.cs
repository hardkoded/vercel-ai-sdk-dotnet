// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class AllowedUrlsTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > allowed URLs::";

    [Fact]
    [UpstreamTest(Suite + "should allow https URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_https_URLs()
    {
        AssertAllowed("https://example.com/image.png");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow http URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_http_URLs()
    {
        AssertAllowed("http://example.com/image.png");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow public IP addresses", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_public_IP_addresses()
    {
        // 8.8.8.8 — a genuinely public, globally-routable address. The TEST-NET
        // documentation ranges (192.0.2/24, 198.51.100/24, 203.0.113/24) are
        // blocked.
        AssertAllowed("https://8.8.8.8/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow URLs with ports", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_URLs_with_ports()
    {
        AssertAllowed("https://example.com:8080/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow data URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_data_URLs()
    {
        AssertAllowed("data:text/plain;base64,aGVsbG8=");
    }
}
