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
    private const string AnthropicModel = "anthropic.claude-sonnet-4-5-20250929-v1:0";
    private const string CustomProfile =
        "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom-profile";

    private static LanguageModelTool[] Tools()
    {
        using var document = JsonDocument.Parse("{}");
        return [new LanguageModelTool("testFunction", "Test", document.RootElement.Clone())];
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields for a declared Anthropic application inference profile",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_use_Anthropic_tool_choice_fields_for_a_declared_Anthropic_application_inference_profile()
    {
        var config = AmazonBedrockTools.Prepare(
            CustomProfile,
            Tools(),
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

    [Theory]
    [InlineData(null, "{\"type\":\"auto\",\"disable_parallel_tool_use\":true}")]
    [InlineData("auto", "{\"type\":\"auto\",\"disable_parallel_tool_use\":true}")]
    [InlineData("required", "{\"type\":\"any\",\"disable_parallel_tool_use\":true}")]
    [InlineData(
        "tool",
        "{\"type\":\"tool\",\"name\":\"testFunction\",\"disable_parallel_tool_use\":true}"
    )]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields when parallel tool use is disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_use_Anthropic_tool_choice_fields_when_parallel_tool_use_is_disabled(
        string? toolChoice,
        string expected
    )
    {
        ToolChoice? choice = toolChoice switch
        {
            "auto" => ToolChoice.Auto,
            "required" => ToolChoice.Required,
            "tool" => ToolChoice.Tool("testFunction"),
            _ => null,
        };

        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            Tools(),
            choice,
            null,
            true,
            out _,
            out var additionalTools
        );

        Assert.Equal("{\"tool_choice\":" + expected + "}", additionalTools!.ToJsonString());
        Assert.Null(config["toolChoice"]);
    }
}
