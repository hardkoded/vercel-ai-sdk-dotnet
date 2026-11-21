// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

public sealed class NamedToolChoiceTests
{
    private const string AnthropicModel = "anthropic.claude-sonnet-4-5-20250929-v1:0";
    private const string NonAnthropicModel = "meta.llama3-70b-instruct-v1:0";
    private const string CustomProfile =
        "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom-profile";

    private static LanguageModelTool Tool(string name, string description)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\"}");
        return new LanguageModelTool(name, description, document.RootElement.Clone());
    }

    private static string[] Names(JsonObject config) =>
        config["tools"]!
            .AsArray()
            .Select(tool => tool!["toolSpec"]!["name"]!.GetValue<string>())
            .ToArray();

    private static LanguageModelTool[] Tools() =>
        [Tool("getWeather", "Get weather"), Tool("getTime", "Get time")];

    [Theory]
    [InlineData(AnthropicModel)]
    [InlineData("us.anthropic.claude-haiku-4-5-20251001-v1:0")]
    [InlineData("us.anthropic.claude-opus-5")]
    [InlineData("anthropic.claude-3-haiku-20240307-v1:0")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > named tool choice::should send every function tool and disable parallel tool use on %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_every_function_tool_and_disable_parallel_tool_use_on(string modelId)
    {
        var config = AmazonBedrockTools.Prepare(
            modelId,
            Tools(),
            ToolChoice.Tool("getWeather"),
            null,
            null,
            out var warnings,
            out var additionalTools
        );

        Assert.Equal(["getWeather", "getTime"], Names(config));
        Assert.Equal(
            "{\"tool_choice\":{\"type\":\"tool\",\"name\":\"getWeather\",\"disable_parallel_tool_use\":true}}",
            additionalTools!.ToJsonString()
        );
        Assert.Null(config["toolChoice"]);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > named tool choice::should send every function tool for a declared Anthropic application inference profile",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_every_function_tool_for_a_declared_Anthropic_application_inference_profile()
    {
        // The port infers an Anthropic profile from the reasoning budget, as upstream's
        // detectAnthropicModel does; it has no `modelFamily` setting.
        var config = AmazonBedrockTools.Prepare(
            CustomProfile,
            Tools(),
            ToolChoice.Tool("getWeather"),
            1024,
            null,
            out _,
            out var additionalTools
        );

        Assert.Equal(["getWeather", "getTime"], Names(config));
        Assert.Equal(
            "{\"tool_choice\":{\"type\":\"tool\",\"name\":\"getWeather\",\"disable_parallel_tool_use\":true}}",
            additionalTools!.ToJsonString()
        );
        Assert.Null(config["toolChoice"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > named tool choice::should allow parallel tool calls when disableParallelToolUse is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_allow_parallel_tool_calls_when_disableParallelToolUse_is_false()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            Tools(),
            ToolChoice.Tool("getWeather"),
            null,
            false,
            out _,
            out var additionalTools
        );

        Assert.Equal(["getWeather", "getTime"], Names(config));
        Assert.Equal("getWeather", config["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
        Assert.Null(additionalTools);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > named tool choice::should send every function tool when parallel tool use is disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_every_function_tool_when_parallel_tool_use_is_disabled()
    {
        var config = AmazonBedrockTools.Prepare(
            AnthropicModel,
            Tools(),
            ToolChoice.Tool("getWeather"),
            null,
            true,
            out _,
            out var additionalTools
        );

        Assert.Equal(["getWeather", "getTime"], Names(config));
        Assert.Equal(
            "{\"tool_choice\":{\"type\":\"tool\",\"name\":\"getWeather\",\"disable_parallel_tool_use\":true}}",
            additionalTools!.ToJsonString()
        );
        Assert.Null(config["toolChoice"]);
    }

    [Theory]
    [InlineData("openai.gpt-oss-20b-1:0")]
    [InlineData("us.amazon.nova-pro-v1:0")]
    [InlineData("amazon.nova-2-lite-v1:0")]
    [InlineData(NonAnthropicModel)]
    [InlineData(CustomProfile)]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > named tool choice::should only send the named function tool on %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_only_send_the_named_function_tool_on(string modelId)
    {
        var config = AmazonBedrockTools.Prepare(
            modelId,
            Tools(),
            ToolChoice.Tool("getWeather"),
            null,
            null,
            out _,
            out var additionalTools
        );

        Assert.Equal(["getWeather"], Names(config));
        Assert.Equal("getWeather", config["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
        Assert.Null(additionalTools);
    }
}
