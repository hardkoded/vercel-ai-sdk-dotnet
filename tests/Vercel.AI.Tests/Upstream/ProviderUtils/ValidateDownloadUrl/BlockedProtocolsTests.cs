// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class BlockedProtocolsTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > blocked protocols::";

    [Fact]
    [UpstreamTest(Suite + "should block file:// URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_file_URLs()
    {
        AssertBlocked("file:///etc/passwd");
    }

    [Fact]
    [UpstreamTest(Suite + "should block ftp:// URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_ftp_URLs()
    {
        AssertBlocked("ftp://example.com/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block javascript: URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_javascript_URLs()
    {
        AssertBlocked("javascript:alert(1)");
    }
}
