// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; multiple tools</c>.</summary>
public sealed class PrepareResponsesToolsMultipleToolsTests
{
    private const string Prefix = TestFile + " > multiple tools::";

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple tools including provider-defined and functions", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_provider_defined_and_functions()
    {
        var result = XaiResponsesTools.Prepare(
            new[] { Function("calculator", "calculate numbers"), ProviderTool("xai.web_search", "web_search"), ProviderTool("xai.x_search", "x_search") },
            null);

        Assert.Equal(new[] { "function", "web_search", "x_search" }, Types(result));
    }
}
