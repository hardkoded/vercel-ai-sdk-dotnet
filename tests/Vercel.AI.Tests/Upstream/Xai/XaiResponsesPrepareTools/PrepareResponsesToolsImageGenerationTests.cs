// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; image_generation</c>.</summary>
public sealed class PrepareResponsesToolsImageGenerationTests
{
    private const string Prefix = TestFile + " > image_generation::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare image_generation tool with no args", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_image_generation_tool_with_no_args()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.image_generation", "image_generation") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"image_generation"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare image_generation tool with action", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_image_generation_tool_with_action()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.image_generation", "image_generation", """{"action":"generate"}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"action":"generate","type":"image_generation"}]""");
    }
}
