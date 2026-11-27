// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doStream > reasoning::should stream reasoning deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_reasoning_deltas()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = false;

        var parts = await Stream(Model(ChunksFixture("cohere-reasoning")), options);

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal(36, parts.OfType<ReasoningDeltaStreamPart>().Count());
        Assert.Equal(9, parts.OfType<TextDeltaStreamPart>().Count());
        Assert.Single(parts.OfType<ReasoningStartStreamPart>());
        Assert.Single(parts.OfType<ReasoningEndStreamPart>());
        Assert.Single(parts.OfType<TextStartStreamPart>());
        Assert.Single(parts.OfType<TextEndStreamPart>());
        Assert.True(
            parts.FindIndex(part => part is ReasoningEndStreamPart)
                < parts.FindIndex(part => part is TextStartStreamPart)
        );
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(1394, finish.Usage.InputTokens);
        Assert.Equal(1360, finish.Usage.CacheReadTokens);
        Assert.Equal(54, finish.Usage.OutputTokens);
    }
}
