// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; view_image</c>.</summary>
public sealed class PrepareResponsesToolsViewImageTests
{
    private const string Prefix = TestFile + " > view_image::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare view_image tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_view_image_tool()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.view_image", "view_image") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"view_image"}]""");
    }
}
