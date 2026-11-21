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
    private const string NonAnthropicModel = "meta.llama3-70b-instruct-v1:0";
    private const string AnthropicModel = "anthropic.claude-sonnet-4-5-20250929-v1:0";

    private static LanguageModelTool Tool(string name, string description)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\"}");
        return new LanguageModelTool(name, description, document.RootElement.Clone());
    }

    private static LanguageModelTool ProviderTool(string id, string name)
    {
        using var document = JsonDocument.Parse("{}");
        return LanguageModelTool.ProviderDefined(id, name, document.RootElement.Clone());
    }

    private static string[] Names(JsonObject config) =>
        config["tools"]!
            .AsArray()
            .Select(tool => tool!["toolSpec"]!["name"]!.GetValue<string>())
            .ToArray();

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn for provider-defined tools on non-anthropic models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_warn_for_provider_defined_tools_on_non_anthropic_models()
    {
        var config = AmazonBedrockTools.Prepare(
            NonAnthropicModel,
            [ProviderTool("some.custom_tool", "custom_tool")],
            null,
            null,
            null,
            null,
            false,
            out var warnings,
            out _,
            out _
        );

        Assert.Empty(config);
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("tool some.custom_tool", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn for unknown provider-defined tools on anthropic models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_warn_for_unknown_provider_defined_tools_on_anthropic_models()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.unknown_tool", "unknown_tool")],
            null,
            null,
            null,
            null,
            false,
            out var warnings,
            out _,
            out _
        );

        Assert.Empty(config);
        // Upstream mocks the Anthropic preparer, so only Bedrock's warning is there. The port runs the real
        // preparer, which adds its own warning for the unknown id, so the test looks for Bedrock's.
        Assert.All(warnings, warning => Assert.Equal("unsupported", warning.Type));
        Assert.Contains(warnings, warning => warning.Message == "tool anthropic.unknown_tool");
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn and filter out web_search_20250305 tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_warn_and_filter_out_web_search_20250305_tool()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.web_search_20250305", "web_search")],
            null,
            null,
            null,
            null,
            false,
            out var warnings,
            out _,
            out _
        );

        Assert.Empty(config);
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("web_search_20250305 tool", warning.Feature);
        Assert.Equal(
            "The web_search_20250305 tool is not supported on Amazon Bedrock.",
            warning.Message
        );
    }

    [Theory]
    [InlineData("anthropic.web_search_20260318")]
    [InlineData("anthropic.web_fetch_20260318")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn and filter out unsupported %s tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_warn_and_filter_out_unsupported_tool(string toolId)
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool(toolId, toolId.Contains("search") ? "web_search" : "web_fetch")],
            null,
            null,
            null,
            null,
            false,
            out var warnings,
            out _,
            out _
        );

        var toolType = toolId.Substring("anthropic.".Length);
        Assert.Empty(config);
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
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should return empty toolConfig when all tools are filtered out",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_return_empty_toolConfig_when_all_tools_are_filtered_out()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            [ProviderTool("anthropic.web_search_20250305", "web_search")],
            null,
            null,
            null,
            null,
            false,
            out _,
            out _,
            out _
        );

        Assert.Empty(config);
    }
}
