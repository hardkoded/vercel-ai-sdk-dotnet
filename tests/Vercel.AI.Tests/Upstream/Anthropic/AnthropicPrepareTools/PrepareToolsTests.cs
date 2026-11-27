// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class PrepareToolsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::";

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve tools with tool choice \"none\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_tools_with_tool_choice_none()
    {
        var tools = new List<AnthropicToolDefinition>
        {
            new AnthropicToolDefinition
            {
                Name = "testFunction",
                Description = "Test",
                InputSchema = AnthropicParity.Json("{}"),
            },
        };

        var result = AnthropicToolPreparer.Prepare(tools, "none", null, null, null, true, true);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "[{\"name\":\"testFunction\",\"description\":\"Test\",\"input_schema\":{}}]"
                ),
                result.Tools
            )
        );
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"none\"}"), result.ToolChoice));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "does not add the strict-tool beta for providers that only support native final output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46"
    )]
    public void Does_not_add_the_strict_tool_beta_for_providers_that_only_support_native_final_output()
    {
        var tools = new List<AnthropicToolDefinition>
        {
            new AnthropicToolDefinition
            {
                Name = "report",
                InputSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}"),
                Strict = true,
            },
        };

        var result = AnthropicToolPreparer.Prepare(tools, null, null, null, null, true, false);

        Assert.Empty(result.Betas);
        Assert.Null(result.Tools![0]!["strict"]);
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "strict"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined tools and tool_choice when tools are null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_tools_and_tool_choice_when_tools_are_null()
    {
        var result = Prepare(null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined tools and tool_choice when tools are empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_tools_and_tool_choice_when_tools_are_empty()
    {
        var result = Prepare([]);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare function tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_function_tools()
    {
        var result = Prepare([
            FunctionTool(
                "testFunction",
                "A test function",
                providerOptions: "{\"anthropic\":{\"eagerInputStreaming\":true}}"
            ),
        ]);

        AssertTools(
            result,
            "[{\"name\":\"testFunction\",\"description\":\"A test function\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"eager_input_streaming\":true}]"
        );
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly preserve tool input examples",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_preserve_tool_input_examples()
    {
        var result = Prepare([
            FunctionTool(
                "tool_with_examples",
                "tool with examples",
                "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"}}}",
                inputExamples: "[{\"input\":{\"a\":1}},{\"input\":{\"a\":2}}]"
            ),
        ]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13", "advanced-tool-use-2025-11-20"],
            "[{\"description\":\"tool with examples\",\"input_examples\":[{\"a\":1},{\"a\":2}],\"input_schema\":{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"}}},\"name\":\"tool_with_examples\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add warnings for unsupported tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_warnings_for_unsupported_tools()
    {
        var result = Prepare([ProviderTool("unsupported.tool", "unsupported_tool", "{}")]);

        Assert.Empty(result.Tools!);
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool unsupported.tool", warning.Feature);
        Assert.Null(warning.Details);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should limit cache breakpoints to 4",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_limit_cache_breakpoints_to_4()
    {
        var cacheControlValidator = new AnthropicCacheControlValidator();
        const string cached = "{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}";
        var result = Prepare(
            [
                FunctionTool("tool1", "Test 1", "{}", cached),
                FunctionTool("tool2", "Test 2", "{}", cached),
                FunctionTool("tool3", "Test 3", "{}", cached),
                FunctionTool("tool4", "Test 4", "{}", cached),
                FunctionTool("tool5", "Test 5 (should be rejected)", "{}", cached),
            ],
            cacheControl: cacheControlValidator
        );

        var ephemeral = JsonNode.Parse("{\"type\":\"ephemeral\"}");
        for (var index = 0; index < 4; index++)
        {
            Assert.True(JsonNode.DeepEquals(ephemeral, result.Tools![index]!["cache_control"]));
        }

        Assert.Null(result.Tools![4]!["cache_control"]);
        var warning = Assert.Single(cacheControlValidator.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("cacheControl breakpoint limit", warning.Feature);
        Assert.Equal(
            "Maximum 4 cache breakpoints exceeded (found 5). This breakpoint will be ignored.",
            warning.Details
        );
    }
}
