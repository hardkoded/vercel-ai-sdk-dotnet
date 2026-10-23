// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaPrepareTools.AlibabaPrepareToolsSupport;
using PrepareTools = global::Vercel.AI.Alibaba.AlibabaPrepareTools;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaPrepareTools;

/// <summary>Port of <c>alibaba-prepare-tools.test.ts</c> &gt; <c>prepareTools &gt; strict mode</c>.</summary>
public sealed class PrepareToolsStrictModeTests
{
    private const string Prefix = TestFile + "::prepareTools > strict mode::";

    [Fact]
    [UpstreamTest(Prefix + "should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_when_strict_is_true()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "A test function", strict: true) }, null);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(result.Tools, """[{"function":{"description":"A test function","name":"testFunction","parameters":{"properties":{},"type":"object"},"strict":true},"type":"function"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_when_strict_is_false()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "A test function", strict: false) }, null);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(result.Tools, """[{"function":{"description":"A test function","name":"testFunction","parameters":{"properties":{},"type":"object"},"strict":false},"type":"function"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not include strict mode when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_include_strict_mode_when_strict_is_undefined()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "A test function") }, null);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(result.Tools, """[{"function":{"description":"A test function","name":"testFunction","parameters":{"properties":{},"type":"object"}},"type":"function"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        var result = PrepareTools.Prepare(
            new[]
            {
                Function("strictTool", "A strict tool", strict: true),
                Function("nonStrictTool", "A non-strict tool", strict: false),
                Function("defaultTool", "A tool without strict setting"),
            },
            null);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(
            result.Tools,
            """
            [
              {"function":{"description":"A strict tool","name":"strictTool","parameters":{"properties":{},"type":"object"},"strict":true},"type":"function"},
              {"function":{"description":"A non-strict tool","name":"nonStrictTool","parameters":{"properties":{},"type":"object"},"strict":false},"type":"function"},
              {"function":{"description":"A tool without strict setting","name":"defaultTool","parameters":{"properties":{},"type":"object"}},"type":"function"}
            ]
            """);
    }
}
