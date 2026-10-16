// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class BlockedIPv6AddressesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > blocked IPv6 addresses::";

    [Fact]
    [UpstreamTest(Suite + "should block ::1 (loopback)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_1_loopback()
    {
        AssertBlocked("http://[::1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block :: (unspecified)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_unspecified()
    {
        AssertBlocked("http://[::]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block fc00::/7 (unique local)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_fc00_7_unique_local()
    {
        AssertBlocked("http://[fc00::1]/file");
        AssertBlocked("http://[fd12::1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block fe80::/10 (link-local)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_fe80_10_link_local()
    {
        AssertBlocked("http://[fe80::1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 2001:db8::/32 (documentation)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_2001_db8_32_documentation()
    {
        AssertBlocked("http://[2001:db8::1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 3fff::/20 (documentation, RFC 9637)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_3fff_20_documentation_RFC_9637()
    {
        AssertBlocked("http://[3fff::1]/file");
        AssertBlocked("http://[3fff:fff::1]/file");
        // 3fff:1000::/20 is outside the documentation range.
        AssertAllowed("http://[3fff:1000::1]/file");
    }
}
