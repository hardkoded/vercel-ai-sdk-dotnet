// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToUiMessageStream;

/// <summary>Port of <c>to-ui-message-stream.test.ts</c> &gt; <c>toUIMessageStream</c>.</summary>
public sealed class ToUIMessageStreamTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream::";

    private const string Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3";

    [Fact]
    [UpstreamTest(
        Prefix + "routes error parts through onError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Routes_error_parts_through_onError()
    {
        var chunks = await Convert(
            [new ErrorPart("boom") { Error = new JsError("boom") }],
            new UIMessageStreamOptions { OnError = error => $"handled: {error.Message}" }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"type":"error","errorText":"handled: boom"}]"""),
                chunks
            ),
            chunks.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "routes an invalid tool call through onError once",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Routes_an_invalid_tool_call_through_onError_once()
    {
        var invalidToolCallError = new NoSuchToolError("test-tool", message: "invalid tool input");
        var errors = new List<Exception>();
        var chunks = await Convert(
            [
                new ToolCallPart(
                    new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"invalid\"}")
                    {
                        Invalid = true,
                        Dynamic = true,
                        Error = invalidToolCallError,
                    }
                ),
                new ToolErrorPart(
                    new ExecutedTool("call-1", "test-tool", "{}", true)
                    {
                        Dynamic = true,
                        Error = invalidToolCallError,
                    }
                ),
            ],
            new UIMessageStreamOptions
            {
                OnError = error =>
                {
                    errors.Add(error);
                    return "formatted tool error";
                },
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"type":"tool-input-error","toolCallId":"call-1","toolName":"test-tool","input":{"value":"invalid"},"dynamic":true,"errorText":"formatted tool error"}]"""
                ),
                chunks
            ),
            chunks.ToJsonString()
        );
        Assert.Same(invalidToolCallError, Assert.Single(errors));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards tool errors for valid tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_tool_errors_for_valid_tool_calls()
    {
        var executionError = new JsError("execution failed");
        var errors = new List<Exception>();
        var chunks = await Convert(
            [
                new ToolCallPart(
                    new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"valid\"}")
                    {
                        Dynamic = true,
                    }
                ),
                new ToolErrorPart(
                    new ExecutedTool("call-1", "test-tool", "{}", true)
                    {
                        Dynamic = true,
                        Error = executionError,
                    }
                ),
            ],
            new UIMessageStreamOptions
            {
                OnError = error =>
                {
                    errors.Add(error);
                    return "formatted tool error";
                },
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    [
                      {"type":"tool-input-available","toolCallId":"call-1","toolName":"test-tool","input":{"value":"valid"},"dynamic":true},
                      {"type":"tool-output-error","toolCallId":"call-1","dynamic":true,"errorText":"formatted tool error"}
                    ]
                    """
                ),
                chunks
            ),
            chunks.ToJsonString()
        );
        Assert.Same(executionError, Assert.Single(errors));
    }

    [Theory]
    [InlineData("tool execution error")]
    [InlineData("invalid tool call")]
    [UpstreamTest(
        Prefix + "sends a $name that converts back to the tool result the model received",
        Coverage = UpstreamCoverage.Covered,
        Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc"
    )]
    public async Task Sends_a_name_that_converts_back_to_the_tool_result_the_model_received(
        string name
    )
    {
        ToolCallPart toolCall;
        ToolErrorPart toolError;
        object errorOutput;
        if (name == "tool execution error")
        {
            errorOutput = new JsError("execution failed");
            toolCall = new ToolCallPart(
                new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"input\"}")
                {
                    Dynamic = true,
                }
            );
            toolError = new ToolErrorPart(
                new ExecutedTool("call-1", "test-tool", "{\"value\":\"input\"}", true)
                {
                    Dynamic = true,
                    Error = (Exception)errorOutput,
                }
            );
        }
        else
        {
            // streamText follows an invalid tool call with a tool-error part that contains the error text for the model
            errorOutput = "Error: invalid tool input";
            toolCall = new ToolCallPart(
                new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"input\"}")
                {
                    Dynamic = true,
                    Invalid = true,
                    Error = new JsError("invalid tool input"),
                }
            );
            toolError = new ToolErrorPart(
                new ExecutedTool("call-1", "test-tool", "{\"value\":\"input\"}", true)
                {
                    Dynamic = true,
                    Error = new JsError("invalid tool input"),
                }
            );
        }

        TextStreamPart[] parts =
        [
            new StartPart(),
            new StartStepPart(),
            toolCall,
            toolError,
            TestStep,
            new FinishPart(FinishReason.Stop, new LanguageModelUsage(0, 0, 0)),
        ];

        UIMessage? uiMessage = null;
        await foreach (
            var message in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(
                    UIMessageStreamConverter.ToUIMessageStream(ToAsync(parts))
                )
            )
        )
        {
            uiMessage = message;
        }

        var modelMessages = UIMessageConversion.ConvertToModelMessages([uiMessage!]);

        var toolMessage = Assert.IsType<ToolModelMessage>(modelMessages[modelMessages.Count - 1]);
        Assert.Equal("call-1", toolMessage.ToolCallId);
        Assert.Equal("test-tool", toolMessage.ToolName);

        // the output that streamText sends to the model
        var expected = Assert.IsType<ErrorTextToolModelOutput>(
            ToolOutputs.CreateToolModelOutput(
                "call-1",
                JsonDocument.Parse("{\"value\":\"input\"}").RootElement,
                errorOutput,
                true,
                null,
                "text"
            )
        );
        Assert.Equal("error-text", toolMessage.OutputType);
        Assert.Equal(expected.Value, toolMessage.OutputJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports UI chunk conversion failures as failed exactly once",
        Coverage = UpstreamCoverage.Covered,
        Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc"
    )]
    public async Task Reports_UI_chunk_conversion_failures_as_failed_exactly_once()
    {
        var conversionError = new JsError("UI chunk conversion failed");
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreamConverter.ToUIMessageStream(
            ToAsync([
                new ErrorPart("generation failed") { Error = new JsError("generation failed") },
            ]),
            new UIMessageStreamOptions
            {
                OnError = _ => throw conversionError,
                OnEnd = onEndEvent =>
                {
                    onEndEvents.Add(onEndEvent);
                    return Task.CompletedTask;
                },
            }
        );

        var thrown = await Assert.ThrowsAsync<JsError>(async () =>
        {
            await foreach (var unused in stream) { }
        });
        Assert.Same(conversionError, thrown);

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(conversionError, onEnd.Outcome.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "includes finish-step metadata in the snapshot before forwarding the boundary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_finish_step_metadata_in_the_snapshot_before_forwarding_the_boundary()
    {
        var onStepEndEvents = new List<UIMessageStreamOnStepEndEvent>();
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        var chunks = await Convert(
            [
                new StartPart(),
                new StartStepPart(),
                TestStep,
                new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2)),
            ],
            new UIMessageStreamOptions
            {
                GenerateMessageId = () => "assistant-1",
                MessageMetadata = part =>
                    part is StepFinishPart finishStep
                        ? JsonSerializer.SerializeToElement(
                            new { totalTokens = finishStep.Step.Usage.TotalTokens }
                        )
                        : null,
                OnStepEnd = onStepEndEvent =>
                {
                    onStepEndEvents.Add(onStepEndEvent);
                    return Task.CompletedTask;
                },
                OnEnd = onEndEvent =>
                {
                    onEndEvents.Add(onEndEvent);
                    return Task.CompletedTask;
                },
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    [
                      {"type":"start","messageId":"assistant-1"},
                      {"type":"start-step"},
                      {"type":"message-metadata","messageMetadata":{"totalTokens":2}},
                      {"type":"finish-step"},
                      {"type":"finish","finishReason":"stop"}
                    ]
                    """
                ),
                chunks
            ),
            chunks.ToJsonString()
        );
        var onStepEnd = Assert.Single(onStepEndEvents);
        Assert.Equal(
            2,
            onStepEnd.ResponseMessage.Metadata!.Value.GetProperty("totalTokens").GetInt32()
        );
        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(
            onStepEnd.ResponseMessage.Metadata!.Value.GetRawText(),
            onEnd.ResponseMessage.Metadata!.Value.GetRawText()
        );
        Assert.Equal(onStepEnd.ResponseMessage.Parts.Count, onEnd.ResponseMessage.Parts.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits separate metadata chunks for non-lifecycle parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_separate_metadata_chunks_for_non_lifecycle_parts()
    {
        var chunks = await Convert(
            [
                new StartPart(),
                new TextDeltaPart("t1", "Hello"),
                new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2)),
            ],
            new UIMessageStreamOptions
            {
                MessageMetadata = part =>
                    JsonSerializer.SerializeToElement(new { partType = part.Type }),
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    [
                      {"type":"start","messageMetadata":{"partType":"start"}},
                      {"type":"text-delta","id":"t1","delta":"Hello"},
                      {"type":"message-metadata","messageMetadata":{"partType":"text-delta"}},
                      {"type":"finish","finishReason":"stop","messageMetadata":{"partType":"finish"}}
                    ]
                    """
                ),
                chunks
            ),
            chunks.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports message metadata failures as failed exactly once",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_message_metadata_failures_as_failed_exactly_once()
    {
        var metadataError = new JsError("message metadata failed");
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreamConverter.ToUIMessageStream(
            ToAsync([new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2))]),
            new UIMessageStreamOptions
            {
                MessageMetadata = _ => throw metadataError,
                OnEnd = onEndEvent =>
                {
                    onEndEvents.Add(onEndEvent);
                    return Task.CompletedTask;
                },
            }
        );

        var thrown = await Assert.ThrowsAsync<JsError>(async () =>
        {
            await foreach (var unused in stream) { }
        });
        Assert.Same(metadataError, thrown);

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(metadataError, onEnd.Outcome.Error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "resolves the deprecated step callback alias (onStepEnd provided: %s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Resolves_the_deprecated_step_callback_alias(bool hasOnStepEnd)
    {
        var onStepEndCalls = 0;
        var onStepFinishCalls = 0;

        Func<UIMessageStreamOnStepEndEvent, Task>? onStepEnd = hasOnStepEnd
            ? _ =>
            {
                onStepEndCalls++;
                return Task.CompletedTask;
            }
            : null;

#pragma warning disable CS0618 // the test calls the deprecated alias
        await Convert(
            [new StartPart(), TestStep],
            new UIMessageStreamOptions
            {
                OnStepEnd = onStepEnd,
                OnStepFinish = _ =>
                {
                    onStepFinishCalls++;
                    return Task.CompletedTask;
                },
            }
        );
#pragma warning restore CS0618

        Assert.Equal(hasOnStepEnd ? 1 : 0, onStepEndCalls);
        Assert.Equal(hasOnStepEnd ? 0 : 1, onStepFinishCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "attaches the generated message id to the start chunk when provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Attaches_the_generated_message_id_to_the_start_chunk_when_provided()
    {
        var generateMessageIdCalls = 0;

        var chunks = await Convert(
            [new StartPart()],
            new UIMessageStreamOptions
            {
                GenerateMessageId = () =>
                {
                    generateMessageIdCalls++;
                    return "msg-123";
                },
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"type":"start","messageId":"msg-123"}]"""),
                chunks
            ),
            chunks.ToJsonString()
        );
        Assert.Equal(1, generateMessageIdCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "injects generated message id and calls onFinish",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Injects_generated_message_id_and_calls_onFinish()
    {
        List<UIMessage> originalMessages =
        [
            new UIMessage(UIMessageRole.User, [new TextUIPart("Hi")]) { Id = "user-msg-1" },
        ];
        var generateMessageIdCalls = 0;
        var onFinishEvents = new List<UIMessageStreamOnEndEvent>();

#pragma warning disable CS0618 // the test calls the deprecated alias
        var chunks = await Convert(
            [
                new StartPart(),
                new TextStartPart("t1"),
                new TextDeltaPart("t1", "Hello"),
                new TextEndPart("t1"),
                new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2)),
            ],
            new UIMessageStreamOptions
            {
                OriginalMessages = originalMessages,
                GenerateMessageId = () =>
                {
                    generateMessageIdCalls++;
                    return "msg-123";
                },
                OnFinish = onFinishEvent =>
                {
                    onFinishEvents.Add(onFinishEvent);
                    return Task.CompletedTask;
                },
            }
        );
