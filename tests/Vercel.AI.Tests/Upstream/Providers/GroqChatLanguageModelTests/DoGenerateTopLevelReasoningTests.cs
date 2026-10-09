// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning max to high with a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_coerce_top_level_reasoning_max_to_high_with_a_warning()
    {
        var capture = new UpstreamCapture();
        var model = (OpenAICompatibleLanguageModel)GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("llama-3.3-70b-versatile");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message == "reasoning \"max\" is not directly supported by this model. mapped to effort \"high\".");
    }
}
