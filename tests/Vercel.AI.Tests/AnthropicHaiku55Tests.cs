// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests;

public sealed class AnthropicHaiku55Tests
{
    private const string Model = "claude-haiku-5-5";

    [Theory]
    [InlineData(Model)]
    [InlineData("us.anthropic.claude-haiku-5-5-v1:0")]
    public void Should_return_known_capabilities_that_allow_disabling_thinking_up_to_high_effort(string modelId)
    {
        var caps = AnthropicModelCapabilities.Get(modelId);

        Assert.Equal(128000, caps.MaxOutputTokens);
        Assert.True(caps.SupportsStructuredOutput);
        Assert.True(caps.SupportsAdaptiveThinking);
        Assert.True(caps.RejectsSamplingParameters);
        Assert.True(caps.SupportsXhighEffort);
        Assert.True(caps.RejectsThinkingDisabledAboveHighEffort);
        Assert.False(caps.RejectsThinkingDisabled);
        Assert.True(caps.RejectsBudgetThinking);
        Assert.False(caps.RejectsForcedToolUse);
        Assert.True(caps.IsKnownModel);
    }

    [Fact]
    public void Should_not_warn_about_an_unknown_model_and_use_the_128k_output_limit()
    {
        var prepared = AnthropicParity.Prepare(Model, AnthropicParity.Hello());

        Assert.Equal(128000, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Null(prepared.Body["thinking"]);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    public void Should_send_disabled_thinking_at_low_effort()
    {
        var prepared = AnthropicParity.Prepare(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"low\"}"));

        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("low", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Theory]
    [InlineData("xhigh")]
    [InlineData("max")]
    public void Should_lower_effort_to_high_when_thinking_is_disabled(string effort)
    {
        var prepared = AnthropicParity.Prepare(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"" + effort + "\"}"));

        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("high", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("providerOptions.anthropic.effort", warning.Feature);
        Assert.Equal("effort '" + effort + "' is not supported by claude-haiku-5-5 when thinking is disabled. The effort has been lowered to 'high'.", warning.Details);
    }

    [Fact]
    public void Should_send_adaptive_thinking_with_xhigh_effort()
    {
        var prepared = AnthropicParity.Prepare(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"adaptive\"},\"effort\":\"xhigh\"}"));

        Assert.Equal("adaptive", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("xhigh", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    public void Should_convert_budget_based_thinking_to_adaptive_thinking()
    {
        var prepared = AnthropicParity.Prepare(Model, AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":4000}}"));

        Assert.Equal("adaptive", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Null(prepared.Body["thinking"]!["budget_tokens"]);
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("providerOptions.anthropic.thinking", warning.Feature);
        Assert.Equal("budget-based thinking is not supported by claude-haiku-5-5. Using adaptive thinking instead. Use 'effort' to control how much the model thinks.", warning.Details);
    }

    [Fact]
    public void Should_map_reasoning_none_to_disabled_thinking()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "none";
        var prepared = AnthropicParity.Prepare(Model, options);

        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    public void Should_use_native_structured_outputs()
    {
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}");
        var prepared = AnthropicParity.Prepare(Model, options);

        Assert.Equal("json_schema", prepared.Body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Null(prepared.Body["tools"]);
    }
}
