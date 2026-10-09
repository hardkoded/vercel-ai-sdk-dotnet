// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AnthropicLanguageModelClaudeHaiku55SpecificBehaviorTests
{
    private const string Model = "claude-haiku-5-5";
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::claude-haiku-5-5 specific behavior::";

    [Fact]
    [UpstreamTest(Prefix + "should return known capabilities that allow disabling thinking up to high effort", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_known_capabilities_that_allow_disabling_thinking_up_to_high_effort()
    {
        var caps = AnthropicModelCapabilities.Get(Model);

        Assert.True(caps.IsKnownModel);
        Assert.Equal(128000, caps.MaxOutputTokens);
        Assert.True(caps.RejectsBudgetThinking);
        Assert.False(caps.RejectsForcedToolUse);
        Assert.True(caps.RejectsSamplingParameters);
        Assert.False(caps.RejectsThinkingDisabled);
        Assert.True(caps.RejectsThinkingDisabledAboveHighEffort);
        Assert.True(caps.SupportsAdaptiveThinking);
        Assert.False(caps.SupportsBetweenToolsThinking);
        Assert.True(caps.SupportsStructuredOutput);
        Assert.True(caps.SupportsXhighEffort);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn about an unknown model and use the 128k output limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_warn_about_an_unknown_model_and_use_the_128k_output_limit()
    {
        var (body, result) = await Run(AnthropicParity.Hello());

        Assert.Equal(128000, body["max_tokens"]!.GetValue<int>());
        Assert.Null(body["thinking"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send disabled thinking at low effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_disabled_thinking_at_low_effort()
    {
        var (body, result) = await Run(AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"low\"}"));

        Assert.Equal("{\"type\":\"disabled\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"low\"}", body["output_config"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("xhigh")]
    [InlineData("max")]
    [UpstreamTest(Prefix + "should lower effort \"%s\" to \"high\" when thinking is disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_lower_effort_to_high_when_thinking_is_disabled(string effort)
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"" + effort + "\"}");
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"disabled\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"high\"}", body["output_config"]!.ToJsonString());
        var details = "effort '" + effort + "' is not supported by claude-haiku-5-5 when thinking is disabled. The effort has been lowered to 'high'.";
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(details, warning.Message);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.effort", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send adaptive thinking with xhigh effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_adaptive_thinking_with_xhigh_effort()
    {
        var (body, result) = await Run(AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"adaptive\"},\"effort\":\"xhigh\"}"));

        Assert.Equal("{\"type\":\"adaptive\"}", body["thinking"]!.ToJsonString());
        Assert.Equal("{\"effort\":\"xhigh\"}", body["output_config"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert budget-based thinking to adaptive thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_budget_based_thinking_to_adaptive_thinking()
    {
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":4000}}");
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"adaptive\"}", body["thinking"]!.ToJsonString());
        Assert.Null(body["thinking"]!["budget_tokens"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("budget-based thinking is not supported by claude-haiku-5-5. Using adaptive thinking instead. Use 'effort' to control how much the model thinks.", warning.Message);
        var prepared = AnthropicParity.Prepare(Model, options);
        Assert.Equal("providerOptions.anthropic.thinking", Assert.Single(prepared.Warnings).Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"none\" to disabled thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_none_to_disabled_thinking()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "none";
        var (body, result) = await Run(options);

        Assert.Equal("{\"type\":\"disabled\"}", body["thinking"]!.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use native structured outputs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_native_structured_outputs()
    {
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}");
        var (body, _) = await Run(options);

        Assert.Equal("json_schema", body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Null(body["tools"]);
    }

    private static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Run(LanguageModelCallOptions options)
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), Model, options).ConfigureAwait(false);
        return (JsonNode.Parse(handler.Body)!, result);
    }
}
