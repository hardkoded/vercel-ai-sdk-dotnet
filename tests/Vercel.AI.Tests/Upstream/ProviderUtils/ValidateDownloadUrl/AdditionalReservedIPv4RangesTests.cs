// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class AdditionalReservedIPv4RangesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > additional reserved IPv4 ranges::";

    [Fact]
    [UpstreamTest(Suite + "should block 100.64.0.0/10 (CGNAT)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_100_64_0_0_10_CGNAT()
    {
        AssertBlocked("http://100.64.0.1/file");
        AssertBlocked("http://100.127.255.255/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should allow 100.63.x.x and 100.128.x.x (public)", Coverage = UpstreamCoverage.Covered)]
    public void Should_allow_100_63_x_x_and_100_128_x_x_public()
    {
        AssertAllowed("http://100.63.0.1/file");
        AssertAllowed("http://100.128.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 198.18.0.0/15 (benchmarking)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_198_18_0_0_15_benchmarking()
    {
        AssertBlocked("http://198.18.0.1/file");
        AssertBlocked("http://198.19.255.255/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 192.0.0.0/24 (IETF protocol assignments)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_192_0_0_0_24_IETF_protocol_assignments()
    {
        AssertBlocked("http://192.0.0.1/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block 240.0.0.0/4 (reserved) and the broadcast address", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_240_0_0_0_4_reserved_and_the_broadcast_address()
    {
        AssertBlocked("http://240.0.0.1/file");
        AssertBlocked("http://255.255.255.255/file");
    }
}