#pragma warning restore CS0618

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""{"type":"start","messageId":"msg-123"}"""),
                chunks[0]
            )
        );
        Assert.Equal(1, generateMessageIdCalls);
        var onFinish = Assert.Single(onFinishEvents);
        Assert.False(onFinish.IsAborted);
        Assert.False(onFinish.IsContinuation);
        Assert.Equal("stop", onFinish.FinishReason);
        Assert.Equal("msg-123", onFinish.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, onFinish.ResponseMessage.Role);
        Assert.Equal(
            "Hello",
            Assert.IsType<TextUIPart>(Assert.Single(onFinish.ResponseMessage.Parts)).Text
        );
        Assert.Equal(2, onFinish.Messages.Count);
        Assert.Same(originalMessages[0], onFinish.Messages[0]);
        Assert.Same(onFinish.ResponseMessage, onFinish.Messages[1]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "calls onStepEnd with the accumulated UI message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Calls_onStepEnd_with_the_accumulated_UI_message()
    {
        List<UIMessage> originalMessages =
        [
            new UIMessage(UIMessageRole.User, [new TextUIPart("Hi")]) { Id = "user-msg-1" },
        ];
        var onStepEndEvents = new List<UIMessageStreamOnStepEndEvent>();

        await Convert(
            [
                new StartPart(),
                new StartStepPart(),
                new TextStartPart("t1"),
                new TextDeltaPart("t1", "First"),
                new TextEndPart("t1"),
                TestStep,
                new StartStepPart(),
                new TextStartPart("t2"),
                new TextDeltaPart("t2", "Second"),
                new TextEndPart("t2"),
                TestStep,
                new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2)),
            ],
            new UIMessageStreamOptions
            {
                OriginalMessages = originalMessages,
                GenerateMessageId = () => "assistant-msg-1",
                OnStepEnd = onStepEndEvent =>
                {
                    onStepEndEvents.Add(onStepEndEvent);
                    return Task.CompletedTask;
                },
            }
        );

        Assert.Equal(2, onStepEndEvents.Count);
        var first = onStepEndEvents[0];
        Assert.False(first.IsContinuation);
        Assert.Equal("assistant-msg-1", first.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, first.ResponseMessage.Role);
        Assert.Collection(
            first.ResponseMessage.Parts,
            part => Assert.IsType<StepStartUIPart>(part),
            part => Assert.Equal("First", Assert.IsType<TextUIPart>(part).Text)
        );
        Assert.Equal(2, first.Messages.Count);
        Assert.Same(originalMessages[0], first.Messages[0]);
        Assert.Equal("assistant-msg-1", first.Messages[1].Id);
        Assert.Collection(
            onStepEndEvents[1].ResponseMessage.Parts,
            part => Assert.IsType<StepStartUIPart>(part),
            part => Assert.Equal("First", Assert.IsType<TextUIPart>(part).Text),
            part => Assert.IsType<StepStartUIPart>(part),
            part => Assert.Equal("Second", Assert.IsType<TextUIPart>(part).Text)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "calls onEnd when stream finishes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Calls_onEnd_when_stream_finishes()
    {
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();
        var onFinishCalls = 0;

#pragma warning disable CS0618 // the test calls the deprecated alias
        await Convert(
            [
                new StartPart(),
                new TextStartPart("t1"),
                new TextDeltaPart("t1", "Hello"),
                new TextEndPart("t1"),
                new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2)),
            ],
            new UIMessageStreamOptions
            {
                GenerateMessageId = () => "msg-123",
                OnEnd = onEndEvent =>
                {
                    onEndEvents.Add(onEndEvent);
                    return Task.CompletedTask;
                },
                OnFinish = _ =>
                {
                    onFinishCalls++;
                    return Task.CompletedTask;
                },
            }
        );
#pragma warning restore CS0618

        var onEnd = Assert.Single(onEndEvents);
        Assert.False(onEnd.IsAborted);
        Assert.False(onEnd.IsContinuation);
        Assert.Equal("stop", onEnd.FinishReason);
        Assert.Equal("msg-123", onEnd.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, onEnd.ResponseMessage.Role);
        Assert.Equal(
            "Hello",
            Assert.IsType<TextUIPart>(Assert.Single(onEnd.ResponseMessage.Parts)).Text
        );
        Assert.Equal(0, onFinishCalls);
    }

    private static readonly TextStreamPart FinishStop = new FinishPart(
        FinishReason.Stop,
        new LanguageModelUsage(1, 1, 2)
    );

    private static void AssertChunks(string expectedJson, JsonArray chunks)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), chunks),
            chunks.ToJsonString()
        );
    }

    private static Func<UIMessageStreamOnEndEvent, Task> Record(
        List<UIMessageStreamOnEndEvent> events
    ) =>
        onEndEvent =>
        {
            events.Add(onEndEvent);
            return Task.CompletedTask;
        };

    [Fact]
    [UpstreamTest(
        Prefix + "awaits the UI callback before delivering finish-step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Awaits_the_UI_callback_before_delivering_finish_step()
    {
        var callbackStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var callbackFinished = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        await using var enumerator = UIMessageStreamConverter
            .ToUIMessageStream(
                ToAsync([new StartPart(), TestStep]),
                new UIMessageStreamOptions
                {
                    OnStepEnd = async _ =>
                    {
                        callbackStarted.SetResult();
                        await callbackFinished.Task;
                    },
                }
            )
            .GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("""{"type":"start"}""", enumerator.Current.ToJsonString());

        var nextRead = enumerator.MoveNextAsync().AsTask();
        await callbackStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Task.Delay(50);
        Assert.False(nextRead.IsCompleted);

        callbackFinished.SetResult();
        Assert.True(await nextRead.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal("""{"type":"finish-step"}""", enumerator.Current.ToJsonString());
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps text and lifecycle parts to UI message chunks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_text_and_lifecycle_parts_to_UI_message_chunks()
    {
        var chunks = await Convert([
            new StartPart(),
            new StartStepPart(),
            new TextStartPart("t1"),
            new TextDeltaPart("t1", "Hello"),
            new TextDeltaPart("t1", ", world!"),
            new TextEndPart("t1"),
            TestStep,
            FinishStop,
        ]);

        AssertChunks(
            """
            [
              {"type":"start"},
              {"type":"start-step"},
              {"type":"text-start","id":"t1"},
              {"type":"text-delta","id":"t1","delta":"Hello"},
              {"type":"text-delta","id":"t1","delta":", world!"},
              {"type":"text-end","id":"t1"},
              {"type":"finish-step"},
              {"type":"finish","finishReason":"stop"}
            ]
            """,
            chunks
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "suppresses start/finish chunks when sendStart/sendFinish are false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Suppresses_start_finish_chunks_when_sendStart_sendFinish_are_false()
    {
        var chunks = await Convert(
            [new StartPart(), new TextStartPart("t1"), new TextEndPart("t1"), FinishStop],
            new UIMessageStreamOptions { SendStart = false, SendFinish = false }
        );

        AssertChunks("""[{"type":"text-start","id":"t1"},{"type":"text-end","id":"t1"}]""", chunks);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "skips reasoning parts when sendReasoning is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Skips_reasoning_parts_when_sendReasoning_is_false()
    {
        var chunks = await Convert(
            [
                new ReasoningStartPart("r1"),
                new ReasoningDeltaPart("r1", "thinking"),
                new ReasoningEndPart("r1"),
            ],
            new UIMessageStreamOptions { SendReasoning = false }
        );

        Assert.Empty(chunks);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports the source outcome to onEnd",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_the_source_outcome_to_onEnd()
    {
        async Task<(UIMessageStreamOutcomeStatus Status, string? ErrorMessage)> Observe(
            params TextStreamPart[] parts
        )
        {
            var onEndEvents = new List<UIMessageStreamOnEndEvent>();
            await Convert(parts, new UIMessageStreamOptions { OnEnd = Record(onEndEvents) });

            var outcome = onEndEvents[0].Outcome;
            return (
                outcome.Status,
                outcome.Status == UIMessageStreamOutcomeStatus.Failed
                    ? outcome.Error?.Message
                    : null
            );
        }

        var abortPart = new AbortPart("user cancelled");
        var errorPart = new ErrorPart("generation failed")
        {
            Error = new JsError("generation failed"),
        };

        Assert.Equal((UIMessageStreamOutcomeStatus.Aborted, null), await Observe(abortPart));
        Assert.Equal((UIMessageStreamOutcomeStatus.Completed, null), await Observe(FinishStop));
        Assert.Equal(
            (UIMessageStreamOutcomeStatus.Failed, "generation failed"),
            await Observe(errorPart)
        );
        Assert.Equal((UIMessageStreamOutcomeStatus.Unknown, null), await Observe());
        Assert.Equal(
            (UIMessageStreamOutcomeStatus.Completed, null),
            await Observe(FinishStop, errorPart)
        );
        Assert.Equal(
            (UIMessageStreamOutcomeStatus.Completed, null),
            await Observe(errorPart, FinishStop)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports an errored source stream as failed exactly once",
        Coverage = UpstreamCoverage.Partial,
        Note = "The source is an IAsyncEnumerable: disposal of its enumerator stands in for upstream's sourceStream.locked check."
    )]
    public async Task Reports_an_errored_source_stream_as_failed_exactly_once()
    {
        var sourceError = new JsError("source stream failed");
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();
        var sourceReleased = false;

        async IAsyncEnumerable<TextStreamPart> Source()
        {
            try
            {
                await Task.Yield();
                throw sourceError;
            }
            finally
            {
                sourceReleased = true;
            }
#pragma warning disable CS0162
            yield break;
#pragma warning restore CS0162
        }

        var stream = UIMessageStreamConverter.ToUIMessageStream(
            Source(),
            new UIMessageStreamOptions { OnEnd = Record(onEndEvents) }
        );

        var thrown = await Assert.ThrowsAsync<JsError>(async () =>
        {
            await foreach (var unused in stream) { }
        });
        Assert.Same(sourceError, thrown);

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(sourceError, onEnd.Outcome.Error);
        Assert.True(sourceReleased);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "releases the source reader lock after completion",
        Coverage = UpstreamCoverage.Partial,
        Note = "The source is an IAsyncEnumerable: disposal of its enumerator stands in for upstream's sourceStream.locked check."
    )]
    public async Task Releases_the_source_reader_lock_after_completion()
    {
        var sourceReleased = false;

        async IAsyncEnumerable<TextStreamPart> Source()
        {
            try
            {
                await Task.Yield();
                yield break;
            }
            finally
            {
                sourceReleased = true;
            }
        }

        await foreach (var unused in UIMessageStreamConverter.ToUIMessageStream(Source())) { }

        Assert.True(sourceReleased);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "releases the source reader lock after cancellation",
        Coverage = UpstreamCoverage.Partial,
        Note = "The source is an IAsyncEnumerable: disposal of its enumerator stands in for upstream's sourceStream.locked check."
    )]
    public async Task Releases_the_source_reader_lock_after_cancellation()
    {
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();
        var sourceReleased = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        async IAsyncEnumerable<TextStreamPart> Source()
        {
            try
            {
                yield return new StartPart();
                await Task.Delay(Timeout.Infinite);
            }
            finally
            {
                sourceReleased.TrySetResult();
            }
        }

        var enumerator = UIMessageStreamConverter
            .ToUIMessageStream(Source(), new UIMessageStreamOptions { OnEnd = Record(onEndEvents) })
            .GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        await enumerator.DisposeAsync(); // the consumer cancels

        await sourceReleased.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var onEnd = Assert.Single(onEndEvents);
        Assert.False(onEnd.IsAborted);
        Assert.True(onEnd.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, onEnd.Outcome.Status);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not let an earlier finish part hide a source stream failure",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_let_an_earlier_finish_part_hide_a_source_stream_failure()
    {
        var sourceError = new JsError("source stream failed after finish");
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        async IAsyncEnumerable<TextStreamPart> Source()
        {
            yield return FinishStop;
            await Task.Yield();
            throw sourceError;
        }

        var stream = UIMessageStreamConverter.ToUIMessageStream(
            Source(),
            new UIMessageStreamOptions { OnEnd = Record(onEndEvents) }
        );

        var thrown = await Assert.ThrowsAsync<JsError>(async () =>
        {
            await foreach (var unused in stream) { }
        });
        Assert.Same(sourceError, thrown);

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(sourceError, onEnd.Outcome.Error);
    }

    private static readonly TextStreamPart TestStep = new StepFinishPart(
        new StepResult("", null, [], [], FinishReason.Stop, new LanguageModelUsage(1, 1, 2), []),
        new LanguageModelResponseMetadata(
            "id-0",
            DateTimeOffset.FromUnixTimeMilliseconds(0),
            "mock-model-id"
        )
    );

    internal static async Task<JsonArray> Convert(
        IReadOnlyList<TextStreamPart> parts,
        UIMessageStreamOptions? options = null
    )
    {
        var chunks = new JsonArray();
        await foreach (
            var chunk in UIMessageStreamConverter.ToUIMessageStream(ToAsync(parts), options)
        )
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static async IAsyncEnumerable<TextStreamPart> ToAsync(
        IReadOnlyList<TextStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }
}
