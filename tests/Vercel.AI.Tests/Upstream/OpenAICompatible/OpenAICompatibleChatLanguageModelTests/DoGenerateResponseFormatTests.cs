// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; response format</c>.</summary>
public sealed class DoGenerateResponseFormatTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should pass top-level max reasoning as reasoning_effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_top_level_max_reasoning_as_reasoning_effort()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("max", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
    }
}
