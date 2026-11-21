// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

public sealed class UnsupportedProviderDefinedToolsTests
{
    private const string AnthropicModel = "anthropic.claude-sonnet-4-5-20250929-v1:0";
    private const string NonAnthropicModel = "meta.llama3-70b-instruct-v1:0";
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::";

    private static LanguageModelTool ProviderTool(string id, string name) =>
        LanguageModelTool.ProviderDefined(id, name, JsonDocument.Parse("{}").RootElement.Clone());

    [Fact]
    [UpstreamTest(
        Prefix + "should warn for provider-defined tools on non-anthropic models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_warn_for_provider_defined_tools_on_non_anthropic_models()
    {
        var config = AmazonBedrockTools.Prepare(
            NonAnthropicModel,
            [ProviderTool("some.custom_tool", "custom_tool")],
            null,
            out var warnings
        );

        Assert.Equal("{}", config.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("tool some.custom_tool", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn for unknown provider-defined tools on anthropic models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_warn_for_unknown_provider_defined_tools_on_anthropic_models()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.unknown_tool", "unknown_tool")],
            null,
            out var warnings
        );

        Assert.Equal("{}", config.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("tool anthropic.unknown_tool", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and filter out web_search_20250305 tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_warn_and_filter_out_web_search_20250305_tool()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.web_search_20250305", "web_search")],
            null,
            out var warnings
        );

        Assert.Equal("{}", config.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("web_search_20250305 tool", warning.Feature);
        Assert.Equal(
            "The web_search_20250305 tool is not supported on Amazon Bedrock.",
            warning.Message
        );
    }

    [Theory]
    [InlineData("anthropic.web_search_20260318", "web_search")]
    [InlineData("anthropic.web_fetch_20260318", "web_fetch")]
    [UpstreamTest(
        Prefix + "should warn and filter out unsupported %s tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_warn_and_filter_out_unsupported_tool(string toolId, string name)
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool(toolId, name)],
            null,
            out var warnings
        );

        var toolType = toolId.Substring("anthropic.".Length);
        Assert.Equal("{}", config.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(toolType + " tool", warning.Feature);
        Assert.Equal(
            "The " + toolType + " tool is not supported on Amazon Bedrock.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return empty toolConfig when all tools are filtered out",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_return_empty_toolConfig_when_all_tools_are_filtered_out()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.web_search_20250305", "web_search")],
            null,
            out _
        );

        Assert.Equal("{}", config.ToJsonString());
    }
}
