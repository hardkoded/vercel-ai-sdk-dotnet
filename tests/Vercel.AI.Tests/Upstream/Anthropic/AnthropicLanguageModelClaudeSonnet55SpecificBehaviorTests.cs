// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AnthropicLanguageModelClaudeSonnet55SpecificBehaviorTests
{
    private const string Model = "claude-sonnet-5-5";
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::claude-sonnet-5-5 specific behavior::";

    [Fact]
    [UpstreamTest(Prefix + "should return capabilities that reject disabled thinking and forced tool use", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_capabilities_that_reject_disabled_thinking_and_forced_tool_use()
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
        Assert.True(caps.SupportsBetweenToolsThinking);
        Assert.True(caps.SupportsStructuredOutput);
        Assert.True(caps.SupportsXhighEffort);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send thinking by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_thinking_by_default()
    {
        var (body, result) = await Run(AnthropicParity.Hello());

        Assert.Null(body["thinking"]);
        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send between_tools thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_between_tools_thinking()
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity
            .Generate(AnthropicParity.Client(handler), Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"between_tools\"},\"effort\":\"medium\"}"))
            .ConfigureAwait(false);
        var body = JsonNode.Parse(handler.Body)!;

        Assert.Equal("{\"type\":\"between_tools\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"medium\"}", body["output_config"]!.ToJsonString());
        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should replace disabled thinking with between_tools thinking and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_replace_disabled_thinking_with_between_tools_thinking_and_warn()
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"low\"}");
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"between_tools\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"low\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("thinking cannot be disabled for claude-sonnet-5-5. Using 'between_tools' thinking, the lowest thinking setting, instead.", warning.Message);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.thinking", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should lower xhigh effort to high with between_tools thinking and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_lower_xhigh_effort_to_high_with_between_tools_thinking_and_warn()
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"between_tools\"},\"effort\":\"xhigh\"}");
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"between_tools\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"high\"}", body["output_config"]!.ToJsonString());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("effort 'xhigh' is not supported with 'between_tools' thinking. The effort has been lowered to 'high'.", warning.Message);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.effort", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should lower max effort to high when disabled thinking is replaced", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_lower_max_effort_to_high_when_disabled_thinking_is_replaced()
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"max\"}");
        var (body, _) = await Run(options);

        Assert.Equal("{\"type\":\"between_tools\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"high\"}", body["output_config"]!.ToJsonString());
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal(
            new[] { "providerOptions.anthropic.thinking", "providerOptions.anthropic.effort" },
            prepared.Warnings.Select(warning => warning.Feature).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert budget-based thinking to adaptive thinking and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_budget_based_thinking_to_adaptive_thinking_and_warn()
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}");
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"adaptive\"}", body["thinking"]!.ToJsonString());
        Assert.Single(result.Warnings);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.thinking", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"none\" to between_tools thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_none_to_between_tools_thinking()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "none";
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"between_tools\"}", body["thinking"]!.ToJsonString());
        Assert.Null(body["output_config"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"xhigh\" to adaptive thinking with effort \"xhigh\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_xhigh_to_adaptive_thinking_with_effort_xhigh()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "xhigh";
        var (body, result) = await Run(options);

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
        options.Tools = new[]
        {
            new LanguageModelTool(
                "testTool",
                null,
                AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}")),
        };
        options.ToolChoice = ToolChoice.Required;
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"auto\"}", body["tool_choice"]!.ToJsonString());
        Assert.Single(result.Warnings);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("toolChoice", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use native structured outputs when jsonTool mode is requested", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_native_structured_outputs_when_jsonTool_mode_is_requested()
    {
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}");
        options = AnthropicParity.WithProvider("{\"structuredOutputMode\":\"jsonTool\"}", options);
        var (body, result) = await Run(options);

        Assert.Equal("json_schema", body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Null(body["tools"]);
        Assert.Single(result.Warnings);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.structuredOutputMode", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep disabled thinking for claude-sonnet-5", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_disabled_thinking_for_claude_sonnet_5()
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity
            .Generate(AnthropicParity.Client(handler), "claude-sonnet-5", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"}}"))
            .ConfigureAwait(false);
        var body = JsonNode.Parse(handler.Body)!;

        Assert.Equal("{\"type\":\"disabled\"}", body["thinking"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    private static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Run(LanguageModelCallOptions options)
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), Model, options).ConfigureAwait(false);
        return (JsonNode.Parse(handler.Body)!, result);
    }
}
