// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

public sealed class ToolChoiceTests
{
    private const string Anthropic = "anthropic.claude-sonnet-4-5-20250929-v1:0";
    private const string Other = "meta.llama3-70b-instruct-v1:0";
    private const string CustomProfile =
        "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom-profile";

    private static LanguageModelTool Tool(string name, string description, string schema = "{}")
    {
        using var document = JsonDocument.Parse(schema);
        return new LanguageModelTool(name, description, document.RootElement.Clone());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields for a declared Anthropic application inference profile",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_use_Anthropic_tool_choice_fields_for_a_declared_Anthropic_application_inference_profile()
    {
        var config = AmazonBedrockTools.Prepare(
            CustomProfile,
            [Tool("testFunction", "Test")],
            null,
            AmazonBedrockChatModelFamily.Anthropic,
            null,
            true,
            false,
            out _,
            out var additionalTools,
            out _
        );

        Assert.Equal(
            "{\"tool_choice\":{\"type\":\"auto\",\"disable_parallel_tool_use\":true}}",
            additionalTools!.ToJsonString()
        );
        Assert.Null(config["toolChoice"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"auto\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Handles_tool_choice_auto()
    {
        var config = AmazonBedrockTools.Prepare(
            Other,
            new[] { Tool("testFunction", "Test") },
            ToolChoice.Auto,
            out _
        );
        Assert.Equal("{}", config["toolChoice"]!["auto"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"required\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Handles_tool_choice_required()
    {
        var config = AmazonBedrockTools.Prepare(
            Other,
            new[] { Tool("testFunction", "Test") },
            ToolChoice.Required,
            out _
        );
        Assert.Equal("{}", config["toolChoice"]!["any"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"none\" by clearing tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Clears_tools_when_tool_choice_is_none()
    {
        var config = AmazonBedrockTools.Prepare(
            Other,
            new[] { Tool("testFunction", "Test") },
            ToolChoice.None,
            out _
        );
        Assert.Equal("{}", config.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Handles_a_named_tool_choice()
    {
        var config = AmazonBedrockTools.Prepare(
            Other,
            new[] { Tool("testFunction", "Test") },
            ToolChoice.Tool("testFunction"),
            out _
        );
        Assert.Equal("testFunction", config["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should filter function tools to only the named tool when tool choice is \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Filters_function_tools_to_the_named_tool()
    {
        var tools = new[] { Tool("getWeather", "Get weather"), Tool("getTime", "Get time") };
        var config = AmazonBedrockTools.Prepare(Other, tools, ToolChoice.Tool("getWeather"), out _);
        var prepared = config["tools"]!.AsArray();
        Assert.Single(prepared);
        Assert.Equal("getWeather", prepared[0]!["toolSpec"]!["name"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(null, "auto", null)]
    [InlineData("auto", "auto", null)]
    [InlineData("required", "any", null)]
    [InlineData("tool", "tool", "testFunction")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields when parallel tool use is disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_use_Anthropic_tool_choice_fields_when_parallel_tool_use_is_disabled(
        string? choice,
        string expectedType,
        string? expectedName
    )
    {
        ToolChoice? toolChoice = choice switch
        {
            "auto" => ToolChoice.Auto,
            "required" => ToolChoice.Required,
            "tool" => ToolChoice.Tool("testFunction"),
            _ => null,
        };

        var config = AmazonBedrockTools.Prepare(
            Anthropic,
            [Tool("testFunction", "Test")],
            toolChoice,
            null,
            true,
            out _,
            out var additionalTools
        );

        var expected = new JsonObject { ["type"] = expectedType };
        if (expectedName != null)
        {
            expected["name"] = expectedName;
        }

        expected["disable_parallel_tool_use"] = true;
        Assert.True(
            JsonNode.DeepEquals(new JsonObject { ["tool_choice"] = expected }, additionalTools)
        );
        Assert.Null(config["toolChoice"]);
    }
}
