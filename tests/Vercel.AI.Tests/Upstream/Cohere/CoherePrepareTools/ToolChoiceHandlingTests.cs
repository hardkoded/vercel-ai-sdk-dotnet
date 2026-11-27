// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Prepare = Vercel.AI.Cohere.CoherePrepareTools;

namespace Vercel.AI.Tests.Upstream.Cohere.CoherePrepareTools;

/// <summary>Port of <c>cohere-prepare-tools.test.ts</c> &gt; <c>tool choice handling</c>.</summary>
public sealed class ToolChoiceHandlingTests
{
    private const string Prefix =
        "packages/cohere/src/cohere-prepare-tools.test.ts::tool choice handling::";

    [Fact]
    [UpstreamTest(Prefix + "should handle auto tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_auto_tool_choice()
    {
        var result = Prepare.Prepare(
            new[] { CoherePrepareToolsTests.BasicTool() },
            ToolChoice.Auto
        );

        Assert.Null(result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle none tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_none_tool_choice()
    {
        var result = Prepare.Prepare(
            new[] { CoherePrepareToolsTests.BasicTool() },
            ToolChoice.None
        );

        JsonAssert.Equal(result.Tools, CoherePrepareToolsTests.ExpectedTools);
        Assert.Equal("NONE", result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle required tool choice",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_required_tool_choice()
    {
        var result = Prepare.Prepare(
            new[] { CoherePrepareToolsTests.BasicTool() },
            ToolChoice.Required
        );

        JsonAssert.Equal(result.Tools, CoherePrepareToolsTests.ExpectedTools);
        Assert.Equal("REQUIRED", result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool type tool choice by filtering tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_type_tool_choice_by_filtering_tools()
    {
        var result = Prepare.Prepare(
            new[] { CoherePrepareToolsTests.BasicTool() },
            ToolChoice.Tool("testFunction")
        );

        JsonAssert.Equal(result.Tools, CoherePrepareToolsTests.ExpectedTools);
        Assert.Equal("REQUIRED", result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }
}
