// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Converse toolConfig preparation.</summary>
public sealed class BedrockPrepareToolsTests
{
    private const string Anthropic = "anthropic.claude-sonnet-4-5-20250929-v1:0";

    private const string Other = "meta.llama3-70b-instruct-v1:0";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should return empty toolConfig when tools are undefined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_config_when_tools_are_missing()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(null, null, Anthropic);

        Assert.Empty(prepared.ToolConfig);
        Assert.Null(prepared.AdditionalTools);
        Assert.Empty(prepared.Betas);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should return empty toolConfig when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_config_when_tools_are_empty()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(Array.Empty<AmazonBedrockToolInput>(), null, Anthropic);

        Assert.Empty(prepared.ToolConfig);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Builds_a_tool_spec_for_a_function()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("testFunction", "A test function", BedrockParity.Node("{\"type\":\"object\",\"properties\":{}}")) },
            null,
            Anthropic);
        var spec = Spec(prepared, 0);

        Assert.Equal("testFunction", spec["name"]!.GetValue<string>());
        Assert.Equal("A test function", spec["description"]!.GetValue<string>());
        Assert.Equal("object", spec["inputSchema"]!["json"]!["type"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should exclude description when it is empty string", Coverage = UpstreamCoverage.Covered)]
    public void Omits_an_empty_description()
    {
        var spec = Spec(Prepare("testFunction", string.Empty), 0);

        Assert.Null(spec["description"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should exclude description when it is whitespace-only", Coverage = UpstreamCoverage.Covered)]
    public void Omits_a_whitespace_description()
    {
        var spec = Spec(Prepare("testFunction", "   "), 0);

        Assert.Null(spec["description"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should include description when it has content", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_description_that_has_content()
    {
        var spec = Spec(Prepare("testFunction", "Valid description"), 0);

        Assert.Equal("Valid description", spec["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn for provider-defined tools on non-anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_and_drops_provider_tools_on_non_anthropic_models()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.ProviderTool("some.custom_tool", "custom_tool") },
            null,
            Other);

        Assert.Empty(prepared.ToolConfig);
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "tool some.custom_tool", null));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn and filter out web_search_20250305 tool", Coverage = UpstreamCoverage.Covered)]
    public void Filters_the_anthropic_web_search_tool()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.ProviderTool("anthropic.web_search_20250305", "web_search") },
            null,
            Anthropic);

        Assert.Empty(prepared.ToolConfig);
        Assert.True(BedrockParity.HasWarning(
            prepared.Warnings,
            "unsupported",
            "web_search_20250305 tool",
            "The web_search_20250305 tool is not supported on Amazon Bedrock."));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should warn and filter out unsupported %s tool", Coverage = UpstreamCoverage.Covered)]
    public void Filters_later_web_search_and_web_fetch_tools()
    {
        foreach (var toolId in new[] { "anthropic.web_search_20260318", "anthropic.web_fetch_20260318" })
        {
            var toolType = toolId.Substring("anthropic.".Length);
            var prepared = AmazonBedrockPrepareTools.Prepare(
                new[] { AmazonBedrockToolInput.ProviderTool(toolId, toolId) },
                null,
                Anthropic);

            Assert.Empty(prepared.ToolConfig);
            Assert.True(BedrockParity.HasWarning(
                prepared.Warnings,
                "unsupported",
                toolType + " tool",
                "The " + toolType + " tool is not supported on Amazon Bedrock."));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > unsupported provider-defined tools::should return empty toolConfig when all tools are filtered out", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_config_when_every_tool_is_filtered()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.ProviderTool("anthropic.web_search_20250305", "web_search") },
            null,
            Anthropic);

        Assert.Empty(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields for a declared Anthropic application inference profile", Coverage = UpstreamCoverage.Covered)]
    public void Uses_anthropic_tool_choice_for_an_application_inference_profile()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("testFunction", "Test", new JsonObject()) },
            null,
            "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom-profile",
            "anthropic",
            null,
            true);
        var choice = prepared.AdditionalTools!["tool_choice"]!.AsObject();

        Assert.Equal("auto", choice["type"]!.GetValue<string>());
        Assert.True(choice["disable_parallel_tool_use"]!.GetValue<bool>());
        Assert.Null(prepared.ToolConfig["toolChoice"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_auto_tool_choice()
    {
        var prepared = Prepare("testFunction", "Test", ToolChoice.Auto, Other);

        Assert.Empty(prepared.ToolConfig["toolChoice"]!["auto"]!.AsObject());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_required_tool_choice_to_any()
    {
        var prepared = Prepare("testFunction", "Test", ToolChoice.Required, Other);

        Assert.Empty(prepared.ToolConfig["toolChoice"]!["any"]!.AsObject());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"none\" by clearing tools", Coverage = UpstreamCoverage.Covered)]
    public void Clears_tools_when_choice_is_none()
    {
        var prepared = Prepare("testFunction", "Test", ToolChoice.None, Other);

        Assert.Empty(prepared.ToolConfig);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_named_tool_choice()
    {
        var prepared = Prepare("testFunction", "Test", ToolChoice.Tool("testFunction"), Other);

        Assert.Equal("testFunction", prepared.ToolConfig["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should use Anthropic tool choice fields when parallel tool use is disabled", Coverage = UpstreamCoverage.Covered)]
    public void Sends_anthropic_tool_choice_when_parallel_use_is_disabled()
    {
        AssertAnthropicChoice(null, "auto", null);
        AssertAnthropicChoice(ToolChoice.Auto, "auto", null);
        AssertAnthropicChoice(ToolChoice.Required, "any", null);
        AssertAnthropicChoice(ToolChoice.Tool("testFunction"), "tool", "testFunction");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should filter function tools to only the named tool when tool choice is \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Sends_only_the_named_function_tool()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[]
            {
                AmazonBedrockToolInput.Function("getWeather", "Get weather", BedrockParity.Node("{\"type\":\"object\"}")),
                AmazonBedrockToolInput.Function("getTime", "Get time", BedrockParity.Node("{\"type\":\"object\"}")),
            },
            ToolChoice.Tool("getWeather"),
            Other);

        Assert.Single(prepared.ToolConfig["tools"]!.AsArray());
        Assert.Equal("getWeather", Spec(prepared, 0)["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_true_for_a_closed_schema()
    {
        var prepared = PrepareStrict(true, "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}");
        var spec = Spec(prepared, 0);

        Assert.True(spec["strict"]!.GetValue<bool>());
        Assert.False(spec["inputSchema"]!["json"]!["additionalProperties"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should keep strict mode enabled for %s", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_strict_for_sonnet_4_6_and_haiku_4_5()
    {
        foreach (var modelId in new[] { "anthropic.claude-sonnet-4-6-v1", "us.anthropic.claude-haiku-4-5-20251001-v1:0" })
        {
            var prepared = AmazonBedrockPrepareTools.Prepare(
                new[] { AmazonBedrockToolInput.Function("testFunction", "A test function", BedrockParity.Node("{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}"), true) },
                null,
                modelId);

            Assert.True(Spec(prepared, 0)["strict"]!.GetValue<bool>());
            Assert.Empty(prepared.Warnings);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should omit strict mode when the top-level object schema is open", Coverage = UpstreamCoverage.Covered)]
    public void Omits_strict_when_the_top_level_object_is_open()
    {
        var prepared = PrepareStrict(true, "{\"type\":\"object\",\"properties\":{}}");

        Assert.Null(Spec(prepared, 0)["strict"]);
        Assert.True(BedrockParity.HasWarning(
            prepared.Warnings,
            "unsupported",
            "strict",
            "Tool 'testFunction' has strict: true, but Amazon Bedrock requires every object in a strict tool schema to set additionalProperties: false. The strict property will be ignored."));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should omit strict mode when a nested object schema is open", Coverage = UpstreamCoverage.Covered)]
    public void Omits_strict_when_a_nested_object_is_open()
    {
        var schema = "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}},\"required\":[\"location\"],\"additionalProperties\":false}";
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("getWeather", null, BedrockParity.Node(schema), true) },
            null,
            Anthropic);

        Assert.Null(Spec(prepared, 0)["strict"]);
        Assert.Equal("getWeather", Spec(prepared, 0)["name"]!.GetValue<string>());
        Assert.Contains("getWeather", prepared.Warnings[0].Details ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should omit strict mode when a referenced object schema in $defs is open", Coverage = UpstreamCoverage.Covered)]
    public void Omits_strict_when_a_definition_object_is_open()
    {
        var schema = "{\"$ref\":\"#/$defs/location\",\"$defs\":{\"location\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}}}";
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("getWeather", null, BedrockParity.Node(schema), true) },
            null,
            Anthropic);

        Assert.Null(Spec(prepared, 0)["strict"]);
        Assert.Equal("object", Spec(prepared, 0)["inputSchema"]!["json"]!["$defs"]!["location"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when all nested object schemas are closed", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_when_every_object_is_closed()
    {
        var schema = "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}},\"required\":[\"location\"],\"additionalProperties\":false}";
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("getWeather", null, BedrockParity.Node(schema), true) },
            null,
            Anthropic);

        Assert.True(Spec(prepared, 0)["strict"]!.GetValue<bool>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_false()
    {
        var prepared = PrepareStrict(false, "{\"type\":\"object\",\"properties\":{}}");

        Assert.False(Spec(prepared, 0)["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should not include strict when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Omits_strict_when_it_is_unset()
    {
        var spec = Spec(Prepare("testFunction", "A test function"), 0);

        Assert.Null(spec["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_each_tools_strict_setting()
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[]
            {
                AmazonBedrockToolInput.Function("strictTool", "A strict tool", BedrockParity.Node("{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}"), true),
                AmazonBedrockToolInput.Function("nonStrictTool", "A non-strict tool", BedrockParity.Node("{\"type\":\"object\",\"properties\":{}}"), false),
                AmazonBedrockToolInput.Function("defaultTool", "A tool without strict setting", BedrockParity.Node("{\"type\":\"object\",\"properties\":{}}")),
            },
            null,
            Anthropic);

        Assert.True(Spec(prepared, 0)["strict"]!.GetValue<bool>());
        Assert.False(Spec(prepared, 1)["strict"]!.GetValue<bool>());
        Assert.Null(Spec(prepared, 2)["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should warn when strict is omitted for %s", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_strict_is_unsupported_by_the_model()
    {
        foreach (var modelId in new[]
        {
            "us.anthropic.claude-opus-4-7",
            "anthropic.claude-opus-4-8",
            "us.anthropic.claude-opus-5",
            "anthropic.claude-sonnet-5",
            "eu.anthropic.claude-fable-5",
            "anthropic.claude-fable-5-1",
            "us.anthropic.claude-fable-5-1",
            "global.anthropic.claude-fable-5-1",
        })
        {
            var prepared = AmazonBedrockPrepareTools.Prepare(
                new[] { AmazonBedrockToolInput.Function("testFunction", "A test function", BedrockParity.Node("{\"type\":\"object\",\"properties\":{}}"), true) },
                null,
                modelId);

            Assert.Null(Spec(prepared, 0)["strict"]);
            Assert.True(BedrockParity.HasWarning(
                prepared.Warnings,
                "unsupported",
                "strict",
                "Tool 'testFunction' has strict: true, but strict mode is not supported by this model on Amazon Bedrock. The strict property will be ignored."));
        }
    }

    private static void AssertAnthropicChoice(ToolChoice? choice, string type, string? name)
    {
        var prepared = AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("testFunction", "Test", new JsonObject()) },
            choice,
            Anthropic,
            null,
            null,
            true);
        var fields = prepared.AdditionalTools!["tool_choice"]!.AsObject();

        Assert.Equal(type, fields["type"]!.GetValue<string>());
        Assert.True(fields["disable_parallel_tool_use"]!.GetValue<bool>());
        if (name == null)
        {
            Assert.Null(fields["name"]);
        }
        else
        {
            Assert.Equal(name, fields["name"]!.GetValue<string>());
        }

        Assert.Null(prepared.ToolConfig["toolChoice"]);
    }

    private static AmazonBedrockPreparedTools Prepare(string name, string? description, ToolChoice? choice = null, string? modelId = null)
    {
        return AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function(name, description, BedrockParity.Node("{\"type\":\"object\",\"properties\":{}}")) },
            choice,
            modelId ?? Anthropic);
    }

    private static AmazonBedrockPreparedTools PrepareStrict(bool strict, string schema)
    {
        return AmazonBedrockPrepareTools.Prepare(
            new[] { AmazonBedrockToolInput.Function("testFunction", "A test function", BedrockParity.Node(schema), strict) },
            null,
            Anthropic);
    }

    private static JsonObject Spec(AmazonBedrockPreparedTools prepared, int index)
    {
        return prepared.ToolConfig["tools"]![index]!["toolSpec"]!.AsObject();
    }
}
