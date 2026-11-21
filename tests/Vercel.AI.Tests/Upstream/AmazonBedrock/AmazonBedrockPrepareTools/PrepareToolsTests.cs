// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

public sealed class PrepareToolsTests
{
    [Theory]
    [InlineData("us.anthropic.claude-opus-6-v1:0")]
    [InlineData("global.anthropic.claude-future-v1:0")]
    [InlineData("us.anthropic.claude-opus-4-7")]
    [InlineData("anthropic.claude-opus-4-8")]
    [InlineData("eu.anthropic.claude-fable-5")]
    [InlineData("anthropic.claude-fable-5-1")]
    [InlineData("anthropic.claude-fable-5-2")]
    [InlineData("us.anthropic.claude-fable-5-1")]
    [InlineData("us.anthropic.claude-fable-5-2")]
    [InlineData("global.anthropic.claude-fable-5-1")]
    [InlineData("global.anthropic.claude-fable-5-2")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should warn when strict is omitted for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_warn_when_strict_is_omitted_for(string modelId)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}");
        var tool = new LanguageModelTool(
            "testFunction",
            "A test function",
            document.RootElement.Clone(),
            true
        );
        var config = AmazonBedrockTools.Prepare(modelId, new[] { tool }, null, out var warnings);
        Assert.Null(config["tools"]![0]!["toolSpec"]!["strict"]);
        Assert.Single(warnings);
        Assert.Contains(
            "Tool 'testFunction' has strict: true, but strict mode is not supported by this model on Amazon Bedrock. The strict property will be ignored.",
            warnings[0].Message
        );
    }

    [Theory]
    [InlineData("anthropic.claude-sonnet-5")]
    [InlineData("us.anthropic.claude-opus-5")]
    [InlineData("global.anthropic.claude-sonnet-5-5")]
    [InlineData("global.anthropic.claude-haiku-5-5")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass strict: true for Claude 5 model %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3"
    )]
    public void Should_pass_strict_true_for_Claude_5_model(string modelId)
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}"
        );
        var tool = new LanguageModelTool(
            "strictTool",
            "A strict tool",
            document.RootElement.Clone(),
            true
        );

        var config = AmazonBedrockTools.Prepare(modelId, new[] { tool }, null, out var warnings);

        Assert.True(config["tools"]![0]!["toolSpec"]!["strict"]!.GetValue<bool>());
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode for multiple tools with different strict settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        using var strictSchema = JsonDocument.Parse(
            "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}"
        );
        using var looseSchema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}");
        var tools = new[]
        {
            new LanguageModelTool("strictTool", "A strict tool", strictSchema.RootElement.Clone(), true),
            new LanguageModelTool("nonStrictTool", "A non-strict tool", looseSchema.RootElement.Clone(), false),
            new LanguageModelTool("defaultTool", "A tool without strict setting", looseSchema.RootElement.Clone()),
        };

        var config = AmazonBedrockTools.Prepare(
            "anthropic.claude-sonnet-4-5-20250929-v1:0",
            tools,
            null,
            out _
        );

        var specs = config["tools"]!.AsArray();
        Assert.True(specs[0]!["toolSpec"]!["strict"]!.GetValue<bool>());
        Assert.False(specs[1]!["toolSpec"]!["strict"]!.GetValue<bool>());
        Assert.False(((System.Text.Json.Nodes.JsonObject)specs[2]!["toolSpec"]!).ContainsKey("strict"));
    }
}
