// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; tool choice</c>.</summary>
public sealed class PrepareResponsesToolsToolChoiceTests
{
    private const string Prefix = TestFile + " > tool choice::";
    private const string ProviderToolNote = "The case uses a function tool: the port has no provider-defined xAI tools.";

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice auto", Coverage = UpstreamCoverage.Partial, Note = ProviderToolNote)]
    public void Should_handle_tool_choice_auto()
    {
        var result = XaiResponsesTools.Prepare(new[] { Function("web_search", "search") }, ToolChoice.Auto);

        Assert.Equal("auto", (string?)result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice required", Coverage = UpstreamCoverage.Partial, Note = ProviderToolNote)]
    public void Should_handle_tool_choice_required()
    {
        var result = XaiResponsesTools.Prepare(new[] { Function("web_search", "search") }, ToolChoice.Required);

        Assert.Equal("required", (string?)result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice none", Coverage = UpstreamCoverage.Partial, Note = ProviderToolNote)]
    public void Should_handle_tool_choice_none()
    {
        var result = XaiResponsesTools.Prepare(new[] { Function("web_search", "search") }, ToolChoice.None);

        Assert.Equal("none", (string?)result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle specific tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_specific_tool_choice()
    {
        var result = XaiResponsesTools.Prepare(new[] { Function("weather", "get weather") }, ToolChoice.Tool("weather"));

        JsonAssert.Equal(result.ToolChoice, """{"type":"function","name":"weather"}""");
    }
}
