// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class DeferLoadingForFunctionToolsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > deferLoading for function tools::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include defer_loading when set to true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_defer_loading_when_set_to_true()
    {
        var result = Prepare([
            FunctionTool(
                "testFunction",
                "A test function",
                providerOptions: "{\"anthropic\":{\"deferLoading\":true}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13"],
            "[{\"defer_loading\":true,\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include defer_loading when set to false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_defer_loading_when_set_to_false()
    {
        var result = Prepare([
            FunctionTool(
                "testFunction",
                "A test function",
                providerOptions: "{\"anthropic\":{\"deferLoading\":false}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13"],
            "[{\"defer_loading\":false,\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"name\":\"testFunction\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include defer_loading when not specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_defer_loading_when_not_specified()
    {
        var result = Prepare([FunctionTool("testFunction", "A test function")]);

        Assert.Null(result.Tools![0]!["defer_loading"]);
    }
}
