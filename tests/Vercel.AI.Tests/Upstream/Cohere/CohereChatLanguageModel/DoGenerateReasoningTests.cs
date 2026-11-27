// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doGenerate > reasoning::should extract reasoning from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_from_response()
    {
        var result = await Model(JsonFixture("cohere-reasoning"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            part =>
                Assert.Equal(
                    "Okay, so I need to figure out what 2 + 2 is. Let me start by recalling what addition means.",
                    Assert.IsType<GeneratedReasoning>(part).Text
                ),
            part => Assert.Equal("2 + 2 = 4", Assert.IsType<GeneratedText>(part).Text)
        );
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(1394, result.Usage.InputTokens);
        Assert.Equal(582, result.Usage.OutputTokens);
        Assert.Equal(1344, result.Usage.CacheReadTokens);
    }
}
