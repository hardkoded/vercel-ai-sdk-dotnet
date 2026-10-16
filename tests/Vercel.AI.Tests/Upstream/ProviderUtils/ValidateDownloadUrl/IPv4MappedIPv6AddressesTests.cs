// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class IPv4MappedIPv6AddressesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > IPv4-mapped IPv6 addresses::";

    [Fact]
    [UpstreamTest(Suite + "should block ::ffff:127.0.0.1", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_ffff_127_0_0_1()
    {
        AssertBlocked("http://[::ffff:127.0.0.1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block ::ffff:10.0.0.1", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_ffff_10_0_0_1()
    {
        AssertBlocked("http://[::ffff:10.0.0.1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block ::ffff:169.254.169.254", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_ffff_169_254_169_254()
    {
        AssertBlocked("http://[::ffff:169.254.169.254]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow ::ffff: with public IP", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_ffff_with_public_IP()
    {
        AssertAllowed("http://[::ffff:8.8.8.8]/file");
    }
}
