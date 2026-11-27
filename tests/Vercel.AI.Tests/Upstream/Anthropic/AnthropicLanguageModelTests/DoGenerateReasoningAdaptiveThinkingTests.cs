// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; reasoning (adaptive thinking)</c>.</summary>
public sealed class DoGenerateReasoningAdaptiveThinkingTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (adaptive thinking)::should send adaptive thinking without budget_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_adaptive_thinking_without_budget_tokens()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-opus-4-6",
            AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"adaptive\"}}")
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal("adaptive", requestBody["thinking"]!["type"]!.GetValue<string>());
        Assert.Null(requestBody["thinking"]!["budget_tokens"]);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (adaptive thinking)::should report thinking tokens as reasoning usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_thinking_tokens_as_reasoning_usage()
    {
        var handler = LanguageModelTestSupport.JsonFixture(
            "anthropic-claude-opus-5-reasoning-high.1"
        );
        var options = AnthropicParity.Hello();
        options.Reasoning = "high";

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-opus-5",
            options
        );

        Assert.Equal(1699, result.Usage.OutputTokens);
        Assert.Equal(1560, result.Usage.TextTokens);
        Assert.Equal(139, result.Usage.ReasoningTokens);
    }
}
