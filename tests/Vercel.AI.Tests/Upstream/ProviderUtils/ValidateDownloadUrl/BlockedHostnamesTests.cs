// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class BlockedHostnamesTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > blocked hostnames::";

    [Fact]
    [UpstreamTest(Suite + "should block localhost", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_localhost()
    {
        AssertBlocked("http://localhost/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block localhost with port", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_localhost_with_port()
    {
        AssertBlocked("http://localhost:3000/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block .local domains", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_local_domains()
    {
        AssertBlocked("http://myhost.local/file");
    }

    [Fact]
    [UpstreamTest(Suite + "should block .localhost domains", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_localhost_domains()
    {
        AssertBlocked("http://app.localhost/file");
    }
}
