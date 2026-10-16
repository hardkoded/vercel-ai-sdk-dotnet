// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class BlockedIPv4AddressesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > blocked IPv4 addresses::";

    [Fact]
    [UpstreamTest(Suite + "should block 127.0.0.1 (loopback)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_127_0_0_1_loopback()
    {
        AssertBlocked("http://127.0.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 127.x.x.x range", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_127_x_x_x_range()
    {
        AssertBlocked("http://127.255.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 10.x.x.x (private)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_10_x_x_x_private()
    {
        AssertBlocked("http://10.0.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 172.16.x.x - 172.31.x.x (private)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_172_16_x_x_172_31_x_x_private()
    {
        AssertBlocked("http://172.16.0.1/file");
        AssertBlocked("http://172.31.255.255/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow 172.15.x.x and 172.32.x.x (public)", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_172_15_x_x_and_172_32_x_x_public()
    {
        AssertAllowed("http://172.15.0.1/file");
        AssertAllowed("http://172.32.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 192.168.x.x (private)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_192_168_x_x_private()
    {
        AssertBlocked("http://192.168.1.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 169.254.x.x (link-local / cloud metadata)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_169_254_x_x_link_local_cloud_metadata()
    {
        AssertBlocked("http://169.254.169.254/latest/meta-data/");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 0.0.0.0", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_0_0_0_0()
    {
        AssertBlocked("http://0.0.0.0/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 224.0.0.0/4 (multicast)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_224_0_0_0_4_multicast()
    {
        AssertBlocked("http://224.0.0.1/file");
        AssertBlocked("http://239.255.255.250/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block the TEST-NET documentation ranges", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_the_TEST_NET_documentation_ranges()
    {
        // TEST-NET-1 (192.0.2.0/24)
        AssertBlocked("http://192.0.2.1/file");
        // TEST-NET-2 (198.51.100.0/24)
        AssertBlocked("http://198.51.100.1/file");
        // TEST-NET-3 (203.0.113.0/24)
        AssertBlocked("http://203.0.113.1/file");
        // Neighboring public /24s stay allowed.
        AssertAllowed("http://192.0.3.1/file");
        AssertAllowed("http://198.51.101.1/file");
        AssertAllowed("http://203.0.114.1/file");
    }
}
