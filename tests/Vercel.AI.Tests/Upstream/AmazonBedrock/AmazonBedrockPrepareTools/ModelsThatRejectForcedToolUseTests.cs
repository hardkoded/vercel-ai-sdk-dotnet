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
    private const string Opus55 = "us.anthropic.claude-opus-5-5";

    private static LanguageModelTool Function(string name, string description)
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\"}");
        return new LanguageModelTool(name, description, document.RootElement.Clone());
    }

    private static LanguageModelTool BashTool()
    {
        using var document = JsonDocument.Parse("{}");
        return LanguageModelTool.ProviderDefined(
            "anthropic.bash_20250124",
            "bash",
            document.RootElement.Clone()
        );
    }

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
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should fall back to auto for tool choice \"required\" on %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_fall_back_to_auto_for_tool_choice_required_on(string modelId)
    {
        var config = Prepare(
            modelId,
            [Function("getWeather", "Get weather")],
            ToolChoice.Required,
            null,
            true,
            out var warnings,
            out _
        );

        Assert.NotNull(config["toolChoice"]!["auto"]);
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
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should fall back to auto and send only the named function tool for tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_fall_back_to_auto_and_send_only_the_named_function_tool_for_tool_choice_tool()
    {
        var config = Prepare(
            Opus55,
            [Function("getWeather", "Get weather"), Function("getTime", "Get time")],
            ToolChoice.Tool("getWeather"),
            null,
            true,
            out var warnings,
            out var additionalTools
        );

        Assert.NotNull(config["toolChoice"]!["auto"]);
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
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should drop provider tools that are not the named tool for tool choice \"tool\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_drop_provider_tools_that_are_not_the_named_tool_for_tool_choice_tool()
    {
        var config = Prepare(
            Opus55,
            [Function("getWeather", "Get weather"), BashTool()],
            ToolChoice.Tool("getWeather"),
            null,
            true,
            out _,
            out var additionalTools
        );

        Assert.NotNull(config["toolChoice"]!["auto"]);
        Assert.Equal(["getWeather"], Names(config));
        Assert.Null(additionalTools);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should pass the forced-tool capability to Anthropic provider tool preparation",
        Coverage = UpstreamCoverage.Partial,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_pass_the_forced_tool_capability_to_Anthropic_provider_tool_preparation()
    {
        // Upstream spies on prepareAnthropicTools. The port reads the result of that call: with
        // rejectsForcedToolUse the Anthropic preparation falls back to auto for the named tool.
        _ = Prepare(
            Opus55,
            [BashTool()],
            ToolChoice.Tool("bash"),
            null,
            true,
            out var warnings,
            out var additionalTools
        );

        Assert.Equal("{\"tool_choice\":{\"type\":\"auto\"}}", additionalTools!.ToJsonString());
        Assert.Contains(warnings, warning => warning.Feature == "toolChoice");
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should preserve disabled parallel tool use in the auto fallback",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_preserve_disabled_parallel_tool_use_in_the_auto_fallback()
    {
        var config = Prepare(
            Opus55,
            [Function("getWeather", "Get weather")],
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
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice > models that reject forced tool use::should keep forced tool choice for models that support it",
        Coverage = UpstreamCoverage.Covered,
        Commit = "62a4dcd641895b3aca4698015c0378b2bd558c4e"
    )]
    public void Should_keep_forced_tool_choice_for_models_that_support_it()
    {
        var config = Prepare(
            "us.anthropic.claude-opus-5",
            [Function("getWeather", "Get weather")],
            ToolChoice.Required,
            null,
            false,
            out var warnings,
            out _
        );

        Assert.NotNull(config["toolChoice"]!["any"]);
        Assert.Empty(warnings);
    }
}
