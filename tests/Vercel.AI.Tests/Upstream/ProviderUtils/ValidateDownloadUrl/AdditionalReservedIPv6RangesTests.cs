// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class AdditionalReservedIPv6RangesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > additional reserved IPv6 ranges::";

    [Fact]
    [UpstreamTest(Suite + "should block fec0::/10 (site-local)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_fec0_10_site_local()
    {
        AssertBlocked("http://[fec0::1]/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block ff00::/8 (multicast)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_ff00_8_multicast()
    {
        AssertBlocked("http://[ff02::1]/file");
    }
}
