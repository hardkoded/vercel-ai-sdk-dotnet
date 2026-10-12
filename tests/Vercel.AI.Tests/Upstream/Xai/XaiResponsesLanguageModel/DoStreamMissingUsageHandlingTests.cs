// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; missing usage handling</c>.</summary>
public sealed class DoStreamMissingUsageHandlingTests
{
    private const string Prefix = TestFile + " > doStream > missing usage handling::";

    [Fact]
    [UpstreamTest(Prefix + "should handle missing usage in streaming response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_missing_usage_in_streaming_response()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"in_progress","output":[]}}""",
            """{"type":"response.output_text.delta","output_index":0,"content_index":0,"delta":"Hello"}""",
            """{"type":"response.completed","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"completed","output":[{"type":"message","id":"msg_001","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Hello"}]}]}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("completed", finish.RawFinishReason);
        Assert.Equal(0, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.NoCacheInputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(0, finish.Usage.CacheWriteTokens);
        Assert.Equal(0, finish.Usage.OutputTokens);
        Assert.Equal(0, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }
}
