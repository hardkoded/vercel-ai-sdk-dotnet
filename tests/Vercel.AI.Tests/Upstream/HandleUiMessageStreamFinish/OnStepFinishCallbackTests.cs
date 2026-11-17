// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>Port of <c>handle-ui-message-stream-finish.test.ts</c> &gt; <c>handleUIMessageStreamFinish</c> &gt; <c>onStepFinish callback</c>.</summary>
public sealed class OnStepFinishCallbackTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/handle-ui-message-stream-finish.test.ts::handleUIMessageStreamFinish > onStepFinish callback::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepEnd when finish-step chunk is encountered",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepEnd_when_finish_step_chunk_is_encountered()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepEndCalls = new List<UIMessageStreamOnStepEndEvent>();

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-step-end"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            originalMessages: [],
            onStepEnd: e =>
            {
                onStepEndCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onStepEndCalls);
        Assert.Equal("msg-step-end", callArgs.ResponseMessage.Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer onStepEnd over deprecated onStepFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_onStepEnd_over_deprecated_onStepFinish()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepEndCalls = 0;
        var onStepFinishCalls = 0;

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-step-end"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            originalMessages: [],
            onStepEnd: _ =>
            {
                onStepEndCalls++;
                return Task.CompletedTask;
            },
            onStepFinish: _ =>
            {
                onStepFinishCalls++;
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        Assert.Equal(1, onStepEndCalls);
        Assert.Equal(0, onStepFinishCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepFinish when finish-step chunk is encountered",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepFinish_when_finish_step_chunk_is_encountered()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = new List<UIMessageStreamOnStepEndEvent>();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-step-1"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Step 1 text"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish-step"}""",
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
            messageId: "msg-step-1",
            originalMessages: originalMessages,
            onStepFinish: e =>
            {
                onStepFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        var callArgs = Assert.Single(onStepFinishCalls);
        Assert.False(callArgs.IsContinuation);
        Assert.Equal("msg-step-1", callArgs.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, callArgs.ResponseMessage.Role);
        Assert.Equal(2, callArgs.Messages.Count);
        Assert.Same(originalMessages[0], callArgs.Messages[0]);
        Assert.Equal("msg-step-1", callArgs.Messages[1].Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onStepFinish multiple times for multiple steps",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepFinish_multiple_times_for_multiple_steps()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = new List<UIMessageStreamOnStepEndEvent>();

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-multi-step"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Step 1"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish-step"}""",
                """{"type":"start-step"}""",
                """{"type":"text-start","id":"text-2"}""",
                """{"type":"text-delta","id":"text-2","delta":"Step 2"}""",
                """{"type":"text-end","id":"text-2"}""",
                """{"type":"finish-step"}""",
                """{"type":"start-step"}""",
                """{"type":"text-start","id":"text-3"}""",
                """{"type":"text-delta","id":"text-3","delta":"Step 3"}""",
                """{"type":"text-end","id":"text-3"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-multi-step",
            originalMessages: [],
            onStepFinish: e =>
            {
                onStepFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        Assert.Equal(3, onStepFinishCalls.Count);

        // Verify each step has the correct accumulated content
        Assert.Single(onStepFinishCalls[0].ResponseMessage.Parts);
        Assert.Equal(3, onStepFinishCalls[1].ResponseMessage.Parts.Count); // step-start + 2 text parts
        Assert.Equal(5, onStepFinishCalls[2].ResponseMessage.Parts.Count); // 2 step-starts + 3 text parts
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call both onStepFinish and onFinish when both are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_both_onStepFinish_and_onFinish_when_both_are_provided()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = 0;
        var onFinishCalls = 0;

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-both"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-both",
            originalMessages: [],
            onStepFinish: _ =>
            {
                onStepFinishCalls++;
                return Task.CompletedTask;
            },
            onFinish: _ =>
            {
                onFinishCalls++;
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        Assert.Equal(1, onStepFinishCalls);
        Assert.Equal(1, onFinishCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle onStepFinish errors by logging and continuing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_onStepFinish_errors_by_logging_and_continuing()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = 0;
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-error"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Step 1"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish-step"}""",
            """{"type":"start-step"}""",
            """{"type":"text-start","id":"text-2"}""",
            """{"type":"text-delta","id":"text-2","delta":"Step 2"}""",
            """{"type":"text-end","id":"text-2"}""",
            """{"type":"finish-step"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-error",
            originalMessages: [],
            onStepFinish: _ =>
            {
                onStepFinishCalls++;
                return Task.FromException(new Exception("DB error"));
            }
        );

        // Stream should complete without throwing
        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);

        // Both steps should have been attempted
        Assert.Equal(2, onStepFinishCalls);

        // Error should have been logged twice
        Assert.Equal(2, harness.Errors.Count);
        Assert.All(harness.Errors, error => Assert.IsType<Exception>(error));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle continuation scenario with onStepFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_continuation_scenario_with_onStepFinish()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = new List<UIMessageStreamOnStepEndEvent>();
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
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-999",
            originalMessages: originalMessages,
            onStepFinish: e =>
            {
                onStepFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        var callArgs = Assert.Single(onStepFinishCalls);
        Assert.True(callArgs.IsContinuation);
        Assert.Equal("assistant-msg-1", callArgs.ResponseMessage.Id);
        Assert.Equal(2, callArgs.Messages.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should provide deep-cloned messages in onStepFinish to prevent mutation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_deep_cloned_messages_in_onStepFinish_to_prevent_mutation()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        var onStepFinishCalls = new List<UIMessageStreamOnStepEndEvent>();
        var onFinishCalls = new List<UIMessageStreamOnEndEvent>();

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(
                """{"type":"start","messageId":"msg-clone"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}"""
            ),
            messageId: "msg-clone",
            originalMessages: [],
            onStepFinish: e =>
            {
                // Mutate the message in the callback
                ((List<UIMessagePart>)e.ResponseMessage.Parts).Add(new TextUIPart("MUTATION!"));
                onStepFinishCalls.Add(e);
                return Task.CompletedTask;
            },
            onFinish: e =>
            {
                onFinishCalls.Add(e);
                return Task.CompletedTask;
            }
        );

        await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        // Verify onStepFinish was called and received the mutated message
        var stepMessage = Assert.Single(onStepFinishCalls).ResponseMessage;
        Assert.Equal(2, stepMessage.Parts.Count); // Original + mutation

        // onFinish should NOT see the mutation from onStepFinish
        var finishMessage = Assert.Single(onFinishCalls).ResponseMessage;
        Assert.Single(finishMessage.Parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not process stream when neither onFinish nor onStepFinish is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_process_stream_when_neither_onFinish_nor_onStepFinish_is_provided()
    {
        var harness = new HandleUIMessageStreamFinishHarness();
        string[] inputChunks =
        [
            """{"type":"start","messageId":"msg-passthrough"}""",
            """{"type":"text-start","id":"text-1"}""",
            """{"type":"text-delta","id":"text-1","delta":"Test"}""",
            """{"type":"text-end","id":"text-1"}""",
            """{"type":"finish-step"}""",
            """{"type":"finish"}""",
        ];

        var resultStream = harness.Handle(
            HandleUIMessageStreamFinishHarness.CreateUIMessageStream(inputChunks),
            messageId: "msg-passthrough",
            originalMessages: []
        ); // Neither onFinish nor onStepFinish provided

        var result = await HandleUIMessageStreamFinishHarness.ToArray(resultStream);

        HandleUIMessageStreamFinishHarness.AssertChunks(inputChunks, result);
        Assert.Empty(harness.Errors);
    }
}
