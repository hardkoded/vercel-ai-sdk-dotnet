// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl.ValidateDownloadUrlAssert;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

public sealed class MalformedUrlsTests
{
    private const string Suite = "packages/provider-utils/src/validate-download-url.test.ts::validateDownloadUrl > malformed URLs::";

    [Fact]
    [UpstreamTest(Suite + "should block invalid URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_block_invalid_URLs()
    {
        AssertBlocked("not-a-url");
    }
}
