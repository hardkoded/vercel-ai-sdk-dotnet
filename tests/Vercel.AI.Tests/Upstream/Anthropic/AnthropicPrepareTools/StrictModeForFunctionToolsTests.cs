// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class StrictModeForFunctionToolsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include beta but not strict property when strict is undefined and supportsStructuredOutput is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_beta_but_not_strict_property_when_strict_is_undefined_and_supportsStructuredOutput_is_true()
    {
        var result = Prepare([FunctionTool("testFunction", "A test function")]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13"],
            "[{\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should not include strict, emit warning, and not add beta when both supportsStructuredOutput and supportsStrictTools are false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_strict_emit_warning_and_not_add_beta_when_both_supportsStructuredOutput_and_supportsStrictTools_are_false()
    {
        var result = Prepare(
            [FunctionTool("testFunction", "A test function", strict: true)],
            false,
            false
        );

        Assert.Empty(result.Betas);
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("strict", warning.Feature);
        Assert.Equal(
            "Tool 'testFunction' has strict: true, but strict mode is not supported by this provider. The strict property will be ignored.",
            warning.Details
        );
        AssertTools(
            result,
            "[{\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include strict but not beta when supportsStructuredOutput is false but supportsStrictTools is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_strict_but_not_beta_when_supportsStructuredOutput_is_false_but_supportsStrictTools_is_true()
    {
        var result = Prepare(
            [FunctionTool("testFunction", "A test function", strict: true)],
            false,
            true
        );

        AssertSnapshot(
            result,
            [],
            "[{\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\",\"strict\":true}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include beta when strict is false and supportsStructuredOutput is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_beta_when_strict_is_false_and_supportsStructuredOutput_is_true()
    {
        var result = Prepare([FunctionTool("testFunction", "A test function", strict: false)]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13"],
            "[{\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\",\"strict\":false}]"
        );
    }
}
