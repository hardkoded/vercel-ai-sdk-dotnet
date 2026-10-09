// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.DeepSeekChatLanguageModelTests;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    [Fact]
    [UpstreamTest("packages/deepseek/src/chat/deepseek-chat-language-model.test.ts::DeepSeekChatLanguageModel > doGenerate > top-level reasoning::should map top-level reasoning max directly to reasoning_effort max", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_max_directly_to_reasoning_effort_max()
    {
        var capture = new UpstreamCapture();
        var model = (OpenAICompatibleLanguageModel)DeepSeekProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("deepseek-reasoner");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "compatibility" && warning.Message.Contains("reasoning"));
    }
}
