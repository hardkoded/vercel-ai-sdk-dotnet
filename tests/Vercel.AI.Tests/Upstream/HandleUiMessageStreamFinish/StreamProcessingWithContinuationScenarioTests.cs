// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>Port of <c>handle-ui-message-stream-finish.test.ts</c> &gt; <c>handleUIMessageStreamFinish</c> &gt; <c>stream processing with continuation scenario</c>.</summary>
public sealed class StreamProcessingWithContinuationScenarioTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/handle-ui-message-stream-finish.test.ts::handleUIMessageStreamFinish > stream processing with continuation scenario::";

    [Fact]
    [UpstreamTest(
        Prefix + "should handle continuation when last message is assistant",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_continuation_when_last_message_is_assistant()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        List<UIMessage> originalMessages =
        [
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-1",
                UIMessageRole.User,
                "Continue this"
            ),
            HandleUIMessageStreamFinishHarness.TextMessage(
                "assistant-msg-1",
                UIMessageRole.Assistant,
                "This is"
            ),
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"assistant-msg-1"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":" continued"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-999", // this should be ignored since we're continuing
            originalMessages: originalMessages,
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onFinishCalls);
        Assert.True(callArgs.IsContinuation);
        Assert.Equal("assistant-msg-1", callArgs.ResponseMessage.Id); // uses the existing assistant message id
        Assert.Equal(2, callArgs.Messages.Count); // original user message + updated assistant message
        Assert.Same(originalMessages[0], callArgs.Messages[0]);
        Assert.Same(callArgs.ResponseMessage, callArgs.Messages[1]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not treat user message as continuation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_treat_user_message_as_continuation()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();
        List<UIMessage> originalMessages =
        [
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-1",
                UIMessageRole.User,
                "Question"
            ),
            HandleUIMessageStreamFinishHarness.TextMessage(
                "user-msg-2",
                UIMessageRole.User,
                "Another question"
            ),
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-001"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"New response"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-001",
            originalMessages: originalMessages,
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onFinishCalls);
        Assert.False(callArgs.IsContinuation);
        Assert.Equal("msg-001", callArgs.ResponseMessage.Id);
        Assert.Equal(3, callArgs.Messages.Count); // 2 user messages + 1 new assistant message
    }
}
