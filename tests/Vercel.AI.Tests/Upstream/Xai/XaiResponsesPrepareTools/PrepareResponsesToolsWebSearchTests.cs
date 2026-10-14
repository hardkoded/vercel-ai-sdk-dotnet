// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; web_search</c>.</summary>
public sealed class PrepareResponsesToolsWebSearchTests
{
    private const string Prefix = TestFile + " > web_search::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare web_search tool with no args", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_no_args()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.web_search", "web_search") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"web_search"}]""");
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare web_search tool with allowed domains", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_allowed_domains()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.web_search", "web_search", """{"allowedDomains":["wikipedia.org","example.com"]}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"allowed_domains":["wikipedia.org","example.com"],"type":"web_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare web_search tool with excluded domains", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_excluded_domains()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.web_search", "web_search", """{"excludedDomains":["spam.com"]}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"excluded_domains":["spam.com"],"type":"web_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare web_search tool with image search", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_image_search()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.web_search", "web_search", """{"enableImageSearch":true}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"enable_image_search":true,"type":"web_search"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare web_search tool with image understanding", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_image_understanding()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.web_search", "web_search", """{"enableImageUnderstanding":true}""") }, null);

        JsonAssert.Equal(result.Tools, """[{"enable_image_understanding":true,"type":"web_search"}]""");
    }
}
