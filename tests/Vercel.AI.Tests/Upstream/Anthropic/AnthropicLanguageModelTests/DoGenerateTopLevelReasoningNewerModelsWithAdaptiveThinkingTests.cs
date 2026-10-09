// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; top-level reasoning (newer models with adaptive thinking)</c>.</summary>
public sealed class DoGenerateTopLevelReasoningNewerModelsWithAdaptiveThinkingTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"max\" directly to adaptive thinking with effort \"max\"", Coverage = UpstreamCoverage.Covered)]
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
}
