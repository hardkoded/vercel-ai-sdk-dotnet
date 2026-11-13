// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AnthropicLanguageModelClaudeOpus55SpecificBehaviorTests
{
    private const string Model = "claude-opus-5-5";
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::claude-opus-5-5 specific behavior::";
    private const string TestToolSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(Prefix + "should return capabilities that always use adaptive thinking and reject forced tool use", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_capabilities_that_always_use_adaptive_thinking_and_reject_forced_tool_use()
    {
        var caps = AnthropicModelCapabilities.Get(Model);

        Assert.True(caps.IsKnownModel);
        Assert.Equal(128000, caps.MaxOutputTokens);
        Assert.True(caps.RejectsBudgetThinking);
        Assert.True(caps.RejectsForcedToolUse);
        Assert.True(caps.RejectsSamplingParameters);
        Assert.True(caps.RejectsThinkingDisabled);
        Assert.True(caps.RejectsThinkingDisabledAboveHighEffort);
        Assert.True(caps.SupportsAdaptiveThinking);
        Assert.False(caps.SupportsBetweenToolsThinking);
        Assert.True(caps.SupportsStructuredOutput);
        Assert.True(caps.SupportsXhighEffort);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send thinking by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_thinking_by_default()
    {
        var (_, body, result) = await Run(Model, AnthropicParity.Hello());

        Assert.Null(body["thinking"]);
        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should remove disabled thinking and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_remove_disabled_thinking_and_warn()
    {
        var (_, body, result) = await Run(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"low\"}"));

        Assert.Null(body["thinking"]);
        Assert.Equal("{\"effort\":\"low\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.anthropic.thinking", warning.Feature);
        Assert.Equal("thinking cannot be disabled for claude-opus-5-5; it always uses adaptive thinking. The thinking setting has been removed. Lower 'effort' to reduce thinking.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not lower the effort when disabled thinking is removed at xhigh effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_lower_the_effort_when_disabled_thinking_is_removed_at_xhigh_effort()
    {
        var (_, body, result) = await Run(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"xhigh\"}"));

        Assert.Null(body["thinking"]);
        Assert.Equal("{\"effort\":\"xhigh\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("providerOptions.anthropic.thinking", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert budget-based thinking to adaptive thinking and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_budget_based_thinking_to_adaptive_thinking_and_warn()
    {
        var (_, body, result) = await Run(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}"));

        Assert.Equal("{\"type\":\"adaptive\"}", body["thinking"]!.ToJsonString());
        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.anthropic.thinking", warning.Feature);
        Assert.Equal("budget-based thinking is not supported by claude-opus-5-5; it always uses adaptive thinking. Using adaptive thinking instead. Use 'effort' to control how much the model thinks.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep adaptive thinking with display and block binding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_adaptive_thinking_with_display_and_block_binding()
    {
        var (handler, body, _) = await Run(
            Model,
            AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"adaptive\",\"display\":\"updates\",\"blockBinding\":{\"prefixMismatchBehavior\":\"drop_block\"}}}"));

        Assert.Equal(
            "{\"type\":\"adaptive\",\"display\":\"updates\",\"block_binding\":{\"prefix_mismatch_behavior\":\"drop_block\"}}",
            body["thinking"]!.ToJsonString());
        Assert.Contains("thinking-display-updates-2026-08-18", handler.Headers["anthropic-beta"]);
        Assert.Contains("thinking-binding-controls-2026-08-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"none\" to low effort instead of disabling thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_none_to_low_effort_instead_of_disabling_thinking()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "none";
        var (_, body, result) = await Run(Model, options);

        Assert.Null(body["thinking"]);
        Assert.Equal("{\"effort\":\"low\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal("reasoning 'none' is not supported by claude-opus-5-5; it always uses adaptive thinking. Using effort 'low' to minimize thinking instead.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"xhigh\" to adaptive thinking with effort \"xhigh\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_xhigh_to_adaptive_thinking_with_effort_xhigh()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "xhigh";
        var (_, body, result) = await Run(Model, options);

        Assert.Equal("adaptive", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("summarized", body["thinking"]!["display"]!.GetValue<string>());
        Assert.Equal("xhigh", body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to auto tool choice when tool choice is \"required\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fall_back_to_auto_tool_choice_when_tool_choice_is_required()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[] { TestTool("testTool") };
        options.ToolChoice = ToolChoice.Required;
        var (_, body, result) = await Run(Model, options);

        Assert.Equal("{\"type\":\"auto\"}", body["tool_choice"]!.ToJsonString());
        Assert.Single(body["tools"]!.AsArray());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Equal("toolChoice 'required' is not supported with this model or thinking configuration. Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should only send the selected tool with auto tool choice when tool choice is \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_send_the_selected_tool_with_auto_tool_choice_when_tool_choice_is_tool()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[] { TestTool("testTool"), TestTool("otherTool") };
        options.ToolChoice = ToolChoice.Tool("otherTool");
        var (_, body, result) = await Run(Model, options);

        Assert.Equal("{\"type\":\"auto\"}", body["tool_choice"]!.ToJsonString());
        Assert.Equal(new[] { "otherTool" }, body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Equal("toolChoice 'tool' is not supported with this model or thinking configuration. Only the 'otherTool' tool is sent with 'auto' tool choice. Instruct the model to use the tool in the prompt and verify that a tool call was made.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use native structured outputs when jsonTool mode is requested", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_native_structured_outputs_when_jsonTool_mode_is_requested()
    {
        const string schema = "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}";
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json(schema);
        options = AnthropicParity.WithProvider("{\"structuredOutputMode\":\"jsonTool\"}", options);
        var (_, body, result) = await Run(Model, options);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"json_schema\",\"schema\":" + schema + "}"), body["output_config"]!["format"]));
        Assert.Null(body["tools"]);
        Assert.Null(body["tool_choice"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.anthropic.structuredOutputMode", warning.Feature);
        Assert.Equal("structuredOutputMode 'jsonTool' is not supported by claude-opus-5-5 because it rejects forced tool use. Using 'outputFormat' instead.", warning.Message);
    }

    [Theory]
    [InlineData("claude-fable-5-1")]
    [InlineData("claude-fable-5-2")]
    [UpstreamTest(Prefix + "should use auto tool choice for %s when tool choice is \"required\"", Coverage = UpstreamCoverage.Covered, Commit = "84208104a4eac2dcc84ef0973ac47d4f9c84088c")]
    public async Task Should_use_auto_tool_choice_for_model_when_tool_choice_is_required(string modelId)
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[] { TestTool("testTool") };
        options.ToolChoice = ToolChoice.Required;
        var (_, body, result) = await Run(modelId, options);

        Assert.Equal("{\"type\":\"auto\"}", body["tool_choice"]!.ToJsonString());
        Assert.Single(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should remove disabled thinking for claude-fable-5-2 and warn", Coverage = UpstreamCoverage.Covered, Commit = "84208104a4eac2dcc84ef0973ac47d4f9c84088c")]
    public async Task Should_remove_disabled_thinking_for_claude_fable_5_2_and_warn()
    {
        var (_, body, result) = await Run("claude-fable-5-2", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"xhigh\"}"));

        Assert.Null(body["thinking"]);
        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        Assert.Equal("{\"effort\":\"xhigh\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("providerOptions.anthropic.thinking", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should still send forced tool choice for claude-opus-5", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_still_send_forced_tool_choice_for_claude_opus_5()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[] { TestTool("testTool") };
        options.ToolChoice = ToolChoice.Required;
        var (_, body, result) = await Run("claude-opus-5", options);

        Assert.Equal("{\"type\":\"any\"}", body["tool_choice"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    private static LanguageModelTool TestTool(string name) => new(name, null, AnthropicParity.Json(TestToolSchema));

    private static async Task<(CaptureHandler Handler, JsonNode Body, LanguageModelGenerateResult Result)> Run(string model, LanguageModelCallOptions options)
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), model, options).ConfigureAwait(false);
        return (handler, JsonNode.Parse(handler.Body)!, result);
    }
}
