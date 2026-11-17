// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages</c>.</summary>
public sealed class ConvertToDeepSeekChatMessagesTests
{
    [Theory]
    [InlineData("deepseek-v5-pro")]
    [InlineData("deepseek-v10-flash")]
    [InlineData("deepseek-next")]
    [UpstreamTest(
        TestFile + "::preserves prior-turn reasoning and fills absent reasoning for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_prior_turn_reasoning_and_fills_absent_reasoning_for(string modelId)
    {
        var result = Run(
            modelId,
            [
                User(new TextContentPart("Hello")),
                Assistant(
                    new AssistantReasoningPart("Prior-turn reasoning."),
                    new AssistantTextPart("Hi")
                ),
                User(new TextContentPart("Again")),
                Assistant(new AssistantTextPart("Hello again")),
                User(new TextContentPart("Continue")),
            ]
        );

        Assert.Equal(
            "Prior-turn reasoning.",
            result.Messages[1]!["reasoning_content"]!.GetValue<string>()
        );
        Assert.Equal("", result.Messages[3]!["reasoning_content"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }
}
