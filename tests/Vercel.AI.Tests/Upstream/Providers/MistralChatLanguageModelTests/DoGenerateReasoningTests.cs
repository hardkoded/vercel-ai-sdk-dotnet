// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should extract reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_content()
    {
        var capture = JsonFixture("mistral-reasoning");

        var result = await Model(capture, "magistral-medium-2507")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(2, result.Content.Count);
        Assert.Equal(
            "The user is asking for 2+2. This is basic arithmetic. 2+2=4.",
            Assert.IsType<GeneratedReasoning>(result.Content[0]).Text
        );
        Assert.Equal("2 + 2 = 4", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(46, result.Usage.OutputTokens);
    }
}
