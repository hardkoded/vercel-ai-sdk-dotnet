// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; x_search</c>.</summary>
public sealed class PrepareResponsesToolsXSearchTests
{
    private const string Prefix = TestFile + " > x_search::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare x_search tool with no args", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_x_search_tool_with_no_args()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.x_search", "x_search") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"x_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare x_search tool with allowed handles", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_x_search_tool_with_allowed_handles()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.x_search", "x_search", """{"allowedXHandles":["elonmusk","xai"]}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"allowed_x_handles":["elonmusk","xai"],"type":"x_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare x_search tool with date range", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_x_search_tool_with_date_range()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.x_search", "x_search", """{"fromDate":"2025-01-01","toDate":"2025-12-31"}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"from_date":"2025-01-01","to_date":"2025-12-31","type":"x_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare x_search tool with video understanding", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_x_search_tool_with_video_understanding()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.x_search", "x_search", """{"enableVideoUnderstanding":true,"enableImageUnderstanding":true}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"enable_image_understanding":true,"enable_video_understanding":true,"type":"x_search"}]""");
    }
}
