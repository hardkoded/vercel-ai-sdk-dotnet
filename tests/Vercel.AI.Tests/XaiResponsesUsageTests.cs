// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests;

public sealed class XaiResponsesUsageTests
{
    [Fact]
    public async Task ShouldAcceptFloatTokenCountsInGenerateResponses()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[],
             "usage":{"input_tokens":10.0,"output_tokens":5.0,"total_tokens":15.5,
                      "input_tokens_details":{"cached_tokens":2.0},"output_tokens_details":{"reasoning_tokens":1.0}}}
            """);

        var usage = (await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None)).Usage;

        Assert.Equal(10, usage.InputTokens);
        Assert.Equal(8, usage.NoCacheInputTokens);
        Assert.Equal(2, usage.CacheReadTokens);
        Assert.Equal(5, usage.OutputTokens);
        Assert.Equal(4, usage.TextTokens);
        Assert.Equal(1, usage.ReasoningTokens);
    }

    [Fact]
    public async Task ShouldAcceptFloatTokenCountsInStreams()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"in_progress","output":[]}}""",
            """{"type":"response.output_text.delta","output_index":0,"content_index":0,"delta":"Hello"}""",
            """{"type":"response.completed","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"completed","output":[{"type":"message","id":"msg_001","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Hello"}]}],"usage":{"input_tokens":10.0,"output_tokens":5.0,"total_tokens":15.0}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(10, finish.Usage.InputTokens);
        Assert.Equal(5, finish.Usage.OutputTokens);
    }
}
