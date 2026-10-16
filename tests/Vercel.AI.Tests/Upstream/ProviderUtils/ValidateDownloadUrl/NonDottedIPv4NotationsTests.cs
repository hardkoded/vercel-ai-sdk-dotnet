// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class NonDottedIPv4NotationsTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > non-dotted IPv4 notations::";

    [Fact]
    [UpstreamTest(Suite + "should block decimal 2130706433 (127.0.0.1)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_decimal_2130706433_127_0_0_1()
    {
        AssertBlocked("http://2130706433/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block hex 0x7f000001 (127.0.0.1)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_hex_0x7f000001_127_0_0_1()
    {
        AssertBlocked("http://0x7f000001/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block octal 0177.0.0.1 (127.0.0.1)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_octal_0177_0_0_1_127_0_0_1()
    {
        AssertBlocked("http://0177.0.0.1/file");
    }
}
