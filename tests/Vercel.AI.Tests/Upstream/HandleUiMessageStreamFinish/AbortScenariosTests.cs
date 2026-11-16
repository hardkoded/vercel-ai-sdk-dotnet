// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>Port of <c>handle-ui-message-stream-finish.test.ts</c> &gt; <c>handleUIMessageStreamFinish</c> &gt; <c>abort scenarios</c>.</summary>
public sealed class AbortScenariosTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/handle-ui-message-stream-finish.test.ts::handleUIMessageStreamFinish > abort scenarios::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set isAborted to true when abort chunk is encountered",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_isAborted_to_true_when_abort_chunk_is_encountered()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-abort-1"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Starting text"}""",
            """{"type":"abort"}""",
            """{"type":"finish"}""",
        ];
        List<UIMessage> originalMessages =
        [
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-1",
                UIMessageRole.User,
                "Test request"
            ),
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-abort-1",
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
        Assert.True(callArgs.IsAborted);
        Assert.False(callArgs.IsContinuation);
        Assert.Equal("msg-abort-1", callArgs.ResponseMessage.Id);
        Assert.Equal(2, callArgs.Messages.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through abort reason when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_abort_reason_when_provided()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-abort-reason"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Starting text"}""",
            """{"type":"abort","reason":"manual abort"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-abort-reason",
            originalMessages: [],
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        Assert.Single(onFinishCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set isAborted to false when no abort chunk is encountered",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_isAborted_to_false_when_no_abort_chunk_is_encountered()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        List<UIMessage> originalMessages =
        [
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-1",
                UIMessageRole.User,
                "Test request"
            ),
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-normal"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Complete text"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-normal",
            originalMessages: originalMessages,
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onFinishCalls);
        Assert.False(callArgs.IsAborted);
        Assert.False(callArgs.IsContinuation);
        Assert.Equal("msg-normal", callArgs.ResponseMessage.Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle abort chunk in pass-through mode without onFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_abort_chunk_in_pass_through_mode_without_onFinish()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-abort-passthrough"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Text before abort"}""",
            """{"type":"abort"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-abort-passthrough",
            originalMessages: []
        ); // onFinish is not provided

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        Assert.Empty(harness.Errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multiple abort chunks correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_multiple_abort_chunks_correctly()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-multiple-abort"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"abort"}""",
            """{"type":"text-delta","id":"text-1","delta":"Some text"}""",
            """{"type":"abort"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-multiple-abort",
            originalMessages: [],
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        var callArgs = Assert.Single(onFinishCalls);
        Assert.True(callArgs.IsAborted);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should report consumer cancellation when the reader is cancelled before an outcome is declared",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_consumer_cancellation_when_the_reader_is_cancelled_before_an_outcome_is_declared()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-1"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}"""
            ),
            messageId: "msg-1",
            originalMessages: [],
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        // the reader reads one chunk and cancels
        await using (var reader = resultStream.GetAsyncEnumerator())
        {
            await reader.MoveNextAsync();
        }

        var callArgs = Assert.Single(onFinishCalls);
        Assert.False(callArgs.IsAborted);
        Assert.True(callArgs.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, callArgs.Outcome.Status);
        Assert.Equal("msg-1", callArgs.ResponseMessage.Id);
    }

    [Theory]
    [InlineData(UIMessageStreamOutcomeStatus.Completed)]
    [InlineData(UIMessageStreamOutcomeStatus.Failed)]
    [InlineData(UIMessageStreamOutcomeStatus.Aborted)]
    [UpstreamTest(
        Prefix + "should preserve a declared $status outcome when the reader is cancelled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_a_declared_status_outcome_when_the_reader_is_cancelled(
        UIMessageStreamOutcomeStatus status
    )
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onEndCalls = new List<UIMessageStreamOnEndEvent>();
        var declaredOutcome = status switch
        {
            UIMessageStreamOutcomeStatus.Failed => new UIMessageStreamOutcome(
                status,
                new Exception("stream failed")
            ),
            _ => new UIMessageStreamOutcome(status),
        };

        async IAsyncEnumerable<JsonObject> Source(
            [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken = default
        )
        {
            yield return new JsonObject { ["type"] = "start", ["messageId"] = "msg-1" };

            // the stream stays open
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        var resultStream = harness.Handle(
            Source(),
            messageId: "msg-1",
            originalMessages: [],
            onEnd: e =>
            {
                onEndCalls.Add(e);
                return Task.CompletedTask;
            },
            getOutcome: () => declaredOutcome
        );

        await using (var reader = resultStream.GetAsyncEnumerator())
        {
            await reader.MoveNextAsync();
        }

        var callArgs = Assert.Single(onEndCalls);
        Assert.Same(declaredOutcome, callArgs.Outcome);
        Assert.False(callArgs.IsCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer an observed abort over cancellation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_an_observed_abort_over_cancellation()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onEndCalls = new List<UIMessageStreamOnEndEvent>();

        async IAsyncEnumerable<JsonObject> Source(
            [System.Runtime.CompilerServices.EnumeratorCancellation]
                CancellationToken cancellationToken = default
        )
        {
            yield return new JsonObject { ["type"] = "start", ["messageId"] = "msg-1" };
            yield return new JsonObject { ["type"] = "abort" };

            // the stream stays open
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        var resultStream = harness.Handle(
            Source(),
            messageId: "msg-1",
            originalMessages: [],
            onEnd: e =>
            {
                onEndCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await using (var reader = resultStream.GetAsyncEnumerator())
        {
            await reader.MoveNextAsync();
            await reader.MoveNextAsync();
        }

        var callArgs = Assert.Single(onEndCalls);
        Assert.True(callArgs.IsAborted);
        Assert.Equal(UIMessageStreamOutcomeStatus.Aborted, callArgs.Outcome.Status);
        Assert.False(callArgs.IsCancelled);
    }
}
