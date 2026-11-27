// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; top-level reasoning (providerOptions precedence)</c>.</summary>
public sealed class DoGenerateTopLevelReasoningProviderOptionsPrecedenceTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (providerOptions precedence)::";

    [Fact]
    [UpstreamTest(
        Prefix + "should let anthropic.thinking take precedence over top-level reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_anthropic_thinking_take_precedence_over_top_level_reasoning()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"}}");
        options.Reasoning = "high";

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-6",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson("{\"type\":\"disabled\"}", requestBody["thinking"]);
        Assert.Null(requestBody["output_config"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should let anthropic.effort take precedence over top-level reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_anthropic_effort_take_precedence_over_top_level_reasoning()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.WithProvider("{\"effort\":\"low\"}");
        options.Reasoning = "high";

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-6",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["thinking"]);
        Assert.Equal("low", requestBody["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not apply top-level reasoning when anthropic.thinking is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_apply_top_level_reasoning_when_anthropic_thinking_is_set()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":5000}}"
        );
        options.Reasoning = "xhigh";

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal("enabled", requestBody["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(5000, requestBody["thinking"]!["budget_tokens"]!.GetValue<int>());
    }
}
