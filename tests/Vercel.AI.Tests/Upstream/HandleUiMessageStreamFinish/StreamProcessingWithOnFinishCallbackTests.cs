// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>Port of <c>handle-ui-message-stream-finish.test.ts</c> &gt; <c>handleUIMessageStreamFinish</c> &gt; <c>stream processing with onFinish callback</c>.</summary>
public sealed class StreamProcessingWithOnFinishCallbackTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/handle-ui-message-stream-finish.test.ts::handleUIMessageStreamFinish > stream processing with onFinish callback::";

    [Fact]
    [UpstreamTest(
        Prefix + "should process stream and call onFinish with correct parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_process_stream_and_call_onFinish_with_correct_parameters()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-456"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
            """{"type":"text-delta","id":"text-1","delta":" World"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish"}""",
        ];
        List<UIMessage> originalMessages =
        [
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-1",
                UIMessageRole.User,
                "Hello"
            ),
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-456",
            originalMessages: originalMessages,
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        var callArgs = Assert.Single(onFinishCalls);
        Assert.False(callArgs.IsContinuation);
        Assert.Equal("msg-456", callArgs.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, callArgs.ResponseMessage.Role);
        Assert.Equal(2, callArgs.Messages.Count);
        Assert.Same(originalMessages[0], callArgs.Messages[0]);
        Assert.Same(callArgs.ResponseMessage, callArgs.Messages[1]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer onEnd over deprecated onFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_onEnd_over_deprecated_onFinish()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onEndCalls = 0;
        var onFinishCalls = 0;

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-456"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-456",
            onEnd: _ =>
            {
                onEndCalls++;
                return Task.CompletedTask;
            },
            onFinish: _ =>
            {
                onFinishCalls++;
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        Assert.Equal(1, onEndCalls);
        Assert.Equal(0, onFinishCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the stream owner outcome to onEnd",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_stream_owner_outcome_to_onEnd()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onEndCalls = new List<UIMessageStreamOnEndEvent>();
        var error = new Exception("stream failed");

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-456"}""",
                """{"type":"error","errorText":"masked error"}"""
            ),
            messageId: "msg-456",
            onEnd: e =>
            {
                onEndCalls.Add(e);
                return Task.CompletedTask;
            },
            getOutcome: () => new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Failed, error)
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onEndCalls);
        Assert.False(callArgs.IsAborted);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, callArgs.Outcome.Status);
        Assert.Same(error, callArgs.Outcome.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report UI message processing failures before and after a declared outcome",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_UI_message_processing_failures_before_and_after_a_declared_outcome()
    {
        string[][] inputChunkSets =
        [
            ["""{"type":"text-delta","id":"missing","delta":"text"}"""],
            ["""{"type":"finish"}""", """{"type":"text-delta","id":"missing","delta":"text"}"""],
        ];

        foreach (var inputChunks in inputChunkSets)
        {
            var harness = new HandleUIMessageStreamFinishHarness();
            var onEndCalls = new List<UIMessageStreamOnEndEvent>();
            var resultStream = harness.Handle(
                HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
                messageId: "msg-processing-error",
                onEnd: e =>
                {
                    onEndCalls.Add(e);
                    return Task.CompletedTask;
                },
                getOutcome: () => new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
            );

            var processingError = await Assert.ThrowsAnyAsync<Exception>(() =>
                HandleUIMessageStreamFinishHarness.ToArray(resultStream)
            );

            var callArgs = Assert.Single(onEndCalls);
            Assert.Equal(UIMessageStreamOutcomeStatus.Failed, callArgs.Outcome.Status);
            Assert.Same(processingError, callArgs.Outcome.Error);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject message IDs without mutating frozen start chunks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_message_IDs_without_mutating_frozen_start_chunks()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onEndCalls = new List<UIMessageStreamOnEndEvent>();
        var startChunk = new JsonObject { ["type"] = "start" };

        async IAsyncEnumerable<JsonObject> Source()
        {
            yield return startChunk;
            await Task.Yield();
        }

        var resultStream = harness.Handle(
            Source(),
            messageId: "msg-injected",
            onEnd: e =>
            {
                onEndCalls.Add(e);
                return Task.CompletedTask;
            },
            getOutcome: () => new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(
            ["""{"type":"start","messageId":"msg-injected"}"""],
            result
        );
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"type":"start"}"""), startChunk));
        var callArgs = Assert.Single(onEndCalls);
        Assert.Equal(UIMessageStreamOutcomeStatus.Completed, callArgs.Outcome.Status);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle empty original messages array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_original_messages_array()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-789"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Response"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-789",
            originalMessages: [],
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onFinishCalls);
        Assert.False(callArgs.IsContinuation);
        Assert.Single(callArgs.Messages);
        Assert.Same(callArgs.ResponseMessage, callArgs.Messages[0]);
    }
}
