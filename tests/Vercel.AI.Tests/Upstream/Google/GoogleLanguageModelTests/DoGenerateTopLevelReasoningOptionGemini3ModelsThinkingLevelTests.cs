// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate &gt; top-level reasoning option &gt; Gemini 3 models (thinkingLevel)</c>.</summary>
public sealed class DoGenerateTopLevelReasoningOptionGemini3ModelsThinkingLevelTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should coerce reasoning \"max\" to \"high\" with compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public void Should_coerce_reasoning_max_to_high_with_compatibility_warning()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Reasoning = "max",
        };

        var prepared = GoogleRequest.Prepare("gemini-3-pro-preview", "google.generative-ai", options, false);

        Assert.Equal("high", prepared.Body["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]!.GetValue<string>());
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && warning.Feature == "reasoning" && warning.Details == "reasoning \"max\" is not directly supported by this model. mapped to effort \"high\".");
    }
}
