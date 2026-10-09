// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; basic text response &gt; top-level reasoning option</c>.</summary>
public sealed class DoGenerateBasicTextResponseTopLevelReasoningOptionTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response > top-level reasoning option::should pass top-level max reasoning to models that support it",
        Coverage = UpstreamCoverage.Partial,
        Note = "Uses gpt-6-sol, the model whose supported efforts list max. The port sends no default reasoning summary, so summary detailed is not asserted.")]
    public void Should_pass_top_level_max_reasoning_to_models_that_support_it()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-6-sol", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Reasoning = "max",
        }, false);

        Assert.Equal("gpt-6-sol", prepared.Body["model"]!.GetValue<string>());
        Assert.Equal("max", prepared.Body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }
}
