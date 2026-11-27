// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>max tokens</c>.</summary>
public sealed class DoGenerateMaxTokensTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doGenerate > max tokens::should map MAX_TOKENS finish reason to length",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_MAX_TOKENS_finish_reason_to_length()
    {
        var result = await Model(JsonFixture("cohere-max-tokens"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("MAX_TOKENS", result.RawFinishReason);
    }
}
