// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; top-level reasoning (newer models with adaptive thinking)</c>.</summary>
public sealed class DoGenerateTopLevelReasoningNewerModelsWithAdaptiveThinkingTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should not set thinking config when reasoning is \"provider-default\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_set_thinking_config_when_reasoning_is_provider_default()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Reasoning = "provider-default";

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-6",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(requestBody["thinking"]);
        Assert.Null(requestBody["output_config"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"max\" directly to adaptive thinking with effort \"max\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_reasoning_max_directly_to_adaptive_thinking_with_effort_max()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "max";

        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);

        Assert.Equal("adaptive", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("summarized", prepared.Body["thinking"]!["display"]!.GetValue<string>());
        Assert.Equal("max", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should strip temperature, topP, topK when reasoning enables thinking",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_temperature_topP_topK_when_reasoning_enables_thinking()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Reasoning = "high";
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.TopK = 10;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-6",
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
        LanguageModelTestSupport.AssertContainsWarning(
            result.Warnings,
            "unsupported",
            "topK",
            "topK is not supported when thinking is enabled"
        );
        LanguageModelTestSupport.AssertContainsWarning(
            result.Warnings,
            "unsupported",
            "topP",
            "topP is not supported when thinking is enabled"
        );
    }
}
