// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

public sealed class ModelsThatRejectForcedToolUseTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::";
    private const string Opus55 = "us.anthropic.claude-opus-5-5";

    private static LanguageModelTool Tool(string name, string description)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\"}");
        return new LanguageModelTool(name, description, document.RootElement.Clone());
    }

    private static LanguageModelTool Bash(string name = "bash") =>
        LanguageModelTool.ProviderDefined(
            "anthropic.bash_20250124",
            name,
            JsonDocument.Parse("{}").RootElement.Clone()
        );

    private static string[] Names(JsonObject config) =>
        config["tools"]!
            .AsArray()
            .Select(tool => tool!["toolSpec"]!["name"]!.GetValue<string>())
            .ToArray();

    private static JsonObject Prepare(
        string modelId,
        LanguageModelTool[] tools,
        ToolChoice? toolChoice,
        bool? disableParallelToolUse,
        bool rejectsForcedToolUse,
        out List<CallWarning> warnings,
        out JsonObject? additionalTools
    ) =>
        AmazonBedrockTools.Prepare(
            modelId,
            tools,
            toolChoice,
            null,
            null,
            disableParallelToolUse,
            rejectsForcedToolUse,
            out warnings,
            out additionalTools,
            out _
        );

    [Theory]
    [InlineData("anthropic.claude-opus-5-5")]
    [InlineData("us.anthropic.claude-opus-5-5")]
    [InlineData("global.anthropic.claude-opus-5-5")]
    [InlineData("anthropic.claude-sonnet-5-5")]
    [InlineData("us.anthropic.claude-sonnet-5-5")]
    [UpstreamTest(
        Prefix + "should fall back to auto for tool choice \"required\" on %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_fall_back_to_auto_for_tool_choice_required_on(string modelId)
    {
        var config = Prepare(
            modelId,
            [Tool("getWeather", "Get weather")],
            ToolChoice.Required,
            null,
            true,
            out var warnings,
            out _
        );

        Assert.Equal("{}", config["toolChoice"]!["auto"]!.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Equal(
            "toolChoice 'required' is not supported by this model because it rejects forced tool use. "
                + "Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should fall back to auto and send only the named function tool for tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_fall_back_to_auto_and_send_only_the_named_function_tool_for_tool_choice_tool()
    {
        var config = Prepare(
            Opus55,
            [Tool("getWeather", "Get weather"), Tool("getTime", "Get time")],
            ToolChoice.Tool("getWeather"),
            null,
            true,
            out var warnings,
            out var additionalTools
        );

        Assert.Equal("{}", config["toolChoice"]!["auto"]!.ToJsonString());
        Assert.Equal(["getWeather"], Names(config));
        Assert.Null(additionalTools);
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Equal(
            "toolChoice 'tool' is not supported by this model because it rejects forced tool use. "
                + "Only the 'getWeather' tool is sent with 'auto' tool choice. "
                + "Instruct the model to use the tool in the prompt and verify that a tool call was made.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should drop provider tools that are not the named tool for tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_drop_provider_tools_that_are_not_the_named_tool_for_tool_choice_tool()
    {
        var config = Prepare(
            Opus55,
            [Tool("getWeather", "Get weather"), Bash()],
            ToolChoice.Tool("getWeather"),
            null,
            true,
            out _,
            out var additionalTools
        );

        Assert.Equal("{}", config["toolChoice"]!["auto"]!.ToJsonString());
        Assert.Equal(["getWeather"], Names(config));
        Assert.Null(additionalTools);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the forced-tool capability to Anthropic provider tool preparation",
        Coverage = UpstreamCoverage.Partial,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_pass_the_forced_tool_capability_to_Anthropic_provider_tool_preparation()
    {
        // Upstream mocks `prepareAnthropicTools` and asserts its arguments. The port asserts what
        // the real preparer returns for them: an `auto` tool choice and its warning.
        var config = Prepare(
            Opus55,
            [Bash()],
            ToolChoice.Tool("bash"),
            null,
            true,
            out var warnings,
            out var additionalTools
        );

        Assert.Equal(["bash"], Names(config));
        Assert.Equal("{\"tool_choice\":{\"type\":\"auto\"}}", additionalTools!.ToJsonString());
        var warning = Assert.Single(warnings);
        Assert.Equal("toolChoice", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve disabled parallel tool use in the auto fallback",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_preserve_disabled_parallel_tool_use_in_the_auto_fallback()
    {
        var config = Prepare(
            Opus55,
            [Tool("getWeather", "Get weather")],
            ToolChoice.Required,
            true,
            true,
            out _,
            out var additionalTools
        );

        Assert.Equal(
            "{\"tool_choice\":{\"type\":\"auto\",\"disable_parallel_tool_use\":true}}",
            additionalTools!.ToJsonString()
        );
        Assert.Null(config["toolChoice"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep forced tool choice for models that support it",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public void Should_keep_forced_tool_choice_for_models_that_support_it()
    {
        var config = Prepare(
            "us.anthropic.claude-opus-5",
            [Tool("getWeather", "Get weather")],
            ToolChoice.Required,
            null,
            false,
            out var warnings,
            out _
        );

        Assert.Equal("{}", config["toolChoice"]!["any"]!.ToJsonString());
        Assert.Empty(warnings);
    }
}
