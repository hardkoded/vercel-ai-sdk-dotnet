// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; top-level reasoning (older models with budget-based thinking)</c>.</summary>
public sealed class DoGenerateTopLevelReasoningOlderModelsWithBudgetBasedThinkingTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should not set thinking config when reasoning is \"provider-default\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_set_thinking_config_when_reasoning_is_provider_default()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Reasoning = "provider-default";

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["thinking"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should adjust max_tokens to include thinking budget",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_adjust_max_tokens_to_include_thinking_budget()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Reasoning = "low";
        options.MaxOutputTokens = 10000;

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal(16400, requestBody["max_tokens"]!.GetValue<int>());
        Assert.Equal(6400, requestBody["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should strip temperature, topP, topK when reasoning enables thinking",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_temperature_topP_topK_when_reasoning_enables_thinking()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Reasoning = "medium";
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.TopK = 10;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["temperature"]);
        Assert.Null(requestBody["top_p"]);
        Assert.Null(requestBody["top_k"]);
        LanguageModelTestSupport.AssertContainsWarning(
            result.Warnings,
            "unsupported",
            "temperature",
            "temperature is not supported when thinking is enabled"
        );
    }
}
