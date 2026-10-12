// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; view_x_video</c>.</summary>
public sealed class PrepareResponsesToolsViewXVideoTests
{
    private const string Prefix = TestFile + " > view_x_video::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare view_x_video tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_view_x_video_tool()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.view_x_video", "view_x_video") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"view_x_video"}]""");
    }
}
