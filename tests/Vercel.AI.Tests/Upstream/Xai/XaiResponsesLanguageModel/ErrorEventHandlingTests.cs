// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; error event handling</c>.</summary>
public sealed class ErrorEventHandlingTests
{
    private const string Prefix = TestFile + " > error event handling::";

    [Fact]
    [UpstreamTest(
        Prefix + "should emit error chunk for server error events",
        Coverage = UpstreamCoverage.Partial,
        Note = "ErrorStreamPart carries only a message, so the case does not assert the error type, code, status code, retry flag, or data.")]
    public async Task Should_emit_error_chunk_for_server_error_events()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"error","code":"server_error","message":"Internal server error","param":null}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        Assert.Equal("Internal server error", First<ErrorStreamPart>(parts).Message);
    }
}
