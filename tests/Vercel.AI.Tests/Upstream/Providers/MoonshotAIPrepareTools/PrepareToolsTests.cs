// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MoonshotAIPrepareTools;

/// <summary>Port of <c>moonshotai-prepare-tools.test.ts</c> &gt; <c>prepareTools</c>.</summary>
public sealed class PrepareToolsTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-prepare-tools.test.ts::prepareTools::";

    private static readonly LanguageModelTool[] Tools =
    [
        new LanguageModelTool(
            "testFunction",
            "Test function",
            JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}").RootElement.Clone()
        ),
    ];

    [Theory]
    [InlineData("kimi-k2.6")]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [UpstreamTest(
        Prefix + "should omit required tool choice and warn for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_required_tool_choice_and_warn_for_model(string modelId)
    {
        var result = MoonshotPrepareTools.PrepareTools(modelId, Tools, ToolChoice.Required);

        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal($"tool choice \"required\" for model \"{modelId}\"", warning.Feature);
        Assert.Equal(
            "Moonshot AI rejects required tool choice for this model. The setting has been omitted; use \"auto\" or select a specific tool instead.",
            warning.Message
        );
    }

    [Theory]
    [InlineData("kimi-k3")]
    [InlineData("custom-kimi-model")]
    [UpstreamTest(
        Prefix + "should preserve required tool choice for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_required_tool_choice_for_model(string modelId)
    {
        var result = MoonshotPrepareTools.PrepareTools(modelId, Tools, ToolChoice.Required);

        Assert.Equal("required", result.ToolChoice!.GetValue<string>());
        Assert.Empty(result.ToolWarnings);
    }

    [Theory]
    [InlineData("auto", "\"auto\"")]
    [InlineData("none", "\"none\"")]
    [InlineData("tool", "{\"type\":\"function\",\"function\":{\"name\":\"testFunction\"}}")]
    [UpstreamTest(
        Prefix + "should preserve tool choice %j for unsupported required-tool-choice models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_tool_choice_for_unsupported_required_tool_choice_models(
        string type,
        string expected
    )
    {
        var toolChoice = type switch
        {
            "auto" => ToolChoice.Auto,
            "none" => ToolChoice.None,
            _ => ToolChoice.Tool("testFunction"),
        };

        var result = MoonshotPrepareTools.PrepareTools("kimi-k2.6", Tools, toolChoice);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), result.ToolChoice));
        Assert.Empty(result.ToolWarnings);
    }
}
