// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; unsupported tools</c>.</summary>
public sealed class PrepareResponsesToolsUnsupportedToolsTests
{
    private const string Prefix = TestFile + " > unsupported tools::";

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_about_unsupported_provider_defined_tools()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("unsupported.tool", "unsupported") }, null);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool unsupported", warning.Feature);
    }
}
