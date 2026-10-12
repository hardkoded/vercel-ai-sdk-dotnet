// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; response.failed handling</c>.</summary>
public sealed class ResponseFailedHandlingTests
{
    private const string Prefix = TestFile + " > response.failed handling::";
    private const string StatusNote = "ErrorStreamPart carries only a message, so the case does not assert the error type, code, status code, retry flag, or data.";
    private const string Created = """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""";

    [Fact]
    [UpstreamTest(Prefix + "should set finish reason to error", Coverage = UpstreamCoverage.Partial, Note = StatusNote)]
    public async Task Should_set_finish_reason_to_error()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.failed","response":{"error":{"code":"server_error","message":"Internal server error"},"usage":{"input_tokens":50,"output_tokens":0}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Equal("error", finish.RawFinishReason);
        Assert.Equal(50, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.OutputTokens);
        Assert.Equal("Internal server error", First<ErrorStreamPart>(parts).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should classify insufficient quota as non-retryable", Coverage = UpstreamCoverage.Partial, Note = StatusNote)]
    public async Task Should_classify_insufficient_quota_as_non_retryable()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.failed","response":{"error":{"code":"insufficient_quota","message":"You exceeded your current quota."},"usage":{"input_tokens":50,"output_tokens":0}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        Assert.Equal("You exceeded your current quota.", First<ErrorStreamPart>(parts).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map incomplete_details reason on failed response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_incomplete_details_reason_on_failed_response()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.failed","response":{"error":{"code":"server_error","message":"response failed"},"incomplete_details":{"reason":"max_output_tokens"},"usage":null}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Length, finish.FinishReason);
        Assert.Equal("max_output_tokens", finish.RawFinishReason);
    }
}
