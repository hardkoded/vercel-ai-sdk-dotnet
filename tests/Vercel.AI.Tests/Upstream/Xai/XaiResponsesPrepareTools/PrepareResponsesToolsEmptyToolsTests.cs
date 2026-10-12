// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; empty tools</c>.</summary>
public sealed class PrepareResponsesToolsEmptyToolsTests
{
    private const string Prefix = TestFile + " > empty tools::";

    [Fact]
    [UpstreamTest(Prefix + "should return undefined for empty tools array", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_empty_tools_array()
    {
        var result = XaiResponsesTools.Prepare(Array.Empty<LanguageModelTool>(), null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined for undefined tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_undefined_tools()
    {
        var result = XaiResponsesTools.Prepare(null, null);

        Assert.Null(result.Tools);
    }
}
