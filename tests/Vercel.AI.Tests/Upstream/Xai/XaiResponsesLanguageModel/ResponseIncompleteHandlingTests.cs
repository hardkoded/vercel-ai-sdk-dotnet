// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; response.incomplete handling</c>.</summary>
public sealed class ResponseIncompleteHandlingTests
{
    private const string Prefix = TestFile + " > response.incomplete handling::";

    [Fact]
    [UpstreamTest(Prefix + "should set finish reason to length for max_output_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_set_finish_reason_to_length_for_max_output_tokens()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.output_text.delta","item_id":"msg_456","output_index":0,"content_index":0,"delta":"partial output"}""",
            """{"type":"response.incomplete","response":{"incomplete_details":{"reason":"max_output_tokens"},"usage":{"input_tokens":100,"output_tokens":4096}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Length, finish.FinishReason);
        Assert.Equal("max_output_tokens", finish.RawFinishReason);
        Assert.Equal(100, finish.Usage.InputTokens);
        Assert.Equal(4096, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should default to length when incomplete_details is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_default_to_length_when_incomplete_details_is_missing()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.incomplete","response":{"usage":{"input_tokens":50,"output_tokens":200}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Equal("incomplete", finish.RawFinishReason);
    }
}
