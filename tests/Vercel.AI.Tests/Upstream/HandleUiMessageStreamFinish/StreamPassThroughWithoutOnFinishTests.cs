// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>Port of <c>handle-ui-message-stream-finish.test.ts</c> &gt; <c>handleUIMessageStreamFinish</c> &gt; <c>stream pass-through without onFinish</c>.</summary>
public sealed class StreamPassThroughWithoutOnFinishTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/handle-ui-message-stream-finish.test.ts::handleUIMessageStreamFinish > stream pass-through without onFinish::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should pass through stream chunks without processing when onFinish is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_stream_chunks_without_processing_when_onFinish_is_not_provided()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-123"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
            """{"type":"text-delta","id":"text-1","delta":" World"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-123",
            originalMessages: []
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        Assert.Empty(harness.Errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject messageId when not present in start chunk",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_messageId_when_not_present_in_start_chunk()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        string[] inputChunks =
        [
            """{"type":"start"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Test"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish"}""",
        ]; // no messageId in the start chunk

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "injected-123",
            originalMessages: []
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(
            ["""{"type":"start","messageId":"injected-123"}"""],
            result.Take(1).ToList()
        );
        HandleUIMessageStreamFinishHarness.AssertChunks(
            inputChunks.Skip(1).ToList(),
            result.Skip(1).ToList()
        );
    }
}
