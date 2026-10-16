// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class TrailingDotHostnamesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > trailing-dot hostnames::";

    [Fact]
    [UpstreamTest(Suite + "should block localhost. (trailing dot)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_localhost_trailing_dot()
    {
        AssertBlocked("http://localhost./file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block .local. (trailing dot)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_dot_local_trailing_dot()
    {
        AssertBlocked("http://myhost.local./file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block .localhost. (trailing dot)", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_dot_localhost_trailing_dot()
    {
        AssertBlocked("http://app.localhost./file");
    }

    [Fact]
    [UpstreamTest(Suite + "should still allow public hosts with a trailing dot", Coverage = UpstreamCoverage.Covered)]
    public void Should_still_allow_public_hosts_with_a_trailing_dot()
    {
        AssertAllowed("https://example.com./image.png");
    }
}
